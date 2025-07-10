using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(GroupBy))]
	sealed class GroupByTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new GroupBy(String.Empty, String.Empty);
		}

		public void TestDisplayName()
		{
			AssertEquals("disp", new GroupBy("disp", "x").DisplayName);
		}

		public void TestFieldList()
		{
			AssertEquals("field+1", new GroupBy("x", "field+1").FieldList);
		}

		public void TestFieldListRemovesSpaces()
		{
			AssertEquals("field+1,test", new GroupBy("x", "field+1   \t  , test").FieldList);
		}

		public void TestJsonConverter()
		{
			var groupBy = new GroupBy("a", "b");

			var result = JsonConverterHelper.Serialize(groupBy);
			var deserialisedField = JsonConverterHelper.Deserialize<GroupBy>(result);

			AssertEquals("DisplayName", groupBy.DisplayName, deserialisedField.DisplayName);
			AssertEquals("FieldList", groupBy.FieldList, deserialisedField.FieldList);
			AssertEquals("Selected", groupBy.Selected, deserialisedField.Selected);
		}
	}
}
