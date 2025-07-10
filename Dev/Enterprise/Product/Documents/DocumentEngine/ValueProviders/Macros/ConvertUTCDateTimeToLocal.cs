using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class ConvertUTCDateTimeToLocal : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<ConvertUTCDateTimeToLocal('{DateValue}')>",
				ResString.GetMultilingualString("DA3A6817-6252-485B-B4A4-005866C6D58B", @"Converts a Universal Date Time to a Local Date Time and returns the value as a date object."),
				new List<(string example, object expectedResult)> {
					("<ConvertUTCDateTimeToLocal('08/08/2007 12:15:30')>", new DateTime(2007, 8, 8, 22, 15, 30)),
					("<ConvertUTCDateTimeToLocal('<ReportDate.CreationTimeUTC>')>", new DateTime(2007, 10, 23, 10, 30, 0)) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = regex.Match(macro);
			if (match.Success)
			{
				var value = match.Groups["Value"].Value;
				using (Culture.SetTemporarily(Culture.Default))
				{
					if (DateTime.TryParse(value, out var date))
					{
						return Env.Time.GetLocalTimeFromUtc(date);
					}
				}
			}
			return null;
		}

		protected override string ReplaceNestedMacro(string macro, IMacroTranslator macroTranslator, Passes currentPass)
		{
			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			using (Culture.SetTemporarily(Culture.Default))
			{
				return base.ReplaceNestedMacro(macro, macroTranslator, currentPass);
			}
		}

		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<[\s]*ConvertUTCDateTimeToLocal[\s]*\([\s]*'(?<Value>[^']*)'[\s]?\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
