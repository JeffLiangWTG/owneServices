using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.H7.Business.Testing
{
	sealed class AsycudaPackedItemLookupsTest : BusinessObjectLookupsTestCase
	{
		[TestDate(2000, 6, 6)]
		public void TestTariffList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Constants.DataGrouping.EuropeanUnion, Constants.TariffTypes.Import);
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Constants.DataGrouping.EuropeanUnion, tariffType.PK, "11112222", new ZDateTime(2000, 1, 1), new ZDateTime(2001, 1, 1));

			var belfast = new RefUNLOCO.Loader(Factory).Load("GBBEL");
			var refCountryStates = Factory.New<RefCountryStates>();
			refCountryStates.RW_RN_NKCountryCode = "UK";
			refCountryStates.RW_Code = "XXX";
			refCountryStates.RW_RegionName = "NORTHERN IRELAND";
			belfast.RL_RW = refCountryStates.PK;
			Factory.Save();

			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfDischarge = "GBBEL";
			manifest.Bills.AddNew();
			var bill = manifest.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			packedItem.Lookups.TariffList.Load();
			AssertCollectionContains("Should have EUN tariff codes for North Ireland", tariff, packedItem.Lookups.TariffList);

			manifest.AMA_RL_NKPortOfDischarge = ZString.Empty;
			packedItem.Lookups.TariffList.Load();
			AssertCollectionNotContains("Should have EUN tariff codes for North Ireland", tariff, packedItem.Lookups.TariffList);
		}
	}
}
