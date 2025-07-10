using System.IO;
using System.Net;
using System.Web;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZMultiLineTextBoxColumnStyle
	{
		protected override MarkupString GetRenderContent(CurrencyManager source, int rowNum, bool alignToRight)
		{
			var text = GetText(GetColumnValueAtRow(source, rowNum));
			using StringReader reader = new StringReader(text);
			return (MarkupString)WebUtility.HtmlEncode(reader.ReadLine());
		}
	}
}
