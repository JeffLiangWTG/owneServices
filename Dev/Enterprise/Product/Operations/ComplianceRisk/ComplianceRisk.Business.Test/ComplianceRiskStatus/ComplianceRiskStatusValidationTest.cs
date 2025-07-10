using CargoWise.EntityFramework.Testing;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceRiskStatusValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCOR_LocationRisk_WithInvalidValue_ShowError()
		{
			var riskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			riskStatus.COR_LocationRisk = "";
			AssertHasErrors("Please enter a value.", riskStatus.COR_LocationRiskInfo);
			riskStatus.COR_LocationRisk = "OVR";
			AssertHasErrors("Please Enter a valid selection", riskStatus.COR_LocationRiskInfo);
			riskStatus.COR_LocationRisk = "CLR";
			AssertNoErrors(riskStatus.COR_LocationRiskInfo);
		}

		public void TestCOR_OverallRisk_WithInvalidValue_ShowError()
		{
			var riskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			riskStatus.COR_OverallRisk = "";
			AssertHasErrors("Please enter a value.", riskStatus.COR_OverallRiskInfo);
			riskStatus.COR_OverallRisk = "INV";
			AssertHasErrors("Please Enter a valid selection", riskStatus.COR_OverallRiskInfo);
			riskStatus.COR_OverallRisk = "OVR";
			AssertNoErrors(riskStatus.COR_OverallRiskInfo);
		}

		public void TestCOR_PartyRisk_WithInvalidValue_ShowError()
		{
			var riskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			riskStatus.COR_PartyRisk = "";
			AssertHasErrors("Please enter a value.", riskStatus.COR_PartyRiskInfo);
			riskStatus.COR_PartyRisk = "OVR";
			AssertHasErrors("Please Enter a valid selection", riskStatus.COR_PartyRiskInfo);
			riskStatus.COR_PartyRisk = "CLR";
			AssertNoErrors(riskStatus.COR_PartyRiskInfo);
		}
	}
}
