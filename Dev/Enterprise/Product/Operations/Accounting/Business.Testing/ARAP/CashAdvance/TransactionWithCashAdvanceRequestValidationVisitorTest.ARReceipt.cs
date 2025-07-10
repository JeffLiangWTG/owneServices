using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	public partial class TransactionWithCashAdvanceRequestValidationVisitorTest : TestCaseWithFactory
	{
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestARReceiptVisit_ValidationErrorWhenReversingARReceipt()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();

			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal2 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 300M, 300M);
			charge2.JR_CAL_ARLine = cal2.PK;
			cal2.CAL_LocalPaidAmount = 300M;
			cal2.CAL_OSPaidAmount = 300M;
			cal2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			Factory.Save();

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1Reloaded);
			var mj1 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1Reloaded.PK));
			AssertNotNull(mj1);
			var receipt1 = ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1Reloaded.MarkAsPaid(true);
			Factory.Save();

			var invoice = GetInvoice(charge3);
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal2.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);

			var journals = GetCAIJournals();
			AssertEquals("Reverse Journnal Count", 1, journals.Length);

			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Debtor.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cal3.CAL_OSPaidAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cal3.CAL_LocalPaidAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), invoice.AH_InvoiceDate, journal.AH_InvoiceDate);

			var errorMessages = new Dictionary<string, List<string>>();
			var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessages);
			validator.Visit(receipt1);
			AssertEquals("Expect no error when Invoice Total is less than the Advance Payment Paid", 1, errorMessages.Count);
			AssertEquals(FormattableString.Invariant($"Receipt cannot be reversed as it is linked to Advance Payment Request [{cah1.CAH_RequestReferenceNumber}] on Job #[{cah1.Job.JH_JobNum}] which has invoiced charge lines. Please reverse the invoice before reversing the receipt"), errorMessages.First().Value.First());
		}

		ARJournal[] GetCAIJournals()
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, TransactionCategory.Codes.CashAdvanceInvoice);
			return Factory.Load<ARJournal>(query);
		}
	}
}
