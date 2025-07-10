using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class ZGridExtensionTest : TestCase
{
	public void TestAddAdjoiningColumn()
	{
		using (var grid = new ZGrid())
		{
			AssertExceptionThrown<ArgumentNullException>("Expected exception when ZGrid parameter is null", () => ZGridExtension.AddAdjoiningColumn(null, "A", new ZTextBoxColumnStyleInfo()));
			AssertExceptionThrown<ArgumentException>("Expected exception when adjoiningColumnName is null or empty", () => ZGridExtension.AddAdjoiningColumn(grid, "", new ZTextBoxColumnStyleInfo()));
			AssertExceptionThrown<ArgumentNullException>("Expected exception when newColumn is null", () => ZGridExtension.AddAdjoiningColumn(grid, "AA", null));

			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("A", 10));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("B", 10));
			grid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo("C", 10));

			grid.AddAdjoiningColumn("B", new ZTextBoxColumnStyleInfo("Z", 1));

			AssertEquals("Columns count", 4, grid.ColumnStyles.Count);
			var columnStyles = grid.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>();
			CombineAssertions("Assert columns position", () =>
			{
				AssertColumnPosition(columnStyles, "A", 0);
				AssertColumnPosition(columnStyles, "B", 1);
				AssertColumnPosition(columnStyles, "Z", 2);
				AssertColumnPosition(columnStyles, "C", 3);
			});
		}

		void AssertColumnPosition(IEnumerable<ZTextBoxColumnStyleInfo> columnStyles, ZString columnName, int expectedColumnPosition)
		{
			AssertEquals($"Column {columnName} position", expectedColumnPosition, columnStyles.IndexOf(x => x.ColumnName == columnName));
		}
	}
}
