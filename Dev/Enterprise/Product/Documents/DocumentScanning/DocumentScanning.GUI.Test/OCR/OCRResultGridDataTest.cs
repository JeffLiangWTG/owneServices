using System.Collections.Specialized;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.OCR.Testing
{
	sealed class OCRResultGridDataTest : TestCase
	{
		public void TestConvertStringToCollection()
		{
			string lines = "What is\ngoing on\n\nhere";
			StringCollection result = OCRResultGridData.ConvertStringToCollection(lines);
			AssertEquals("Count", 4, result.Count);

			AssertEquals("string 1", "What is", result[0]);
			AssertEquals("string 2", "going on", result[1]);
			AssertEquals("string 3", "", result[2]);
			AssertEquals("string 4", "here", result[3]);
		}

		public void TestConvertStringToCollectionEmpty()
		{
			StringCollection result = OCRResultGridData.ConvertStringToCollection("");
			AssertEquals("Count", 1, result.Count);

			AssertEquals("empty string", "", result[0]);
		}

		public void TestSetColumnData()
		{
			StringCollection names = new StringCollection();
			names.AddRange(new string[] { "Smith", "Jones" });

			StringCollection numbers = new StringCollection();
			numbers.AddRange(new string[] { "100", "200", "300" });

			OCRResultGridData data = new OCRResultGridData(2);
			AssertEquals("Row Count", 0, data.ResultTable.Rows.Count);

			data.SetColumnData(names, 0);
			AssertEquals("Row Count", 2, data.ResultTable.Rows.Count);

			data.SetColumnData(numbers, 1);
			AssertEquals("Row Count", 3, data.ResultTable.Rows.Count);

			string curColumnName = OCRResultGridData.GetColumnName(0);
			string curValue = (string)data.ResultTable.Rows[0][curColumnName];
			AssertEquals("Name 1", "Smith", curValue);

			curValue = (string)data.ResultTable.Rows[1][curColumnName];
			AssertEquals("Name 2", "Jones", curValue);

			curValue = (string)data.ResultTable.Rows[2][curColumnName];
			AssertEquals("Name 2", "", curValue);

			curColumnName = OCRResultGridData.GetColumnName(1);
			curValue = (string)data.ResultTable.Rows[0][curColumnName];
			AssertEquals("Number 1", "100", curValue);

			curValue = (string)data.ResultTable.Rows[1][curColumnName];
			AssertEquals("Number 2", "200", curValue);

			curValue = (string)data.ResultTable.Rows[2][curColumnName];
			AssertEquals("Number 3", "300", curValue);
		}

		public void TestSetColumnDataEmptyColumn()
		{
			StringCollection emptyLines = new StringCollection();
			OCRResultGridData data = new OCRResultGridData(2);
			data.SetColumnData(emptyLines, 0);

			AssertEquals("Row Count", 0, data.ResultTable.Rows.Count);
		}
	}
}
