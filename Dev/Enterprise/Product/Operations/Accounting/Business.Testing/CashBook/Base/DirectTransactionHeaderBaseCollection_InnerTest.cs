using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Accounting.Business.CashBook.DirectTransactionHeaderBaseCollection;

namespace Enterprise.Accounting.Business.CashBook.Testing
{
	[TestedType(typeof(DirectTransactionHeaderBaseCollection))]
	public class DirectTransactionHeaderBaseCollection_InnerTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DirectTransactionHeaderBaseCollection(Factory, ZDateTime.Empty);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(BankReconDirectPayment));
		}

		#region Test Notify User About Not Supported Exception

		public void TestNotifyUserAboutNotSupportedException()
		{
			DirectTransactionHeaderBaseCollection testCollection = (DirectTransactionHeaderBaseCollection)GetCollectionToTest();
			NumberOfTimesEventWasCalledDuringTest = 0;
			testCollection.NotifyUserAboutNotSupportedException += new ShowNotSupportedExceptionHandler(TestNotifyUserAboutNotSupportedException);
			BankReconDirectPayment directPayment = (BankReconDirectPayment)GetNewElementToAddToTheCollection();
			testCollection.Add(directPayment);
			testCollection.RemoveAndDelete(directPayment);
			AssertEquals("Event should not been raised", 0, NumberOfTimesEventWasCalledDuringTest);
			directPayment = (BankReconDirectPayment)GetNewElementToAddToTheCollection();
			testCollection.Add(directPayment);
			directPayment.Factory.Save();
			ExpectedEventErrorMessage = "You cannot delete Accounting Transactions in Database.";
			testCollection.RemoveAndDelete(directPayment);
			AssertEquals("Event should have been raised", 1, NumberOfTimesEventWasCalledDuringTest);
		}

		void TestNotifyUserAboutNotSupportedException(object sender, string message)
		{
			NumberOfTimesEventWasCalledDuringTest++;
			AssertEquals("Event Message", ExpectedEventErrorMessage, message);
			ExpectedEventErrorMessage = ZString.Empty;
		}

		int NumberOfTimesEventWasCalledDuringTest;
		string ExpectedEventErrorMessage;

		#endregion
	}
}
