using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.RemotePrinting.Server.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using static Enterprise.RemotePrinting.Server.Testing.GlobalTest;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class SupportDiagnosticsTest : WebControlTest
	{
		protected override Control GetNewControl()
		{
			return new SupportDiagnostics();
		}

		public void TestIsValidInfoForSupportDiagnosticsControls()
		{
			EnterpriseInformationRetriever enterpriseRetriever = new EnterpriseInformationRetriever();
			var supportDiagnosticsInfo = new SupportDiagnosticsForTest();
			AssertEquals(enterpriseRetriever.VersionDate, supportDiagnosticsInfo.VersionDate);
			AssertEquals(enterpriseRetriever.VersionNumber, supportDiagnosticsInfo.VersioNumberVal);
			AssertEquals(enterpriseRetriever.LicenceCode, supportDiagnosticsInfo.LicenseCode);
			AssertEquals(enterpriseRetriever.Release, supportDiagnosticsInfo.ReleaseText);
		}

		public void TestIsValidPrintQueueGridSource()
		{
			var supportDiagnosticsInfo = new SupportDiagnosticsInfo();

			var printQueue = Factory.New<StmPrintQueueExt>();
			printQueue.SQ_ServerName = "Server1";
			printQueue.SQ_QueueName = "Queue1";
			printQueue.SQ_WebPrintServiceAddress = "http://10.61.224.23:4545/";
			printQueue.SQ_AllowPrinting = true;

			var printJob = Factory.New<StmPrintJob>();
			printJob.SP_SQ = printQueue.PK;
			printJob.SP_Status = "FAL";
			printJob.SP_JobType = "PRN";

			var printJob2 = Factory.New<StmPrintJob>();
			printJob2.SP_SQ = printQueue.PK;
			printJob2.SP_Status = "QUE";
			printJob2.SP_JobType = "PRN";

			var printJob3 = Factory.New<StmPrintJob>();
			printJob3.SP_SQ = printQueue.PK;
			printJob3.SP_Status = "WRK";
			printJob3.SP_JobType = "PRN";

			var printJob4 = Factory.New<StmPrintJob>();
			printJob4.SP_SQ = printQueue.PK;
			printJob4.SP_Status = "WRK";
			printJob4.SP_JobType = "PRN";

			var printQueue2 = Factory.New<StmPrintQueueExt>();
			printQueue2.SQ_ServerName = "Server2";
			printQueue2.SQ_QueueName = "Queue2";
			printQueue2.SQ_WebPrintServiceAddress = "http://10.61.224.24:4545/";
			printQueue2.SQ_AllowPrinting = true;

			Factory.Save();

			var supportDiagnosticsPage = new SupportDiagnosticsForTest();
			supportDiagnosticsPage.SetupPrintQueueGridForTest(supportDiagnosticsInfo);
			StmPrintQueueExtCollection printQueueGridData = (StmPrintQueueExtCollection)supportDiagnosticsPage.PrintQueuesDataGrid.DataSource;

			StmPrintQueueExt stmPrintQueueExt = printQueueGridData.Where(p => p.SQ_ServerName == printQueue.SQ_ServerName).FirstOrDefault();

			AssertNotNull(stmPrintQueueExt);
			AssertEquals(2, printQueueGridData.Count);
			AssertEquals(printQueue.SQ_ServerName, stmPrintQueueExt.SQ_ServerName);
			AssertEquals(printQueue.SQ_QueueName, stmPrintQueueExt.SQ_QueueName);
			AssertEquals(true, stmPrintQueueExt.SQ_AllowPrinting);
			AssertEquals(1, stmPrintQueueExt.FailedPrintJobs);
			AssertEquals(2, stmPrintQueueExt.WorkingPrintJobs);
			AssertEquals(1, stmPrintQueueExt.QueuedPrintJobs);
		}

		public void TestIsValidSignalRGridSource()
		{
			var supportDiagnosticsInfo = new SupportDiagnosticsInfo();
			var signalRClient1 = new SignalRClientBusinessObject()
			{
				ServerName = "server1",
				PrintersCount = 3,
				WebPrintServerAddress = "webprintserver",
				WebPrintClientVersionNumber = "1.0"
			};
			var signalRClient2 = new SignalRClientBusinessObject()
			{
				ServerName = "server2",
				PrintersCount = 2,
				WebPrintServerAddress = "webprintserver2",
				WebPrintClientVersionNumber = "1.0"
			};
			supportDiagnosticsInfo.SignalRClients.Add(signalRClient1);
			supportDiagnosticsInfo.SignalRClients.Add(signalRClient2);

			var supportDiagnosticsForTest = new SupportDiagnosticsForTest();
			supportDiagnosticsForTest.SetupSignalRGridForTest(supportDiagnosticsInfo);

			SignalRClientCollection signalRGridDataSource = (SignalRClientCollection)supportDiagnosticsForTest.SignalRClientGrid.DataSource;
			var signalRClient = (SignalRClientBusinessObject)signalRGridDataSource
									.FirstOrDefault(a => a[SignalRClientBusinessObject.Schema.ServerName].ToString() == "server1");

			AssertNotNull(signalRClient);
			AssertEquals(supportDiagnosticsInfo.SignalRClients.Count, signalRGridDataSource.Count);
			AssertEquals("server1", signalRClient.ServerName);
			AssertEquals(3, signalRClient.PrintersCount);
			AssertEquals("1.0", signalRClient.WebPrintClientVersionNumber);
		}

		public void TestReloadSupportDiagnosticsPageScriptBlock()
		{
			var supportDiagnosticsPage = new SupportDiagnosticsForTest();
			var expectedScript = @"<SCRIPT type=""text/javascript"">
						function Reload_SupportDiagnostics_Page()
						{
							window.location.href='/Support/Diagnostics.aspx?TimeStamp=' + Date.now();
						}
						</SCRIPT>";
			AssertEquals(expectedScript, supportDiagnosticsPage.ReloadSupportDiagnosticsPageScriptBlock);
		}

		public void TestRegisterReloadSupportDiagnosticsPageScriptBlock()
		{
			var supportDiagnosticsPage = new SupportDiagnosticsForTest();
			var registered = supportDiagnosticsPage.ZClientScript.IsStartupScriptRegistered(supportDiagnosticsPage.GetType(), SupportDiagnostics.ReloadSupportDiagnosticsPageScriptBlockKey);
			AssertEquals("ReloadSupportDiagnosticsPageScriptBlock should not registered", false, registered);

			supportDiagnosticsPage.RenderPageSpecificScripts_Exposed();
			registered = supportDiagnosticsPage.ZClientScript.IsStartupScriptRegistered(supportDiagnosticsPage.GetType(), SupportDiagnostics.ReloadSupportDiagnosticsPageScriptBlockKey);
			AssertEquals("ReloadSupportDiagnosticsPageScriptBlock should registered", true, registered);
		}
	}

	class SupportDiagnosticsForTest : SupportDiagnostics
	{
		public string LastNavigatedUrl { get; set; }

		public void SetupPageForTest()
		{
			base.VersioNumberVal = new Label();
			base.VersionDate = new Label();
			base.LicenseCode = new Label();
			base.ReleaseText = new Label();
		}

		public void RenderPageSpecificScripts_Exposed() => base.RenderPageSpecificScripts();

		public void SetupPrintQueueGridForTest(SupportDiagnosticsInfo supportDiagnosticsInfo)
		{
			PrintQueuesDataGrid.DataSource = supportDiagnosticsInfo.PrintQueues;
			base.SetupPrintQueueGrid();
		}

		public void SetupSignalRGridForTest(SupportDiagnosticsInfo supportDiagnosticsInfo)
		{
			SignalRClientGrid.DataSource = supportDiagnosticsInfo.SignalRClients;
			base.SetupSignalRClientGrid();
		}

		public SupportDiagnosticsForTest()
		{
			SetupPageForTest();
			LoadOrCreateDataSource();
			Page_Load(this, EventArgs.Empty);
		}

		public new DataGrid PrintQueuesDataGrid
		{
			get => base.PrintQueuesDataGrid ?? (base.PrintQueuesDataGrid = new ZDataGrid());
		}

		public new DataGrid SignalRClientGrid
		{
			get => base.SignalRClientGrid ?? (base.SignalRClientGrid = new ZDataGrid());
		}

		public new ZString VersioNumberVal { get { return base.VersioNumberVal.Text; } }
		public new ZString LicenseCode { get { return base.LicenseCode.Text; } }
		public new ZString VersionDate { get { return base.VersionDate.Text; } }
		public new ZString ReleaseText { get { return base.ReleaseText.Text; } }

		ZGlobal global;
		public override ZGlobal AppInstance => global ?? (global = new GlobalForTest());
	}
}
