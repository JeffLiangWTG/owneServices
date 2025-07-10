using System;
using CargoWise.Common;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ClientOverrideInitUninitTestListenerTest : TestCase
	{
		public void TestResetSecurityAfterDisposeTemporaryUserContextForClientExtension()
		{
			var client = ClientOverrideInitUninitTestListener.Instance;

			using (new DisposableAction(() => client.IsTestClientAssembly = true, () => client.IsTestClientAssembly = false))
			{
				var clientHook = new TestClientHook();
				using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
				using (ClientHookLoader.Instance.OverrideClientHookForTest(clientHook))
				{
					client.StartTest(this, DateTime.Today);
					_ = EnvProxy.Instance.Security;
					AssertEquals("Pre-condition", true, (EnvProxy.Instance as IEnvironmentForTest).IsSecurityCreatedForTest);

					client.EndTest(this, DateTime.Today.AddSeconds(5));
					AssertEquals("Security instance should be reset", false, (EnvProxy.Instance as IEnvironmentForTest).IsSecurityCreatedForTest);
				}
			}
		}

		public void TestNotResetSecurityAfterDisposeTemporaryUserContext()
		{
			var client = ClientOverrideInitUninitTestListener.Instance;
			client.StartTest(this, DateTime.Today);
			_ = EnvProxy.Instance.Security;
			AssertEquals("Pre-condition", true, (EnvProxy.Instance as IEnvironmentForTest).IsSecurityCreatedForTest);

			client.EndTest(this, DateTime.Today.AddSeconds(5));
			AssertEquals("Security instance should NOT be reset", true, (EnvProxy.Instance as IEnvironmentForTest).IsSecurityCreatedForTest);
		}

		public void TestEndTest_NoErrorReportGeneratedAfterDisposingTemporaryUserContext()
		{
			var client = ClientOverrideInitUninitTestListener.Instance;

			using (new DisposableAction(() => client.IsTestClientAssembly = true, () => client.IsTestClientAssembly = false))
			{
				var clientHook = new TestClientHook();
				using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
				using (ClientHookLoader.Instance.OverrideClientHookForTest(clientHook))
				{
					ErrorReporter.Clear();
					client.StartTest(this, DateTime.Today);

					var userContext = EnvProxy.Instance.CurrentUserContext;
					EnvProxy.Instance.ClearUserContext();
					EnvProxy.Instance.SetUserContext(userContext);

					client.EndTest(this, DateTime.Today.AddSeconds(5));
					AssertEquals(0, ErrorReporter.TotalErrorCount);
				}
			}
		}
	}
}
