using System;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Module.Controller.Testing
{
	[TestedType(typeof(PowerBiAnalyticsReportsModule))]
	// [RequiresSoftware(RequiredSoftware.IsVM | RequiredSoftware.PowerBi)]
	public class PowerBiAnalyticsReportsModuleTest : ZPopupModuleBasherTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			if (TestingState.IsRunningOnDAT)
			{
				Name = "SkipTest";
			}
		}

		public void SkipTest()
		{
			Assert(true);
		}

		public void TestLicenseCheckpoint()
		{
			using (var module = (ZPopupModule)ZModule.GetZModule(ModuleID))
			{
				AssertEquals("Module Licence should be BusinessIntelligence", Env.Licence.BusinessIntelligence, module.LicenceCheckPoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AnalyticsReports;
		}

		public void TestShow()
		{
			GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myserver/Portals");
			using (var module = new PowerBiAnalyticsReportsModule())
			{
				module.Show();
			}

			var uri = new Uri(WebUrlLauncher.LastUrlLaunched);
			AssertEquals("#/analytics/logistics", uri.Fragment);
			AssertStartsWith("should construct correct uri", "https://myserver/Portals/BIA/Desktop?sso_otp=", uri.AbsoluteUri);
		}
	}
}
