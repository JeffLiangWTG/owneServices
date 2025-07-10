using CargoWise.EntityFramework;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRule))]
	public class ComplianceRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCurrentCountry()
		{
			var rule = Factory.New<ComplianceRule>();
			rule.CurrentCountryCode = "C1";
			rule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			AssertNull(rule.CurrentCountry);

			rule.CurrentCountryCode = "AU";
			AssertEquals("AU", rule.CurrentCountry?.RN_Code);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var rule = Factory.New<ComplianceRule>();
			rule.CRU_Origin = "AU";
			rule.CRU_Destination = "US";
			rule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			return rule;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var rule = Factory.New<ComplianceRule>();
			rule.CRU_Origin = "AU";
			rule.CRU_Destination = "US";
			rule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			return rule;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var rule = Factory.New<ComplianceRule>();
			rule.CRU_Origin = "AU";
			rule.CRU_Destination = "US";
			rule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			return rule;
		}
	}
}
