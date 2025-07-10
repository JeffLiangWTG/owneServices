using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsSupportingDocumentPhase4LookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTypeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListType.Code.SupportingDocumentOfNCTS, "Supporting Document Of NCTS");
			helper.CreateNewOrGetExistingCusCodeType("DC000", "Invalid Type");
			Factory.Save();

			var refCusCodeList1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD01", "SD01 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList2 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "SD02", "SD02 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList5 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV01", "Invalid Country", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList6 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "DC000", "INV02", "Invalid Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList7 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV03", "Invalid StartDate", ZDateTime.Today.AddDays(2), ZDateTime.MaxSmallDateTimeValue);
			var refCusCodeList8 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, RefCusCodeListType.Code.SupportingDocumentOfNCTS, "INV04", "Invalid EndDate", ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddDays(-2));
			Factory.Save();

			var typeCodesList = supportingDocument.Lookups.TypeCodeList;
			var completeFilter = typeCodesList.CompleteFilter;
			CombineAssertions(() =>
			{
				AssertType<ZZRefCusCodeListCombinedCollection>("List Type", typeCodesList);
				AssertSame("Cached", typeCodesList, supportingDocument.Lookups.TypeCodeList);
				AssertEquals("Matched EUN", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList1.PK).MatchesFilter(completeFilter));
				AssertEquals("Matched LV", true, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList2.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched DataGroupingCode", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList5.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched CodeType", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList6.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched StartDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList7.PK).MatchesFilter(completeFilter));
				AssertEquals("Unmatched EndDate", false, Factory.Load<ZZRefCusCodeListCombined>(refCusCodeList8.PK).MatchesFilter(completeFilter));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			supportingDocument = goodsItem.SupportingDocuments.AddNew();
		}
		NctsSupportingDocument supportingDocument;
	}
}
