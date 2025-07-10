using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSGoodsShipmentWrapperTest : WrapperHelperTest<T2LPOUSGoodsShipmentWrapper>
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown("Constructor Throws Exception if cusEntryHeader is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","cusEntryHeader"), () => GetWrapper(null));

			var entryHeader = Factory.New<CusEntryHeader>();
			AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => GetWrapper(entryHeader));
		});
	}

	public void TestContainerIndication()
	{
		CombineAssertions(() =>
		{
			var containerIndication = wrapper.ContainerIndication;
			AssertNotNull("Expected filled ContainerIndication", containerIndication);
			AssertSame("Cached ContainerIndication", wrapper.ContainerIndication, containerIndication);
			AssertEquals("Expected false ContainerIndication.IsContainerised", false, containerIndication.IsContainerised);

			var containerTag = "CONTAINER";
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = containerTag;
			invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected true IsContainerised", true, wrapper.ContainerIndication.IsContainerised);
		});
	}

	public void TestTransportEquipment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TransportEquipment", 0, wrapper.TransportEquipment.Count);

			declaration.JE_ContainerMode = "ULD";
			var package1 = declaration.Packages.AddNew();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			invoiceLine.PackagesPivot.AddPivotFor(package1);
			wrapper = GetWrapper(entryHeader);
			var transportEquipment = wrapper.TransportEquipment;
			AssertEquals("Expected filled TransportEquipment", 1, transportEquipment.Count);
			AssertSame("Cached TransportEquipment", wrapper.TransportEquipment, transportEquipment);
		});
	}

	public void TestAdditionalInformation()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Expected empty AdditionalInformation list", 0, wrapper.AdditionalInformation.Count);
			AddAdditionalInfos(declaration.AdditionalInfos, ("9001", "TRA"), ("Y001", "INF"));
			AddAdditionalInfos(entryInstruction.AdditionalInfos, ("9002", "TRA"), ("Y002", "INF"));
			AddAdditionalInfos(invoiceHeader.AdditionalInfos, ("9003", "TRA"), ("Y003", "INF"));
			AddAdditionalInfos(invoiceLine.AdditionalInfos, ("9004", "TRA"));

			wrapper = GetWrapper(entryHeader);
			var documents = wrapper.AdditionalInformation;
			AssertEquals("Expected filled AdditionalInformation (only included those that that have subType INF and are in entryInstruction)", 1, documents.Count);
			AssertSame("Cached AdditionalInformation", wrapper.AdditionalInformation, documents);
		});
	}

	public void TestPreviousDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Expected empty PreviousDocument list", 0, wrapper.PreviousDocument.Count);
			AddPreviousDocuments(declaration.PreviousDocuments, "9001");
			AddPreviousDocuments(invoiceHeader.PreviousDocuments, "9002");
			AddPreviousDocuments(invoiceLine.PreviousDocuments, "9003");

			wrapper = GetWrapper(entryHeader);
			var documents = wrapper.PreviousDocument;
			AssertEquals("Expected filled PreviousDocument (only from declaration)", 1, documents.Count);
			AssertSame("Cached PreviousDocument", wrapper.PreviousDocument, documents);
		});
	}

	public void TestSupportingDocument()
	{
		IReadOnlyCollection<IDocumentsCommon> supportingDocument;

		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Expected empty SupportingDocument list", 0, wrapper.SupportingDocument.Count);
			AddSupportingDocuments(declaration.SupportingDocuments, "Y001", "9001");
			AddSupportingDocuments(entryInstruction.SupportingDocuments, "Y002", "9002");
			AddSupportingDocuments(invoiceHeader.SupportingDocuments, "Y003", "9003");
			AddSupportingDocuments(invoiceLine.SupportingDocuments, "Y004", "9004", "9005");

			var documents = GetWrapperSupportingDocumentValues();

			AssertArrayEqualsByElements("SupportingDocument expected to include Supporting Documents with code prefix non-Y from Declaration (Misc), Entry Instruction, and Invoice Header (only 1 invoice)", ["9001", "9002", "9003"], documents);

			AddInvoiceNotIncludedIntoEntry();

			documents = GetWrapperSupportingDocumentValues();

			AssertArrayEqualsByElements("SupportingDocument expected to include Supporting Documents with code prefix non-Y from Declaration (Misc), Entry Instruction, and Invoice Header (2 invoices, but only 1 invoice in the entry)", ["9001", "9002", "9003"], documents);

			AddInvoiceAndMerge();
			AddSupportingDocuments(invoice2Header.SupportingDocuments, "Y021", "9021");
			AddSupportingDocuments(invoice2Line.SupportingDocuments, "Y022", "9022", "9023");

			documents = GetWrapperSupportingDocumentValues();

			AssertArrayEqualsByElements("SupportingDocument expected to include Supporting Documents with code prefix non-Y from Declaration (Misc), and Entry Instruction. Invoice Header items should be excluded (2 invoices in the entry)", ["9001", "9002"], documents);

			AssertSame("Cached SupportingDocument", supportingDocument, wrapper.SupportingDocument);

			string[] GetWrapperSupportingDocumentValues()
			{
				wrapper = GetWrapper(entryHeader);
				supportingDocument = wrapper.SupportingDocument;
				return supportingDocument.Select(x => (string)x.Name).OrderBy(x => x).ToArray();
			}
		});
	}

	public void TestTransportDocument()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Expected empty TransportDocument list", 0, wrapper.TransportDocument.Count);
			AddAdditionalInfos(declaration.AdditionalInfos, ("9001", "TRA"), ("Y001", "INF"));
			AddAdditionalInfos(entryInstruction.AdditionalInfos, ("9002", "TRA"), ("Y002", "INF"));
			AddAdditionalInfos(invoiceHeader.AdditionalInfos, ("9003", "TRA"), ("Y003", "INF"));
			AddAdditionalInfos(invoiceLine.AdditionalInfos, ("9004", "TRA"));

			wrapper = GetWrapper(entryHeader);
			var documents = wrapper.TransportDocument;
			AssertEquals("Expected filled TransportDocument (only included those that that have subType TRA and are in declaration and entryInstruction)", 2, documents.Count);
			AssertSame("Cached TransportDocument", wrapper.TransportDocument, documents);
		});
	}

	public void TestAdditionalReference()
	{
		IReadOnlyCollection<IDocumentsCommon> additionalReference;

		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);
			AddSupportingDocuments(declaration.SupportingDocuments, "Y001", "9001");
			AddSupportingDocuments(entryInstruction.SupportingDocuments, "Y002", "9002");
			AddSupportingDocuments(invoiceHeader.SupportingDocuments, "Y003", "Y003", "9003");
			AddSupportingDocuments(invoiceLine.SupportingDocuments, "Y004", "Y004", "9004", "9005");

			var documents = GetWrapperAdditionalReferenceValues();

			AssertArrayEqualsByElements("AdditionalReference expected to include Supporting Documents with code prefix Y from Declaration (Misc), Entry Instruction, and Invoice Header (only 1 invoice)", ["Y001", "Y002", "Y003"], documents);

			AddInvoiceNotIncludedIntoEntry();

			documents = GetWrapperAdditionalReferenceValues();

			AssertArrayEqualsByElements("AdditionalReference expected to include Supporting Documents with code prefix Y from Declaration (Misc), Entry Instruction, and Invoice Header (2 invoices, but only 1 invoice in the entry)", ["Y001", "Y002", "Y003"], documents);

			AddInvoiceAndMerge();
			AddSupportingDocuments(invoice2Header.SupportingDocuments, "Y021", "9021");
			AddSupportingDocuments(invoice2Line.SupportingDocuments, "Y022", "9022", "9023");

			documents = GetWrapperAdditionalReferenceValues();

			AssertArrayEqualsByElements("AdditionalReference expected to include Supporting Documents with code prefix Y from Declaration (Misc), and Entry Instruction. Invoice Header items should be excluded (2 invoices in the entry)", ["Y001", "Y002"], documents);

			AssertSame("Cached AdditionalReference", additionalReference, wrapper.AdditionalReference);

			string[] GetWrapperAdditionalReferenceValues()
			{
				wrapper = GetWrapper(entryHeader);
				additionalReference = wrapper.AdditionalReference;
				return additionalReference.Select(x => (string)x.Name).OrderBy(x => x).ToArray();
			}
		});
	}

	public void TestAdditionalReference_ContainingREFDocuments()
	{
		IReadOnlyCollection<IDocumentsCommon> additionalReference;

		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);
			AddAdditionalInfos(declaration.AdditionalInfos, ("declaration", "REF"));
			AddAdditionalInfos(entryInstruction.AdditionalInfos, ("entryInstruction", "REF"));
			AddAdditionalInfos(declaration.AdditionalInfos, ("declaration", "INF"));
			AddAdditionalInfos(entryInstruction.AdditionalInfos, ("entryInstruction", "TRA"));
			AddAdditionalInfos(invoiceHeader.AdditionalInfos, ("invoice", "REF"));
			AddAdditionalInfos(invoiceLine.AdditionalInfos, ("invoiceLine", "REF"));

			var documents = GetWrapperAdditionalReferenceValues();

			AssertArrayEqualsByElements("AdditionalReference expected to include Additional documents with SubType = 'REF' from Declaration (Misc), Entry Instruction.", ["declaration", "entryInstruction"], documents);

			string[] GetWrapperAdditionalReferenceValues()
			{
				wrapper = GetWrapper(entryHeader);
				additionalReference = wrapper.AdditionalReference;
				return additionalReference.Select(x => (string)x.Name).OrderBy(x => x).ToArray();
			}
		});
	}

	public void TestAdditionalReference_ContainingBothSupDocsAndREFDocs()
	{
		IReadOnlyCollection<IDocumentsCommon> additionalReference;

		CombineAssertions(() =>
		{
			AssertEquals("Pre-requisite: Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);
			AddSupportingDocuments(declaration.SupportingDocuments, "Y001", "9001");
			AddSupportingDocuments(entryInstruction.SupportingDocuments, "Y002", "9002");
			AddSupportingDocuments(invoiceHeader.SupportingDocuments, "Y003", "Y003", "9003");
			AddSupportingDocuments(invoiceLine.SupportingDocuments, "Y004", "Y004", "9004", "9005");

			AddAdditionalInfos(declaration.AdditionalInfos, ("declaration", "REF"), ("declaration2", "INF"));
			AddAdditionalInfos(entryInstruction.AdditionalInfos, ("entryInstruction", "REF"), ("entryInstruction2", "TRA"));
			AddAdditionalInfos(invoiceHeader.AdditionalInfos, ("invoice", "REF"));
			AddAdditionalInfos(invoiceLine.AdditionalInfos, ("invoiceLine", "REF"));

			var documents = GetWrapperAdditionalReferenceValues();

			AssertArrayEqualsByElements("AdditionalReference expected to include 1) Supporting Documents with code prefix Y from Declaration (Misc), Entry Instruction, and Invoice Header (only 1 invoice) and 2) AdditionalInfos with CSI_SubType = 'REF' from Declaration (Misc), Entry Instruction.", ["declaration", "entryInstruction", "Y001", "Y002", "Y003"], documents);

			string[] GetWrapperAdditionalReferenceValues()
			{
				wrapper = GetWrapper(entryHeader);
				additionalReference = wrapper.AdditionalReference;
				return additionalReference.Select(x => (string)x.Name).OrderBy(x => x).ToArray();
			}
		});
	}

	public void TestGoodItems()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected 1 GoodItem (mandatory at least one)", 1, wrapper.GoodItems.Count);

			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2203001011";

			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = "2203001012";

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryHeader = declaration.CustomsEntryHeaders[0];
			wrapper = GetWrapper(entryHeader);

			var goodItems = wrapper.GoodItems;

			AssertEquals("Expected 3 GoodItems", 3, goodItems.Count);
			AssertSame("Cached GoodItems", wrapper.GoodItems, goodItems);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;

		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "11";

		invoice2Header = null;
		invoice2Line = null;

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader);
	}

	void AddInvoiceNotIncludedIntoEntry()
	{
		var newInvoiceHeader = declaration.Invoices.AddNew();
		newInvoiceHeader.InvoiceLines.AddNew();
	}

	void AddInvoiceAndMerge()
	{
		invoice2Header = declaration.Invoices.AddNew();
		invoice2Line = invoice2Header.InvoiceLines.AddNew();
		invoice2Line.JI_Tariff = "11";

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice2Header;
	JobComInvoiceLine invoice2Line;
	CusEntryHeader entryHeader;
	T2LPOUSGoodsShipmentWrapper wrapper;

	T2LPOUSGoodsShipmentWrapper GetWrapper(CusEntryHeader entryHeader) => new T2LPOUSGoodsShipmentWrapper(entryHeader);

	protected override T2LPOUSGoodsShipmentWrapper GetProvider() => wrapper;
}
