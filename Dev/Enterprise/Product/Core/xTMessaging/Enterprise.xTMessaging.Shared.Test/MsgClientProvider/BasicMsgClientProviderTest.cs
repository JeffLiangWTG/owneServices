using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.xTMessaging.Business;
using Grpc.Core;
using Xware.Xt.Grpc.Config;

namespace Enterprise.xTMessaging.Shared.Test
{
	sealed class BasicMsgClientProviderTest : TestCaseWithFactory
	{
		public void TestHandleExceptions_EmptyConfiguration()
		{
			var tester = new BasicMsgClientProvider("", "", "", "", timeSpan, new CancellationToken());
			AssertEquals(null, tester.MsgClient);
			AssertEquals(BasicMsgClientProvider.InvalidConfigMessage, tester.ErrorMessage);
		}

		public void TestHandleExceptions_XtToObjFilter()
		{
			var shortName = "ASDFGH";
			var guid = Guid.NewGuid();
			var prefix = "xt-application:";
			var msgClientProvider = new BasicMsgClientProvider(TestUtils.ConfigurationConnect, TestUtils.ConfigurationCa, shortName, TestUtils.ConfigurationPassword, timeSpan, new CancellationToken());
			AssertEquals($"{prefix}{shortName}", msgClientProvider.XtToObjFilter);

			msgClientProvider = new BasicMsgClientProvider(TestUtils.ConfigurationConnect, TestUtils.ConfigurationCa, $"{prefix}{shortName}", TestUtils.ConfigurationPassword, timeSpan, new CancellationToken());
			AssertEquals($"{prefix}{shortName}", msgClientProvider.XtToObjFilter);

			msgClientProvider = new BasicMsgClientProvider(TestUtils.ConfigurationConnect, TestUtils.ConfigurationCa, guid.ToString(), TestUtils.ConfigurationPassword, timeSpan, new CancellationToken());
			AssertEquals($"{prefix}{guid}", msgClientProvider.XtToObjFilter);

			msgClientProvider = new BasicMsgClientProvider(TestUtils.ConfigurationConnect, TestUtils.ConfigurationCa, $"{prefix}{guid}", TestUtils.ConfigurationPassword, timeSpan, new CancellationToken());
			AssertEquals($"{prefix}{guid}", msgClientProvider.XtToObjFilter);
		}

		public void TestHandleExceptions_InvalidAddress()
		{
			var config = TestUtils.GetTestConfiguration();
			config.Connect = "notlocalhost";

			AssertThrowMsgServerConnectionException(config, Utils.RpcErrorReportKey, Utils.RpcErrorMessage);
		}

		public void TestHandleExceptions_ValidAddressInvalidPort()
		{
			var config = TestUtils.GetTestConfiguration();
			config.Connect = "localhost:xxx";

			AssertThrowMsgServerConnectionException(config, Utils.RpcErrorReportKey, Utils.RpcErrorMessage);
		}

		public void TestHandleExceptions_LogOnTimeoutException()
		{
			var config = TestUtils.GetTestConfiguration();
			var provider = new MsgClientProviderForTimeoutErrorTesting(TestUtils.ConfigurationConnect, TestUtils.ConfigurationCa, TestUtils.ConfigurationUri, TestUtils.ConfigurationPassword, timeSpan, new CancellationToken());
			var exception = AssertExceptionThrown<MsgServerConnectionException>(() => { _ = provider.MsgClient; });

			var errorMsg = Utils.GetDeadlineExceededErrorMessage(timeSpan);
			AssertStartsWith($"should be recognized as {Utils.DeadlineExceededErrorReportKey} issue", "LogOn" + errorMsg, provider.ErrorMessage);
			AssertEquals(Utils.DeadlineExceededErrorReportKey, exception.Message);
		}

		public void TestHandleExceptions_ValidAddressWithInvalidEndPoint()
		{
			AssertThrowMsgServerConnectionException(TestUtils.GetTestConfiguration(), Utils.RpcErrorReportKey, Utils.RpcErrorMessage);
		}

		void AssertThrowMsgServerConnectionException(Configuration config, string errorKey, string errorMessage)
		{
			var tester = new BasicMsgClientProvider(config.Connect, config.CA, config.Application.URI, config.Application.Password, timeSpan, new CancellationToken());

			var exception = AssertExceptionThrown<MsgServerConnectionException>(() => { _ = tester.MsgClient; });

			AssertStartsWith($"should be recognized as {errorKey} issue", errorMessage, tester.ErrorMessage);
			AssertEquals(errorKey, exception.Message);
		}

		public void TestTearDown()
		{
			var tester = new BasicMsgClientProvider("localhost", "", "", "", timeSpan, new CancellationToken());
			AssertEquals(null, tester.MsgClient);
			AssertNoExceptionThrown(() => tester.TearDown());
		}

		public void TestTearDown_LogOffTimeoutException()
		{
			var provider = new MsgClientProviderForTimeoutErrorTesting(TestUtils.ConfigurationConnect, TestUtils.ConfigurationCa, TestUtils.ConfigurationUri, TestUtils.ConfigurationPassword, timeSpan,new CancellationToken());
			provider.SetSessionForTesting();
			var exception = AssertExceptionThrown<MsgServerConnectionException>(() => { provider.TearDown(); });

			var errorMsg = Utils.GetDeadlineExceededErrorMessage(timeSpan);
			AssertStartsWith("should be recognized as timeout issue", "LogOff" + errorMsg, provider.ErrorMessage);
			AssertEquals(Utils.DeadlineExceededErrorReportKey, exception.Message);
		}

		class MsgClientProviderForTimeoutErrorTesting : BasicMsgClientProvider
		{
			public MsgClientProviderForTimeoutErrorTesting(string connect, string ca, string uri, string password, TimeSpan timeout, CancellationToken token)
			: base(connect, ca, uri, password, timeout, token)
			{ }

			protected override CallOptions GetCallOptions() => throw new Grpc.Core.RpcException(new Status(StatusCode.DeadlineExceeded, "DeadlineExceeded Exception"));

			public void SetSessionForTesting() => session = new GRPCInstance();
		}

		readonly TimeSpan timeSpan = TimeSpan.FromSeconds(DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.Value);
	}

	sealed class MsgClientProviderNonTransactionedTest : NonTransactionedTestCase
	{
		/// <summary>
		/// This ideally would test that TearDown does not open a connection,
		/// but setting up a MsgClientProvider that can Initialize and TearDown properly was too hard.
		/// Since TearDown calls GetCallOptions as the only code that may open a DbConnection (for the XTServerMessageTimeoutInSeconds registry)
		/// we test GetCallOptions instead.
		/// </summary>
		public void TestGetCallOptions_DoesNotOpenDbConnection()
		{
			var provider = new MsgClientProviderForTest(TestUtils.ConfigurationConnect, TestUtils.ConfigurationCa, TestUtils.ConfigurationUri, TestUtils.ConfigurationPassword, TimeSpan.FromSeconds(DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.Value), new CancellationToken());
			Db.Connection.CloseConnection();
			ZArchitecture.Environment.RegistryItemDictionary.Instance.PurgeAll();
			provider.GetCallOptions_Exposed();
			AssertEquals("ConnectionState", System.Data.ConnectionState.Closed, Db.Connection.State);
		}

		class MsgClientProviderForTest : BasicMsgClientProvider
		{
			public MsgClientProviderForTest(string connect, string ca, string uri, string password, TimeSpan timeout, CancellationToken token)
				: base(connect, ca, uri, password, timeout, token)
			{ }
			public void GetCallOptions_Exposed() => base.GetCallOptions();
		}
	}
}
