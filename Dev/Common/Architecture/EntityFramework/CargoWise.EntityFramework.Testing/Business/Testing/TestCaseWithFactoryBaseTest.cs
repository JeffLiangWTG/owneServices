using System;

namespace CargoWise.EntityFramework.Testing
{
	abstract class TestCaseWithFactoryBaseTest : TestCaseWithFactory
	{
		public void TestRollbackOfFactoryConnection()
		{
			Type actualConnectionType = Factory.RowFactory.DbConnection.GetType();
			AssertEquals("DbConnection Type: " + actualConnectionType.FullName, true, actualConnectionType.IsSubclassOf(ExpectedConnectionType));
		}

		protected abstract Type ExpectedConnectionType { get; }
	}
}
