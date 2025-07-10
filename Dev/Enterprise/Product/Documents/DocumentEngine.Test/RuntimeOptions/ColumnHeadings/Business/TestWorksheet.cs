using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(Worksheet))]
	sealed class TestWorksheet : ValueObjectTestCase
	{
		public void TestTestSheetNameConstuctorANDToString()
		{
			Worksheet sheet = new Worksheet("Sheet1");
			AssertEquals("Sheet name contsructor should set sheet name", "Sheet1", sheet.Name);
			AssertEquals("to string should be sheet name", sheet.Name, sheet.ToString());
		}

		public void TestColumnHeadingCollectionConstructorandClone()
		{
			ColumnHeading[] headings = new ColumnHeading[]
			{
				new ColumnHeading("Column 1"),
				new ColumnHeading("Column 2")
			};

			Worksheet sheet = new Worksheet(new ColumnHeadingCollection(headings), "Sheet1");
			AssertEquals("column heading collection ontsructor should set sheet name", "Sheet1", sheet.Name);
			AssertEquals("column heading collection ontsructor should set columns", 2, sheet.ColumnHeadings.Count);
		}

		public void TestTestSheetNameAndTitleConstuctor()
		{
			Worksheet sheet = new Worksheet("Sheet1", "Title1");
			AssertEquals("Sheet name contsructor should set sheet name", "Sheet1", sheet.Name);
			AssertEquals("The sheet nam and title contsructor should set title", "Title1", sheet.Title);
		}

		public void TestTestColumnHeadingsAndSheetNameAndTitleConstuctor()
		{
			ColumnHeading[] headings = new ColumnHeading[]
			{
				new ColumnHeading("Column 1"),
				new ColumnHeading("Column 2")
			};
			
			Worksheet sheet = new Worksheet(new ColumnHeadingCollection(headings), "Sheet1", "Title1");
			AssertEquals("Sheet name contsructor should set sheet name", "Sheet1", sheet.Name);
			AssertEquals("The sheet nam and title contsructor should set title", "Title1", sheet.Title);
			AssertEquals("Column heading collection contsructor should set columns", 2, sheet.ColumnHeadings.Count);
		}
	}
}
