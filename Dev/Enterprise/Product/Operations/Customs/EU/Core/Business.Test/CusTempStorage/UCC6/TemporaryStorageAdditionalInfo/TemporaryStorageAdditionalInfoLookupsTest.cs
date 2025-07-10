using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public class TemporaryStorageAdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeType("AI44T", "AI44T", Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeType("AI44R", "AI44R", Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeType("AR44T", "AR44T", Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeType("TD44T", "TD44T", Core.Constants.CountryCodes.Latvia);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "AI44T", "code1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "AI44R", "code2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "AR44T", "code3", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, "TD44T", "code4", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var addInfo = packedItem.AdditionalInfos.AddNew();
			AssertEquals(0, addInfo.Lookups.CodeList.Count);

			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			var codeList = addInfo.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
			codeList.Load();
			AssertEquals(1, codeList.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "code1" }, codeList.Select(x => x.ZZD_Code));

			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			codeList = addInfo.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
			codeList.Load();
			AssertEquals(1, codeList.Count);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "code3" }, codeList.Select(x => x.ZZD_Code));

			addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			codeList = addInfo.Lookups.CodeList as ZZRefCusCodeListCombinedCollection;
			codeList.Load();
			AssertCollectionContains("code4", codeList.Select(x => x.ZZD_Code));

			addInfo.CSI_SubType = "BLA";
			AssertEquals(0, addInfo.Lookups.CodeList.Count);
		}

		public void TestSubTypeList()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			var addInfo = packedItem.AdditionalInfos.AddNew();
			AssertEquals("INF, REF, TRA", addInfo.Lookups.SubTypeList.CodesAsString);
		}
	}
}
