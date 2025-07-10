using System;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ClientDllCheckerTest : TransactionedTestCase
	{
		public void TestCheckRegistry_NotExpected_NotLoaded()
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = null;
			var result = ClientDllChecker.CheckRegistry();
			AssertEquals("IsOKToRun?", true, result.IsOKToRun);
			AssertEquals("ErrorMessage", null, result.ErrorMessage);
			AssertEquals("ExpectedClientDLL", null, EnvProxy.Instance.Registry.ExpectedClientDLL);
			AssertEquals("Loaded assembly client", Clients.None, ClientHookLoader.Instance.Client);
		}

		public void TestCheckRegistry_NotExpected_Loaded()
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = null;
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var result = ClientDllChecker.CheckRegistry();
				CombineAssertions(() =>
				{
					AssertEquals("IsOKToRun?", false, result.IsOKToRun);
					AssertEquals("ErrorMessage", true, result.ErrorMessage.Contains("is configured to run without any client-specific module, but it has loaded ('ZClientEDI').", StringComparison.OrdinalIgnoreCase));
					AssertEquals("ExpectedClientDLL", null, EnvProxy.Instance.Registry.ExpectedClientDLL);
					AssertEquals("Loaded assembly client", Clients.EDI, ClientHookLoader.Instance.Client);
				});
			}
		}

		public void TestCheckRegistry_Expected_Loaded()
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var result = ClientDllChecker.CheckRegistry();
				AssertEquals("IsOKToRun?", true, result.IsOKToRun);
				AssertEquals("ErrorMessage", null, result.ErrorMessage);
				AssertEquals("ExpectedClientDLL", "ZClientEDI", EnvProxy.Instance.Registry.ExpectedClientDLL);
				AssertEquals("Loaded assembly client", Clients.EDI, ClientHookLoader.Instance.Client);
			}
		}

		public void TestCheckRegistry_Expected_NotLoaded()
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientXXX";
			var result = ClientDllChecker.CheckRegistry();
			AssertEquals("IsOKToRun?", false, result.IsOKToRun);
			AssertEquals("ErrorMessage", true, result.ErrorMessage.Contains("is configured to run with the 'ZClientXXX' client-specific module, but it has not loaded.", StringComparison.OrdinalIgnoreCase));
			AssertEquals("ExpectedClientDLL", "ZClientXXX", EnvProxy.Instance.Registry.ExpectedClientDLL);
			AssertEquals("Loaded assembly client", Clients.None, ClientHookLoader.Instance.Client);

			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientGEO";
			result = ClientDllChecker.CheckRegistry();
			AssertEquals("IsOKToRun?", true, result.IsOKToRun);
			AssertEquals("ErrorMessage", null, result.ErrorMessage);
			AssertEquals("ExpectedClientDLL", null, EnvProxy.Instance.Registry.ExpectedClientDLL);
			AssertEquals("Loaded assembly client", Clients.None, ClientHookLoader.Instance.Client);
		}

		public void TestCheckRegistry_ExpectedOne_LoadedDifferent()
		{
			EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientSomethingElse";
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var result = ClientDllChecker.CheckRegistry();
				AssertEquals("IsOKToRun?", false, result.IsOKToRun);
				AssertEquals("ErrorMessage", true, result.ErrorMessage.Contains("is configured to run with the 'ZClientSomethingElse' client-specific module, but another one has loaded ('ZClientEDI').", StringComparison.OrdinalIgnoreCase));
				AssertEquals("ExpectedClientDLL", "ZClientSomethingElse", EnvProxy.Instance.Registry.ExpectedClientDLL);
				AssertEquals("Loaded assembly client", Clients.EDI, ClientHookLoader.Instance.Client);
			}
		}
	}
}
