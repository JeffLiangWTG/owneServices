using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class ComplianceRuleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOriginDestinationAllEmpty()
		{
			var error = "At least one of Origin or Destination country/region should be entered.";
			var rule = CreateComplianceRule(string.Empty, string.Empty, ComplianceRiskStatusCodeList.Codes.Released);
			AssertHasError(rule.CRU_DestinationInfo, error);
			AssertHasError(rule.CRU_OriginInfo, error);
		}

		public void TestOriginDestinationWithCurrentCountryCode()
		{
			var error = "At least one of Origin or Destination country/region should be 'AU'.";
			var rule = CreateComplianceRule("CN", "US", ComplianceRiskStatusCodeList.Codes.Released);
			AssertHasError(rule.CRU_DestinationInfo, error);
			AssertHasError(rule.CRU_OriginInfo, error);

			rule.CRU_Origin = "AU";
			AssertNoErrors(rule.CRU_DestinationInfo);
			AssertNoErrors(rule.CRU_OriginInfo);
		}

		public void TestOriginDestinationSame()
		{
			var error = "Origin and Destination cannot be the same.";
			var rule = CreateComplianceRule("AU", "AU", ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertHasError(rule.CRU_DestinationInfo, error);
			AssertHasError(rule.CRU_OriginInfo, error);
		}

		public void TestOriginDestinationInvalidCountryCode()
		{
			var error = "Enter a valid selection.";
			var rule = CreateComplianceRule("12", "32", ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertHasError(rule.CRU_DestinationInfo, error);
			AssertHasError(rule.CRU_OriginInfo, error);
		}

		public void TestInvalidCurrentCountryCode()
		{
			var error = "Can't find the Country/Region 'C1', please save the form and reload to add the Compliance Rules.";
			var rule = CreateComplianceRule(string.Empty, string.Empty, ComplianceRiskStatusCodeList.Codes.Released, "C1");
			AssertHasError(rule.CRU_DestinationInfo, error);
			AssertHasError(rule.CRU_OriginInfo, error);
		}

		public void TestValidateHarmonizedCode()
		{
			var error = "Invalid code entered. A valid Harmonized Code can only contain 2 to 6 digits and cannot start with 00.";
			var rule = CreateComplianceRule(string.Empty, string.Empty, ComplianceRiskStatusCodeList.Codes.Released);

			for (var i = 2; i <= 6; i++)
			{
				rule.CRU_HarmonizedCode = "1".PadRight(i, '1');
				AssertNoErrors(rule.CRU_HarmonizedCodeInfo);
			}

			for (var i = 2; i <= 6; i++)
			{
				rule.CRU_HarmonizedCode = "00".PadRight(i, '1');
				AssertHasError(rule.CRU_HarmonizedCodeInfo, error);
			}

			for (var i = 2; i <= 6; i++)
			{
				rule.CRU_HarmonizedCode = "A~".PadRight(i, '1');
				AssertHasError(rule.CRU_HarmonizedCodeInfo, error);
			}

			rule.CRU_HarmonizedCode = "1";
			AssertHasError(rule.CRU_HarmonizedCodeInfo, error);

			rule.CRU_HarmonizedCode = "1".PadRight(7, '1');
			AssertHasError(rule.CRU_HarmonizedCodeInfo, error);

			// non-ascii digits
			rule.CRU_HarmonizedCode = "߁١";
			AssertHasError(rule.CRU_HarmonizedCodeInfo, error);

			rule.CRU_HarmonizedCode = string.Empty;
			AssertNoErrors(rule.CRU_HarmonizedCodeInfo);
		}

		public void TestDuplicateRecords()
		{
			var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var duplicatedOriginError = "Duplicate Origin (AU) cannot be added.";
			var duplicatedDestinationError = "Duplicate Origin (AU) + Destination (US) cannot be added.";
			var duplicatedHarmonizedCodeError = "Duplicate Origin (AU) + Destination (US) + Harmonized Code (123456) cannot be added.";
			var rule1 = (country.ComplianceRules as ComplianceRuleCollection).AddNew();
			rule1.CRU_Origin = "AU";

			var rule2 = (country.ComplianceRules as ComplianceRuleCollection).AddNew();
			rule2.CRU_Origin = "AU";
			AssertHasRowError(rule2, duplicatedOriginError);

			rule1.CRU_Destination = "US";
			rule2.CRU_Destination = "US";
			AssertHasRowError(rule2, duplicatedDestinationError);

			rule1.CRU_HarmonizedCode = "123456";
			rule2.CRU_HarmonizedCode = "123456";
			AssertHasRowError(rule2, duplicatedHarmonizedCodeError);

			rule2.CRU_HarmonizedCode = "654321";
			AssertNoRowError(rule2, duplicatedHarmonizedCodeError);
		}

		public void TestRiskStatus()
		{
			var rule = CreateComplianceRule("AU", "US", string.Empty);
			AssertHasErrors("Please enter a value.", rule.CRU_RiskStatusInfo);

			rule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			AssertNoErrors(rule.CRU_RiskStatusInfo);

			rule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			AssertHasErrors("Enter a valid selection.", rule.CRU_RiskStatusInfo);

			rule.CRU_RiskStatus = "ABC";
			AssertHasErrors("Enter a valid selection.", rule.CRU_RiskStatusInfo);
		}

		ComplianceRule CreateComplianceRule(string origin, string destination, string riskStatus, string currentCountryCode = "AU")
		{
			var rule = Factory.New<ComplianceRule>();
			rule.CurrentCountryCode = currentCountryCode;
			rule.CRU_Origin = origin;
			rule.CRU_Destination = destination;
			rule.CRU_RiskStatus = riskStatus;

			return rule;
		}
	}
}
