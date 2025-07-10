using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.BatchProcessor.Accounting;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.Netting;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingImportInitiator))]
	class NettingImportInitiatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestQueueTransactionsForImport_CorrectValuesPopulated()
		{
			InvoicingBase invoice;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Participant1Branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M
					, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK, ZDateTime.Today, false); //invoiced to a company org proxy, expect this transaction to be queued

				Factory.Save();
			}

			var expectedLogReferenceEnd = AccountingConstants.InvoiceAdditionalReference.Separator + AccountingConstants.InvoiceAdditionalReference.Posted;
			var logs = invoice.Logs.Find(x => x.SL_Reference.EndsWith(expectedLogReferenceEnd));
			AssertEquals("Precondition: logs.Count", 1, logs.Count());

			var expectedEventCode = "SSS";
			var expectedUserCode = "BBB";
			var expectedReference = "DDDDDDD" + expectedLogReferenceEnd;

			var logForTest = logs.First();
			using (logForTest.GetValidationSuspender())
			{
				using (logForTest.LockForUpdatingKeyFieldsForTesting())
				{
					logForTest.SL_SE_NKEvent = expectedEventCode;
					logForTest.SL_GS_NKUser = expectedUserCode;
					logForTest.SL_Reference = expectedReference;
				}
				Factory.Save();

				var nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, false);
				var result = nettingImportIniator.QueueTransactionsForImport();
				AssertEquals("Postcondition: result of QueueTransactionsForImport", 1, result);

				var transactionNettingTransmitterRecords = Factory.Load<IQueuedLog>(new ZQuery(StmJobQueueSchema.SJ_FilterName, TransactionNettingSubscriber.TransactionNettingTransmitter));
				AssertEquals("stmJobQueueRecords.Length", 1, transactionNettingTransmitterRecords.Length);
				var stmJobQueue = transactionNettingTransmitterRecords[0];
				CombineAssertions(() =>
				{
					AssertEquals("SJ_ALogReference", logForTest.PK, stmJobQueue.SJ_ALogReference);
					AssertEquals("SJ_SE_NKEvent", expectedEventCode, stmJobQueue.SJ_SE_NKEvent);
					AssertEquals("SJ_PostedTimeUtc", logForTest.SL_PostedTimeUtc, stmJobQueue.SJ_PostedTimeUtc);
					AssertEquals("SJ_EventTime", logForTest.SL_EventTime, stmJobQueue.SJ_EventTime);
					AssertEquals("SJ_EventTimeUtc", logForTest.SL_EventTimeUtc, stmJobQueue.SJ_EventTimeUtc);
					AssertEquals("SJ_GS_NKUser", expectedUserCode, stmJobQueue.SJ_GS_NKUser);
					AssertEquals("SJ_Reference", expectedReference, stmJobQueue.SJ_Reference);
					AssertEquals("SJ_ParentTableCode", AccTransactionHeaderSchema.Constants.Prefix, stmJobQueue.SJ_ParentTableCode);
					AssertEquals("SJ_ParentID", invoice.PK, stmJobQueue.SJ_ParentID);
					AssertEquals("SJ_Status", JobQueueStatus.StatusQueued, stmJobQueue.SJ_Status);
				});
			}
		}

		[TestDate(2017, 11, 05)]
		public void TestQueueTransactionsForImport_FromASingleParticipant()
		{
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Participant1Branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M
					, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK, ZDateTime.Today, false); //invoiced to a company org proxy, expect this transaction to be queued

				TestObjectCreator.CreateInvoiceWithLine(typeof(ARCreditNote), "002", TestObjectCreator.AUD, 1M, 200M, 0M, 100M, 0M
					, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK, ZDateTime.Today, false); //invoiced to a branch org proxy, expect this transaction to be queued

				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "003", TestObjectCreator.AUD, 1M, 300M, 0M, 100M, 0M
					, TestObjectCreator.Creditor2, TestObjectCreator.GLHeader1.PK, ZDateTime.Today, false); //invoiced to an org neither company or branch org proxy, do not expect this transaction to be queued

				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "004", TestObjectCreator.AUD, 1M, 400M, 0M, 400M, 0M
					, TestObjectCreator.Creditor1, TestObjectCreator.GLHeader1.PK, ZDateTime.Today.AddMonths(1), false); //invoiced to a company org proxy, due date is in next month
			}

			Factory.Save();

			var nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsPayable, NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, false);
			var result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("0 transactions should be queued, as No AP transaction was issued with due dates within those dates", 0, result);

			nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsPayable, NettingImportInitiator.Unpaid, firstDayOfMonth.AddMonths(1), lastDayOfMonth.AddMonths(1), false);
			result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("1 transaction should be queued, only 1 AP transaction was issued with due date within those dates", 1, result);

			nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.Unpaid, firstDayOfMonth.AddMonths(1), lastDayOfMonth.AddMonths(1), false);
			result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("0 transactions should be queued, as No AR transaction was issued with due dates within those dates", 0, result);

			nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, false);
			result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("2 transactions should be queued", 2, result);
		}

		[TestDate(2017, 11, 05)]
		public void TestQueueTransactionsForImport_FromDifferentParticipants()
		{
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Participant1Branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M
					, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK, ZDateTime.Today, false); //invoiced to a company org proxy, expect this transaction to be queued
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Participant2Branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M
					, TestObjectCreator.ABIGAS, TestObjectCreator.GLHeader1.PK, ZDateTime.Today, false); //invoiced to a company org proxy, expect this transaction to be queued
			}

			Factory.Save();

			var expectedTrailLog = @"No transactions imported for this Netting System.";
			var nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, false);
			AssertEquals(expectedTrailLog, nettingImportIniator.TrailLog);
			var result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("1 transaction should be queued", 1, result);

			expectedTrailLog = @"User: 'E', Ledger: 'AR', Payment Status: 'UPD', Dates From: '01-Nov-17' To: '30-Nov-17', NC part of Netting: 'N', Total Queued Transaction(s): 1";
			nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsPayable, NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, false);
			AssertContains(expectedTrailLog, nettingImportIniator.TrailLog);
			result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("1 transaction should be queued", 1, result);

			expectedTrailLog = @"User: 'E', Ledger: 'AP', Payment Status: 'UPD', Dates From: '01-Nov-17' To: '30-Nov-17', NC part of Netting: 'N', Total Queued Transaction(s): 1";
			nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, "BTH", NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, false);
			result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("2 transaction should be queued", 2, result);
		}

		[TestDate(2017, 11, 05)]
		public void TestQueueTransactionsForImport_FromNettingCentreCompany()
		{
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Nettingbranch.PK.ToGuid(), department.PK.ToGuid()))
			{
				TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M
					, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK, ZDateTime.Today, false); //invoiced to a company org proxy, expect this transaction to be queued
			}

			Factory.Save();

			var nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, false);
			var result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("No transactions should be queued since Transactions from the Netting Centre is not included and the only AR transaction is issued from the Netting Centre", 0, result);

			nettingImportIniator = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, true);
			result = nettingImportIniator.QueueTransactionsForImport();
			AssertEquals("1 transaction should be queued", 1, result);
		}

		[TestDate(2017, 11, 05)]
		public void TestQueueTransactionsForImport_PaymentStatus()
		{
			InvoicingBase invoice1 = null;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Participant1Branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", TestObjectCreator.AUD, 1M, 100M, 0M, 100M, 0M
					, TestObjectCreator.AALSHI, TestObjectCreator.GLHeader1.PK, ZDateTime.Today, false); //invoiced to a company org proxy, expect this transaction to be queued
			}

			Factory.Save();

			var nettingImportIniatorUnpaid = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.Unpaid, firstDayOfMonth, lastDayOfMonth, false);
			var nettingImportIniatorPaid = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.Paid, firstDayOfMonth, lastDayOfMonth, false);
			var nettingImportIniatorALL = new NettingImportInitiator(Factory, NettingSystemCode, LedgerTypes.AccountsReceivable, NettingImportInitiator.ALL, firstDayOfMonth, lastDayOfMonth, false);
			AssertEquals("1 transaction should be queued", 1, nettingImportIniatorUnpaid.QueueTransactionsForImport());
			AssertEquals("0 transaction should be queued, as the transaction is not paid yet", 0, nettingImportIniatorPaid.QueueTransactionsForImport());
			AssertEquals("1 transaction should be queued", 1, nettingImportIniatorALL.QueueTransactionsForImport());

			var recepit = TestObjectCreator.CreateAndMatchARReceiptForARInvoice(invoice1);
			recepit.AH_GB = invoice1.AH_GB;
			Factory.Save();

			AssertEquals("0 transaction should be queued, as the transaction is paid already", 0, nettingImportIniatorUnpaid.QueueTransactionsForImport());
			AssertEquals("1 transaction should be queued", 1, nettingImportIniatorPaid.QueueTransactionsForImport());
			AssertEquals("1 transaction should be queued", 1, nettingImportIniatorALL.QueueTransactionsForImport());
		}

		protected override void SetUp()
		{
			base.SetUp();

			//NettingCompanyOrgProxy = TestObjectCreator.CreateOrgHeader("NettingOrg", true, true);
			NettingCompany = TestObjectCreator.CreateNewCompany("NCO", TestObjectCreator.TestOrganisation);
			Nettingbranch = TestObjectCreator.CreateBranch("001", NettingCompany, TestObjectCreator.TestOrganisation);

			Participant1Company = TestObjectCreator.CreateNewCompany("PA1", TestObjectCreator.ABIGAS);
			Participant1Branch = TestObjectCreator.CreateBranch("002", Participant1Company);

			Participant2Company = TestObjectCreator.CreateNewCompany("PA2", TestObjectCreator.AALSHI);
			Participant2Branch = TestObjectCreator.CreateBranch("003", Participant2Company, TestObjectCreator.Creditor1);

			var setupManager = new NettingSetupManager(Factory, new ZGuid[] { NettingCompany.PK, Participant1Company.PK, Participant2Company.PK });
			setupManager.NettingSystemCompanyCode = NettingCompany.GC_Code;
			setupManager.NettingSystemCode = NettingSystemCode;
			setupManager.NettingSystemDescription = "Test netting system";
			setupManager.NettingCycleStartDate = ZDateTime.Today;

			setupManager.SetupNetting();

			firstDayOfMonth = new DateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, 1);
			lastDayOfMonth = new DateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, DateTime.DaysInMonth(ZDateTime.Now.Year, ZDateTime.Now.Month));
		}

		GlbCompany NettingCompany, Participant1Company, Participant2Company;
		GlbBranch Nettingbranch, Participant1Branch, Participant2Branch;
		ZDateTime firstDayOfMonth, lastDayOfMonth;
		readonly GlbStaff staff = GlbStaff.CurrentUser;
		readonly GlbDepartment department = GlbDepartment.CurrentDepartment;

		const string NettingSystemCode = "TST";
		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
