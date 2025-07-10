using System;
using System.Linq;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	static class EUICS2GUITestHelper
	{
		public static void AssertGridLayout(ZUserControl userControl, string gridName, params string[] columnNames)
		{
			AssertGridLayout(userControl, gridName, columnNames.Select(c => (c, default(Type))).ToArray());
		}

		public static void AssertGridLayout(ZUserControl userControl, string gridName, params (string Name, Type Type)[] columnInfos)
		{
			var grid = userControl.FindSingle<ZGrid>(gridName);
			Assertion.AssertEquals(string.Format("{0} is visible", gridName), true, grid.Visible);

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
				Assertion.AssertEquals(string.Format("{0} column is visible", column.CaptionResourceString.Caption), true, column.IsVisible);
			}
		}

		public static void AssertReadOnlyGridColumns(ZUserControl userControl, string gridName, bool shouldBeReadOnly, params string[] columnNames)
		{
			var grid = userControl.FindSingle<ZGrid>(gridName);

			foreach (var columnName in columnNames)
			{
				var column = grid.GetColumnStyle(columnName);
				Assertion.AssertEquals(string.Format("{0} column {1} be readonly", column.CaptionResourceString.Caption, shouldBeReadOnly ? "should" : "should not"), shouldBeReadOnly, column.IsReadOnly);
			}
		}
	}
}
