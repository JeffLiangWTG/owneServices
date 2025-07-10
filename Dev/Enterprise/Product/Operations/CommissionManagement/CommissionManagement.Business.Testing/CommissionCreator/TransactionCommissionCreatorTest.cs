using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.OpeningReceipt;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business.Testing
{
	public class TransactionCommissionCreatorTest : TestCaseWithFactory
	{
		public void TestGetIsCommissionableInvoiceQuery_OnlyReturnsTransactionThatAreCommissionable()
		{
			var transactionTypesToTest =
				new[]
				{
					typeof(ARInvoice),
					typeof(APInvoice),
					typeof(UAInvoice),
					typeof(ARCreditNote),
					typeof(APCreditNote),
					typeof(UACreditNote),
					typeof(ARAdjustmentNote),
					typeof(APAdjustmentNote),
					typeof(APJournal),
					typeof(ARReceipt),
					typeof(OpeningReceipt),
					typeof(JobRevenueJournal),
				};

			List<AccTransactionHeader> expected = new List<AccTransactionHeader>();

			var typeDecider = new TransactionHeaderTypeDecider();
			foreach (var transactionType in transactionTypesToTest)
			{
				var transaction = (AccTransactionHeader)Factory.New(transactionType);
				if ((transaction is InvoicingBase && ((InvoicingBase)transaction).IsCommissionable)
					|| (transaction is JobRevenueJournal))
				{
					expected.Add(transaction);
				}
			}

			var actual = Factory.Load<TransactionHeader>(TransactionCommissionCreator.GetIsCommissionableTransactionQuery());
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<AccTransactionHeader>.PKOnlyComparer, x => x.GetType().ToString(), expected, actual);
		}
	}
}
