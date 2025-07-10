using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Netting.Testing
{
	class NettingHelperTest : TestCaseWithFactory
	{
		public void TestMoveUnmatchedTransactions_OneToMany_PartiallyMatchedAtLineLevel()
		{
			var arTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref1", "USD", 600M, NettingTransactionApprovalStatus.Approved);
			var arTransaction1Line1 = testObjectCreator.CreateNettingTransactionLine(arTransaction1, "S1", 200M, "USD");
			var arTransaction1Line2 = testObjectCreator.CreateNettingTransactionLine(arTransaction1, "S1", 300M, "USD");
			var arTransaction1Line3 = testObjectCreator.CreateNettingTransactionLine(arTransaction1, "S1", 100M, "USD");

			var apTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "random ref", "USD", 600M, NettingTransactionApprovalStatus.Approved);
			var apTransaction1Line1 = testObjectCreator.CreateNettingTransactionLine(apTransaction1, "S1", 200M, "USD");

			var apTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "random ref", "USD", 600M, NettingTransactionApprovalStatus.Approved);
			var apTransaction2Line1 = testObjectCreator.CreateNettingTransactionLine(apTransaction2, "S1", 300M, "USD");

			var pivot1 = CreateMatchPivot(period, null, null, arTransaction1Line1, apTransaction1Line1);
			var pivot2 = CreateMatchPivot(period, null, null, arTransaction1Line2, apTransaction2Line1);

			apTransaction1.ApprovalStatus = NettingTransactionApprovalStatus.Matched; //AP transaction1 matched with AR transaction1 Line1
			apTransaction2.ApprovalStatus = NettingTransactionApprovalStatus.Matched; //AP transaction2 matched with AR transaction1 Line2

			Factory.Save();

			NettingHelper.MoveUnmatchedTransactionsToTheNextPeriod(period, Factory, Db.Connection);

			var newFactory = new BusinessObjectFactory();
			arTransaction1 = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);
			apTransaction1 = newFactory.Load<NettingPayableTransaction>(apTransaction1.PK);
			apTransaction2 = newFactory.Load<NettingPayableTransaction>(apTransaction2.PK);

			//transactions are moved to next period
			AssertEquals("Receivable transaction 1 moved to next period", nextPeriod.PK, arTransaction1.NettingPeriodPK);
			AssertEquals("Payable transaction 1 moved to next period", nextPeriod.PK, apTransaction1.NettingPeriodPK);
			AssertEquals("Payable transaction 2 moved to next period", nextPeriod.PK, apTransaction2.NettingPeriodPK);

			arTransaction1Line1 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction1Line1.PK);
			arTransaction1Line2 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction1Line2.PK);
			arTransaction1Line3 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction1Line3.PK);
			apTransaction1Line1 = newFactory.Load<NettingPayableTransactionLine>(apTransaction1Line1.PK);
			apTransaction2Line1 = newFactory.Load<NettingPayableTransactionLine>(apTransaction2Line1.PK);

			//lines are moved to next period
			AssertEquals("Receivable transaction 1 line 1 moved to next period", nextPeriod.PK, arTransaction1Line1.NettingPeriodPK);
			AssertEquals("Receivable transaction 1 line 2 moved to next period", nextPeriod.PK, arTransaction1Line2.NettingPeriodPK);
			AssertEquals("Receivable transaction 1 line 3 moved to next period", nextPeriod.PK, arTransaction1Line3.NettingPeriodPK);
			AssertEquals("Payable transaction 1 line 1 moved to next period", nextPeriod.PK, apTransaction1Line1.NettingPeriodPK);
			AssertEquals("Payable transaction 2 line 1 moved to next period", nextPeriod.PK, apTransaction2Line1.NettingPeriodPK);

			pivot1 = newFactory.Load<NettingMatchPivot>(pivot1.PK);
			pivot2 = newFactory.Load<NettingMatchPivot>(pivot2.PK);

			//matching pivots are moved to next period
			AssertEquals("Pivot 1 moved to next period", nextPeriod.PK, pivot1.NMP_NSP_Period);
			AssertEquals("Pivot 2 moved to next period", nextPeriod.PK, pivot2.NMP_NSP_Period);
		}

		public void TestMoveUnmatchedTransactions_ManyToOne_PartiallyMatchedAtLineLevel()
		{
			var arTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref1", "USD", 200M, NettingTransactionApprovalStatus.Approved);
			var arTransaction1Line1 = testObjectCreator.CreateNettingTransactionLine(arTransaction1, "S1", 200M, "USD");

			var arTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref2", "USD", 300M, NettingTransactionApprovalStatus.Approved);
			var arTransaction2Line1 = testObjectCreator.CreateNettingTransactionLine(arTransaction2, "S2", 300M, "USD");

			var apTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "random ref", "USD", 600M, NettingTransactionApprovalStatus.Approved);
			var apTransaction1Line1 = testObjectCreator.CreateNettingTransactionLine(apTransaction1, "S1", 200M, "USD");
			var apTransaction1Line2 = testObjectCreator.CreateNettingTransactionLine(apTransaction1, "S1", 300M, "USD");
			var apTransaction1Line3 = testObjectCreator.CreateNettingTransactionLine(apTransaction1, "S1", 100M, "USD");

			var pivot1 = CreateMatchPivot(period, null, null, arTransaction1Line1, apTransaction1Line1);
			var pivot2 = CreateMatchPivot(period, null, null, arTransaction2Line1, apTransaction1Line2);

			arTransaction1.ApprovalStatus = NettingTransactionApprovalStatus.Matched; //AR transaction1 matched with AP transaction1 Line1
			arTransaction2.ApprovalStatus = NettingTransactionApprovalStatus.Matched; //AR transaction2 matched with AP transaction1 Line2

			Factory.Save();

			NettingHelper.MoveUnmatchedTransactionsToTheNextPeriod(period, Factory, Db.Connection);

			var newFactory = new BusinessObjectFactory();
			arTransaction1 = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);
			arTransaction2 = newFactory.Load<NettingReceivableTransaction>(arTransaction2.PK);
			apTransaction1 = newFactory.Load<NettingPayableTransaction>(apTransaction1.PK);

			//transactions are moved to next period
			AssertEquals("Receivable transaction 1 moved to next period", nextPeriod.PK, arTransaction1.NettingPeriodPK);
			AssertEquals("Receivable transaction 2 moved to next period", nextPeriod.PK, arTransaction2.NettingPeriodPK);
			AssertEquals("Payable transaction 1 moved to next period", nextPeriod.PK, apTransaction1.NettingPeriodPK);

			arTransaction1Line1 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction1Line1.PK);
			arTransaction2Line1 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction2Line1.PK);
			apTransaction1Line1 = newFactory.Load<NettingPayableTransactionLine>(apTransaction1Line1.PK);
			apTransaction1Line2 = newFactory.Load<NettingPayableTransactionLine>(apTransaction1Line2.PK);
			apTransaction1Line3 = newFactory.Load<NettingPayableTransactionLine>(apTransaction1Line3.PK);

			//lines are moved to next period
			AssertEquals("Receivable transaction 1 line 1 moved to next period", nextPeriod.PK, arTransaction1Line1.NettingPeriodPK);
			AssertEquals("Receivable transaction 2 line 1 moved to next period", nextPeriod.PK, arTransaction2Line1.NettingPeriodPK);
			AssertEquals("Payable transaction 1 line 1 moved to next period", nextPeriod.PK, apTransaction1Line1.NettingPeriodPK);
			AssertEquals("Payable transaction 1 line 2 moved to next period", nextPeriod.PK, apTransaction1Line2.NettingPeriodPK);
			AssertEquals("Payable transaction 1 line 3 moved to next period", nextPeriod.PK, apTransaction1Line3.NettingPeriodPK);

			pivot1 = newFactory.Load<NettingMatchPivot>(pivot1.PK);
			pivot2 = newFactory.Load<NettingMatchPivot>(pivot2.PK);

			//matching pivots are moved to next period
			AssertEquals("Pivot 1 moved to next period", nextPeriod.PK, pivot1.NMP_NSP_Period);
			AssertEquals("Pivot 2 moved to next period", nextPeriod.PK, pivot2.NMP_NSP_Period);
		}

		public void TestMoveUnmatchedTransactions()
		{
			var arTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref1", "USD", 200M, NettingTransactionApprovalStatus.Approved);
			var arTransaction1Line1 = testObjectCreator.CreateNettingTransactionLine(arTransaction1, "S1", 200M, "USD");

			var arTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref2", "USD", 300M, NettingTransactionApprovalStatus.Added);
			var arTransaction2Line1 = testObjectCreator.CreateNettingTransactionLine(arTransaction2, "S2", 300M, "USD");

			var arTransaction3 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref3", "USD", 100M, NettingTransactionApprovalStatus.Approved);

			var apTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "ref1", "USD", 500M, NettingTransactionApprovalStatus.Disputed);
			var apTransaction1Line1 = testObjectCreator.CreateNettingTransactionLine(apTransaction1, "S1", 200M, "USD");
			var apTransaction1Line2 = testObjectCreator.CreateNettingTransactionLine(apTransaction1, "S1", 300M, "USD");

			var apTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "ref1", "USD", 100M, NettingTransactionApprovalStatus.Approved);

			var pivot1 = CreateMatchPivot(period, arTransaction3, apTransaction2, null, null);

			arTransaction3.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			NettingHelper.MoveUnmatchedTransactionsToTheNextPeriod(period, Factory, Db.Connection);

			var newFactory = new BusinessObjectFactory();
			arTransaction1 = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);
			arTransaction2 = newFactory.Load<NettingReceivableTransaction>(arTransaction2.PK);
			arTransaction3 = newFactory.Load<NettingReceivableTransaction>(arTransaction3.PK);
			apTransaction1 = newFactory.Load<NettingPayableTransaction>(apTransaction1.PK);
			apTransaction2 = newFactory.Load<NettingPayableTransaction>(apTransaction2.PK);

			AssertEquals("Receivable transaction 1 moved to next period", nextPeriod.PK, arTransaction1.NettingPeriodPK);
			AssertEquals("Receivable transaction 2 moved to next period", nextPeriod.PK, arTransaction2.NettingPeriodPK);
			AssertEquals("Receivable transaction 3 stays in current period", period.PK, arTransaction3.NettingPeriodPK);
			AssertEquals("Payable transaction 1 moved to next period", nextPeriod.PK, apTransaction1.NettingPeriodPK);
			AssertEquals("Payable transaction 2 stays in current period", period.PK, apTransaction2.NettingPeriodPK);

			arTransaction1Line1 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction1Line1.PK);
			arTransaction2Line1 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction2Line1.PK);
			apTransaction1Line1 = newFactory.Load<NettingPayableTransactionLine>(apTransaction1Line1.PK);
			apTransaction1Line2 = newFactory.Load<NettingPayableTransactionLine>(apTransaction1Line2.PK);

			//lines should not be moved as transactins are not moved
			AssertEquals("Receivable transaction 1 line 1 moved to next period", nextPeriod.PK, arTransaction1Line1.NettingPeriodPK);
			AssertEquals("Receivable transaction 2 line 1 moved to next period", nextPeriod.PK, arTransaction2Line1.NettingPeriodPK);
			AssertEquals("Payable transaction 1 line 1 moved to next period", nextPeriod.PK, apTransaction1Line1.NettingPeriodPK);
			AssertEquals("Payable transaction 1 line 2 moved to next period", nextPeriod.PK, apTransaction1Line2.NettingPeriodPK);

			pivot1 = newFactory.Load<NettingMatchPivot>(pivot1.PK);

			//matching pivots should not be moved as transactins are not moved
			AssertEquals("Pivot 1 stays in current period", period.PK, pivot1.NMP_NSP_Period);
		}

		public void TestMoveUnmatchedTransactions_ManyToOne_MatchedAtLineLevel()
		{
			var arTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref1", "USD", 200M, NettingTransactionApprovalStatus.Approved);
			var arTransaction1Line1 = testObjectCreator.CreateNettingTransactionLine(arTransaction1, "S1", 200M, "USD");

			var arTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref1", "USD", 300M, NettingTransactionApprovalStatus.Approved);
			var arTransaction2Line1 = testObjectCreator.CreateNettingTransactionLine(arTransaction2, "S1", 300M, "USD");

			var arTransaction3 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref1", "USD", 100M, NettingTransactionApprovalStatus.Approved);
			var arTransaction3Line1 = testObjectCreator.CreateNettingTransactionLine(arTransaction3, "S2", 100M, "USD");

			var apTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "ref1", "USD", 500M, NettingTransactionApprovalStatus.Approved);
			var apTransaction1Line1 = testObjectCreator.CreateNettingTransactionLine(apTransaction1, "S1", 200M, "USD");
			var apTransaction1Line2 = testObjectCreator.CreateNettingTransactionLine(apTransaction1, "S1", 300M, "USD");

			var pivot1 = CreateMatchPivot(period, null, null, arTransaction1Line1, apTransaction1Line1);
			var pivot2 = CreateMatchPivot(period, null, null, arTransaction2Line1, apTransaction1Line2);

			arTransaction1.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			arTransaction2.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apTransaction1.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			NettingHelper.MoveUnmatchedTransactionsToTheNextPeriod(period, Factory, Db.Connection);

			var newFactory = new BusinessObjectFactory();
			arTransaction1 = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);
			arTransaction2 = newFactory.Load<NettingReceivableTransaction>(arTransaction2.PK);
			arTransaction3 = newFactory.Load<NettingReceivableTransaction>(arTransaction3.PK);
			apTransaction1 = newFactory.Load<NettingPayableTransaction>(apTransaction1.PK);

			AssertEquals("Receivable transaction 1 stays in current period", period.PK, arTransaction1.NettingPeriodPK);
			AssertEquals("Receivable transaction 2 stays in current period", period.PK, arTransaction2.NettingPeriodPK);
			AssertEquals("Receivable transaction 3 moved to next period", nextPeriod.PK, arTransaction3.NettingPeriodPK);
			AssertEquals("Payable transaction 1 stays in current period", period.PK, apTransaction1.NettingPeriodPK);

			arTransaction1Line1 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction1Line1.PK);
			arTransaction2Line1 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction2Line1.PK);
			arTransaction3Line1 = newFactory.Load<NettingReceivableTransactionLine>(arTransaction3Line1.PK);
			apTransaction1Line1 = newFactory.Load<NettingPayableTransactionLine>(apTransaction1Line1.PK);
			apTransaction1Line2 = newFactory.Load<NettingPayableTransactionLine>(apTransaction1Line2.PK);

			AssertEquals("Receivable transaction 1 line 1 stays in current period", period.PK, arTransaction1Line1.NettingPeriodPK);
			AssertEquals("Receivable transaction 2 line 1 stays in current period", period.PK, arTransaction2Line1.NettingPeriodPK);
			AssertEquals("Receivable transaction 3 line 1 moved to next period", nextPeriod.PK, arTransaction3Line1.NettingPeriodPK);
			AssertEquals("Payable transaction 1 line 1 stays in current period", period.PK, apTransaction1Line1.NettingPeriodPK);
			AssertEquals("Payable transaction 1 line 2 stays in current period", period.PK, apTransaction1Line2.NettingPeriodPK);

			pivot1 = newFactory.Load<NettingMatchPivot>(pivot1.PK);
			pivot2 = newFactory.Load<NettingMatchPivot>(pivot2.PK);

			//matching pivots should not be moved as transactins are not moved
			AssertEquals("Pivot 1 stays in current period", period.PK, pivot1.NMP_NSP_Period);
			AssertEquals("Pivot 2 stays in current period", period.PK, pivot2.NMP_NSP_Period);
		}

		public void TestMoveUnmatchedTransactions_ManyToOne_MatchedAtHeaderLevel()
		{
			var arTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref1", "USD", 200M, NettingTransactionApprovalStatus.Matched);

			var arTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, "ref1", "USD", 300M, NettingTransactionApprovalStatus.Matched);

			var apTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "ref1", "USD", 500M, NettingTransactionApprovalStatus.Matched);

			var apTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "ref1", "USD", 500M, NettingTransactionApprovalStatus.Approved);

			var pivot1 = CreateMatchPivot(period, arTransaction1, apTransaction1, null, null);
			var pivot2 = CreateMatchPivot(period, arTransaction2, apTransaction1, null, null);

			Factory.Save();

			NettingHelper.MoveUnmatchedTransactionsToTheNextPeriod(period, Factory, Db.Connection);

			var newFactory = new BusinessObjectFactory();
			arTransaction1 = newFactory.Load<NettingReceivableTransaction>(arTransaction1.PK);
			arTransaction2 = newFactory.Load<NettingReceivableTransaction>(arTransaction2.PK);
			apTransaction1 = newFactory.Load<NettingPayableTransaction>(apTransaction1.PK);
			apTransaction2 = newFactory.Load<NettingPayableTransaction>(apTransaction2.PK);

			AssertEquals("Receivable transaction 1 stays in current period", period.PK, arTransaction1.NettingPeriodPK);
			AssertEquals("Receivable transaction 2 stays in current period", period.PK, arTransaction2.NettingPeriodPK);
			AssertEquals("Payable transaction 1 stays in current period", period.PK, apTransaction1.NettingPeriodPK);
			AssertEquals("Payable transaction 2 moved to next period", nextPeriod.PK, apTransaction2.NettingPeriodPK);

			pivot1 = newFactory.Load<NettingMatchPivot>(pivot1.PK);
			pivot2 = newFactory.Load<NettingMatchPivot>(pivot2.PK);

			//matching pivots should not be moved as transactins are not moved
			AssertEquals("Pivot 1 stays in current period", period.PK, pivot1.NMP_NSP_Period);
			AssertEquals("Pivot 2 stays in current period", period.PK, pivot2.NMP_NSP_Period);
		}

		public void TestGetOrgHeaderByEhubID()
		{
			var eHubID = "123456";
			var orgX = testObjectCreator.CreateOrgHeader("XXX", true, true, "AUSYD");
			orgX.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);

			var orgY = testObjectCreator.CreateOrgHeader("YYY", true, true, "AUSYD");
			orgY.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);

			var participantX = testObjectCreator.CreateNettingOrganisation(nettingSystem, orgX, "FUL");

			Factory.Save();

			var participantOrgHeader = NettingHelper.GetOrgHeaderByEhubID(eHubID, Factory);

			AssertNotNull(participantOrgHeader);
			AssertEquals(orgX.PK, participantOrgHeader.PK);

			participantOrgHeader = NettingHelper.GetOrgHeaderByEhubID("MadeUpEhubID", Factory);

			AssertNull(participantOrgHeader);
		}

		public void TestGetNettingReceivableTransaction()
		{
			var participant1EHubID = "123456";
			org1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, participant1EHubID);

			var participant2EHubID = "456789";
			org2.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, participant2EHubID);

			var invoiceNumber = "ref1";

			var arTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, invoiceNumber, "USD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.Invoice);
			var arTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant2, participant1, invoiceNumber, "USD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.Invoice);
			var arTransaction3 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant1, participant2, invoiceNumber, "USD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.CreditNote);
			var arTransaction4 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsReceivable, period, participant2, participant1, invoiceNumber, "AUD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.AdjustmentNote);

			Factory.Save();

			ReleaseFactory();

			var transaction1 = NettingHelper.GetNettingReceivableTransaction(TransactionTypes.Invoice, invoiceNumber, participant1EHubID, Factory);
			AssertNotNull(transaction1);
			AssertEquals(arTransaction1.PK, transaction1.PK);

			var transaction2 = NettingHelper.GetNettingReceivableTransaction(TransactionTypes.Invoice, invoiceNumber, participant2EHubID, Factory);
			AssertNotNull(transaction2);
			AssertEquals(arTransaction2.PK, transaction2.PK);

			var transaction3 = NettingHelper.GetNettingReceivableTransaction(TransactionTypes.CreditNote, invoiceNumber, participant1EHubID, Factory);
			AssertNotNull(transaction3);
			AssertEquals(arTransaction3.PK, transaction3.PK);

			var transaction4 = NettingHelper.GetNettingReceivableTransaction(TransactionTypes.AdjustmentNote, invoiceNumber, participant2EHubID, Factory);
			AssertNotNull(transaction4);
			AssertEquals(arTransaction4.PK, transaction4.PK);
		}

		public void TestGetNettingPayableTransaction()
		{
			var participant1EHubID = "123456";
			org1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, participant1EHubID);

			var participant2EHubID = "456789";
			org2.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, participant2EHubID);

			var internalReferenceNumber = "INTRef1";

			var apTransaction1 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "ref1", "USD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.Invoice);
			testObjectCreator.AddNettingTransactionReference(apTransaction1, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, internalReferenceNumber);

			var apTransaction2 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant2, participant1, "ref1", "USD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.Invoice);
			testObjectCreator.AddNettingTransactionReference(apTransaction2, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, internalReferenceNumber);

			var apTransaction3 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "ref1", "USD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.CreditNote);
			testObjectCreator.AddNettingTransactionReference(apTransaction3, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, internalReferenceNumber);

			var apTransaction4 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant2, participant1, "ref1", "AUD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.CreditNote);
			testObjectCreator.AddNettingTransactionReference(apTransaction4, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, internalReferenceNumber);

			var apTransaction5 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant1, participant2, "ref1", "USD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.AdjustmentNote);
			testObjectCreator.AddNettingTransactionReference(apTransaction5, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, internalReferenceNumber);

			var apTransaction6 = testObjectCreator.CreateNettingTransaction(LedgerTypes.AccountsPayable, period, participant2, participant1, "ref1", "AUD", 200M, NettingTransactionApprovalStatus.Matched, TransactionTypes.AdjustmentNote);
			testObjectCreator.AddNettingTransactionReference(apTransaction6, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, internalReferenceNumber);

			Factory.Save();

			ReleaseFactory();

			var transaction1 = NettingHelper.GetNettingPayableTransaction(TransactionTypes.Invoice, internalReferenceNumber, participant1EHubID, Factory);
			AssertNotNull(transaction1);
			AssertEquals(apTransaction2.PK, transaction1.PK);

			var transaction2 = NettingHelper.GetNettingPayableTransaction(TransactionTypes.Invoice, internalReferenceNumber, participant2EHubID, Factory);
			AssertNotNull(transaction2);
			AssertEquals(apTransaction1.PK, transaction2.PK);

			var transaction3 = NettingHelper.GetNettingPayableTransaction(TransactionTypes.CreditNote, internalReferenceNumber, participant1EHubID, Factory);
			AssertNotNull(transaction3);
			AssertEquals(apTransaction4.PK, transaction3.PK);

			var transaction4 = NettingHelper.GetNettingPayableTransaction(TransactionTypes.CreditNote, internalReferenceNumber, participant2EHubID, Factory);
			AssertNotNull(transaction4);
			AssertEquals(apTransaction3.PK, transaction4.PK);

			var transaction5 = NettingHelper.GetNettingPayableTransaction(TransactionTypes.AdjustmentNote, internalReferenceNumber, participant1EHubID, Factory);
			AssertNotNull(transaction5);
			AssertEquals(apTransaction6.PK, transaction5.PK);

			var transaction6 = NettingHelper.GetNettingPayableTransaction(TransactionTypes.AdjustmentNote, internalReferenceNumber, participant2EHubID, Factory);
			AssertNotNull(transaction6);
			AssertEquals(apTransaction5.PK, transaction6.PK);
		}

		public void TestGetNettingSystem()
		{
			var getEHubID = new Func<OrgHeader, ZString>(org =>
			{
				var eHubCusCode = org?.CustomsCodes.GetOrgCusCodesForCodeIgnoringCountry(OrgCusCode.CodeTypes.EHubOrganisationID).FirstOrDefault();
				AssertNotNull(eHubCusCode);
				return eHubCusCode.OK_CustomsRegNo;
			});

			AssertEquals(nettingSystem.PK, NettingHelper.GetNettingSystem(Factory, getEHubID(localIssuer)).PK);
			AssertEquals(nettingSystem.PK, NettingHelper.GetNettingSystem(Factory, getEHubID(localRecipient)).PK);
			AssertEquals(nettingSystem.PK, NettingHelper.GetNettingSystem(Factory, getEHubID(GlbBranch.CurrentBranch.OrgProxy)).PK);
		}

		public void TestGetNettingPeriod()
		{
			AssertEquals(period.PK.ToGuid(), NettingHelper.GetNettingPeriod(nettingSystem, ZDateTime.Today.AddDays(-10), true));
			AssertEquals(period.PK.ToGuid(), NettingHelper.GetNettingPeriod(nettingSystem, ZDateTime.Today.AddDays(-10), false));

			AssertEquals(period.PK.ToGuid(), NettingHelper.GetNettingPeriod(nettingSystem, ZDateTime.Today, true));
			AssertEquals(period.PK.ToGuid(), NettingHelper.GetNettingPeriod(nettingSystem, ZDateTime.Today, false));

			AssertEquals(nextPeriod.PK.ToGuid(), NettingHelper.GetNettingPeriod(nettingSystem, ZDateTime.Today.AddDays(100), true));
			AssertEquals(nextPeriod.PK.ToGuid(), NettingHelper.GetNettingPeriod(nettingSystem, ZDateTime.Today.AddDays(100), false));
		}

		public void TestGetARTransaction()
		{
			var transaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			Factory.Save();
			AssertEquals(transaction.PK, NettingHelper.GetNettingReceivableTransaction(issuer.PK, recipient.PK, "INV101", "USD", new BusinessObjectFactory()).PK);
		}

		public void TestGetAPTransaction()
		{
			var transaction = (NettingPayableTransaction)testObjectCreator.CreateNettingTransaction("AP", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			Factory.Save();
			AssertEquals(transaction.PK, NettingHelper.GetNettingPayableTransaction(issuer.PK, recipient.PK, "INV101", "USD", new BusinessObjectFactory()).PK);
		}

		public void TestUnmatchTransaction()
		{
			var nettingARTransaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");
			var nettingAPTransaction = (NettingPayableTransaction)testObjectCreator.CreateNettingTransaction("AP", period, issuer, recipient, "INV101", "USD", 500M, "APP");

			var pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = nettingARTransaction.PK;
			pivot.NMP_NPT_PayableTransaction = nettingAPTransaction.PK;
			nettingARTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			nettingAPTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			Factory.Save();

			NettingHelper.UnmatchTransaction(Db.Connection, period.PK.ToGuid(), nettingARTransaction, NettingTransactionApprovalStatus.Approved);
			Factory.Save();

			nettingARTransaction.Reload();
			nettingAPTransaction.Reload();
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingARTransaction.ApprovalStatus);
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingAPTransaction.ApprovalStatus);
		}

		public void TestHandleSettledOutOfNettingEvent()
		{
			var nettingARTransaction = (NettingReceivableTransaction)testObjectCreator.CreateNettingTransaction("AR", period, issuer, recipient, "INV101", "USD", 500M, "APP");

			NettingHelper.HandleSettledOutOfNettingEvent(Db.Connection, nettingARTransaction, InvoiceAdditionalReference.FullyMatched);
			AssertEquals(NettingTransactionApprovalStatus.SettledOutOfNetting, nettingARTransaction.ApprovalStatus);

			NettingHelper.HandleSettledOutOfNettingEvent(Db.Connection, nettingARTransaction, InvoiceAdditionalReference.UndoFullyMatched);
			AssertEquals(NettingTransactionApprovalStatus.Approved, nettingARTransaction.ApprovalStatus);

			NettingHelper.HandleSettledOutOfNettingEvent(Db.Connection, nettingARTransaction, InvoiceAdditionalReference.PostedAndFullyMatched);
			AssertEquals(NettingTransactionApprovalStatus.SettledOutOfNetting, nettingARTransaction.ApprovalStatus);
		}

		NettingMatchPivot CreateMatchPivot(NettingSystemPeriod period, INettingTransaction arHeader, INettingTransaction apHeader, INettingTransactionLine arLine, INettingTransactionLine apLine)
		{
			var pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;

			pivot.NMP_NRT_ReceivableTransaction = arHeader != null ? arHeader.PK : ZGuid.Empty;
			pivot.NMP_NPT_PayableTransaction = apHeader != null ? apHeader.PK : ZGuid.Empty;
			pivot.NMP_NRL_ReceivableLine = arLine != null ? arLine.PK : ZGuid.Empty;
			pivot.NMP_NPL_PayableLine = apLine != null ? apLine.PK : ZGuid.Empty;

			return pivot;
		}

		NettingOrganisation CreateNettingOrgHeader(NettingSystem ns, OrgHeader org, string eHubID)
		{
			org.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);
			return testObjectCreator.CreateNettingOrganisation(ns, org, "CUR");
		}

		NettingSystemPeriod CreatePeriod(NettingSystem nettingSystem)
		{
			period = Factory.New<NettingSystemPeriod>();
			period.NSP_Period = "201504";
			period.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-7);
			period.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			period.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(17);

			period.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(5);
			period.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			period.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(10);
			period.NSP_ValueDate = ZDate.Today.AddDays(12);
			period.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(12);

			period.NSP_NS_NettingSystem = nettingSystem.PK;

			CreateNextPeriod(nettingSystem);

			return period;
		}

		void CreateNextPeriod(NettingSystem nettingSystem)
		{
			nextPeriod = Factory.New<NettingSystemPeriod>();
			nextPeriod.NSP_Period = "201505";
			nextPeriod.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(23);
			nextPeriod.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(45);
			nextPeriod.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(37);

			nextPeriod.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(35);
			nextPeriod.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(38);
			nextPeriod.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(40);
			nextPeriod.NSP_ValueDate = ZDate.Today.AddDays(42);
			nextPeriod.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(42);

			nextPeriod.NSP_NS_NettingSystem = nettingSystem.PK;
		}

		NettingSystem CreateNettingSystem(string code, string description, GlbCompany company)
		{
			var nettingSystem = Factory.New<NettingSystem>();
			nettingSystem.NS_Code = code;
			nettingSystem.NS_Description = description;
			nettingSystem.NS_GC = company.PK;
			nettingSystem.NS_IsActive = true;
			return nettingSystem;
		}

		protected override void SetUp()
		{
			base.SetUp();

			testObjectCreator = new NettingObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());

			nettingSystem = CreateNettingSystem("EDINETTING", "Netting System for Testing", GlbCompany.CurrentCompany);
			period = CreatePeriod(nettingSystem);

			org1 = testObjectCreator.CreateOrgHeader("AAAAA", true, true, "AUSYD");
			org2 = testObjectCreator.CreateOrgHeader("BBBBB", true, true, "AUSYD");

			participant1 = testObjectCreator.CreateNettingOrganisation(nettingSystem, org1, "FUL");
			participant2 = testObjectCreator.CreateNettingOrganisation(nettingSystem, org2, "FUL");

			localIssuer = testObjectCreator.CreateOrgHeader("Issuer1", true, true);
			issuer = CreateNettingOrgHeader(nettingSystem, localIssuer, "EDINETING");
			localRecipient = testObjectCreator.CreateOrgHeader("Recipient1", true, true);
			recipient = CreateNettingOrgHeader(nettingSystem, localRecipient, "EDINETING");

			nettingSystemOrg = CreateNettingOrgHeader(nettingSystem, GlbBranch.CurrentBranch.OrgProxy, "EDINETING");

			Factory.Save();
		}

		NettingSystem nettingSystem;
		NettingSystemPeriod period;
		NettingSystemPeriod nextPeriod;
		OrgHeader org1, org2;
		NettingOrganisation participant1, participant2;
		NettingObjectCreator testObjectCreator;
		NettingOrganisation issuer;
		OrgHeader localIssuer;
		NettingOrganisation recipient;
		OrgHeader localRecipient;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Needed in Setup")]
		NettingOrganisation nettingSystemOrg = null;
	}
}

