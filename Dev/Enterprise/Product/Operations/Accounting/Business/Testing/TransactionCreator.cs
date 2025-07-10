#if DEBUG

using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.OpeningPayment;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public class TransactionCreator : ITransactionCreator
	{
		#region Implement ITransactionCreator

		public AccTransactionHeader CreateTransaction(BusinessObjectFactory factory, string ledger, string transactionType, Guid? initialPK = null, int numberOfLines = 1)
		{
			var testObjectCreator = new TestObjectCreator(factory, true);
			AccTransactionHeader result = null;

			PrepareTransactionEnvironment(factory, testObjectCreator, ledger);

			// AP / AR
			if (ledger == LedgerTypes.AccountsPayable || ledger == LedgerTypes.AccountsReceivable)
			{
				// Avoid CA1502 warning.
				result = TryCreateAPARTransactions(testObjectCreator, ledger, transactionType, numberOfLines);
			}

			// CB
			else if (ledger == LedgerTypes.CashBook && transactionType == TransactionTypes.DirectPayment)
			{
				result = testObjectCreator.CreateDirectPayment(postDate, 100m, 0m, 80m, 0m);
				//Avoid CA1800 warning.
				var directTransaction = (DirectPayment)result;
				directTransaction.ChequeBookPK = chequeBook.PK;
				directTransaction.Lines[0].AL_AT = taxRate.PK;
				directTransaction.Lines[1].AL_AT = taxRate.PK;
			}
			else if (ledger == LedgerTypes.CashBook && transactionType == TransactionTypes.DirectReceipt)
			{
				result = testObjectCreator.CreateDirectReceipt(postDate, 100m, 0m, 80m, 0m);
				var directTransaction = (DirectReceipt)result;
				directTransaction.ChequeBookPK = chequeBook.PK;
				directTransaction.Lines[0].AL_AT = taxRate.PK;
				directTransaction.Lines[1].AL_AT = taxRate.PK;
			}
			else if (ledger == LedgerTypes.CashBook && transactionType == TransactionTypes.OpeningPayment)
			{
				result = factory.NewWithValidTestData<OpeningPayment>();
				var openingPayment = (OpeningPayment)result;
				openingPayment.AH_OH = org.PK;
				openingPayment.AH_AB = testObjectCreator.AUDBankAccount.PK;
				openingPayment.AH_OSExTaxAmount = 100M;
				openingPayment.AH_ChequeOrReference = "001";
			}
			else if (ledger == LedgerTypes.CashBook && transactionType == TransactionTypes.OpeningReceipt)
			{
				result = factory.NewWithValidTestData<OpeningReceipt>();
				var openingReceipt = (OpeningReceipt)result;
				openingReceipt.AH_OH = org.PK;
				openingReceipt.AH_AB = testObjectCreator.AUDBankAccount.PK;
				openingReceipt.AH_OSExTaxAmount = 100M;
				openingReceipt.AH_ChequeOrReference = "001";
			}

			// GL
			else if (ledger == LedgerTypes.General && (
				transactionType == TransactionTypes.GLStandardJournal ||
				transactionType == TransactionTypes.GLReversingJournal ||
				transactionType == TransactionTypes.GLAutoJournal ||
				transactionType == TransactionTypes.GLNoteJournal))
			{
				var glHeader1 = transactionType == TransactionTypes.GLNoteJournal ? testObjectCreator.GLHeaderNTE1.PK : testObjectCreator.GLHeader1.PK;
				var glHeader2 = transactionType == TransactionTypes.GLNoteJournal ? testObjectCreator.GLHeaderNTE2.PK : testObjectCreator.GLHeader2.PK;

				result = testObjectCreator.CreateGLJournal(transactionType, postDate, postDate, postDate.AddMonths(1));
				var journal = (GLJournal)result;
				testObjectCreator.CreateGLJournalLine(journal, 100m, DebitCredit.DR, glHeader1);
				testObjectCreator.CreateGLJournalLine(journal, 100m, DebitCredit.CR, glHeader2);
			}

			// JC
			else if (ledger == LedgerTypes.JobCosting && transactionType == TransactionTypes.Journal)
			{
				var job = testObjectCreator.CreateJob(org, 10m, testObjectCreator.ABIGAS, 11m);
				result = testObjectCreator.CreateJCJournalHeader(postDate, 100m);
				testObjectCreator.CreateJCJournalLine((JCJournalHeader)result, chargeCode, job, postDate, 100m, false);
			}
			else if (ledger == LedgerTypes.JobCosting && transactionType == TransactionTypes.JobRevenueJournal)
			{
				var job = testObjectCreator.CreateJob(org, 10m, testObjectCreator.ABIGAS, 11m);
				result = testObjectCreator.CreateJobRevenueJournal(chargeCode, job, 100m);
				var journal = (JobRevenueJournal)result;
				journal.Lines[0].AL_GE = department.PK;
				journal.Lines[1].AL_GE = department.PK;
			}

			// UA
			if (ledger == LedgerTypes.UnapprovedPayableTransactions && transactionType == TransactionTypes.UAInvoice)
			{
				result = testObjectCreator.CreateAPInvoiceForApprovalRequest<UAInvoice>(org, 100m, transactionNum);
			}
			else if (ledger == LedgerTypes.UnapprovedPayableTransactions && transactionType == TransactionTypes.UACreditNote)
			{
				result = testObjectCreator.CreateAPInvoiceForApprovalRequest<UACreditNote>(org, 100m, transactionNum);
			}

			return result;
		}

		public AccTransactionHeader CreateInvoiceBatch(BusinessObjectFactory factory, params AccTransactionHeader[] headers)
		{
			InvoiceBatchHeader testHeader = factory.NewWithValidTestData<InvoiceBatchHeader>();

			foreach (var header in headers)
			{
				testHeader.Line.Add(header);
			}

			return testHeader;
		}

		#endregion

		#region Implementation
		
		OrgHeader CreateFullOrgSafe(BusinessObjectFactory factory, string code)
		{
			var debtor = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, code));
			if (debtor == null)
			{
				var newFactory = new BusinessObjectFactory();
				debtor = newFactory.New<OrgHeader>();
				debtor.OH_FullName = "Test Company Name";
				debtor.MainAddress.OA_Address1 = "185 Bourke Road";
				debtor.MainAddress.OA_City = "Alexandria";
				debtor.MainAddress.OA_State = "NSW";

				debtor.OH_IsDebtor = true;
				debtor.OH_IsCreditor = true;
				debtor.CompanyData.SetAPTaxApplicable(true);
				debtor.MiscServ.OM_APWHTApplicable = true;
				debtor.CompanyData.SetARTaxApplicable(true);
				debtor.MiscServ.OM_ARWHTApplicable = true;

				debtor.OH_Code = code;
				newFactory.Save();
				debtor = factory.Load<OrgHeader>(debtor.PK);
			}

			return debtor;
		}

		void PrepareTransactionEnvironment(BusinessObjectFactory factory, TestObjectCreator testObjectCreator, string ledger)
		{
			// When add new properties here, please make sure they have Load-Before-Create protection.
			org = CreateFullOrgSafe(factory, "ZOrg01");
			transactionNum = Guid.NewGuid().ToString();
			postDate = ZDateTime.Today;
			chargeCode = testObjectCreator.CC1;
			currency = testObjectCreator.AUD;
			taxRate = testObjectCreator.GSTFREE1;
			department = testObjectCreator.FEADepartment;

			if (ledger == LedgerTypes.AccountsPayable)
			{
				glHeaderDR = testObjectCreator.CreateAccGLHeader("9900.98.99", "TS", "Test GL", AccountType.ProfitAndLossAccount, DebitCreditDataEntry.DR);
			}

			if (ledger == LedgerTypes.AccountsReceivable)
			{
				glHeaderCR = testObjectCreator.CreateAccGLHeader("9900.99.99", "TS", "Test GL", AccountType.ProfitAndLossAccount, DebitCreditDataEntry.CR);
			}

			if (ledger == LedgerTypes.CashBook)
			{
				chequeBook = testObjectCreator.AUDChequeBook;
			}

			if (ledger == LedgerTypes.JobCosting)
			{
				var rjGLHeader = testObjectCreator.CreateJobRevenueJournalControlAccount();
				AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rjGLHeader.PK.ToGuid());
			}
		}

		AccTransactionHeader TryCreateAPARTransactions(TestObjectCreator testObjectCreator, string ledger, string transactionType, int numberOfLines)
		{
			AccTransactionHeader result = null;
			decimal totalAmount = 100m;
			decimal lineAmount = numberOfLines == 0m ? 0m : totalAmount / numberOfLines;
			decimal exchangeRate = 1m;
			var desc = "Test Desc";

			// AP
			if (ledger == LedgerTypes.AccountsPayable && transactionType == TransactionTypes.Invoice)
			{
				result = testObjectCreator.CreateInvoice(typeof(APInvoice), transactionNum, currency, exchangeRate);
				result.AH_OH = org.PK;

				for (int i = 0; i < numberOfLines; i++)
				{
					var line = testObjectCreator.CreateInvoiceLine((APInvoice)result, currency, exchangeRate, totalAmount, 0m, 0m, totalAmount, 0m, 0m);
					line.GenericCharge = glHeaderDR.PK;
					line.AL_AT = taxRate.PK;
				}
			}
			else if (ledger == LedgerTypes.AccountsPayable && transactionType == TransactionTypes.AdjustmentNote)
			{
				result = testObjectCreator.CreateAdjustmentNote<APAdjustmentNote>(transactionNum, totalAmount, 0m, postDate, org.PK);

				for (int i = 0; i < numberOfLines; i++)
				{
					var line = testObjectCreator.CreateAdjusmentNoteLine((APAdjustmentNote)result, ZGuid.Empty, lineAmount, 0m);
					line.GenericCharge = glHeaderDR.PK;
					line.AL_AT = taxRate.PK;
				}
			}
			else if (ledger == LedgerTypes.AccountsPayable && transactionType == TransactionTypes.CreditNote)
			{
				result = testObjectCreator.CreateAPCreditNote(transactionNum, org, currency, exchangeRate, desc, postDate.AddMonths(1), false);

				for (int i = 0; i < numberOfLines; i++)
				{
					var line = testObjectCreator.CreateAPCreditNoteLine((APCreditNote)result, null, chargeCode, currency, exchangeRate, desc, lineAmount);
					line.AL_AC = ZGuid.Empty;
					line.GenericCharge = glHeaderDR.PK;
					line.AL_AT = taxRate.PK;
				}

				((APCreditNote)result).Lines.Cast<InvoicingLineBase>().ForEach(x => x.AL_ReverseToGL = "N");
			}
			else if (ledger == LedgerTypes.AccountsPayable && transactionType == TransactionTypes.Payment)
			{
				result = testObjectCreator.CreateAPPayment(1m, 0m, ZDateTime.Today, ZDateTime.Today, testObjectCreator.ActiveOrg.PK, testObjectCreator.AUDBankAccount.PK);
			}

			// AR
			else if (ledger == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Invoice)
			{
				result = testObjectCreator.CreateInvoice(typeof(ARInvoice), transactionNum, currency, exchangeRate);
				result.AH_OH = org.PK;

				for (int i = 0; i < numberOfLines; i++)
				{
					var line = testObjectCreator.CreateInvoiceLine((ARInvoice)result, currency, exchangeRate, totalAmount, 0m, 0m, totalAmount, 0m, 0m);
					line.GenericCharge = glHeaderCR.PK;
					line.AL_AT = taxRate.PK;
				}
			}
			else if (ledger == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.AdjustmentNote)
			{
				result = testObjectCreator.CreateAdjustmentNote<ARAdjustmentNote>(transactionNum, 100m, 10m, postDate, org.PK);

				for (int i = 0; i < numberOfLines; i++)
				{
					var line = testObjectCreator.CreateAdjusmentNoteLine((ARAdjustmentNote)result, ZGuid.Empty, 100m, 10m);
					line.GenericCharge = glHeaderCR.PK;
					line.AL_AT = taxRate.PK;
				}
			}
			else if (ledger == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.CreditNote)
			{
				result = testObjectCreator.CreateARCreditNote(transactionNum, org, currency, exchangeRate, desc, postDate.AddMonths(1), false);

				for (int i = 0; i < numberOfLines; i++)
				{
					var line = testObjectCreator.CreateARCreditNoteLine((ARCreditNote)result, null, chargeCode, lineAmount, currency, exchangeRate, desc);
					line.AL_AC = ZGuid.Empty;
					line.GenericCharge = glHeaderCR.PK;
					line.AL_AT = taxRate.PK;
				}

				((ARCreditNote)result).Lines.Cast<InvoicingLineBase>().ForEach(x => x.AL_ReverseToGL = "N");
			}
			else if (ledger == LedgerTypes.AccountsReceivable && transactionType == TransactionTypes.Receipt)
			{
				result = testObjectCreator.CreateARReceipt(exchangeRate, totalAmount, ZDateTime.Today, ZDateTime.Today, testObjectCreator.ActiveOrg.PK, testObjectCreator.AUDBankAccount.PK);
			}

			return result;
		}

		OrgHeader org;
		string transactionNum;
		ZDateTime postDate;
		AccChargeCode chargeCode;
		RefCurrency currency;
		AccTaxRate taxRate;
		GlbDepartment department;
		AccGLHeader glHeaderCR;
		AccGLHeader glHeaderDR;
		AccChequeBook chequeBook;

		#endregion
	}
}

#endif
