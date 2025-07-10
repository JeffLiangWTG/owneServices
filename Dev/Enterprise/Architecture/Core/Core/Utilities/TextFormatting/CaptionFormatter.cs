using System;
using System.Text.RegularExpressions;

namespace Enterprise.ZArchitecture.Core
{
	public static class CaptionFormatter
	{
		public static string WithoutHotKeyFormatting(string caption) => FindHotKeyRegex.Replace(caption, regexReplacement);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regex pattern")]
		const string regexPattern = "(?<amp>&)&|&(?!=&)";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Regex pattern")]
		const string regexReplacement = "${amp}";

		static Regex FindHotKeyRegex => findHotKeyRegex ?? (findHotKeyRegex = new Regex(regexPattern, RegexOptions.Compiled));
		[ThreadStatic]
		static Regex findHotKeyRegex;
	}
}
