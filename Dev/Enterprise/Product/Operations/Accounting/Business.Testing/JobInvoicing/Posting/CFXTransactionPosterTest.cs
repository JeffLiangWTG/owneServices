using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	public class CFXTransactionPosterTest : TestCaseWithFactory
	{
		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCFXTransactionCreated()
		{
			ZDateTime now = ZDateTime.Now;

			TestObjectCreator creator = new TestObjectCreator(Factory);

			Job job = creator.CreateJob("Z00001000", creator.LocalClient, 5M, creator.Agent, 10M);
			ExchangeRate rate1 = creator.CreateExchangeRate(job, creator.USD, .7M);

			Charge charge1 = creator.CreateCharge(job, creator.CC1, "Charge Code 1", creator.AUD, 100M, creator.Creditor1, creator.AUD, 150M, creator.LocalClient);
			Charge charge4 = creator.CreateCharge(job, creator.CC4, "Charge Code 4", null, 0M, null, creator.USD, 500M, creator.LocalClient);
			Charge charge5 = creator.CreateCharge(job, creator.CC5, "Charge Code 5", creator.GBP, 100M, creator.Creditor1, creator.AUD, 300M, creator.Agent);
			Charge charge6 = creator.CreateCharge(job, creator.CC6, "Charge Code 6", creator.AUD, 100M, creator.Creditor1, creator.USD, 100M, creator.Agent);

			Factory.Save();

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("2 sets of charges - one for local client, one for Agent", 2, distributedCharges.Count);

			ChargePoster poster = new ChargePoster(Factory);
			foreach (IReceivablesPostingChargeCollection postingCharges in distributedCharges)
			{
				poster.Post(postingCharges);
			}

			#region Job Costing Journal

			InvoicingBase[] localClientInvoices = poster.GetInvoices(creator.AUD, creator.LocalClient);
			AssertEquals(1, localClientInvoices.Length);

			IReceivablesPostingChargeCollection localClientCharges = distributedCharges.GetCharges(creator.LocalClient, creator.AUD);
			JCJournalHeader cFXJournal = localClientCharges.CFXJournal;
			AssertEquals("CFX Line Count", 1, cFXJournal.Lines.Count);

			AssertTransactionHeaderValues(cFXJournal, "JC", "JNL", null, "Job Costing Journal (CFX)", now, now,
				0M, 0M, 0M, 0M, creator.AUD, 1, now, ZBool.False, null, null, "", 0M, "", 0, ZString.Empty, ZString.Empty, null, ZBool.False, ZDateTime.Empty, ZBool.False);
			AssertTransactionHeaderDefaults(cFXJournal);

			//AccGLHeader GLHeader = (AccGLHeader)Factory.Load(typeof(AccGLHeader), new ZGuid(AccountingConfigurationRegistry.Instance.CFXAccount.Value));
			TransactionLine cFXLine1 = cFXJournal.FindTransactionLine("REV", creator.CC4, job.PK);
			AssertTransactionLineValues(cFXLine1, "REV", 1, "Charge Code 4", -37.59M, null, 0M, null, 0M, -37.59M, creator.AUD, 1, now,
				ZBool.False, cFXJournal, job, creator.CC4, null);
			AssertTransactionLineDefaults(cFXLine1);

			InvoicingBase[] agentInvoices = poster.GetInvoices(creator.AUD, creator.Agent);
			AssertEquals(1, agentInvoices.Length);

			IReceivablesPostingChargeCollection agentCharges = distributedCharges.GetCharges(creator.Agent, creator.AUD);
			ARInvoice agentInvoice = (ARInvoice)agentCharges.PostedInvoice;
			cFXJournal = agentCharges.CFXJournal;
			AssertEquals("CFX Line Count", 1, cFXJournal.Lines.Count);

			AssertTransactionHeaderValues(cFXJournal, "JC", "JNL", null, "Job Costing Journal (CFX)", now, now,
				0M, 0M, 0M, 0M, creator.AUD, 1, now, ZBool.False, null, null, "", 0M, "", 0, ZString.Empty, ZString.Empty, null, ZBool.False, ZDateTime.Empty, ZBool.False);
			AssertTransactionHeaderDefaults(cFXJournal);

			cFXLine1 = cFXJournal.FindTransactionLine("REV", creator.CC6, job.PK);
			AssertTransactionLineValues(cFXLine1, "REV", 1, "Charge Code 6", -15.87M, null, 0M, null, 0M, -15.87M, creator.AUD, 1, now,
				ZBool.False, cFXJournal, job, creator.CC6, null);
			AssertTransactionLineDefaults(cFXLine1);

			#endregion
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateOneCFXJournalPerInvoice()
		{
			Job job = Creator.CreateJob("Z00001000", Creator.LocalClient, 5M, Creator.Agent, 10M);
			ExchangeRate rate1 = Creator.CreateExchangeRate(job, Creator.USD, .7M);

			Charge charge1 = Creator.CreateCharge(job, Creator.CC1, "Charge Code 1", Creator.AUD, 100M, Creator.Creditor1, Creator.AUD, 150M, Creator.LocalClient);
			Charge charge4 = Creator.CreateCharge(job, Creator.CC4, "Charge Code 4", null, 0M, null, Creator.USD, 500M, Creator.LocalClient);
			Charge charge5 = Creator.CreateCharge(job, Creator.CC5, "Charge Code 5", Creator.GBP, 100M, Creator.Creditor1, Creator.USD, 300M, Creator.Agent);

			Factory.Save();

			PostingChargeCollection distributedCharges = new PostingChargeDistributor().DistributeCharges(job.ReceivableCharges);
			AssertEquals("2 sets of charges - one for local client, one for Agent", 2, distributedCharges.Count);

			ChargePoster poster = new ChargePoster(Factory);
			foreach (IReceivablesPostingChargeCollection postingCharges in distributedCharges)
			{
				poster.Post(postingCharges);
			}

			JCJournalHeader localClientCFX = distributedCharges.GetCharges(Creator.LocalClient, Creator.AUD).CFXJournal;
			JCJournalHeader agentCFX = distributedCharges.GetCharges(Creator.Agent, Creator.USD).CFXJournal;
			AssertNotNull(localClientCFX);
			AssertNotNull(agentCFX);

			InvoicingBase[] invoice1s = poster.GetInvoices(Creator.AUD, Creator.LocalClient);
			AssertEquals(1, invoice1s.Length);

			InvoicingBase[] invoice2s = poster.GetInvoices(Creator.AUD, Creator.Agent);
			AssertEquals(1, invoice2s.Length);

			AssertEquals("Should only be one line per journal", 1, localClientCFX.Lines.Count);
			AssertEquals("Should only be one line per journal", 1, agentCFX.Lines.Count);
		}

		#region Implementation

		TestObjectCreator Creator;

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
		}

		void AssertTransactionHeaderValues(AccTransactionHeader header, string ledger, string type, string number, string description, ZDateTime invoiceDate,
			ZDateTime dueDate, decimal invoiceAmount, decimal gSTAmount, decimal wHTAmount, decimal oSTotal, RefCurrency currency, decimal exchangeRate, ZDateTime postDate,
			bool isDisbursment, OrgHeader client, JobHeader job, string consolidatedInvoiceRef, decimal outstandingAmount, string invoiceTerm, int invoiceTermDays,
			string chequeOrReference, string receiptType, AccBankAccount bankAccount, bool invoiceApproved, ZDateTime fullyPaidDate, bool paymentApproved)
		{
			AssertEquals("AH_Ledger", ledger, header.AH_Ledger);
			AssertEquals("AH_TransactionType", type, header.AH_TransactionType);
			if (header is APInvoice)
			{
				AssertEquals("AH_TransactionNum", number, header.AH_TransactionNum);
			}
			AssertEquals("AH_Desc", description.ToUpper(), header.AH_Desc.ToUpper());
			AssertZDatesWithin5Minutes("AH_InvoiceDate", invoiceDate, header.AH_InvoiceDate);
			AssertZDatesWithin5Minutes("AH_DueDate", dueDate, header.AH_DueDate);
			AssertEquals("AH_InvoiceAmount", invoiceAmount, header.AH_InvoiceAmount);
			AssertEquals("AH_GSTAmount", gSTAmount, header.AH_GSTAmount);
			AssertEquals("AH_WithholdingTax", wHTAmount, header.AH_WithholdingTax);
			AssertEquals("AH_OSTotal", oSTotal, header.AH_OSTotal);
			AssertEquals("AH_RX_NKTransactionCurrency", currency.RX_Code, header.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_ExchangeRate", exchangeRate, header.AH_ExchangeRate);
			AssertZDatesWithin5Minutes("AH_PostDate", postDate, header.AH_PostDate);
			//AssertEquals("AH_IsDisbursement", IsDisbursment, Header.AH_IsDisbursement);
			AssertEquals("AH_OH", client != null ? client.PK : ZGuid.Empty, header.AH_OH);
			AssertEquals("AH_JH", job != null ? job.PK : ZGuid.Empty, header.AH_JH);
			AssertEquals("AH_ConsolidatedInvoiceRef", consolidatedInvoiceRef, header.AH_ConsolidatedInvoiceRef);
			AssertEquals("AH_OutstandingAmount", outstandingAmount, header.AH_OutstandingAmount);
			AssertEquals("AH_InvoiceTerm", invoiceTerm, header.AH_InvoiceTerm);
			AssertEquals("AH_InvoiceTermDays", invoiceTermDays, header.AH_InvoiceTermDays);
			AssertEquals("AH_ChequeOrReference", chequeOrReference, header.AH_ChequeOrReference);
			AssertEquals("AH_ReceiptType", receiptType, header.AH_ReceiptType);
			AssertEquals("AH_AB", bankAccount != null ? bankAccount.PK : ZGuid.Empty, header.AH_AB);
			AssertEquals("AH_InvoiceApproved", invoiceApproved, header.AH_InvoiceApproved);
			AssertZDatesWithin5Minutes("AH_FullyPaidDate", fullyPaidDate, header.AH_FullyPaidDate);
		}

		void AssertTransactionHeaderDefaults(AccTransactionHeader header)
		{
			AssertTransactionHeaderDefaults(header, false);
		}

		void AssertTransactionHeaderDefaults(AccTransactionHeader header, bool isAgentConsolInvoice)
		{
			AssertEquals("AH_TransactionCount", (byte)1, header.AH_TransactionCount);
			AssertEquals("AH_TransactionReference", "", header.AH_TransactionReference);
			AssertEquals("AH_TransactionCategory", "", header.AH_TransactionCategory);
			AssertEquals("AH_AgePeriod", 0, header.AH_AgePeriod);
			AssertEquals("AH_PostPeriod", 0, header.AH_PostPeriod);
			AssertEquals("AH_CashBasisGSTIndicator", ZBool.False, header.AH_CashBasisGSTIndicator);
			AssertEquals("AH_CashBasisGSTRealisedToGL", ZBool.False, header.AH_CashBasisGSTRealisedToGL);
			AssertEquals("AH_ChequeDrawer", "", header.AH_ChequeDrawer);
			AssertEquals("AH_DrawerBank", "", header.AH_DrawerBank);
			AssertEquals("AH_DrawerBranch", "", header.AH_DrawerBranch);
			AssertEquals("AH_GB", GlbBranch.CurrentBranch.PK, header.AH_GB);
			if (!isAgentConsolInvoice)
			{
				AssertEquals("AH_GE", GlbDepartment.CurrentDepartment.PK, header.AH_GE);
			}
			AssertEquals("AH_AG", ZGuid.Empty, header.AH_AG);

			AssertEquals("AH_InvoicePrinted", ZBool.False, header.AH_InvoicePrinted);
			AssertEquals("AH_IsCancelled", ZBool.False, header.AH_IsCancelled);
			//AssertEquals("AH_IsClearedInCashbook", ZBool.False, Header.AH_IsClearedInCashbook);
			AssertEquals("AH_DateClearedInCashbook", ZDateTime.Empty, header.AH_DateClearedInCashbook);
			AssertEquals("AH_NotAllocated", ZBool.False, header.AH_NotAllocated);

			AssertEquals("AH_POST1", ZBool.False, header.AH_POST1);
			AssertEquals("AH_POST2", ZBool.False, header.AH_POST2);
			AssertEquals("AH_POST3", ZBool.False, header.AH_POST3);
			AssertEquals("AH_POST4", ZBool.False, header.AH_POST4);
			AssertEquals("AH_PostedToEFT", ZBool.False, header.AH_PostedToEFT);
			AssertEquals("AH_PostToGL", "N", header.AH_PostToGL);
			AssertEquals("AH_ReceiptBatchNo", "", header.AH_ReceiptBatchNo);
			AssertEquals("AH_TransactionBelongsToGroup", ZGuid.Empty, header.AH_TransactionBelongsToGroup);
		}

		void AssertTransactionLineValues(AccTransactionLines line, string type, int sequence, string description, decimal lineAmount, AccTaxRate gST, decimal gSTAmount, AccWithholding wHT, decimal wHTAmount, decimal oSAmount, RefCurrency currency, decimal exchangeRate, ZDateTime postDate, ZBool preventInvoicePrintGrouping, AccTransactionHeader transactionHeader, JobHeader job, AccChargeCode chargeCode, OrgHeader client)
		{
			AssertEquals("AL_LineType", type, line.AL_LineType);
			AssertEquals("AL_Sequence", sequence, line.AL_Sequence);
			AssertEquals("AL_Desc", description, line.AL_Desc);
			AssertEquals("AL_LineAmount", lineAmount, line.AL_LineAmount);
			AssertEquals("AL_AT", gST != null ? gST.PK : ZGuid.Empty, line.AL_AT);
			AssertEquals("AL_GSTVAT", gSTAmount, line.AL_GSTVAT);
			AssertEquals("AL_AW", wHT != null ? wHT.PK : ZGuid.Empty, line.AL_AW);
			AssertEquals("AL_WithholdingTax", wHTAmount, line.AL_WithholdingTax);
			AssertEquals("AL_OSAmount", oSAmount, line.AL_OSAmount);
			AssertEquals("AL_RX_NKTransactionCurrency", currency.RX_Code, line.AL_RX_NKTransactionCurrency);
			AssertEquals("AL_ExchangeRate", exchangeRate, line.AL_ExchangeRate);
			AssertZDatesWithin5Minutes("AL_PostDate", postDate, line.AL_PostDate);
			AssertEquals("AL_PreventInvoicePrintGrouping", preventInvoicePrintGrouping, line.AL_PreventInvoicePrintGrouping);
			AssertEquals("AL_AH", transactionHeader.PK, line.AL_AH);
			AssertEquals("AL_JH", job.PK, line.AL_JH);
			AssertEquals("AL_AC", chargeCode.PK, line.AL_AC);
			AssertNotEquals("AL_AG", ZGuid.Empty, line.AL_AG);
			AssertEquals("AL_OH", client != null ? client.PK : ZGuid.Empty, line.AL_OH);
		}

		void AssertTransactionLineDefaults(AccTransactionLines line)
		{
			AssertEquals("AL_UnitQty", 0, line.AL_UnitQty);
			AssertEquals("AL_UnitPrice", 0M, line.AL_UnitPrice);
			AssertEquals("AL_OSUnitPrice", 0M, line.AL_OSUnitPrice);
			AssertEquals("AL_PostPeriod", 0, line.AL_PostPeriod);
			AssertEquals("AL_PostToGL", "N", line.AL_PostToGL);
			AssertEquals("AL_ReversePeriod", 0, line.AL_ReversePeriod);
			AssertZDatesWithin5Minutes("AL_ReverseDate", ZDateTime.Empty, line.AL_ReverseDate);
			AssertEquals("AL_ReverseToGL", "N", line.AL_ReverseToGL);
			AssertEquals("AL_GE", GlbDepartment.CurrentDepartment.PK, line.AL_GE);
			AssertEquals("AL_GB", GlbBranch.CurrentBranch.PK, line.AL_GB);
			AssertEquals("AL_AG_PercentOf", ZGuid.Empty, line.AL_AG_PercentOf);
			AssertEquals("AL_PercentageOfPeriod", 0, line.AL_PercentageOfPeriod);
		}

		#endregion
	}
}