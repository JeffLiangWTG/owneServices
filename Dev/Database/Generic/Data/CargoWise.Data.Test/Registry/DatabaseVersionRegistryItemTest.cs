using System;
using CargoWise.Common;
using NUnit.Framework;

namespace CargoWise.Data.Registry.Testing
{
	sealed class DatabaseVersionRegistryItemTest : TransactionedTestCase
	{
		public void TestWrapFormatException()
		{
			var databaseVersionRegistryItem = new DatabaseVersionRegistryItem("TestItem");
			var stringRegistryItem = new StringDbRegistryItem(databaseVersionRegistryItem.ItemName);

			AssertEquals("Default value when not overridden", 0, databaseVersionRegistryItem.LoadValue(TestConnection));

			stringRegistryItem.SaveValue("12", TestConnection);
			AssertEquals("Valid integer value", 12, databaseVersionRegistryItem.LoadValue(TestConnection));

			stringRegistryItem.SaveValue("abc", TestConnection);
			var expectedMessage = "Format exception converting 'abc' to integer when loading registry item TestItem.";

			AssertExceptionThrown<FormatException>("Should throw exception for invalid value", expectedMessage, () => databaseVersionRegistryItem.LoadValue(TestConnection));
			AssertEquals("Should report error", expectedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();

			stringRegistryItem.SaveValue("", TestConnection);
			expectedMessage = "Format exception converting '' to integer when loading registry item TestItem.";

			AssertExceptionThrown<FormatException>("Should throw exception for empty value", expectedMessage, () => databaseVersionRegistryItem.LoadValue(TestConnection));
			AssertEquals("Should report error", expectedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}
	}
}
