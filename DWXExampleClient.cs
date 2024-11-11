using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using static DWXConnect.Helpers;

/*

Example DWXConnect client in C#


This example client will subscribe to tick data and bar data. 
It will also request historic data. 


compile and run:

dotnet build
dotnet run

*/

namespace DWXConnect
{
    class DWXExampleClient
    {
	    
	    // string terminal_data_path=TerminalInfoString(TERMINAL_DATA_PATH);
        static string MetaTraderDirPath = "C:\\Users\\psyto\\AppData\\Roaming\\MetaQuotes\\Terminal\\D0E8209F77C8CF37AD8BF550E51FF075\\MQL5\\Files";
		
        static int sleepDelay = 5;  // 5 milliseconds
        static int maxRetryCommandSeconds = 10;
        static bool loadOrdersFromFile = true;
        static bool verbose = true;

        static void Main(string[] args)
        {
	        //FilePatterTest();
	        // Output the current DateTime in UTC offset format
// 	        DateTimeOffset now = DateTimeOffset.Now;
// 	        string formattedDate = now.ToString("yyyy-MM-ddTHH:mm:sszzz");
// 	        Console.WriteLine("Formatted DateTime with UTC Offset: " + formattedDate);
//
// 	        // Parse the DateTime back from the string with UTC offset
// 	        DateTimeOffset parsedDate = DateTimeOffset.Parse(formattedDate);
// 	        Console.WriteLine("Parsed DateTimeOffset: " + parsedDate);
// 	        
// 	        
// 	        string dateString = "2024.11.11 04:56.687-00:00";
// 	        parsedDate = DateTime.ParseExact(dateString, "yyyy.MM.dd HH:mm.fffzzz", null);
// 	        Console.WriteLine("Parsed DateTime: " + parsedDate);
// // Convert DateTimeOffset to DateTime
// 	        DateTime dateTime = parsedDate.DateTime;
// 	        Console.WriteLine("Converted DateTime: " + dateTime);
            MyEventHandler eventHandler = new MyEventHandler();
            
            Client dwx = new Client(eventHandler, MetaTraderDirPath, sleepDelay,
                                    maxRetryCommandSeconds, loadOrdersFromFile, verbose);
        }

        static void FilePatterTest()
        {
	        string directoryPath = Path.Join(MetaTraderDirPath, "DWX"); // Replace with your directory path
	        string filePattern = "\\.cs$"; // Replace with your regex pattern
	        Regex regex = new Regex("DWX_Historic_Data.*\\.txt");
	        DirectoryInfo di = new DirectoryInfo(directoryPath);

	        //var matches = Directory.EnumerateFiles(directoryPath).Where(f => regex.IsMatch(f));
			Console.WriteLine(directoryPath);
	        try
	        {
		        // Get all files in the directory
		        //string[] files = Directory.GetFiles(directoryPath);
		       // var files = Directory.EnumerateFiles(directoryPath);//.Where(f => regex.IsMatch(f));
		        Console.WriteLine("Matching files:");
		        foreach (var fi in di.GetFiles("DWX_Historic_Data*txt"))
		        {
			        Console.WriteLine(fi.Name);
			        
			        Console.WriteLine(fi.FullName); // this reveals the bug
		        }
		        
		        /*foreach (string file in files)
		        {
			        string fileName = Path.GetFileName(file);
			        if (Regex.IsMatch(fileName, filePattern))
			        {
				        Console.WriteLine(fileName);

				        // Read all lines from the matching file
				        string[] lines = File.ReadAllLines(file);
				        foreach (string line in lines)
				        {
					        Console.WriteLine(line);
				        }
			        }
		        }*/
	        }
	        catch (Exception ex)
	        {
		        Console.WriteLine($"Error reading files: {ex.Message}");
	        }
        }
    }

	
	/*Custom event handler implementing the EventHandler interface. 
	*/
    class MyEventHandler : EventHandler
    {
        bool first = true;

        public void start(Client dwx)
        {
			// account information is stored in dwx.accountInfo.
			print("\nAccount info:\n" + dwx.accountInfo + "\n");
		
			// subscribe to tick data:
   //          string[] symbols = { "EURUSD", "GBPUSD"};
   //          dwx.subscribeSymbols(symbols);
			//
			// // subscribe to bar data:
			// string[,] symbolsBarData = new string[,]{ { "EURUSD", "M1" }, { "AUDCAD", "M5" }, { "GBPCAD", "M15" } };
   //         dwx.subscribeSymbolsBarData(symbolsBarData);
			
			// request historic data:
			long end = DateTimeOffset.Now.ToUnixTimeSeconds();
			long start = end - 16*24*60*60;  // last 10 days
			//dwx.getHistoricData("EURUSD", "D1", start, end);
			
			dwx.getHistoricTickData("EURUSD", start, end);
			
        }

        public void onTick(Client dwx, string symbol, double bid, double ask)
        {
            print("onTick: " + symbol + " | bid: " + bid + " | ask: " + ask);
			
			// print(dwx.accountInfo);
			// print(dwx.openOrders);
            
            // to open multiple orders:
			// if (first) {
			// 	first = false;
            // // dwx.closeAllOrders();
			// 	for (int i=0; i<5; i++) {
			// 		dwx.openOrder(symbol, "buystop", 0.05, ask+0.01, 0, 0, 77, "", 0);
			// 	}
			// }
        }

        public void onBarData(Client dwx, string symbol, string timeFrame, string time, double open, double high, double low, double close, int tickVolume)
        {
            print("onBarData: " + symbol + ", " + timeFrame + ", " + time + ", " + open + ", " + high + ", " + low + ", " + close + ", " + tickVolume);

            foreach (var x in dwx.historicData)
                print(x.Key + ": " + x.Value);
        }

        public void onHistoricData(Client dwx, String symbol, String timeFrame, Dictionary<string, object> data)
        {

            // you can also access historic data via: dwx.historicData.keySet()
            print("onHistoricData: " + symbol + ", " + timeFrame + ", " + data);
        }

        public void onHistoricTickData(Client dwx, String symbol, Dictionary<string, object> data)
        {

	        // you can also access historic data via: dwx.historicData.keySet()
	        print("onHistoricTickData: " + symbol + ", " + data);
        }
        public void onHistoricTrades(Client dwx)
        {
            print("onHistoricTrades: " + dwx.historicTrades);
        }


        public void onMessage(Client dwx, JObject message)
        {
            if (((string)message["type"]).Equals("ERROR")) 
				print(message["type"] + " | " + message["error_type"] + " | " + message["description"]);
			else if (((string)message["type"]).Equals("INFO")) 
				print(message["type"] + " | " + message["message"]);
        }

        public void onOrderEvent(Client dwx)
        {
            print("onOrderEvent: " + dwx.openOrders.Count + " open orders");

            // dwx.openOrders is a JSONObject, which can be accessed like this:
            // foreach (var x in dwx.openOrders)
            //     print(x.Key + ": " + x.Value);
        }
    }
}
