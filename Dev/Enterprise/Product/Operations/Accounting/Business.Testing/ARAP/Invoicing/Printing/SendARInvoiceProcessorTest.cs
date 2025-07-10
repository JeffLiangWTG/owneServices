using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Printing.Testing
{
	class SendARInvoiceProcessorTest : TestCaseWithFactory
	{
		public void TestProcessorCreatesPrintJobWhenDebtorSetupWithContact()
		{
			var contact = TestObjectCreator.ABIGAS.Contacts.AddNew();
			contact.OC_ContactName = "Fred";
			contact.OC_Email = "fred@wisetechglobal.com.au";
			var document = contact.Documents.AddNew();
			document.OD_DocumentGroup = ContactType.Receivables.Code;
			document.OD_DefaultContact = true;

			Factory.Save();
			ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			Factory.Save();
			process(arInvoice, false);

			AssertContains("AR INV 00001000 was successfully sent", Notifications.AsString);
			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = Factory.Load<StmPrintJob>(query);
			AssertEquals("Should create a print job when contact details are setup for debtor", 1, printJobs.Length);
			AssertEquals("print job should not be in the database", false, printJobs[0].IsInDatabase);

			Factory.Save();
			AssertEquals("print job should be in the database", true, printJobs[0].IsInDatabase);
		}

		public void TestProcessorCreatesPrintJobWithEPrintMethod()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			var ePrintEmailAddress = "printer@eprint.com";
			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				var contact = TestObjectCreator.ABIGAS.Contacts.AddNew();
				var document = contact.Documents.AddNew();
				document.OD_DocumentGroup = ContactType.Receivables.Code;
				document.OD_DeliverBy = Enterprise.Core.Constants.ContactNotifyModes.EPrint;

				Factory.Save();
				ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
				Factory.Save();
				process(arInvoice);

				AssertContains("AR INV 00001000 was successfully sent", Notifications.AsString);
				ZQuery query = new ZQuery();
				StmPrintJob[] printJobs = Factory.Load<StmPrintJob>(query);
				AssertEquals("Should create a print job when contact details are setup for debtor", 1, printJobs.Length);
				AssertEquals("Print job should have destination", ePrintEmailAddress, printJobs[0].SP_Destination);
			}
		}

		public void TestProcessorProcessesOnlyEligibleTransactions()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			var ePrintEmailAddress = "printer@eprint.com";

			using (AccountingMasterFilesRegistry.Instance.AllowRePrintingOfInvoicesAndCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (DocumentsDataRegistry.Instance.EPrintEmailAddress.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ePrintEmailAddress))
			{
				var contact = TestObjectCreator.ABIGAS.Contacts.AddNew();
				var document = contact.Documents.AddNew();
				document.OD_DocumentGroup = ContactType.Receivables.Code;
				document.OD_DeliverBy = Enterprise.Core.Constants.ContactNotifyModes.EPrint;
				Factory.Save();

				var invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
				Factory.Save();

				var eDoc = invoice.DocManagerInfo.AddFileOrDocument(new byte[] { 0 }, "Invoice.pdf", "INV");
				((DocumentScanning.Business.StorageDocsBase)eDoc).SC_IsSystemGenerated = true;
				invoice.DocManagerInfo.Save();
				AssertEquals("AR Invoice should have invoice attached in EDocs", 1, invoice.DocManagerInfo.AllEDocs.Count);
				eDoc = invoice.DocManagerInfo.AllEDocs[0];
				AssertEquals("INV", eDoc.DocType);
				Assert(eDoc.IsSystemGenerated);

				process(invoice);

				var expected = @"AR INV 00001000 was not sent. Error Details: Re-printed versions of a receivables document in your country/region must reflect the data used at the time of posting, re-printing is not allowed through this module.
Please go to the eDocs tab of the transaction and re-print the first invoice version stored there.
";
				AssertContains(expected, Notifications.AsString);
				ZQuery query = new ZQuery();
				StmPrintJob[] printJobs = Factory.Load<StmPrintJob>(query);
				AssertEquals("Should not create a print job ", 0, printJobs.Length);
			}
		}

		public void TestProcessorWhenNoEmailSettingsForDebtor()
		{
			ARInvoice arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			Factory.Save();
			process(arInvoice);

			Assert("Warning should be added", Notifications.HasWarnings);
			AssertContains("Not able to find contact information for ABIGAS. AR INV 00001000 was not sent", Notifications.AsString);
			ZQuery query = new ZQuery();
			BusinessObject[] printJobs = Factory.Load<StmPrintJob>(query);
			AssertEquals("Should not create any print jobs when no contact information", 0, printJobs.Length);
		}

		public void TestWarningShouldBeAddedIfDocumentPackNotExists()
		{
			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1.0M, TestObjectCreator.ABIGAS);
			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentBranchPK))
			{
				process(arInvoice);
			}

			var printJobs = Factory.Load<StmPrintJob>(new ZQuery());
			AssertEquals("Should not create any print jobs when no document pack exists", 0, printJobs.Length);
			var expectMessage = @$"Unable to find AR {arInvoice.AH_TransactionType} {arInvoice.AH_TransactionNum} while running LWK Service Task.
This could be caused by incorrect settings of the workflow trigger’s running company.";
			AssertEquals(1, Notifications.Events.Length);
			AssertEquals(expectMessage, Notifications.Events[0].Message);
			Notifications.Clear();
		}

		void process(InvoicingBase invoice, bool doSave = true)
		{
			var processor = new SendARInvoiceProcessor(invoice);
			Notifications = new NotificationBuffer();
			Assert(!Notifications.HasWarnings);
			processor.Process(Notifications);

			if (doSave)
			{
				Factory.Save();
			}
		}
		NotificationBuffer Notifications;

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;
	}
}
