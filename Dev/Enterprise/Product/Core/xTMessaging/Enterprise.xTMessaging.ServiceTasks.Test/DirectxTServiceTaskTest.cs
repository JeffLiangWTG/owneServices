using System;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.xTMessaging.ServiceTasks.Test
{
	sealed class DirectxTServiceTaskTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			TestUtils.InsertRefSysConfig(Factory);
		}
		public void TestRunTask()
		{
			var serviceTask = new DirectxTServiceTaskForTest(null);
			AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
			AssertEquals("Should not have any retries.", 1, serviceTask.ProcessTimes);
		}

		public void TestRunTaskWithMsgServerConnectionException()
		{
			using (DirectxTMessagingRegistry.Instance.XTServerOutageExpiryTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var serviceTask = new DirectxTServiceTaskForTest(() => throw new MsgServerConnectionException("TEST MsgServerConnectionException", new Exception("Inner Exception Test")));

				var logger = new TestServiceLogger();
				serviceTask.ServiceLogger = logger;

				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertEquals("Retries should be dealt with outside the service task", serviceTask.ProcessTimes, 1);
				Assert("Should log the exception message.", logger.ToString().Contains("DirectxT Service Task Error. Message: Inner Exception Test"));
			}
		}

		public void TestRunTaskWithMsgSessionTimeoutException()
		{
			using (DirectxTMessagingRegistry.Instance.XTServerOutageExpiryTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var serviceTask = new DirectxTServiceTaskForTest(() => throw new MsgSessionTimeoutException("TEST MsgServerConnectionException", new Exception("Inner Exception Test")));

				var logger = new TestServiceLogger();
				serviceTask.ServiceLogger = logger;

				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertEquals("Retries should be dealt with outside the service task", serviceTask.ProcessTimes, 1);
				Assert("Should log the exception message.", logger.ToString().Contains("DirectxT Service Task Error. Message: Inner Exception Test"));
			}
		}

		public void TestRunTaskWithMsgServerConnectionExceptionIncludingErrorDetail()
		{
			using (DirectxTMessagingRegistry.Instance.XTServerOutageExpiryTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var serviceTask = new DirectxTServiceTaskForTest(() => throw new MsgServerConnectionException("TEST MsgServerConnectionException", new Exception("Inner Exception Test"), "Error details"));

				var logger = new TestServiceLogger();
				serviceTask.ServiceLogger = logger;

				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertEquals("Retries should be dealt with outside the service task", serviceTask.ProcessTimes, 1);
				Assert("Should log the Error details", logger.ToString().Contains("DirectxT Service Task Error. Message: Error details"));
			}
		}

		[TestDate(2020, 06, 12)]
		public void TestRunTaskInProductionEnv_FormatExceptionKey()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "DAT";
			registrationKey.ServerCodeForTest = "JFL";
			registrationKey.PasswordForTest = "xyz123";

			using (DirectxTMessagingRegistry.Instance.XTServerOutageExpiryTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			{
				var serviceTask = new DirectxTServiceTaskForTest(() => throw new MsgServerConnectionException("Direct xT Client - Rpc Error", new Exception("Grpc.Core.RpcException: Status(StatusCode=\"Unavailable\", Detail=\"failed to connect to all addresses\", DebugException=\"Grpc.Core.Internal.CoreErrorDetailException: ")), true);

				var config = new CW1RegistryConfigurationProvider().GetConfiguration();
				ConfigurationUtils.StandardizeConfig(config);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertNull("No exception reported on first run, outage expiry time hasn't been reached,", ErrorReporter.LastExceptionReported);
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(2);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertContains("Should report the exception in production which include PRD xTConnectionType", "Direct xT Client - Rpc Error\r\nfailed to connect to all addresses on PRD xT Server\r\n", ErrorReporter.LastKeyReported);
			}
			ErrorReporter.Clear();

			using (DirectxTMessagingRegistry.Instance.XTServerOutageExpiryTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				var serviceTask = new DirectxTServiceTaskForTest(() => throw new MsgServerConnectionException("Direct xT Client - Rpc Error", new Exception("Grpc.Core.RpcException: Status(StatusCode=\"Unavailable\", Detail=\"failed to connect to all addresses\", DebugException=\"Grpc.Core.Internal.CoreErrorDetailException: ")), true);

				var config = new CW1RegistryConfigurationProvider().GetConfiguration();
				ConfigurationUtils.StandardizeConfig(config);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertNull("No exception reported on first run, outage expiry time hasn't been reached,", ErrorReporter.LastExceptionReported);
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(2);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertContains("Should report the exception in production which include TST xTConnectionType", "Direct xT Client - Rpc Error\r\nfailed to connect to all addresses on TST xT Server\r\n", ErrorReporter.LastKeyReported);
			}

			ErrorReporter.Clear();
		}

		[TestDate(2020, 06, 12)]
		public void TestRunTaskWithOperationCanceledException()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "DAT";
			registrationKey.ServerCodeForTest = "JFL";
			registrationKey.PasswordForTest = "xyz123";

			using (DirectxTMessagingRegistry.Instance.XTServerOutageExpiryTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtProduction.Code))
			{
				var serviceTask = new DirectxTServiceTaskForTest(() => throw new OperationCanceledException(), true);

				var config = new CW1RegistryConfigurationProvider().GetConfiguration();
				ConfigurationUtils.StandardizeConfig(config);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertNull("No exception reported on first run, outage expiry time hasn't been reached,", ErrorReporter.LastExceptionReported);
				TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(2);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertContains("Should report the exception in production which include PRD xTConnectionType", "The operation was canceled", ErrorReporter.LastKeyReported);
			}
			ErrorReporter.Clear();
		}

		public void TestRunTaskWithNormalException()
		{
			var serviceTask = new DirectxTServiceTaskForTest(() => throw new Exception("TEST Normal Exception"));

			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;

			AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
			AssertEquals("Should not have any retries.", 1, serviceTask.ProcessTimes);

			var fullLog = logger.ToString();

			Assert("Should log the exception message.", fullLog.Contains("DirectxT Service Task Error. Message: TEST Normal Exception"));
			Assert("Should log the full exception details in trainning system.", fullLog.Contains("Error Details:"));
		}

		public void TestRunTaskInProductionEnv()
		{
			var serviceTask = new DirectxTServiceTaskForTest(() => throw new Exception("TEST Exception In Production Env"), true);
			AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
			AssertEquals("Should not have any retries.", 1, serviceTask.ProcessTimes);
			AssertContains("Should report the exception in production environment.", "Error when connecting from WTG internal system client", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		public void TestRunTaskInProductionEnv_ExceptionContainsClientInfo()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "DAT";
			registrationKey.ServerCodeForTest = "JFL";
			registrationKey.PasswordForTest = "xyz123";
			registrationKey.HostedLocationForTest = "NCW";

			var serviceTask = new DirectxTServiceTaskForTest(() => throw new Exception("TEST Exception In Production Env"), true);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				var config = new CW1RegistryConfigurationProvider().GetConfiguration();
				ConfigurationUtils.StandardizeConfig(config);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertEquals("Should not have any retries.", 1, serviceTask.ProcessTimes);
				AssertContains("Should report the exception in production which include client info and server info", "Error when connecting from self-hosted client EnterpriseCode: DAT ServerCode: JFL", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();

			registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "DAT";
			registrationKey.ServerCodeForTest = "JFL";
			registrationKey.HostedLocationForTest = "Sydney";

			serviceTask = new DirectxTServiceTaskForTest(() => throw new Exception("TEST Exception In Production Env"), true);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				var config = new CW1RegistryConfigurationProvider().GetConfiguration();
				ConfigurationUtils.StandardizeConfig(config);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertEquals("Should not have any retries.", 1, serviceTask.ProcessTimes);
				AssertContains("Should report the exception in production which include client info and server info", "Error when connecting from Sydney hosted client EnterpriseCode: DAT ServerCode: JFL", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();

			registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "WTL";
			registrationKey.ServerCodeForTest = "JFL";

			serviceTask = new DirectxTServiceTaskForTest(() => throw new Exception("TEST Exception In Production Env"), true);

			using (DirectxTMessagingRegistry.Instance.ConnectionToXTServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ConnectionToXTServerOptions.XtTest.Code))
			{
				var config = new CW1RegistryConfigurationProvider().GetConfiguration();
				ConfigurationUtils.StandardizeConfig(config);
				AssertNoExceptionThrown(() => { serviceTask.RunTask(); });
				AssertEquals("Should not have any retries.", 1, serviceTask.ProcessTimes);
				AssertContains("Should report the exception in production which include client info and server info", "Error when connecting from WTG internal system client EnterpriseCode: WTL ServerCode: JFL", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		public void TestRunTaskwithLog()
		{
			var serviceTask = new DirectxTServiceTaskForTest(null);

			var logger = new TestServiceLogger();
			serviceTask.ServiceLogger = logger;

			AssertNoExceptionThrown(() => { serviceTask.RunTask(); });

			var fullLog = logger.ToString();

			Assert("Should log task start.", fullLog.Contains("DirectxT Service Task start"));
		}
	}

	sealed class DirectxTServiceTaskForTest : DirectxTServiceTask
	{
		public DirectxTServiceTaskForTest(Action runTaskAction, bool? isProductionDatabaseOverridden = null)
			: base()
		{
			ProcessTimes = 0;

			this.runTaskAction = runTaskAction;
			this.isProductionDatabaseOverridden = isProductionDatabaseOverridden;

			ServiceLogger = new TestServiceLogger();
		}

		readonly Action runTaskAction;
		readonly bool? isProductionDatabaseOverridden;

		public int ProcessTimes { get; set; }
		protected override DateTime OutageStartTime { get; set; }

		protected override void RunTaskCore(CancellationToken token)
		{
			ProcessTimes++;
			runTaskAction?.Invoke();
		}

		protected override IInterchangeProcessor GetMessageProcessor() => null;

		protected override bool IsProduction() => isProductionDatabaseOverridden ?? base.IsProduction();
	}
}
