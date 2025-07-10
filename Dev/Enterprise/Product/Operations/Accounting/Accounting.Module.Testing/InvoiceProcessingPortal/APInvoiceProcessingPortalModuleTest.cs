using System;
using System.Web;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module.Testing.InvoiceProcessingPortal
{
	public class APInvoiceProcessingPortalModuleTest : TestCaseWithFactory
	{
		public void TestModule()
		{
			using (var module = new APInvoiceProcessingPortalModule())
			{
				AssertEquals(ModuleIDs.APInvoiceProcessingPortal, module.ID);
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
				AssertEquals(Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
			}
		}

		public void TestModule_Show()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

			using var module = new APInvoiceProcessingPortalModule();
			module.Show();

			var launchedUrl = WebUrlLauncher.LastUrlLaunched;
			var uri = new Uri(launchedUrl, UriKind.Absolute);
			var queryKeyValuePairs = HttpUtility.ParseQueryString(uri.Query);

			AssertEquals("/goto/payables", uri.AbsolutePath);
			AssertNotNull("A Glow Access Token should be attached", queryKeyValuePairs["sso_otp"]);
		}

		public void TestModule_ShouldReturnErrorWhenGlowRegistryIsEmpty()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);

			using var module = new APInvoiceProcessingPortalModule();
			module.Show();

			var assertMessage = "Should display error when GLOW URL was not configured in registry.";

			AssertEquals
			(
				assertMessage,
				@"The invoice processing portal cannot be opened in a browser as GLOW has not been configured for this client.",
				UnitTestUserNotification.Instance.LastMessage.Text
			);
			AssertNullOrEmpty("No URL was launched", WebUrlLauncher.LastUrlLaunched);
		}

		public void TestModuleSecurityCheckpoint()
		{
			using (var module = new APInvoiceProcessingPortalModule())
			{
				AssertEquals(Env.Security.None, module.SecurityCheckpoint);
			}
		}
	}
}
