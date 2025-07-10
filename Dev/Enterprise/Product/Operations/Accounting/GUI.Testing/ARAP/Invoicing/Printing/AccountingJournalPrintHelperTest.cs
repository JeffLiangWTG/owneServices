using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	public class AccountingJournalPrintHelperTest : TestCaseWithFactory
	{
		public void TestPrintAccountingJournalWithNoTransactionEmptyHeaderAndEmptyLines()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AccountingJournalPrintHelper.PrintAccountingJournal(Enumerable.Empty<TransactionHeader>(), Enumerable.Empty<BaseWIPAccrual>());
			AssertEquals("There is no accounting journal within the given selection criteria.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPrintAccountingJournalWithTransactionEmptyHeaderAndLinesAndUserSaysYes()
		{
			AssertPrintAccountingJournalWithTransactionEmptyHeaderAndLines(true);
		}

		public void TestPrintAccountingJournalWithTransactionEmptyHeaderAndLinesAndUserSaysNo()
		{
			AssertPrintAccountingJournalWithTransactionEmptyHeaderAndLines(false);
		}

		public void TestPrintAccountingJournalWithTransactioHeaderAndnEmptyLinesAndUserSaysYes()
		{
			AssertPrintAccountingJournalWithTransactionHeaderAndEmptyLines(true);
		}

		public void TestPrintAccountingJournalWithTransactionHeaderAndnEmptyLinesAndUserSaysNo()
		{
			AssertPrintAccountingJournalWithTransactionHeaderAndEmptyLines(false);
		}

		public void TestPrintAccountingJournalWithTransactioHeaderAndnLinesAndUserSaysYes()
		{
			AssertPrintAccountingJournalWithTransactionHeaderAndLines(true);
		}

		public void TestPrintAccountingJournalWithTransactionHeaderAndnLinesAndUserSaysNo()
		{
			AssertPrintAccountingJournalWithTransactionHeaderAndLines(false);
		}

		public void TestPrintAccountingJournalWithNoTransactions()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AccountingJournalPrintHelper.PrintAccountingJournal(Array.Empty<TransactionHeader>());
			AssertEquals("There is no accounting journal within the given selection criteria.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestPrintAccountingJournalWithAccountingJournalsAndUserSaysYes()
		{
			AssertPrintAccountingJournalWithAccountingJournalsWithUserConfirmation(true);
		}

		public void TestPrintAccountingJournalWithAccountingJournalsAndUserSaysNo()
		{
			AssertPrintAccountingJournalWithAccountingJournalsWithUserConfirmation(false);
		}

		void AssertPrintAccountingJournalWithAccountingJournalsWithUserConfirmation(bool didUserSayYes)
		{
			var (invoicings, aRSuspenseControlAccount) = CreateInvoicingsAndControlAccount();

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var dialogResult = didUserSayYes ? System.Windows.Forms.DialogResult.Yes : System.Windows.Forms.DialogResult.No;
			UnitTestUserNotification.Instance.AddAnswer(dialogResult);
			AccountingJournalPrintHelper.PrintAccountingJournal(new TransactionHeader[] { invoicings[0], invoicings[1], invoicings[2] });
			if (didUserSayYes)
			{
				AssertEquals("There are 3 accounting journals to print. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(DocDeliveryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
			else
			{
				AssertEquals("There are 3 accounting journals to print. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		(InvoicingBase[] invoicings, AccGLHeader aRSuspenseControlAccount) CreateInvoicingsAndControlAccount()
		{
			var invoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			var invoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV002", TestObjectCreator.AUD, 1m, 200m, 0m, 200m, 0m);
			var invoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "ARINV003", TestObjectCreator.AUD, 1m, 300m, 0m, 300m, 0m);

			invoice1.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
			invoice2.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;
			invoice3.Lines[0].AL_AG = TestObjectCreator.GLHeader1.PK;

			var aRSuspenseControlAccount = TestObjectCreator.CreateARSuspenseControlAccount();

			return (new InvoicingBase[] { invoice1, invoice2, invoice3 }, aRSuspenseControlAccount);
		}

		(WIP wip, Accrual accrual) CreateWipAndAccral()
		{
			var job = TestObjectCreator.CreateJob("S00001222", TestObjectCreator.ABIGAS, 0m, null, 0m);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var charge1 = job.Charges.AddNew();
			charge1.JR_AC = TestObjectCreator.CC1.PK;

			var wip = TestObjectCreator.CreateWIP(charge1);
			var accrual = TestObjectCreator.CreateAccrual(charge1);

			return (wip, accrual);
		}

		void AssertPrintAccountingJournalWithTransactionEmptyHeaderAndLines(bool didUserSayYes)
		{
			var (wip, accrual) = CreateWipAndAccral();

			Factory.Save();

			AssertPrintAccountingJournal(2, didUserSayYes, Enumerable.Empty<TransactionHeader>(), new BaseWIPAccrual[] { wip, accrual });
		}

		void AssertPrintAccountingJournalWithTransactionHeaderAndLines(bool didUserSayYes)
		{
			var (wip, accrual) = CreateWipAndAccral();
			var (invoicings, aRSuspenseControlAccount) = CreateInvoicingsAndControlAccount();

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());

			AssertPrintAccountingJournal(5, didUserSayYes, new TransactionHeader[] { invoicings[0], invoicings[1], invoicings[2] }, new BaseWIPAccrual[] { wip, accrual });
		}

		void AssertPrintAccountingJournalWithTransactionHeaderAndEmptyLines(bool didUserSayYes)
		{
			var (invoicings, aRSuspenseControlAccount) = CreateInvoicingsAndControlAccount();

			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());

			AssertPrintAccountingJournal(3, didUserSayYes, new TransactionHeader[] { invoicings[0], invoicings[1], invoicings[2] }, Enumerable.Empty<BaseWIPAccrual>());
		}

		void AssertPrintAccountingJournal(int printCount, bool didUserSayYes, IEnumerable<TransactionHeader> transactionsHeaderToPrint, IEnumerable<BaseWIPAccrual> transactionsLinesToPrint)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var dialogResult = didUserSayYes ? System.Windows.Forms.DialogResult.Yes : System.Windows.Forms.DialogResult.No;
			UnitTestUserNotification.Instance.AddAnswer(dialogResult);
			AccountingJournalPrintHelper.PrintAccountingJournal(transactionsHeaderToPrint, transactionsLinesToPrint);
			if (didUserSayYes)
			{
				AssertEquals("There are " + printCount + " accounting journals to print. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(typeof(DocDeliveryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
			else
			{
				AssertEquals("There are " + printCount + " accounting journals to print. Do you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
