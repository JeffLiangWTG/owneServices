using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class CapitaliseFirstLetterOfWords : DBOrBOValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<CapitaliseFirstLetterOfWords({fieldname})>",
				ResString.GetMultilingualString("263e5a17-4d7a-4d76-856d-2c21b5221267", @"Capitalizes the first letter of each word and converts all other characters to lower case."),
				new List<(string example, object expectedResult)> { ((NoResString)"<CapitaliseFirstLetterOfWords(Job.Consignee)>", (NoResString)"Consignee Test Company                  ") });
		}

		#region GetReplacement
		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = Regex.Match(macro);
			var columnName = match.Groups[1].ToString();
			object fieldValue = null;

			if (report.Renderer.CurrentAreaToProcess != null)
			{
				fieldValue = report.Renderer.CurrentAreaToProcess.GetColumnValue(report.Renderer.CurrentDataRow, columnName);
			}

			if (fieldValue != null)
			{
				fieldValue = GetCapitalisedFirstLetterOfWords(fieldValue.ToString());
			}

			return fieldValue ?? string.Empty;
		}

		protected string GetCapitalisedFirstLetterOfWords(string value)
		{
			var result = value.Replace("\r\n", "\n").Replace("\n", "\r\n").ToLower(CultureInfo.CurrentCulture);
			result = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result);
			return result;
		}
		#endregion

		public override Regex Regex
		{
			get { return fRegex; }
		}
		static readonly Regex fRegex = new Regex(@"^<(?:[\s]*)CapitaliseFirstLetterOfWords(?:[\s]*)\((?:[\s]*)(?:[\s]*)([^> ()]+)(?:[\s]*)(?:[\s]*)(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
