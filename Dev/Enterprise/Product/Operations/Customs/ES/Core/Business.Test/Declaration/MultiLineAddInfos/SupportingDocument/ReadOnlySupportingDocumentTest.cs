using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;
using ReadOnlySupportingDocument = Enterprise.Customs.ES.Business.Declaration.ReadOnlySupportingDocument;
using ReadOnlySupportingDocumentCollection = Enterprise.Customs.ES.Business.Declaration.ReadOnlySupportingDocumentCollection;
using SupportingDocument = Enterprise.Customs.ES.Business.Declaration.SupportingDocument;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(ReadOnlySupportingDocument))]
public class ReadOnlySupportingDocumentTest : NonPersistentBusinessObjectTestCase
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null Supporting Documnet", () => new ReadOnlySupportingDocument(null));
	}

	public void TestIsDocumentHeader()
	{
		var supportingDoc = (ReadOnlySupportingDocument)GetNewBusinessObject();
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(supportingDoc.IsDocumentHeaderInfo);
		AssertEquals("Caption", "Header", captionResourceString.Caption);
	}

	public void TestIsDocumentHeaderImport()
	{
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;

		var supDoc1 = GetSupportingDoc(1, "REF111");
		declaration.SupportingDocuments.Add(supDoc1);
		var supDoc2 = GetSupportingDoc(2, "REF222");
		entryInstruction.SupportingDocuments.Add(supDoc2);
		var supDoc3 = GetSupportingDoc(3, "REF333");
		invoice.SupportingDocuments.Add(supDoc3);
		var supDoc4 = GetSupportingDoc(4, "REF444");
		invoiceLine.SupportingDocuments.Add(supDoc4);

		var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
		readOnlySupportingDocumentCollection.LoadNew();
		var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			entryInstruction.CEI_Style = ZString.Empty;
			AssertEquals("In Declaration, SubStyle T2C and Style Empty", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
			AssertEquals("In EntryInstruction, SubStyle T2C and Style Empty", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
			AssertEquals("In Invoice, SubStyle T2C and Style Empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
			AssertEquals("In Invoice Line, SubStyle T2C and Style Empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("In Declaration, SubStyle T2L and Style Empty", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
			AssertEquals("In EntryInstruction, SubStyle T2L and Style Empty", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
			AssertEquals("In Invoice, SubStyle T2L and Style Empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
			AssertEquals("In Invoice Line, SubStyle T2L and Style Empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6())
			{
				AssertEquals("In Declaration, SubStyle not T2L/T2C and Style not H2 and UCC6", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
				AssertEquals("In EntryInstruction, SubStyle not T2L/T2C and Style not H2 and UCC6", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
				AssertEquals("In Invoice, SubStyle not T2L/T2C and Style not H2 and UCC6", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
				AssertEquals("In Invoice Line, SubStyle not T2L/T2C and Style not H2 and UCC6", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);
			}

			using (CustomsFunctionalityTemporarySetterHelper.SetESFUNCSImportMessageVersionUCC6(enabled: false))
			{
				AssertEquals("In Declaration, SubStyle not T2L/T2C and Style not H2 and no UCC6", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
				AssertEquals("In EntryInstruction, SubStyle not T2L/T2C and Style not H2 no UCC6", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
				AssertEquals("In Invoice, SubStyle not T2L/T2C and Style not H2 no UCC6", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
				AssertEquals("In Invoice Line, SubStyle not T2L/T2C and Style not H2 no UCC6", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);
			}

			entryInstruction.CEI_Style = IMPDeclarationTypeList.Codes.H2;
			AssertEquals("In Declaration, SubStyle not T2L/T2C and Style H2", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
			AssertEquals("In EntryInstruction, SubStyle not T2L/T2C and Style H2", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
			AssertEquals("In Invoice, SubStyle not T2L/T2C and Style H2", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
			AssertEquals("In Invoice Line, SubStyle not T2L/T2C and Style H2", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);
		});
	}

	public void TestIsDocumentHeaderExport()
	{
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;

		var supDoc1 = GetSupportingDoc(1, "REF111");
		declaration.SupportingDocuments.Add(supDoc1);
		var supDoc2 = GetSupportingDoc(2, "REF222");
		entryInstruction.SupportingDocuments.Add(supDoc2);
		var supDoc3 = GetSupportingDoc(3, "REF333");
		invoice.SupportingDocuments.Add(supDoc3);
		var supDoc4 = GetSupportingDoc(4, "REF444");
		invoiceLine.SupportingDocuments.Add(supDoc4);

		var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
		readOnlySupportingDocumentCollection.LoadNew();
		var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();

		CombineAssertions(() =>
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			entryInstruction.CEI_Style = ZString.Empty;
			AssertEquals("In Declaration, SubStyle T2C and Style Empty", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
			AssertEquals("In EntryInstruction, SubStyle T2C and Style Empty", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
			AssertEquals("In Invoice, SubStyle T2C and Style Empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
			AssertEquals("In Invoice Line, SubStyle T2C and Style Empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
			AssertEquals("In Declaration, SubStyle T2L and Style Empty", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
			AssertEquals("In EntryInstruction, SubStyle T2L and Style Empty", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
			AssertEquals("In Invoice, SubStyle T2L and Style Empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
			AssertEquals("In Invoice Line, SubStyle T2L and Style Empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
			{
				AssertEquals("In Declaration, SubStyle not T2L/T2C/EXS and Style empty in transition period", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
				AssertEquals("In EntryInstruction, SubStyle not T2L/T2C/EXS and Style empty in transition period", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
				AssertEquals("In Invoice, SubStyle not T2L/T2C/EXS and Style empty in transition period", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
				AssertEquals("In Invoice Line, SubStyle not T2L/T2C/EXS and Style empty in transition period", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);
			}

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
			{
				AssertEquals("In Declaration, SubStyle not T2L/T2C/EXS and Style empty in final period", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
				AssertEquals("In EntryInstruction, SubStyle not T2L/T2C/EXS and Style empty in final period", true, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
				AssertEquals("In Invoice, SubStyle not T2L/T2C/EXS and Style empty in final period", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
				AssertEquals("In Invoice Line, SubStyle not T2L/T2C/EXS and Style empty in final period", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);
			}

			entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
			AssertEquals("In Declaration, SubStyle EXS and Style empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111").IsDocumentHeader);
			AssertEquals("In EntryInstruction, SubStyle EXS and Style empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222").IsDocumentHeader);
			AssertEquals("In Invoice, SubStyle EXS and Style empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333").IsDocumentHeader);
			AssertEquals("In Invoice Line, SubStyle EXS and Style empty", false, readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444").IsDocumentHeader);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var supportingDocument = GetSupportingDoc(1, "REF111");

		return new ReadOnlySupportingDocument(supportingDocument);
	}

	protected SupportingDocument GetSupportingDoc(int i, ZString refNumber, decimal qty3 = 10.0m)
	{
		var supDoc = Factory.New<SupportingDocument>();
		supDoc.SuspendValidation();

		supDoc.CSI_Code = "1234";
		supDoc.CSI_ReferenceNumber = refNumber;
		supDoc.CSI_SubType = "A"; //Part

		supDoc.CSI_Quantity = i * 10;
		supDoc.CSI_UnitOfQuantity = "BAG";
		supDoc.CSI_Quantity2 = i * 10.1;
		supDoc.CSI_UnitOfQuantity2 = "PKT";
		supDoc.CSI_Value = i * 1000;
		supDoc.CSI_RX_NKCurrency = "GBP";
		supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
		supDoc.CSI_Quantity3 = qty3;

		supDoc.CSI_Description = "Testing"; //reason
		supDoc.CSI_ReferenceNumber2 = "REFNUM2"; //Issueing Authority
		supDoc.CSI_AdditionalDescription = "AddDescr";
		supDoc.CSI_CustomsOffice = "ABC";
		supDoc.CSI_Procedure = "X";
		supDoc.CSI_RN_NKCountryCode = "GB";
		supDoc.CSI_Status = "QWE";
		supDoc.CSI_Tariff = "12345";
		supDoc.CSI_Type = "SUP";
		supDoc.CSI_UnitOfQuantity3 = "U3";
		supDoc.CSI_AdditionalDescription = "AddInfo";
		supDoc.CSI_ItemNumber = 2;

		return supDoc;
	}

	protected IDisposable TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(JobDeclaration declaration, bool configurationValue)
	{
		ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);
		return ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "IsTransitionPeriodAES30Core", configurationValue, declaration.GetDefaultDataGroupingCode());
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;

		entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryLine = entryHeader.MergedLines.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Weight = 100m;
		invoiceLine.JI_NetWeight = 200m;
		invoiceLine.JI_CL = entryLine.PK;
		Factory.Save();
	}

	CusEntryHeader entryHeader;
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryLine entryLine;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;
}
