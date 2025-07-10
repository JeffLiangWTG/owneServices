using System;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(WorksheetCollection))]
	sealed class WorksheetCollectionTest : ValueObjectCollectionTestCase
	{
		public void TestStringIndexerAndContains()
		{
			WorksheetCollection collection = new WorksheetCollection();

			Worksheet sheet1 = collection.AddNew("Sheet1");
			AssertEquals("AddNew(string) creates new sheet with sheet 1 as name and string indexer works", "Sheet1", collection["Sheet1"].Name);
			AssertEquals("contains is true for new addition", true, collection.Contains("Sheet1"));

			collection.Remove(sheet1);
			try
			{
				string sheetName = collection["Sheet1"].Name;
				Assert("Should Not reach this point", false);
			}
			catch (Exception ex)
			{
				AssertEquals("Should get out of range exception", "Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", ex.Message);
			}
			AssertEquals("contains false for new addition", false, collection.Contains("Sheet1"));

			collection.Add(new Worksheet("Sheet2"));
			AssertEquals("Add updates string index", true, collection.Contains("Sheet2"));

			collection.RemoveAt(0);
			AssertEquals("RemoveAt updates string index", false, collection.Contains("Sheet2"));

			try
			{
				collection.AddNew();
				Assert("Should Not reach this point", false);
			}
			catch (Exception ex)
			{
				AssertEquals("AddNew raises exception", "Worksheet to be added must have a value for name.", ex.Message);
			}

			collection.AddNew("Sheet1");
			collection.AddNew("Sheet2");
			collection.Clear();
			AssertEquals("Clear empties Collection", 0, collection.Count);
			AssertEquals("string index doesnt find sheet1", false, collection.Contains("Sheet1"));
			AssertEquals("string index doesnt find sheet2", false, collection.Contains("Sheet2"));
		}

		public void TestClone()
		{
			WorksheetCollection collection = new WorksheetCollection();
			collection.AddNew("Sheet1");
			collection.AddNew("Sheet2", "Title2");
			WorksheetCollection collectionClone = collection.Clone();

			AssertEquals("Clone contains sheet 1", true, collectionClone.Contains("Sheet1"));
			AssertEquals("Clone contains sheet 2", true, collectionClone.Contains("Sheet2"));
			AssertEquals("Clone contains sheet 2 with the set title", "Title2", collectionClone["Sheet2"].Title);
		}

		public void TestIsEmpty()
		{
			WorksheetCollection collection = new WorksheetCollection();
			collection.AddNew("Sheet1");
			collection.AddNew("Sheet2");
			AssertEquals("Collection has no worksheets that have columns and is empty", true, collection.IsEmpty);

			collection["Sheet1"].ColumnHeadings.Add(new ColumnHeading("Column 1"));
			AssertEquals("Collection has a worksheet with a column and is not empty", false, collection.IsEmpty);

			collection["Sheet1"].ColumnHeadings.RemoveAt(0);
			AssertEquals("Collection no longer has a worksheet with a column and is empty", true, collection.IsEmpty);
		}
	}
}
