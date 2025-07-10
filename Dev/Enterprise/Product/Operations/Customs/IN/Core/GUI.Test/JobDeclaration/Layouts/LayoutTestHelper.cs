using System;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

sealed class LayoutTestHelper : TestCase
{
	public static void AssertGridColumn<T>(IGridColumnReference column, string columnName, int width, Action<T> additionalAssertsAction = null) where T : ZGridColumnInfo
	{
		var columnInfo = column.CreateGridColumnInfo() as T;
		AssertNotNull(columnInfo);
		AssertEquals("ColumnName", columnName, columnInfo.ColumnName);
		AssertEquals("Width", width, columnInfo.Width);
		additionalAssertsAction?.Invoke(columnInfo);
	}
}
