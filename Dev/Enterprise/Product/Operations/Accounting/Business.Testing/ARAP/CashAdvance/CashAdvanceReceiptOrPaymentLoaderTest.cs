using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public class CashAdvanceReceiptOrPaymentLoaderTest : TestCaseWithFactory
	{
		public void TestARCashAdvanceReceiptsLoader()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(job, ObjectCreator.Debtor, LedgerTypes.AccountsReceivable, 250M, 250M, "AUD");
			var cahReloaded = Factory.Load<CashAdvanceRequestHeader>(cah.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cahReloaded);

			var matchingJournal = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cahReloaded.PK));
			AssertNotNull(matchingJournal);
			var receipt = ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(matchingJournal);
			Factory.Save();

			var receiptDetailsLoader = new CashAdvanceReceiptOrPaymentLoader(cahReloaded);
			var receiptDetails = receiptDetailsLoader.Get();
			AssertEquals("Receipt info count", 1, receiptDetails.Count);

			var receiptDet = receiptDetails[0];
			AssertEquals("ChequeOrReferenceNumber", receipt.AH_ChequeOrReference, receiptDet.ChequeOrReferenceNumber);
			AssertEquals("ReceiptNumber", receipt.AH_TransactionNum, receiptDet.TransactionNumber);
			AssertEquals("ReceiptPK", receipt.PK, receiptDet.TransactionPK);
		}

		public void TestAPCashAdvancePaymentLoader()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(job, ObjectCreator.Debtor, LedgerTypes.AccountsPayable, 250M, 250M, "AUD");
			var cahReloaded = Factory.Load<CashAdvanceRequestHeader>(cah.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cahReloaded);

			var matchingJournal = Factory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cahReloaded.PK));
			AssertNotNull(matchingJournal);
			var payment = ObjectCreator.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(matchingJournal);
			Factory.Save();

			var paymentDetailsLoader = new CashAdvanceReceiptOrPaymentLoader(cahReloaded);
			var paymentDetails = paymentDetailsLoader.Get();
			AssertEquals("Payment info count", 1, paymentDetails.Count);

			var paymentDet = paymentDetails[0];
			AssertEquals("ChequeOrReferenceNumber", payment.AH_ChequeOrReference, paymentDet.ChequeOrReferenceNumber);
			AssertEquals("ReceiptNumber", payment.AH_TransactionNum, paymentDet.TransactionNumber);
			AssertEquals("ReceiptPK", payment.PK, paymentDet.TransactionPK);
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
