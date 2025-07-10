using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	class CFXJournalReverser_InnerTest : TestCaseWithFactory
	{
		[NUnit.Framework.TestDate(2012, 1, 15)]
		public void TestReverseJournalUsesReversalTransactionsPostDate()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			LocalClientChargeWithCFX.JR_AC = ChargeCode.PK;
			LocalClientChargeWithCFX.JR_RX_NKSellCurrency = ObjectCreator.USD.RX_Code;
			LocalClientChargeWithCFX.JR_OH_SellAccount = LocalClient.PK;
			LocalClientChargeWithCFX.JR_OSSellAmt = 100.00m;
			LocalClientChargeWithCFX.JR_GE = ObjectCreator.FESDepartment.PK;
			AssertEquals("Precondition - CFX should be set", 8.77m, LocalClientChargeWithCFX.JR_CFXAmt);
			new InvoicingPostManager(Job).CreateTransactions(JobInvoicingPostingOption.All);

			ZQuery cFXFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.JobCosting);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			JCJournalHeader[] cFXJournals = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);
			AssertEquals("Should only be one CFX Journal posted", 1, cFXJournals.Length);
			JCJournalHeader journal = cFXJournals[0];
			AssertEquals(new ZDateTime(2012, 1, 15), journal.AH_PostDate);
			AssertNull("Journal has not be reversed yet", journal.ReverseTransaction);

			ZQuery invoicesFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);
			invoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			invoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			TransactionHeader[] loadedInovices = (TransactionHeader[])Factory.Load(typeof(TransactionHeader), invoicesFilter);
			AssertEquals("Should be 1 invoices created", 1, loadedInovices.Length);
			ARInvoice invoice = (ARInvoice)loadedInovices[0];
			invoice.GenerateReverseTransaction(true);
			AssertNotNull("AR invoice should be reversed", invoice.ReverseTransaction);
			((InvoicingBase)invoice.ReverseTransaction).AH_PostDate = new ZDateTime(2011, 8, 29);

			CFXJournalReverser reverser = new CFXJournalReverser();
			reverser.ReverseJournal(invoice);
			cFXJournals = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);
			AssertEquals("Should be two CFX Journal posted", 2, cFXJournals.Length);
			AssertNotNull("Journal should be reversed", journal.ReverseTransaction);
			AssertEquals("Reverse journal's Post date should be the credit note's post date", new ZDateTime(2011, 8, 29), ((TransactionHeader)journal.ReverseTransaction).AH_PostDate);
		}

		public void TestReverseJournal()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			LocalClientChargeWithCFX.JR_AC = ChargeCode.PK;
			LocalClientChargeWithCFX.JR_RX_NKSellCurrency = ObjectCreator.USD.RX_Code;
			LocalClientChargeWithCFX.JR_OH_SellAccount = LocalClient.PK;
			LocalClientChargeWithCFX.JR_OSSellAmt = 100.00m;
			LocalClientChargeWithCFX.JR_GE = ObjectCreator.FESDepartment.PK;

			AgentChargeWithoutCFX.JR_AC = ChargeCode.PK;
			AgentChargeWithoutCFX.JR_RX_NKSellCurrency = ObjectCreator.USD.RX_Code;
			AgentChargeWithoutCFX.JR_OH_SellAccount = Agent.PK;
			AgentChargeWithoutCFX.JR_OSSellAmt = 100.00m;
			AgentChargeWithoutCFX.JR_GE = ObjectCreator.FESDepartment.PK;

			AssertEquals("Precondition - CFX should be set", 8.77m, LocalClientChargeWithCFX.JR_CFXAmt);
			AssertEquals("Precondition - CFX should be zero - does not apply", 0m, AgentChargeWithoutCFX.JR_CFXAmt);

			new InvoicingPostManager(Job).CreateTransactions(JobInvoicingPostingOption.All);

			ZQuery cFXFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.JobCosting);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			JCJournalHeader[] cFXJournals = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);

			AssertEquals("Should only be one CFX Journal posted", 1, cFXJournals.Length);

			JCJournalHeader cFXJournal = cFXJournals[0];
			AssertEquals("Should only have one line", 1, cFXJournal.Lines.Count);
			AssertEquals("Should be for -8.77", -8.77m, cFXJournal.Lines[0].AL_LineAmount);

			ZQuery invoicesFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);
			invoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			invoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			TransactionHeader[] loadedInovices = (TransactionHeader[])Factory.Load(typeof(TransactionHeader), invoicesFilter);
			AssertEquals("Should be 2 invoices created", 2, loadedInovices.Length);

			ARInvoice localClientInvoice = null;
			ARInvoice agentInvoice = null;

			foreach (InvoicingBase invoice in loadedInovices)
			{
				if (invoice.AH_OH == Agent.PK)
				{
					agentInvoice = (ARInvoice)invoice;
				}
				else if (invoice.AH_OH == LocalClient.PK)
				{
					localClientInvoice = (ARInvoice)invoice;
				}
				else
				{
					Fail("Org was not local client or agent");
				}
			}

			cFXJournal.AH_PostToGL = "Y";
			cFXJournal.Lines[0].AL_PostToGL = "Y";

			CFXJournalReverser reverser = new CFXJournalReverser();
			reverser.ReverseJournal(agentInvoice);

			AssertEquals("CFXJournal shouldnt have been touched as it was for local client", false, cFXJournal.AH_IsCancelled);

			reverser.ReverseJournal(localClientInvoice);
			AssertEquals("CFXJournal should be cancelled", true, cFXJournal.AH_IsCancelled);
			AssertEquals("Values for Journal should still be the same", -8.77m, cFXJournal.Lines[0].AL_LineAmount);

			ZQuery reversingCFXFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.JobCosting);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, cFXJournal.PK);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			JCJournalHeader[] reversingCFXJournals = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);

			AssertEquals("Should only be one reversing journal", 1, reversingCFXJournals.Length);

			JCJournalHeader reversingCFXJournal = reversingCFXJournals[0];

			AssertEquals("Should have AH_TransactionBelongsToGroup Set to original transaction", cFXJournal.PK, reversingCFXJournal.AH_TransactionBelongsToGroup);
			AssertEquals("Should only have one line", 1, reversingCFXJournal.Lines.Count);
			AssertEquals("Values for Journal should be 8.77", 8.77m, reversingCFXJournal.Lines[0].AL_LineAmount);
			AssertEquals("Should not copy GL flags on header or lines", "N", reversingCFXJournal.AH_PostToGL);
			AssertEquals("Should not copy GL flags on header or lines", "N", reversingCFXJournal.Lines[0].AL_PostToGL);

			AssertEquals("Reversing Journal should be cancelled", true, reversingCFXJournal.AH_IsCancelled);
			AssertEquals("Original Journal should be cancelled", true, cFXJournal.AH_IsCancelled);

			AssertNull("Charge CFX Line should be cleared", LocalClientChargeWithCFX.CFXLine);
			AssertNull("Charge CFX Line should not be filled", AgentChargeWithoutCFX.CFXLine);
		}

		public void TestReverseJournalIfNoJobChargeLoaded()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			ARCreditNote aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.Lines.AddNew();

			new CFXJournalReverser().ReverseJournal(aRCreditNote);

			ZQuery cFXFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.JobCosting);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			JCJournalHeader[] reversingCFXJournals = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);
			AssertEquals("Should not have created any reversing journal", 0, reversingCFXJournals.Length);
		}

		public void TestReversingCancelledWhenJournalRelatesToMoreThanOneInvoice() //NOTE: This is old behaviour - from this point on, all CFX journals have a 1:1 relationship with invoices
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			LocalClientChargeWithCFX.JR_AC = ChargeCode.PK;
			LocalClientChargeWithCFX.JR_RX_NKSellCurrency = ObjectCreator.USD.RX_Code;
			LocalClientChargeWithCFX.JR_OH_SellAccount = LocalClient.PK;
			LocalClientChargeWithCFX.JR_OSSellAmt = 100.00m;
			LocalClientChargeWithCFX.JR_GE = ObjectCreator.FESDepartment.PK;

			AgentChargeWithoutCFX.JR_AC = ChargeCode.PK;
			AgentChargeWithoutCFX.JR_RX_NKSellCurrency = ObjectCreator.USD.RX_Code;
			AgentChargeWithoutCFX.JR_OH_SellAccount = Agent.PK;
			AgentChargeWithoutCFX.JR_OSSellAmt = 100.00m;
			AgentChargeWithoutCFX.JR_GE = ObjectCreator.FESDepartment.PK;

			AssertEquals("Precondition - CFX should be set", 8.77m, LocalClientChargeWithCFX.JR_CFXAmt);
			AssertEquals("Precondition - CFX should be zero - does not apply", 0m, AgentChargeWithoutCFX.JR_CFXAmt);

			new InvoicingPostManager(Job).CreateTransactions(JobInvoicingPostingOption.All);

			ZQuery cFXFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.JobCosting);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			JCJournalHeader[] cFXJournals = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);

			AssertEquals("Should only be one CFX Journal posted", 1, cFXJournals.Length);

			JCJournalHeader cFXJournal = cFXJournals[0];
			AssertEquals("Should only have one line", 1, cFXJournal.Lines.Count);
			AssertEquals("Should be for -8.77", -8.77m, cFXJournal.Lines[0].AL_LineAmount);

			JCJournalLine newCFXLineForDifferentInvoice = cFXJournal.Lines.AddNew(); //This makes the CFX journal invalid for reversing

			ZQuery invoicesFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Invoice);
			invoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			invoicesFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			TransactionHeader[] loadedInovices = (TransactionHeader[])Factory.Load(typeof(TransactionHeader), invoicesFilter);
			AssertEquals("Should be 2 invoices created", 2, loadedInovices.Length);

			ARInvoice localClientInvoice = null;
			ARInvoice agentInvoice = null;

			foreach (InvoicingBase invoice in loadedInovices)
			{
				if (invoice.AH_OH == Agent.PK)
				{
					agentInvoice = (ARInvoice)invoice;
				}
				else if (invoice.AH_OH == LocalClient.PK)
				{
					localClientInvoice = (ARInvoice)invoice;
				}
				else
				{
					Fail("Org was not local client or agent");
				}
			}

			new CFXJournalReverser().ReverseJournal(localClientInvoice);

			ZQuery reversingCFXFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.JobCosting);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.PK, SQLComparisonOperator.NotEqual, cFXJournal.PK);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			JCJournalHeader[] reversingCFXJournals = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);

			AssertEquals("Should not have created any reversing journal", 0, reversingCFXJournals.Length);
			Assert("Should not have cancelled original CFX Journal", !cFXJournal.AH_IsCancelled);
		}

		#region Implementation

		Job Job;
		Charge LocalClientChargeWithCFX;
		Charge AgentChargeWithoutCFX;
		OrgHeader LocalClient;
		OrgHeader Agent;
		AccChargeCode ChargeCode;

		protected override void SetUp()
		{
			base.SetUp();
			ObjectCreator = new TestObjectCreator(Factory);
			LocalClient = ObjectCreator.CreateOrgHeader("zub", false, true, false, false, false, false);
			Agent = ObjectCreator.CreateOrgHeader("imr", false, true, false, false, false, false);
			ChargeCode = ObjectCreator.CreateChargeCode("ABC", "ABC ABC", Core.Constants.ChargeType.Margin, 100m, null, null, "ALL");

			Job = ObjectCreator.CreateJob(LocalClient, 5M, Agent, 0m);
			ObjectCreator.CreateExchangeRate(Job, ObjectCreator.USD, 0.6m);

			LocalClientChargeWithCFX = Job.Charges.AddNew();
			AgentChargeWithoutCFX = Job.Charges.AddNew();
		}

		TestObjectCreator ObjectCreator;

		#endregion
	}
}