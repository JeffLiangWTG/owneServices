using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS313AndTS315GoodsShipmentItemTypeProviderBaseOnlyTest : TS313AndTS315GoodsShipmentItemTypeProviderTest<TS313AndTS315GoodsShipmentItemTypeProvider>
	{
		protected override TS313AndTS315GoodsShipmentItemTypeProvider GetProvider(TemporaryStoragePackedItem item)
		{
			var headerProvider = new TS313AndTS315GoodsShipmentTypeProvider((TemporaryStorageHeader)item.Bill.Header);
			return (TS313AndTS315GoodsShipmentItemTypeProvider)headerProvider.GoodsShipmentItem.First();
		}
	}

	abstract class TS313AndTS315GoodsShipmentItemTypeProviderTest<T> : DataProviderTestCase<T> where T : TS313AndTS315GoodsShipmentItemTypeProvider
	{
		public void TestGoodsInformation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.MasterBill;
			var item = bill.PackedItems.AddNew();
			item.API_GoodsDescription = "123";
			AssertEquals("123", GetProvider(item).GoodsInformation.GoodsDescription);
		}

		public void TestContainerIds()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.MasterBill;
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "A";
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;

			var item1 = bill.PackedItems.AddNew();
			item1.TemporaryStorageLinkPackages.First().IsLinked = true;

			var item2 = bill.PackedItems.AddNew();
			item2.TemporaryStorageLinkPackages.First().IsLinked = false;

			AssertEquals("A", GetProvider(item1).ContainerIds.ToArray()[0]);
		}

		public void TestGoodsItemNumber()
		{
			var item = Factory.New<TemporaryStorageHeader>().MasterBill.PackedItems.AddNew();
			item.API_LineNo = 1;
			AssertEquals("1", GetProvider(item).GoodsItemNumber);
		}

		public void TestDocumentsAuthorisations()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var item = header.MasterBill.PackedItems.AddNew();
			item.SupportingDocuments.AddNew().CSI_Code = "SUP";
			AssertEquals("SUP", GetProvider(item).DocumentsAuthorisations.ProducedDocuments.ToArray()[0].Type);
		}

		public void TestSupplyChainActors()
		{
			var item = Factory.New<TemporaryStorageHeader>().MasterBill.PackedItems.AddNew();
			item.SupplyChainActors.AddNew().CFR_Code = "ACT";
			AssertEquals("ACT", GetProvider(item).SupplyChainActors.ToArray()[0].Role);
		}

		protected abstract T GetProvider(TemporaryStoragePackedItem item);
		protected override T GetProvider() => GetProvider(Factory.New<TemporaryStorageHeader>().MasterBill.PackedItems.AddNew());
	}
}
