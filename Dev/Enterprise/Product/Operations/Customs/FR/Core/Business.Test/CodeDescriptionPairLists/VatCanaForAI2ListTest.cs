using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	class VatCanaForAI2ListTest : TestCaseWithFactory
	{
		public void TestGetAllCodesAndDescriptions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "VAT National Additional Codes");
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1001", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1002", "Je m''engage à respecter les conditions de l''article 275 du CGI - taxes fiscales seules", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1003", "Je m''engage à respecter les conditions de l''article 275 du CGI - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1011", "Article 275 du CGI sans dispense de visa - TVA seule", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1012", "Article 275 du CGI sans dispense de visa - taxes fiscales seules", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1013", "Article 275 du CGI sans dispense de visa - TVA et taxes fiscales", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.AI2);
			Factory.Save();

			var list = new VatCanaForAI2List(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { "1001", "1002", "1003", "1011", "1012", "1013" }, list.GetAllCodes());
			AssertEquals("Je m''engage à respecter les conditions de l''article 275 du CGI - TVA seule", list.GetDescriptionFromCode("1001"));
			AssertEquals("Je m''engage à respecter les conditions de l''article 275 du CGI - taxes fiscales seules", list.GetDescriptionFromCode("1002"));
			AssertEquals("Je m''engage à respecter les conditions de l''article 275 du CGI - TVA et taxes fiscales", list.GetDescriptionFromCode("1003"));
			AssertEquals("Article 275 du CGI sans dispense de visa - TVA seule", list.GetDescriptionFromCode("1011"));
			AssertEquals("Article 275 du CGI sans dispense de visa - taxes fiscales seules", list.GetDescriptionFromCode("1012"));
			AssertEquals("Article 275 du CGI sans dispense de visa - TVA et taxes fiscales", list.GetDescriptionFromCode("1013"));
		}
	}
}
