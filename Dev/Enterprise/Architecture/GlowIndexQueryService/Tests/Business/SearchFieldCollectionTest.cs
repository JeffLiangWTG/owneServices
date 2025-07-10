using GlowIndexQueryService.Business;
using NUnit.Framework;

namespace GlowIndexQueryService.Tests.Business
{
	sealed class SearchFieldCollectionTest : TestCase
	{
		public void TestIndexer_ReturnsCorrectSearchField()
		{
			var searchField1 = SearchField.Create("Field1");
			var searchField2 = SearchField.Create("Field2");
			var searchField3 = SearchField.Create("Field3");
			var searchFields = new[] { searchField1, searchField2, searchField3 };

			var collection = new SearchFieldCollection("", searchFields);

			var result1 = collection["Field1"];
			var result2 = collection["Field2"];
			var result3 = collection["Field3"];

			AssertEquals(searchField1, result1);
			AssertEquals(searchField2, result2);
			AssertEquals(searchField3, result3);
		}

		public void TestIndexer_ReturnsNullForNonExistingField()
		{
			var searchField1 = SearchField.Create("Field1");
			var searchField2 = SearchField.Create("Field2");
			var searchField3 = SearchField.Create("Field3");
			var searchFields = new[] { searchField1, searchField2, searchField3 };

			var collection = new SearchFieldCollection("", searchFields);

			var result = collection["NonExistingField"];

			AssertNull(result);
		}

		public void TestDefaultHiddenIndexSearchFields()
		{
			var hiddenSearchField = new SearchField("UiHiddenField", "Description", typeof(string), uiHidden: true);
			var defaultHiddenField = SearchField.Create("CWDEFAULTHIDDENField");
			var searchField3 = SearchField.Create("Field3");

			var collection = new SearchFieldCollection("", [hiddenSearchField, defaultHiddenField, searchField3]);

			var result = collection.DefaultHiddenIndexSearchFields;

			AssertEquals(2, result.Length);
			AssertEquals(hiddenSearchField, result[0]);
			AssertEquals(defaultHiddenField, result[1]);
		}

		public void TestNormalizeTypeName()
		{
			var collection = new SearchFieldCollection("IStmScheduleTask[[IStmMenuItem]]", []);
			AssertEquals("IStmScheduleTask.Of.IStmMenuItem", collection.EntityType);
		}
	}
}
