using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusAuthorisationRuleLookups))]
	class CusAuthorisationRuleLookupsTest : Customs.Business.Testing.CusAuthorisationRuleLookupsAbstractTest<CusAuthorisationRuleLookups>
	{
		[ExpectNoExceptions]
		public void TestRuleCodeListDefault()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			NUnit.Framework.Assert.That(lookups.RuleCodeList.CodesAsString, Is.EqualTo("LOC"));
		}

		[ExpectNoExceptions]
		public void TestRuleCodeListForSimplifiedDeclaration()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			NUnit.Framework.Assert.That(lookups.RuleCodeList.CodesAsString, Is.EqualTo("LOC, USE"));
		}

		[ExpectNoExceptions]
		public void TestRuleCodeListForEntryOfDataInTheDeclarantsRecords()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			lookups.Parent.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			NUnit.Framework.Assert.That(lookups.RuleCodeList.CodesAsString, Is.EqualTo("BRE, LOC, USE"));
		}

		[ExpectNoExceptions]
		public void TestRuleValueListForRuleCodeUSE_UnknownType()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			lookups.Parent.CPR_RuleCode = "AAA";
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.ValueList).CodesAsString, Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestRuleValueListForAuthorisationTypeEIRAndRuleCodeUSE()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			var cusAuthorisationRule = lookups.Parent;
			cusAuthorisationRule.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.ValueList).CodesAsString, Is.EqualTo("AEX, CWP, CW1, IMP, IPO"));
		}

		[ExpectNoExceptions]
		public void TestRuleValueListForAuthorisationTypeSDEAndRuleCodeUSE()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			var cusAuthorisationRule = lookups.Parent;
			cusAuthorisationRule.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.ValueList).CodesAsString, Is.EqualTo("AEX, CWP, CW1, IMP, IPO, OPO"));
		}

		[ExpectNoExceptions]
		public void TestRuleCodesListForAuthorisationTypeEIRAndUseValueIMP()
		{
			AssertRuleCodesListForAuthorisationTypeEIRAndUseValue(CusAuthorisationUsageRuleList.Codes.FreeCirculation, "BRE, LOC, MRE, REL, USE", "BRE, LOC, USE");
		}

		[ExpectNoExceptions]
		public void TestRuleCodesListForAuthorisationTypeSDEAndUseValueIMP()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			var cusAuthorisationRule = lookups.Parent;
			NUnit.Framework.Assert.Multiple(() =>
			{
				cusAuthorisationRule.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
				cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				cusAuthorisationRule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.FreeCirculation;
				NUnit.Framework.Assert.That(lookups.RuleCodeList.CodesAsString, Is.EqualTo("LOC, MRE, USE"));

				cusAuthorisationRule.CPR_ValueFrom = CusAuthorisationUsageRuleList.Codes.AccreditedExporter;
				NUnit.Framework.Assert.That(lookups.RuleCodeList.CodesAsString, Is.EqualTo("LOC, USE"));
			});
		}

		[ExpectNoExceptions]
		public void TestRuleCodesListForAuthorisationTypeEIRAndUseValueIPO()
		{
			AssertRuleCodesListForAuthorisationTypeEIRAndUseValue(CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure, "BRE, LOC, REL, USE", "BRE, LOC, USE");
		}

		[ExpectNoExceptions]
		public void TestRuleCodesListForAuthorisationTypeEIRAndUseValueCWP()
		{
			AssertRuleCodesListForAuthorisationTypeEIRAndUseValue(CusAuthorisationUsageRuleList.Codes.CustomsWarehousing, "BRE, LOC, REL, USE", "BRE, LOC, USE");
		}

		[ExpectNoExceptions]
		public void TestRuleCodesListForAuthorisationTypeEIRAndUseValueCW1()
		{
			AssertRuleCodesListForAuthorisationTypeEIRAndUseValue(CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1, "BRE, LOC, REL, USE", "BRE, LOC, USE");
		}

		[ExpectNoExceptions]
		public void TestValueListForRuleCodeREL()
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			lookups.Parent.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Release;
			NUnit.Framework.Assert.That(((CodeDescriptionPairList)lookups.ValueList).CodesAsString, Is.EqualTo("1, 2"), "List");
		}

		protected override CusAuthorisationRuleLookups CusAuthorisationRuleLookupsForTesting()
		{
			var cusAuthorisationRule = Factory.NewWithValidTestData<CusAuthorisationRule>();
			cusAuthorisationRule.AuthorisationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			cusAuthorisationRule.AuthorisationHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			return new CusAuthorisationRuleLookups(cusAuthorisationRule);
		}

		[ExpectNoExceptions]
		void AssertRuleCodesListForAuthorisationTypeEIRAndUseValue(string useValue, string expectedCodes, string expectedCodesOtherwise)
		{
			var lookups = CusAuthorisationRuleLookupsForTesting();
			var cusAuthorisationRule = lookups.Parent;
			NUnit.Framework.Assert.Multiple(() =>
			{
				cusAuthorisationRule.CPR_RuleCode = CusAuthorisationRuleTypeList.Codes.Usage;
				cusAuthorisationRule.CPR_ValueFrom = useValue;
				NUnit.Framework.Assert.That(lookups.RuleCodeList.CodesAsString, Is.EqualTo(expectedCodes));

				cusAuthorisationRule.CPR_ValueFrom = "XYZ";
				NUnit.Framework.Assert.That(lookups.RuleCodeList.CodesAsString,	Is.EqualTo(expectedCodesOtherwise));
			});
		}
	}
}
