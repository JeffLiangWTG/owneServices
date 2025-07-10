using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	internal class ReportName : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ReportName>",
				ResString.GetMultilingualString("515fbb6c-ed6b-4d2d-8985-612708b63695", @"Returns the localized Report Name from the link record linking the Menu Item and the Template on the 'Customize Documents' form.
See also: {0} and {1}.", "<MenuTitle>", "<ReportNameUntranslated>"),
				new List<(string example, object expectedResult)> { ("<ReportName>", (NoResString)"Cover Sheet") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			if (regexToFindMacroAnyWhereInString.IsMatch(report.Name))
			{
				var message = Res.GetString("3db1a612-384e-4e87-b70e-8b8ee2599e99", "The macro {0} cannot be used in the '{1}' field of the config area of the document.", "<ReportName>", "Name=");
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
						item.TranslatableText = stringData != null && !string.IsNullOrEmpty(stringData.Caption) ? stringData.Caption : item.TranslatableText;
					}
				}
				var cellContentReplacer = new CellContentReplacer(report, cellAnalyzer.CellText);
				cellContentReplacer.ReplaceMacros();
				return cellContentReplacer.ContentAsString;
			}
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex("^" + regexPattern + "$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly Regex regexToFindMacroAnyWhereInString = new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regular Expression")]
		const string regexPattern = @"<(?:[\s]*)Report(?:[\s]*)Name(?:[\s]*)>";
	}
}
