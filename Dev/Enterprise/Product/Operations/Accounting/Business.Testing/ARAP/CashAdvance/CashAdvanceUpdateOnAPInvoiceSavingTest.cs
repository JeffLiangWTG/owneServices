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
	public class CashAdvanceUpdateOnAPInvoiceSavingTest : TestCaseWithFactory
	{
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_NoCashAdvanceClearingAccount_ReportException()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_APLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateAPCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah4.CAH_RequestReferenceNumber = "00001004";
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			charge3.JR_OSCostAmt = 100m;
			var invoice = GetAPInvoice(charge3, charge4);
			invoice.AH_JH = job.PK;

			AssertExceptionThrown<ZCannotSaveException>(@"On posting this transaction, a journal to the Payables Advance Payment Clearing Account will be created, which requires a Advance Payment Clearing Account to be recorded in the Accounting > Advance Payments > Payables > Payables Advance Payment Clearing Account registry.
Please ensure this registry has an account recorded and then post the transaction."
			, () =>
			{
				Factory.Save();
				Assert(!invoice.IsInDatabase);
			});
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_UpdatedToPaidStatusManually()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_APLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateAPCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var invoice = GetAPInvoice(charge3, charge4);
			Factory.Save();

			AssertEquals(true, charge2.JR_IsAPCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Fully Paid", ZDecimal.Zero, invoice.AH_OutstandingAmount);
			AssertNotEquals("Fully Paid Date is Not Empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_ConvertedFromPendingAllocation()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			charge1.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;
			Factory.Save();

			var pendingAllocation = ObjectCreator.CreateTransactionPendingAllocation("INV", ObjectCreator.Creditor1, 100);
			Factory.Save();
			AssertEquals(LedgerTypes.TransactionsPendingAllocation, pendingAllocation.AH_Ledger);
			AssertEquals(TransactionTypes.InvoicePendingAllocation, pendingAllocation.AH_TransactionType);

			var apInvoice = TransactionAllocationConverter.ConvertUnallocatedToAP(pendingAllocation).Invoice;
			var paFactory = apInvoice.Factory;
			var charge1Reloaded = paFactory.Load<JobCharge>(charge1.PK);
			charge1Reloaded.JR_OSCostAmt = 120m;
			var line = (InvoicingLineBase)apInvoice.Lines.AddNew();
			line.AL_JH = job.PK;
			apInvoice.ImportJobChargesIntoInvoice(new[] { charge1Reloaded }.ToList<Charge>(), line);
			paFactory.Save();
			AssertEquals(LedgerTypes.AccountsPayable, apInvoice.AH_Ledger);
			AssertEquals(TransactionTypes.Invoice, apInvoice.AH_TransactionType);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, journals.Length);
			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal.AH_Desc);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_ConvertedFromIncompleteInvoice()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			charge1.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;
			Factory.Save();

			charge1.JR_OSCostAmt = 120m;
			Factory.Save();

			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), "234", ObjectCreator.AUD, 1M);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			invoice.ImportJobChargesIntoInvoice(new[] { charge1 }.ToList(), line);
			invoice.SaveAsIncomplete();
			Factory.Save();
			AssertEquals(LedgerTypes.IncompleteTransactions, invoice.AH_Ledger);
			AssertEquals(TransactionTypes.IncompleteInvoice, invoice.AH_TransactionType);

			invoice.MoveFromIncompleteToPayableLedger();
			Factory.Save();
			AssertEquals(LedgerTypes.AccountsPayable, invoice.AH_Ledger);
			AssertEquals(TransactionTypes.Invoice, invoice.AH_TransactionType);

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, journals.Length);
			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal.AH_Desc);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_ConvertedFromUAInvoice()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			charge1.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;
			Factory.Save();

			charge1.JR_OSCostAmt = 120m;
			Factory.Save();

			var uaInvoice = Factory.New<UAInvoice>();
			uaInvoice.AH_TransactionNum = "00001000";
			uaInvoice.AH_OH = ObjectCreator.Creditor1.PK;
			Factory.Save();
			AssertEquals(LedgerTypes.UnapprovedPayableTransactions, uaInvoice.AH_Ledger);
			AssertEquals(TransactionTypes.UAInvoice, uaInvoice.AH_TransactionType);

			var converter = new UnapprovedTransactionConverter(Factory);
			var apInvoice = (APInvoice)converter.ConvertToAP(uaInvoice, false);

			var apFactory = apInvoice.Factory;
			var charge1Reloaded = apFactory.Load<JobCharge>(charge1.PK);
			var line = (InvoicingLineBase)apInvoice.Lines.AddNew();
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_JH = job.PK;
			apInvoice.ImportJobChargesIntoInvoice(new[] { charge1Reloaded }.ToList<Charge>(), line);
			apFactory.Save();

			AssertEquals(LedgerTypes.AccountsPayable, apInvoice.AH_Ledger);
			AssertEquals(TransactionTypes.Invoice, apInvoice.AH_TransactionType);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, journals.Length);
			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal.AH_Desc);
		}

		[TestDate(2022, 12, 15)]
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_UpdatedToPaidStatusManually_LocalCurrency_Overpaid()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_APLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateAPCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah4.CAH_RequestReferenceNumber = "00001004";
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			charge3.JR_OSCostAmt = 100m;
			var invoice = GetAPInvoice(charge3, charge4);
			invoice.AH_JH = job.PK;
			invoice.AH_PostDate = new ZDateTime(2022, 11, 11);
			invoice.AH_InvoiceDate = new ZDateTime(2022, 11, 20);
			Factory.Save();

			AssertEquals(true, charge2.JR_IsAPCashAdvance);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal3.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal4.CAL_Status);

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.PartiallyInvoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah4.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 300M, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 300M, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), new ZDateTime(2022, 11, 11), journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), new ZDateTime(2022, 11, 20), journal1.AH_InvoiceDate);
			AssertEquals(nameof(journal1.AH_FullyPaidDate), ZDateTime.Empty, journal1.AH_FullyPaidDate);
			AssertEquals(nameof(journal1.AH_TransactionBelongsToGroup), invoice.PK, journal1.AH_TransactionBelongsToGroup);

			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			//The following test the reverse process
			AssertEquals(false, invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice);
			reverser.Reverse();
			if (reverser.ReverseTransaction != null && reverser.ReverseTransaction is AccTransactionHeader reverseTransaction)
			{
				reverseTransaction.AH_TransactionNum = "TRAN111";
			}
			Factory.Save();

			AssertEquals(true, invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 2, journals.Length);
			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_Desc), "REVERSAL RELATED TO 00001000", journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), journal1.AH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 300M, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 300M, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2022, 12, 15), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), new ZDateTime(2022, 12, 15), journal2.AH_InvoiceDate);
			AssertEquals(nameof(journal2.AH_FullyPaidDate), new ZDateTime(2022, 12, 15), journal2.AH_FullyPaidDate);
			AssertEquals(nameof(journal2.AH_TransactionBelongsToGroup), journal1.PK, journal2.AH_TransactionBelongsToGroup);
		}

		[TestDate(2022, 12, 15)]
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_UpdatedToPaidStatusManually_ForeignCurrency_Overpaid()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var invoiceDate = ZDateTime.Today;
			var invoiceNum = "APINV12345";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			charge1.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge1.JR_APInvoiceNum = invoiceNum;
			charge1.JR_APInvoiceDate = invoiceDate;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			charge2.JR_AT_CostGSTRate = ObjectCreator.GST1.PK;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge2.JR_APInvoiceNum = invoiceNum;
			charge2.JR_APInvoiceDate = invoiceDate;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			charge3.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			charge3.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge3.JR_APInvoiceNum = invoiceNum;
			charge3.JR_APInvoiceDate = invoiceDate;

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			Factory.Save();

			charge1.LoadRelevantChargesForAPCashAdvance();
			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "Charge 04"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 150M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 150M, debtor: objectCreator.Debtor);
			charge4.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			charge4.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			charge4.JR_APInvoiceNum = invoiceNum;
			charge4.JR_APInvoiceDate = invoiceDate;
			charge4.JR_IsAPCashAdvance = true;
			Factory.Save();

			charge4.LoadRelevantChargesForAPCashAdvance();
			new APCashAdvanceRequestor(charge4).GenerateRequest();
			Factory.Save();

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
			charge3.JR_OSCostAmt = 100m;
			charge4.JR_OSCostAmt = 80m;
			Factory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(carJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah4.CAH_Status);

			AssertEquals("Posted AP Invoice", 1, invoices.Length);
			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is not empty", ZDateTime.Today, invoice.AH_FullyPaidDate);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 2, journals.Length);
			var sortedJournals = journals.OrderBy(j => j.AH_Desc).ToList();

			var journal1 = sortedJournals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
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
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.DebitCreditSign), "CR", journal2.DebitCreditSign);
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
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			reverser.Reverse();
			if (reverser.ReverseTransaction != null && reverser.ReverseTransaction is AccTransactionHeader reverseTransaction)
			{
				reverseTransaction.AH_TransactionNum = "TRAN111";
			}
			caiJournalFactory.Save();

			AssertEquals(true, invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 4, journals.Length);
			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Creditor1.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.DebitCreditSign), "DR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_Desc), "REVERSAL RELATED TO 00001000", journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), journal1.AH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), 300m, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), 150m, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);
			AssertEquals(nameof(journal3.AH_PostDate), new ZDateTime(2022, 12, 15), journal3.AH_PostDate);
			AssertEquals(nameof(journal3.AH_InvoiceDate), new ZDateTime(2022, 12, 15), journal3.AH_InvoiceDate);
			AssertEquals(nameof(journal3.AH_FullyPaidDate), new ZDateTime(2022, 12, 15), journal3.AH_FullyPaidDate);
			AssertEquals(nameof(journal3.AH_TransactionBelongsToGroup), journal1.PK, journal3.AH_TransactionBelongsToGroup);

			var journal4 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal2.PK);
			AssertNotNull(journal4);
			AssertEquals(nameof(journal4.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal4.AH_TransactionCategory);
			AssertEquals(nameof(journal4.AH_OH), ObjectCreator.Creditor1.PK, journal4.AH_OH);
			AssertEquals(nameof(journal4.DebitCreditSign), "DR", journal4.DebitCreditSign);
			AssertEquals(nameof(journal4.AH_Desc), "REVERSAL RELATED TO 00001001", journal4.AH_Desc);
			AssertEquals(nameof(journal4.AH_RX_NKTransactionCurrency), journal2.AH_RX_NKTransactionCurrency, journal4.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal4.AH_OSExTaxAmount), 70m, journal4.AH_OSExTaxAmount);
			AssertEquals(nameof(journal4.AH_LocalExTaxAmount), 35m, journal4.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal4.AH_AG), ObjectCreator.GLHeader1.PK, journal4.AH_AG);
			AssertEquals(nameof(journal4.AH_PostDate), new ZDateTime(2022, 12, 15), journal4.AH_PostDate);
			AssertEquals(nameof(journal4.AH_InvoiceDate), new ZDateTime(2022, 12, 15), journal4.AH_InvoiceDate);
			AssertEquals(nameof(journal4.AH_FullyPaidDate), new ZDateTime(2022, 12, 15), journal4.AH_FullyPaidDate);
			AssertEquals(nameof(journal4.AH_TransactionBelongsToGroup), journal2.PK, journal4.AH_TransactionBelongsToGroup);
		}

		[TestDate(2022, 12, 15)]
		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestAPInvoiceVisit_UpdatedToPaidStatusManually_Overpaid_ReduceAmountAndPostInSteps()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 100M, 100M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 200M, 200M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			charge1.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			charge2.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 100M, 100M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cah1.CAH_RequestReferenceNumber = "00001001";
			cal1.CAL_LocalPaidAmount = 100M;
			cal1.CAL_OSPaidAmount = 100M;

			var cal2 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 200M, 200M);
			charge2.JR_CAL_APLine = cal2.PK;
			cal2.CAL_LocalPaidAmount = 200M;
			cal2.CAL_OSPaidAmount = 200M;
			cal2.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			Factory.Save();

			charge1.JR_OSCostAmt = 10m;
			var invoice = GetAPInvoice(charge1);
			invoice.AH_JH = job.PK;
			invoice.AH_PostDate = new ZDateTime(2022, 11, 11);
			invoice.AH_InvoiceDate = new ZDateTime(2022, 11, 20);
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Paid, cal2.CAL_Status);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 90M, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 90M, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), new ZDateTime(2022, 11, 11), journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), new ZDateTime(2022, 11, 20), journal1.AH_InvoiceDate);
			AssertEquals(nameof(journal1.AH_FullyPaidDate), ZDateTime.Empty, journal1.AH_FullyPaidDate);
			AssertEquals(nameof(journal1.AH_TransactionBelongsToGroup), invoice.PK, journal1.AH_TransactionBelongsToGroup);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);

			charge2.JR_OSCostAmt = 20m;
			invoice = GetAPInvoice(charge2);
			invoice.AH_JH = job.PK;
			invoice.AH_PostDate = new ZDateTime(2022, 11, 11);
			invoice.AH_InvoiceDate = new ZDateTime(2022, 11, 20);
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal1.CAL_Status);
			AssertEquals(CashAdvanceStatusCodes.RequestLine.Invoiced, cal2.CAL_Status);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 2, journals.Length);
			var journal2 = journals.First(x => x.PK != journal1.PK);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.DebitCreditSign), "CR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001001 / JOB S0001234", journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 180M, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 180M, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2022, 11, 11), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), new ZDateTime(2022, 11, 20), journal2.AH_InvoiceDate);
			AssertEquals(nameof(journal2.AH_FullyPaidDate), ZDateTime.Empty, journal2.AH_FullyPaidDate);
			AssertEquals(nameof(journal2.AH_TransactionBelongsToGroup), invoice.PK, journal2.AH_TransactionBelongsToGroup);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestNoCriticalValidationErrorThrown_WhenCashAdvanceFunctionalityDisabled()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;

			var (cah1, cal1) = CreateAPCashAdvanceRequest(charge1, 200M, 200M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal1.CAL_LocalPaidAmount = 200M;
			cal1.CAL_OSPaidAmount = 200M;

			var cal3 = ObjectCreator.CreateCashAdvanceRequestLine(cah1, 400M, 400M);
			charge3.JR_CAL_APLine = cal3.PK;
			cal3.CAL_LocalPaidAmount = 400M;
			cal3.CAL_OSPaidAmount = 400M;
			cal3.CAL_Status = CashAdvanceStatusCodes.RequestLine.Paid;

			var (cah4, cal4) = CreateAPCashAdvanceRequest(charge4, 150M, 150M, CashAdvanceStatusCodes.RequestLine.Paid, "AUD");
			cal4.CAL_LocalPaidAmount = 150M;
			cal4.CAL_OSPaidAmount = 150M;
			Factory.Save();

			var invoice = GetAPInvoice(charge3, charge4);
			AssertNoExceptionThrown("As Advance Payment functionality is disabled, outstanding amount is not updated on saving. Therefore, no error should be thrown", () => Factory.Save());
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		public void TestCriticalValidationErrorThrown_CashAdvanceFunctionalityEnabledButNoCashAdvanceLinkedWithInvoice()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableReportCriticalValidationErrorsAfterDBSaving.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, 200M, 200M);
			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, 300M, 300M);
			charge2.JR_OH_CostAccount = ObjectCreator.Creditor1.PK;
			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, 400M, 400M);
			charge3.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, 150M, 150M);
			charge4.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;

			charge1.JR_IsAPCashAdvance = true;
			charge2.JR_IsAPCashAdvance = true;
			charge3.JR_IsAPCashAdvance = true;
			charge4.JR_IsAPCashAdvance = true;
			Factory.Save();

			try
			{
				var invoice = GetAPInvoice(charge3, charge4);
				invoice.AH_OutstandingAmount = -256M;
				Factory.Save();
			}
			catch (OnSavingCriticalCheckException exception)
			{
				AssertEquals("Incorrect outstanding amount should trigger a cirtical validation exception", nameof(CriticalValidationErrorType.TransactionHeaderIncorrectOutstandingAmount_12), exception.ErrorType);
				AssertContains("Incorrect outstanding amount should trigger a cirtical validation exception", "Incorrect outstanding amount.", exception.Message);
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		(AccCashAdvanceRequestHeader cah, AccCashAdvanceRequestLine cal) CreateAPCashAdvanceRequest(Charge charge, ZDecimal localAmount, ZDecimal osAmount, ZString status, ZString currency, OrgHeader creditor = null)
		{
			var cah = ObjectCreator.CreateCashAdvanceRequestHeader(charge.Job as Job, creditor ?? ObjectCreator.Creditor1, LedgerTypes.AccountsPayable, 0, 0, currency);
			var cal = ObjectCreator.CreateCashAdvanceRequestLine(cah, localAmount, osAmount);
			charge.JR_CAL_APLine = cal.PK;
			cal.CAL_Status = status;
			return (cah, cal);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_NotSameExRate()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(capJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);
			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount", -57.14M, exxTransaction.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			Assert(reverser.IsOutstandingAmountOnlyPaidViaCashAdvance);
			reverser.Reverse();
			reverser.ReverseTransaction.PostDate = new ZDateTime(2023, 02, 27);
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);
			AssertEquals(new ZDateTime(2023, 02, 27), reversingCrd.AH_PostDate);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 1, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2023, 02, 27), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), new ZDateTime(2023, 02, 24), journal2.AH_InvoiceDate);

			exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 2, exxTransactions.Length);
			exxTransactions.ForEach(x => Assert(x.IsReversed));
			var reverseExx = exxTransactions.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == exxTransaction.PK);
			AssertNotNull(reverseExx);
			AssertEquals("EXX transaction amount", 57.14M, reverseExx.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", reverseExx.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(reverseExx.AH_PostDate), new ZDateTime(2023, 02, 27), reverseExx.AH_PostDate);
			AssertEquals(nameof(reverseExx.AH_InvoiceDate), new ZDateTime(2023, 02, 24), reverseExx.AH_InvoiceDate);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_NotSameExRate_FromIncompleteInvoice()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), "234", ObjectCreator.EUR, 1.75M, ObjectCreator.Creditor1);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			invoice.ImportJobChargesIntoInvoice(new[] { charge1, charge2, charge3 }.ToList(), line);
			invoice.SaveAsIncomplete();
			AssertEquals(LedgerTypes.IncompleteTransactions, invoice.AH_Ledger);
			AssertEquals(TransactionTypes.IncompleteInvoice, invoice.AH_TransactionType);

			invoice.MoveFromIncompleteToPayableLedger();
			Factory.Save();
			AssertEquals(LedgerTypes.AccountsPayable, invoice.AH_Ledger);
			AssertEquals(TransactionTypes.Invoice, invoice.AH_TransactionType);
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);
			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount", -57.14M, exxTransaction.AH_OSTotal * exxTransaction.Multiplier_ForTestOnly);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			reverser.Reverse();
			reverser.ReverseTransaction.PostDate = new ZDateTime(2023, 02, 27);
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);
			AssertEquals(new ZDateTime(2023, 02, 27), reversingCrd.AH_PostDate);

			journals = GetCAIJournals(null);
			AssertEquals("Reverse Journal Count", 2, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2023, 02, 27), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), new ZDateTime(2023, 02, 24), journal2.AH_InvoiceDate);

			exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 2, exxTransactions.Length);
			exxTransactions.ForEach(x => Assert(x.IsReversed));
			var reverseExx = exxTransactions.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == exxTransaction.PK);
			AssertNotNull(reverseExx);
			AssertEquals("EXX transaction amount", 57.14M, reverseExx.AH_OSTotal * reverseExx.Multiplier_ForTestOnly);
			AssertEquals("EXX transaction currency", "AUD", reverseExx.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(reverseExx.AH_PostDate), new ZDateTime(2023, 02, 27), reverseExx.AH_PostDate);
			AssertEquals(nameof(reverseExx.AH_InvoiceDate), new ZDateTime(2023, 02, 24), reverseExx.AH_InvoiceDate);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_NotSameExRate_FromPendingAllocation()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			var pendingAllocation = ObjectCreator.CreateTransactionPendingAllocation("INV", ObjectCreator.Creditor1, 800, currency: objectCreator.EUR, exchangeRate: 1.75m);
			Factory.Save();
			AssertEquals(LedgerTypes.TransactionsPendingAllocation, pendingAllocation.AH_Ledger);
			AssertEquals(TransactionTypes.InvoicePendingAllocation, pendingAllocation.AH_TransactionType);

			var invoice = TransactionAllocationConverter.ConvertUnallocatedToAP(pendingAllocation).Invoice;
			var paFactory = invoice.Factory;
			var charge1Reloaded = paFactory.Load<JobCharge>(charge1.PK);
			var charge2Reloaded = paFactory.Load<JobCharge>(charge2.PK);
			var charge3Reloaded = paFactory.Load<JobCharge>(charge3.PK);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_JH = job.PK;
			invoice.ImportJobChargesIntoInvoice(new[] { charge1Reloaded, charge2Reloaded, charge3Reloaded }.ToList<Charge>(), line);
			paFactory.Save();
			AssertEquals(LedgerTypes.AccountsPayable, invoice.AH_Ledger);
			AssertEquals(TransactionTypes.Invoice, invoice.AH_TransactionType);
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);
			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount", -57.14M, exxTransaction.AH_OSTotal * exxTransaction.Multiplier_ForTestOnly);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			reverser.Reverse();
			reverser.ReverseTransaction.PostDate = new ZDateTime(2023, 02, 27);
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);
			AssertEquals(new ZDateTime(2023, 02, 27), reversingCrd.AH_PostDate);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 1, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2023, 02, 27), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), new ZDateTime(2023, 02, 24), journal2.AH_InvoiceDate);

			exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 2, exxTransactions.Length);
			exxTransactions.ForEach(x => Assert(x.IsReversed));
			var reverseExx = exxTransactions.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == exxTransaction.PK);
			AssertNotNull(reverseExx);
			AssertEquals("EXX transaction amount", 57.14M, reverseExx.AH_OSTotal * reverseExx.Multiplier_ForTestOnly);
			AssertEquals("EXX transaction currency", "AUD", reverseExx.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(reverseExx.AH_PostDate), new ZDateTime(2023, 02, 27), reverseExx.AH_PostDate);
			AssertEquals(nameof(reverseExx.AH_InvoiceDate), new ZDateTime(2023, 02, 24), reverseExx.AH_InvoiceDate);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_NotSameExRate_ConvertedFromUAInvoice()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 800M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 800M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			var uaInvoice = Factory.New<UAInvoice>();
			uaInvoice.AH_TransactionNum = "00001000";
			uaInvoice.AH_OH = ObjectCreator.Creditor1.PK;
			uaInvoice.AH_RX_NKTransactionCurrency = ObjectCreator.EUR.RX_Code;
			uaInvoice.AH_ExchangeRate = 1.75M;
			Factory.Save();
			AssertEquals(LedgerTypes.UnapprovedPayableTransactions, uaInvoice.AH_Ledger);
			AssertEquals(TransactionTypes.UAInvoice, uaInvoice.AH_TransactionType);

			var converter = new UnapprovedTransactionConverter(Factory);
			var invoice = (APInvoice)converter.ConvertToAP(uaInvoice, false);

			var apFactory = invoice.Factory;
			var charge1Reloaded = apFactory.Load<JobCharge>(charge1.PK);
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_LineType = TransactionLineTypes.Cost;
			line.AL_JH = job.PK;
			invoice.ImportJobChargesIntoInvoice(new[] { charge1Reloaded }.ToList<Charge>(), line);
			apFactory.Save();

			AssertEquals(LedgerTypes.AccountsPayable, invoice.AH_Ledger);
			AssertEquals(TransactionTypes.Invoice, invoice.AH_TransactionType);
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);
			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount", -57.14M, exxTransaction.AH_OSTotal * exxTransaction.Multiplier_ForTestOnly);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);

			//The following test the reverse process
			var reverseFactory = new BusinessObjectFactory();
			var reloadedInvoice = reverseFactory.Load<APInvoice>(invoice.PK);
			AssertEquals(0m, reloadedInvoice.AH_OutstandingAmount);
			Assert(!reloadedInvoice.AH_FullyPaidDate.IsEmpty);
			Assert(!reloadedInvoice.IsReversed);
			var reverser = new APInvoiceReversing(reloadedInvoice);
			reverser.Reverse();
			reverser.ReverseTransaction.PostDate = new ZDateTime(2023, 02, 27);
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			reverseFactory.Save();
			Assert(reloadedInvoice.IsReversed);
			Assert(!reloadedInvoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, reloadedInvoice.AH_OutstandingAmount);
			var reversingCrd = reloadedInvoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);
			AssertEquals(new ZDateTime(2023, 02, 27), reversingCrd.AH_PostDate);

			journals = GetCAIJournals(null);
			AssertEquals("Reverse Journal Count", 2, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2023, 02, 27), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), new ZDateTime(2023, 02, 24), journal2.AH_InvoiceDate);

			exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 2, exxTransactions.Length);
			exxTransactions.ForEach(x => Assert(x.IsReversed));
			var reverseExx = exxTransactions.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == exxTransaction.PK);
			AssertNotNull(reverseExx);
			AssertEquals("EXX transaction amount", 57.14M, reverseExx.AH_OSTotal * reverseExx.Multiplier_ForTestOnly);
			AssertEquals("EXX transaction currency", "AUD", reverseExx.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(reverseExx.AH_PostDate), new ZDateTime(2023, 02, 27), reverseExx.AH_PostDate);
			AssertEquals(nameof(reverseExx.AH_InvoiceDate), new ZDateTime(2023, 02, 24), reverseExx.AH_InvoiceDate);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_NotSameExRate_AmountOverpaid()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			charge1.JR_OSCostAmt = 70m;
			charge2.JR_OSCostAmt = 260m;
			charge3.JR_OSCostAmt = 380m;
			Factory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(capJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);
			var exxTransaction = exxTransactions.First();
			AssertEquals("EXX transaction amount", -50.71M, exxTransaction.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", exxTransaction.AH_RX_NKTransactionCurrency);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), "EUR", journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 710m, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 355m, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);

			var overpaymentJournals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, overpaymentJournals.Length);
			var overpaymentJournal = overpaymentJournals[0];
			AssertEquals(nameof(overpaymentJournal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, overpaymentJournal.AH_TransactionCategory);
			AssertEquals(nameof(overpaymentJournal.AH_OH), ObjectCreator.Creditor1.PK, overpaymentJournal.AH_OH);
			AssertEquals(nameof(overpaymentJournal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001000 / JOB S0001234", overpaymentJournal.AH_Desc);
			AssertEquals(nameof(overpaymentJournal.AH_RX_NKTransactionCurrency), "EUR", overpaymentJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(overpaymentJournal.AH_ExchangeRate), 2.0m, overpaymentJournal.AH_ExchangeRate);
			AssertEquals(nameof(overpaymentJournal.AH_OSExTaxAmount), 90m, overpaymentJournal.AH_OSExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_LocalExTaxAmount), 45m, overpaymentJournal.AH_LocalExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_AG), ObjectCreator.GLHeader1.PK, overpaymentJournal.AH_AG);
			AssertEquals(nameof(overpaymentJournal.AH_PostDate), invoice.AH_PostDate, overpaymentJournal.AH_PostDate);
			AssertEquals(nameof(overpaymentJournal.AH_InvoiceDate), invoice.AH_InvoiceDate, overpaymentJournal.AH_InvoiceDate);
			AssertEquals(nameof(overpaymentJournal.AH_FullyPaidDate), ZDateTime.Empty, overpaymentJournal.AH_FullyPaidDate);
			AssertEquals(invoice.PK, overpaymentJournal.AH_TransactionBelongsToGroup);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			Assert(reverser.IsOutstandingAmountOnlyPaidViaCashAdvance);
			reverser.Reverse();
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 3, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 710m, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 355m, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);

			exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 2, exxTransactions.Length);
			exxTransactions.ForEach(x => Assert(x.IsReversed));
			var reverseExx = exxTransactions.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == exxTransaction.PK);
			AssertNotNull(reverseExx);
			AssertEquals("EXX transaction amount", 50.71M, reverseExx.AH_OSExTaxAmount);
			AssertEquals("EXX transaction currency", "AUD", reverseExx.AH_RX_NKTransactionCurrency);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_SameExRate()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(capJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 0, exxTransactions.Length);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			reverser.Reverse();
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 1, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_SameExRate_AmountOverpaid()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			charge1.JR_OSCostAmt = 80m;
			charge2.JR_OSCostAmt = 270m;
			Factory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(capJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 0, exxTransactions.Length);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), 750m, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), 375m, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);

			var overpaymentJournals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, overpaymentJournals.Length);
			var overpaymentJournal = overpaymentJournals[0];
			AssertEquals(nameof(overpaymentJournal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, overpaymentJournal.AH_TransactionCategory);
			AssertEquals(nameof(overpaymentJournal.AH_OH), ObjectCreator.Creditor1.PK, overpaymentJournal.AH_OH);
			AssertEquals(nameof(overpaymentJournal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001000 / JOB S0001234", overpaymentJournal.AH_Desc);
			AssertEquals(nameof(overpaymentJournal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, overpaymentJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(overpaymentJournal.AH_OSExTaxAmount), 50m, overpaymentJournal.AH_OSExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_LocalExTaxAmount), 25m, overpaymentJournal.AH_LocalExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_AG), ObjectCreator.GLHeader1.PK, overpaymentJournal.AH_AG);
			AssertEquals(nameof(overpaymentJournal.AH_PostDate), invoice.AH_PostDate, overpaymentJournal.AH_PostDate);
			AssertEquals(nameof(overpaymentJournal.AH_InvoiceDate), invoice.AH_InvoiceDate, overpaymentJournal.AH_InvoiceDate);
			AssertEquals(nameof(overpaymentJournal.AH_FullyPaidDate), ZDateTime.Empty, overpaymentJournal.AH_FullyPaidDate);
			AssertEquals(invoice.PK, overpaymentJournal.AH_TransactionBelongsToGroup);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			Assert(reverser.IsOutstandingAmountOnlyPaidViaCashAdvance);
			reverser.Reverse();
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 3, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 750m, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 375m, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);

			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == overpaymentJournal.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Creditor1.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {overpaymentJournal.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.DebitCreditSign), "DR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), 50m, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), 25m, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);

			exxTransactions = GetEXXTransactions();
			AssertEquals("EXX transaction should NOT exist", 0, exxTransactions.Length);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_ForeignCurrency_NotSameExRate_HasOutstandingAmount()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 2.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.EUR, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.EUR, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			charge3.JR_IsAPCashAdvance = false;
			ObjectCreator.SetExchangeRate(job, ObjectCreator.EUR, 1.75M);
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(capJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("NOT Fully Paid", -228.57M, invoice.AH_OutstandingAmount);
			AssertEquals("Fully Paid Date is Empty", ZDateTime.Empty, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 1, exxTransactions.Length);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Creditor1.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), invoice.AH_InvoiceDate, journal.AH_InvoiceDate);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_LocalCurrency()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(capJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 0, exxTransactions.Length);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), new ZDateTime(2023, 02, 24), journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), new ZDateTime(2023, 02, 24), journal1.AH_InvoiceDate);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			Assert(reverser.IsOutstandingAmountOnlyPaidViaCashAdvance);
			reverser.Reverse();
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			reverser.ReverseTransaction.PostDate = new ZDateTime(2023, 02, 27);
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);
			AssertEquals(new ZDateTime(2023, 02, 27), reversingCrd.AH_PostDate);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 1, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
			AssertEquals(nameof(journal2.AH_PostDate), new ZDateTime(2023, 02, 27), journal2.AH_PostDate);
			AssertEquals(nameof(journal2.AH_InvoiceDate), new ZDateTime(2023, 02, 24), journal2.AH_InvoiceDate);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_LocalCurrency_AmountOverpaid()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);
			job.JH_JobNum = "S0001234";

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			charge1.JR_OSCostAmt = 80m;
			charge2.JR_OSCostAmt = 270m;
			Factory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(capJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("Fully Paid", 0M, invoice.AH_OutstandingAmount);
			AssertEquals("Paid Date is NOT Empty", invoiceDate, invoice.AH_FullyPaidDate.Date);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 0, exxTransactions.Length);

			var journals = GetCAIJournals(hasTransactionBelongsToGroup: false);
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal = journals[0];
			AssertEquals(nameof(journal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal.AH_TransactionCategory);
			AssertEquals(nameof(journal.AH_OH), ObjectCreator.Creditor1.PK, journal.AH_OH);
			AssertEquals(nameof(journal.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal.AH_Desc);
			AssertEquals(nameof(journal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal.DebitCreditSign), "CR", journal.DebitCreditSign);
			AssertEquals(nameof(journal.AH_OSExTaxAmount), 750m, journal.AH_OSExTaxAmount);
			AssertEquals(nameof(journal.AH_LocalExTaxAmount), 750m, journal.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal.AH_AG), ObjectCreator.GLHeader1.PK, journal.AH_AG);
			AssertEquals(nameof(journal.AH_PostDate), invoice.AH_PostDate, journal.AH_PostDate);
			AssertEquals(nameof(journal.AH_InvoiceDate), invoice.AH_InvoiceDate, journal.AH_InvoiceDate);

			var overpaymentJournals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("Overpayment Journal Count", 1, overpaymentJournals.Length);
			var overpaymentJournal = overpaymentJournals[0];
			AssertEquals(nameof(overpaymentJournal.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, overpaymentJournal.AH_TransactionCategory);
			AssertEquals(nameof(overpaymentJournal.AH_OH), ObjectCreator.Creditor1.PK, overpaymentJournal.AH_OH);
			AssertEquals(nameof(overpaymentJournal.AH_Desc), "OVERPAYMENT OF ADVANCE PAYMENT 00001000 / JOB S0001234", overpaymentJournal.AH_Desc);
			AssertEquals(nameof(overpaymentJournal.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, overpaymentJournal.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(overpaymentJournal.AH_OSExTaxAmount), 50m, overpaymentJournal.AH_OSExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_LocalExTaxAmount), 50m, overpaymentJournal.AH_LocalExTaxAmount);
			AssertEquals(nameof(overpaymentJournal.AH_AG), ObjectCreator.GLHeader1.PK, overpaymentJournal.AH_AG);
			AssertEquals(nameof(overpaymentJournal.AH_PostDate), invoice.AH_PostDate, overpaymentJournal.AH_PostDate);
			AssertEquals(nameof(overpaymentJournal.AH_InvoiceDate), invoice.AH_InvoiceDate, overpaymentJournal.AH_InvoiceDate);
			AssertEquals(nameof(overpaymentJournal.AH_FullyPaidDate), ZDateTime.Empty, overpaymentJournal.AH_FullyPaidDate);
			AssertEquals(invoice.PK, overpaymentJournal.AH_TransactionBelongsToGroup);

			//The following test the reverse process
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			Assert(reverser.IsOutstandingAmountOnlyPaidViaCashAdvance);
			reverser.Reverse();
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 3, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), 750m, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), 750m, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);

			var journal3 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == overpaymentJournal.PK);
			AssertNotNull(journal3);
			AssertEquals(nameof(journal3.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal3.AH_TransactionCategory);
			AssertEquals(nameof(journal3.AH_OH), ObjectCreator.Creditor1.PK, journal3.AH_OH);
			AssertEquals(nameof(journal3.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {overpaymentJournal.AH_TransactionNum}"), journal3.AH_Desc);
			AssertEquals(nameof(journal3.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal3.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal3.DebitCreditSign), "DR", journal3.DebitCreditSign);
			AssertEquals(nameof(journal3.AH_OSExTaxAmount), 50m, journal3.AH_OSExTaxAmount);
			AssertEquals(nameof(journal3.AH_LocalExTaxAmount), 50m, journal3.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal3.AH_AG), ObjectCreator.GLHeader1.PK, journal3.AH_AG);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_LocalCurrency_hasOutstandingAmount()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			charge3.JR_IsAPCashAdvance = false;
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var capJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAPJournalFactory = new TestObjectCreator(capJournalFactory);

			var cah1 = capJournalFactory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = capJournalFactory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = objectCreatorForCAPJournalFactory.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			capJournalFactory.Save();

			var caiJournalFactory = new BusinessObjectFactory();
			var objectCreatorForCAIJournalFactory = new TestObjectCreator(capJournalFactory);
			var reloadedJob = caiJournalFactory.Load<Job>(job.PK);
			var transactionHashTable = objectCreatorForCAIJournalFactory.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			caiJournalFactory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("NOT Fully Paid", -400M, invoice.AH_OutstandingAmount);
			AssertEquals("Paid Date is Empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			var exxTransactions = GetEXXTransactions();
			AssertEquals("An EXX transaction should exist", 0, exxTransactions.Length);

			var journals = GetCAIJournals();
			AssertEquals("CAI Journal Count", 1, journals.Length);

			var journal1 = journals[0];
			AssertEquals(nameof(journal1.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal1.AH_TransactionCategory);
			AssertEquals(nameof(journal1.AH_OH), ObjectCreator.Creditor1.PK, journal1.AH_OH);
			AssertEquals(nameof(journal1.AH_Desc), FormattableString.Invariant($"Invoice received for charges prepaid on Advance Payment Request {cah1.CAH_RequestReferenceNumber} by AP Payment {payment1.AH_TransactionNum}/{payment1.AH_ChequeOrReference}"), journal1.AH_Desc);
			AssertEquals(nameof(journal1.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal1.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal1.DebitCreditSign), "CR", journal1.DebitCreditSign);
			AssertEquals(nameof(journal1.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal1.AH_OSExTaxAmount);
			AssertEquals(nameof(journal1.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal1.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal1.AH_AG), ObjectCreator.GLHeader1.PK, journal1.AH_AG);
			AssertEquals(nameof(journal1.AH_PostDate), invoice.AH_PostDate, journal1.AH_PostDate);
			AssertEquals(nameof(journal1.AH_InvoiceDate), invoice.AH_InvoiceDate, journal1.AH_InvoiceDate);

			//The following test the reverse process
			AssertEquals(-400m, invoice.AH_OutstandingAmount);
			Assert(invoice.AH_FullyPaidDate.IsEmpty);
			Assert(!invoice.IsReversed);
			var reverser = new APInvoiceReversing(invoice as APInvoice);
			reverser.Reverse();
			reverser.ReverseTransaction.TransactionNumber = "ABC12345";
			invoice.Factory.Save();
			Assert(invoice.IsReversed);
			Assert(!invoice.AH_FullyPaidDate.IsEmpty);
			AssertEquals(0m, invoice.AH_OutstandingAmount);
			var reversingCrd = invoice.ReverseTransaction as CreditNote;
			AssertNotNull(reversingCrd);

			journals = GetCAIJournals(hasTransactionBelongsToGroup: true);
			AssertEquals("CAI Journal Count", 1, journals.Length);
			journals.ForEach(x => Assert(x.IsReversed));

			var journal2 = journals.FirstOrDefault(x => x.AH_TransactionBelongsToGroup == journal1.PK);
			AssertNotNull(journal2);
			AssertEquals(nameof(journal2.AH_TransactionCategory), TransactionCategory.Codes.CashAdvanceInvoice, journal2.AH_TransactionCategory);
			AssertEquals(nameof(journal2.AH_OH), ObjectCreator.Creditor1.PK, journal2.AH_OH);
			AssertEquals(nameof(journal2.AH_Desc), FormattableString.Invariant($"REVERSAL RELATED TO {journal1.AH_TransactionNum}"), journal2.AH_Desc);
			AssertEquals(nameof(journal2.AH_RX_NKTransactionCurrency), cah1.CAH_RX_NKTransactionCurrency, journal2.AH_RX_NKTransactionCurrency);
			AssertEquals(nameof(journal2.DebitCreditSign), "DR", journal2.DebitCreditSign);
			AssertEquals(nameof(journal2.AH_OSExTaxAmount), cah1.CAH_OSPaidAmount, journal2.AH_OSExTaxAmount);
			AssertEquals(nameof(journal2.AH_LocalExTaxAmount), cah1.CAH_LocalPaidAmount, journal2.AH_LocalExTaxAmount);
			AssertEquals(nameof(journal2.AH_AG), ObjectCreator.GLHeader1.PK, journal2.AH_AG);
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[TestDate(2023, 02, 24, 0, 0, 0)]
		public void TestAPInvoiceVisit_UpdateToInvoicedStatusViaCAIJournal_CanNotReverseWhenInvoiceMatchedWithNonCashAdvanceTransaction()
		{
			AccountingConfigurationRegistry.Instance.PayablesCashAdvanceClearingAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ObjectCreator.GLHeader1.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var invoiceNum = "APINV001";
			var invoiceDate = TestDateAttribute.Date;
			var job = ObjectCreator.CreateJob(ObjectCreator.LocalClient, 1.0M, ObjectCreator.Agent, 1.0M);

			var charge1 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 01"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 100M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 100M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge1, invoiceNum, invoiceDate);

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "Charge 02"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 300M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 300M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge2, invoiceNum, invoiceDate);

			Factory.Save();

			new APCashAdvanceRequestor(charge1).GenerateRequest();
			Factory.Save();

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "Charge 03"
													, costCurrency: ObjectCreator.AUD, osCostAmt: 400M, creditor: ObjectCreator.Creditor1
													, sellCurrency: ObjectCreator.AUD, osSellAmt: 400M, debtor: objectCreator.Debtor);
			SetupChargeForAPCashAdvance(charge3, invoiceNum, invoiceDate);
			charge3.JR_IsAPCashAdvance = false;
			Factory.Save();

			var sortedCAHs = job.CashAdvanceRequests.Cast<AccCashAdvanceRequestHeader>()
								.OrderBy(x => x.CAH_OSAmount)
								.ToList();

			var cah1 = Factory.Load<CashAdvanceRequestHeader>(sortedCAHs[0].PK);
			new CashAdvanceReceiptOrPaymentJournalCreator(ZDateTime.Today).Generate(cah1);
			var mj1 = Factory.LoadTop1<APJournal>(new ZQuery(AccTransactionHeaderSchema.AH_CAH_CashAdvanceRequestHeader, cah1.PK));
			AssertNotNull(mj1);
			var payment1 = ObjectCreator.CreateAndMatchAPPaymentForCashAdvanceMatchingJournal(mj1, matchGroupNumber: "M0001");
			cah1.MarkAsPaid(true);
			Factory.Save();

			var reloadedJob = Factory.Load<Job>(job.PK);
			var transactionHashTable = ObjectCreator.PostJobAsBillingTab(reloadedJob, JobInvoicingPostingOption.Costs);
			var invoices = transactionHashTable.GetAllAPInvoicesAndCreditNotes();
			Factory.Save();

			AssertEquals(CashAdvanceStatusCodes.RequestHeader.Invoiced, cah1.CAH_Status);
			AssertEquals("Posted AP Invoice", 1, invoices.Length);

			var invoice = invoices.First();
			invoice.Reload();
			AssertEquals("NOT Fully Paid", -400M, invoice.AH_OutstandingAmount);
			AssertEquals("Paid Date is Empty", ZDateTime.Empty, invoice.AH_FullyPaidDate);

			//Match Invoice outstanding amount with another non cash advance journal
			var anotherJournal = ObjectCreator.CreateJournal<APJournal>(-400M, invoice.AH_PostDate, objectCreator.Debtor.PK);
			anotherJournal.DebitCreditSign = DebitCreditDataEntry.DR;
			Factory.Save();

			var matching = new APMatchingBase(Factory);
			matching.PrimaryOrganization = ObjectCreator.Creditor1.PK;
			var transactionsToMatch = new Dictionary<BusinessObject, ZDecimal>
			{
				{ invoice, invoice.AH_OutstandingAmount },
				{ anotherJournal, anotherJournal.AH_OSTotal }
			};
			matching.MoveFromUnmatchToMatch(transactionsToMatch);
			matching.MatchAndClearTransactions();

			Factory.Save();

			var reverser = new APInvoiceReversing(invoice as APInvoice);
			AssertEquals("Should not be able to reverse transaction", false, reverser.CanReverseTransaction);
			AssertEquals("This transaction cannot be reversed because it has been matched with other transactions.", reverser.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		[SkipReportingWhenReversedWIPACRLinkedToJobCharge]
		[ExpectNoExceptions]
		public void TestReverseNonJobRelatedInvoice_NoExceptionThrown()
		{
			AccountingConfigurationRegistry.Instance.EnablePayablesCashAdvanceFunctionality.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.AllowManualSettingOfPayablesCashAdvanceRequestStatusToPaid.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var apInvoice = ObjectCreator.CreateAPInvoice<APInvoice>("111", ObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, ObjectCreator.AALSHI);
			Factory.Save();
			var reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(apInvoice);
			reversing.Reverse();
			Assert(apInvoice.IsReversed);
		}

		APInvoice GetAPInvoice(params Charge[] charges)
		{
			var invoice = ObjectCreator.CreateInvoice(typeof(APInvoice), currency: ObjectCreator.AUD, organisation: ObjectCreator.Creditor1) as APInvoice;
			foreach (var charge in charges)
			{
				var line = ObjectCreator.CreateAPInvoiceLine(invoice, charge.Job as Job, charge.ChargeCode, charge.CostCurrency, 1.0M, "DESC01", charge.JR_OSCostAmt);
				charge.Accrual.AL_ReverseDate = ZDateTime.Today;
				charge.JR_AL_APLine = line.PK;
			}
			return invoice;
		}

		APJournal[] GetCAIJournals(bool? hasTransactionBelongsToGroup = false)
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.Journal);
			query.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsPayable);
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, TransactionCategory.Codes.CashAdvanceInvoice);
			if (hasTransactionBelongsToGroup.HasValue)
			{
				if (hasTransactionBelongsToGroup.Value)
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.NotEqual, null);
				}
				else
				{
					query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.Equal, null);
				}
			}
			return Factory.Load<APJournal>(query);
		}

		ExchangeDifference[] GetEXXTransactions()
		{
			var query = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, TransactionTypes.ExchangeDifference);
			return Factory.Load<APExchangeDifference>(query);
		}

		void SetupChargeForAPCashAdvance(Charge charge, ZString invoiceNum, ZDateTime invDate)
		{
			charge.JR_AT_CostGSTRate = ObjectCreator.GSTFREE1.PK;
			charge.JR_APInvoiceNum = invoiceNum;
			charge.JR_APInvoiceDate = invDate;
			charge.JR_IsAPCashAdvance = true;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var periodHelper = new AccountingPeriodTestHelper();
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year - 1);
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year);
			periodHelper.PostPeriodsForEntireYear(ZDateTime.Today.Year + 1);
		}

		TestObjectCreator ObjectCreator => objectCreator ?? (objectCreator = new TestObjectCreator(Factory));
		TestObjectCreator objectCreator;
	}
}
