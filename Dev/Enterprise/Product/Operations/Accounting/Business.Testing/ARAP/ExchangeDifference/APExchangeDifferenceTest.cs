using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Testing
{
	[TestedType(typeof(APExchangeDifference))]
	public class APExchangeDifferenceTest : ExchangeDifferenceTest
	{
		public void TestLedger()
		{
			AssertEquals("should be AP", ZArchitecture.Core.LedgerTypes.AccountsPayable, ((APExchangeDifference)GetNewBusinessObject()).Ledger_ForTestOnly);
		}
	}
}
