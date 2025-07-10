using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(CusGuaranteeRule))]
	sealed class CusGuaranteeRuleTest : EnterpriseBusinessObjectTestCase
	{
		[ExpectNoExceptions]
		public void TestCPR_ValueTo_ReadOnly() => CombineAssertions(() =>
		{
			var cusGuaranteeRuleTest = Factory.New<CusGuaranteeRule>();
			NUnit.Framework.Assert.That(cusGuaranteeRuleTest.CPR_RuleCode, NUnit.Framework.Is.Not.EqualTo(PermitRuleCodeList.Codes.LAP).Using(CustomComparers.TypeComparison), "PRE-CONDITION CPR_RuleCode is not 'LAP'");
			NUnit.Framework.Assert.That(cusGuaranteeRuleTest.CPR_RuleCode, NUnit.Framework.Is.Not.EqualTo(PermitRuleCodeList.Codes.TSP).Using(CustomComparers.TypeComparison), "PRE-CONDITION CPR_RuleCode is not 'TSP'");
			NUnit.Framework.Assert.That(cusGuaranteeRuleTest.CPR_RuleCode, NUnit.Framework.Is.Not.EqualTo(PermitRuleCodeList.Codes.CUS).Using(CustomComparers.TypeComparison), "PRE-CONDITION CPR_RuleCode is not 'CUS'");
			NUnit.Framework.Assert.That(cusGuaranteeRuleTest.CPR_RuleCode, NUnit.Framework.Is.Not.EqualTo(PermitRuleCodeList.Codes.PCP).Using(CustomComparers.TypeComparison), "PRE-CONDITION CPR_RuleCode is not 'PCP'");
			NUnit.Framework.Assert.That(cusGuaranteeRuleTest.CPR_RuleCode, NUnit.Framework.Is.Not.EqualTo(PermitRuleCodeList.Codes.PCV).Using(CustomComparers.TypeComparison), "PRE-CONDITION CPR_RuleCode is not 'PCV'");
			NUnit.Framework.Assert.That(cusGuaranteeRuleTest.CPR_RuleCode, NUnit.Framework.Is.Not.EqualTo(PermitRuleCodeList.Codes.PCD).Using(CustomComparers.TypeComparison), "PRE-CONDITION CPR_RuleCode is not 'PCD'");
			AssertValueTo_ReadOnly_When(PermitRuleCodeList.Codes.LAP);
			AssertValueTo_ReadOnly_When(PermitRuleCodeList.Codes.TSP);
			AssertValueTo_ReadOnly_When(PermitRuleCodeList.Codes.CUS);
			AssertValueTo_ReadOnly_When(PermitRuleCodeList.Codes.PCP);
			AssertValueTo_ReadOnly_When(PermitRuleCodeList.Codes.PCV);
			AssertValueTo_ReadOnly_When(PermitRuleCodeList.Codes.PCD);
			void AssertValueTo_ReadOnly_When(string cpr_RuleCode)
			{
				var previousRuleCode = cusGuaranteeRuleTest.CPR_RuleCode;
				NUnit.Framework.Assert.That(cusGuaranteeRuleTest.CPR_ValueToInfo.ReadOnly, NUnit.Framework.Is.EqualTo(false), $"When CPR_RuleCode is not '{cpr_RuleCode}'");
				cusGuaranteeRuleTest.CPR_RuleCode = cpr_RuleCode;
				NUnit.Framework.Assert.That(cusGuaranteeRuleTest.CPR_ValueToInfo.ReadOnly, NUnit.Framework.Is.EqualTo(true), $"When CPR_RuleCode is '{cpr_RuleCode}'");
				cusGuaranteeRuleTest.CPR_RuleCode = previousRuleCode;
			}
		});
	}
}
