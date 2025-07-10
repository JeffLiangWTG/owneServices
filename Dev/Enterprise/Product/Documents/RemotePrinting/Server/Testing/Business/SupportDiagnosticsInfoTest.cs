using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.RemotePrinting.Server.Business;
using Enterprise.RemotePrinting.Server.Model;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing.Business
{
	[TestedType(typeof(SupportDiagnosticsInfo))]
	class SupportDiagnosticsInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsValidSupportDiagnosticsInfo()
		{
			EnterpriseInformationRetriever enterpriseRetriever = new EnterpriseInformationRetriever();
			var supportDiagnosticsInfo = new SupportDiagnosticsInfo();
			AssertEquals(enterpriseRetriever.VersionDate, supportDiagnosticsInfo.VersionDate);
			AssertEquals(enterpriseRetriever.VersionNumber, supportDiagnosticsInfo.VersionNumber);
			AssertEquals(enterpriseRetriever.LicenceCode, supportDiagnosticsInfo.LicenseCode);
			AssertEquals(enterpriseRetriever.Release, supportDiagnosticsInfo.Release);
		}

		public void TestIsValidPrintQueueInfo()
		{
			var printServer = Factory.New<StmPrintServer>();
			printServer.SPS_ServerName = "TestServer1";

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_SPS_Server = printServer.PK;
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_WebPrintServiceAddress = "http://10.61.224.23:4545/";
			printQueue.SQ_AllowPrinting = true;

			var printQueue2 = Factory.New<StmPrintQueue>();
			printQueue2.SQ_SPS_Server = printServer.PK;
			printQueue2.SQ_QueueName = "Queue12";
			printQueue2.SQ_WebPrintServiceAddress = "http://10.61.224.24:4545/";
			printQueue2.SQ_AllowPrinting = false;

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_Status = "FAL";
			printJob.SP_JobType = "PRN";

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_SQ = printQueue.PK;
			printJob2.SP_Status = "QUE";
			printJob2.SP_JobType = "PRN";
			Factory.Save();

			var supportDiagnosticsInfo = new SupportDiagnosticsInfo();
			StmPrintQueueExt stmPrintQueueExt = (StmPrintQueueExt)supportDiagnosticsInfo.PrintQueues.FirstOrDefault(p => p.PK == printQueue.PK);

			AssertNotNull(stmPrintQueueExt);
			AssertEquals(2, supportDiagnosticsInfo.PrintQueues.Count);
			AssertEquals(printQueue.SQ_QueueName, stmPrintQueueExt.SQ_QueueName);
			AssertEquals(printQueue.SQ_ServerName, stmPrintQueueExt.SQ_ServerName);
			AssertEquals(printQueue.SQ_WebPrintServiceAddress, stmPrintQueueExt.SQ_WebPrintServiceAddress);
			AssertEquals(printQueue.SQ_AllowPrinting, stmPrintQueueExt.SQ_AllowPrinting);
			AssertEquals(1, stmPrintQueueExt.FailedPrintJobs);
			AssertEquals(1, stmPrintQueueExt.QueuedPrintJobs);
			AssertEquals(0, stmPrintQueueExt.WorkingPrintJobs);
		}

		public void TestIsValidSignalRClientInfo()
		{
			var supportDiagnosticsInfo = new SupportDiagnosticsInfoForTest();

			var printServer = Factory.New<StmPrintServer>();
			printServer.SPS_ServerName = "TestServer1";

			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_SPS_Server = printServer.PK;
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_WebPrintServiceAddress = "http://10.61.224.23:4545/|host:test.wtg.zone";
			printQueue.SQ_AllowPrinting = true;

			Factory.Save();

			var printers = new[] { "print1", "print2", "print3" };
			RemoteHub.Controller.RegisterClientForTest("client1", "server1", printers, "1.0");

			AssertEquals(1, supportDiagnosticsInfo.SignalRClients.Count);

			var signalRClient = supportDiagnosticsInfo.SignalRClients[0];

			AssertNotNull(signalRClient);
			AssertEquals("server1", signalRClient.ServerName);
			AssertEquals(3, signalRClient.PrintersCount);
			AssertEquals("1.0", signalRClient.WebPrintClientVersionNumber);
		}
	}

	public class SupportDiagnosticsInfoForTest : SupportDiagnosticsInfo
	{
		public override SignalRClientInfo[] GetSignalRClients(string webPrintServiceAddress)
		{
			return RemoteHub.Controller.GetRegisteredClients();
		}
	}
}

