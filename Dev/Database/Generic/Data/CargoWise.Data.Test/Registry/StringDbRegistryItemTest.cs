using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.Data.Registry.Testing
{
	sealed class StringDbRegistryItemTest : TransactionedTestCase
	{
		public void TestValues()
		{
			var testItem = new StringDbRegistryItem("StringDbRegistryItemForTesting", DefaultValueConst);
			AssertEquals("Initially - Default Value", DefaultValueConst, testItem.LoadValue(TestConnection));

			testItem.SaveValue("New Value", TestConnection);
			AssertEquals("After Save", "New Value", testItem.LoadValue(TestConnection));

			TestConnection.ExecuteNonQuery("DELETE dbo.StmData WHERE SD_Name = @Name", cmd => cmd.AddParameterBasedOnDbColumn("@Name", testItem.ItemName, StmDataSchema.SD_Name));
			AssertEquals("After Delete - Default Value", DefaultValueConst, testItem.LoadValue(TestConnection));
		}

		public const string DefaultValueConst = "Initial Value";
	}
}
