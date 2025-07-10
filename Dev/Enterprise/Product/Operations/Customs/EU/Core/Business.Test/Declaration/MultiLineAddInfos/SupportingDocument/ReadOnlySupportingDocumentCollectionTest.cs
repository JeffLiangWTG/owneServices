using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(ReadOnlySupportingDocumentCollection))]
	class ReadOnlySupportingDocumentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReadOnlySupportingDocumentCollection>
	{
		public void TestReadOnlySupportingDocumentCollectionWithNoSupportingDocuments()
		{
			var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
			readOnlySupportingDocumentCollection.LoadNew();
			AssertEquals("When there are no supporting documents, ReadOnlySupportingDocumentCollection.Count", 0, readOnlySupportingDocumentCollection.Count);
		}

		public void TestIsLoaded()
		{
			var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("Not Loaded", false, readOnlySupportingDocumentCollection.IsLoaded);
				readOnlySupportingDocumentCollection.LoadNew();
				AssertEquals("Loaded", true, readOnlySupportingDocumentCollection.IsLoaded);
			});
		}

		public void TestReadOnlySupportingDocumentCollectionDeclaration()
		{
			var supDoc1 = GetSupportingDoc(1, "REF111");
			declaration.SupportingDocuments.Add(supDoc1);
			var supDoc2 = GetSupportingDoc(2, "REF222");
			declaration.SupportingDocuments.Add(supDoc2);
			var supDoc3 = GetSupportingDoc(3, "REF333");
			declaration.SupportingDocuments.Add(supDoc3);

			var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
			readOnlySupportingDocumentCollection.LoadNew();

			AssertEquals("ReadOnlySupportingDocumentCollection.Count", 3, readOnlySupportingDocumentCollection.Count);

			var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), supDoc1);
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), supDoc2);
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), supDoc3);
		}

		public void TestReadOnlySupportingDocumentCollectionInvoiceHeader()
		{
			var supDoc1 = GetSupportingDoc(1, "REF111");
			invoice.SupportingDocuments.Add(supDoc1);
			var supDoc2 = GetSupportingDoc(2, "REF222");
			invoice.SupportingDocuments.Add(supDoc2);
			var supDoc3 = GetSupportingDoc(3, "REF333");
			invoice.SupportingDocuments.Add(supDoc3);

			var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
			readOnlySupportingDocumentCollection.LoadNew();
			AssertEquals("ReadOnlySupportingDocumentCollection.Count", 3, readOnlySupportingDocumentCollection.Count);

			var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), supDoc1);
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), supDoc2);
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), supDoc3);
		}

		public void TestReadOnlySupportingDocumentCollectionInvoiceLines()
		{
			var supDoc1 = GetSupportingDoc(1, "REF111");
			invoiceLine.SupportingDocuments.Add(supDoc1);
			var supDoc2 = GetSupportingDoc(2, "REF222");
			invoiceLine.SupportingDocuments.Add(supDoc2);
			var supDoc3 = GetSupportingDoc(3, "REF333");
			invoiceLine.SupportingDocuments.Add(supDoc3);

			var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
			readOnlySupportingDocumentCollection.LoadNew();
			AssertEquals("ReadOnlySupportingDocumentCollection.Count", 3, readOnlySupportingDocumentCollection.Count);

			var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), supDoc1);
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), supDoc2);
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), supDoc3);
		}

		public void TestReadOnlySupportingDocumentCollectionInAllLevels()
		{
			var supDoc1 = GetSupportingDoc(1, "REF111");
			declaration.SupportingDocuments.Add(supDoc1);
			var supDoc2 = GetSupportingDoc(2, "REF222");
			invoice.SupportingDocuments.Add(supDoc2);
			var supDoc3 = GetSupportingDoc(3, "REF333");
			invoiceLine.SupportingDocuments.Add(supDoc3);

			var readOnlySupportingDocumentCollection = new ReadOnlySupportingDocumentCollection(entryLine);
			readOnlySupportingDocumentCollection.LoadNew();
			AssertEquals("ReadOnlySupportingDocumentCollection.Count", 3, readOnlySupportingDocumentCollection.Count);

			var readOnlySupportingList = readOnlySupportingDocumentCollection.Cast<ReadOnlySupportingDocument>();
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF111"), supDoc1);
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF222"), supDoc2);
			CheckReadOnlyDocument(readOnlySupportingList.SingleOrDefault(x => x.CSI_ReferenceNumber == "REF333"), supDoc3);
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
				AssertEquals("CSI_CodeDescription", supportingDocument.CSI_CodeDescription, readOnlySupportingDocument.CSI_CodeDescription);
				AssertEquals("CSI_SubType", supportingDocument.CSI_SubType, readOnlySupportingDocument.CSI_SubType);
				AssertEquals("CSI_AdditionalDescription", supportingDocument.CSI_AdditionalDescription, readOnlySupportingDocument.CSI_AdditionalDescription);
				AssertEquals("CSI_ItemNumber", supportingDocument.CSI_ItemNumber, readOnlySupportingDocument.CSI_ItemNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "1234", "1234", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryLine = entryHeader.MergedLines.AddNew();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Weight = 100m;
			invoiceLine.JI_NetWeight = 200m;
			invoiceLine.JI_CL = entryLine.PK;
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

		JobDeclaration declaration;
		CusEntryLine entryLine;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		SupportingDocument GetSupportingDoc(int i, ZString refNumber, decimal qty3 = 10.0m)
		{
			var supDoc = Factory.New<SupportingDocument>();
			supDoc.SuspendValidation();

			supDoc.CSI_Code = "1234";
			supDoc.CSI_ReferenceNumber = refNumber;
			supDoc.CSI_SubType = "A";

			supDoc.CSI_Quantity = i * 10;
			supDoc.CSI_UnitOfQuantity = "BAG";
			supDoc.CSI_Quantity2 = i * 10.1;
			supDoc.CSI_UnitOfQuantity2 = "PKT";
			supDoc.CSI_Value = i * 1000;
			supDoc.CSI_RX_NKCurrency = "GBP";
			supDoc.CSI_DateOfIssue = new ZDateTime(2020, 01, 01);
			supDoc.CSI_DateOfExpiry = new ZDateTime(2020, 12, 31);
			supDoc.CSI_Quantity3 = qty3;

			supDoc.CSI_Description = "Testing";
			supDoc.CSI_ReferenceNumber2 = "REFNUM2";
			supDoc.CSI_AdditionalDescription = "AddDescr";
			supDoc.CSI_CustomsOffice = "ABC";
			supDoc.CSI_Procedure = "XYZ";
			supDoc.CSI_RN_NKCountryCode = "GB";
			supDoc.CSI_Status = "QWE";
			supDoc.CSI_Tariff = "12345";
			supDoc.CSI_Type = "SUP";
			supDoc.CSI_UnitOfQuantity3 = "U3";

			supDoc.CSI_AdditionalDescription = "AddInfo";
			supDoc.CSI_ItemNumber = 2;

			return supDoc;
		}
	}
}
