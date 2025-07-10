using System.Collections.Generic;
using System.Collections.Specialized;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business;

internal class HtmlTable
{
	readonly List<HtmlRow> rows = new List<HtmlRow>();

	public HtmlRow AddRow(params HtmlCell[] cells)
	{
		var row = new HtmlRow(cells);
		rows.Add(row);
		return row;
	}

	public HtmlTableCreator CreateHtmlTableCreator()
	{
		var tableCreator = new HtmlTableCreator(new NameValueCollection
			{
				{ (NoResString)"cellpadding", "1" },
				{ (NoResString)"cellspacing", "0" },
				{ (NoResString)"width", "100%" }
			})
			{ EnableHTMLEncoding = false };

		rows.ForEach(row =>
		{
			var attributes = new NameValueCollection();
			if (row.IsCaption)
			{
				attributes.Add((NoResString)"style", (NoResString)"text-align:center;background-color:lightgray");
			}
			tableCreator.WriteRowWithFormatting(attributes, row.CreateCellWithFormattings());
		});

		return tableCreator;
	}
}
