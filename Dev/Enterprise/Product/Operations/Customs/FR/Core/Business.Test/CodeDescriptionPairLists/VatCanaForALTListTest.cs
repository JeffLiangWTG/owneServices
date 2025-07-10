using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	class VatCanaForALTListTest : TestCaseWithFactory
	{
		public void TestGetAllCodesAndDescriptions()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "VAT National Additional Codes");
			helper.CreateCusCodeListWithAttribute("FR", UniversalReferenceConstants.RefCusCodeListTypes.Codes.VatCana, "1035", "Article 1695 du CGI - Autoliquidation de la TVA à l''importation", new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), UniversalReferenceConstants.RefCusCodeListAttributeNames.Names.VatProcedure, GuaranteeTypeList.Codes.ALT);
			Factory.Save();

			var list = new VatCanaForALTList(Factory);
			AssertContainsExactElementsInAnyOrder(new[] { "1035" }, list.GetAllCodes());
			AssertEquals("Article 1695 du CGI - Autoliquidation de la TVA à l''importation", list.GetDescriptionFromCode("1035"));
		}
	}
}
