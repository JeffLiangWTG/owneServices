using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.DataTransfer.Integration.Testing
{
	public class TransactionPaymentStatusTest : TestCaseWithFactory
	{
		public void TestGetTransactionPaymentStatus()
		{
			MockCreditLimitServiceClientProvider.Reset();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_IsDebtor = true;

			TransactionPaymentStatus status = new TransactionPaymentStatus(org.OH_Code);

			AssertEquals("Outstanding Amount", 40.00m, status.OutstandingAmount(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00001000", ""));
			AssertEquals("Payment Status", "PARTPAID", status.PaymentStatus(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00001000", ""));
			DateTime? date = status.FullyPaidDate(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00001000", "");
			AssertEquals("Fully Paid Date Has Value", false, date.HasValue);

			AssertEquals("Outstanding Amount", 100.00m, status.OutstandingAmount(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00002000", ""));
			AssertEquals("Payment Status", "UNPAID", status.PaymentStatus(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00002000", ""));
			date = status.FullyPaidDate(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00002000", "");
			AssertEquals("Fully Paid Date Has Value", false, date.HasValue);

			AssertEquals("Outstanding Amount", 0.00m, status.OutstandingAmount(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00003000", ""));
			AssertEquals("Payment Status", "PAID", status.PaymentStatus(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00003000", ""));
			date = status.FullyPaidDate(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00003000", "");
			AssertEquals("Fully Paid Date Has Value", true, date.HasValue);
			AssertEquals("Fully Paid Date", new DateTime(2010, 10, 31), date.Value);

			AssertEquals("Outstanding Amount", 0.00m, status.OutstandingAmount(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "", "S00001000"));
			AssertEquals("Payment Status", "PAID", status.PaymentStatus(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "", "S00001000"));
			date = status.FullyPaidDate(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "", "S00001000");
			AssertEquals("Fully Paid Date Has Value", true, date.HasValue);
			AssertEquals("Fully Paid Date", new DateTime(2010, 11, 30), date.Value);

			try
			{
				decimal amount = status.OutstandingAmount(LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00004000", "");
				Assert("Should have thrown exception", false);
			}
			catch (TransactionNotFoundException ex)
			{
				AssertEquals("Exception message", string.Format("A transaction was not found in the remote system for Company Code:'{0}' Organisation Code:'{1}' Ledger:'{2}' TransactionType:'{3}' TransactionNumber:'{4}' Job Transaction Number:'{5}'",
																	GlbCompany.CurrentCompany.GC_Code, org.OH_Code, LedgerTypes.AccountsReceivable, TransactionTypes.Invoice, "00004000", ""), ex.Message);
			}

			AssertEquals("Calls to CheckTransactionPaymentStatus", 5, MockCreditLimitServiceClientProvider.CountCheckTransactionPaymentStatusWasInvoked);
		}
	}
}
