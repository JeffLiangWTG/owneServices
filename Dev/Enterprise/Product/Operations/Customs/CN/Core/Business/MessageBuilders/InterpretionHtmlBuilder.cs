using System.Collections.Generic;
using System.Text;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business;

internal class InterpretionHtmlBuilder
{
	const string HtmlHeader = @"<html>
<head>
	<meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
	<style>
		body,
		table,
		td {
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: 13px;
			border: 1px solid gray;
			border-collapse: collapse;
		}

		td.text-align-right {
			text-align: right;
			white-space: nowrap;
			background-color: lightgray;
		}
	</style>
</head>
<body>";

	static readonly string HtmlFooter = (NoResString)@"
</body>
</html>";

	readonly List<HtmlTable> tables = new List<HtmlTable>();

	public HtmlTable AddTable()
	{
		var table = new HtmlTable();
		tables.Add(table);
		return table;
	}

	public string ToHtml()
	{
		var htmlBuilder = new StringBuilder();
		htmlBuilder.AppendLine(HtmlHeader);

		tables.ForEach(t => htmlBuilder.Append(t.CreateHtmlTableCreator().ToHtml()));

		htmlBuilder.AppendLine(HtmlFooter);

		return htmlBuilder.ToString()
			.Replace("<table", "\r\n\t<table")
			.Replace("</table>", "\r\n\t</table>")
			.Replace("<tr", "\r\n\t\t<tr")
			.Replace("</tr>", "\r\n\t\t</tr>")
			.Replace("<td", "\r\n\t\t\t<td");
	}
}
