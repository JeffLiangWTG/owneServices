using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class CreditNoteAmendingHelperTest : TestCaseWithFactory
	{
		public void TestAmendARTransaction_NoSecurityRights()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AmendTransactionWCreditNote);
			checkpoint.IsAllowed = false;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
			testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_SellGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			invoiceWithLine.AH_IsCancelled = true;
			((IMatching)invoiceWithLine).CurrentMatchGroup.Add(testObjectCreator.CreateMatchLink(invoiceWithLine, 0m));
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Message", SecurityCore.AmendTransactionWCreditNote, Message);
			Assert(Caption.IsEmpty);
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction_OriginalTransactionIsReversedTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AmendTransactionWCreditNote);
			checkpoint.IsAllowed = true;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
			testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_SellGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			invoiceWithLine.AH_IsCancelled = true;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Message", "Cannot amend selected transaction as this has been reversed.", Message);
			AssertEquals("Caption", "Cannot amend transaction", Caption);
		}

		[SuspendCriticalValidation]
		public void TestAmendAPTransaction_OriginalTransactionIsReversedTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.APAmendWithCreditNote);
			checkpoint.IsAllowed = true;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 100M, testObjectCreator.Creditor1, testObjectCreator.AUD, -100, null);
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_CostGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Creditor1.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			invoiceWithLine.AH_IsCancelled = true;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendAPTransaction(invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Message", "Cannot amend selected transaction as this has been reversed.", Message);
			AssertEquals("Caption", "Cannot amend transaction", Caption);
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction_OriginalTransactionIsAmendedTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AmendTransactionWCreditNote);
			checkpoint.IsAllowed = true;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
			testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_SellGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertEquals("Original transaction", invoiceWithLine.PK, creditNote.OriginalTransaction.PK);
			Assert(Message.IsEmpty);
			Assert(Caption.IsEmpty);

			creditNote.Factory.Save();
			var creditNote1 = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, (TransactionHeader)creditNote, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote1);
			AssertEquals("Message", "Cannot amend selected transaction as this is an amendment transaction.", Message);
			AssertEquals("Caption", "Cannot amend transaction", Caption);
		}

		[SuspendCriticalValidation]
		public void TestAmendAPTransaction_OriginalTransactionIsAmendedTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.APAmendWithCreditNote);
			checkpoint.IsAllowed = true;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 100M, testObjectCreator.Creditor1, testObjectCreator.AUD, -100, null);
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_CostGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Creditor1.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendAPTransaction(invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertEquals("Original transaction", invoiceWithLine.PK, creditNote.OriginalTransaction.PK);
			Assert(Message.IsEmpty);
			Assert(Caption.IsEmpty);

			creditNote.Factory.Save();
			var creditNote1 = CreditNoteAmendingHelper.AmendAPTransaction((TransactionHeader)creditNote, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote1);
			AssertEquals("Message", "Only AP Invoice can be amended.", Message);
			AssertEquals("Caption", "Cannot amend transaction", Caption);
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction_CopyRevRecognitionTypeForOriginalTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			invoiceWithLine.Lines[0].AL_RevRecognitionType = "ABC";
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertEquals("Original transaction", invoiceWithLine.PK, creditNote.OriginalTransaction.PK);
			AssertEquals("ABC", ((InvoicingBase)creditNote).Lines[0].AL_RevRecognitionType);
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AmendTransactionWCreditNote);
			checkpoint.IsAllowed = true;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
			testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_SellGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertEquals("Original transaction", invoiceWithLine.PK, creditNote.OriginalTransaction.PK);
			Assert(Message.IsEmpty);
			Assert(Caption.IsEmpty);
		}

		[SuspendCriticalValidation]
		public void TestAmendAPTransaction()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.APAmendWithCreditNote);
			checkpoint.IsAllowed = true;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 100M, testObjectCreator.Creditor1, testObjectCreator.AUD, -100, null);
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_CostGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Creditor1.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendAPTransaction(invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertEquals("Original transaction", invoiceWithLine.PK, creditNote.OriginalTransaction.PK);
			Assert(Message.IsEmpty);
			Assert(Caption.IsEmpty);
		}

		[SuspendCriticalValidation]
		public void TestAmendAPTransaction_NoSecurityRights()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.APAmendWithCreditNote);
			checkpoint.IsAllowed = false;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 100M, testObjectCreator.Creditor1, testObjectCreator.AUD, -100, null);
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_CostGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Creditor1.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendAPTransaction(invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Message", SecurityCore.APAmendWithCreditNote, Message);
			Assert(Caption.IsEmpty);
		}

		[SuspendCriticalValidation]
		public void TestAmendAPTransaction_PreventAmendAPCreditNote()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.APAmendWithCreditNote);
			checkpoint.IsAllowed = true;

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 100M, testObjectCreator.Creditor1, testObjectCreator.AUD, -100, null);
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			charge.JR_AT_CostGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(APCreditNote), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Creditor1.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendAPTransaction(invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Caption", "Cannot amend transaction", Caption);
			AssertEquals("Message", "Only AP Invoice can be amended.", Message);
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction_VietnamEInvoicing()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
				var shipment = testObjectCreator.CreateShipment("S00001000");
				var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
				testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
				Factory.Save();

				var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
				invoiceWithLine.Lines[0].AL_JH = job.PK;

				var pivot = testObjectCreator.CreateEInvoicingTransactionPivot(invoiceWithLine);

				Factory.Save();

				var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
				CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
					(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

				AssertEquals("Message", "Invoice cannot be amended until it has been submitted with status 'SUC'.", Message);
			}
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction_EInvoicingTransactionValidation()
		{
			var eInvoicingValidationMock = new Mock<IEInvoicingTransactionValidation>();
			var extensionFactory = new Mock<ICountryComplianceEInvoicingExtensionFactory>();
			ObjectFactory.Substitute(extensionFactory.Object);
			extensionFactory.Setup(x => x.GetIEInvoicingTransactionValidation(GlbCompany.CurrentCompany.Country.Code)).Returns(() => eInvoicingValidationMock.Object);

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
			testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
			invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
			invoiceWithLine.AH_TransactionCategory = "FIN";
			invoiceWithLine.AH_JH = job.PK;
			invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var pivot = testObjectCreator.CreateEInvoicingTransactionPivot(invoiceWithLine, status: EInvoicingPivotState.Succeed);
			Factory.Save();

			var expectedError = "EInvoicing validation error message.";
			eInvoicingValidationMock.Setup(x => x.GetCantAmendErrorMessage(invoiceWithLine, TransactionTypes.CreditNote)).Returns(() => expectedError);

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Message", expectedError, Message);
			AssertEquals("Caption", "Cannot amend transaction", Caption);

			SetMessageAndCaption(ZString.Empty, ZString.Empty);
			eInvoicingValidationMock.Setup(x => x.GetCantAmendErrorMessage(invoiceWithLine, TransactionTypes.CreditNote)).Returns(() => ZString.Empty);
			creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertEquals("Original transaction", invoiceWithLine.PK, creditNote.OriginalTransaction.PK);
			Assert(Message.IsEmpty);
			Assert(Caption.IsEmpty);
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction_PreventAmendingInvoiceWithCreditNoteIsRegistryOn()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
			testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Caption", "Cannot amend transaction", Caption);
			AssertEquals("Message", "Cannot amend selected transaction with Credit Note as Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Creation of Credit Notes.", Message);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Factory.Save();

			AssertNoExceptionThrown(() => CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
					(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption)));
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction_PreventAmendingCreditNoteWithCreditNoteIsRegistryOn()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
			testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Factory.Save();

			var creditNoteWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			creditNoteWithLine.AH_OH = testObjectCreator.Debtor.PK;
			creditNoteWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
			creditNoteWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, creditNoteWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Caption", "Cannot amend transaction", Caption);
			AssertEquals("Message", "Cannot amend selected transaction with Credit Note as Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> Receivable Defaults -> Default Settings -> Prevent Creation of Credit Notes.", Message);

			AccountingMasterFilesRegistry.Instance.ReceivablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Factory.Save();

			AssertNoExceptionThrown(() => CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, creditNoteWithLine, securityHelper,
					(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption)));
		}

		[SuspendCriticalValidation]
		public void TestAmendAPTransaction_PreventAmendingInvoiceWithCreditNoteIsRegistryOn()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 100M, testObjectCreator.Creditor1, testObjectCreator.AUD, -100, null);
			testObjectCreator.Creditor1.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_OH = testObjectCreator.Creditor1.PK;
			invoiceWithLine.Lines[0].AL_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);

			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			var creditNote = CreditNoteAmendingHelper.AmendAPTransaction(invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNull(creditNote);
			AssertEquals("Caption", "Cannot amend transaction", Caption);
			AssertEquals("Message", "Cannot amend selected transaction with Credit Note as Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> Payable Defaults -> Default Settings -> Prevent Creation of Credit Notes.", Message);

			AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Factory.Save();

			AssertNoExceptionThrown(() => CreditNoteAmendingHelper.AmendAPTransaction(invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption)));
		}

		public void TestAmendARTransaction_CorrespondInvoiceHasNoComplianceDocument()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
				var shipment = testObjectCreator.CreateShipment("S00001000");
				var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

				var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AmendTransactionWCreditNote);
				checkpoint.IsAllowed = true;

				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, 100, testObjectCreator.Debtor);
				testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
				charge.JR_AT_SellGSTRate = testObjectCreator.GST1.PK;
				Factory.Save();

				var poster = new ChargePoster(Factory);
				var invoice = poster.Post(charge);
				Factory.Save();
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
				var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoice, securityHelper,
					(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

				AssertEquals("Please create compliance document record and allocate compliance document number before proceeding to amend with credit note.", Message);
				AssertEquals("Cannot amend transaction", Caption);

				charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, 100, testObjectCreator.Debtor);
				testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
				Factory.Save();

				var invoice1 = poster.Post(charge);
				Factory.Save();

				AssertNoExceptionThrown(() => CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoice1, securityHelper,
					(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption)));
			}
		}

		public void TestAmendARTransaction_PreventAmendingInvoiceWithoutSUCEReportStatusWhenTXE()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Taiwan))
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
				var shipment = testObjectCreator.CreateShipment("S00001000");
				var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

				var checkpoint = Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AmendTransactionWCreditNote);
				checkpoint.IsAllowed = true;

				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
				testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
				charge.JR_AT_SellGSTRate = testObjectCreator.CC1.AC_AT_GSTRate;
				Factory.Save();
				AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				var invoice = testObjectCreator.CreateARInvoice<ARInvoice>("00001018", testObjectCreator.AUD, 1, testObjectCreator.Debtor) as InvoicingBase;
				invoice.Lines.RemoveAndDeleteAll();
				var invoiceLine = testObjectCreator.CreateARInvoiceLine(invoice as ARInvoice, null, testObjectCreator.FRT, testObjectCreator.AUD, 1, "desc", 100) as InvoicingLineBase;
				invoiceLine.AL_AT = testObjectCreator.CAP.PK;
				new ComplianceDocumentCreator(new[] { invoice }, OrganisationCreateComplianceDocumentOnPostingTypes.RollupByCharge).CreateComplianceDocumentRecords();
				Factory.Save();

				var header = invoice.GetTransactionGeneratedComplianceDocument();
				header.ADH_ComplianceSubType = TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE;
				var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
				pivot.AIP_ParentID = header.PK;
				pivot.AIP_Status = EInvoicingPivotState.Queued;
				pivot.AIP_ParentTableCode = AccComplianceDocumentHeaderSchema.Constants.Prefix;
				Factory.Save();

				var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
				var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoice, securityHelper,
					(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

				AssertEquals("Credit Note can only be created after the Original TXE documents have been successfully uploaded (i.e., E-Reporting Status = SUC).", Message);
				AssertEquals("Cannot amend transaction", Caption);
			}
		}

		public void TestAmendAPTransaction_BranchAndDepartmentCopiedCorrectlyFromInvoiceHeader()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var branch = testObjectCreator.CreateBranch("BCH", GlbCompany.CurrentCompany);
			var department = testObjectCreator.CreateDepartment("DEP");

			var shipment = testObjectCreator.CreateShipment("S00001000");
			var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);
			Factory.Save();

			var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
			invoiceWithLine.AH_GB = branch.PK;
			invoiceWithLine.AH_GE = department.PK;
			AssertNotEquals(invoiceWithLine.AH_GB, GlbBranch.CurrentBranch);
			AssertNotEquals(invoiceWithLine.AH_GE, GlbDepartment.CurrentDepartment);
			invoiceWithLine.AH_JH = job.PK;
			Factory.Save();

			var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
			var creditNote = CreditNoteAmendingHelper.AmendAPTransaction(invoiceWithLine, securityHelper,
				(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

			AssertNotNull(creditNote);
			var transactionHeader = (InvoicingBase)creditNote;
			AssertEquals(transactionHeader.AH_GB, branch.PK);
			AssertEquals(transactionHeader.AH_GE, department.PK);
		}

		void SetMessageAndCaption(ZString message, ZString caption)
		{
			Message = message;
			Caption = caption;
		}

		ZString Message { get; set; }
		ZString Caption { get; set; }
	}
}
