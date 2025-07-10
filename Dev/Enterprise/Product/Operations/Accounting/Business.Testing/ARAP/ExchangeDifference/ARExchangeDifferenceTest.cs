using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(ARExchangeDifference))]
	public class ARExchangeDifferenceTest : ExchangeDifferenceTest
	{
		public void TestLedger()
		{
			AssertEquals("should be AR", ZArchitecture.Core.LedgerTypes.AccountsReceivable, ((ARExchangeDifference)GetNewBusinessObject()).Ledger_ForTestOnly);
		}
	}
}
