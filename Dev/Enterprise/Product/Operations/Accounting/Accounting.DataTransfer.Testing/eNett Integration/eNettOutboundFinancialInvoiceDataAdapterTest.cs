using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.DataTransfer.Invoices.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.eNett_Integration.Testing
{
	[TestedType(typeof(eNettOutboundFinancialInvoiceDataAdapter))]
	sealed class eNettOutboundFinancialInvoiceDataAdapterTest : FinancialInvoiceXmlDataAdapterTest
	{
		[TestDate(2009, 1, 16, 17, 11, 00)]
		public void TestPdfAttachments_Original()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertPdfAttachments(GetPopulatedInvoice());
		}

		[TestDate(2009, 1, 16, 17, 11, 00)]
		public void TestPdfAttachments_DocBuilder()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertPdfAttachments(GetPopulatedInvoice());
		}

		public void TestTemplateSelectionDependsOnRegistrySetting_NormalInvoice()
		{
			InvoicingBase invoice = GetPopulatedInvoice();
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			AssertNoExceptionThrown(() => AssertPdfAttachments(invoice));
			AssertEquals("Invoice", invoice.EnterpriseInvoiceMenuName);
		}

		public void TestTemplateSelectionDependsOnRegistrySetting_DocBuilderInvoice()
		{
			InvoicingBase invoice = GetPopulatedInvoice();
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			AssertNoExceptionThrown(() => AssertPdfAttachments(invoice));
			AssertEquals("DocBuilder Invoice", invoice.EnterpriseInvoiceMenuName);
		}

		public void TestExportToValueObjectShouldWorkWhenReportRelatedDocTypeFilesExistedInEdocs()
		{
			DocumentsDataRegistry.Instance.UseNewDocBuilderARInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testFileName1 = "StorageFile.txt";
			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 0, 1, 2, 3 }), testFileName1, "ACV", description: "StorageFile type file");

			var testFileName2 = "StorageDocs.tif";
			var storageMain = invoice.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(invoice, Core.Constants.DocManagerCodes.ReceivableInvoice);
			storageMain.AddFileOrDocument(new byte[] { 0, 1, 2, 3 }, testFileName2, "ACV", false, description: "StorageDocs type file");
			invoice.DocManagerInfo.Save();

			var command = Factory.LoadTop1<DocumentCommand>(new ZQuery(new ZQuery(StmMenuItemSchema.SU_MenuName, "DocBuilder Invoice"), new ZQuery(StmMenuItemSchema.SU_BusinessContext, "ARInvoice")));
			var acvDocType = Factory.LoadTop1<RefDocType>(new ZQuery(RefDocTypeSchema.RT_DocType, "ACV"));
			command.AddEDoc(acvDocType);

			Factory.Save();

			var xmlTransaction = new TxnHeader();
			var adapter = new eNettOutboundFinancialInvoiceDataAdapter();

			AssertEquals(0, ErrorReporter.TotalErrorCount);
			adapter.ExportToValueObject(invoice, xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));
			AssertNull(ErrorReporter.LastExceptionReported?.Message);
		}

		#region Implementation

		void AssertPdfAttachments(InvoicingBase invoice)
		{
			TxnHeader xmlTransaction = new TxnHeader();
			eNettOutboundFinancialInvoiceDataAdapter adapter = new eNettOutboundFinancialInvoiceDataAdapter();
			adapter.ExportToValueObject(invoice, xmlTransaction, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Number of attachments", 1, xmlTransaction.Attachments.Count);
			AssertEquals("Filename", "Invoice 00001000.pdf", xmlTransaction.Attachments[0].FileName);
			var length = xmlTransaction.Attachments[0].Data.Length;
			Assert(string.Format("We expected the serialized invoice document's content length to be at least 4000 bytes, but the content length was actually {0} bytes", length), length > 4000);
		}

		InvoicingBase GetPopulatedInvoice()
		{
			InvoicingBase result = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			result.AH_TransactionNum = "00001";
			result.AH_OH = TestObjectCreator.AALSHI.PK;
			InvoicingLineBase line = TestObjectCreator.CreateInvoiceLine(result, TestObjectCreator.AUD, 1, 10);
			line.AL_AC = TestObjectCreator.CC1.PK;
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_ParentID = shipment.PK;
			job.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			line.AL_JH = job.PK;
			ObjectCreator.CreateJobCharge(line, job, TestObjectCreator.CC1, ObjectCreator.AUD);
			result.Factory.Save();

			return result;
		}

		#endregion
	}
}
