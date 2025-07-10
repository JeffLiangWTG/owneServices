using System;
using CargoWise.Data;
using NUnit.Framework;

namespace CargoWise.Bi.Product.DataLoad.Testing
{
	class EtlManagerForRecurringExecutionTest : TestCase
	{
		public void TestNewWithNullConnectionThrowsAnException()
		{
			AssertExceptionThrown(
				"Attempting to instantiate EtlExecutionManager with a null main connection...",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: mainDbConnection",
#else
				"Value cannot be null. (Parameter 'mainDbConnection')",
#endif
				() => EtlManagerForRecurringExecution.New(
					mainDbConnection: null,
					biConnection: Db.Connection,
					logger: null // irrelevant to this test
				)
			);

			AssertExceptionThrown(
				"Attempting to instantiate EtlExecutionManager with a null BI connection...",
				typeof(ArgumentNullException),
#if NETFRAMEWORK
				"Value cannot be null.\r\nParameter name: biConnection",
#else
				"Value cannot be null. (Parameter 'biConnection')",
#endif
				() => EtlManagerForRecurringExecution.New(
					mainDbConnection: Db.Connection,
					biConnection: null,
					logger: null // irrelevant to this test
				)
			);
		}
	}
}
