using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Testing;

[TestedType(typeof(ReadOnlySupportingDocumentCollection))]
class ReadOnlySupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReadOnlySupportingDocumentCollection>
{
	public void TestReadOnlySupportingDocumentCollectionEntryLines()
	{
		var supDoc1 = GetSupportingDoc(1, "REF111");
		supDoc1.CSI_ParentID = entryLine.PK;
		supDoc1.CSI_ParentTableCode = entryLine.TablePrefix;
		supDoc1.CSI_Status = "ACC";
		var supDoc2 = GetSupportingDoc(2, "REF222");
		supDoc2.CSI_ParentID = entryLine.PK;
		supDoc2.CSI_ParentTableCode = entryLine.TablePrefix;
		supDoc2.CSI_Status = "ACC";
		var supDoc3 = GetSupportingDoc(3, "REF333");
		supDoc3.CSI_ParentID = entryLine.PK;
		supDoc3.CSI_ParentTableCode = entryLine.TablePrefix;
		supDoc3.CSI_Status = "ACC";

		Factory.Save();

		var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
		readOnlySupportingDocumentCollection.LoadNew();
		AssertEquals("ReadOnlySupportingDocumentCollection.Count", 3, readOnlySupportingDocumentCollection.Count);

		var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), supDoc1);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), supDoc2);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), supDoc3);
	}

	public void TestReadOnlySupportingDocumentCollectionInAllLevelsIncludingEntryLines()
	{
		var supDoc1 = GetSupportingDoc(1, "REF111");
		declaration.SupportingDocuments.Add(supDoc1);
		var supDoc2 = GetSupportingDoc(2, "REF222");
		entryInstruction.SupportingDocuments.Add(supDoc2);
		var supDoc3 = GetSupportingDoc(3, "REF333");
		invoice.SupportingDocuments.Add(supDoc3);
		var supDoc4 = GetSupportingDoc(4, "REF444");
		invoiceLine.SupportingDocuments.Add(supDoc4);
		var supDoc4B = GetSupportingDoc(9, "REF111");
		invoiceLine.SupportingDocuments.Add(supDoc4B);
		var supDoc5 = GetSupportingDoc(5, "REF555");
		supDoc5.CSI_ParentID = entryLine.PK;
		supDoc5.CSI_ParentTableCode = entryLine.TablePrefix;
		supDoc5.CSI_Status = "ACC";
		var supDoc6 = GetSupportingDoc(6, "REF000");
		supDoc6.CSI_ParentID = entryLine.Header.PK;
		supDoc6.CSI_ParentTableCode = entryLine.Header.TablePrefix;
		supDoc6.CSI_Status = "ACC";
		var supDoc7 = GetSupportingDoc(7, "REF666");
		supDoc7.CSI_ParentID = entryLine.PK;
		supDoc7.CSI_ParentTableCode = entryLine.TablePrefix;
		supDoc7.CSI_Status = "ACC";
		var supDoc9 = GetSupportingDoc(9, "REF999");
		supDoc9.CSI_ParentID = entryLine.PK;
		supDoc9.CSI_ParentTableCode = entryLine.TablePrefix;
		supDoc9.CSI_Status = "ACC";
		var supDoc10 = GetSupportingDoc(8, "REF999");
		supDoc10.CSI_ParentID = entryLine.Header.PK;
		supDoc10.CSI_ParentTableCode = entryLine.Header.TablePrefix;
		supDoc10.CSI_Status = "ACC";

		var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
		readOnlySupportingDocumentCollection.LoadNew();
		AssertEquals("ReadOnlySupportingDocumentCollection.Count", 8, readOnlySupportingDocumentCollection.Count);

		var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), supDoc1);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), supDoc2);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), supDoc3);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444"), supDoc4);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF555"), supDoc5);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF000"), supDoc6);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF666"), supDoc7);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF999"), supDoc10);
		AssertContainsExactElementsInExactOrder("The 7 SupportingDocuments after calling ReadOnlySupportingDocumentCollection have the correct order (First CH)", new ZString[] { "REF000", "REF999", "REF555", "REF666", "REF222", "REF111", "REF333", "REF444" }, readOnlySupportingList.Select(x => x.CSI_ReferenceNumber).ToArray());
	}

	public void TestReadOnlySupportingDocumentCollectionWithDuplicates()
	{
		var supDoc1 = GetSupportingDoc(1, "REF111");
		declaration.SupportingDocuments.Add(supDoc1);
		var supDoc2 = GetSupportingDoc(2, "REF222");
		entryInstruction.SupportingDocuments.Add(supDoc2);
		var supDoc3 = GetSupportingDoc(3, "REF333");
		invoice.SupportingDocuments.Add(supDoc3);
		var supDoc4 = GetSupportingDoc(4, "REF444");
		invoiceLine.SupportingDocuments.Add(supDoc4);
		var supDoc5 = GetSupportingDoc(5, "REF444");
		supDoc5.CSI_ParentID = entryLine.PK;
		supDoc5.CSI_ParentTableCode = entryLine.TablePrefix;
		supDoc5.CSI_Status = "ACC";

		var supDoc6 = GetSupportingDoc(6, "REF444");
		supDoc6.CSI_Procedure = "N";
		invoiceLine.SupportingDocuments.Add(supDoc6);

		var supDoc7 = GetSupportingDoc(3, "REF222");
		invoiceLine.SupportingDocuments.Add(supDoc7);

		var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
		readOnlySupportingDocumentCollection.LoadNew();
		AssertEquals("ReadOnlySupportingDocumentCollection.Count", 5, readOnlySupportingDocumentCollection.Count);

		var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), supDoc1);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), supDoc2);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), supDoc3);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444" && x.CSI_Procedure == "A"), supDoc5);
		CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF444" && x.CSI_Procedure == "N"), supDoc6);
	}

	protected void CheckReadOnlyDocument(ReadOnlySupportingDocument readOnlySupportingDocument, SupportingDocument supportingDocument)
	{
		CombineAssertions("Grouped Supporting Document" + readOnlySupportingDocument.CSI_ReferenceNumber, () =>
		{
			AssertEquals("CSI_Code", supportingDocument.CSI_Code, readOnlySupportingDocument.CSI_Code);
			AssertEquals("CSI_ReferenceNumber", supportingDocument.CSI_ReferenceNumber, readOnlySupportingDocument.CSI_ReferenceNumber);
			AssertEquals("CSI_Status", supportingDocument.CSI_Status, readOnlySupportingDocument.CSI_Status);
			AssertEquals("CSI_Quantity", supportingDocument.CSI_Quantity, readOnlySupportingDocument.CSI_Quantity);
			AssertEquals("CSI_UnitOfQuantity", supportingDocument.CSI_UnitOfQuantity, readOnlySupportingDocument.CSI_UnitOfQuantity);
			AssertEquals("CSI_Quantity2", supportingDocument.CSI_Quantity2, readOnlySupportingDocument.CSI_Quantity2);
			AssertEquals("CSI_UnitOfQuantity2", supportingDocument.CSI_UnitOfQuantity2, readOnlySupportingDocument.CSI_UnitOfQuantity2);
			AssertEquals("CSI_Value", supportingDocument.CSI_Value, readOnlySupportingDocument.CSI_Value);
			AssertEquals("CSI_RX_NKCurrency", supportingDocument.CSI_RX_NKCurrency, readOnlySupportingDocument.CSI_RX_NKCurrency);
			AssertEquals("CSI_DateOfIssue", supportingDocument.CSI_DateOfIssue, readOnlySupportingDocument.CSI_DateOfIssue);
			AssertEquals("CSI_DateOfExpiry", supportingDocument.CSI_DateOfExpiry, readOnlySupportingDocument.CSI_DateOfExpiry);
			AssertEquals("CSI_Procedure", supportingDocument.CSI_Procedure, readOnlySupportingDocument.CSI_Procedure);
			AssertEquals("CSI_AdditionalDescription", supportingDocument.CSI_AdditionalDescription, readOnlySupportingDocument.CSI_AdditionalDescription);
			AssertEquals("CSI_ItemNumber", supportingDocument.CSI_ItemNumber, readOnlySupportingDocument.CSI_ItemNumber);
		});
	}

	protected override ReadOnlySupportingDocumentCollection GetCollectionToTest()
	{
		invoiceLine.SupportingDocuments.Add(GetSupportingDoc(1, "REF111"));
		invoiceLine.SupportingDocuments.Add(GetSupportingDoc(2, "REF222"));
		invoiceLine.SupportingDocuments.Add(GetSupportingDoc(3, "REF333"));
		var result = new ReadOnlySupportingDocumentCollection(entryLine);
		result.LoadNew();
		return result;
	}

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var supDoc = GetSupportingDoc(3, "REF333");
		return new ReadOnlySupportingDocument(supDoc);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.A;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryLine = entryHeader.MergedLines.AddNew();
		invoice = declaration.Invoices.AddNew();
		invoiceLine = invoice.InvoiceLines.AddNew();
		invoiceLine.JI_Weight = 100m;
		invoiceLine.JI_NetWeight = 200m;
		invoiceLine.JI_CL = entryLine.PK;
		Factory.Save();
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	CusEntryLine entryLine;
	JobComInvoiceHeader invoice;
	JobComInvoiceLine invoiceLine;

	SupportingDocument GetSupportingDoc(int i, ZString refNumber)
	{
		var supDoc = Factory.New<SupportingDocument>();
		supDoc.SuspendValidation();

		supDoc.CSI_Code = "1234";
		supDoc.CSI_ReferenceNumber = refNumber;
		supDoc.CSI_SubType = "A";

		supDoc.CSI_Quantity = i * 10;
		supDoc.CSI_UnitOfQuantity = i + "AG";
		supDoc.CSI_Quantity2 = i * 10.1;
		supDoc.CSI_UnitOfQuantity2 = i + "KT";
		supDoc.CSI_Value = i * 1000;
		supDoc.CSI_RX_NKCurrency = i + "BP";
		supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
		supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
		supDoc.CSI_Quantity3 = 10.0m;

		supDoc.CSI_Description = "Testing";
		supDoc.CSI_ReferenceNumber2 = "REFNUM2";
		supDoc.CSI_AdditionalDescription = "AddDescr";
		supDoc.CSI_CustomsOffice = "ABC";
		supDoc.CSI_RN_NKCountryCode = "GB";
		supDoc.CSI_Status = "QWE";
		supDoc.CSI_Tariff = "12345";
		supDoc.CSI_Type = "SUP";
		supDoc.CSI_UnitOfQuantity3 = "U3";
		supDoc.CSI_Procedure = "A";
		supDoc.CSI_AdditionalDescription = "AddInfo";
		supDoc.CSI_ItemNumber = 2;

		return supDoc;
	}
}
