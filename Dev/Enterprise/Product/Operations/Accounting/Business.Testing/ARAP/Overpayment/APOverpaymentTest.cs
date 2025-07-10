using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Overpayment.Testing
{
	[TestedType(typeof(APOverpayment))]
	public class APOverpaymentTest : OverpaymentTest
	{
		public void TestLedger()
		{
			AssertEquals("should be AP", LedgerTypes.AccountsPayable, ((APOverpayment)GetNewBusinessObject()).Ledger_ForTestOnly);
		}

		public void TestCheckpointToUnmatch()
		{
			AssertSame("Should be checkpoint to unmatch AP overpayment", Env.Security.PayablesUnMatchTransactionsOverpaymentType, ((IMiscellaneousTransaction)GetNewBusinessObject()).CheckpointForUnmatch);
		}
	}
}
