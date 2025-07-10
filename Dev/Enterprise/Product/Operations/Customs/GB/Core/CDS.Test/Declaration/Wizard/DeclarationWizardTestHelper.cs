using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public static class DeclarationWizardTestHelper
	{
		public static void SetUpRefCusCodeListData(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, "Customs Declaration Service", eun);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "Entry Sub Styles");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "A", "Standard customs declaration (Goods arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "B", "Simplified declaration on an occasional basis (Goods arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "C", "Simplified declaration with regular use (pre-authorised) (Goods arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "D", "Standard customs declaration (Goods not arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "E", "Simplified declaration on an occasional basis (Goods not arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "F", "Simplified declaration with regular use (pre-authorised) (Goods not arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "J", "C21 (Goods arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "K", "C21 (Goods not arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "X", "Supplementary declaration covered by types B and E (Goods arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "Y", "Supplementary declaration covered by types C and F (Goods arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub, "Z", "Supplementary declarations for Entry in Declarants Records (Goods arrived)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			factory.Save();
		}
	}
}
