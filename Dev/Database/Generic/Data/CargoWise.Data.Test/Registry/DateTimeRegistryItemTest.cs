using System;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.Data.Registry.Testing
{
	sealed class DateTimeRegistryItemTest : TransactionedTestCase
	{
		public void TestValues()
		{
			var testItem = new DateTimeRegistryItem("DateTimeRegistryItemForTesting", DefaultValueConst);
			TestConnection.ExecuteNonQuery("DELETE dbo.StmData WHERE SD_Name = @Name", cmd => cmd.AddParameterBasedOnDbColumn("@Name", testItem.ItemName, StmDataSchema.SD_Name));
			AssertEquals("Initially - Default Value", DefaultValueConst, testItem.LoadValue(TestConnection));

			SqlDateTime newValue = DateTime.UtcNow;
			testItem.SaveValue(newValue.Value, TestConnection);
			AssertEquals("After Save", newValue.Value, testItem.LoadValue(TestConnection));
		}

		static readonly DateTime DefaultValueConst = new DateTime(1978, 10, 11, 4, 20, 16, 200);
	}
}
