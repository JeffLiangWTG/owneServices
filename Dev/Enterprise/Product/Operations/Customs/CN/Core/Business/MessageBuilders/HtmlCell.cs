using System.Globalization;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business;

internal class HtmlCell
{
	public string CellValue { get; private set; }
	public bool IsCaption { get; private set; }
	public int Colspan { get; private set; }
	public bool Bold { get; set; }
	public bool EnableHTMLEncoding { get; set; } = true;

	public HtmlCell() : this(false, 0, string.Empty)
	{
	}

	public HtmlCell(string cellValue) : this(false, 0, cellValue)
	{
	}

	public HtmlCell(bool isCaption, string cellValue) : this(isCaption, 0, cellValue)
	{
	}

	public HtmlCell(int colspan, string cellValue) : this(false, colspan, cellValue)
	{
	}

	public HtmlCell(int colspan) : this(false, colspan, string.Empty)
	{
	}

	public HtmlCell(bool isCaption, int colspan, string cellValue)
	{
		CellValue = cellValue;
		IsCaption = isCaption;
		Colspan = colspan;
	}

	public CellWithFormatting CreateCellWithFormatting()
	{
		var cell = new CellWithFormatting(EnableHTMLEncoding ? CellValue.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") : CellValue);

		if (IsCaption)
		{
			cell.HtmlAttributes.Add((NoResString)"class", "text-align-right");
		}
		if (Bold)
		{
			cell.HtmlAttributes.Add((NoResString)"style", (NoResString)"font-weight: bold;");
		}
		if (Colspan > 0)
		{
			cell.HtmlAttributes.Add((NoResString)"colspan", Colspan.ToString(CultureInfo.InvariantCulture));
		}

		return cell;
	}
}
