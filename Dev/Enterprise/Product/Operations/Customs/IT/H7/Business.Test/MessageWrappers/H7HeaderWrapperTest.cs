using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.H7.Business.Testing;

[TestedType(typeof(H7HeaderWrapper))]
public sealed class H7HeaderWrapperTest : DataProviderTestCase<H7HeaderWrapper>
{
	public void TestAdditionalDeclarationType()
	{
		SetUpTests();
		bill.ABL_ShipmentType = EntrySubStyleList.Codes.NormalDeclaration;

		var wrapper = new H7HeaderWrapper(bill);
		AssertEquals(nameof(H7HeaderWrapper.AdditionalDeclarationType), "A", wrapper.AdditionalDeclarationType);
	}

	public void TestAdditionalInformation()
	{
		SetUpTests();
		var addInfo1 = bill.AdditionalInfos.AddNew();
		addInfo1.CSI_Code = "00100";
		addInfo1.CSI_Description = "Description1";
		addInfo1.CSI_SubType = "INF";
		var addInfo2 = bill.AdditionalInfos.AddNew();
		addInfo2.CSI_Code = "00200";
		addInfo2.CSI_Description = "Description2";
		addInfo2.CSI_SubType = "INF";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.AdditionalInformation);
			AssertEquals(2, wrapper.AdditionalInformation.Count);
			var wrapperAddInfo1 = wrapper.AdditionalInformation.First();
			AssertEquals("00100", wrapperAddInfo1.Code);
			AssertEquals("Description1", wrapperAddInfo1.Description);
			var wrapperAddInfo2 = wrapper.AdditionalInformation.Skip(1).First();
			AssertEquals("00200", wrapperAddInfo2.Code);
			AssertEquals("Description2", wrapperAddInfo2.Description);
		});
	}

	public void TestAdditionalReferences()
	{
		SetUpTests();
		var addRef1 = bill.AdditionalInfos.AddNew();
		addRef1.CSI_Code = "00100";
		addRef1.CSI_ReferenceNumber = "Ref1";
		addRef1.CSI_SubType = "REF";
		var addRef2 = bill.AdditionalInfos.AddNew();
		addRef2.CSI_Code = "00200";
		addRef2.CSI_ReferenceNumber = "Ref2";
		addRef2.CSI_SubType = "REF";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.AdditionalReferences);
			AssertEquals(2, wrapper.AdditionalReferences.Count);
			var wrapperAddRef1 = wrapper.AdditionalReferences.First();
			AssertEquals("00100", wrapperAddRef1.ReferenceType);
			AssertEquals("Ref1", wrapperAddRef1.ReferenceNumber);
			var wrapperAddRef2 = wrapper.AdditionalReferences.Skip(1).First();
			AssertEquals("00200", wrapperAddRef2.ReferenceType);
			AssertEquals("Ref2", wrapperAddRef2.ReferenceNumber);
		});
	}

	public void TestAmendment()
	{
		AssertNull(nameof(H7HeaderWrapper.Amendment), Provider.Amendment);
	}

	public void TestDeclarant()
	{
		SetUpTests();
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var address = orgHeader.Addresses.AddNew();
		var customCode = address.CustomsCodes.AddNew("EOR", "IT123456789", "IT");
		header.AMA_OA_Declarant = address.PK;

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			var declarant = wrapper.Declarant;
			AssertNotNull(declarant);
			AssertSame("Cached", declarant, wrapper.Declarant);
		});
	}

	public void TestDeclarationCustomsOffice()
	{
		SetUpTests();
		header.PresentationOffice = "ABC";

		var wrapper = new H7HeaderWrapper(bill);
		AssertEquals(nameof(H7HeaderWrapper.DeclarationCustomsOffice), "ABC", wrapper.DeclarationCustomsOffice);
	}

	public void TestDeferredPayment()
	{
		SetUpTests();
		header.AMA_PaymentAccountNumber = "12345";

		var wrapper = new H7HeaderWrapper(bill);
		AssertEquals(nameof(H7HeaderWrapper.DeferredPayment), "12345", wrapper.DeferredPayment);
	}

	public void TestExporter()
	{
		SetUpTests();
		bill.ABL_ShipperName = "Shipper Name";
		bill.ABL_ShipperStreet1 = "Shipper Street 1";
		bill.ABL_ShipperStreet2 = "Street 2 ";
		bill.ABL_RN_NKShipperCountry = "AU";
		bill.ABL_ShipperPostcode = "2000";
		bill.ABL_ShipperCity = "ShipperSydney";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			var exporter = wrapper.Exporter;
			AssertNotNull(exporter);
			AssertSame("Cached", exporter, wrapper.Exporter);
			AssertNull(exporter.IdentificationNumber);
			AssertNotNull(exporter.Address);
			AssertEquals("Shipper Name", exporter.Address.Name);
			AssertEquals("Shipper Street 1 Street 2", exporter.Address.StreetAndNumber);
			AssertEquals("AU", exporter.Address.Country);
			AssertEquals("2000", exporter.Address.ZipCode);
			AssertEquals("ShipperSydney", exporter.Address.City);
		});
	}

	public void TestFiscalReferences()
	{
		SetUpTests();

		bill.ABL_SellerRegNo = ZString.Empty;

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions("When ABL_SellerRegNo is empty, there are no FiscalReferences", () =>
		{
			AssertNotNull(wrapper.FiscalReferences);
			AssertEquals(0, wrapper.FiscalReferences.Count);
		});

		bill.ABL_SellerRegNo = "Test123456";
		bill.ABL_SellerRegNoType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
		wrapper = new H7HeaderWrapper(bill);
		CombineAssertions("When conditions met, FiscalReferences is populated", () =>
		{
			AssertNotNull(wrapper.FiscalReferences);
			AssertEquals(1, wrapper.FiscalReferences.Count);
			var fiscalReference = wrapper.FiscalReferences.First();
			AssertEquals("Test123456", fiscalReference.IdentificationNumber);
			AssertEquals("FR5", fiscalReference.Role);
		});
	}

	public void TestGrossMass()
	{
		SetUpTests();
		bill.ABL_GrossWeight = 980;
		bill.ABL_GrossWeightUQ = "KG";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(H7HeaderWrapper.GrossMass), 980m, wrapper.GrossMass);

			bill.ABL_GrossWeightUQ = "G";
			AssertEquals(nameof(H7HeaderWrapper.GrossMass), 0.98m, wrapper.GrossMass);
		});
	}

	public void TestImporter()
	{
		SetUpTests();
		bill.ABL_ConsigneeRegNo = ZString.Empty;

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions("When ABL_ConsigneeRegNo is empty, Importer is Null", () =>
		{
			AssertNull(wrapper.Importer);
		});

		bill.ABL_ConsigneeRegNo = "Cons123456";
		bill.ABL_ConsigneeRegNoType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		bill.ABL_ConsigneeName = "Consignee Name";
		bill.ABL_ConsigneeStreet1 = "Consignee Street 1";
		bill.ABL_ConsigneeStreet2 = "Street 2 ";
		bill.ABL_RN_NKConsigneeCountry = "CN";
		bill.ABL_ConsigneePostcode = "510000";
		bill.ABL_ConsigneeCity = "ConsigneeGuangzhou";

		wrapper = new H7HeaderWrapper(bill);
		CombineAssertions("When conditions met, Importer is populated", () =>
		{
			var importer = wrapper.Importer;
			AssertNotNull(wrapper.Importer);
			AssertSame("Cached", importer, wrapper.Importer);
			AssertEquals("Cons123456", wrapper.Importer.IdentificationNumber);
			AssertEquals("Consignee Name", wrapper.Importer.Address.Name);
			AssertEquals("Consignee Street 1 Street 2", wrapper.Importer.Address.StreetAndNumber);
			AssertEquals("CN", wrapper.Importer.Address.Country);
			AssertEquals("510000", wrapper.Importer.Address.ZipCode);
			AssertEquals("ConsigneeGuangzhou", wrapper.Importer.Address.City);
		});
	}

	public void TestLrn()
	{
		SetUpTests();
		var wrapper = new H7HeaderWrapper(bill);
		bill.LocalReferenceNumber = "LRN12345";

		AssertEquals(@"LRN returns place holder, even if LRN is already set on bill.
This could not happen in production because the LRN will generate after the XML message is generated", "_IT_MSG_NMBR_PLCHLDR_", wrapper.Lrn);
	}

	public void TestLocationOfGoods()
	{
		SetUpTests();

		var goodsLocation = bill.CusGoodsLocation;
		goodsLocation.CGL_Type = "A";
		goodsLocation.CGL_Qualifier = "Z";
		goodsLocation.CGL_AdditionalIdentifier = "AdditionalIdentifier";

		var address = goodsLocation.Address;
		address.E2_CompanyName = "WiseTech";
		address.E2_Address1 = "25 Bourke Road";
		address.E2_Address2 = "Alexandria";
		address.E2_RN_NKCountryCode = "AU";
		address.E2_GovRegNum = "GovRegNum12345";
		address.Postcode = "2015";
		address.City = "Sydney";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.LocationOfGoods);
			AssertEquals("A", wrapper.LocationOfGoods.Type);
			AssertEquals("Z", wrapper.LocationOfGoods.Qualifier);
			AssertEquals("AdditionalIdentifier", wrapper.LocationOfGoods.LocationIdentifier);

			AssertNotNull(wrapper.LocationOfGoods.Address);
			AssertEquals("WiseTech", wrapper.LocationOfGoods.Address.Name);
			AssertEquals("25 Bourke Road Alexandria", wrapper.LocationOfGoods.Address.StreetAndNumber);
			AssertEquals("AU", wrapper.LocationOfGoods.Address.Country);
			AssertEquals("2015", wrapper.LocationOfGoods.Address.ZipCode);
			AssertEquals("Sydney", wrapper.LocationOfGoods.Address.City);
		});
	}

	public void TestRepresentative()
	{
		SetUpTests();
		header.AMA_AgentType = EUH7AgentTypes.Codes.DIR;
		var orgHeader = Factory.New<OrgHeader>();
		var address = orgHeader.Addresses.AddNew();
		var orgCusCode = address.CustomsCodes.AddNew();
		orgCusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		orgCusCode.OK_CustomsRegNo = "RepRegNo123";
		header.AMA_OA_Representative = address.PK;

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertEquals(2, wrapper.Representative.RepresentativeType);
			AssertNull(wrapper.Representative.EoriNumber);
			AssertEquals("RepRegNo123", wrapper.Representative.IdentificationNumber);
		});
	}

	public void TestPreviousDocuments()
	{
		SetUpTests();

		var previousDoc1 = bill.PreviousDocuments.AddNew();
		previousDoc1.CSI_Code = "123";
		previousDoc1.CSI_ReferenceNumber = "Ref1";
		var previousDoc2 = bill.PreviousDocuments.AddNew();
		previousDoc2.CSI_Code = "234";
		previousDoc2.CSI_ReferenceNumber = "Ref2";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.PreviousDocuments);
			AssertEquals(2, wrapper.PreviousDocuments.Count);
			var wrapperPreviousDoc1 = wrapper.PreviousDocuments.First();
			AssertEquals("123", wrapperPreviousDoc1.DocumentType);
			AssertEquals("Ref1", wrapperPreviousDoc1.ReferenceNumber);
			var wrapperPreviousDoc2 = wrapper.PreviousDocuments.Skip(1).First();
			AssertEquals("234", wrapperPreviousDoc2.DocumentType);
			AssertEquals("Ref2", wrapperPreviousDoc2.ReferenceNumber);
		});
	}

	public void TestSupportingDocuments()
	{
		SetUpTests();

		var supportingDoc1 = bill.SupportingDocuments.AddNew();
		supportingDoc1.CSI_Code = "YYY";
		supportingDoc1.CSI_ReferenceNumber = "Ref1";
		var supportingDoc2 = bill.SupportingDocuments.AddNew();
		supportingDoc2.CSI_Code = "ZZZ";
		supportingDoc2.CSI_ReferenceNumber = "Ref2";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.SupportingDocuments);
			AssertEquals(2, wrapper.SupportingDocuments.Count);
			var wrapperSupportingDoc1 = wrapper.SupportingDocuments.First();
			AssertEquals("YYY", wrapperSupportingDoc1.Code);
			AssertEquals("Ref1", wrapperSupportingDoc1.ReferenceNumber);
			var wrapperSupportingDoc2 = wrapper.SupportingDocuments.Skip(1).First();
			AssertEquals("ZZZ", wrapperSupportingDoc2.Code);
			AssertEquals("Ref2", wrapperSupportingDoc2.ReferenceNumber);
		});
	}

	public void TestTransportDocuments()
	{
		SetUpTests();

		var transportDoc1 = bill.AdditionalInfos.AddNew();
		transportDoc1.CSI_Code = "00100";
		transportDoc1.CSI_ReferenceNumber = "Ref1";
		transportDoc1.CSI_SubType = "TRA";
		var transportDoc2 = bill.AdditionalInfos.AddNew();
		transportDoc2.CSI_Code = "00200";
		transportDoc2.CSI_ReferenceNumber = "Ref2";
		transportDoc2.CSI_SubType = "TRA";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.TransportDocuments);
			AssertEquals(2, wrapper.TransportDocuments.Count);
			var wrapperTransportDoc1 = wrapper.TransportDocuments.First();
			AssertEquals("00100", wrapperTransportDoc1.DocumentType);
			AssertEquals("Ref1", wrapperTransportDoc1.ReferenceNumber);
			var wrapperTransportDoc2 = wrapper.TransportDocuments.Skip(1).First();
			AssertEquals("00200", wrapperTransportDoc2.DocumentType);
			AssertEquals("Ref2", wrapperTransportDoc2.ReferenceNumber);
		});
	}

	public void TestTransportCosts()
	{
		SetUpTests();
		bill.ABL_InsuranceValue = 1200;
		bill.ABL_TransportValue = 34;
		bill.ABL_RX_NKTransportValueCurrency = "EUR";

		var wrapper = new H7HeaderWrapper(bill);
		CombineAssertions(() =>
		{
			AssertEquals(nameof(wrapper.TransportCosts.Amount), 1234m, wrapper.TransportCosts.Amount);
			AssertEquals(nameof(wrapper.TransportCosts.Currency), "EUR", wrapper.TransportCosts.Currency);
		});
	}

	public void TestUcr()
	{
		SetUpTests();
		bill.ABL_UCRNumber = "1234";

		var wrapper = new H7HeaderWrapper(bill);
		AssertEquals(nameof(H7HeaderWrapper.Ucr), "1234", wrapper.Ucr);
	}

	void SetUpTests()
	{
		header = Factory.New<AsycudaManifestHeader>();
		bill = header.Bills.AddNew();
	}

	protected override H7HeaderWrapper GetProvider()
	{
		var pHeader = Factory.New<AsycudaManifestHeader>();
		var pBill = pHeader.Bills.AddNew();
		return new H7HeaderWrapper(pBill);
	}

	AsycudaManifestHeader header;
	AsycudaBill bill;
}
