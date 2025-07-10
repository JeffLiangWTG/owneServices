using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(PreviousDocument))]
	public class PreviousDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<PreviousDocument>
	{
		public void TestCSI_ReferenceNumber_MaxLength()
		{
			AssertEquals(70, previousDocument.GetPossiblyCustomPropertyMaxLength(nameof(PreviousDocument.CSI_ReferenceNumber)));
		}

		public void TestParentIsJobComInvoiceLine()
		{
			Assert("Parent is InvoiceHeader", !previousDocument.ParentIsJobComInvoiceLine);
			var line = declaration.InvoiceLines.AddNew();
			var linePreviousDocument = line.PreviousDocuments.AddNew();
			Assert("Parent is InvoiceLine", linePreviousDocument.ParentIsJobComInvoiceLine);
		}

		public void TestCSI_DateOfIssueReadonly()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default", true, previousDocument.CSI_DateOfIssueInfo.ReadOnly);
				previousDocument.CSI_Code = PreviousDocumentTypeList.Codes.ReferenceDateOfEntryInTheDeclarantRecords;
				AssertEquals("CLE", false, previousDocument.CSI_DateOfIssueInfo.ReadOnly);
				previousDocument.CSI_Code = PreviousDocumentTypeList.Codes.TemporaryStorageDeclaration;
				AssertEquals("Not CLE", true, previousDocument.CSI_DateOfIssueInfo.ReadOnly);
			});
		}

		public void TestClearCSI_DateOfIssueIfReadOnly()
		{
			previousDocument.CSI_DateOfIssue = ZDateTime.Today;
			previousDocument.CSI_Code = PreviousDocumentTypeList.Codes.TemporaryStorageDeclaration;
			AssertEquals(ZDateTime.Empty, previousDocument.CSI_DateOfIssue);
		}

		public void TestCSI_CodeInfo_ResourceStringData()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_CodeInfo, multipleResourceKeys: null, "Type", fullDescription: "[12 01 002 000] Type");
		}

		public void TestCSI_PackTypeInfo_ResourceStringData()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_PackTypeInfo, multipleResourceKeys: null, "Package Type", fullDescription: "[12 01 003 000] Type of Packages");
		}

		public void TestCSI_PackQtyInfo_ResourceStringData()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_PackQtyInfo, multipleResourceKeys: null, "Number of Packages", fullDescription: "[12 01 004 000] Number of Packages", shortCaption: "Package No.");
		}

		public void TestCSI_UnitOfQuantityInfo_ResourceStringData()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_UnitOfQuantityInfo, multipleResourceKeys: null, "Measurement Unit and Qualifier", fullDescription: "[12 01 005 000] Measurement Unit and Qualifier", shortCaption: "UOM");
		}

		public void TestCSI_QuantityInfo_ResourceStringData()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_QuantityInfo, multipleResourceKeys: null, "Quantity", fullDescription: "[12 01 006 000] Quantity");
		}

		public void TestCSI_ItemNumberInfo_ResourceStringData()
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(previousDocument.CSI_ItemNumberInfo, multipleResourceKeys: null, "Goods Item Identifier", fullDescription: "[12 01 007 000] Goods Item Identifier", shortCaption: "Item No.");
		}

		public void TestLookups()
		{
			AssertType("Lookups should return the new IE PreviousDocumentLookups", typeof(PreviousDocumentLookups), previousDocument.Lookups);
		}

		public void TestValidation()
		{
			var invoiceLinePreviousDocument = declaration.Invoices.AddNew().InvoiceLines.AddNew().PreviousDocuments.AddNew();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportPreviousDocumentValidation>("Invoice - Import", previousDocument.Validation);
			AssertType<InvoiceLineImportPreviousDocumentValidation>("Invoice Line - Import", invoiceLinePreviousDocument.Validation);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportPreviousDocumentValidation>("Dec - Export", previousDocument.Validation);
			AssertType<InvoiceLineExportPreviousDocumentValidation>("Invoice Line - Export", invoiceLinePreviousDocument.Validation);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<CommonPreviousDocumentValidation>("Dec - MSC", previousDocument.Validation);
			AssertType<CommonPreviousDocumentValidation>("Invoice Line - MSC", invoiceLinePreviousDocument.Validation);
		}

		protected override IEnumerable<PreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			declaration = factory.New<JobDeclaration>();
			yield return declaration.PreviousDocuments.AddNew();

			invoice = declaration.Invoices.AddNew();
			previousDocument = invoice.PreviousDocuments.AddNew();
			yield return previousDocument;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.PreviousDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject() => previousDocument;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			previousDocument = invoice.PreviousDocuments.AddNew();
		}

		PreviousDocument previousDocument;
		JobComInvoiceHeader invoice;
		JobDeclaration declaration;
	}
}
