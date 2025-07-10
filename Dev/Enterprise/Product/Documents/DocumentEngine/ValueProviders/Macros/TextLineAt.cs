using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class TextLineAt : DBOrBOValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<TextLineAt({fieldname},{lineno})>",
				ResString.GetMultilingualString("5cc112d0-1bea-42cd-aa9a-f4b0ab271ac6", @"Returns the specified line from the multi line value in the field specified."),
				new List<(string example, object expectedResult)> { ((NoResString)"<TextLineAt(Header.MultiLineText, 4)>", " Line4                                                                             ") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			Match match = Regex.Match(macro);
			string fieldName = match.Groups[1].ToString();
			string lineNumberAsString = match.Groups[2].ToString();
			int lineNumber = 0;
			try
			{
				lineNumber = int.Parse(lineNumberAsString);
			}
			catch (FormatException e)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Line number was an invalid integer. Macro: {0}", macro), e);
			}

			if (report.Renderer == null)
			{
				throw new InvalidOperationException("Report.Renderer should not be null in GetReplacementCore.");
			}
			if (report.Renderer.CurrentAreaToProcess == null)
			{
				throw new InvalidOperationException("Report.Renderer.CurrentAreaToProcess should not be null in GetReplacementCore.");
			}

			object fieldValue = report.Renderer.CurrentAreaToProcess.GetColumnValue(report.Renderer.CurrentDataRow, fieldName);

			if (fieldValue is byte[] && ORtfTextUtil.IsRtf((byte[])fieldValue))
			{
				fieldValue = ORtfTextUtil.RtfToText((byte[])fieldValue);
			}

			if (fieldValue != null)
			{
				string[] lines = fieldValue.ToString().Split('\n');
				if (lineNumber - 1 < lines.Length)
				{
					fieldValue = lines[lineNumber - 1];
				}
				else
				{
					fieldValue = "";
				}
			}

			return fieldValue ?? string.Empty;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)TextLineAt(?:[\s]*)\((?:[\s]*)(?:[\s]*)([^\s]+)(?:[\s]*)(?:[\s]*),(?:[\s]*)([0123456789]+)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
