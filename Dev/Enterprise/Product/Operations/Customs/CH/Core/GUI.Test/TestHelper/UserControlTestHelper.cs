using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.CH.GUI.Testing;

public sealed class UserControlTestHelper
{
	public static void AssertColumnStyles(IEnumerable<ZGridColumnInfo> columnsStyleList, string columnName, int index, Type type = null, string caption = null, int? width = null, string groupName = null, CharacterCasing? characterCasing = null)
	{
		var columnStyle = columnsStyleList.FirstOrDefault(x => x.ColumnName == columnName);
		AssertNotNull($"{columnName} is missing (expected at index {index})", columnStyle);
		if (columnStyle != null)
		{
			var visibleColumns = (from column in columnsStyleList where column.IsVisible && !column.IsUnavailable select column).ToArray();
			AssertEquals($"{columnName} index", index, Array.IndexOf(visibleColumns, columnStyle));
			if (caption != null)
			{
				AssertEquals($"{columnName} Caption", caption, columnStyle.CaptionResourceString?.Caption ?? columnStyle.Caption);
			}
			if (type != null)
			{
				AssertType($"{columnName} Type", type, columnStyle);
			}
			if (width != null)
			{
				AssertEquals($"{columnName} Width", width.Value, columnStyle.Width);
			}
			if (groupName != null)
			{
				AssertEquals($"{columnName} GroupName.Caption", groupName, columnStyle.GroupName.Caption);
			}
			if (characterCasing != null)
			{
				AssertEquals($"{columnName} CharacterCasing", characterCasing, columnStyle.CharacterCasing);
			}
		}
	}

	public static void AssertColumnStyles<T>(ZGridColumnInfo[] columnsStyleList, string columnName, int index, string caption = null, int? width = null, string groupName = null, CharacterCasing? characterCasing = null) where T : ZGridColumnInfo
	{
		AssertColumnStyles(columnsStyleList, columnName: columnName, index: index, type: typeof(T), caption: caption, width: width, characterCasing: characterCasing);
	}
}
