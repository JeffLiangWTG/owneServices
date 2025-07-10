using System.Net;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Microsoft.AspNetCore.Components;
using WinzorFramework.Extensions;

namespace Enterprise.ZArchitecture
{
	public partial class ZTextBoxColumnStyle : ZGridColumnStyle
	{
		protected override string GetCellStyleString(CurrencyManager source, int rowNum)
		{
			var cellStyleString = base.GetCellStyleString(source, rowNum);

			var customRowBackgroundColour = parentZGrid.GetCustomRowBackgroundColour(rowNum, IsCellReadOnly(source, rowNum));

			if (IsCellReadOnly(source, rowNum) && customRowBackgroundColour.ToArgb() == 0)
			{
				customRowBackgroundColour = parentZGrid.ReadOnlyColorForRowNum(rowNum);
			}
			cellStyleString += $"background-color: {customRowBackgroundColour.GetColorStyleValue()};";
			cellStyleString += $"white-space: pre;";

			var cellFont = parentZGrid.GetCustomCellFont(rowNum, ColumnInfo.ColumnName);
			if (cellFont != null)
			{
				cellStyleString += cellFont.FontStyleString(parent: parentZGrid);
			}

			return cellStyleString;
		}

		protected override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			var cellText = GetColumnTextAtRow(source, rowNum);
			var textBox = TextBox;
			if (textBox != null
				&& !textBox.IsDisposed
				&& textBox.PasswordChar != '\0')
			{
				cellText = new string(textBox.PasswordChar, cellText.Length);
			}

			if (textBox != null && !textBox.Multiline)
			{
				cellText = cellText.ReplaceLineEndings(" ");
			}
			return (MarkupString)WebUtility.HtmlEncode(cellText);
		}
	}
}
