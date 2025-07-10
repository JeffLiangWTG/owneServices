using System;
using System.Collections;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class CusGuaranteeRuleLookupsTest : Customs.Business.Testing.CusGuaranteeRuleLookupsTest<CusGuaranteeRuleLookups, CusGuaranteeRule>
	{
		protected virtual Type ExpectedPermitRuleCodesCoreType => typeof(PermitRuleCodeList);

		[ExpectNoExceptions]
		public override void TestPermitRuleCodes()
		{
			NUnit.Framework.Assert.That(!PermitRule.Lookups.PermitRuleCodes.ContainsCode(PermitRuleCodeList.Codes.TSP), NUnit.Framework.Is.True, "when CPH_Type is not 'TST' PermitRuleCodes should not include 'TSP'");
			PermitHeader.CPH_Type = EUGuaranteeTypeList.Codes.TST;
			NUnit.Framework.Assert.That(PermitRule.Lookups.PermitRuleCodes.ContainsCode(PermitRuleCodeList.Codes.TSP), NUnit.Framework.Is.True, "when CPH_Type is 'TST' PermitRuleCodes should include 'TSP'");
		}

		[ExpectNoExceptions]
		public void TestPermitRuleCodesCore()
		{
			var lookup = PermitRule.Lookups;
			var codeList = lookup.GetType()
				.GetProperty("PermitRuleCodesCore", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
				.GetValue(lookup);
			NUnit.Framework.Assert.That(codeList, NUnit.Framework.Is.TypeOf(ExpectedPermitRuleCodesCoreType));
		}

		public override void TestCPR_ValueFromList()
		{
			SetUpCustomsOffices();
			Factory.Save();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(ICollection).IsAssignableFrom(PermitRule.Lookups.CPR_ValueFromList.GetType()), NUnit.Framework.Is.True);

				PermitRule.CPR_RuleCode = PermitRuleCodeList.Codes.INV;
				NUnit.Framework.Assert.That(PermitRule.Lookups.CPR_ValueFromList, NUnit.Framework.Is.TypeOf<RefCountryCollection>());

				PermitRule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
				NUnit.Framework.Assert.That(PermitRule.Lookups.CPR_ValueFromList, NUnit.Framework.Is.TypeOf<OrgAddressCollection>());

				PermitRule.CPR_RuleCode = PermitRuleCodeList.Codes.LAP;
				var actualLiabilityApplicablePercentageList = PermitRule.Lookups.CPR_ValueFromList;
				NUnit.Framework.Assert.That(actualLiabilityApplicablePercentageList, NUnit.Framework.Is.TypeOf<LiabilityApplicablePercentageCodeList>());
				NUnit.Framework.Assert.That(actualLiabilityApplicablePercentageList, NUnit.Framework.Is.SameAs(Factory.GetCachedValue<LiabilityApplicablePercentageCodeList>()));

				PermitRule.CPR_RuleCode = PermitRuleCodeList.Codes.CUS;
				var actualEuAndCtOffices = PermitRule.Lookups.CPR_ValueFromList;
				NUnit.Framework.Assert.That(actualEuAndCtOffices, NUnit.Framework.Is.TypeOf<EUCustomsOfficeCodeCollection>());
				var customsOfficesCollection = (EUCustomsOfficeCodeCollection)actualEuAndCtOffices;
				customsOfficesCollection.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "XXXX2222", "XXXX3333" }, actualEuAndCtOffices.Cast<ZZRefCusCodeListCombined>().Select(x => x.ZZD_Code));
			});
		}

		[ExpectNoExceptions]
		public override void TestCPR_ValueToList()
		{
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(typeof(ICollection).IsAssignableFrom(PermitRule.Lookups.CPR_ValueToList.GetType()), NUnit.Framework.Is.True);

				PermitRule.CPR_RuleCode = PermitRuleCodeList.Codes.INV;
				NUnit.Framework.Assert.That(PermitRule.Lookups.CPR_ValueToList, NUnit.Framework.Is.TypeOf<RefCountryCollection>());

				PermitRule.CPR_RuleCode = "";
				NUnit.Framework.Assert.That(PermitRule.Lookups.CPR_ValueToList.Count, NUnit.Framework.Is.EqualTo(0));
			});
		}

		void SetUpCustomsOffices()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("CUSOF", "Customs Office");
			helper.CreateCusCodeType("ROLE", "ROLE");

			helper.CreateNewOrGetExistingDataGrouping("AU", "Australia");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ROLE", "ROLE", "CUSOF", "AU", "CUSOF", false, false);
			helper.CreateCusCodeListWithAttribute("AU", "CUSOF", "XXXX1111", "Office 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "GUA");

			helper.CreateNewOrGetExistingDataGrouping("DE", "Germany");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ROLE", "ROLE", "CUSOF", "DE", "CUSOF", false, false);
			helper.CreateCusCodeListWithAttribute("DE", "CUSOF", "XXXX2222", "Office 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "GUA");

			helper.CreateNewOrGetExistingDataGrouping("XI", "Northern Ireland Part of Great Britain");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("ROLE", "ROLE", "CUSOF", "XI", "CUSOF", false, false);
			helper.CreateCusCodeListWithAttribute("XI", "CUSOF", "XXXX3333", "Office 3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "GUA");
			helper.CreateCusCodeListWithAttribute("XI", "CUSOF", "XXXX4444", "Office 4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "ROLE", "XXX");
		}
	}
}
