using System;
using System.Collections;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing
{
	[TestedType(typeof(CusGuaranteeRuleLookups))]
	sealed class CusGuaranteeRuleLookupsTest : EU.Business.Testing.CusGuaranteeRuleLookupsTest
	{
		protected override Type ExpectedPermitRuleCodesCoreType => typeof(PermitRuleCodeList);

		public override void TestCPR_ValueFromList()
		{
			CombineAssertions(() =>
			{
				Assert(typeof(ICollection).IsAssignableFrom(PermitRule.Lookups.CPR_ValueFromList.GetType()));
				var guaranteeRule = GetNewRule(Factory);
				var listOfCountry = new RefCountryCollection(Factory).ToString();
				var customsOffice = EUCustomsOfficeCodeCollection.AllEuropeanUnionCustomsOfficesWithRequiredRoles(Factory, "CGU", "GUA").ToString();

				guaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.CUS;
				var valueFromList = guaranteeRule.Lookups.CPR_ValueFromList;
				var list = valueFromList.ToString();
				AssertNotEquals("Not equals", listOfCountry, list);
				AssertEquals("Equals", customsOffice, list);
			});
		}
	}
}
