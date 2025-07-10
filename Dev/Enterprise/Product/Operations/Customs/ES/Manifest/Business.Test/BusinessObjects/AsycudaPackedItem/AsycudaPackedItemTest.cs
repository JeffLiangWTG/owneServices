using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDescriptionSetFromTariff()
		{
			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var esCountryCode = Core.Constants.CountryCodes.Spain;
			var groupingCode = "EUN";
			var grouping = helper.CreateNewOrGetExistingDataGrouping(groupingCode);
			helper.CreateNewOrGetExistingDataGrouping(esCountryCode, parent: grouping);
			var exportTariffType = helper.CreateNewOrGetExistingTariffType(groupingCode, "EXP");
			exportTariffType.ZZI_ZZ9_NKNomenclatureGroupType = "CN";
			Factory.Save();
			helper.CreateTariff(groupingCode, exportTariffType.PK, "01051111", startDate, endDate, description: "Laying stocks", compositeKey: "01.01..05.1.1.10");
			helper.CreateTariff(groupingCode, exportTariffType.PK, "99999999", startDate, endDate, description: new string('0', 2000), compositeKey: "");
			helper.CreateNomenclatureGroup(groupingCode, "01", startDate, endDate, "LIVE ANIMALS", compositeKey: "01.01", nomenclatureGroupType: "CN");
			helper.CreateNomenclatureGroup(groupingCode, "0105", startDate, endDate, "Live poultry, that is to say, fowls of the species Gallus domesticus, ducks, geese, turkeys and guinea fowls", compositeKey: "01.01..05", nomenclatureGroupType: "CN");
			helper.CreateNomenclatureGroup(groupingCode, "010511", startDate, endDate, "Fowls of the species Gallus domesticus", compositeKey: "01.01..05.1.1", nomenclatureGroupType: "CN");
			Factory.Save();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
			header.AMA_ManifestType = ESManifestTypes.Codes.ICS;
			header.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var item = pack.PackedItems.AddNewPackedItem();
			CombineAssertions(() =>
			{
				AssertNullOrEmpty("API_GoodsDescription should be empty", item.API_GoodsDescription);
				item.API_Tariff = "01051111";
				AssertEquals("API_GoodsDescription should be the decription for the 8 characters tariff", "LIVE ANIMALS LIVE POULTRY, THAT IS TO SAY, FOWLS OF THE SPECIES GALLUS DOMESTICUS, DUCKS, GEESE, TURKEYS AND GUINEA FOWLS FOWLS OF THE SPECIES GALLUS DOMESTICUS LAYING STOCKS", item.API_GoodsDescription);
				item.API_Tariff = "";
				AssertEquals("JI_Description should not be changed of it was not empty", "LIVE ANIMALS LIVE POULTRY, THAT IS TO SAY, FOWLS OF THE SPECIES GALLUS DOMESTICUS, DUCKS, GEESE, TURKEYS AND GUINEA FOWLS FOWLS OF THE SPECIES GALLUS DOMESTICUS LAYING STOCKS", item.API_GoodsDescription);
				item.API_GoodsDescription = "";
				item.API_Tariff = "010511";
				AssertEquals("API_GoodsDescription should be the decription for the 6 characters tariff", "LIVE ANIMALS LIVE POULTRY, THAT IS TO SAY, FOWLS OF THE SPECIES GALLUS DOMESTICUS, DUCKS, GEESE, TURKEYS AND GUINEA FOWLS FOWLS OF THE SPECIES GALLUS DOMESTICUS", item.API_GoodsDescription);
				item.API_GoodsDescription = "";
				item.API_Tariff = "0105";
				AssertEquals("API_GoodsDescription should be the decription for the 4 characters tariff", "LIVE ANIMALS LIVE POULTRY, THAT IS TO SAY, FOWLS OF THE SPECIES GALLUS DOMESTICUS, DUCKS, GEESE, TURKEYS AND GUINEA FOWLS", item.API_GoodsDescription);
				item.API_GoodsDescription = "";
				item.API_Tariff = "99999999";
				AssertEquals("JI_Description has been truncated", item.API_GoodsDescriptionInfo.MaxLength, item.API_GoodsDescription.Length);
			});
		}

		public void TestHeader()
		{
			var item = (AsycudaPackedItem)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(item.Header);
		}

		public void TestPack()
		{
			var item = (AsycudaPackedItem)GetNewBusinessObject();
			AssertType<AsycudaPack>(item.Pack);
		}

		public void TestValidation()
		{
			var item = (AsycudaPackedItem)GetNewBusinessObject();
			AssertType<AsycudaPackedItemValidation>(item.Validation);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItems.AddNewPackedItem();
			return packedItem;
		}
	}
}
