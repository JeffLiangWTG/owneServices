using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	[TestedType(typeof(AROverpayment))]
	public class AROverpaymentTest : OverpaymentTest
	{
		public void TestLedger()
		{
			AssertEquals("should be AR", LedgerTypes.AccountsReceivable, ((AROverpayment)GetNewBusinessObject()).Ledger_ForTestOnly);
		}

		public void TestCheckpointToUnmatch()
		{
			AssertSame("Should be checkpoint to unmatch AR overpayment", Env.Security.ReceivablesUnMatchTransactionOverpaymentType, ((IMiscellaneousTransaction)GetNewBusinessObject()).CheckpointForUnmatch);
		}
	}
}
