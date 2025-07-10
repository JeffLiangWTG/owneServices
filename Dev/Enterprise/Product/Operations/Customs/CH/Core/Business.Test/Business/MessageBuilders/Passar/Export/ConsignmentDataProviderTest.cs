using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(ConsignmentDataProvider))]
sealed class ConsignmentDataProviderTest : BasePassarDataProviderTest<ConsignmentDataProvider>
{
	public void TestConstructorNullArgument() => AssertNull(ConsignmentDataProvider.New(null));

	public void TestProvider() => CombineAssertions(() =>
	{
		Declaration.JE_GoodsDestination = "DE";
		Declaration.JE_UCR = "UCR123";
		AssertEquals("CountryOfDestination", "DE", DataProvider.CountryOfDestination);
		AssertEquals("PartialDelivery", false, DataProvider.PartialDelivery);
		AssertEquals("Preference", false, DataProvider.Preference);
		AssertEquals("ReferenceNumberUCR", "UCR123", DataProvider.ReferenceNumberUCR);
	});

	public void TestUnusedProperties() => CombineAssertions(() =>
	{
		AssertEquals("CountryOfDispatch", null, DataProvider.CountryOfDispatch);
		AssertEquals("Consignee", null, DataProvider.Consignee);
		AssertEquals("PlaceOfLoading", null, DataProvider.PlaceOfLoading);
		AssertEquals("PlaceOfUnloading", null, DataProvider.PlaceOfUnloading);
		AssertEquals("ActiveBorderTransportMeans", null, DataProvider.ActiveBorderTransportMeans);
		AssertEquals("CountryOfRoutingOfConsignment", null, DataProvider.CountryOfRoutingOfConsignments);
		AssertEquals("HouseConsignments", null, DataProvider.CountryOfDispatch);
		AssertEquals("AdditionalReferences", null, DataProvider.AdditionalReferences);
		AssertEquals("ConsignmentItems", null, DataProvider.ConsignmentItems);
		AssertEquals("Importer", null, DataProvider.Importer);
		AssertEquals("PartialShipment", null, DataProvider.PartialShipment);
	});

	public void TestTransportEquipment() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceLine;
		var invoiceLine2 = CreateInvoiceLine(InvoiceHeader);

		var unlinkedContainer = Declaration.CusContainers.AddNew();
		unlinkedContainer.CO_ContainerNumber = "CNT100";

		AssertEquals("PRE-CONDITION", 0, invoiceLine1.ContainersPivot.Count);
		AssertEquals("PRE-CONDITION", 0, invoiceLine2.ContainersPivot.Count);

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertNull("TransportEquipments when declaration is Simplified and has no linked containers", DataProvider.TransportEquipments);
		ResetDataProvider();

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("TransportEquipments when declaration is Ordinary and has no linked containers", 0, DataProvider.TransportEquipments.Count);
		AssertEquals("TransportEquipments cached when declaration is Ordinary and has no linked containers", DataProvider.TransportEquipments, DataProvider.TransportEquipments);
		ResetDataProvider();

