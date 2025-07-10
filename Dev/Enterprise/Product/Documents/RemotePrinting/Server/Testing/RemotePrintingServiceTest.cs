using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Script.Services;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.RemotePrinting.Server.JobPrinting;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Moq;
using Moq.Protected;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class RemotePrintingServiceTest : TestCaseWithFactory
	{
		[HttpContextEnabledTest]
		public void TestCheckClientUpdate_WithNotificationMessage()
		{
			var service = new RemotePrintingService();
			service.Header = new RemotePrintingSoapHeader
			{
				VersionInfo = "2.138.0",
				OSVersion = "10.0",
				InstalledDotNetVersion = "4.8",
				LocalMachineName = "Test"
			};
			var clientUpdate = service.CheckClientUpdate();

			AssertEquals("0.0.0", clientUpdate.Version);
			AssertEquals("There is a Print Server with outdated RemotePrinting Client application that should be manually re-installed using current installation distributive.", clientUpdate.Link);
		}

		[HttpContextEnabledTest]
		public void TestCheckClientUpdate2WithHTTPSWebServiceAddress()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			AssertEquals("IsHostedWithCargowise", true, EnvProxy.IsHostedWithCargowise);

			var originContext = HttpContext.Current;
			try
			{
				var testContext = new HttpContext(new HttpRequest(string.Empty, "http://test.com:80", string.Empty), new HttpResponse(new StringWriter()));
				HttpContext.Current = testContext;

				var serviceProvider = new Mock<IServiceProvider>();
				ConfigurationManager.AppSettings[AppSettingsHelper.Keys.ClientInstallationFolder] = string.Empty;
				var serviceUrl = string.Empty;

				var service = new Mock<RemotePrintingService>() { CallBase = true };
				service.Protected().Setup<string>("GetCurrentWebServiceAddress", ItExpr.IsAny<Uri>(), ItExpr.IsAny<DbConnection>()).Returns("https://test.com:443|host:test.com");
				service.Protected().Setup<ClientUpdate>("GetClientUpdate", ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<string>(), ItExpr.IsAny<ClientRequirements>(), ItExpr.IsAny<ClientUpdateErrorNotifier>(), ItExpr.IsAny<ClientInfo>(), ItExpr.IsAny<bool>())
					.Returns(new ClientUpdate())
					.Callback<string, string, string, ClientRequirements, ClientUpdateErrorNotifier, ClientInfo, bool>((folder, installation, url, requirements, errorNotifier, clientInfo, reportMissingClientInfo) =>
					{
						serviceUrl = url;
					});
				var clientInfo = new ClientInfo()
				{
					ClientVersion = "2.138.0",
					OSVersion = "10.0",
					DotNetVersion = "4.8",
					MachineName = "Test"
				};
				var clientUpdate = service.Object.CheckClientUpdate2(clientInfo);

				AssertEquals("Should force to use HTTPS url", "https://test.com:443/webapp/", serviceUrl);
			}
			finally
			{
				HttpContext.Current = originContext;
			}
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestSetJobSuccess()
		{
			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;
			var service = new Mock<RemotePrintingService>() { CallBase = true };
			service.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			service.Protected().Setup("NewPrintServer").Callback(() =>
			{
				callNewPrintServer = true;
			});

			SetHttpContextUser("CWSupport-XYZ");
			service.Object.SetJobSuccess(new List<Guid>());
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			service.Object.SetJobSuccess(new List<Guid>());
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestSetQueuesForSupportUser()
		{
			SetHttpContextUser("CWSupport-XYZ");
			var service = new RemotePrintingService();
			string serverName = "Server1";

			List<string> printQueues = new List<string>() { "Queue1" + DateTime.Now.ToString("HH:mm:ss.ffffff") };

			service.SetQueues(serverName, printQueues);

			var printQueueInDb = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_QueueName, printQueues[0]));

			AssertNotEquals(printQueues[0], printQueueInDb?.SQ_QueueName.ToString());

			SetHttpContextUser("XYZW");
			printQueues[0] = "Queue2" + DateTime.Now.ToString("HH:mm:ss.ffffff");

			service.SetQueues(serverName, printQueues);

			printQueueInDb = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_QueueName, printQueues[0]));

			AssertEquals(printQueues[0], printQueueInDb?.SQ_QueueName.ToString());
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestBeginGetJobsForSupportUser()
		{
			var serverName = "Server1";

			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;
			var service = new Mock<RemotePrintingService>() { CallBase = true };
			service.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			service.Protected().Setup("NewPrintServer").Callback(() =>
			{
				callNewPrintServer = true;
			});
			var callback = new AsyncCallback(CallBackMethod);

			SetHttpContextUser("CWSupport-XYZ");
			service.Object.BeginGetJobs(serverName, callback, service);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			service.Object.BeginGetJobs(serverName, callback, service);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);

			void CallBackMethod(IAsyncResult result)
			{
				//do nothing
			}
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestSetJobFailureV2ForSupportUser()
		{
			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;

			var service = new Mock<RemotePrintingService>() { CallBase = true };
			service.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			service.Protected().Setup("NewPrintServer").Callback(() =>
			{
				callNewPrintServer = true;
			});

			SetHttpContextUser("CWSupport-XYZ");
			service.Object.SetJobFailureV2(new List<PrintJobFailed>());
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			service.Object.SetJobFailureV2(new List<PrintJobFailed>());
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestEndGetJobsForSupportUser()
		{
			var result = new DummyAsyncResult();
			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;

			var serviceMock = new Mock<RemotePrintingService>() { CallBase = true };
			serviceMock.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			serviceMock.Protected().Setup("NewPrintServer").Callback(() =>
			{
				callNewPrintServer = true;
			});

			SetHttpContextUser("CWSupport-XYZ");
			serviceMock.Object.EndGetJobs(result);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			serviceMock.Object.EndGetJobs(result);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestEndGetJobsCompressedForSupportUser()
		{
			IAsyncResult result = new DummyAsyncResult();
			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;

			var serviceMock = new Mock<RemotePrintingService>() { CallBase = true };
			serviceMock.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			serviceMock.Protected().Setup("NewPrintServer").Callback(() =>
			{
				callNewPrintServer = true;
			});

			SetHttpContextUser("CWSupport-XYZ");
			serviceMock.Object.EndGetJobsCompressed(result);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			serviceMock.Object.EndGetJobsCompressed(result);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);
		}

		class DummyAsyncResult : IAsyncResult, IDisposable
		{
			readonly AutoResetEvent asyncWaitHandleEvent = new AutoResetEvent(false);

			public bool IsCompleted => throw new NotImplementedException();
			public WaitHandle AsyncWaitHandle => asyncWaitHandleEvent;
			public object AsyncState => new object();
			public bool CompletedSynchronously => throw new NotImplementedException();

			WaitHandle IAsyncResult.AsyncWaitHandle => throw new NotImplementedException();

			public void Dispose() => asyncWaitHandleEvent.Dispose();
			public void Complete() => asyncWaitHandleEvent.Set();
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestGetChangedQueuesForSupportUser()
		{
			var serverName = "Server1";

			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;
			var serviceMock = new Mock<RemotePrintingService>() { CallBase = true };
			serviceMock.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			serviceMock.Protected().Setup("NewPrintServer").Callback(() =>
			{
				callNewPrintServer = true;
			});

			SetHttpContextUser("CWSupport-XYZ");
			serviceMock.Object.GetChangedQueues(serverName, new List<string>());
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			serviceMock.Object.GetChangedQueues(serverName, new List<string>());
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestGetCNSWClientSettingForSupportUser()
		{
			var machineName = "Machine1";

			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;
			var service = new Mock<RemotePrintingService>() { CallBase = true };
			service.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			service.Protected().Setup("NewCNSWServer").Callback(() =>
			{
				callNewPrintServer = true;
			});

			SetHttpContextUser("CWSupport-XYZ");
			service.Object.GetCNSWClientSetting(machineName);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			service.Object.GetCNSWClientSetting(machineName);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestTWNCATKServerForSupportUser()
		{
			var machineName = "Machine1";

			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;
			var service = new Mock<RemotePrintingService>() { CallBase = true };
			service.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			service.Protected().Setup("NewTWNCATKServer").Callback(() =>
			{
				callNewPrintServer = true;
			});

			SetHttpContextUser("CWSupport-XYZ");
			service.Object.GetTWNCATKClientSetting(machineName);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			service.Object.GetTWNCATKClientSetting(machineName);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestCLSMSClientSettingForSupportUser()
		{
			var machineName = "Machine1";

			var callIsCurrentUserSupportUser = false;
			var callNewPrintServer = false;
			var service = new Mock<RemotePrintingService>() { CallBase = true };
			service.Protected().Setup("IsCurrentUserSupportUser").Callback(() =>
			{
				callIsCurrentUserSupportUser = true;
			});
			service.Protected().Setup("NewCLSMSServer").Callback(() =>
			{
				callNewPrintServer = true;
			});

			SetHttpContextUser("CWSupport-XYZ");
			service.Object.GetCLSMSClientSetting(machineName);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer was not called", false, callNewPrintServer);

			callIsCurrentUserSupportUser = false;
			callNewPrintServer = false;

			SetHttpContextUser("XYZW");
			service.Object.GetCLSMSClientSetting(machineName);
			AssertEquals("IsCurrentUserSupportUser has been called", true, callIsCurrentUserSupportUser);
			AssertEquals("NewPrintServer has been called", true, callNewPrintServer);
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestBeginGetJobsCompressedForSupportUser()
		{
			SetHttpContextUser("CWSupport-XYZ");
			var service = new RemotePrintingService();
			string serverName = "Server1";

			var callback = new AsyncCallback(CallBackMethod);

			var printJobAsyncResult = (PrintJobAsyncResult)service.BeginGetJobsCompressed(serverName, callback, service);
			AssertNotEquals("Servername won t be set as we don t call PrintServer().BeginGetPrintJobs", "Server1", printJobAsyncResult.ServerName);

			SetHttpContextUser("XYZW");

			printJobAsyncResult = (PrintJobAsyncResult)service.BeginGetJobsCompressed(serverName, callback, service);

			AssertEquals("Server1", printJobAsyncResult.ServerName);

			void CallBackMethod(IAsyncResult result)
			{
				//do nothing GetJobsCompressed2
			}
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestSetQueuesExForSupportUser()
		{
			SetHttpContextUser("CWSupport-XYZ");
			var service = new RemotePrintingService();
			string serverName = "Server1";
			List<PrintQueueInfo> printQueues = new List<PrintQueueInfo>() { new PrintQueueInfo() {
														Name = "Queue1" + DateTime.Now.ToString("HH:mm:ss.ffffff"),
														IsSuspectedSurrogate = false
													} };

			service.SetQueuesEx(serverName, printQueues);

			var printQueueInDb = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_QueueName, printQueues[0].Name));
			AssertNotEquals(printQueues[0].Name, printQueueInDb?.SQ_QueueName);

			SetHttpContextUser("XYZW");

			printQueues[0].Name = "Queue2" + DateTime.Now.ToString("HH: mm:ss.ffffff");
			service.SetQueuesEx(serverName, printQueues);

			printQueueInDb = Factory.LoadTop1<StmPrintQueue>(new ZQuery(StmPrintQueueSchema.SQ_QueueName, printQueues[0].Name));

			AssertEquals(printQueues[0].Name, printQueueInDb?.SQ_QueueName);
		}

		[UseSnapshotProtection]
		[HttpContextEnabledTest]
		public void TestSendNotificationEmailForSupportUser()
		{
			SetHttpContextUser("CWSupport-XYZ");
			var service = new RemotePrintingService();
			string subject = "subject1" + DateTime.Now.ToString("HH:mm:ss.ffffff");
			service.SendNotificationEmail("subject" + subject, "some body");

			int emailsInDB = MailItemsCount(subject);
			AssertEquals(0, emailsInDB);

			subject = "subject2" + DateTime.Now.ToString("HH:mm:ss.ffffff");
			SetHttpContextUser("XYZW");

			service.SendNotificationEmail("subject" + subject, "some test body");

			emailsInDB = MailItemsCount(subject);
			AssertEquals(0, emailsInDB);
		}

		public void TestShouldEnableScriptServiceForRemotePrintingService()
		{
			var services = typeof(RemotePrintingService).GetCustomAttributes(typeof(ScriptServiceAttribute), false);

			AssertNotNull("RemotePrintingService should add ScriptServiceAttribute", services);
		}

		public void TestShouldEnableScriptMethodForNudge2()
		{
			var methods = typeof(RemotePrintingService).GetMethods().Where(m => m.GetCustomAttributes(typeof(ScriptMethodAttribute), false).Length > 0).ToArray();

			AssertEquals(1, methods.Length);
			AssertEquals("Nudge2 should add ScriptMethodAttribute", "Nudge2", methods[0].Name);
		}

		void SetHttpContextUser(string userName)
		{
			HttpContext.Current.User = new GenericPrincipal(new GenericIdentity(userName, "Enterprise.RemotePrinting.Server.Digest"), Array.Empty<string>());
		}

		int MailItemsCount(string subject)
		{
			return (int)TestConnection.Command(string.Format("SELECT COUNT(*) FROM dbo.MailDBItems WHERE MI_Subject = '{0}'", subject)).ExecuteScalar();
		}
	}
}
