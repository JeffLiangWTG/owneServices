using System;
using Enterprise.Integration.Accounting;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ExchangeRateEnumsExtensionsTest : TestCase
	{
		public void TestGetLedgerFromCode()
		{
			AssertEquals(ExchangeRateValidLedgerEnum.None, ExchangeRateEnumsExtensions.GetLedgerFromCode(null));
			AssertEquals(ExchangeRateValidLedgerEnum.None, ExchangeRateEnumsExtensions.GetLedgerFromCode(string.Empty));

			AssertEquals(ExchangeRateValidLedgerEnum.AR, ExchangeRateEnumsExtensions.GetLedgerFromCode(LedgerTypes.AccountsReceivable));

			AssertEquals(ExchangeRateValidLedgerEnum.AP, ExchangeRateEnumsExtensions.GetLedgerFromCode(LedgerTypes.AccountsPayable));
			AssertEquals(ExchangeRateValidLedgerEnum.AP, ExchangeRateEnumsExtensions.GetLedgerFromCode(LedgerTypes.IncompleteTransactions));
			AssertEquals(ExchangeRateValidLedgerEnum.AP, ExchangeRateEnumsExtensions.GetLedgerFromCode(LedgerTypes.UnapprovedPayableTransactions));
			AssertEquals(ExchangeRateValidLedgerEnum.AP, ExchangeRateEnumsExtensions.GetLedgerFromCode(LedgerTypes.TransactionsPendingAllocation));

			AssertExceptionThrown<InvalidOperationException>("Unsupported ledger ZzZ", () => ExchangeRateEnumsExtensions.GetLedgerFromCode("ZzZ"));
		}
	}
}