		AddLinkedContainer(invoiceLine1, "CNT200");
		AddLinkedContainer(invoiceLine2, "CNT300");

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertNull("TransportEquipments when declaration is Simplified and has linked containers", DataProvider.TransportEquipments);
		ResetDataProvider();

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("TransportEquipments when declaration is Ordinary and has linked containers", 2, DataProvider.TransportEquipments.Count);
		Assert("TransportEquipments type when declaration is Ordinary and has linked containers", DataProvider.TransportEquipments.All(te => te is TransportEquipmentDataProvider));
		AssertEquals("TransportEquipments cached when declaration is Ordinary and has linked containers", DataProvider.TransportEquipments, DataProvider.TransportEquipments);
		AssertContainsExactElementsInExactOrder("TransportEquipments value when declaration is Ordinary and has linked containers", new string[] { "CNT200", "CNT300" }, DataProvider.TransportEquipments.Select(c => c.ContainerIdentificationNumber));
	});

	public void TestContainerIndicator() => CombineAssertions(() =>
	{
		var invoiceLine1 = InvoiceLine;
		var invoiceLine2 = CreateInvoiceLine(InvoiceHeader);

		var unlinkedContainer = Declaration.CusContainers.AddNew();
		unlinkedContainer.CO_ContainerNumber = "CNT100";

		AssertEquals("PRE-CONDITION", 0, invoiceLine1.ContainersPivot.Count);
		AssertEquals("PRE-CONDITION", 0, invoiceLine2.ContainersPivot.Count);

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertEquals("ContainerIndicator when declaration is Simplified and has no linked containers", expected: false, DataProvider.ContainerIndicator);
		ResetDataProvider();

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("ContainerIndicator when declaration is Ordinary and has no linked containers", expected: false, DataProvider.ContainerIndicator);
		ResetDataProvider();

		AddLinkedContainer(invoiceLine1, "CNT200");
		AddLinkedContainer(invoiceLine2, "CNT300");

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertEquals("ContainerIndicator when declaration is Simplified and has linked containers", expected: false, DataProvider.ContainerIndicator);
		ResetDataProvider();

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		AssertEquals("ContainerIndicator when declaration is Ordinary and has linked containers", expected: true, DataProvider.ContainerIndicator);
	});

	public void TestConsignee() => CombineAssertions(() =>
	{
		Declaration.JE_OH_Importer = CreaterOrgHeader("Consignee").PK;
		AssertEquals("DataProvider provided", "Consignee", DataProvider.Consignee.Name);
		AssertEquals("Cached", DataProvider.Consignee, DataProvider.Consignee);
		ResetDataProvider();
		Declaration.JE_OH_Importer = ZGuid.Empty;
		AssertNull("Null when empty", DataProvider.Consignee);
	});

	public void TestConsignor() => CombineAssertions(() =>
	{
		var consignorDocAddress = Factory.New<JobDocAddress>();
		consignorDocAddress.E2_AddressType = DocAddressTypes.Codes.ConsignorDocumentaryAddress;
		consignorDocAddress.E2_AddressOverride = true;

		Declaration.ConsignorDocAddress.OrganisationPK = consignorDocAddress.Organisation.PK;

		AssertNotNull(DataProvider.Consignor);
		AssertSame("cached", DataProvider.Consignor, DataProvider.Consignor);

		ResetDataProvider();
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertNull("null when simplified declaration", DataProvider.Consignor);

		ResetDataProvider();
		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Ordinary;
		Declaration.ConsignorDocAddress.OrganisationPK = ZGuid.Empty;
		AssertNull("null when empty", DataProvider.Consignor);
	});

	public void TestExporter() => CombineAssertions(() =>
	{
		Declaration.JE_OH_Supplier = CreaterOrgHeader("Exporter").PK;
		AssertEquals("DataProvider provided", "Exporter", DataProvider.Exporter.Name);
		AssertEquals("Cached", DataProvider.Exporter, DataProvider.Exporter);
		ResetDataProvider();
		Declaration.JE_OH_Supplier = ZGuid.Empty;
		AssertNull("Null when empty", DataProvider.Exporter);
	});

	public void TestHouseConsignments() => CombineAssertions(() =>
	{
		Declaration.CustomsEntryHeaders.AddNew();
		AssertNotNull(DataProvider.HouseConsignments);
		AssertSame("cached", DataProvider.HouseConsignments, DataProvider.HouseConsignments);

		Declaration.CustomsEntryHeaders.AddNew();
		ResetDataProvider();
		AssertEquals("Single House Consignment", 1, DataProvider.HouseConsignments.Count);
	});

	public void TestPreviousDocuments() => CombineAssertions(() =>
	{
		AddDocument(EntryInstruction.PreviousDocuments, "1", "01");
		AddDocument(EntryInstruction.PreviousDocuments, "2", "02");
		AddDocument(EntryInstruction.PreviousDocuments, "3", "03");

		var invoice1 = CreateInvoiceHeader();
		AddDocument(invoice1.PreviousDocuments, "4", "04");
		AddDocument(invoice1.PreviousDocuments, "1", "04");
		AddDocument(invoice1.PreviousDocuments, "4", "01");
		AddDocument(invoice1.PreviousDocuments, "1", "01");

		var invoice2 = CreateInvoiceHeader();
		AddDocument(invoice2.PreviousDocuments, "2", "02");
		AddDocument(invoice2.PreviousDocuments, "5", "05");

		Declaration.DoMergeForTesting();

		var dataProviders = DataProvider.PreviousDocuments;

		AssertNotNull(dataProviders);
		AssertContainsExactElementsInAnyOrder(new[] { "101", "202", "303", "404", "104", "401", "505" }, dataProviders.Select(x => x.Type + x.ReferenceNumber));

		AssertSame("cached", DataProvider.PreviousDocuments, DataProvider.PreviousDocuments);
	});

	public void TestSupportingDocuments() => CombineAssertions(() =>
	{
		AddDocument(EntryInstruction.SupportingDocuments, "1", "01");
		AddDocument(EntryInstruction.SupportingDocuments, "2", "02");
		AddDocument(EntryInstruction.SupportingDocuments, "3", "03");

		var invoice1 = CreateInvoiceHeader();
		AddDocument(invoice1.SupportingDocuments, "4", "04");
		AddDocument(invoice1.SupportingDocuments, "1", "04");
		AddDocument(invoice1.SupportingDocuments, "4", "01");
		AddDocument(invoice1.SupportingDocuments, "1", "01");

		var invoice2 = CreateInvoiceHeader();
		AddDocument(invoice2.SupportingDocuments, "2", "02");
		AddDocument(invoice2.SupportingDocuments, "5", "05");

		Declaration.DoMergeForTesting();

		var dataProviders = DataProvider.SupportingDocuments;

		AssertNotNull(dataProviders);
		AssertContainsExactElementsInAnyOrder(new[] { "101", "202", "303", "404", "104", "401", "505" }, dataProviders.Select(x => x.Type + x.ReferenceNumber));

		AssertSame("cached", DataProvider.SupportingDocuments, DataProvider.SupportingDocuments);
	});

	public void TestPartialDelivery() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_Style = InputControlCodes.Ordinary;
		EntryInstruction.CEI_PartialDelivery = ZBool.True;
		AssertEquals("CEI_Style=2 CEI_PartialDelivery=true", true, DataProvider.PartialDelivery);
		ResetDataProvider();
		EntryInstruction.CEI_PartialDelivery = ZBool.False;
		AssertEquals("CEI_Style=2 CEI_PartialDelivery=false", false, DataProvider.PartialDelivery);

		EntryInstruction.CEI_Style = InputControlCodes.Simplified;
		EntryInstruction.CEI_PartialDelivery = ZBool.True;
		AssertNull("CEI_Style=1 CEI_PartialDelivery=true", DataProvider.PartialDelivery);
		EntryInstruction.CEI_PartialDelivery = ZBool.False;
		AssertNull("CEI_Style=1 CEI_PartialDelivery=false", DataProvider.PartialDelivery);
	});

	public void TestTransportDocuments() => CombineAssertions(() =>
	{
		AddDocument(EntryInstruction.TransportDocuments, "1", "01");
		AddDocument(EntryInstruction.TransportDocuments, "2", "02");
		AddDocument(EntryInstruction.TransportDocuments, "3", "03");

		var invoice1 = CreateInvoiceHeader();
		AddDocument(invoice1.TransportDocuments, "4", "04");
		AddDocument(invoice1.TransportDocuments, "1", "04");
		AddDocument(invoice1.TransportDocuments, "4", "01");
		AddDocument(invoice1.TransportDocuments, "1", "01");

		var invoice2 = CreateInvoiceHeader();
		AddDocument(invoice2.TransportDocuments, "2", "02");
		AddDocument(invoice2.TransportDocuments, "5", "05");

		Declaration.DoMergeForTesting();

		var dataProviders = DataProvider.TransportDocuments;

		AssertNotNull(dataProviders);
		AssertContainsExactElementsInAnyOrder(new[] { "101", "202", "303", "404", "104", "401", "505" }, dataProviders.Select(x => x.Type + x.ReferenceNumber));
	});

	public void TestAdditionalSupplyChainActors() => CombineAssertions(() =>
	{
		EntryInstruction.SupplyChainActors.AddNew();
		EntryInstruction.SupplyChainActors.AddNew();
		AssertEquals("Number of total SupplyChainActors on bizObj should be: ", 2, EntryInstruction.SupplyChainActors.Count);

		var additionalSupplyChainActors = DataProvider.AdditionalSupplyChainActors;
		AssertNotNull("AdditionalSupplyChainActors should be defined", additionalSupplyChainActors);
		AssertEquals("AdditionalSupplyChainActor is IAdditionalSupplyChainActor", true, additionalSupplyChainActors.FirstOrDefault() is IAdditionalSupplyChainActor);
		AssertEquals("Number of total AdditionalSupplyChainActors should be: ", 2, additionalSupplyChainActors.Count);
	});

	public void TestTransportCharges() => CombineAssertions(() =>
	{
		EntryInstruction.CEI_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cash;

		AssertType<TransportChargesDataProvider>(DataProvider.TransportCharges.First());
		AssertSame("cached", DataProvider.TransportCharges, DataProvider.TransportCharges);

		AssertEquals(EntryInstruction.CEI_TransportChargesMethodOfPayment, DataProvider.TransportCharges.First().MethodOfPayment);

		EntryInstruction.CEI_Style = UniversalReferenceConstants.InputControlCodes.Simplified;
		AssertNull("simplified", DataProvider.TransportCharges);
	});

	public void TestPreference()
	{
		AssertPreference(new[] { UniversalReferenceConstants.SupportingDocumentTypeCodes.WVBEUR1, UniversalReferenceConstants.SupportingDocumentTypeCodes.WVBEUR1CN, UniversalReferenceConstants.SupportingDocumentTypeCodes.WVBEURMED, UniversalReferenceConstants.SupportingDocumentTypeCodes.WVBEUR1TransitionalRules }, true);
		AssertPreference(new[] { "1" }, false);
	}

	void AssertPreference(string[] codes, bool expectedValue) => CombineAssertions(() =>
	{
		var entryInstruction = Declaration.CustomsEntryInstructions.AddNew();
		var supportingDocument1 = entryInstruction.SupportingDocuments.AddNew();
		supportingDocument1.CSI_ReferenceNumber = "R1";
		var supportingDocument2 = entryInstruction.SupportingDocuments.AddNew();
		supportingDocument2.CSI_Code = "2";
		supportingDocument2.CSI_ReferenceNumber = "R2";
		Declaration.CustomsEntryHeaders.First().CH_CEI_Instruction = entryInstruction.PK;

		codes.ForEach(code =>
		{
			supportingDocument1.CSI_Code = code;
			var provider = CreateDataProvider();
			AssertEquals($"Preference for code {code} should be: ", expectedValue, provider.Preference);
		});
	});

	public void TestReferenceNumberUCR() => CombineAssertions(() =>
	{
		var invoice1 = InvoiceHeader;
		invoice1.InvoiceLines[0].JI_Tariff = "1";
		var invoice2 = CreateInvoiceHeader();
		invoice2.InvoiceLines[0].JI_Tariff = "2";

		Declaration.DoMergeForTesting();
		AssertEquals("Pre-check: # of entry lines", 2, EntryHeader.AllEntryLines.Count);

		AssertReferenceNumberUCR(null, ZString.Empty, ZString.Empty, ZString.Empty);
		AssertReferenceNumberUCR("UCR1", "UCR1", ZString.Empty, ZString.Empty);
		AssertReferenceNumberUCR(null, "UCR1", "UCR2", ZString.Empty);
		AssertReferenceNumberUCR(null, ZString.Empty, "UCR2", ZString.Empty);
		AssertReferenceNumberUCR(null, ZString.Empty, "UCR2", "UCR3");

		Declaration.JE_UCR = "UCR1";
		invoice1.JZ_UCR = ZString.Empty;
		invoice2.JZ_UCR = ZString.Empty;
		AssertEquals("Cached (value not recaclulated)", null, DataProvider.ReferenceNumberUCR);

		void AssertReferenceNumberUCR(string expectedUCR, ZString declarationUCR, ZString invoice1UCR, ZString invoice2UCR, [CallerLineNumber] int callerLineNumber = 0)
		{
			Declaration.JE_UCR = declarationUCR;
			invoice1.JZ_UCR = invoice1UCR;
			invoice2.JZ_UCR = invoice2UCR;
			ResetDataProvider();
			AssertEquals($"[{callerLineNumber}] DeclarationUCR={declarationUCR} Invoice1UCR={invoice1UCR} Invoice2UCR={invoice2UCR}", expectedUCR, DataProvider.ReferenceNumberUCR);
		}
	});

	public void TestAdditionalInformations() => CombineAssertions(() =>
	{
		EntryInstruction.AdditionalInformations.AddNew();

		AssertNotNull(DataProvider.AdditionalInformations);
		AssertSame("cached", DataProvider.AdditionalInformations, DataProvider.AdditionalInformations);
		AssertEquals("Count", 1, DataProvider.AdditionalInformations.Count);
		AssertEquals("SequenceNumber first element", 1, DataProvider.AdditionalInformations.ElementAt(0).SequenceNumber);
	});

	void AddLinkedContainer(JobComInvoiceLine invoiceLine, string containerNumber)
	{
		var container = Declaration.CusContainers.AddNew();
		container.CO_ContainerNumber = containerNumber;
		var pivot = invoiceLine.ContainersPivot.AddNew();
		pivot.C2_CO = container.PK;
	}

	OrgHeader CreaterOrgHeader(ZString name)
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = name;
		return orgHeader;
	}

	protected override ConsignmentDataProvider CreateDataProvider() => ConsignmentDataProvider.New(EntryHeader);
}
