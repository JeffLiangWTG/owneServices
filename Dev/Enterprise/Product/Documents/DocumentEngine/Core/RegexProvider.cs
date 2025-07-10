using System;
using System.Text.RegularExpressions;

namespace Enterprise.DocumentEngine
{
	public static class RegexProvider
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded regex")]
		const string OutermostMacroRegexString =
			@"(?<!\\)< # Opening <
   (?>
        (\\.) # Backslash escapes anything
      |
        [^\\<>]+ # Any characters other than \, <, or >
      |
        (?<!\\)< (?<DEPTH>) # Opening < adds one to DEPTH counter
      |
        (?<!\\)> (?<-DEPTH>) # Closing > subtracts one from DEPTH counter
   )*

   (?(DEPTH)(?!)) # DEPTH must be zero (i.e. brackets must match)
(?<!\\)> # Closing >
";

		internal static readonly Regex SingleMacroOnlyRegex = new Regex(@"^(?<!\\)<([^<]+?)(?<!\\)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex InnermostMacrosRegex = new Regex(@"(?<!\\)<(\\<|\\>|[^<>])+(?<!\\)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex MacroRegexDouble = new Regex(@"(?<!\\)<<((?:\\<|\\>|[^<>])+)(?<!\\)>>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		// This regex was intended to match strings that were entirely a single macro, but due to the comment at the end of the OutermostMacroRegexString was ignoring the "$" at the end.
		internal static readonly Regex OutermostSingleMacroRegex = new Regex("^" + OutermostMacroRegexString, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
		internal static readonly Regex OutermostMacroRegex = new Regex(@OutermostMacroRegexString, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
		internal static readonly Regex TableNamePlusColumnNameWithOptionalTotalRegex = new Regex(@"^<(?:[\s]*)(?:Total )?(?:[\s]*)([^>\.]+)\.([^>\.]+)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex TotalMacroRegex = new Regex(@"^<(?:[\s]*)Total(?:[\s]+)(.+)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex TotalMacroRegexMatchingToFirstEndingBracket = new Regex(@"^<(?:[\s]*)Total(?:[\s]+)([^>^\s]+)(?:[\s]*)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex TotalMacroRegexAnyWhereInString = new Regex(@"<(?:[\s]*)Total(?:[\s]+)(.+)(?:[\s]*)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex SelectListMacroRegex = new Regex(@"<(?:[\s]*)([^>\.]+)\.selectlist(?:[\s]*)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex UserRepositoryMacroRegex = new Regex(@"<(?:[\s]*)UserRepository(?:[\s]*)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex HtmlNewLineTagMacroRegex = new Regex(@"<\s*br\s*/>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		internal static readonly Regex AccumulativeTotalMacroRegex = new Regex(@"^<[\s]*AccumulativeTotal(?<WithReset>WithReset)*[\s]+(?<FieldName>.*)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		internal static bool IsSingleMacro(string macro)
		{
			bool result = true;
			if (OutermostMacroRegex.Matches(macro).Count != 1)
			{
				result = false;
			}
			else
			{
				if (!string.IsNullOrEmpty(OutermostMacroRegex.Replace(macro, "")))
				{
					result = false;
				}
			}
			return result;
		}

		public static Regex ColumnHeadingDisplayLabelRegex
		{
			get { return columnHeadingDisplayLabelRegex ?? (columnHeadingDisplayLabelRegex = new Regex(@"\s*Display\s*Label\s*=\s*\""([^\""]+)\""\s*(:?[,]*)\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex columnHeadingDisplayLabelRegex;

		public static Regex ColumnHeadingDescriptionRegex
		{
			get { return columnHeadingDescriptionRegex ?? (columnHeadingDescriptionRegex = new Regex(@"\s*Description\s*=\s*\""([^\""]+)\""\s*(:?[,]*)\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex columnHeadingDescriptionRegex;

		public static Regex ColumnHeadingHeadingTextRegex
		{
			get { return columnHeadingHeadingTextRegex ?? (columnHeadingHeadingTextRegex = new Regex(@"\s*Heading\s*Text\s*=\s*\""([^\""]+)\""\s*(:?[,]*)\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex columnHeadingHeadingTextRegex;

		public static Regex ColumnHeadingTagNameTextRegex
		{
			get { return columnHeadingTagNameTextRegex ?? (columnHeadingTagNameTextRegex = new Regex(@"\s*Tag\s*Name\s*=\s*\""([^\""]+)\""\s*(:?[,]*)\s*", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex columnHeadingTagNameTextRegex;

		public static Regex CustomisedColumnRegex
		{
			get { return customisedColumnRegex ?? (customisedColumnRegex = new Regex(@"<CustomisedColumn\(\""*([^\""]+)\""*\)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex customisedColumnRegex;
	}
}
