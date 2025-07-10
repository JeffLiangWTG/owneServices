using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TSGoodsShipmentItemTypeGoodsInformationProviderTest : DataProviderTestCase<TSGoodsShipmentItemTypeGoodsInformationProvider>
	{
		public void TestGoodsDescription()
		{
			packedItem.API_GoodsDescription = "Sample";
			AssertEquals("GoodsDescription", "Sample", Provider.GoodsDescription);
		}

		public void TestGrossMass()
		{
			packedItem.API_GrossWeight = 1000;
			packedItem.API_GrossWeightUQ = "KG";
			AssertEquals("PackedItem GrossMass", 1000m, Provider.GrossMass);
		}

		public void TestPackages()
		{
			Assert("Should be IReadOnlyCollection<PackageProvider>", Provider.Packages is IReadOnlyCollection<IPackaging>);
			package.IsLinked = true;
			AssertEquals("Count", 1, GetProvider().Packages.Count);
			package.IsLinked = false;
			AssertEquals("Count", 0, GetProvider().Packages.Count);
		}

		public void TestCusCode()
		{
			packedItem.API_ChemicalSubstanceCode = "CLR";
			AssertEquals("CusCode", "CLR", Provider.CusCode);
		}

		public void TestCommodityCode()
		{
			packedItem.API_Tariff = "89001262";
			AssertEquals("CommodityCode", "890012", Provider.CommodityCode);
		}

		protected override TSGoodsShipmentItemTypeGoodsInformationProvider GetProvider() => TSGoodsShipmentItemTypeGoodsInformationProvider.New(packedItem);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			packedItem = bill.PackedItems.AddNew();
			package = packedItem.TemporaryStorageLinkPackages.FirstOrDefault();
			package.Package = pack;
		}

		TemporaryStoragePackedItem packedItem;
		EU.Business.CusTempStorage.TemporaryStorageLinkPackage package;
	}
}
