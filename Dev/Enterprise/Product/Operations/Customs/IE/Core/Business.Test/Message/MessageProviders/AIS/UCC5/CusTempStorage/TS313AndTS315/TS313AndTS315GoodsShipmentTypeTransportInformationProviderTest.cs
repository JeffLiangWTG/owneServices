using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS313AndTS315GoodsShipmentTypeTransportInformationProviderTest : DataProviderTestCase<TS313AndTS315GoodsShipmentTypeTransportInformationProvider>
	{
		public void TestActiveBorderTransportMeans()
		{
			Assert(Provider.ActiveBorderTransportMeans is IMeansIdentityAtBorderMandatory);
		}

		public void TestType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var arrivalTransportMeans = header.ArrivalTransportMeans;
			var provider = new TS313AndTS315GoodsShipmentTypeTransportInformationProvider(header, arrivalTransportMeans);
			arrivalTransportMeans.TPM_TypeOfIdentification = "A";
			AssertEquals("A", provider.Type);

			arrivalTransportMeans.TPM_TypeOfIdentification = "B";
			AssertEquals("B", provider.Type);
		}

		public void TestNumber()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var arrivalTransportMeans = header.ArrivalTransportMeans;
			var provider = new TS313AndTS315GoodsShipmentTypeTransportInformationProvider(header, arrivalTransportMeans);
			arrivalTransportMeans.TPM_IdentificationNumber = "IE123";
			AssertEquals("IE123", provider.Number);

			arrivalTransportMeans.TPM_IdentificationNumber = "IE456";
			AssertEquals("IE456", provider.Number);
		}

		public void TestContainerIdentificationNumber_Aggregation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var arrivalTransportMeans = header.ArrivalTransportMeans;
			var container1 = header.Containers.AddNew();
			container1.ACN_ContainerNumber = "CN1";
			var container2 = header.Containers.AddNew();
			container2.ACN_ContainerNumber = "CN2";
			var container3 = header.Containers.AddNew();
			container3.ACN_ContainerNumber = "CN3";

			var pack1 = header.MasterBill.Packs.AddNew();
			pack1.ContainerPK = container1.PK;
			var pack2 = header.MasterBill.Packs.AddNew();
			pack2.ContainerPK = container2.PK;
			var pack3 = header.MasterBill.Packs.AddNew();
			pack3.ContainerPK = container3.PK;

			var packedItem1 = header.MasterBill.PackedItems.AddNew();
			packedItem1.API_LineNo = 1;
			packedItem1.TemporaryStorageLinkPackages.FirstOrDefault(link => link.Package.Container == container1).IsLinked = true;
			packedItem1.TemporaryStorageLinkPackages.FirstOrDefault(link => link.Package.Container == container3).IsLinked = true;
			var packedItem2 = header.MasterBill.PackedItems.AddNew();
			packedItem2.API_LineNo = 2;
			packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(link => link.Package.Container == container1).IsLinked = true;
			packedItem2.TemporaryStorageLinkPackages.FirstOrDefault(link => link.Package.Container == container2).IsLinked = true;

			var headerProvider = new TS313AndTS315GoodsShipmentTypeProvider(header);
			var transportInformationProvider = (TS313AndTS315GoodsShipmentTypeTransportInformationProvider)headerProvider.TransportInformation;

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("Container selected by all items will be promoted to header.", new[] { "CN1" }, transportInformationProvider.ContainerIdentificationNumber);

				var itemProvider1 = headerProvider.GoodsShipmentItem.First(item => item.GoodsItemNumber == "1");
				AssertContainsExactElementsInAnyOrder("Container only selected by item 1 should be in item 1.", new[] { "CN3" }, itemProvider1.ContainerIds);
				var itemProvider2 = headerProvider.GoodsShipmentItem.First(item => item.GoodsItemNumber == "2");
				AssertContainsExactElementsInAnyOrder("Container only selected by item 2 should be in item 2.", new[] { "CN2" }, itemProvider2.ContainerIds);
			});
		}

		public void TestContainerIdentificationNumber()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var arrivalTransportMeans = header.ArrivalTransportMeans;
			var container1 = header.Containers.AddNew();
			container1.ACN_ContainerNumber = "CN1";
			var container2 = header.Containers.AddNew();
			container2.ACN_ContainerNumber = "CN2";
			var pack1 = header.MasterBill.Packs.AddNew();
			pack1.ContainerPK = container1.PK;
			var pack2 = header.MasterBill.Packs.AddNew();
			pack2.ContainerPK = container2.PK;

			var packedItem1 = header.MasterBill.PackedItems.AddNew();
			packedItem1.API_LineNo = 1;
			packedItem1.TemporaryStorageLinkPackages.ForEach(link => link.IsLinked = true);

			var headerProvider = new TS313AndTS315GoodsShipmentTypeProvider(header);
			var transportInformationProvider = (TS313AndTS315GoodsShipmentTypeTransportInformationProvider)headerProvider.TransportInformation;

			AssertContainsExactElementsInAnyOrder(new[] { "CN1","CN2" }, transportInformationProvider.ContainerIdentificationNumber);
		}

		public void TestSeal()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var arrivalTransportMeans = header.ArrivalTransportMeans;
			var container1 = header.Containers.AddNew();
			container1.ACN_Seal1 = "2";
			container1.ACN_Seal2 = "4";
			var container2 = header.Containers.AddNew();
			container2.ACN_Seal3 = "1";
			var container3 = header.Containers.AddNew();
			container3.AdditionalSeals.AddNew().BK_SealNumber = "3";
			container3.AdditionalSeals.AddNew().BK_SealNumber = "5";
			container3.AdditionalSeals.AddNew().BK_SealNumber = "";
			var provider = new TS313AndTS315GoodsShipmentTypeTransportInformationProvider(header, arrivalTransportMeans);
			AssertEquals("5", provider.Seal.SealNumber);
			AssertContainsExactElementsInExactOrder(new[] { "1", "2", "3", "4", "5" }, provider.Seal.SealIds);
		}

		protected override TS313AndTS315GoodsShipmentTypeTransportInformationProvider GetProvider()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var container = header.Containers.AddNew();
			var pack = header.MasterBill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			var packedItem = header.MasterBill.PackedItems.AddNew();
			foreach (var item in packedItem.TemporaryStorageLinkPackages)
			{
				item.IsLinked = true;
			}
			var headerProvider = new TS313AndTS315GoodsShipmentTypeProvider(header);
			return (TS313AndTS315GoodsShipmentTypeTransportInformationProvider)headerProvider.TransportInformation;
		}
	}
}
