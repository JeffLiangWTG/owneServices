using System;
using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	[TestedType(typeof(CusGuaranteeRuleLookups))]
	sealed class CusGuaranteeRuleLookupsTest : EU.Business.Testing.CusGuaranteeRuleLookupsTest
	{
		protected override Type ExpectedPermitRuleCodesCoreType => typeof(PermitRuleCodeList);

		public new void TestCPR_ValueFromList()
		{
			Assert(typeof(ICollection).IsAssignableFrom(PermitRule.Lookups.CPR_ValueFromList.GetType()));
			var guaranteeRule = GetNewRule(Factory);
			var guaranteeModeCodeList = new GuaranteeModeCodeList();
			guaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.MOD;
			var list = guaranteeRule.Lookups.CPR_ValueFromList;
			AssertContainsExactElementsInAnyOrder(guaranteeModeCodeList, list);

			guaranteeRule = GetNewRule(Factory);
			var listOfCountry = new RefCountryCollection(Factory);
			guaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.INV;
			list = guaranteeRule.Lookups.CPR_ValueFromList;
			AssertContainsExactElementsInAnyOrder(listOfCountry, list);

			guaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			list = guaranteeRule.Lookups.CPR_ValueFromList;
			AssertContainsExactElementsInAnyOrder(new ZArchitecture.Core.CodeDescriptionPairList(), list);

			guaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.ENT;
			list = guaranteeRule.Lookups.CPR_ValueFromList;
			AssertContainsExactElementsInAnyOrder(new GuaranteeEntryTypeList(), list);
		}

		public void TestValuefromForCANCPR_RuleCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "VAT National Additional Codes");
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1035", "Article 1695 du CGI - Autoliquidation de la TVA à l''importation", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.ALT);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1001", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1002", "Je m''engage à respecter les conditions de l''article 275 du CGI - taxes fiscales seules", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1003", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1011", "Article 275 du CGI sans dispense de visa - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1012", "Article 275 du CGI sans dispense de visa - taxes fiscales seules", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1013", "Article 275 du CGI sans dispense de visa - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			Factory.Save();

			var guaranteeRule = GetNewRule(Factory);
			var header = guaranteeRule.GuaranteeHeader;
			var vATguaranteeModeCodeList = new VatCanaForALTList(Factory);
			header.CPH_Type = GuaranteeTypeList.Codes.ALT;
			guaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			var list = guaranteeRule.Lookups.CPR_ValueFromList;
			AssertContainsExactElementsInAnyOrder(vATguaranteeModeCodeList, list);

			var aI2guaranteeModeCodeList = new VatCanaForAI2List(Factory);
			header.CPH_Type = GuaranteeTypeList.Codes.AI2;
			guaranteeRule.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			var list2 = guaranteeRule.Lookups.CPR_ValueFromList;
			AssertContainsExactElementsInAnyOrder(aI2guaranteeModeCodeList, list2);
		}

		new CusGuaranteeRule GetNewRule(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var guaranteeRule = guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.FillWithValidTestData();
			return (CusGuaranteeRule)guaranteeRule;
		}
	}
}
