using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using Enterprise.Messaging.MessageProcessors;
using Res = Enterprise.Customs.Common.Res;

namespace Enterprise.Customs.Business.MessageInterpretation
{
	public static class TableInterpretation
	{
		#region Attributes

		public static class Attributes
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html text")]
			public static NameValueCollection NoBorder
			{
				get { return new NameValueCollection { { "border", "0" } }; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html text")]
			public static NameValueCollection AlignLeft
			{
				get { return new NameValueCollection { { "align", "left" } }; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html text")]
			public static NameValueCollection AlignRight
			{
				get { return new NameValueCollection { { "align", "right" } }; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html text")]
			public static NameValueCollection AlignCenter
			{
				get { return new NameValueCollection { { "align", "center" } }; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html text")]
			public static NameValueCollection GetColspanAttribute(int count)
			{
				return new NameValueCollection { { "colspan", count.ToString(CultureInfo.InvariantCulture) } };
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html text")]
			public static NameValueCollection FullWidth
			{
				get { return new NameValueCollection { { "width", "100%" } }; }
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Html text")]
			public static NameValueCollection GetColorAttribute(string color)
			{
				return new NameValueCollection { { "bgcolor", color } };
			}
		}

		#endregion

		#region AddTableInterpretationIfRequired

		public static void AddTableInterpretationIfRequired(HtmlTableCreator table, IEnumerable lines, ITableValues total = null, NameValueCollection align = null)
		{
			AddTableInterpretationIfRequired(table, lines.Cast<ITableInterpretation>(), total, align);
		}

		public static void AddTableInterpretationIfRequired(HtmlTableCreator table, IEnumerable<ITableInterpretation> lines, ITableValues total = null, NameValueCollection align = null)
		{
			if (lines.Any())
			{
				table.WriteRow(GetTableInterpretation(lines, total, align));
			}
		}

		public static void AddTableInterpretationIfRequired(HtmlTableCreator table, ITableInterpretation line, ITableValues total = null)
		{
			if (line != null)
			{
				table.WriteRow(GetTableInterpretation(new[] { line }, total));
			}
		}

		#endregion

		#region GetTableInterpretation

		public static string GetTableInterpretation(IEnumerable lines, ITableValues total = null, NameValueCollection align = null)
		{
			return GetTableInterpretation(lines.Cast<ITableInterpretation>(), total, align);
		}

		public static string GetTableInterpretation(IEnumerable<ITableInterpretation> lines, ITableValues total = null, NameValueCollection align = null)
		{
			var result = string.Empty;
			var firstLine = lines.FirstOrDefault();
			if (firstLine != null)
			{
				if (!string.IsNullOrEmpty(firstLine.Caption))
				{
					var caption = new HtmlTableCreator(new[] { firstLine.Caption }, Attributes.FullWidth);
					result = caption.ToHtml();
				}

				if (align == null)
				{
					align = Attributes.AlignRight;
				}

				var table = new HtmlTableCreator(firstLine.Titles, Attributes.FullWidth) { EnableHTMLEncoding = false };
				foreach (var line in lines)
				{
					table.WriteRow(align, line.Values.ToArray());
				}

				if (total != null)
				{
					table.WriteRow(align, total.Values.ToArray());
				}
				result += table.ToHtml();
			}
			return result;
		}

		#endregion

		public static string GetBooleanAsString(bool value)
		{
			return value ? Res.GetString("19b72db7-83c0-4875-81a3-a0be729b6420", "Yes") : Res.GetString("80e7c808-b836-4964-91ad-e43d2aa3d4c1", "No");
		}

		public static string WrapText(string text, int maxLength)
		{
			const char space = ' ';
			if (text.LastIndexOf(space) <= maxLength)
			{
				return text;
			}

			var max = maxLength;
			var words = text.Split(new[] { space }, StringSplitOptions.RemoveEmptyEntries);
			var builder = new StringBuilder();
			foreach (var word in words)
			{
				builder.Append(word);
				builder.Append(space);
				if (builder.Length >= max)
				{
					builder.Append("<br>");
					max = builder.Length + maxLength;
				}
			}
			return builder.ToString();
		}
	}

	#region ITableInterpretation

	public interface ITableInterpretation : ITableValues
	{
		string Caption { get; }
		IEnumerable<string> Titles { get; }
	}

	public interface ITableValues
	{
		IEnumerable<object> Values { get; }
	}

	#endregion
}
