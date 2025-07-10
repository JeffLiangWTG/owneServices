using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class DependencyBetweenModulesRuleTest : TestCaseWithFactory
	{
		public void TestExecuteRule()
		{
			var lic = new LegacyLicence();
			var dependentOn = new string[] { lic.SeaCargoDepot.Name, lic.SeaCargoReport.Name };
			DependencyBetweenModulesRule newRule = new DependencyBetweenModulesRule(lic.HVSO.Name, dependentOn);

			Assert("Module HVSO is not toggled", !TestLicHeader.Modules.FindByCode(lic.HVSO.Name).LM_Calc_IsEnabled);
			Assert("Module SeaCargoDepot is not toggled", !TestLicHeader.Modules.FindByCode(lic.SeaCargoDepot.Name).LM_Calc_IsEnabled);
			Assert("Module SeaCargoReport is not toggled", !TestLicHeader.Modules.FindByCode(lic.SeaCargoReport.Name).LM_Calc_IsEnabled);

			// Test Setting Dependents Does NOT Force Error on Child
			TestLicHeader.Modules.FindByCode(lic.SeaCargoDepot.Name).LM_Calc_IsEnabled = true;
			Assert("Module HVSO Has No Errors", !TestLicHeader.Modules.FindByCode(lic.HVSO.Name).HasRowErrors);

			TestLicHeader.Modules.FindByCode(lic.SeaCargoReport.Name).LM_Calc_IsEnabled = true;
			Assert("Module HVSO Has No Errors", !TestLicHeader.Modules.FindByCode(lic.HVSO.Name).HasRowErrors);

			TestLicHeader.Modules.FindByCode(lic.SeaCargoReport.Name).LM_Calc_IsEnabled = false;
			TestLicHeader.Modules.FindByCode(lic.SeaCargoDepot.Name).LM_Calc_IsEnabled = false;
			Assert("Module HVSO Has No Errors", !TestLicHeader.Modules.FindByCode(lic.HVSO.Name).HasRowErrors);

			// Test Setting Child without dependents forces error
			TestLicHeader.Modules.FindByCode(lic.HVSO.Name).LM_Calc_IsEnabled = true;
			Assert("Module HVSO Has Errors", TestLicHeader.Modules.FindByCode(lic.HVSO.Name).HasRowErrors);

			TestLicHeader.Modules.FindByCode(lic.SeaCargoReport.Name).LM_Calc_IsEnabled = true;
			Assert("Module HVSO Has Errors", TestLicHeader.Modules.FindByCode(lic.HVSO.Name).HasRowErrors);

			TestLicHeader.Modules.FindByCode(lic.SeaCargoDepot.Name).LM_Calc_IsEnabled = true;
			Assert("Module HVSO Has No Errors", !TestLicHeader.Modules.FindByCode(lic.HVSO.Name).HasRowErrors);
		}

		protected override void SetUp()
		{
			base.SetUp();
			EDIOrgHeader testHeader = Factory.New<EDIOrgHeader>();
			testHeader.OH_RL_NKClosestPort = "AUSYD";
			testHeader.CreateAndLoadLicenceForOrg();

			TestCompany = testHeader.LicCompany;
			TestDatabase = TestCompany.LicDatabases.AddNew();
			TestLicHeader = TestCompany.GetHeader(TestDatabase);
			TestDatabase.LD_ServerCode = "SYD";
			TestDatabase.LD_Product = ProductTypes.Codes.Enterprise;
		}

		LicenceCompany TestCompany;
		LicenceDatabase TestDatabase;
		LicenceHeader TestLicHeader;
	}
}
