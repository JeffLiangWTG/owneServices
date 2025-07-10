using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.Data.Registry.Testing
{
	sealed class BaseDbRegistryItemTest : TransactionedTestCase
	{
		public void TestValues()
		{
			ByteArrayDbRegistryItemForTesting testItem = new ByteArrayDbRegistryItemForTesting(preserveTestValue: true);
			AssertEquals("Initially - Default Value", ByteArrayDbRegistryItemForTesting.DefaultValueConst, testItem.LoadValue(TestConnection));

			testItem.SaveValue(new byte[] { 9, 9 }, TestConnection);
			AssertEquals("After Save", new byte[] { 9, 9 }, testItem.LoadValue(TestConnection));
			Assert("PreserveTestValue should be true", testItem.GetPreserveTestValue(TestConnection));

			TestConnection.ExecuteNonQuery("DELETE dbo.StmData WHERE SD_Name = @Name", cmd => cmd.AddParameterBasedOnDbColumn("@Name", testItem.ItemName, StmDataSchema.SD_Name));
			AssertEquals("After Delete - Default Value", ByteArrayDbRegistryItemForTesting.DefaultValueConst, testItem.LoadValue(TestConnection));

			testItem = new ByteArrayDbRegistryItemForTesting(preserveTestValue: false);
			Assert("PreserveTestValue should be false", !testItem.GetPreserveTestValue(TestConnection));
		}

		public void TestAccessingRegistryThrowsExceptionIfSystemDb()
		{
			AssertAccessToRegistryFromDatabase(Db.DatabaseName, true);
			AssertAccessToRegistryFromDatabase(Db.SqlMasterDb, false);
			AssertAccessToRegistryFromDatabase(Db.SqlMsdb, false);
		}

		void AssertAccessToRegistryFromDatabase(string dbName, bool shouldSucceed)
		{
			var testItem = new BoolDbRegistryItem("SomeTestBoolRegItem", false, false);

			using (var conn = Db.NewAdminConnection(Db.ServerName, dbName))
			using (conn.BeginTransactionWithManager())
			{
				CombineAssertions("Attempt to Load/Save Registry", () =>
				{
					if (shouldSucceed)
					{
						AssertNoExceptionThrown("Load", () => testItem.LoadValue(conn));
						AssertNoExceptionThrown("Save", () => testItem.SaveValue(false, conn));
					}
					else
					{
						var expectedError = $"Attempt to access Registry whilst connected to a system database ({dbName}).";
						AssertExceptionThrown<RegistryAccessFromSystemDatabaseException>("Load", expectedError, () => testItem.LoadValue(conn));
						AssertExceptionThrown<RegistryAccessFromSystemDatabaseException>("Save", expectedError, () => testItem.SaveValue(false, conn));
					}
				});
			}
		}

		protected override DbConnection TestConnection
		{
			get { return testConnection; }
		}
		readonly DbConnection testConnection = Db.Connection;
	}
}
