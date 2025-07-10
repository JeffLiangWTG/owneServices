using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	[TestedType(typeof(eNettPaymentDataAdapter))]
	sealed class eNettPaymentDataAdapterTest : ARAPPaymentDataAdapterTest
	{
		#region Exporting

		public void TestExportExcludingNonInvoiceTransactionTypeTransactions()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			TestObjectCreator creator = new TestObjectCreator(testDataFactory);
			Job job = creator.CreateJob("S00001001", creator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			AccChargeCode chargeCode = creator.CC1;
			testDataFactory.Save();

			APInvoice invoice = testDataFactory.New<APInvoice>();
			invoice.SubmittedFromInvoicingForm = true;
			invoice.AH_OH = creator.ABIGAS.PK;
			invoice.AH_RX_NKTransactionCurrency = creator.AUD.RX_Code;
			invoice.AH_ExchangeRate = 1m;
			invoice.AH_TransactionNum = "INVOICE";
			APInvoiceLine line1 = (APInvoiceLine)invoice.Lines.AddNew();
			line1.GenericCharge = chargeCode.PK;
			line1.AL_JH = job.PK;
			line1.AL_OSExTaxAmount = 50m;

			APCreditNote creditNote = testDataFactory.New<APCreditNote>();
			creditNote.SubmittedFromInvoicingForm = true;
			creditNote.AH_OH = creator.ABIGAS.PK;
			creditNote.AH_RX_NKTransactionCurrency = creator.AUD.RX_Code;
			creditNote.AH_ExchangeRate = 1m;
			creditNote.AH_TransactionNum = "CREDITNOTE";
			APCreditNoteLine line2 = (APCreditNoteLine)creditNote.Lines.AddNew();
			line2.GenericCharge = chargeCode.PK;
			line2.AL_JH = job.PK;
			line2.AL_OSExTaxAmount = 20m;

			APReceipt testAPRec = testDataFactory.NewWithValidTestData<APReceipt>();
			testAPRec.AH_OH = creator.ABIGAS.PK;
			testAPRec.AH_LocalExTaxAmount = 50m;
			testAPRec.AH_OSExTaxAmount = 50m;
			testAPRec.AH_Desc = "desc";

			Factory.Save();

			APPayment payment = testDataFactory.New<APPayment>();
			payment.AH_OH = creator.ABIGAS.PK;
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.eNettDirectDebit;
			payment.AH_AB = creator.AUDBankAccount.PK;
			payment.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;
			payment.ExchangeRate.Rate = 1.00m;
			payment.AH_ChequeOrReference = "ENETTREF";
			payment.AH_OSExTaxAmount = 80m;

			MatchingBase paymentMatcher = payment.MatchingBaseObject;
			paymentMatcher.UnmatchedTransactions.Load();
			paymentMatcher.MoveFromUnmatchToMatch(new BusinessObject[] { invoice, creditNote, testAPRec });
			paymentMatcher.MatchedTransactions.SetPartialPaidAmount();

			paymentMatcher.MatchAndClearTransactions();

			testDataFactory.Save();

			APPayment paymentInNewFactory = Factory.Load<APPayment>(payment.PK);

			eNettPaymentDataAdapter adapter = new eNettPaymentDataAdapter();
			Xsd.TxnHeader xmlPayment = adapter.ExportToValueObject(paymentInNewFactory, new ValueObjectExportContext(new NotificationBuffer()));

			AssertEquals("Paid Transactions Count should be 1", 1, xmlPayment.PaidTransactions.Count);
		}

		#endregion
	}
}
