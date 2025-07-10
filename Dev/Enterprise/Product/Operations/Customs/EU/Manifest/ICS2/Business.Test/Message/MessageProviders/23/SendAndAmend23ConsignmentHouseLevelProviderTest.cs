using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend23ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend23ConsignmentHouseLevelProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("ManifestHeader missing", () => new SendAndAmend23ConsignmentHouseLevelProvider(null));
				AssertNoExceptionThrown(() => new SendAndAmend23ConsignmentHouseLevelProvider(bill));
			});
		}

		public void TestTotalGrossMass()
		{
			bill.ABL_GrossWeight = 12357.24m;
			bill.ABL_GrossWeightUQ = "G";
			AssertEquals("GrossMass", 12.357240m, Provider.TotalGrossMass);
		}

		public void TestAdditionalInformationCollection()
		{
			bill.AdditionalInfos.AddNew();
			AssertEquals("AdditionalInformation", 1, Provider.AdditionalInformationCollection.Count);
		}

		public void TestAdditionalSupplyChainActors()
		{
			bill.CusSupplyChainActorReferences.AddNew();
			AssertEquals("AdditionalSupplyChainActors", 1, Provider.AdditionalSupplyChainActors.Count);
		}

		public void TestConsignee()
		{
			bill.ABL_ConsigneeName = "TestConsignee";
			AssertType<BillPartyProvider>("Consignee", Provider.Consignee);
			AssertEquals("Consignee", "TestConsignee", Provider.Consignee.Name);
		}

		public void TestGoodsItems()
		{
			bill.Packs.AddNew();
			AssertEquals("GoodsItem", 1, Provider.GoodsItems.Count);
		}

		public void TestConsignor()
		{
			bill.ABL_ShipperName = "TestConsignor";
			AssertType<BillPartyProvider>("Consignor", Provider.Consignor);
			AssertEquals("Consignor", "TestConsignor", Provider.Consignor.Name);
		}

		public void TestTransportDocumentHouseLevel()
		{
			bill.ABL_BillNumber = "TestBillNumber";
			AssertEquals("TransportDocumentHouseLevel", "TestBillNumber", Provider.TransportDocumentHouseLevel.Identifier);
		}

		SendAndAmend23ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend23ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected sealed override SendAndAmend23ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
