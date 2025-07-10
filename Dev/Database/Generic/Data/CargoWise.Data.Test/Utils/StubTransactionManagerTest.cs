using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class StubTransactionManagerTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestDoesNothing()
		{
			using (var manager = new StubTransactionManager())
			{
				manager.CommitTransaction();
				manager.RollbackTransaction();
			}
		}
	}
}
