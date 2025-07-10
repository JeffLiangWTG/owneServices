using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackedItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMessageStatusList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.CreatePackedItemForTesting();
			AssertEquals("MessageStatusList", true, object.ReferenceEquals(Factory.GetCachedValue<MessageStatusCodeList>(), packedItem.Lookups.MessageStatusList));
		}

		public void TestCustomsStatusList()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "CustomsManifestStatus");
			var sg8 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "8", "Proceed to Border", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var sg9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var au9 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsManifestStatus, "9", "Already on Customs system", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			header.FillWithValidTestData();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			var list = packedItem.Lookups.CustomsStatusList;
			AssertEquals("Proceed to Border", list.GetDescriptionFromCode("8"));
			AssertEquals("Already on Customs system", list.GetDescriptionFromCode("9"));
			AssertEquals(false, list.ContainsCode("10"));
		}

		public void TestCustomsUQList()
		{
			var helper = new ZZDataTestHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "BG", "BAG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "KG", "Keg", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.PackageTypes, "PK", "Packet", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
			header.FillWithValidTestData();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItemForTesting();
			var list = packedItem.Lookups.CustomsUQList;
			AssertEquals(typeof(UntranslatableCodeDescriptionPairList), list.GetType());
			AssertEquals(2, list.Count);
			AssertEquals("BAG", list.GetDescriptionFromCode("BG"));
			AssertEquals("Keg", list.GetDescriptionFromCode("KG"));
		}

		public void TestLookupsItems()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItems.AddNewPackedItem();
			var lookups = packedItem.Lookups;
			AssertNotNull(lookups.TariffList);
			AssertEquals(lookups.TariffList.GetType(), typeof(TariffViewCollection));

			AssertNotNull(lookups.Currencies);
			AssertEquals(lookups.Currencies.GetType(), typeof(RefCurrencyCollection));

			AssertNotNull(lookups.Countries);
			AssertEquals(lookups.Countries.GetType(), typeof(RefCountryCollection));

			AssertNotNull(lookups.GrossWeightUQList);
			AssertNotNull(lookups.NetWeightUQList);
		}
	}
}
