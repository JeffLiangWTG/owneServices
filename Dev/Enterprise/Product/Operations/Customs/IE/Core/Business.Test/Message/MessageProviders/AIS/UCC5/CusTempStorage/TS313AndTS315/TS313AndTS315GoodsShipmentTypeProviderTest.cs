using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TS313AndTS315GoodsShipmentTypeProviderBaseOnlyTest : TS313AndTS315GoodsShipmentTypeProviderTest<TS313AndTS315GoodsShipmentTypeProvider>
	{
		protected override TS313AndTS315GoodsShipmentTypeProvider GetProvider(TemporaryStorageHeader header) => new TS313AndTS315GoodsShipmentTypeProvider(header);
	}

	abstract class TS313AndTS315GoodsShipmentTypeProviderTest<T> : DataProviderTestCase<T>
		where T : TS313AndTS315GoodsShipmentTypeProvider
	{
		public void TestDocumentsAuthorisations()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.MasterBill;
			bill.ABL_UCRNumber = "UCR12";
			var provider = GetProvider(header);
			var documentsAuthorisations = provider.DocumentsAuthorisations;
			AssertEquals("UCR12", documentsAuthorisations.UCR);

			bill.ABL_UCRNumber = "UCR34";
			provider = GetProvider(header);
			documentsAuthorisations = provider.DocumentsAuthorisations;
			AssertEquals("UCR34", documentsAuthorisations.UCR);
		}

		public void TestGoodsLocation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var goodsLocation = header.GoodsLocation;
			goodsLocation.CGL_Type = "AB";
			var provider = GetProvider(header);
			var iGoodsLocation = provider.GoodsLocation;
			AssertEquals("AB", iGoodsLocation.Type);

			goodsLocation.CGL_Type = "CD";
			provider = GetProvider(header);
			iGoodsLocation = provider.GoodsLocation;
			AssertEquals("CD", iGoodsLocation.Type);
		}

		public void TestPresentation()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var orgHeader = Factory.New<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgHeader.CustomsCodes.AddNew("EOR", "IE123456789", "IE");
			header.AMA_OA_Presenter = orgAddress.PK;
			header.CustomsOfficeOfFirstEntry = "Test";
			header.AMA_TransportMeans = "10";
			header.AMA_VesselName = "IMO8712345";
			var provider = GetProvider(header);
			var iTSPresentation = provider.Presentation;
			AssertEquals("PresentationTrader", "IE123456789", iTSPresentation.PresentationTrader);
			AssertEquals("FirstEntryCustomsOffice", "Test", iTSPresentation.FirstEntryCustomsOffice);
			AssertEquals("TransportType", "10", iTSPresentation.ActiveBorderTransportMeansId.Type);
			AssertEquals("Id", "IMO8712345", iTSPresentation.ActiveBorderTransportMeansId.Id);
		}

		public void TestTransportInformation()
		{
			AssertType<TS313AndTS315GoodsShipmentTypeTransportInformationProvider>(Provider.TransportInformation);
		}

		public void TestGoodsShipmentItem()
		{
			Assert("Should be IReadOnlyCollection<TS313AndTS315GoodsShipmentItemTypeProvider>", Provider.GoodsShipmentItem is IReadOnlyCollection<TS313AndTS315GoodsShipmentItemTypeProvider>);
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.MasterBill;
			var item = bill.PackedItems.AddNew();
			item.API_GoodsDescription = "123";
			AssertEquals("123", GetProvider(header).GoodsShipmentItem.ToArray()[0].GoodsInformation.GoodsDescription);
		}

		public void TestGrossMass()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.MasterBill;
			bill.ABL_GrossWeight = 1;
			bill.ABL_GrossWeightUQ = "T";
			AssertEquals("Convert to KG", 1000m, GetProvider(header).GrossMass);
		}

		public void TestSupplyChainActors()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill = header.MasterBill;
			bill.SupplyChainActors.AddNew().CFR_Code = "ACT";
			AssertEquals("ACT", GetProvider(header).SupplyChainActors.ToArray()[0].Role);
		}

		protected abstract T GetProvider(TemporaryStorageHeader header);
		protected override T GetProvider() => GetProvider(header);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<TemporaryStorageHeader>();
		}
		TemporaryStorageHeader header;
	}
}
