using System.Collections.Generic;
using System.Linq;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CN.Business;

internal class HtmlRow
{
	public HtmlRow(params HtmlCell[] cells)
	{
		if (cells != null)
		{
			this.cells.AddRange(cells);
		}
	}

	public bool IsCaption { get; set; }

	readonly List<HtmlCell> cells = new List<HtmlCell>();

	public void AddCell(HtmlCell cell) => cells.Add(cell);

	public void AddCells(params HtmlCell[] cells) => this.cells.AddRange(cells);

	public CellWithFormatting[] CreateCellWithFormattings()
	{
		return cells.Select(c => c.CreateCellWithFormatting()).ToArray();
	}
}
