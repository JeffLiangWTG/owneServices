using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSRequestAndReceptionGoodItemWrapperTest : WrapperHelperTest<T2LPOUSRequestAndReceptionGoodItemWrapper>
{
	public void TestCommodityCode()
	{
		var commodityCode = wrapper.CommodityCode;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled CommodityCode", commodityCode);
			AssertSame("Cached CommodityCode", wrapper.CommodityCode, commodityCode);
		});
	}

	public void TestDescription()
	{
		invoiceLine.JI_Description = "description";
		AssertEquals("Expected filled Description", "description", wrapper.Description);
	}

	public void TestCusCode()
	{
		invoiceLine.ZG_CusNumber = "0111001-6";
		AssertEquals("Expected filled CusCode", "0111001-6", wrapper.CusCode);
	}

	public void TestGoodsMeasure()
	{
		var goodsMeasure = wrapper.GoodsMeasure;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled GoodsMeasure", goodsMeasure);
			AssertSame("Cached GoodsMeasure", wrapper.GoodsMeasure, goodsMeasure);
		});
	}

	public void TestPackage()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Package list", 0, wrapper.Package.Count);

			var pack1 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo1 = Factory.New<Customs.Business.BasePackage>();
			packageInfo1.CW_PackType = InternalPackage1.Type;
			pack1.CHC_CW = packageInfo1.PK;
			invoiceLine.PackagesPivot.Add(pack1);

			var pack2 = Factory.New<Customs.Business.InvoiceLinePackagePivot>();
			var packageInfo2 = Factory.New<Customs.Business.BasePackage>();
			packageInfo2.CW_PackType = InternalPackage2.Type;
			pack2.CHC_CW = packageInfo2.PK;
			invoiceLine.PackagesPivot.Add(pack2);

			wrapper = GetWrapper(entryLine);
			var package = wrapper.Package;

			AssertEquals("Expected filled Package", 2, package.Count);
			AssertSame("Cached Package", wrapper.Package, package);
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
			AddAdditionalInfos(invoiceLine.AdditionalInfos, ("9004", "TRA"), ("9005", "INF"), ("Y004", "INF"));

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.AdditionalInformation;
			AssertEquals("Expected filled AdditionalInformation (only included those that have subType INF and are in invoiceHeader and invoiceLines)", 3, documents.Count);
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
			AddPreviousDocuments(invoiceLine.PreviousDocuments, "9003", "9004");

			wrapper = GetWrapper(entryLine);
			var documents = wrapper.PreviousDocument;
			AssertEquals("Expected filled PreviousDocument (only included those that are in invoiceHeader and invoiceLines)", 3, documents.Count);
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
			AddSupportingDocuments(invoiceHeader.SupportingDocuments, "Y003", "9003", "9003");
			AddSupportingDocuments(invoiceLine.SupportingDocuments, "Y004", "9004", "9005", "9005");

			var documents = GetWrapperSupportingDocumentValues();
			AssertArrayEqualsByElements("SupportingDocument expected to include Supporting Documents with code prefix non-Y from Invoice Lines only. Invoice Header items should be excluded (only 1 invoice)", ["9004", "9005"], documents);

			AddInvoiceNotIncludedIntoEntry();
			documents = GetWrapperSupportingDocumentValues();
			AssertArrayEqualsByElements("SupportingDocument expected to include Supporting Documents with code prefix non-Y from Invoice Lines only. Invoice Header items should be excluded (2 invoices, but only 1 invoice in the entry)", ["9004", "9005"], documents);

			invoiceHeader.SupportingDocuments.RemoveAndDeleteAll();
			invoiceLine.SupportingDocuments.RemoveAndDeleteAll();

			AddInvoiceAndMerge();
			AddSupportingDocuments(invoiceHeader.SupportingDocuments, "Y003", "9003", "9003");
			AddSupportingDocuments(invoiceLine.SupportingDocuments, "Y004", "9004", "9005", "9005");
			AddSupportingDocuments(invoice2Header.SupportingDocuments, "Y021", "9021");
			AddSupportingDocuments(invoice2Line.SupportingDocuments, "Y022", "9022", "9023");
			documents = GetWrapperSupportingDocumentValues();
			AssertArrayEqualsByElements("SupportingDocument expected to include Supporting Documents with code prefix non-Y from Invoice Lines and Invoice Header (2 invoices in the entry)", ["9003", "9004", "9005", "9021", "9022", "9023"], documents);

			AssertSame("Cached SupportingDocument", supportingDocument, wrapper.SupportingDocument);

			string[] GetWrapperSupportingDocumentValues()
			{
				wrapper = GetWrapper(entryLine);
				supportingDocument = wrapper.SupportingDocument;
				return supportingDocument.Select(x => (string)x.Name).OrderBy(x => x).ToArray();
			}
		});
	}

	public void TestAdditionalReference_ContainingSupportingDocuments()
	{
		IReadOnlyCollection<IDocumentsCommon> additionalReference;

		CombineAssertions("For AdditionalReference that contain Supporting documents starting with Y.", () =>
		{
			AssertEquals("Pre-requisite: Expected empty AdditionalReference list", 0, wrapper.AdditionalReference.Count);
			AddSupportingDocuments(declaration.SupportingDocuments, "Y001", "9001");
			AddSupportingDocuments(entryInstruction.SupportingDocuments, "Y002", "9002");
			AddSupportingDocuments(invoiceHeader.SupportingDocuments, "Y003", "Y003", "9003");
			AddSupportingDocuments(invoiceLine.SupportingDocuments, "Y004", "Y004", "9004", "9005");

			var documents = GetWrapperAdditionalReferenceValues();
			AssertArrayEqualsByElements("AdditionalReference expected to include Supporting Documents with code prefix Y from Invoice Lines only. Invoice Header items should be excluded (only 1 invoice)", ["Y004"], documents);

			AddInvoiceNotIncludedIntoEntry();
			documents = GetWrapperAdditionalReferenceValues();
			AssertArrayEqualsByElements("AdditionalReference expected to include Supporting Documents with code prefix Y from Invoice Lines only. Invoice Header items should be excluded (2 invoices, but only 1 invoice in the entry)", ["Y004"], documents);

			invoiceHeader.SupportingDocuments.RemoveAndDeleteAll();
			invoiceLine.SupportingDocuments.RemoveAndDeleteAll();

			AddInvoiceAndMerge();
			AddSupportingDocuments(invoiceHeader.SupportingDocuments, "Y003", "Y003", "9003");
			AddSupportingDocuments(invoiceLine.SupportingDocuments, "Y004", "Y004", "9004", "9005");
			AddSupportingDocuments(invoice2Header.SupportingDocuments, "Y021", "9021");
			AddSupportingDocuments(invoice2Line.SupportingDocuments, "Y022", "Y022", "9022", "9023");

			documents = GetWrapperAdditionalReferenceValues();
			AssertArrayEqualsByElements("AdditionalReference expected to include Supporting Documents with code prefix Y from Invoice Lines and Invoice Header (2 invoices in the entry)", ["Y003", "Y004", "Y021", "Y022"], documents);

			AssertSame("Cached AdditionalReference", additionalReference, wrapper.AdditionalReference);

			string[] GetWrapperAdditionalReferenceValues()
			{
				wrapper = GetWrapper(entryLine);
				additionalReference = wrapper.AdditionalReference;
				return additionalReference.Select(x => (string)x.Name).OrderBy(x => x).ToArray();
			}
		});
	}

	public void TestAdditionalReferences_ContainingREFDocuments()
	{
		IReadOnlyCollection<IDocumentsCommon> additionalReference;

		AddAdditionalInfos(declaration.AdditionalInfos, ("DEC", "REF"));
		AddAdditionalInfos(entryInstruction.AdditionalInfos, ("ENT", "REF"));
		AddAdditionalInfos(invoiceHeader.AdditionalInfos, ("HEA", "REF"), ("HEA2", "INF"));
		AddAdditionalInfos(invoiceLine.AdditionalInfos, ("LIN", "REF"), ("LIN2", "TRA"));

		var documents = GetWrapperAdditionalReferenceValues();

		AssertArrayEqualsByElements("AdditionalReference expected to include AdditionalInfos with SubType = 'REF' from Invoice Lines and Invoice Header(only 1 invoice and 1 header)", ["HEA", "LIN"], documents);

		string[] GetWrapperAdditionalReferenceValues()
		{
			wrapper = GetWrapper(entryLine);
			additionalReference = wrapper.AdditionalReference;
			return additionalReference.Select(x => (string)x.Name).OrderBy(x => x).ToArray();
		}
	}

	public void TestAdditionalReferences_ContainingBothSupDocsAndREFDocs()
	{
		IReadOnlyCollection<IDocumentsCommon> additionalReference;
		AddInvoiceAndMerge();
		AddSupportingDocuments(declaration.SupportingDocuments, "Y001", "9001");
		AddSupportingDocuments(entryInstruction.SupportingDocuments, "Y002", "9002");
		AddSupportingDocuments(invoiceHeader.SupportingDocuments, "Y003", "Y003", "9003");
		AddSupportingDocuments(invoiceLine.SupportingDocuments, "Y004", "Y004", "9004", "9005");
		AddSupportingDocuments(invoice2Header.SupportingDocuments, "Y021", "9021");
		AddSupportingDocuments(invoice2Line.SupportingDocuments, "Y022", "Y022", "9022", "9023");

		AddAdditionalInfos(declaration.AdditionalInfos, ("DEC", "REF"));
		AddAdditionalInfos(entryInstruction.AdditionalInfos, ("ENT", "REF"));
		AddAdditionalInfos(invoiceHeader.AdditionalInfos, ("HEA", "REF"), ("HEA2", "INF"));
		AddAdditionalInfos(invoiceLine.AdditionalInfos, ("LIN", "REF"), ("LIN2", "TRA"));

		var documents = GetWrapperAdditionalReferenceValues();
		AssertArrayEqualsByElements("AdditionalReference expected to include 1) Supporting Documents with code prefix Y from Invoice Lines and Invoice Header (2 invoices in the entry) 2) AdditionalInfos with SubType = 'REF' from Invoice Lines and Invoice Header.", ["HEA", "LIN", "Y003", "Y004", "Y021", "Y022"], documents);

		string[] GetWrapperAdditionalReferenceValues()
		{
			wrapper = GetWrapper(entryLine);
			additionalReference = wrapper.AdditionalReference;
			return additionalReference.Select(x => (string)x.Name).OrderBy(x => x).ToArray();
		}
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
		invoiceLine.JI_CEI = entryInstruction.PK;

		invoice2Header = null;
		invoice2Line = null;

		invoice3Header = null;
		invoice3Line = null;

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];

		wrapper = GetWrapper(entryLine);
	}

	void AddInvoiceNotIncludedIntoEntry()
	{
		invoice3Header = declaration.Invoices.AddNew();
		invoice3Line = invoice3Header.InvoiceLines.AddNew();
	}

	void RemoveInvoiceNotIncludedIntoEntry()
	{
		if (invoice3Line is not null)
		{
			invoice3Line.Delete();
			invoice3Line = null;
		}

		if (invoice3Header is not null)
		{
			invoice3Header.Delete();
			invoice3Header = null;
		}
	}

	void AddInvoiceAndMerge()
	{
		RemoveInvoiceNotIncludedIntoEntry();

		invoice2Header = declaration.Invoices.AddNew();
		invoice2Line = invoice2Header.InvoiceLines.AddNew();
		invoice2Line.JI_Tariff = "11";
		invoice2Line.JI_CEI = entryInstruction.PK;

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	JobComInvoiceHeader invoice2Header;
	JobComInvoiceLine invoice2Line;
	JobComInvoiceHeader invoice3Header;
	JobComInvoiceLine invoice3Line;
	CusEntryLine entryLine;
	T2LPOUSRequestAndReceptionGoodItemWrapper wrapper;

	T2LPOUSRequestAndReceptionGoodItemWrapper GetWrapper(CusEntryLine entryLine) => new T2LPOUSRequestAndReceptionGoodItemWrapper(entryLine);

	protected override T2LPOUSRequestAndReceptionGoodItemWrapper GetProvider() => wrapper;
}
