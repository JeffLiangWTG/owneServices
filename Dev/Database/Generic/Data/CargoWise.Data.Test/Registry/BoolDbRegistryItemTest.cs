using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.Data.Registry.Testing
{
	sealed class BoolDbRegistryItemTest : TransactionedTestCase
	{
		public void TestValues()
		{
			var testItem = new BoolDbRegistryItem("BoolDbRegistryItemForTesting", DefaultValueConst);
			AssertEquals("Initially - Default Value", DefaultValueConst, testItem.LoadValue(TestConnection));

			testItem.SaveValue(false, TestConnection);
			AssertEquals("After Save", false, testItem.LoadValue(TestConnection));

			TestConnection.ExecuteNonQuery("DELETE dbo.StmData WHERE SD_Name = @Name", cmd => cmd.AddParameterBasedOnDbColumn("@Name", testItem.ItemName, StmDataSchema.SD_Name));
			AssertEquals("After Delete - Default Value", DefaultValueConst, testItem.LoadValue(TestConnection));
		}

		public const bool DefaultValueConst = true;
	}
}
