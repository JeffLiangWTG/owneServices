using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class SendAndAmend24ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend24ConsignmentHouseLevelProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("AsycudaBill missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(bill));
			});
		}

		public void TestTotalGrossMass()
		{
			bill.ABL_GrossWeight = 1231m;
			bill.ABL_GrossWeightUQ = "G";

			AssertEquals("GrossMass", 1.231m, Provider.TotalGrossMass);
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

		public void TestGoodsItems()
		{
			bill.Packs.AddNew();
			AssertEquals("GoodsItem", 1, Provider.GoodsItems.Count);
		}

		public void TestTransportDocumentMasterLevel()
		{
			bill.Header.AMA_MasterBill = "TransportDocumentMasterLevel";
			AssertEquals("TransportDocumentMasterLevel", "TransportDocumentMasterLevel", Provider.TransportDocumentMasterLevel.Identifier);
		}

		public void TestCarrierIdentificationNumber()
		{
			var carrierOrg = Factory.New<OrgHeader>();

			var orgCusCode = Factory.New<OrgCusCode>();
			orgCusCode.OK_RN_NKCodeCountry = "DE";
			orgCusCode.OK_CodeType = "EOR";
			orgCusCode.OK_CustomsRegNo = "123456";
			orgCusCode.OK_OH = carrierOrg.PK;

			var carrier = carrierOrg.MainAddress;
			bill.Header.AMA_OA_Carrier = carrier.PK;

			AssertEquals("DE123456", SendAndAmend24ConsignmentHouseLevelProvider.NewOrNull(bill).CarrierIdentificationNumber);
		}

		public void TestConsignee()
		{
			bill.ABL_ConsigneeName = "TestConsignee";
			AssertType<BillPartyProvider>("Consignee", Provider.Consignee);
			AssertEquals("Consignee", "TestConsignee", Provider.Consignee.Name);
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

		SendAndAmend24ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend24ConsignmentHouseLevelProvider(bill);

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<AsycudaManifestHeader>();
			bill = header.Bills.AddNew();
		}
		AsycudaBill bill;

		protected sealed override SendAndAmend24ConsignmentHouseLevelProvider GetProvider()
		{
			return GenerateProvider(bill);
		}
	}
}
