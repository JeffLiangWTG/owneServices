using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.IE;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(SupportingDocument))]
	public class SupportingDocumentTest : Customs.Business.Testing.CusSupportingInfoTest<SupportingDocument>
	{
		public void TestUnitOfQuantityFieldType()
		{
			AssertEquals(nameof(FieldType.TextDropEdit), Factory.New<SupportingDocument>().UnitOfQuantityFieldType);
		}

		public void TestCSI_Code_MaxLength()
		{
			AssertEquals(4, supportingDocument.GetPossiblyCustomPropertyMaxLength(SupportingDocument.Schema.CSI_Code));
		}

		public void TestCSI_ReferenceNumber_MaxLength()
		{
			AssertEquals(70, supportingDocument.GetPossiblyCustomPropertyMaxLength(SupportingDocument.Schema.CSI_ReferenceNumber));
		}

		public void TestCSI_ReferenceNumber2_MaxLength()
		{
			AssertEquals(70, supportingDocument.GetPossiblyCustomPropertyMaxLength(SupportingDocument.Schema.CSI_ReferenceNumber2));
		}

		public void TestCSI_AdditionalDescription_Caption()
		{
			CombineAssertions(() =>
			{
				var data = DataBoundResourceStrings.GetDataForProperty(supportingDocument.CSI_AdditionalDescriptionInfo);
				AssertEquals("Caption of CSI_AdditionalDescription", "Issuing Authority Name", data.Caption);
				AssertEquals("Short Caption of CSI_AdditionalDescription", "Authority Name", data.ShortCaption);
			});
		}

		public void TestCSI_AdditionalDescription_MaxLength()
		{
			AssertEquals(70, supportingDocument.GetPossiblyCustomPropertyMaxLength(SupportingDocument.Schema.CSI_AdditionalDescription));
		}

		public void TestCSI_DateOfExpiry_Caption()
		{
			AssertEquals("Date of Validity", DataBoundResourceStrings.GetDataForProperty(supportingDocument.CSI_DateOfExpiryInfo).Caption);
		}

		public void TestCSI_Value_Caption()
		{
			AssertEquals("Amount", DataBoundResourceStrings.GetDataForProperty(supportingDocument.CSI_ValueInfo).Caption);
		}

		public void TestLookups()
		{
			AssertType<SupportingDocumentLookups>("Lookups", supportingDocument.Lookups);
		}

		public void TestValidation()
		{
			var invoiceLineSupportingDocument = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew().SupportingDocuments.AddNew();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
			AssertType<ImportSupportingDocumentValidation>("Dec - Import", supportingDocument.Validation);
			AssertType<InvoiceLineImportSupportingDocumentValidation>("Invoice Line - Import", invoiceLineSupportingDocument.Validation);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Export;
			AssertType<ExportSupportingDocumentValidation>("Dec - Export", supportingDocument.Validation);
			AssertType<InvoiceLineExportSupportingDocumentValidation>("Invoice Line - Export", invoiceLineSupportingDocument.Validation);
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.MiscellaneousCustoms;
			AssertType<CommonSupportingDocumentValidation>("Dec - MSC", supportingDocument.Validation);
			AssertType<CommonSupportingDocumentValidation>("Invoice Line - MSC", invoiceLineSupportingDocument.Validation);
		}

		protected override IEnumerable<SupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.SupportingDocuments.AddNew();

			var invoice = declaration.Invoices.AddNew();
			yield return invoice.SupportingDocuments.AddNew();

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.SupportingDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			return declaration.SupportingDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => supportingDocument;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			supportingDocument = declaration.SupportingDocuments.AddNew();
		}
		JobDeclaration declaration;
		SupportingDocument supportingDocument;
	}
}
