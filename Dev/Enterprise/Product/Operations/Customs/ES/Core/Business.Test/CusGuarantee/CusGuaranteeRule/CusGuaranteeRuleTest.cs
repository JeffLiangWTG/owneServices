using Enterprise.Customs.ES.Business.CusGuarantee;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using PermitRuleCodeList = Enterprise.Customs.EU.Business.PermitRuleCodeList;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusGuaranteeRule))]
	public class CusGuaranteeRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLookups()
		{
			AssertType<CusGuaranteeRuleLookups>(Factory.New<CusGuaranteeRule>().Lookups);
		}

		public void TestCPR_ValueTo_ReadOnly()
		{
			var cusGuaranteeRuleTest = Factory.New<CusGuaranteeRule>();
			AssertNotEquals("PRE-CONDITION", PermitRuleCodeList.Codes.CUS, cusGuaranteeRuleTest.CPR_RuleCode);
			AssertEquals("When CPR_RuleCode != CUS", false, cusGuaranteeRuleTest.CPR_ValueToInfo.ReadOnly);
			cusGuaranteeRuleTest.CPR_RuleCode = PermitRuleCodeList.Codes.CUS;
			AssertEquals("When CPR_RuleCode = CUS", true, cusGuaranteeRuleTest.CPR_ValueToInfo.ReadOnly);
		}
	}
}
