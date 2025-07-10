using System;
using CargoWise.Definitions;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceBatchHeaderDocumentSupporter))]
	public class InvoiceBatchHeaderTestForDocumentSupporter : DocumentSupporterTest
	{
		public void TestCreateInvoiceBatchShouldNotThrowException()
		{
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "MTH");

			var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var line = TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.FRT, TestObjectCreator.AUD, 1m, "Desc", 50m);
			Factory.Save();

			Assert("Invoice should be posted.", invoice.IsPosted);

			var invoiceBatch = Factory.New<InvoiceBatchHeader>();
			invoiceBatch.AH_Ledger = LedgerTypes.AccountsReceivable;
			invoiceBatch.AH_OH = invoice.AH_OH;
			invoiceBatch.Line.Add(invoice);

			AssertNoExceptionThrown("Should not throw critical validation exception here.", Factory.Save);
		}

		public void TestGetDocBusinessObjects()
		{
			InvoiceBatchHeaderDocumentSupporter docSupporter = InvoiceBatchHeaderDocumentSupporter.New(Factory.New<InvoiceBatchHeader>());

			DocumentWrapper[] wrappers = docSupporter.GetDocumentWrappers(Constants.DataContext.ARBatchInvoice, null);
			AssertEquals("Size of DocumentBusinessObjectWrappers Array", 1, wrappers.Length);
			AssertEquals("Type of DocumentBusinessObjectWrapper", "Enterprise.DocumentWrappers.DocARBatchInvoice", wrappers[0].GetType().ToString());

			wrappers = docSupporter.GetDocumentWrappers(Constants.DataContext.ARInvoice, null);
			AssertNull("DocumentBusinessObjectWrappers Array should be null", wrappers);
		}

		public void TestCheckBusinessContext()
		{
			InvoiceBatchHeader invoiceBatch = Factory.New<InvoiceBatchHeader>();
			invoiceBatch.AH_Ledger = LedgerTypes.AccountsPayable;
			InvoiceBatchHeaderDocumentSupporter docSupporter = InvoiceBatchHeaderDocumentSupporter.New(invoiceBatch);

			AssertEquals(BusinessContext.INVALID, docSupporter.BusinessContext);

			invoiceBatch.AH_Ledger = LedgerTypes.AccountsReceivable;
			AssertEquals(BusinessContext.ARBatchInvoice, docSupporter.BusinessContext);
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			JobHeader testJob = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			testJob.JH_ParentTableCode = JobCartageSchema.Constants.Prefix;

			InvoiceBatchHeader header = Factory.New<InvoiceBatchHeader>();
			header.AH_OH = TestObjectCreator.AALSHI.PK;
			ARInvoice invoice1 = Factory.New<ARInvoice>();
			invoice1.AH_AH_InvoiceStatement = header.PK;
			invoice1.AH_JH = testJob.PK;
			header.Line.Add(invoice1);

			ARInvoice invoice2 = Factory.New<ARInvoice>();
			invoice2.AH_AH_InvoiceStatement = header.PK;
			invoice2.AH_JH = testJob.PK;
			header.Line.Add(invoice2);

			ARInvoice invoice3 = Factory.New<ARInvoice>();
			invoice3.AH_AH_InvoiceStatement = header.PK;
			invoice3.AH_JH = testJob.PK;
			header.Line.Add(invoice3);

			AssertEquals("Precondition: Header.Line count", 3, header.Line.Count);

			return header;
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
	}
}
