using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AccountingAuditHelperTest : TestCaseWithFactory
	{
		public void TestHandleAuditTransaction()
		{
			var securityCheckpoint = Env.Security.ReceivablesAuditTransaction;
			var currentUser = GlbStaff.CurrentUser.GS_Code;
			const string anotherUser = "JYW";
			AssertNotEquals("Precondition: The two users should not be identical.", currentUser, anotherUser);

			var eventArgs = new AuditAndCashEventArgs(null, null);
			AccountingAuditHelper.HandleAuditTransaction(null, eventArgs);
			AssertEquals("Please select a transaction to audit.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			securityCheckpoint.IsAllowed = false;
			var transaction = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction });
			AccountingAuditHelper.HandleAuditTransaction(null, eventArgs);
			var expectedErrorMsg = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Actions -> Audit Transaction";
			AssertEquals(expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			securityCheckpoint.IsAllowed = true;
			transaction.AH_GS_NKAuditedBy = anotherUser;
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction });
			AccountingAuditHelper.HandleAuditTransaction(null, eventArgs);
			AssertEquals("Selected transaction has been already audited.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction1 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction1.AH_GS_NKAuditedBy = anotherUser;
			var transaction2 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction1, transaction2 });
			AccountingAuditHelper.HandleAuditTransaction(null, eventArgs);
			AssertEquals("Some of the selected transactions have been already audited.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction3 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction3.AH_SystemCreateUser = currentUser;
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction3 });
			AccountingAuditHelper.HandleAuditTransaction(null, eventArgs);
			AssertEquals("You cannot audit transaction created by yourself.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction4 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction4.AH_SystemCreateUser = currentUser;
			var transaction5 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction4, transaction5 });
			AccountingAuditHelper.HandleAuditTransaction(null, eventArgs);
			AssertEquals("You cannot audit transactions created by yourself.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction6 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction6.AH_SystemCreateUser = anotherUser;
			var transaction7 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction7.AH_SystemCreateUser = anotherUser;
			Factory.Save();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction6, transaction7 });
			AccountingAuditHelper.HandleAuditTransaction(null, eventArgs);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(currentUser, transaction6.AH_GS_NKAuditedBy);
			AssertEquals(1, transaction6.Logs.Find(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.DisplayEventReference == "Transaction Audited").Count());
			AssertEquals(currentUser, transaction7.AH_GS_NKAuditedBy);
			AssertEquals(1, transaction7.Logs.Find(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.DisplayEventReference == "Transaction Audited").Count());
		}

		public void TestHandleUndoAuditTransaction()
		{
			var securityCheckpoint = Env.Security.ReceivablesUndoAuditTransaction;
			var currentUser = GlbStaff.CurrentUser.GS_Code;
			const string anotherUser = "JYW";
			AssertNotEquals("Precondition: The two users should not be identical.", currentUser, anotherUser);

			var eventArgs = new AuditAndCashEventArgs(null, null);
			AccountingAuditHelper.HandleUndoAuditTransaction(null, eventArgs);
			AssertEquals("Please select a transaction to undo audit.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			securityCheckpoint.IsAllowed = false;
			var transaction = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction });
			AccountingAuditHelper.HandleUndoAuditTransaction(null, eventArgs);
			var expectedErrorMsg = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Actions -> Undo Audit Transaction";
			AssertEquals(expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			securityCheckpoint.IsAllowed = true;
			var transaction1 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction1 });
			AccountingAuditHelper.HandleUndoAuditTransaction(null, eventArgs);
			AssertEquals("Selected transaction has not been audited yet.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction2 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction1.AH_GS_NKAuditedBy = anotherUser;
			var transaction3 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction2, transaction3 });
			AccountingAuditHelper.HandleUndoAuditTransaction(null, eventArgs);
			AssertEquals("Some of the selected transactions have not been audited yet.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction4 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction4.AH_SystemCreateUser = currentUser;
			transaction4.AH_GS_NKAuditedBy = anotherUser;
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction4 });
			AccountingAuditHelper.HandleUndoAuditTransaction(null, eventArgs);
			AssertEquals("You cannot undo audit transaction not audited by yourself.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction5 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction5.AH_SystemCreateUser = currentUser;
			transaction5.AH_GS_NKAuditedBy = anotherUser;
			var transaction6 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction6.AH_GS_NKAuditedBy = anotherUser;
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction5, transaction6 });
			AccountingAuditHelper.HandleUndoAuditTransaction(null, eventArgs);
			AssertEquals("Some of the selected transactions were not audited by yourself.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction7 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction7.AH_SystemCreateUser = anotherUser;
			transaction7.AH_GS_NKAuditedBy = currentUser;
			var transaction8 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			transaction8.AH_SystemCreateUser = anotherUser;
			transaction8.AH_GS_NKAuditedBy = currentUser;
			Factory.Save();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction7, transaction8 });
			AccountingAuditHelper.HandleUndoAuditTransaction(null, eventArgs);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(ZString.Empty, transaction7.AH_GS_NKAuditedBy);
			AssertEquals(1, transaction7.Logs.Find(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.DisplayEventReference == "Transaction Undo Audited").Count());
			AssertEquals(ZString.Empty, transaction8.AH_GS_NKAuditedBy);
			AssertEquals(1, transaction8.Logs.Find(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.DisplayEventReference == "Transaction Undo Audited").Count());
		}

		public void TestHandleRecordCashier()
		{
			var securityCheckpoint = Env.Security.ReceivablesRecordCashier;
			var currentUser = GlbStaff.CurrentUser.GS_Code;
			const string anotherUser = "JYW";
			AssertNotEquals("Precondition: The two users should not be identical.", currentUser, anotherUser);

			var eventArgs = new AuditAndCashEventArgs(null, null);
			AccountingAuditHelper.HandleRecordCashier(null, eventArgs);
			AssertEquals("Please select a transaction to record cashier.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			securityCheckpoint.IsAllowed = false;
			var arReceipt = Factory.NewWithValidTestData<ARReceipt>();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt });
			AccountingAuditHelper.HandleRecordCashier(null, eventArgs);
			var expectedErrorMsg = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Actions -> Record Cashier";
			AssertEquals(expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			securityCheckpoint.IsAllowed = true;
			var arReceipt1 = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt1.AH_GS_NKCashier = anotherUser;
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt1 });
			AccountingAuditHelper.HandleRecordCashier(null, eventArgs);
			AssertEquals("Cashier detail has been recorded against selected transaction.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var arReceipt2 = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt2.AH_GS_NKCashier = anotherUser;
			var arReceipt3 = Factory.NewWithValidTestData<ARReceipt>();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt2, arReceipt3 });
			AccountingAuditHelper.HandleRecordCashier(null, eventArgs);
			AssertEquals("Cashier details have been recorded against some of the selected transactions.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var transaction = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { transaction });
			AccountingAuditHelper.HandleRecordCashier(null, eventArgs);
			AssertEquals("Selected transaction is non-cash transaction.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var arReceipt4 = Factory.NewWithValidTestData<ARReceipt>();
			var transaction2 = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new[] { arReceipt4, transaction2 });
			AccountingAuditHelper.HandleRecordCashier(null, eventArgs);
			AssertEquals("Selected transactions include non-cash transactions.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var arReceipt5 = Factory.NewWithValidTestData<ARReceipt>();
			var arReceipt6 = Factory.NewWithValidTestData<ARReceipt>();
			Factory.Save();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt5, arReceipt6 });
			AccountingAuditHelper.HandleRecordCashier(null, eventArgs);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(currentUser, arReceipt5.AH_GS_NKCashier);
			AssertEquals(1, arReceipt5.Logs.Find(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.DisplayEventReference == "Cashier Recorded").Count());
			AssertEquals(currentUser, arReceipt6.AH_GS_NKCashier);
			AssertEquals(1, arReceipt6.Logs.Find(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.DisplayEventReference == "Cashier Recorded").Count());
		}

		public void TestHandleClearCashier()
		{
			var securityCheckpoint = Env.Security.ReceivablesClearCashier;
			var currentUser = GlbStaff.CurrentUser.GS_Code;
			var anotherUser = "JYW";
			AssertNotEquals("Precondition: The two users should not be identical.", currentUser, anotherUser);

			var eventArgs = new AuditAndCashEventArgs(null, null);
			AccountingAuditHelper.HandleClearCashier(null, eventArgs);
			AssertEquals("Please select a transaction to clear cashier.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessages();
			securityCheckpoint.IsAllowed = false;
			var arReceipt = Factory.NewWithValidTestData<ARReceipt>();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt });
			AccountingAuditHelper.HandleClearCashier(null, eventArgs);
			var expectedErrorMsg = @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Receivables -> Receivables Transactions -> Actions -> Clear Cashier";
			AssertEquals(expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			securityCheckpoint.IsAllowed = true;
			var arReceipt1 = Factory.NewWithValidTestData<ARReceipt>();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt1 });
			AccountingAuditHelper.HandleClearCashier(null, eventArgs);
			AssertEquals("No cashier detail has been recorded against selected transaction.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var arReceipt2 = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt2.AH_GS_NKCashier = anotherUser;
			var arReceipt3 = Factory.NewWithValidTestData<ARReceipt>();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt2, arReceipt3 });
			AccountingAuditHelper.HandleClearCashier(null, eventArgs);
			AssertEquals("No cashier details have been recorded against some of the selected transactions.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var arReceipt4 = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt4.AH_GS_NKCashier = anotherUser;
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt4 });
			AccountingAuditHelper.HandleClearCashier(null, eventArgs);
			AssertEquals("You cannot clear cashier details of transaction of which you are not the cashier.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var arReceipt5 = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt5.AH_GS_NKCashier = anotherUser;
			var arReceipt6 = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt6.AH_GS_NKCashier = anotherUser;
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt5, arReceipt6 });
			AccountingAuditHelper.HandleClearCashier(null, eventArgs);
			AssertEquals("You cannot clear cashier details of transactions of which you are not the cashier.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var arReceipt7 = Factory.NewWithValidTestData<ARReceipt>();
			var arReceipt8 = Factory.NewWithValidTestData<ARReceipt>();
			arReceipt7.AH_GS_NKCashier = currentUser;
			arReceipt8.AH_GS_NKCashier = currentUser;
			Factory.Save();
			eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { arReceipt7, arReceipt8 });
			AccountingAuditHelper.HandleClearCashier(null, eventArgs);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(ZString.Empty, arReceipt7.AH_GS_NKCashier);
			AssertEquals(1, arReceipt7.Logs.Find(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.DisplayEventReference == "Cashier Cleared").Count());
			AssertEquals(ZString.Empty, arReceipt8.AH_GS_NKCashier);
			AssertEquals(1, arReceipt8.Logs.Find(x => x.SL_SE_NKEvent == Events.EditedARecordCode && x.DisplayEventReference == "Cashier Cleared").Count());
		}

		public void TestTransfersShouldHandledInPairs()
		{
			var securityCheckpoint = Env.Security.ReceivablesUndoAuditTransaction;
			var currentUser = GlbStaff.CurrentUser.GS_Code;
			var groupId = ZGuid.NewZGuid();

			var transferFromRow = Factory.New<BankTransferFromRow>();
			var transferToRow = Factory.New<BankTransferToRow>();
			var reversedCharges = TestObjectCreator.CreateDirectPayment(ZDateTime.Now, 0m, 0m, 0m, 0m);

			SetTransferSettings(transferToRow, (ZByte)4, "T0088895", 200M, groupId);
			SetTransferSettings(transferFromRow, (ZByte)5, "T0088895", -200M, groupId);
			SetTransferSettings(reversedCharges, (ZByte)6, "T0088896", -2M, groupId);
			reversedCharges.Lines[0].AL_LineAmount = reversedCharges.Lines[1].AL_LineAmount = -1m;
			reversedCharges.Lines[0].AL_OSAmount = reversedCharges.Lines[1].AL_OSAmount = -1m;

			Factory.Save();

			var eventArgs = new AuditAndCashEventArgs(securityCheckpoint, new AccTransactionHeader[] { transferFromRow });
			AccountingAuditHelper.HandleRecordCashier(null, eventArgs);
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(currentUser, transferFromRow.AH_GS_NKCashier);
			AssertEquals(currentUser, transferToRow.AH_GS_NKCashier);
		}

		void SetTransferSettings(TransactionHeader transfer, ZByte count, ZString transactionNum, ZDecimal amount, ZGuid groupID)
		{
			transfer.AH_TransactionCount = count;
			transfer.AH_TransactionNum = transactionNum;
			transfer.AH_InvoiceAmount = amount;
			transfer.AH_OSTotal = amount;
			transfer.AH_TransactionBelongsToGroup = groupID;
		}

		protected TestObjectCreator TestObjectCreator => fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory));
		protected TestObjectCreator fTestObjectCreator;
	}
}
