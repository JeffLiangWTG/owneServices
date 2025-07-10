using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(WIPACRAccountingJournal))]
	public class WIPACRAccountingJournalTest : AccountingJournalTest
	{
		public override void TestAJOptionalFields()
		{
			Creator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
			Factory.Save();

			var job = Creator.CreateJob("S00001001", Creator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = Creator.CC1.PK;
			var acr = Creator.CreateAccrual(charge1);
			Factory.Save();

			acr.Logs.CreateRecreateOrUpdateEventLog(Events.TransactionReversed, EstimateActual.Actual, ZDateTimeOffset.Today);
			Factory.Save();

			var aj = new WIPACRAccountingJournal(acr, ReadonlyFactory);
			AssertEquals("Optional Field Count", 2, aj.ApplicableOptionalFields.Count);
			AssertEquals("DATE REVERSED", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.DateReversedText));
			AssertEquals("DATE REVERSED Value", ZDateTime.Today.ToShortDateString(), aj.ApplicableOptionalFields[AccountingJournal.DateReversedText]);
			AssertEquals("REVERSED BY", true, aj.ApplicableOptionalFields.ContainsKey(AccountingJournal.ReversedByText));
			AssertEquals("REVERSED BY Value", Env.CurrentUser.FullName, aj.ApplicableOptionalFields[AccountingJournal.ReversedByText]);
		}

		protected override void AssertTransaction()
		{
			AssertNotNull("Underlying Transaction", transactionLine);
		}

		protected override void AssertLedger()
		{
			AssertEquals("Ledger", LedgerTypes.JobCosting, accountingJournal.Ledger);
		}

		protected override void AssertTransactionType()
		{
			AssertEquals("TransactionType", transactionLine.AL_LineType, accountingJournal.TransactionType);
		}

		protected override void AssertTransactionNumber()
		{
			AssertEquals("TransactionNumber", transactionLine.InvoicingJob.JH_JobLocalReference, accountingJournal.TransactionNumber);
		}

		protected override void AssertTransactionDescription()
		{
			AssertEquals("TransactionDescription", transactionLine.InvoicingJob.JH_JobNum, accountingJournal.TransactionDescription);
		}

		protected override void AssertCurrency()
		{
			AssertEquals("Currency", transactionLine.TransactionCurrency.RX_Code, accountingJournal.Currency);
		}

		protected override void AssertJob()
		{
			AssertEquals("Job", transactionLine.Job, accountingJournal.Job);
		}

		protected override void AssertCreatedDate()
		{
			AssertEquals("CreatedDate", transactionLine.AL_SystemCreateTimeUtc.ToLocalBranchTime(), accountingJournal.CreatedDate);
		}

		protected override void AssertJournalName()
		{
			AssertEquals("JournalName", string.Format("{0} {1} {2}", LedgerTypes.JobCosting, transactionLine.AL_LineType, transactionLine.InvoicingJob.JH_JobLocalReference), accountingJournal.JournalName);
		}

		protected override void AssertCreatedBy()
		{
			AssertEquals("CreatedBy", transactionLine.AL_SystemCreateUser, accountingJournal.CreatedBy);
		}

		public override void TestTransactionReference()
		{
			accountingJournal = CreateTestAccountingJournal();
			AssertNull("Precondition: Transaction", transaction);
			AssertEquals("TransactionReference", ZString.Empty, accountingJournal.TransactionReference);
		}

		public override void TestConsolidatedInvoiceRef()
		{
			accountingJournal = CreateTestAccountingJournal();
			AssertNull("Precondition: Transaction", transaction);
			AssertEquals("ConsolidatedInvoiceRef", ZString.Empty, accountingJournal.ConsolidatedInvoiceRef);
		}

		public override void TestComplianceSubType()
		{
			accountingJournal = CreateTestAccountingJournal();
			AssertNull("Precondition: Transaction", transaction);
			AssertEquals("ComplianceSubType", ZString.Empty, accountingJournal.ComplianceSubType);
		}

		protected override void AssertRelatedGLJournals()
		{
			Assert("Not Applicable", true);
		}

		protected override void AssertPostToPeriod()
		{
			Assert("Not Applicable", true);
		}

		public void TestALDescForReportingBook()
		{
			var factory = new ReadOnlyBusinessObjectFactory();
			var reportingBook = factory.New<AccReportingBook>();
			var creator = new TestObjectCreator(factory);
			var dataTable = new System.Data.DataTable("GeneralLedgerTransactionData");
			dataTable.Columns.Add("TransactionHeaderID", typeof(Guid));
			dataTable.Columns.Add("TransactionLineID", typeof(Guid));
			dataTable.Columns.Add("GLAmountLocalBalance", typeof(decimal));
			AssertLineDesc();

			void AssertLineDesc()
			{
				var wip = creator.CreateWIP();
				var row = dataTable.Rows.Add();
				row["TransactionLineID"] = wip.PK.ToGuid();
				row["GLAmountLocalBalance"] = 10m;
				Factory.Save();

				var accountingJournal = new WIPACRAccountingJournal(wip, factory, reportingBook, dataTable);
				Assert(accountingJournal.Lines.All(x => x.AL_Desc == ZString.Empty));
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var job = Creator.CreateJob("S00001222", Creator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = Creator.CC1.PK;
			var wip = Creator.CreateWIP(charge1);

			return new WIPACRAccountingJournal(wip, ReadonlyFactory);
		}

		protected override AccountingJournal CreateTestAccountingJournal()
		{
			var job = Creator.CreateJob("S00001001", Creator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = Creator.CC1.PK;
			transactionLine = Creator.CreateAccrual(charge1);

			return new WIPACRAccountingJournal(transactionLine, ReadonlyFactory);
		}

		TransactionLine transactionLine;

		public override void TestAccountingJournalLines()
		{
			Assert(true);
		}
	}
}
