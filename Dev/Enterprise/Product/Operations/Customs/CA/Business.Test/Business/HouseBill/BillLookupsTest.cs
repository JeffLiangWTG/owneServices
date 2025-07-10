using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class BillLookupsTest : Customs.Business.Testing.CusDecHouseBillLookupsTest
	{
		public void TestHouseBill()
		{
			Bill parent = Factory.New<Bill>();
			AssertEquals(parent.Lookups.HouseBill, parent);
		}

		public new void TestNoOfPacksPackType_List()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AAA", "AAAAA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BG", "BG DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"BBB", "BBB DESC", new ZDateTime(1994, 3, 3), ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			Assert("AAA", bill.Lookups.NoOfPacksPackType_List.ContainsCode("AAA"));
			Assert("BAG should not appear as it is not in the list", !bill.Lookups.NoOfPacksPackType_List.ContainsCode("BAG"));
			Assert("BBB should not appear as it is too new", !bill.Lookups.NoOfPacksPackType_List.ContainsCode("BBB"));
		}
	}
}
