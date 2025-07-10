using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public class AccountingMessageHelperTest : TestCase
	{
		public void TestShowOKCancelMessageReturnsCancel()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AccountingMessageHelper myMessageHelper = new AccountingMessageHelper();

			myMessageHelper.ShowOKCancelMessageReturnsCancel("my message", "my caption");
			AssertEquals("Last message", "my message", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowMessageIfChequeBookUsesSamePrinterReturnsCancel()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			BusinessObject printer = (BusinessObject)factory.New<Enterprise.Integration.DocumentEngine.IStmPrintQueue>();
			printer[StmPrintQueueSchema.SQ_QueueName] = "queque1";
			AccChequeBook chequeBook = factory.New<AccChequeBook>();
			chequeBook.AK_Code = "book1";
			chequeBook.AK_SQ = printer.PK;

			AccChequeBook book2 = factory.New<AccChequeBook>();
			book2.AK_Code = "Book2";
			book2.AK_AutoPrintCheque = ZBool.True;
			book2.AK_SQ = printer.PK;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AccountingMessageHelper myMessageHelper = new AccountingMessageHelper();
			myMessageHelper.ShowMessageIfChequeBookUsesSamePrinterReturnsCancel(book2);

			AssertEquals("Last message is good hopefully", AccChequeBook.WarningPaymentWithChequeBookWithSamePrinterMessage(((ZString)printer[StmPrintQueueSchema.SQ_QueueName]), book2.AK_CurrentNo), UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}
}
