using System;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

sealed class SendAndAmend17ConsignmentHouseLevelProviderTest : DataProviderTestCase<SendAndAmend17ConsignmentHouseLevelProvider>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentException>("AsycudaBill missing", () => GenerateProvider(null));
			AssertNoExceptionThrown(() => GenerateProvider(bill));
		});
	}

	public void TestSupportingDocuments()
	{
		bill.SupportingDocuments.AddNew();
		AssertEquals("SupportingDocuments Count", 1, Provider.SupportingDocuments.Count);
	}

	public void TestAdditionalInformations()
	{
		bill.AdditionalInfos.AddNew();
		AssertEquals("AdditionalInformation", 1, Provider.AdditionalInformations.Count);
	}

	public void TestAdditionalSupplyChainActors()
	{
		bill.CusSupplyChainActorReferences.AddNew();
		AssertEquals("AdditionalSupplyChainActors", 1, Provider.AdditionalSupplyChainActors.Count);
	}

	public void TestTransportDocumentMasterLevel()
	{
		bill.Header.AMA_MasterBill = "TransportDocumentMasterLevel";
		AssertEquals("TransportDocumentMasterLevel", "TransportDocumentMasterLevel", Provider.TransportDocumentMasterLevel.Identifier);
	}

	public void TestCarrierIdentificationNumber()
	{
		AssertNotNull("CarrierIdentificationNumber", Provider.CarrierIdentificationNumber);
	}

	public void TestGoodsShipmentBuyer()
	{
		bill.ABL_BuyerName = "TestBuyer";
		AssertType<BillPartyProvider>("NotifyBuyer", Provider.GoodsShipmentBuyer);
		AssertEquals("Buyer Name", "TestBuyer", Provider.GoodsShipmentBuyer.Name);
	}

	public void TestGoodsShipmentSeller()
	{
		bill.ABL_SellerName = "TestSeller";
		AssertType<BillPartyProvider>("NotifySeller", Provider.GoodsShipmentSeller);
		AssertEquals("Seller Name", "TestSeller", Provider.GoodsShipmentSeller.Name);
	}

	public void TestTransportDocumentHouseLevel()
	{
		bill.ABL_BillNumber = "TestBillNumber";
		AssertEquals("TransportDocumentHouseLevel", "TestBillNumber", Provider.TransportDocumentHouseLevel.Identifier);
	}

	public void TestUCRNumber()
	{
		bill.ABL_UCRNumber = "1234";
		AssertEquals("UCRNumber", "1234", Provider.UCRNumber);
	}

	SendAndAmend17ConsignmentHouseLevelProvider GenerateProvider(AsycudaBill bill) => new SendAndAmend17ConsignmentHouseLevelProvider(bill);

	protected override void SetUp()
	{
		base.SetUp();

		var header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
	}
	AsycudaBill bill;

	protected sealed override SendAndAmend17ConsignmentHouseLevelProvider GetProvider()
	{
		return GenerateProvider(bill);
	}
}
