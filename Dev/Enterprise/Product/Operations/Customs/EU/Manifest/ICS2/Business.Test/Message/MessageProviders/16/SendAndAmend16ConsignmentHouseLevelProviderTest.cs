using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test;

sealed class SendAndAmend16ConsignmentHouseLevelProviderTest : Customs.Business.Testing.DataProviderTestCase<SendAndAmend16ConsignmentHouseLevelProvider>
{
	public void TestSupportingDocuments()
	{
		var bill = CreateBill();
		bill.SupportingDocuments.AddNew();
		var provider = CreateProvider(bill);

		AssertEquals(1, provider.SupportingDocuments.Count);
	}

	public void TestAdditionalInformationCollection()
	{
		var bill = CreateBill();
		bill.AdditionalInfos.AddNew();
		var provider = CreateProvider(bill);

		AssertEquals(1, provider.AdditionalInformationCollection.Count);
	}

	public void TestAdditionalSupplyChainActors()
	{
		var bill = CreateBill();
		bill.CusSupplyChainActorReferences.AddNew();
		var provider = CreateProvider(bill);

		AssertEquals(1, provider.AdditionalSupplyChainActors.Count);
	}

	public void TestTransportDocumentMasterLevel()
	{
		var bill = CreateBill();
		bill.Header.AMA_MasterBill = "TransportDocumentMasterLevel";
		var provider = CreateProvider(bill);

		AssertEquals("TransportDocumentMasterLevel", provider.TransportDocumentMasterLevel.Identifier);
	}

	public void TestCarrierIdentificationNumber()
	{
		var orgCusCode = Factory.New<OrgCusCode>();
		orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Germany;
		orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		orgCusCode.OK_CustomsRegNo = "123456";

		var carrierOrg = Factory.New<OrgHeader>();
		orgCusCode.OK_OH = carrierOrg.PK;

		var bill = CreateBill();
		bill.Header.AMA_OA_Carrier = carrierOrg.MainAddress.PK;

		var provider = CreateProvider(bill);

		AssertEquals("DE123456", provider.CarrierIdentificationNumber);
	}

	public void TestGoodsShipmentBuyer()
	{
		var bill = CreateBill();
		bill.ABL_BuyerName = "TestBuyer";
		var provider = CreateProvider(bill);

		CombineAssertions(() =>
		{
			AssertType<BillPartyProvider>(provider.GoodsShipmentBuyer);
			AssertEquals("Name", "TestBuyer", provider.GoodsShipmentBuyer.Name);
		});
	}

	public void TestGoodsShipmentSeller()
	{
		var bill = CreateBill();
		bill.ABL_SellerName = "TestSeller";
		var provider = CreateProvider(bill);

		CombineAssertions(() =>
		{
			AssertType<BillPartyProvider>(provider.GoodsShipmentSeller);
			AssertEquals("Name", "TestSeller", provider.GoodsShipmentSeller.Name);
		});
	}

	public void TestTransportDocumentHouseLevel()
	{
		var bill = CreateBill();
		bill.ABL_BillNumber = "TestBillNumber";
		var provider = CreateProvider(bill);

		AssertEquals("TransportDocumentHouseLevel", "TestBillNumber", provider.TransportDocumentHouseLevel.Identifier);
	}

	public void TestUCRNumber()
	{
		var bill = CreateBill();
		bill.ABL_UCRNumber = "1234";
		var provider = CreateProvider(bill);

		AssertEquals("1234", provider.UCRNumber);
	}

	AsycudaBill CreateBill()
	{
		var header = Factory.New<AsycudaManifestHeader>();
		return header.Bills.AddNew();
	}

	SendAndAmend16ConsignmentHouseLevelProvider CreateProvider(AsycudaBill bill) => new(bill);

	protected sealed override SendAndAmend16ConsignmentHouseLevelProvider GetProvider() => CreateProvider(CreateBill());
}
