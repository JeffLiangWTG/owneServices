using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class ReportNameShort : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ReportNameShort>",
				ResString.GetMultilingualString("58b3652f-f5c8-4c5a-8e10-112c7877030f",
				@"Returns the Report Name from the link record linking the Menu Item and the Template on the 'Customize Documents' form, but with any domain specific prefixes like 'Consol' or 'Declaration' or 'Shipment' removed.
See also: {0}.", "ReportName"),
				new List<(string example, object expectedResult)> { ("<ReportNameShort>", (NoResString)"Cartage Advice") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (regexToFindMacroAnyWhereInString.IsMatch(report.Name))
			{
				var message = Res.GetString("3db1a612-384e-4e87-b70e-8b8ee2599e99", "The macro {0} cannot be used in the '{1}' field of the config area of the document.", "<ReportNameShort>", "Name=");
				report.ErrorManager.Add(new ReportProcessingError(message, ReportProcessingErrorSeverity.ErrorWithoutErrorReport));
				return string.Empty;
			}
			else
			{
				var cellAnalyzer = new ExcelCellAnalyzer(report.Name);
				var translatable = cellAnalyzer.GetTextToBeTranslated();
				if (translatable.Length > 0)
				{
					foreach (var item in translatable)
					{
						ResourceStringData stringData;
						if (report.Style == Report.Styles.Report)
						{
							stringData = Res._GetData(DocBuilderResourceStrings.ReportTitleAsmid, DocBuilderResourceStrings.ReportTitleKeyPrefix + item.TranslatableText, item.TranslatableText);
						}
						else
						{
							stringData = Res._GetData(DocBuilderResourceStrings.ReportNamesAsmid, DocBuilderResourceStrings.ReportNameKeyPrefix + item.TranslatableText, string.Empty);
						}
						item.TranslatableText = stringData != null && !string.IsNullOrEmpty(stringData.ShortCaption) ? stringData.ShortCaption : GetShortReportName(item.TranslatableText);
					}
				}
				var cellContentReplacer = new CellContentReplacer(report, cellAnalyzer.CellText);
				cellContentReplacer.ReplaceMacros();
				return cellContentReplacer.ContentAsString;
			}
		}

		public static string GetShortReportName(string reportName)
		{
			string[] words = reportName.Split(' ');
			if (words.Length > 1)
			{
				switch (words[0].ToUpperInvariant())
				{
					case "CONSOL":
					case "DECLARATION":
					case "SHIPMENT":
					case "CFS":
					case "CFSSHIPMENT":
					case "BOOKING":
					case "CONTAINERLEG":
					case "LOCAL":
					case "TRANSHIPMENT":
						var builder = new ZStringBuilder();
						for (int index = 1; index < words.Length; index++)
						{
							builder.Append(words[index]);
						}

						reportName = builder.ToStringWithDelimiterBetweenAppends(" ");
						break;
				}
			}
			return reportName;
		}

		public override Regex Regex
		{
			get { return fRegex; }
		}

		static readonly Regex fRegex = new Regex("^" + regexPattern + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly Regex regexToFindMacroAnyWhereInString = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		const string regexPattern = @"<(?:[\s]*)Report(?:[\s]*)Name(?:[\s]*)Short(?:[\s]*)>";
	}
}
