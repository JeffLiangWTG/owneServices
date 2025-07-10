using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPreviousDocumentPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList_DataGroup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var previoucDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			helper.CreateNewOrGetExistingCusCodeType(previoucDocumentType, "Previous Documents Transit NCTS (BOX40)");
			Create_CusCodeList_RefData(helper, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previoucDocumentType, "T1");
			Create_CusCodeList_RefData(helper, Core.Constants.CountryCodes.Latvia, previoucDocumentType, "AA");
			Create_CusCodeList_RefData(helper, Core.Constants.CountryCodes.Latvia, previoucDocumentType, "BB");
			Factory.Save();

			var codeListLookup = (CodeDescriptionPairList)lookups.CodeList;
			AssertContainsExactElementsInAnyOrder("Only gets Country's previous document types", new string[] { "AA", "BB" }, codeListLookup.GetAllCodes());
		}

		public void TestCodeList_Fallback()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var previoucDocumentType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfNCTS;
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "EUN"));
			helper.CreateNewOrGetExistingCusCodeType(previoucDocumentType, "Previous Documents Transit NCTS (BOX40)");
			Create_CusCodeList_RefData(helper, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previoucDocumentType, "T1");
			Create_CusCodeList_RefData(helper, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previoucDocumentType, "T2");
			Factory.Save();

			var codeListLookup = (CodeDescriptionPairList)lookups.CodeList;
			AssertContainsExactElementsInAnyOrder("FallBack to EU previous document types", new string[] { "T1", "T2" }, codeListLookup.GetAllCodes());
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
			lookups = new NctsPreviousDocumentPhase4Lookups(previousDocument);
		}
		NctsPreviousDocument previousDocument;
		NctsPreviousDocumentLookups lookups;

		void Create_CusCodeList_RefData(UniversalReferenceTestDataHelper helper, string countryCode, string refDataType, string code)
		{
			var dateMIN = ZDateTime.Today.AddMonths(-1);
			var dateMAX = ZDateTime.Today.AddMonths(3);

			helper.CreateCusCodeList(countryCode, refDataType, code, dateMIN, dateMAX);
		}
	}
}
