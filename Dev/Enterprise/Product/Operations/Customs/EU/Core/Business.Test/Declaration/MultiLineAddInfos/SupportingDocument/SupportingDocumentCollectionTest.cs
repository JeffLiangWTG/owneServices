using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(SupportingDocumentCollection<SupportingDocument>))]
	class SupportingDocumentCollectionGenericTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new SupportingDocumentCollection<SupportingDocument>(declaration);
		}
	}

	[TestedType(typeof(SupportingDocumentCollection))]
	public class SupportingDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<SupportingDocument>
	{
		protected override CusSupportingInfoCollection<SupportingDocument> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new SupportingDocumentCollection(declaration);
		}

		public virtual void TestAddNewInvoiceDocumentAndCopyDataFromInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = supplier.PK;
			supplier.MainAddress.OA_RN_NKCountryCode = "GB";
			invoice.JZ_InvoiceNumber = "1";
			invoice.JZ_InvoiceDate = ZDate.Today;

			var invoiceDocument =
				invoice.SupportingDocuments.AddNewInvoiceDocumentAndCopyDataFromInvoice(UniversalReferenceConstants.SupportingDocumentTypes.N380);

			CombineAssertions("N380 Invoice Supporting Document", () =>
			{
				AssertEquals("CSI_Code", "N380", invoiceDocument.CSI_Code);
				AssertEquals("CSI_ReferenceNumber", "1", invoiceDocument.CSI_ReferenceNumber);
				AssertDateOfIssue(invoiceDocument);
				AssertEquals("CSI_RN_NKCountryCode", "GB", invoiceDocument.CSI_RN_NKCountryCode);
			});
		}

		public void TestAddNewIfNotExsistWithSameCodeAndReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocumentCollection = declaration.SupportingDocuments;

			AssertEquals("[PRE-CONDITION] Supporting Documents should be empty", 0, supportingDocumentCollection.Count);

			var returnedSupportingDocument = supportingDocumentCollection.AddNewIfNotExsistWithSameCodeAndReference(code: "XXX", referenceNumber: "123456");
			AssertNotNull("Returned Supporting Document", returnedSupportingDocument);
			AssertEquals("Supporting Documents should contain only one element", 1, supportingDocumentCollection.Count);

			var insertedSupportingDocument = supportingDocumentCollection[0];
			CombineAssertions("Asserting Supporting Document properties", () =>
			{
				AssertEquals("Code", "XXX", insertedSupportingDocument.CSI_Code);
				AssertEquals("Reference", "123456", insertedSupportingDocument.CSI_ReferenceNumber);
				AssertSame("Returned SupportingDocument must be the inserted", insertedSupportingDocument, returnedSupportingDocument);
			});

			returnedSupportingDocument = supportingDocumentCollection.AddNewIfNotExsistWithSameCodeAndReference(code: "XXX", referenceNumber: "123456");
			AssertNull("Returned Supporting Document", returnedSupportingDocument);
			AssertEquals("Supporting Documents should contain only one element (latest AddNew() should be skipped)", 1, supportingDocumentCollection.Count);

			returnedSupportingDocument = supportingDocumentCollection.AddNewIfNotExsistWithSameCodeAndReference(code: "XXX", referenceNumber: "654321");
			AssertNotNull("Returned Supporting Document", returnedSupportingDocument);
			AssertEquals("Supporting Documents should contain two elements", 2, supportingDocumentCollection.Count);

			returnedSupportingDocument = supportingDocumentCollection.AddNewIfNotExsistWithSameCodeAndReference(code: "ZZZ", referenceNumber: "123456");
			AssertNotNull("Returned Supporting Document", returnedSupportingDocument);
			AssertEquals("Supporting Documents should contain three elements", 3, supportingDocumentCollection.Count);
		}

		protected virtual void AssertDateOfIssue(SupportingDocument supportingDocument)
		{
			AssertEquals("CSI_DateOfIssue", ZDateTime.Today, supportingDocument.CSI_DateOfIssue);
		}

		public void TestHelper()
		{
			var supportingDocumentCollection = (SupportingDocumentCollection)GetCusSupportingInfoCollection();
			AssertNotNull("Helper", supportingDocumentCollection.Helper);
			AssertType("Helper Type", ExpectedHelperType, supportingDocumentCollection.Helper);
		}

		public void TestAddNewWithCodeAndReferenceNumber()
		{
			var supportingDocumentCollection = (SupportingDocumentCollection)GetCusSupportingInfoCollection();
			AssertEquals("PRE-CONDITION", 0, supportingDocumentCollection.Count);

			supportingDocumentCollection.AddNew("XYZ", "1234");
			AssertEquals("POST-CONDITION", 1, supportingDocumentCollection.Count);
			CombineAssertions("CSI_Code and CSI_ReferenceNumber", () =>
			{
				var singleSupportingDocument = supportingDocumentCollection[0];
				AssertEquals(nameof(singleSupportingDocument.CSI_Code), "XYZ", singleSupportingDocument.CSI_Code);
				AssertEquals(nameof(singleSupportingDocument.CSI_ReferenceNumber), "1234", singleSupportingDocument.CSI_ReferenceNumber);
			});
		}

		public void TestDeleteAllDocumentsHavingCode()
		{
			var supportingDocumentCollection = (SupportingDocumentCollection)GetCusSupportingInfoCollection();
			var supportingDocument1 = supportingDocumentCollection.AddNew();
			supportingDocument1.CSI_Code = "ABC";
			var supportingDocument2 = supportingDocumentCollection.AddNew();
			supportingDocument2.CSI_Code = "DEF";
			AssertEquals("PRE-CONDITION", 2, supportingDocumentCollection.Count);

			supportingDocumentCollection.DeleteAllDocumentsHavingCode("ABC");
			CombineAssertions("POST-CONDITIONS", () =>
			{
				AssertEquals("SupportingDocumentCollection Count", 1, supportingDocumentCollection.Count);
				AssertEquals("SupportingDocument1 IsDeleted", true, supportingDocument1.IsDeleted);
				AssertEquals("SupportingDocument2 IsDeleted", false, supportingDocument2.IsDeleted);
			});
		}

		protected virtual SupportingDocumentCollection GetSupportingDocumentCollectionFromInvoice(JobComInvoiceHeader invoice) => new SupportingDocumentCollection(invoice);
		protected virtual Type ExpectedHelperType => typeof(EUSupportingDocumentHelper);
	}
}
