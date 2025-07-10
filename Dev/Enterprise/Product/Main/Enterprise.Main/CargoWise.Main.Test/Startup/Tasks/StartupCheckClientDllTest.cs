using System;
using CargoWise.BrandManager;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Test.Utilities;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Startup.Testing
{
	sealed class StartupCheckClientDllTest : AbstractApplicationStartupTaskTest<StartupCheckClientDll>
	{
		public void TestOkToRun()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(null))
			{
				EnvProxy.Instance.Registry.ExpectedClientDLL = null;
				bool result = new StartupCheckClientDll().Execute(new ApplicationArguments(Array.Empty<string>()));
				Assert("Execute should return true", result);
				Assert("No messages should have been displayed", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestNotOkToRun()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(null))
			{
				EnvProxy.Instance.Registry.ExpectedClientDLL = "ZClientEDI";
				bool result = new StartupCheckClientDll().Execute(new ApplicationArguments(Array.Empty<string>()));
				Assert("Execute should return false", !result);
				Assert("Client dll checker error should have been displayed", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(ClientDllChecker.CheckRegistry().ErrorMessage));
				Assert("Client dll checker error should have been displayed", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText(String.Format("Would you like to attempt to repair the {0} installation?", BrandingFactory.Instance.ProductName)));
			}
		}

		public void TestClientCommandOptionShouldFailOnRegisteredSystem()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(null))
			{
				bool result = new StartupCheckClientDll().Execute(new ApplicationArguments(new string[] { "-Client:TIP" }));
				AssertEquals(false, result);
				AssertContains("The system has a valid registration, but the enterprise code does not match the dll.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override int DefaultErrorExitCode => ExitCodes.StartupCheckClientDllError;
	}
}
