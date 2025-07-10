using System.Net;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	public partial class ZTimeEditExColumnStyle : ZCustomControlColumnStyle
	{
		protected override string GetCellStyleString(CurrencyManager source, int rowNum)
		{
			return $"{base.GetCellStyleString(source, rowNum)};white-space: pre;";
		}

		protected override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			var dateTimeText = GetTextFromDateTimeWithColonIfEmpty(source, rowNum);
			return (MarkupString)WebUtility.HtmlEncode(dateTimeText);
		}
	}
}
