using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Interfaces.Testing;

namespace Enterprise.Accounting.Business.Base.Reversing.Testing
{
	class MiscellaneousTransactionReversingTest : ReversingBaseTest
	{
		public void TestCanTransactionBeReversed()
		{
			Assert("Transaction must not be reversed.", !MiscellaneousTransactionReversing.CanReverseTransaction);
			AssertEquals("Transaction must not be reversed with correct message.", MiscellaneousTransactionReversing.CantReverseError_ForTestOnly,
				MiscellaneousTransactionReversing.GenerateCantReverseErrorMessage_ForTestOnly());
		}

		public new void TestInactiveOrgDoesNotAllowReversing()
		{
			Assert("All Miscellaneous Transactions can't be reversed.", true);
		}

		public override void TestErrorMessages()
		{
			ZString expectedErrorMessage = "Overpayments, Discounts and Exchange Differences can only be reversed by unmatching.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, MiscellaneousTransactionReversing.AlreadyReversedErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be reversed because it has been matched with other transactions.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, MiscellaneousTransactionReversing.MatchedAndCantReverseErrorMessage_ForTestOnly);
			expectedErrorMessage = "This transaction cannot be reversed because it has been cleared in Cashbook. Please unclear this transaction from the cashbook before reversing.";
			AssertEquals("The Error Message should be generated for reversing.", expectedErrorMessage, MiscellaneousTransactionReversing.ClearedInCashBookErrorMessage_ForTestOnly);
		}

		protected override Type GetTestingClassType()
		{
			return typeof(MiscellaneousTransactionReversing);
		}

		protected override void SetupReversingInstance()
		{
			TestIReversingInstance = new TestIMiscellaneousTransaction();
		}

		protected override void SetupReversingIReversingInstance()
		{
			TestReversingIReversingInstance = new TestIMiscellaneousTransaction();
		}

		MiscellaneousTransactionReversing MiscellaneousTransactionReversing
		{
			get { return (MiscellaneousTransactionReversing)Reversing; }
		}
	}
}
