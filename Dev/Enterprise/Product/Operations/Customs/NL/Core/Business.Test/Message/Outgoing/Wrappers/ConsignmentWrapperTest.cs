using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class ConsignmentWrapperTest : DataProviderTestCase<ConsignmentWrapper>
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("entry header null", () => new ConsignmentWrapper(null, new JobDeclarationMessageSendingObject(entryHeader)));
		AssertExceptionThrown<ArgumentNullException>("message sending object null", () => new ConsignmentWrapper(entryHeader, null));
	});

	public void TestContainerCode()
	{
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "MSCU0051257";

		AssertEquals("1", wrapper.ContainerCode);
	}

	public void TestTotalGrossMassMeasure()
	{
		var invoice = declaration.Invoices.AddNew();
		var invoiceline2 = invoice.InvoiceLines.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		invoiceline.JI_CEI = entryInstruction.PK;
		invoiceline2.JI_CEI = entryInstruction.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryline = entryHeader.MergedLines.AddNew();
		var entryline2 = entryHeader.MergedLines.AddNew();
		invoiceline.JI_CL = entryline.PK;
		invoiceline2.JI_CL = entryline2.PK;
		invoiceline.JI_Weight = 700m;
		invoiceline2.JI_Weight = 500m;
		entryline.CL_LineNumber = 1;
		entryline2.CL_LineNumber = 2;
		entryline.InvoiceLines.Add(invoiceline);
		entryline2.InvoiceLines.Add(invoiceline2);

		wrapper = new ConsignmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));

		AssertEquals(1200m, wrapper.TotalGrossMassMeasure);
	}

	public void TestArrivalTransportMeans()
	{
		AssertNotNull(wrapper.ArrivalTransportMeans);
		AssertType<ArrivalTransportMeansWrapper>(wrapper.ArrivalTransportMeans);
	}

	public void TestBorderTransportMeans()
	{
		AssertNotNull(wrapper.BorderTransportMeans);
		AssertType<BorderTransportMeansWrapper>(wrapper.BorderTransportMeans);
	}

	public void TestGoodsLocation()
	{
		AssertNotNull(wrapper.GoodsLocation);
		AssertType<GoodsLocationWrapper>(wrapper.GoodsLocation);
	}

	public void TestTransportContractDocuments()
	{
		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.TransportContractDocuments.FirstOrDefault());
			AssertType<AdditionalReferenceWrapper>(wrapper.TransportContractDocuments.FirstOrDefault());
			AssertEquals("No TransportContractDocuments", 2, wrapper.TransportContractDocuments.Count);
			AssertEquals("ID of 2nd TransportContractDocuments", "TRA-456", wrapper.TransportContractDocuments.ElementAt(1).Id);
		});
	}

	public void TestTransportEquipments()
	{
		var container = declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = "MSCU0051257";

		var container2 = declaration.CusContainers.AddNew();
		container2.CO_ContainerNumber = "APLU8521458";

		CombineAssertions(() =>
		{
			AssertNotNull(wrapper.TransportEquipments);
			AssertType<TransportEquipmentWrapper>(wrapper.TransportEquipments.FirstOrDefault());
			AssertEquals("No TransportEquipments", 2, wrapper.TransportEquipments.Count);
			AssertEquals("ID of 2nd TransportEquipments", "MSCU0051257", wrapper.TransportEquipments.ElementAt(1).Id);
		});
	}

	public void TestUCR()
	{
		entryHeader.RandomHeader.JZ_UCR = "Trader reference";
		AssertEquals("Trader reference", wrapper.UCR);
	}

	public void TestCarrier()
	{
		var orgHeaderExporter = WrapperTestHelper.CreateOrgHeader(Factory, "Exporter Full Name", "EXPORTER", "654321");
		WrapperTestHelper.CreateAddress(orgHeaderExporter, "EXP", "Havenweg 3", "Rotterdam", "1079CK");
		declaration.CarrierEUBorderDocAddress.OrganisationPK = orgHeaderExporter.PK;

		var carrier = wrapper.Carrier;
		var carrierAddress = carrier.Address;
		AssertNotNull(carrier);
		AssertType<PartyWrapper>(carrier);
		AssertNull("Carrier Name", carrier.Name);
		AssertEquals("Carrier Eori", "NL654321", carrier.Id);
		AssertNull("Carrier Address Type", carrierAddress);
	}

	public void TestConsignee() => CombineAssertions(() =>
	{
		var orgHeaderImporter = WrapperTestHelper.CreateOrgHeader(Factory, "Importer Full Name", "IMPORTER", "22334455");
		WrapperTestHelper.CreateAddress(orgHeaderImporter, "CST", "Importeursweg 37", "IJburg", "2222AB");
		declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeaderImporter.PK;

		var consignee = wrapper.Consignee;
		var consigneeAddress = consignee.Address;
		AssertNotNull(consignee);
		AssertType<PartyWrapper>(consignee);
		AssertNull("Consignee Name", consignee.Name);
		AssertEquals("Consignee Eori", "NL22334455", consignee.Id);
		AssertNull("Consignee Address Type", consigneeAddress);

		declaration.JE_MessageType = "EXP";
		var messageSendingObject = new JobDeclarationMessageSendingObject(entryHeader);
		messageSendingObject.MessageType = "DEC";
		var addInfo = invoiceline.AdditionalInfos.AddNew();
		addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		addInfo.CSI_Code = EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._30600;
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertNull("30600 addinfo present on invoiceLine", wrapper.Consignee);

		invoiceline.AdditionalInfos.RemoveAndDeleteAll();
		addInfo = invoiceline.EntryInstruction.AdditionalInfos.AddNew();
		addInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
		addInfo.CSI_Code = EU.Business.UniversalReferenceConstants.AdditionalDocumentTypes._30600;
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertNull("30600 addInfo present on entryInstruction", wrapper.Consignee);

		invoiceline.EntryInstruction.AdditionalInfos.RemoveAndDeleteAll();
		invoiceline.JI_OA_ConsigneeAddress_ZAddress.OrgPK = entryHeader.Declaration.ImporterDocumentaryAddress.OrganisationPK;
		invoiceline.JI_OA_ConsigneeAddress_ZAddress.AddressFK = entryHeader.Declaration.ImporterDocumentaryAddress.E2_OA_Address;
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertNull("consignee at header level will be empty", wrapper.Consignee);

		var differentOrgHeader = WrapperTestHelper.CreateOrgHeader(Factory, "diff", "SELLER2", "654321");
		var differentOrgAddress = WrapperTestHelper.CreateAddress(differentOrgHeader, "SEL", "diff", "Deventer", "1000MG");
		invoiceline.JI_OA_ConsigneeAddress_ZAddress.OrgPK = differentOrgHeader.PK;
		invoiceline.JI_OA_ConsigneeAddress_ZAddress.AddressFK = differentOrgAddress.PK;
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertNull("consignee at header level will be empty", wrapper.Consignee);
	});

	public void TestConsignor()
	{
		var org = Factory.New<OrgHeader>();
		org.OH_Code = "ORG";
		org.OH_FullName = "DUMMY COMP";
		org.MainAddress.OA_Address1 = "Address 1";
		org.MainAddress.OA_City = "Deventer";
		org.MainAddress.OA_PostCode = "7201MG";
		declaration = entryHeader.Declaration;
		declaration.SupplierDocumentaryAddress.OrganisationPK = org.PK;

		var consignor = wrapper.Consignor;
		var consignorAddress = consignor.Address;
		AssertNotNull(consignor);
		AssertType<PartyWrapper>(consignor);
		AssertEquals("Consignor Name", "DUMMY COMP", consignor.Name);
		AssertType<AddressWrapper>("Consignor Address Type", consignorAddress);
		AssertEquals("Consignor Address city", "Deventer", consignorAddress.CityName);
		AssertEquals("Consignor Address country", "NL", consignorAddress.CountryCode);
		AssertEquals("Consignor Address line", "Address 1", consignorAddress.Line);
		AssertEquals("Consignor Address postcode", "7201MG", consignorAddress.PostcodeId);

		declaration.JE_MessageType = "EXP";
		var messageSendingObject = new JobDeclarationMessageSendingObject(entryHeader);
		messageSendingObject.MessageType = "DEC";

		invoiceline.JI_OA_ExporterAddress_ZAddress.OrgPK = entryHeader.Declaration.SupplierDocumentaryAddress.OrganisationPK;
		invoiceline.JI_OA_ExporterAddress_ZAddress.AddressFK = entryHeader.Declaration.SupplierDocumentaryAddress.E2_OA_Address;
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertNull("consignor at header level will be empty", wrapper.Consignor);

		var differentOrgHeader = WrapperTestHelper.CreateOrgHeader(Factory, "diff", "SELLER2", "654321");
		var differentOrgAddress = WrapperTestHelper.CreateAddress(differentOrgHeader, "SEL", "diff", "Deventer", "1000MG");
		invoiceline.JI_OA_ExporterAddress_ZAddress.OrgPK = differentOrgHeader.PK;
		invoiceline.JI_OA_ExporterAddress_ZAddress.AddressFK = differentOrgAddress.PK;
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertNull("consignor at header level will be empty", wrapper.Consignor);
	}

	public void TestDepartureTransportMeans()
	{
		declaration.JE_MessageType = "EXP";
		var messageSendingObject = new JobDeclarationMessageSendingObject(entryHeader);
		messageSendingObject.MessageType = "DEC";

		declaration.JE_TransportIDInland = "123";
		declaration.JE_TransportMeans = "10";
		declaration.JE_RN_NKTransportNationalityInland = "NL";
		declaration.JE_TransportModeInland = "ROA";
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertEquals("Default transport means count", 1, wrapper.DepartureTransportMeans.Count);
		AssertEquals("Default transport means ID", "123", wrapper.DepartureTransportMeans.First().Id);

		declaration.JE_Trailer1RegNo = "TRAILER1";
		declaration.JE_RN_NKTrailer1Nationality = "DE";
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertEquals("Road transport with trailer 1 count", 2, wrapper.DepartureTransportMeans.Count);
		AssertEquals("Trailer 1 ID", "TRAILER1", wrapper.DepartureTransportMeans.ElementAt(1).Id);

		declaration.JE_Trailer2RegNo = "TRAILER2";
		declaration.JE_RN_NKTrailer2Nationality = "FR";
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertEquals("Road transport with trailer 2 count", 3, wrapper.DepartureTransportMeans.Count);
		AssertEquals("Trailer 2 ID", "TRAILER2", wrapper.DepartureTransportMeans.ElementAt(2).Id);

		declaration.JE_TransportIDInland = string.Empty;
		declaration.JE_TransportMeans = string.Empty;
		declaration.JE_RN_NKTransportNationalityInland = string.Empty;
		declaration.JE_TransportModeInland = string.Empty;
		declaration.JE_Trailer1RegNo = string.Empty;
		declaration.JE_Trailer2RegNo = string.Empty;
		wrapper = new ConsignmentWrapper(entryHeader, messageSendingObject);
		AssertEquals("No transport means count", 0, wrapper.DepartureTransportMeans.Count);
	}

	public void TestFreight()
	{
		AssertNotNull(wrapper.Freight);
		AssertType<FreightWrapper>(wrapper.Freight);
	}

	public void TestItineraries() => CombineAssertions(() =>
	{
		AssertEquals("Length of Itineraries before inserting", 0, wrapper.Itineraries.Count);
		declaration.ItineraryCountries.AddNew();
		declaration.ItineraryCountries.AddNew();
		wrapper = new ConsignmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
		AssertEquals("Length of Itineraries", 2, wrapper.Itineraries.Count);
	});

	protected override ConsignmentWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		var invoice = declaration.Invoices.AddNew();
		invoiceline = invoice.InvoiceLines.AddNew();
		var additionalInfo = invoice.AdditionalInfos.AddNew();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();

		invoiceline.JI_CEI = entryInstruction.PK;
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		var entryline = entryHeader.MergedLines.AddNew();
		invoiceline.JI_CL = entryline.PK;
		entryline.CL_LineNumber = 1;
		entryline.InvoiceLines.Add(invoiceline);

		WrapperTestHelper.CreateAddInfo(entryInstruction.AdditionalInfos, "TRA", "TRA-123", "TRA");
		WrapperTestHelper.CreateAddInfo(entryInstruction.AdditionalInfos, "TRA", "TRA-456", "TR2");

		wrapper = new ConsignmentWrapper(entryHeader, new JobDeclarationMessageSendingObject(entryHeader));
	}

	CusEntryHeader entryHeader;
	ConsignmentWrapper wrapper;
	JobDeclaration declaration;
	JobComInvoiceLine invoiceline;
}
