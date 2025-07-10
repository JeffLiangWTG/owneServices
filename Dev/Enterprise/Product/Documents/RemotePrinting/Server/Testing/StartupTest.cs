using System;
using CargoWise.Data;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.RemotePrinting.Server.RPSCore;
using Microsoft.AspNet.SignalR;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.Testing
{
	class StartupTest : TransactionedTestCase
	{
		public void TestEnableDetailedErrors()
		{
			Assert("Startup should enable detailed errors", new StartupForTest().AppHubConfiguration_Exposed.EnableDetailedErrors);
		}

		public void TestMaxIncomingWebSocketMessageSizeIsSet()
		{
			var startup = new StartupForTest();

			DocumentsDataRegistry.Instance.WebPrintSignalRIncomingMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			using (DbHelper.OverrideConnectionForTest(TestConnection))
			{
				startup.SetupSignalRMaxIncomingWebSocketMessageSize();
			}

			Assert(GlobalHost.Configuration.MaxIncomingWebSocketMessageSize.HasValue);
			AssertEquals(15 * 1024, GlobalHost.Configuration.MaxIncomingWebSocketMessageSize);
		}

		public void TestMaxIncomingWebSocketMessageSizeIsNotSet()
		{
			var startup = new StartupForTest();

			DocumentsDataRegistry.Instance.WebPrintSignalRIncomingMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 0);
			using (DbHelper.OverrideConnectionForTest(TestConnection))
			{
				startup.SetupSignalRMaxIncomingWebSocketMessageSize();
			}

			Assert(!GlobalHost.Configuration.MaxIncomingWebSocketMessageSize.HasValue);
		}

		public void TestHandleDatabaseUpgradedExceptionInConfiguration()
		{
			var startup = new StartupForTest();

			using (DbHelper.SetExceptionToThrowInTest(new DatabaseUpgradedException(true)))
			{
				AssertNoExceptionThrown("Should handle DatabaseUpgradedException", () => { startup.SetupSignalRMaxIncomingWebSocketMessageSize(); });
			}

			using (DbHelper.SetExceptionToThrowInTest(new DatabaseUpgradeInProgressException()))
			{
				AssertNoExceptionThrown("Should handle DatabaseUpgradeInProgressException", () => { startup.SetupSignalRMaxIncomingWebSocketMessageSize(); });
			}

			using (DbHelper.SetExceptionToThrowInTest(new ArgumentException("abc")))
			{
				AssertExceptionThrown<ArgumentException>("Should not silence other exceptions", () => { startup.SetupSignalRMaxIncomingWebSocketMessageSize(); });
			}
		}

		public void TestSetDefaultMaxIncomingWebSocketMessageSizeIfDbUpgradeException()
		{
			var startup = new StartupForTest();

			DocumentsDataRegistry.Instance.WebPrintSignalRIncomingMaxSize.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 15);
			using (DbHelper.OverrideConnectionForTest(TestConnection))
			using (DbHelper.SetExceptionToThrowInTest(new DatabaseUpgradedException(true)))
			{
				startup.SetupSignalRMaxIncomingWebSocketMessageSize();
			}

			Assert(GlobalHost.Configuration.MaxIncomingWebSocketMessageSize.HasValue);
			AssertEquals("In case of db exception should use default value", DocumentsDataRegistry.Instance.WebPrintSignalRIncomingMaxSize.DefaultValue * 1024, GlobalHost.Configuration.MaxIncomingWebSocketMessageSize);
		}
	}

	class StartupForTest : Startup
	{
		public new void SetupSignalRMaxIncomingWebSocketMessageSize()
		{
			base.SetupSignalRMaxIncomingWebSocketMessageSize();
		}

		public HubConfiguration AppHubConfiguration_Exposed => AppHubConfiguration;
	}
}
