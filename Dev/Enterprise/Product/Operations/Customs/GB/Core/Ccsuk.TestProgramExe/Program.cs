using System;
using System.Net.Sockets;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.Ccsuk.TestProgramExe
{
	public class Program
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "Debugging is my choice")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1060:DoNotUseDateTimeNow", Justification = "I do not have a full connection to the DB, so no ZDateTime access.")]
		static void Main(string[] args)
		{
			string intro = string.Format(@"
**Enterprise.Customs.GB.Ccsuk.TestProgramExe** 
Date-time now = {0}
Mode = {1} 
Listen port = {2} 
Database = {3}
Server = {4} 
Listen IP = {5} 
This application was spawned by Enterprise.Customs.GB.Ccsuk.Test.EndToEndTest.  
It is pretending that it is the CCSUK host and is a communicating 
  bidirectionally over two sockets with the test runner. 
It should quit after it finishes, a few seconds after starting.
If you see it crash, please click any 'view more details' button and send a
 screenshot to Daniel Clarke.
 
", DateTime.Now, args[0], args[2], args[3], args[4], args[5]); // I do not have a full connection to the DB, so no ZDateTime access.

			Console.Write(intro);

			try
			{
				CcsukTestRunner ccsukTestRunner = new CcsukTestRunner(DebugToConsoleAndFile);
				ccsukTestRunner.TempDirForInboundInterchange = args[1];
				ccsukTestRunner.ListenPort = int.Parse(args[2]);
				ccsukTestRunner.DatabaseName = args[3];
				ccsukTestRunner.ServerName = args[4];
				ccsukTestRunner.ListenIP = args[5];
				EnterpriseApplicationConfiguration.ConfigureObjectFactory();

				if (args[0] == argSendMe)
				{
					ccsukTestRunner.DoSendToParticipant();
				}
				else if (args[0] == argReceive)
				{
					ccsukTestRunner.DoReceiveFromParticipant();
				}
				else if (args[0] == argBadHandshake)
				{
					ccsukTestRunner.DoBadHandshakeTest();
				}
				else if (args[0] == argSendMeCDS)
				{
					ccsukTestRunner.DoSendToParticipantCDS();
				}
				else if (args[0] == argSendMeUtf8)
				{
					ccsukTestRunner.DoSendToParticipantUtf8();
				}
				else if (args[0] == argSendMeChunks)
				{
					ccsukTestRunner.DoSendToParticipantInChunks();
				}
				else if (args[0] == argSendMeNothing)
				{
					ccsukTestRunner.DoSendToParticipantNothing();
				}
				else
				{
					Console.WriteLine("***** Unhandled scenario in runner.....  Cannot understand scenario=" + args[0]);
				}
				ccsukTestRunner.ShutDownNicely();
				Console.WriteLine("Runner completed OK, about to exit");
			}
			catch (SocketException ex)
			{
				Console.WriteLine("***** SocketException in runner..... ");
				Console.WriteLine(ex.ToString());
				System.Environment.ExitCode = ex.ErrorCode;
			}
			catch (Exception ex)
			{
				Console.WriteLine("***** Exception in runner..... ");
				Console.WriteLine(ex.ToString());
				System.Environment.ExitCode = -1;
			}
		}

		const string argSendMe = "SENDMESOMETHING";
		const string argSendMeCDS = "SENDMESOMETHINGCDS";
		const string argSendMeUtf8 = "SENDMESOMETHINGUTF8";
		const string argSendMeChunks = "SENDMESOMETHINGINCHUNKS";
		const string argSendMeNothing = "SENDMENOTHING";
		const string argReceive = "YOUWILLRECEIVESOMETHING";
		const string argBadHandshake = "BADHANDSHAKE";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1106:DebugMessages", Justification = "You'll be sorry if you have a whole bunch of black boxes on your screen without any indication of WTF they are!")]
		static void DebugToConsoleAndFile(string message, string debugDirectory)
		{
			Console.WriteLine(message);
		}
	}
}
