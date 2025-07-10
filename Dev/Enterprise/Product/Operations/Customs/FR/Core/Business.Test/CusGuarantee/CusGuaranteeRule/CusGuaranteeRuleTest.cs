using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusGuarantee.Testing
{
	[TestedType(typeof(CusGuaranteeRule))]
	public class CusGuaranteeRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaultValueForRuleCodeMod()
		{
			var guarantee = Factory.New<CusGuaranteeHeader>();
			var rule = guarantee.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.ADD;
			AssertNotEquals(GuaranteeModeCodeList.Codes.Guarantee, rule.CPR_ValueFrom);

			rule.CPR_RuleCode = PermitRuleCodeList.Codes.MOD;
			AssertEquals(GuaranteeModeCodeList.Codes.Guarantee, rule.CPR_ValueFrom);
		}

		public void TestDefaultValueForRuleCodeCAN()
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

			var guarantee = Factory.New<CusGuaranteeHeader>();

			guarantee.CPH_Type = GuaranteeTypeList.Codes.AI2;
			var rule = guarantee.CusGuaranteeRules.AddNew();
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			AssertEquals(ZString.Empty, rule.CPR_ValueFrom);

			guarantee.CPH_Type = GuaranteeTypeList.Codes.ALT;
			rule.CPR_RuleCode = PermitRuleCodeList.Codes.CAN;
			AssertEquals("1035", rule.CPR_ValueFrom);
		}
	}
}
