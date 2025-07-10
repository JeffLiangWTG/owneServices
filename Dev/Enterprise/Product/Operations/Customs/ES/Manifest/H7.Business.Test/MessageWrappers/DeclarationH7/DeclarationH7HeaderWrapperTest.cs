using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class DeclarationH7HeaderWrapperTest : DataProviderTestCase<DeclarationH7HeaderWrapper>
{
	public void TestSupervisingCustomsOffice()
	{
		SetUpTestData();
		bill.Header.AMA_CustomsOffice = "CusOffice";
		AssertEquals("CusOffice", wrapper.SupervisingCustomsOffice);
	}

	public void TestReferenceNumberUCR()
	{
		SetUpTestData();
		bill.ABL_UCRNumber = "UCRNumber";
		AssertEquals("UCRNumber", wrapper.ReferenceNumberUCR);
	}

	public void TestAdditionalProcedures()
	{
		SetUpTestData();
		bill.ABL_Procedure = "Procedure";
		AssertEquals("Procedure", wrapper.AdditionalProcedures.First());
	}

	public void TestGoodsLocation()
	{
		SetUpTestData();
		bill.CusGoodsLocation.Address.AuthorisationNumber = "AuthorizationNumber";
		var prevDoc = bill.PreviousDocuments.AddNew();
		prevDoc.CSI_Code = "999";
		AssertEquals("don't send GoodsLocation if no G3MRN, and there is no previous document with type 337", string.Empty, wrapper.GoodsLocation);

		prevDoc.CSI_Code = "337";
		AssertEquals("send Bill.LocatinOfGoods when any bill on the header has previous document type 337", "AuthorizationNumber", wrapper.GoodsLocation);

		prevDoc.CSI_Code = "335";
		AssertEquals("post-assertion: don't send GoodsLocation", string.Empty, wrapper.GoodsLocation);

		bill.G3MovementReferenceNumber = "123";
		header.CusGoodsLocation.Address.AuthorisationNumber = "HeaderAN";
		AssertEquals("send Bill.LocatinOfGoods if it is not empty", "AuthorizationNumber", wrapper.GoodsLocation);

		bill.CusGoodsLocation.Address.AuthorisationNumber = string.Empty;
		AssertEquals("send Header.LocationOfGoods if Bill's is empty", "HeaderAN", wrapper.GoodsLocation);
	}

	public void TestDeclarationH7HeaderWrapper_SplitProcedureCodesWithPlusAsDelimiter()
	{
		SetUpTestData();
		bill.ABL_Procedure = "C07++F48+";

		CombineAssertions("Should split ABL_Procedure using '+' as delimiter and filters empty codes", () =>
		{
			AssertEquals("AdditionalProcedures count", 2, wrapper.AdditionalProcedures.Count);
			AssertEquals("First AdditionalProcedure", "C07", wrapper.AdditionalProcedures.First());
			AssertEquals("Second AdditionalProcedure", "F48", wrapper.AdditionalProcedures.Skip(1).First());
		});
	}

	public void TestSupportingDocuments()
	{
		SetUpTestData();
		var supportingDoc = bill.SupportingDocuments.AddNew();
		supportingDoc.CSI_Code = "CODE";
		CombineAssertions("Should find one supporting document with designated CSI_Code", () =>
		{
			Assert("SupportingDocuments contain only one element", wrapper.SupportingDocuments.Count == 1);
			Assert("SupportingDocument name is CODE", wrapper.SupportingDocuments.First().Name == "CODE");
		});
	}

	public void TestAdditionalReferences()
	{
		SetUpTestData();
		var additionalRef = bill.AdditionalDocuments.AddNew();
		additionalRef.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
		additionalRef.CSI_Code = "CODE";
		CombineAssertions("Should find one additional document with designated CSI_Code of subtype REF", () =>
		{
			Assert("AdditionalReferences contain only one element", wrapper.AdditionalReferences.Count == 1);
			Assert("AdditionalReference name is CODE", wrapper.AdditionalReferences.First().Name == "CODE");
		});
	}

	public void TestTransportDocuments()
	{
		SetUpTestData();
		bill.ABL_BillNumber = "BillNumber";
		CombineAssertions("Should find one additional document with designated CSI_Code 5025 of subtype TRA", () =>
		{
			Assert("TransportDocuments contain only one element", wrapper.TransportDocuments.Count == 1);
			Assert("TransportDocument name is 5025", wrapper.TransportDocuments.First().Name == "5025");
			Assert("TransportDocument number is BillNumber", wrapper.TransportDocuments.First().Number == "BillNumber");
		});

		var transportDoc = bill.AdditionalDocuments.AddNew();
		transportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
		transportDoc.CSI_Code = "5026";
		transportDoc.CSI_ReferenceNumber = "RefNumber";
		var wrapper1 = new DeclarationH7HeaderWrapper(bill);

		CombineAssertions("Should find two additional document with designated CSI_Code 5025 and 5026", () =>
		{
			Assert("TransportDocuments contain two elements", wrapper1.TransportDocuments.Count == 2);
			Assert("TransportDocuments contain one with name 5025 and number BillNumber", wrapper1.TransportDocuments.Any(t => t.Name == "5025" && t.Number == "BillNumber"));
			Assert("TransportDocuments contain one with name 5026 and number RefNumber", wrapper1.TransportDocuments.Any(t => t.Name == "5026" && t.Number == "RefNumber"));
		});
	}

	public void TestAdditionalInformation()
	{
		SetUpTestData();
		var additionalInfo = bill.AdditionalDocuments.AddNew();
		additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		additionalInfo.CSI_Code = "CODE";
		CombineAssertions("Should find one additional document with designated CSI_Code of subtype INF", () =>
		{
			Assert("AdditionalInformation contain only one element", wrapper.AdditionalInformation.Count == 1);
			Assert("AdditionalInformation name is CODE", wrapper.AdditionalInformation.First().Code == "CODE");
		});
	}

	public void TestPreviousDocuments()
	{
		SetUpTestData();
		var previousDoc = bill.PreviousDocuments.AddNew();
		previousDoc.CSI_Code = "CODE";
		CombineAssertions("Should find one previous document with designated CSI_Code", () =>
		{
			Assert("PreviousDocuments contain only one element", wrapper.PreviousDocuments.Count == 1);
			Assert("PreviousDocument name is CODE", wrapper.PreviousDocuments.First().Name == "CODE");
		});
	}

	public void TestDeclarant()
	{
		SetUpTestData();
		AssertNotNull(wrapper.Declarant);
		AssertType<H7DeclarantWrapper>(wrapper.Declarant);

		header.AMA_OA_Declarant = ZGuid.Empty;
		Assert("When AsycudaManifestHeader has no declarant, wrapper declarant should be null", wrapper.Declarant == null);
	}

	public void TestRepresentative()
	{
		SetUpTestData();
		CombineAssertions(() =>
		{
			bill.Header.AMA_AgentType = ESH7AgentTypes.Codes.SEL;
			AssertNull("When AMA_AgentType is SEL, the wrapper Representative is null", wrapper.Representative);

			bill.Header.AMA_AgentType = EUH7AgentTypes.Codes.IND;
			AssertNotNull("When AMA_AgentType is IND, the wrapper Representative is not null", wrapper.Representative);

			bill.Header.AMA_OA_Representative = Guid.Empty;
			AssertNull("When Representative is null, the wrapper Representative is null", wrapper.Representative);
		});
	}

	public void TestGrossWeight()
	{
		SetUpTestData();
		bill.ABL_GrossWeight = 500;
		bill.ABL_GrossWeightUQ = Constants.Weight.Grams;
		AssertEquals("Should convert to KG", 0.5m, wrapper.GrossWeight);
	}

	public void TestExporter()
	{
		SetUpTestData();
		var address = Factory.NewWithValidTestData<OrgAddress>();
		bill.ABL_OA_Shipper = address.PK;
		AssertType<PartyWrapper>(wrapper.Exporter);

		bill.ABL_OA_Shipper = ZGuid.Empty;
		AssertType<H7ExporterNonOrganizationWrapper>(wrapper.Exporter);
	}

	public void TestImporter()
	{
		SetUpTestData();
		AssertType<H7ImporterWrapper>(wrapper.Importer);
	}

	public void TestTransportCostToDestination()
	{
		SetUpTestData();
		AssertType<TransportCostWrapper>(wrapper.TransportCostToDestination);
	}

	public void TestAdditionalFiscalRef()
	{
		SetUpTestData();
		AssertEquals(1, wrapper.AdditionalFiscalRef.Count);
		AssertType<H7AdditionalFiscalRefWrapper>(wrapper.AdditionalFiscalRef.First());
	}

	void SetUpTestData()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;

		var broker = Factory.NewWithValidTestData<GlbStaff>();
		header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
		header.AMA_GS_NKCustomsAgent = broker.GS_Code;
		header.AMA_OA_Representative = orgAddress.PK;
		header.AMA_OA_Declarant = orgAddress.PK;
		bill = header.Bills.AddNew();

		wrapper = new DeclarationH7HeaderWrapper(bill);
	}

	protected override DeclarationH7HeaderWrapper GetProvider()
	{
		SetUpTestData();
		return wrapper;
	}

	DeclarationH7HeaderWrapper wrapper;
	AsycudaBill bill;
	AsycudaManifestHeader header;
}
