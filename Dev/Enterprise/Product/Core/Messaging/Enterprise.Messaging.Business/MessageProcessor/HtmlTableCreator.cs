using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Messaging.MessageProcessors
{
	public class HtmlTableCreator
	{
		readonly ColumnValueTracker[] columnTracker;
		readonly bool removeEmptyColumnsFromTable;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		public HtmlTableCreator()
			: this("table", (IEnumerable<string>)null)
		{ }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		public HtmlTableCreator(IEnumerable<string> columnTitles, NameValueCollection additionalTableAttributes = null, bool removeEmptyColumns = false)
			: this("table", columnTitles, additionalTableAttributes, removeEmptyColumns)
		{ }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		public HtmlTableCreator(IEnumerable<CellWithFormatting> columnTitlesWithAttributes, NameValueCollection additionalTableAttributes = null, bool removeEmptyColumns = false)
			: this("table", columnTitlesWithAttributes, additionalTableAttributes, removeEmptyColumns)
		{ }

		public HtmlTableCreator(string tableClass)
			: this(tableClass, (IEnumerable<string>)null)
		{ }

		public HtmlTableCreator(string tableClass, IEnumerable<string> columnTitles, NameValueCollection additionalTableAttributes = null, bool removeEmptyColumns = false)
			: this(GetDefaultTableAttributes(additionalTableAttributes, tableClass), columnTitles == null ? null : CreateColumnTitlesWithNullAttributes(columnTitles), removeEmptyColumns)
		{ }

		public HtmlTableCreator(string tableClass, IEnumerable<CellWithFormatting> columnTitlesWithAttributes, NameValueCollection additionalTableAttributes = null, bool removeEmptyColumns = false)
			: this(GetDefaultTableAttributes(additionalTableAttributes, tableClass), columnTitlesWithAttributes, removeEmptyColumns)
		{ }

		public HtmlTableCreator(NameValueCollection tableAttributes, IEnumerable<string> columnTitles = null, bool removeEmptyColumns = false)
			: this(tableAttributes, columnTitles == null ? null : CreateColumnTitlesWithNullAttributes(columnTitles), removeEmptyColumns)
		{ }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		public HtmlTableCreator(NameValueCollection tableAttributes, IEnumerable<CellWithFormatting> columnTitlesWithAttributes, bool removeEmptyColumns = false)
		{
			removeEmptyColumnsFromTable = removeEmptyColumns;
			if (columnTitlesWithAttributes != null)
			{
				columnTracker = columnTitlesWithAttributes.Select(x => new ColumnValueTracker { Column = x.CellValue, HasValue = false }).ToArray();
			}

			Argument.NotNull(tableAttributes, nameof(tableAttributes));
			stringWriter = new StringWriter(CultureInfo.CurrentCulture);
			EnableHTMLEncoding = true;
			stringWriter.Write("<table");

			for (var i = 0; i < tableAttributes.Count; i++)
			{
				stringWriter.Write($" {tableAttributes.Keys[i]}=\"{tableAttributes[i]}\"");
			}

			stringWriter.Write(">");

			if (columnTitlesWithAttributes != null)
			{
				stringWriter.Write("<thead>");
				stringWriter.Write("<tr");
				stringWriter.Write(" class=\"tableheadings\"");
				stringWriter.Write(">");

				foreach (var columnTitleWithAttributes in columnTitlesWithAttributes)
				{
					if (!columnTitleWithAttributes.IsTitle)
					{
						throw new ArgumentException("Column '" + columnTitleWithAttributes.CellValue + "' in columnTitlesWithAttributes must have IsTitle set to true.", nameof(columnTitlesWithAttributes));
					}
					WriteCell(stringWriter, columnTitleWithAttributes);
				}
				stringWriter.Write("</tr>");
				stringWriter.Write("</thead>");
			}
		}

		static IEnumerable<CellWithFormatting> CreateColumnTitlesWithNullAttributes(IEnumerable<string> columnTitles)
		{
			foreach (string columnTitle in columnTitles)
			{
				yield return new CellWithFormatting(columnTitle, true);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		static NameValueCollection GetDefaultTableAttributes(NameValueCollection additionalTableAttributes, string tableClass)
		{
			var attributes = new NameValueCollection { { "border", "1" }, { "cellpadding", "1" }, { "cellspacing", "0" } };
			if (additionalTableAttributes != null)
			{
				attributes.Add(additionalTableAttributes);
			}

			if (tableClass.Length > 0)
			{
				attributes.Add("class", tableClass);
			}

			return attributes;
		}

		readonly StringWriter stringWriter;

		public void WriteRow(params object[] values)
		{
			WriteRow(new NameValueCollection(), values);
		}

		public void WriteRow(NameValueCollection attributes, params object[] values)
		{
			var cells = new List<CellWithFormatting>();
			foreach (var value in values)
			{
				if (value is CellWithFormatting)
				{
					cells.Add((CellWithFormatting)value);
				}
				else
				{
					var text = value != null ? value.ToString() : string.Empty;
					cells.Add(new CellWithFormatting(text));
				}
			}
			WriteRowWithFormatting(attributes, cells.ToArray());
		}

		public void WriteRowWithFormatting(params CellWithFormatting[] cells)
		{
			WriteRowWithFormatting("", cells);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		public void WriteRowWithFormatting(string rowClass, params CellWithFormatting[] cells)
		{
			var attributes = new NameValueCollection();
			if (rowClass.Length > 0)
			{
				attributes.Add("class", rowClass);
			}

			WriteRowWithFormatting(attributes, cells);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		public void WriteRowWithFormatting(NameValueCollection attributes, params CellWithFormatting[] cells)
		{
			Argument.NotNull(attributes, "attributes");
			if (tableTagClosed)
			{
				throw new InvalidOperationException("Html has already been generated - cannot regenerate");
			}
			stringWriter.Write("<tr");

			for (var i = 0; i < attributes.Count; i++)
			{
				stringWriter.Write($" {attributes.Keys[i]}=\"{attributes[i]}\"");
			}

			stringWriter.Write(">");

			int columnIndex = 0;
			foreach (var cell in cells)
			{
				WriteCell(stringWriter, cell);
				if (columnTracker != null && columnIndex < columnTracker.Length && !string.IsNullOrEmpty(cell.CellValue) && !columnTracker[columnIndex].HasValue)
				{
					columnTracker[columnIndex].HasValue = true;
				}
				columnIndex++;
			}
			stringWriter.Write("</tr>");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		void WriteCell(TextWriter writer, CellWithFormatting cell)
		{
			var tag = cell.IsTitle ? "th" : "td";
			writer.Write($"<{tag}");

			for (var i = 0; i < cell.HtmlAttributes.Count; i++)
			{
				writer.Write($" {cell.HtmlAttributes.Keys[i]}=\"{cell.HtmlAttributes[i]}\"");
			}

			writer.Write(">");

			if (cell.CellValue.IsEmpty)
			{
				writer.Write("&nbsp;");
			}
			else
			{
				if (EnableHTMLEncoding)
				{
					writer.Write(ReplaceNewLineCharacters(WebUtility.HtmlEncode(cell.CellValue)));
				}
				else
				{
					writer.Write(ReplaceNewLineCharacters(cell.CellValue));
				}
			}
			writer.Write($"</{tag}>");
		}

		ZString ReplaceNewLineCharacters(ZString input)
		{
			return input.Replace("\r\n", "<br>").Replace("\n", "<br>");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Used only for HTML")]
		public string ToHtml()
		{
			if (!tableTagClosed)
			{
				stringWriter.Write("</table>");
				tableTagClosed = true;
			}
			ZStringBuilder sb = new ZStringBuilder();

			if (removeEmptyColumnsFromTable)
			{
				if (columnTracker.Any(c => !c.HasValue))
				{
					sb.Append("<style>");
					for (int i = 0; i < columnTracker.Length; i++)
					{
						if (!columnTracker[i].HasValue)
						{
							sb.Append(string.Format(CultureInfo.InvariantCulture, "td:nth-of-type({0}),th:nth-of-type({0}){{display: none;}}", i + 1));
						}
					}
					sb.Append("</style>");
				}
			}

			return sb.ToString() + stringWriter.ToString();
		}

		public bool EnableHTMLEncoding;

		bool tableTagClosed;
	}

	class ColumnValueTracker
	{
		public string Column { get; set; }
		public bool HasValue { get; set; }
	}

	public class CellWithFormatting
	{
		public CellWithFormatting()
		{
		}

		public CellWithFormatting(string cellValue, bool isTitle = false)
			: this()
		{
			CellValue = cellValue;
			IsTitle = isTitle;
		}

		public CellWithFormatting(string cellValue, ZString attributeName, ZString attributeValue, bool isTitle = false)
			: this(cellValue, isTitle)
		{
			if (!attributeName.IsEmpty && !attributeValue.IsEmpty)
			{
				HtmlAttributes.Add(attributeName, attributeValue);
			}
		}

		public CellWithFormatting(string cellValue, NameValueCollection attributesValues, bool isTitle = false)
			: this(cellValue, isTitle)
		{
			if (attributesValues.Count > 0)
			{
				HtmlAttributes.Add(attributesValues);
			}
		}

		public ZString CellValue { get; set; }
		public bool IsTitle { get; set; }

		public NameValueCollection HtmlAttributes
		{
			get { return fHtmlAttributes ?? (fHtmlAttributes = new NameValueCollection()); }
		}
		NameValueCollection fHtmlAttributes;
	}
}
