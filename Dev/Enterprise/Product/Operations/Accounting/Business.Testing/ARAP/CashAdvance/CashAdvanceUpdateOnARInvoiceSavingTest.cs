using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.CashAdvance.Testing
{
	[TestDate(2024, 6, 11, 00, 01, 01, 253)]
	[TestDateIncremental(seconds: 1)]
	public class CashAdvanceUpdateOnARInvoiceSavingTest : TestCaseWithFactory
	{
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdatedToPaidStatusManually()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var invoice = GetInvoice(charge3, charge4);
			Factory.Save();

			AssertEquals(true, charge2.JR_IsARCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Fully Paid", ZDecimal.Zero, invoice.AH_OutstandingAmount);
			AssertNotEquals("Fully Paid Date is Not Empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_LocalCurrency_LocalInvoiceType_NoOutstandingAmount()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

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

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var cahReloaded = Factory.Load<CashAdvanceRequestHeader>(cah4.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cahReloaded);

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1Reloaded);
			var mj1 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1Reloaded.PK));
			AssertNotNull(mj1);
			var receipt1 = ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1Reloaded.MarkAsPaid(true);
			Factory.Save();

			var cah4Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah4.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4Reloaded);
			var mj4 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4Reloaded.PK));
			AssertNotNull(mj4);
			var receipt4 = ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4Reloaded.MarkAsPaid(true);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var invoice = GetInvoice(charge3, charge4);
			invoice.AH_InvoiceDate = new ZDateTime(2023, 01, 30);
			invoice.AH_PostDate = new ZDateTime(2023, 01, 31);
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal2.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is Not Empty", new ZDateTime(2023, 01, 31), invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_LocalExTaxAmount).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "DR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), new ZDateTime(2023, 01, 31), journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDateTime.Now.Date, journal1.AH_InvoiceDate.Date);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cal3.CAL_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cal3.CAL_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2023, 01, 31), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice);
			reverser.Reverse();
			reverser.ReverseTransaction.PostDate = new ZDateTime(2023, 02, 27);
			Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);
			AssertEquals(new ZDateTime(2023, 02, 27), reversingCrd.AH_PostDate);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Reverse Journal Count", 2, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);
			AssertEquals(nameof(journal3.AH_PostDate), new ZDateTime(2023, 02, 27), journal3.AH_PostDate);
			AssertEquals(nameof(journal3.AH_InvoiceDate), ZDateTime.Now.Date, journal3.AH_InvoiceDate.Date);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), cal3.CAL_OSPaidAmount, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), cal3.CAL_LocalPaidAmount, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);
			AssertEquals(nameof(journal4.AH_PostDate), new ZDateTime(2023, 02, 27), journal4.AH_PostDate);
			AssertEquals(nameof(journal4.AH_InvoiceDate), ZDateTime.Now.Date, journal4.AH_InvoiceDate.Date);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_LocalCurrency_LocalInvoiceType_HasOutstandingAmount()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 100M, 100M);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = false;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 100M, 100M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			charge1.JR_CAL_ARLine = cal1.PK;
			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			Factory.Save();

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1Reloaded);
			var mj1 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1Reloaded.PK));
			AssertNotNull(mj1);
			var receipt1 = ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1Reloaded.MarkAsPaid(true);
			Factory.Save();

			var cah4Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah4.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4Reloaded);
			var mj4 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4Reloaded.PK));
			AssertNotNull(mj4);
			var receipt4 = ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4Reloaded.MarkAsPaid(true);
			Factory.Save();

			var invoice = GetInvoice(charge1, charge2, charge3, charge4);
			Factory.Save();

			AssertEquals(false, charge2.JR_IsARCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Not Fully Paid", 330M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is Empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_LocalExTaxAmount).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDate.Today, journal1.AH_InvoiceDate.Date);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), invoice.AH_PostDate, journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);

			//The following test the reverse process
			AssertEquals(330m, invoice.AH_OutstandingAmount);
			Assert(invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice);
			reverser.Reverse();
			Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Reverse Journal Count", 2, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_CanNotReverseWhenInvoiceMatchedWithNonCashAdvanceTransaction()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 100M, 100M);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = false;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 100M, 100M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			charge1.JR_CAL_ARLine = cal1.PK;
			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Requested, "AUD");
			Factory.Save();

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1Reloaded);
			var mj1 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1Reloaded.PK));
			AssertNotNull(mj1);
			ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1Reloaded.MarkAsPaid(true);
			Factory.Save();

			var cah4Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah4.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4Reloaded);
			var mj4 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4Reloaded.PK));
			AssertNotNull(mj4);
			ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4Reloaded.MarkAsPaid(true);
			Factory.Save();

			var invoice = GetInvoice(charge1, charge2, charge3, charge4);
			Factory.Save();

			AssertEquals(false, charge2.JR_IsARCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Not Fully Paid", 330M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is Empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals();
			AssertEquals("Reverse Journal Count", 2, journals.Length);

			//Match Invoice outstanding amount with another non cash advance journal
			var anotherJournal = ObjectCreator.CreateJournal<ARJournal>(330M, invoice.AH_PostDate, objectCreator.Debtor.PK);
			anotherJournal.DebitCreditSign = DebitCreditDataEntry.DR;
			Factory.Save();

			var matching = new ARMatchingBase(Factory);
			matching.PrimaryOrganization = ObjectCreator.Debtor.PK;
			var transactionsToMatch = new Dictionary<BusinessObject, ZDecimal>
			{
				{ invoice, invoice.AH_OutstandingAmount },
				{ anotherJournal, anotherJournal.AH_OSTotal }
			};
			matching.MoveFromUnmatchToMatch(transactionsToMatch);
			matching.MatchAndClearTransactions();

			Factory.Save();

			var reverser = new ARInvoiceReversing(invoice);
			AssertEquals("Should not be able to reverse transaction", false, reverser.CanReverseTransaction);
			AssertEquals("This transaction cannot be reversed because it has been matched with other transactions.", reverser.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusManually_LocalCurrency_AmountOverpaid()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
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

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah4.CAH_RequestReferenceNumber = "00001004";
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			cah1Reloaded.MarkAsPaid(false);
			Factory.Save();

			var cah4Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah4.PK);
			cah4Reloaded.MarkAsPaid(false);
			Factory.Save();

			charge3.JR_OSSellAmt = 100m;
			charge4.JR_OSSellAmt = 80m;
			var invoice = GetInvoice(charge3, charge4);
			invoice.AH_JH = job.PK;
			invoice.AH_PostDate = new ZDateTime(2022, 11, 11);
			invoice.AH_InvoiceDate = new ZDateTime(2022, 11, 20);
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal2.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is Not Empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 2, journals.Length);
			var sortedJournals = journals.OrderBy(j => j.AH_Desc).ToList();

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.DebitCreditSign), "DR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 300M, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 300M, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), new ZDateTime(2022, 11, 11), journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), new ZDateTime(2022, 11, 20), journal1.AH_InvoiceDate);
			AssertEquals(nameof(journal1.AH_FullyPaidDate), ZDateTime.Empty, journal1.AH_FullyPaidDate);
			AssertEquals(nameof(journal1.AH_TransactionBelongsToGroup), invoice.PK, journal1.AH_TransactionBelongsToGroup);

			var journal2 = journals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001004 / JOB S0001234", journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 70M, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 70M, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2022, 11, 11), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), new ZDateTime(2022, 11, 20), journal2.AH_InvoiceDate);
			AssertEquals(nameof(journal2.AH_FullyPaidDate), ZDateTime.Empty, journal2.AH_FullyPaidDate);
			AssertEquals(nameof(journal2.AH_TransactionBelongsToGroup), invoice.PK, journal2.AH_TransactionBelongsToGroup);

			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			//The following test the reverse process
			AssertEquals(false, invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice);
			reverser.Reverse();
			Factory.Save();

			AssertEquals(true, invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 4, journals.Length);
			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_Desc), "REVERSAL RELATED TO 00001000", journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), journal1.AH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), 300M, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), 300M, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);
			AssertEquals(nameof(journal3.AH_PostDate), ZDateTime.Now.Date, journal3.AH_PostDate.Date);
			AssertEquals(nameof(journal3.AH_InvoiceDate), ZDateTime.Now.Date, journal3.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal3.AH_FullyPaidDate), ZDateTime.Now.Date, journal3.AH_FullyPaidDate.Date);
			AssertEquals(nameof(journal3.AH_TransactionBelongsToGroup), journal1.PK, journal3.AH_TransactionBelongsToGroup);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_Desc), "REVERSAL RELATED TO 00001001", journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), journal1.AH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), 70M, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), 70M, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);
			AssertEquals(nameof(journal4.AH_PostDate), ZDateTime.Now.Date, journal4.AH_PostDate.Date);
			AssertEquals(nameof(journal4.AH_InvoiceDate), ZDateTime.Now.Date, journal4.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal4.AH_FullyPaidDate), ZDateTime.Now.Date, journal4.AH_FullyPaidDate.Date);
			AssertEquals(nameof(journal4.AH_TransactionBelongsToGroup), journal2.PK, journal4.AH_TransactionBelongsToGroup);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_NoCashAdvanceClearingAccount_ReportException()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
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

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah4.CAH_RequestReferenceNumber = "00001004";
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			cah1Reloaded.MarkAsPaid(false);
			Factory.Save();

			var cah4Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah4.PK);
			cah4Reloaded.MarkAsPaid(false);
			Factory.Save();

			charge3.JR_OSSellAmt = 100m;
			charge4.JR_OSSellAmt = 80m;
			var invoice = GetInvoice(charge3, charge4);
			invoice.AH_JH = job.PK;

			AssertExceptionThrown<ZCannotSaveException>(@"On posting this transaction, a journal to the Receivables Advance Payment Clearing Account will be created, which requires a Advance Payment Clearing Account to be recorded in the Accounting > Advance Payments > Receivables > Receivables Advance Payment Clearing Account registry.
Please ensure this registry has an account recorded and then post the transaction."
			, () =>
			{
				Factory.Save();
				Assert(!invoice.IsInDatabase);
			});
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_LocalCurrency_AmountOverpaid()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
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

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah4.CAH_RequestReferenceNumber = "00001004";
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1Reloaded);
			var mj1 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1Reloaded.PK));
			AssertNotNull(mj1);
			var receipt1 = ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1Reloaded.MarkAsPaid(true);
			Factory.Save();

			var cah4Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah4.PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4Reloaded);
			var mj4 = Factory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4Reloaded.PK));
			AssertNotNull(mj4);
			var receipt4 = ObjectCreator.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4Reloaded.MarkAsPaid(true);
			Factory.Save();

			charge3.JR_OSSellAmt = 100m;
			var invoice = GetInvoice(charge3, charge4);
			invoice.AH_JH = job.PK;
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal2.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is Not Empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var caiJournals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("Reverse Journal Count", 2, caiJournals.Length);
			var sortedJournals = caiJournals.OrderBy(j => j.AH_Desc).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 100m, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 100m, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDateTime.Now.Date, journal1.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal1.AH_FullyPaidDate), ZDateTime.Today, journal1.AH_FullyPaidDate);
			Assert(journal1.AH_TransactionBelongsToGroup.IsEmpty);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 150m, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 150m, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), invoice.AH_PostDate, journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal2.AH_FullyPaidDate), ZDateTime.Today, journal2.AH_FullyPaidDate);
			Assert(journal2.AH_TransactionBelongsToGroup.IsEmpty);

			var overpaymentJournals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, overpaymentJournals.Length);
			var overpaymentJournal = overpaymentJournals[0];
			AssertEquals(nameof(overpaymentJournal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, overpaymentJournal.AH_TransactionCategory);
			AssertEquals(nameof(overpaymentJournal.AH_OH), ObjectCreator.Debtor.PK, overpaymentJournal.AH_OH);
			AssertEquals(nameof(overpaymentJournal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", overpaymentJournal.AH_Desc);
			AssertEquals(nameof(overpaymentJournal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, overpaymentJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(overpaymentJournal.AH_OSExTaxAmount), 300m, overpaymentJournal.AH_OSExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_LocalExTaxAmount), 300m, overpaymentJournal.AH_LocalExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_AG), ObjectCreator.GLHeader1.PK, overpaymentJournal.AH_AG);
			AssertEquals(nameof(overpaymentJournal.AH_PostDate), invoice.AH_PostDate, overpaymentJournal.AH_PostDate);
			AssertEquals(nameof(overpaymentJournal.AH_InvoiceDate), ZDateTime.Now.Date, overpaymentJournal.AH_InvoiceDate.Date);
			AssertEquals(nameof(overpaymentJournal.AH_FullyPaidDate), ZDateTime.Empty, overpaymentJournal.AH_FullyPaidDate);
			AssertEquals(invoice.PK, overpaymentJournal.AH_TransactionBelongsToGroup);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("EXX transaction should NOT exist", 0, exxTransactions.Length);

			//The following test the reverse process
			AssertEquals(false, invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice);
			Assert(reverser.IsOutstandingAmountOnlyPaidViaCashAdvance);
			reverser.Reverse();
			Factory.Save();

			AssertEquals(true, invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 4, journals.Length);
			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), journal1.AH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), 100m, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), 100m, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), journal2.AH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), 150m, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), 150m, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);

			var journal5 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == overpaymentJournal.PK);
			AssertNotNull(journal5);
			AssertEquals(nameof(journal5.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal5.AH_TransactionCategory);
			AssertEquals(nameof(journal5.AH_OH), ObjectCreator.Debtor.PK, journal5.AH_OH);
			AssertEquals(nameof(journal5.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {overpaymentJournal.AH_TransactionNum}"), journal5.AH_Desc);
			AssertEquals(nameof(journal5.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal5.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal5.DebitCreditSign), "CR", journal5.DebitCreditSign);
			AssertEquals(nameof(journal5.AH_OSExTaxAmount), 300m, journal5.AH_OSExTaxAmount);
			AssertEquals(nameof(journal5.AH_LocalExTaxAmount), 300m, journal5.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal5.AH_AG), ObjectCreator.GLHeader1.PK, journal5.AH_AG);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusManually_ForeignCurrency_AmountOverpaid()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GST1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "Charge 04"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 150M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 150M, debtor: objectCreator.Debtor);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			cah1.MarkAsPaid(false);
			carJournalFactory.Save();

			var cah4 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[1].PK);
			cah4.MarkAsPaid(false);
			carJournalFactory.Save();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 3.0M);
			charge3.JR_OSSellAmt = 100m;
			charge4.JR_OSSellAmt = 80m;
			Factory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is not empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 2, journals.Length);
			var sortedJournals = journals.OrderBy(j => j.AH_Desc).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.DebitCreditSign), "DR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001000 / JOB S0001234", journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 300m, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 150m, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);
			AssertEquals(nameof(journal1.AH_FullyPaidDate), ZDateTime.Empty, journal1.AH_FullyPaidDate);
			AssertEquals(nameof(journal1.AH_TransactionBelongsToGroup), invoice.PK, journal1.AH_TransactionBelongsToGroup);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 70m, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 35m, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), invoice.AH_PostDate, journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), invoice.AH_InvoiceDate, journal2.AH_InvoiceDate);
			AssertEquals(nameof(journal2.AH_FullyPaidDate), ZDateTime.Empty, journal2.AH_FullyPaidDate);
			AssertEquals(nameof(journal2.AH_TransactionBelongsToGroup), invoice.PK, journal2.AH_TransactionBelongsToGroup);

			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			//The following test the reverse process
			AssertEquals(false, invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice as ARInvoice);
			reverser.Reverse();
			caiJournalFactory.Save();

			AssertEquals(true, invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 4, journals.Length);
			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_Desc), "REVERSAL RELATED TO 00001000", journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), journal1.AH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), 300m, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), 150m, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);
			AssertEquals(nameof(journal3.AH_PostDate), ZDateTime.Now.Date, journal3.AH_PostDate.Date);
			AssertEquals(nameof(journal3.AH_InvoiceDate), ZDateTime.Now.Date, journal3.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal3.AH_FullyPaidDate), ZDateTime.Now.Date, journal3.AH_FullyPaidDate.Date);
			AssertEquals(nameof(journal3.AH_TransactionBelongsToGroup), journal1.PK, journal3.AH_TransactionBelongsToGroup);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_Desc), "REVERSAL RELATED TO 00001001", journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), journal2.AH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), 70m, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), 35m, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);
			AssertEquals(nameof(journal4.AH_PostDate), ZDateTime.Now.Date, journal4.AH_PostDate.Date);
			AssertEquals(nameof(journal4.AH_InvoiceDate), ZDateTime.Now.Date, journal4.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal4.AH_FullyPaidDate), ZDateTime.Now.Date, journal4.AH_FullyPaidDate.Date);
			AssertEquals(nameof(journal4.AH_TransactionBelongsToGroup), journal2.PK, journal4.AH_TransactionBelongsToGroup);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_SameExRate_AmountOverpaid()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "Charge 04"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 150M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 150M, debtor: objectCreator.Debtor);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			var cah4 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[1].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4);
			var mj4 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4.PK));
			AssertNotNull(mj4);
			var receipt4 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4.MarkAsPaid(true);
			carJournalFactory.Save();

			charge3.JR_OSSellAmt = 100m;
			Factory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is not empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_Desc).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 500m, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 250m, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDateTime.Now.Date, journal1.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal1.AH_FullyPaidDate), ZDateTime.Today, journal1.AH_FullyPaidDate);
			Assert(journal1.AH_TransactionBelongsToGroup.IsEmpty);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 150m, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 75m, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), invoice.AH_PostDate, journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal2.AH_FullyPaidDate), ZDateTime.Today, journal2.AH_FullyPaidDate);
			Assert(journal2.AH_TransactionBelongsToGroup.IsEmpty);

			var overpaymentJournals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, overpaymentJournals.Length);
			var overpaymentJournal = overpaymentJournals[0];
			AssertEquals(nameof(overpaymentJournal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, overpaymentJournal.AH_TransactionCategory);
			AssertEquals(nameof(overpaymentJournal.AH_OH), ObjectCreator.Debtor.PK, overpaymentJournal.AH_OH);
			AssertEquals(nameof(overpaymentJournal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001000 / JOB S0001234", overpaymentJournal.AH_Desc);
			AssertEquals(nameof(overpaymentJournal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, overpaymentJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(overpaymentJournal.AH_OSExTaxAmount), 300m, overpaymentJournal.AH_OSExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_LocalExTaxAmount), 150m, overpaymentJournal.AH_LocalExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_AG), ObjectCreator.GLHeader1.PK, overpaymentJournal.AH_AG);
			AssertEquals(nameof(overpaymentJournal.AH_PostDate), invoice.AH_PostDate, overpaymentJournal.AH_PostDate);
			AssertEquals(nameof(overpaymentJournal.AH_InvoiceDate), ZDateTime.Now.Date, overpaymentJournal.AH_InvoiceDate.Date);
			AssertEquals(nameof(overpaymentJournal.AH_FullyPaidDate), ZDateTime.Empty, overpaymentJournal.AH_FullyPaidDate);
			AssertEquals(invoice.PK, overpaymentJournal.AH_TransactionBelongsToGroup);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("EXX transaction should NOT exist", 0, exxTransactions.Length);

			//The following test the reverse process
			AssertEquals(false, invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice as ARInvoice);
			Assert(reverser.IsOutstandingAmountOnlyPaidViaCashAdvance);
			reverser.Reverse();
			caiJournalFactory.Save();

			AssertEquals(true, invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 4, journals.Length);
			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), journal1.AH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), 500m, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), 250m, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), journal2.AH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), 150m, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), 75m, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);

			var journal5 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == overpaymentJournal.PK);
			AssertNotNull(journal5);
			AssertEquals(nameof(journal5.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal5.AH_TransactionCategory);
			AssertEquals(nameof(journal5.AH_OH), ObjectCreator.Debtor.PK, journal5.AH_OH);
			AssertEquals(nameof(journal5.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {overpaymentJournal.AH_TransactionNum}"), journal5.AH_Desc);
			AssertEquals(nameof(journal5.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal5.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal5.DebitCreditSign), "CR", journal5.DebitCreditSign);
			AssertEquals(nameof(journal5.AH_OSExTaxAmount), 300m, journal5.AH_OSExTaxAmount);
			AssertEquals(nameof(journal5.AH_LocalExTaxAmount), 150m, journal5.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal5.AH_AG), ObjectCreator.GLHeader1.PK, journal5.AH_AG);

			exxTransactions = GetEXXTransactions();
			AssertEquals("EXX transaction should NOT exist", 0, exxTransactions.Length);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_NotSameExRate_AmountOverpaid()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			ObjectCreator.SetExchangeRate(job, ObjectCreator.USD, 0.7M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.USD, osCostAmt: 285M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.USD, osSellAmt: 285M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge1.JR_IsARCashAdvance = true;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.USD, osCostAmt: 2700M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.USD, osSellAmt: 2700M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();
			Factory.Save();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.USD, 0.65M);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.USD, osCostAmt: 900M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.USD, osSellAmt: 900M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
					.OrderBy(x => x.CAH_OSAmount)
					.ToList();

			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			var cah2 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[1].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah2);
			var mj2 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah2.PK));
			AssertNotNull(mj2);
			var receipt4 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj2, matchGroupNumber: "M0004");
			cah2.MarkAsPaid(true);
			carJournalFactory.Save();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.USD, 0.6M);
			charge1.JR_OSSellAmt = 270m;
			charge2.JR_OSSellAmt = 2500m;
			charge3.JR_OSSellAmt = 800m;
			Factory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah2.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is not empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_Desc).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah2.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), "USD", journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_ExchangeRate), 0.700001m, journal1.AH_ExchangeRate);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 2770m, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 3957.14m, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDateTime.Now.Date, journal1.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal1.AH_FullyPaidDate), ZDateTime.Today, journal1.AH_FullyPaidDate);
			Assert(journal1.AH_TransactionBelongsToGroup.IsEmpty);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), "USD", journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_ExchangeRate), 0.649998m, journal2.AH_ExchangeRate);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 800m, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 1230.77m, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), invoice.AH_PostDate, journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);
			AssertEquals(nameof(journal2.AH_FullyPaidDate), ZDateTime.Today, journal2.AH_FullyPaidDate);
			Assert(journal2.AH_TransactionBelongsToGroup.IsEmpty);

			var overpaymentJournals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 2, overpaymentJournals.Length);
			sortedJournals = overpaymentJournals.OrderBy(j => j.AH_Desc).ToList();
			var overpaymentJournal1 = sortedJournals[0];
			AssertEquals(nameof(overpaymentJournal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, overpaymentJournal1.AH_TransactionCategory);
			AssertEquals(nameof(overpaymentJournal1.AH_OH), ObjectCreator.Debtor.PK, overpaymentJournal1.AH_OH);
			AssertEquals(nameof(overpaymentJournal1.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001000 / JOB S0001234", overpaymentJournal1.AH_Desc);
			AssertEquals(nameof(overpaymentJournal1.AH_RX_NKTransactionCurrency), "USD", overpaymentJournal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(overpaymentJournal1.AH_ExchangeRate), 0.700001m, overpaymentJournal1.AH_ExchangeRate);
			AssertEquals(nameof(overpaymentJournal1.AH_OSExTaxAmount), 215m, overpaymentJournal1.AH_OSExTaxAmount);
			AssertEquals(nameof(overpaymentJournal1.AH_LocalExTaxAmount), 307.14m, overpaymentJournal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(overpaymentJournal1.AH_AG), ObjectCreator.GLHeader1.PK, overpaymentJournal1.AH_AG);
			AssertEquals(nameof(overpaymentJournal1.AH_PostDate), invoice.AH_PostDate, overpaymentJournal1.AH_PostDate);
			AssertEquals(nameof(overpaymentJournal1.AH_InvoiceDate), ZDateTime.Now.Date, overpaymentJournal1.AH_InvoiceDate.Date);
			AssertEquals(nameof(overpaymentJournal1.AH_FullyPaidDate), ZDateTime.Empty, overpaymentJournal1.AH_FullyPaidDate);
			AssertEquals(invoice.PK, overpaymentJournal1.AH_TransactionBelongsToGroup);

			var overpaymentJournal2 = sortedJournals[1];
			AssertEquals(nameof(overpaymentJournal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, overpaymentJournal2.AH_TransactionCategory);
			AssertEquals(nameof(overpaymentJournal2.AH_OH), ObjectCreator.Debtor.PK, overpaymentJournal2.AH_OH);
			AssertEquals(nameof(overpaymentJournal2.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", overpaymentJournal2.AH_Desc);
			AssertEquals(nameof(overpaymentJournal2.AH_RX_NKTransactionCurrency), "USD", overpaymentJournal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(overpaymentJournal2.AH_ExchangeRate), 0.649998m, overpaymentJournal2.AH_ExchangeRate);
			AssertEquals(nameof(overpaymentJournal2.AH_OSExTaxAmount), 100m, overpaymentJournal2.AH_OSExTaxAmount);
			AssertEquals(nameof(overpaymentJournal2.AH_LocalExTaxAmount), 153.85m, overpaymentJournal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(overpaymentJournal2.AH_AG), ObjectCreator.GLHeader1.PK, overpaymentJournal2.AH_AG);
			AssertEquals(nameof(overpaymentJournal2.AH_PostDate), invoice.AH_PostDate, overpaymentJournal2.AH_PostDate);
			AssertEquals(nameof(overpaymentJournal2.AH_InvoiceDate), ZDateTime.Now.Date, overpaymentJournal2.AH_InvoiceDate.Date);
			AssertEquals(nameof(overpaymentJournal2.AH_FullyPaidDate), ZDateTime.Empty, overpaymentJournal2.AH_FullyPaidDate);
			AssertEquals(invoice.PK, overpaymentJournal2.AH_TransactionBelongsToGroup);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("an EXX transaction should exist", 1, exxTransactions.Length);
			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount", 762.09M, exxTransaction.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);
			AssertEquals("EXX transaction exchange rate", 1.0m, exxTransaction.AH_ExchangeRate);

			//The following test the reverse process
			AssertEquals(false, invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice as ARInvoice);
			Assert(reverser.IsOutstandingAmountOnlyPaidViaCashAdvance);
			reverser.Reverse();
			caiJournalFactory.Save();

			AssertEquals(true, invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 6, journals.Length);
			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), "USD", journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), 2770m, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), 3957.14m, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), journal2.AH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), 800m, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), 1230.77m, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);

			var journal5 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == overpaymentJournal1.PK);
			AssertNotNull(journal5);
			AssertEquals(nameof(journal5.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal5.AH_TransactionCategory);
			AssertEquals(nameof(journal5.AH_OH), ObjectCreator.Debtor.PK, journal5.AH_OH);
			AssertEquals(nameof(journal5.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {overpaymentJournal1.AH_TransactionNum}"), journal5.AH_Desc);
			AssertEquals(nameof(journal5.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal5.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal5.DebitCreditSign), "CR", journal5.DebitCreditSign);
			AssertEquals(nameof(journal5.AH_OSExTaxAmount), 215m, journal5.AH_OSExTaxAmount);
			AssertEquals(nameof(journal5.AH_LocalExTaxAmount), 307.14m, journal5.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal5.AH_AG), ObjectCreator.GLHeader1.PK, journal5.AH_AG);

			var journal6 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == overpaymentJournal2.PK);
			AssertNotNull(journal6);
			AssertEquals(nameof(journal6.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal6.AH_TransactionCategory);
			AssertEquals(nameof(journal6.AH_OH), ObjectCreator.Debtor.PK, journal6.AH_OH);
			AssertEquals(nameof(journal6.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {overpaymentJournal2.AH_TransactionNum}"), journal6.AH_Desc);
			AssertEquals(nameof(journal6.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal6.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal6.DebitCreditSign), "CR", journal6.DebitCreditSign);
			AssertEquals(nameof(journal6.AH_OSExTaxAmount), 100m, journal6.AH_OSExTaxAmount);
			AssertEquals(nameof(journal6.AH_LocalExTaxAmount), 153.85m, journal6.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal6.AH_AG), ObjectCreator.GLHeader1.PK, journal6.AH_AG);

			exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 2, exxTransactions.Length);
			exxTransactions.ForEach(x => Assert(x.IsReversed));
			var reverseExx = exxTransactions.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == exxTransaction.PK);
			AssertNotNull(reverseExx);
			AssertEquals("EXX transaction amount", -762.09M, reverseExx.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", reverseExx.AH_RX_NKTransactionCurrency);
			AssertEquals("EXX transaction exchange rate", 1.0m, reverseExx.AH_ExchangeRate);
		}

		[SuspendCriticalValidation]
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusManually_OverpaymentJournalDescription()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var job2 = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job2.JH_JobNum = "S0005678";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job2, ObjectCreator.CC3, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var (cah2, cal2) = CreateCashAdvanceRequest(charge2, 300M, 300M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah2.CAH_RequestReferenceNumber = "00001002";
			cal2.CAL_LocalPaidAmount = 300M;
			cal2.CAL_OSPaidAmount = 300M;
			Factory.Save();

			var cah1Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah1.PK);
			cah1Reloaded.MarkAsPaid(false);
			var cah2Reloaded = Factory.Load<CashAdvanceRequestHeader>(cah2.PK);
			cah2Reloaded.MarkAsPaid(false);
			Factory.Save();

			charge1.JR_OSSellAmt = 100m;
			var invoice = GetInvoice(charge1, charge2);
			invoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;
			invoice.AH_JH = ZGuid.Empty;
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal2.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is Not Empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, journals.Length);

			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Debtor.PK, journal.AH_OH);
			AssertEquals(nameof(journal.DebitCreditSign), "DR", journal.DebitCreditSign);
			AssertEquals(nameof(journal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), 100M, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), 100M, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), invoice.AH_InvoiceDate, journal.AH_InvoiceDate);
			AssertEquals(nameof(journal.AH_FullyPaidDate), ZDateTime.Empty, journal.AH_FullyPaidDate);
			AssertEquals(nameof(journal.AH_TransactionBelongsToGroup), invoice.PK, journal.AH_TransactionBelongsToGroup);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_LocalInvoiceType_NotSameExRate()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "Charge 04"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 150M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 150M, debtor: objectCreator.Debtor);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			var cah4 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[1].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4);
			var mj4 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4.PK));
			AssertNotNull(mj4);
			var receipt4 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4.MarkAsPaid(true);
			carJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Not Fully Paid", 67.85M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is Empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_LocalExTaxAmount).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDateTime.Now.Date, journal1.AH_InvoiceDate.Date);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), invoice.AH_PostDate, journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);

			//The following test the reverse process
			AssertEquals(67.85m, invoice.AH_OutstandingAmount);
			Assert(invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice as ARInvoice);
			reverser.Reverse();
			caiJournalFactory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Reverse Journal Count", 2, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("EXX transaction should NOT exist", 0, exxTransactions.Length);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_LocalInvoiceType_SameExRate()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "Charge 04"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 150M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 150M, debtor: objectCreator.Debtor);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			var cah4 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[1].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4);
			var mj4 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4.PK));
			AssertNotNull(mj4);
			var receipt4 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4.MarkAsPaid(true);
			carJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is not empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_LocalExTaxAmount).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDateTime.Now.Date, journal1.AH_InvoiceDate.Date);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), invoice.AH_PostDate, journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice as ARInvoice);
			reverser.Reverse();
			caiJournalFactory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Reverse Journal Count", 2, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("EXX transaction should NOT exist", 0, exxTransactions.Length);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_ForeignInvoiceType_SameExRate()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "Charge 04"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 150M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 150M, debtor: objectCreator.Debtor);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			var cah4 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[1].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4);
			var mj4 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4.PK));
			AssertNotNull(mj4);
			var receipt4 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4.MarkAsPaid(true);
			carJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is not empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_LocalExTaxAmount).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDateTime.Now.Date, journal1.AH_InvoiceDate.Date);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), invoice.AH_PostDate, journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice as ARInvoice);
			reverser.Reverse();
			caiJournalFactory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Reverse Journal Count", 2, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("EXX transaction should NOT exist", 0, exxTransactions.Length);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_ForeignInvoiceType_NotSameExRate()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "Charge 04"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 150M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 150M, debtor: objectCreator.Debtor);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			new ARCashAdvanceRequestor(job).GenerateRequests();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			var cah4 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[1].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4);
			var mj4 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4.PK));
			AssertNotNull(mj4);
			var receipt4 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4.MarkAsPaid(true);
			carJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var invoice = invoices.FirstOrDefault();
			invoice.AH_InvoiceDate = new ZDateTime(2023, 01, 30);
			invoice.AH_PostDate = new ZDateTime(2023, 01, 31);

			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is not empty", new ZDateTime(2023, 01, 31), invoice.AH_FullyPaidDate);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);

			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount", 67.85M, exxTransaction.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(exxTransaction.AH_PostDate), new ZDateTime(2023, 01, 31), exxTransaction.AH_PostDate);
			AssertEquals(nameof(exxTransaction.AH_InvoiceDate), ZDateTime.Now.Date, exxTransaction.AH_InvoiceDate.Date);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_LocalExTaxAmount).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Debtor.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "DR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), new ZDateTime(2023, 01, 31), journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), ZDateTime.Now.Date, journal1.AH_InvoiceDate.Date);

			var journal2 = sortedJournals[1];
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Debtor.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2023, 01, 31), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), ZDateTime.Now.Date, journal2.AH_InvoiceDate.Date);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new ARInvoiceReversing(invoice as ARInvoice);
			reverser.Reverse();
			reverser.ReverseTransaction.PostDate = new ZDateTime(2023, 02, 27);
			caiJournalFactory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);
			AssertEquals(new ZDateTime(2023, 02, 27), reversingCrd.AH_PostDate);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Reverse Journal Count", 2, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Debtor.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.DebitCreditSign), "CR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);
			AssertEquals(nameof(journal3.AH_PostDate), new ZDateTime(2023, 02, 27), journal3.AH_PostDate);
			AssertEquals(nameof(journal3.AH_InvoiceDate), ZDateTime.Now.Date, journal3.AH_InvoiceDate.Date);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Debtor.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal2.AH_TransactionNum}"), journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.DebitCreditSign), "CR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);
			AssertEquals(nameof(journal4.AH_PostDate), new ZDateTime(2023, 02, 27), journal4.AH_PostDate);
			AssertEquals(nameof(journal4.AH_InvoiceDate), ZDateTime.Now.Date, journal4.AH_InvoiceDate.Date);

			exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 2, exxTransactions.Length);
			exxTransactions.ForEach(x => Assert(x.IsReversed));
			var reverseExx = exxTransactions.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == exxTransaction.PK);
			AssertNotNull(reverseExx);
			AssertEquals("EXX transaction amount", -67.85M, reverseExx.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", reverseExx.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(exxTransaction.AH_PostDate), new ZDateTime(2023, 02, 27), reverseExx.AH_PostDate);
			AssertEquals(nameof(exxTransaction.AH_InvoiceDate), ZDateTime.Now.Date, reverseExx.AH_InvoiceDate.Date);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_ForeignInvoiceType_NotSameExRate_OutstandingAmount_MultipleCashAdvances()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);

			//Initial Exchange Rate
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = false;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			//First Cash Advance
			new ARCashAdvanceRequestor(job).GenerateRequests();

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "Charge 04"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 150M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 150M, debtor: objectCreator.Debtor);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			//Second Cash Advance
			new ARCashAdvanceRequestor(job).GenerateRequests();

			//Exchange Rate is changed before posting invoice
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			//Creating CAR journals to change cash advance status to PAID
			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			var cah4 = carJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[1].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah4);
			var mj4 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah4.PK));
			AssertNotNull(mj4);
			var receipt4 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj4, matchGroupNumber: "M0004");
			cah4.MarkAsPaid(true);
			carJournalFactory.Save();

			//Posting AR Invoice. Saving process will create CAI journals and EXX transactions
			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("OS Outstanding Amount", 299.99M, invoice.OSOutstandingAmountMatching);
			AssertEquals("Local Outstanding Amount", 171.42M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);

			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount = Total Advance Payment Invoice Line Local Amount (371.43) - Total of two Advance Payment Header Local Amount (250+75)", 46.43M, exxTransaction.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);

			var journals = GetCAIJournals();
			AssertEquals("Reverse Journal Count", 2, journals.Length);

			var sortedJournals = journals.OrderBy(j => j.AH_LocalExTaxAmount).ToList();

			var journal = sortedJournals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Debtor.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), ZDateTime.Now.Date, journal.AH_InvoiceDate.Date);

			journal = sortedJournals[1];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Debtor.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah4.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt4.AH_TransactionNum}/{receipt4.AH_ChequeOrReference}."), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cah4.CAH_OSPaidAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cah4.CAH_LocalPaidAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), ZDateTime.Now.Date, journal.AH_InvoiceDate.Date);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_ForeignInvoiceType_NotSameExRate_OutstandingAmount_RoundingIssue()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			//Initial Exchange Rate
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 0.725M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 5000M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 5000M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 500M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 500M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			//Create cash advance
			new ARCashAdvanceRequestor(job).GenerateRequests();

			//Change Exchange Rate before posting invoice
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 0.74M);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 1400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 1400M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GST1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2.JR_IsARCashAdvance = false;
			Factory.Save();

			//Create CAR journal to update cash advance status to PAID
			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(job.CashAdvanceRequests[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			//Posting AR Invoice. Saving process will create CAI journals and EXX transactions
			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("OS Oustanding Amount", 1540.01M, invoice.OSOutstandingAmountMatching);
			AssertEquals("Local Oustanding Amount", 2081.09M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);

			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount = Total Advance Payment Invoice Line Local Amount (7432.45) - Advance Payment Header Local Amount (7586.21)", -153.78M, exxTransaction.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);

			var journals = GetCAIJournals();
			AssertEquals("Reverse Journal Count", 1, journals.Length);

			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Debtor.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), ZDateTime.Now.Date, journal.AH_InvoiceDate.Date);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_ForeignInvoiceType_NotSameExRate_OutstandingAmount_RoundingIssue_Case2()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			//Initial Exchange Rate
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 0.74M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 5000M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 5000M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 500M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 500M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			//Create cash advance
			new ARCashAdvanceRequestor(job).GenerateRequests();

			//Change Exchange Rate before posting invoice
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 0.725M);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 1400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 1400M, debtor: objectCreator.Debtor);
			charge2.JR_AT_SellGSTRate = ObjectCreator.GST1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2.JR_IsARCashAdvance = false;
			Factory.Save();

			//Create CAR journal to update cash advance status to PAID
			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(job.CashAdvanceRequests[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			//Posting AR Invoice. Saving process will create CAI journals and EXX transactions
			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("OS Oustanding Amount", 1540M, invoice.OSOutstandingAmountMatching);
			AssertEquals("Local Oustanding Amount", 2124.13M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);

			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount = Total Advance Payment Invoice Line Local Amount (7586.21) - Advance Payment Header Local Amount (7432.44)", 153.77M, exxTransaction.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);

			var journals = GetCAIJournals();
			AssertEquals("Reverse Journal Count", 1, journals.Length);

			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Debtor.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), ZDateTime.Now.Date, journal.AH_InvoiceDate.Date);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_ForeignInvoiceType_SameExRate_ChargeAmountChangedAfterCashAdvanceCreation()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			//Initial Exchange Rate
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 0.74M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 5000M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 5000M, debtor: objectCreator.Debtor);
			charge1.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 500M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 500M, debtor: objectCreator.Debtor);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			charge1.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			Factory.Save();

			//Create cash advance
			new ARCashAdvanceRequestor(job).GenerateRequests();

			//Create CAR journal to update cash advance status to PAID
			var carJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCARJournalFactory = new TestObjectCreator(carJournalFactory);

			var cah1 = carJournalFactory.Load<CashAdvanceRequestHeader>(job.CashAdvanceRequests[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = carJournalFactory.LoadTop1<ARJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var receipt1 = objectCreatorForCARJournalFactory.CreateAndMatchARReceiptForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			carJournalFactory.Save();

			charge1.JR_OSSellAmt = 5200M;
			charge3.JR_OSSellAmt = 420M;
			Factory.Save();

			//Posting AR Invoice. Saving process will create CAI journals and EXX transactions
			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Revenue);
			var invoices = transactionHashTable.GetAllARInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);

			AssertEquals("Posted AR Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("OS Oustanding Amount", 120.01M, invoice.OSOutstandingAmountMatching);
			AssertEquals("Local Oustanding Amount", 162.17M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Debtor.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice raised for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AR Receipt(s) {receipt1.AH_TransactionNum}/{receipt1.AH_ChequeOrReference}."), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), ZDateTime.Now.Date, journal.AH_InvoiceDate.Date);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestARInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ErrorThrownWhenCARJournalIsMissing()
		{
			AccountingConfigurationRegistry.Instance.CashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var invoice = GetInvoice(charge3, charge4);

			AssertExceptionThrown<ZCannotSaveException>(FormattableString.Invariant($"There is no matching AR CAR journal for Advance Payment request {cah4.CAH_RequestReferenceNumber}")
			, () =>
			{
				Factory.Save();
				Assert(!invoice.IsInDatabase);
			});

			AssertEquals(true, charge2.JR_IsARCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			Assert(cah1.HasChanges);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);
			Assert(cah4.HasChanges);

			var journals = GetCAIJournals();
			AssertEquals("Reverse Journal Count", 0, journals.Length);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestNoCriticalValidationErrorThrown_WhenCashAdvanceFunctionalityDisabled()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;

			var (cah1, cal1) = CreateCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_ARLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var invoice = GetInvoice(charge3, charge4);
			AssertNoExceptionThrown("As Advance Payment functionality is disabled, outstanding amount is not updated on saving. Therefore, no error should be thrown", () => Factory.Save());
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestCriticalValidationErrorThrown_CashAdvanceFunctionalityEnabledButNoCashAdvanceLinkedWithInvoice()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_SellAccount = ObjectCreator.Debtor.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_SellGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsARCashAdvance = true;
			charge2.JR_IsARCashAdvance = true;
			charge3.JR_IsARCashAdvance = true;
			charge4.JR_IsARCashAdvance = true;
			Factory.Save();

			try
			{
				var invoice = GetInvoice(charge3, charge4);
				invoice.AH_OutstandingAmount = 256M;
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException exception)
			{
				AssertEquals("Incorrect outstanding amount should trigger a cirtical validation exception", nameof(CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12), exception.ErrorType);
				AssertContains("Incorrect outstanding amount should trigger a cirtical validation exception", "Incorrect outstanding amount.", exception.Message);
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[ExpectNoExceptions]
		public void TestReverseNonJobRelatedInvoice_NoExceptionThrown()
		{
			AccountingConfigurationRegistry.Instance.EnableReceivablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfReceivablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var arInvoice = ObjectCreator.CreateARInvoice<ARInvoice>("00001001", ObjectCreator.LocalCurrency, 1, ObjectCreator.Debtor);
			ObjectCreator.CreateARInvoiceLine(arInvoice, null, ObjectCreator.FRT, ObjectCreator.LocalCurrency, 1, "desc", 100);
			Factory.Save();
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(arInvoice);
			reversing.Reverse();
			Assert(arInvoice.IsReversed);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year - 1);
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year + 1);
		}

		ARJournal[] GetCAIJournals(bool hasTransactionBelongsToGroup = false)
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, TransactionCategory.Codes.CashAdvanceInvoice);
			if (hasTransactionBelongsToGroup)
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, null);
			}
			return Factory.Load<ARJournal>(query);
		}

		ExchangeDifference[] GetEXXTransactions()
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference);
			return Factory.Load<ARExchangeDifference>(query);
		}

		(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateCashAdvanceRequest(Charge charge, ZDecimal localAmount, ZDecimal osAmount, ZString status, ZString currency, OrgHeader debtor = null)
		{
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, debtor ?? ObjectCreator.Debtor, LedgerTypes.AccountsReceivable, 0, 0, currency);
			var cal = ObjectCreator.CreateCashAdvanceRequestLine(cah, localAmount, osAmount);
			charge.JR_CAL_ARLine = cal.PK;
			cal.CAL_Status = status;
			return (cah, cal);
		}

		ARInvoice GetInvoice(params Charge[] charges)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(ARInvoice), currency: ObjectCreator.AUD, organisation: ObjectCreator.Debtor) as ARInvoice;
			foreach (var charge in charges)
			{
				var line = ObjectCreator.CreateARInvoiceLine(invoice, charge.Job as Job, charge.ChargeCode, charge.SellCurrency, 1.0M, "DESC01", charge.JR_OSSellAmt);
				charge.WIP.AL_ReverseDate = ZDateTime.Today;
				charge.JR_AL_ARLine = line.PK;
			}
			return invoice;
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
