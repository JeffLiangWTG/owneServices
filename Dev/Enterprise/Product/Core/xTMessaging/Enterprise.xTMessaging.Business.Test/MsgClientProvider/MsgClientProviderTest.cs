using System;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.xTMessaging.Shared;
using Enterprise.xTMessaging.Shared.Test;
using Grpc.Core;
using Xware.Xt.Grpc.Config;
using SharedUtils = Enterprise.xTMessaging.Shared.Utils;

namespace Enterprise.xTMessaging.Business.Test
{
	sealed class MsgClientProviderTest : TestCaseWithFactory
	{
		public void TestHandleExceptions_EmptyConfiguration()
		{
			var tester = new MsgClientProvider(new Configuration(), new CancellationToken());
			AssertEquals(null, tester.MsgClient);
			AssertEquals(MsgClientProvider.InvalidConfigMessage, tester.ErrorMessage);
		}

		public void TestHandleExceptions_XtToObjFilter()
		{
			var config = TestUtils.GetTestConfiguration();

			CombineAssertions(() =>
			{
				config.Application.URI = "ASDFGH";
				var msgClientProvider = new MsgClientProvider(config, new CancellationToken());
				AssertEquals("xt-application:ASDFGH", msgClientProvider.XtToObjFilter);

				config.Application.URI = "xt-application:ASDFGH";
				msgClientProvider = new MsgClientProvider(config, new CancellationToken());
				AssertEquals("xt-application:ASDFGH", msgClientProvider.XtToObjFilter);

				config.Application.URI = "00000000-0000-0000-0000-000000000000";
				msgClientProvider = new MsgClientProvider(config, new CancellationToken());
				AssertEquals("xt-application:00000000-0000-0000-0000-000000000000", msgClientProvider.XtToObjFilter);

				config.Application.URI = "xt-application:{00000000-0000-0000-0000-000000000000}";
				msgClientProvider = new MsgClientProvider(config, new CancellationToken());
				AssertEquals("xt-application:{00000000-0000-0000-0000-000000000000}", msgClientProvider.XtToObjFilter);
			});
		}

		public void TestHandleExceptions_InvalidAddress()
		{
			var config = TestUtils.GetTestConfiguration();
			config.Connect = "notlocalhost";

			AssertThrowMsgServerConnectionException(config, SharedUtils.RpcErrorReportKey, SharedUtils.RpcErrorMessage);
		}

		public void TestHandleExceptions_ValidAddressInvalidPort()
		{
			var config = TestUtils.GetTestConfiguration();
			config.Connect = "localhost:xxx";

			AssertThrowMsgServerConnectionException(config, SharedUtils.RpcErrorReportKey, SharedUtils.RpcErrorMessage);
		}

		public void TestHandleExceptions_LogOnTimeoutException()
		{
			var config = TestUtils.GetTestConfiguration();
			var provider = new MsgClientProviderForTimeoutErrorTesting(config, new CancellationToken());
			var exception = AssertExceptionThrown<MsgServerConnectionException>(() => { _ = provider.MsgClient; });

			var errorMsg = SharedUtils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(DirectxTMessagingRegistry.Instance.XTServerMessageTimeoutInSeconds.Value));
			AssertStartsWith($"should be recognized as {SharedUtils.DeadlineExceededErrorReportKey} issue", "LogOn" + errorMsg, provider.ErrorMessage);
			AssertEquals(SharedUtils.DeadlineExceededErrorReportKey, exception.Message);
		}

		public void TestHandleExceptions_ValidAddressWithInvalidEndPoint()
		{
			AssertThrowMsgServerConnectionException(TestUtils.GetTestConfiguration(), SharedUtils.RpcErrorReportKey, SharedUtils.RpcErrorMessage);
		}

		void AssertThrowMsgServerConnectionException(Configuration config, string errorKey, string errorMessage)
		{
			var tester = new MsgClientProvider(config, new CancellationToken());

			var exception = AssertExceptionThrown<MsgServerConnectionException>(() => { _ = tester.MsgClient; });

			AssertStartsWith($"should be recognized as {errorKey} issue", errorMessage, tester.ErrorMessage);
			AssertEquals(errorKey, exception.Message);
		}

		public void TestTearDown()
		{
			var tester = new MsgClientProvider(new Configuration() { Connect = "localhost" }, new CancellationToken());
			AssertEquals(null, tester.MsgClient);
			AssertNoExceptionThrown(() => tester.TearDown());
		}

		public void TestTearDown_LogOffTimeoutException()
		{
			var config = TestUtils.GetTestConfiguration();
			var provider = new MsgClientProviderForTimeoutErrorTesting(config, new CancellationToken());
			provider.SetSessionForTesting();
			var exception = AssertExceptionThrown<MsgServerConnectionException>(() => { provider.TearDown(); });

			var errorMsg = SharedUtils.GetDeadlineExceededErrorMessage(TimeSpan.FromSeconds(0));
			AssertStartsWith("should be recognized as timeout issue", "LogOff" + errorMsg, provider.ErrorMessage);
			AssertEquals(SharedUtils.DeadlineExceededErrorReportKey, exception.Message);
		}

		class MsgClientProviderForTimeoutErrorTesting : MsgClientProvider
		{
			public MsgClientProviderForTimeoutErrorTesting(Configuration config, CancellationToken token)
			: base(config, token)
			{ }

			protected override CallOptions GetCallOptions() => throw new RpcException(new Status(StatusCode.DeadlineExceeded, "DeadlineExceeded Exception"));

			public void SetSessionForTesting() => session = new GRPCInstance();
		}
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
			var provider = new MsgClientProviderForTest(TestUtils.GetTestConfiguration(), new CancellationToken());
			Db.Connection.CloseConnection();
			ZArchitecture.Environment.RegistryItemDictionary.Instance.PurgeAll();
			provider.GetCallOptions_Exposed();
			AssertEquals("ConnectionState", System.Data.ConnectionState.Closed, Db.Connection.State);
		}

		class MsgClientProviderForTest : MsgClientProvider
		{
			public MsgClientProviderForTest(Configuration config, CancellationToken token)
				: base(config, token)
			{ }
			public void GetCallOptions_Exposed() => base.GetCallOptions();
		}
	}
}
