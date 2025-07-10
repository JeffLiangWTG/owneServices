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
		[TestDate(2024, 03, 23, 23, 15, 15)]
		public void TestAPPaymentVisit_ValidationErrorWhenReversingAPPayment()
		{
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.SetupPeriods();
			
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader2.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			charge1.JR_IsAPCashAdvance = true;
			charge1.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;
			Factory.Save();

			charge1.JR_OSCostAmt = 200M;
			Factory.Save();

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1Reloaded);
			var mj1 = Factory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1Reloaded.PK));
			AssertNotNull(mj1);
			var payment = ObjectCreator.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1Reloaded.MarkAsPaid(true);
			Factory.Save();

			var invoice = GetAPInvoice(charge1);
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);

			var journals = GetAPCAIJournals();
			AssertEquals("Reverse Journnal Count", 1, journals.Length);

			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Creditor1.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment.AH_TransactionNum}/{payment.AH_ChequeOrReference}"), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cal1.CAL_OSPaidAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cal1.CAL_LocalPaidAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			Assert(FormattableString.Invariant($"{nameof(journal.AH_PostDate)} - Expected: {invoice.AH_PostDate} - Actual: {journal.AH_PostDate}"), invoice.AH_PostDate == journal.AH_PostDate);
			Assert(FormattableString.Invariant($"{nameof(journal.AH_InvoiceDate)} - Expected: {TestDateAttribute.Date} - Actual: {journal.AH_InvoiceDate}") , TestDateAttribute.Date == journal.AH_InvoiceDate);

			var errorMessages = new Dictionary<string, List<string>>();
			var validator = new TransactionWithCashAdvanceRequestValidationVisitor(errorMessages);
			validator.Visit(payment);
			AssertEquals("Expect no error when Invoice Total is less than the Advance Payment Paid", 1, errorMessages.Count);
			AssertEquals(FormattableString.Invariant($"Payment cannot be reversed as it is linked to Advance Payment Request [{cah1.CAH_RequestReferenceNumber}] on Job #[{cah1.Job.JH_JobNum}] which has costs posted for one or more of the Advance Payment lines. Please reverse the invoice before reversing the payment"), errorMessages.First().Value.First());
		}

		APJournal[] GetAPCAIJournals()
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, TransactionCategory.Codes.CashAdvanceInvoice);
			return Factory.Load<APJournal>(query);
		}
	}
}
