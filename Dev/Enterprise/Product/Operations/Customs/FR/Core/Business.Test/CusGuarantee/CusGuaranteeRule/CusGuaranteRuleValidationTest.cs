using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	class CusGuaranteRuleValidationTest : SharedCusPermitRuleValidationTest<CusGuaranteeRuleValidation, CusGuaranteeRule>
	{
		#region Implementation

		protected override CusGuaranteeRule GetNewRule(BusinessObjectFactory factory)
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			var guaranteeRule = (CusGuaranteeRule)guaranteeHeader.CusGuaranteeRules.AddNew();
			guaranteeRule.FillWithValidTestData();
			return guaranteeRule;
		}
		#endregion

		public void TestCheckCPR_ValueFrom()
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

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = org.PK;

			PermitRule.GuaranteeHeader.CPH_OH_PermitHolder = org.PK;

			PermitRule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(PermitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);
			PermitRule.CPR_ValueFrom = "1";
			AssertNoErrorContaining(PermitRule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			var rule = PermitRule.GuaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = "ADD";
			rule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			rule.CPR_ValueFrom = "1";
			AssertHasMessageErrorContaining(rule.CPR_ValueFromInfo, "The selected address short code does not exist on the organization");

			rule.CPR_ValueFrom = org.MainAddress.OA_Code;
			AssertNoMessageErrorContaining(rule.CPR_ValueFromInfo, "The selected address short code does not exist on the organization");

			rule.CPR_RuleCode = "MOD";
			rule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			rule.CPR_ValueFrom = "T";
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_ValueFrom = "C";
			AssertNoErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_ValueFrom = "M";
			AssertNoErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			PermitRule.GuaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.AI2;
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			rule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			rule.CPR_ValueFrom = "1035";
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_ValueFrom = "1002";
			AssertNoErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_ValueFrom = "1012";
			AssertNoErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			PermitRule.GuaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.ALT;
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			rule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			rule.CPR_ValueFrom = "1002";
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_ValueFrom = "1035";
			AssertNoErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ENT;
			rule.CPR_ValueFrom = ZString.Empty;
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, MandatoryValidation.MustBeEntered);

			rule.CPR_ValueFrom = "1002";
			AssertHasErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_ValueFrom = GuaranteeEntryTypeList.Codes.BTH;
			AssertNoErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_ValueFrom = GuaranteeEntryTypeList.Codes.EXP;
			AssertNoErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);

			rule.CPR_ValueFrom = GuaranteeEntryTypeList.Codes.IMP;
			AssertNoErrorContaining(rule.CPR_ValueFromInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckCPR_RuleCode()
		{
			var message = "Error : CANA for re-export Exemption (AI2) can be entered only one time";
			PermitRule.GuaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.ALT;
			var rule = PermitRule.GuaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			AssertNoErrorContaining(rule.CPR_RuleCodeInfo, message);

			var rule2 = PermitRule.GuaranteeHeader.CusGuaranteeRules.AddNew();
			rule2.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			AssertHasErrorContaining(rule2.CPR_RuleCodeInfo, message);
		}
	}
}
