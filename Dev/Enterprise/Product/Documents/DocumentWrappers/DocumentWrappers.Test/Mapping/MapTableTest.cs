using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.DocumentWrappers.Mapping.Testing
{
	sealed class MapTableTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			MapTable wrapperTable = new MapTable("Roitter", "Tickers", "Fred", 30, "Flintstone", 25, true);
			AssertEquals("Roitter", wrapperTable.TypeTitle);
			AssertEquals("Tickers", wrapperTable.DefaultFieldName);
			AssertEquals("Fred", wrapperTable.LeftColumnTitle);
			AssertEquals("Flintstone", wrapperTable.RightColumnTitle);
			AssertEquals(true, wrapperTable.IsNonGeneric);
		}

		public void TestToString()
		{
			MapTable wrapperTable = new MapTable("Roitter", "Tickers", "Fred", 30, "Flintstone", 25, true);
			wrapperTable.AddLine("4", "Barney", "Rubble");
			wrapperTable.AddLine("1", "Hot", "Nuts");
			wrapperTable.AddLine("3", "", "");
			wrapperTable.AddLine("2", "Another", "Toblerone");
			AssertMultilineASCIIEquals("wrapperTable.ToString()", expectedToString.Trim(), wrapperTable.ToString());
		}
		#region expectedToString
		const string expectedToString = @"
Roitter                          (Default Field: Tickers)
=========================================================
Fred                            Flintstone
---------------------------------------------------------
Hot                             Nuts
Another                         Toblerone

Barney                          Rubble
";
		#endregion

		public void TestGetLines()
		{
			MapTable wrapperTable = new MapTable("Rolling", "Stone", "Frod", 32, "Fluntstone", 28, false);
			wrapperTable.AddLine("3", "Barney", "Rubble");
			wrapperTable.AddLine("1", "Hot", "Nuts");
			wrapperTable.AddLine("2", "Another", "Toblerone");
			List<MapTable.Line> lines = wrapperTable.GetLines();
			AssertEquals(3, lines.Count);
			AssertEquals("Hot", lines[0].LeftColumnValue);
			AssertEquals("Another", lines[1].LeftColumnValue);
			AssertEquals("Barney", lines[2].LeftColumnValue);
		}

		public void TestDefaultFieldIncludingTitle()
		{
			MapTable wrapperTable = new MapTable("", "Lickers", "", 0, "", 0, true);
			AssertEquals("(Default Field: Lickers)", wrapperTable.DefaultFieldIncludingTitle);

			wrapperTable = new MapTable("", "", "", 0, "", 0, true);
			AssertEquals("", wrapperTable.DefaultFieldIncludingTitle);
		}
	}
}
