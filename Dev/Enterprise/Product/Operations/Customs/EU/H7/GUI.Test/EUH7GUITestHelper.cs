using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing
{
	public static class EUH7GUITestHelper
	{
		public static void AssertGridLayout(ZGrid grid, IReadOnlyList<string> columnNames)
		{
			AssertGridLayout(grid, columnNames.Select(c => (c, default(Type))).ToArray());
		}

		public static void AssertGridLayout(ZGrid grid, IReadOnlyList<(string Name, Type Type)> columnInfos)
		{
			Assertion.Assert(string.Format("{0} is visible", grid.Name), grid.Visible);

			var columnsCount = grid.ColumnStyles.Cast<ZGridColumnInfo>().Count(c => !c.IsUnavailable && c.IsVisible);
			Assertion.AssertEquals($"Expected columns: {columnInfos.Count}, Actual columns: {columnsCount}", columnInfos.Count, columnsCount);

			foreach (var columnInfo in columnInfos)
			{
				var columnName = columnInfo.Name;
				var columnType = columnInfo.Type;

				var column = grid.GetColumnStyle(columnName);

				if (columnType != null)
				{
					Assertion.AssertType(columnInfo.Type, column);
				}

				Assertion.AssertNotNull(string.Format("{0} column exists", columnName), column);
				Assertion.Assert(string.Format("{0}({1}) column is visible", column.CaptionResourceString.Caption, columnName), column.IsVisible);
			}
		}

		public static void AssertReadOnlyGridColumns(ZGrid grid, bool shouldBeReadOnly, params string[] columnNames)
		{
			foreach (var columnName in columnNames)
			{
				var columnStyle = grid.Columns.Single(c => c.ColumnName == columnName).ColumnStyle;
				Assertion.AssertEquals(string.Format("{0} column {1} be readonly", columnStyle.HeaderText, shouldBeReadOnly ? "should" : "should not"), shouldBeReadOnly, columnStyle.ReadOnly || columnStyle.PropertyDescriptor.IsReadOnly);
			}
		}
	}
}
