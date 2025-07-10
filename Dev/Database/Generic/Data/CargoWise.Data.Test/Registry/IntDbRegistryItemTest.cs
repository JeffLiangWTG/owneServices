using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.Data.Registry.Testing
{
	sealed class IntDbRegistryItemTest : TransactionedTestCase
	{
		public void TestValues()
		{
			var testItem = new IntDbRegistryItem("IntDbRegistryItemForTesting", DefaultValueConst);
			AssertEquals("Initially - Default Value", DefaultValueConst, testItem.LoadValue(TestConnection));

			testItem.SaveValue(200, TestConnection);
			AssertEquals("After Save", 200, testItem.LoadValue(TestConnection));

			TestConnection.ExecuteNonQuery("DELETE dbo.StmData WHERE SD_Name = @Name", cmd => cmd.AddParameterBasedOnDbColumn("@Name", testItem.ItemName, StmDataSchema.SD_Name));
			AssertEquals("After Delete - Default Value", DefaultValueConst, testItem.LoadValue(TestConnection));
		}

		public const int DefaultValueConst = 23;
	}
}
