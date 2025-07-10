using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Macros.UtilityClasses;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class AddDurationToDate : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?:\s*)AddDurationToDate(?:\s*)\((?:\s*)(?<operands>((?:\s*)"".*""(?:\s*),(?:\s*))+(?:\s*)"".*""(?:\s*))\)(?:\s*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
		static readonly Regex operandRegex = new Regex(@"""[^""]*""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<AddDurationToDate({date},{span},{value})>",
	ResString.GetMultilingualString("5b4739cf-6b11-407b-b08d-966deb770d30", @"Add or subtract time from a date. Span can be any of the following time spans:
{0}, {1}, {2}, {3} or {4}.", "DAYS", "HOURS", "MINUTES", "SECONDS", "MILLISECONDS"),
	new List<(string example, object expectedResult)> { ("<AddDurationToDate(\"<JK_MasterBillIssueDate>\", \"DAYS\", \"1\")>", new ZDate(2004, 1, 20)) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = regex.Match(macro);
			var valueAsString = match.Groups["operands"].Value;
			var matches = operandRegex.Matches(valueAsString);

			if (matches.Count != 3)
			{
				ReportMacroError(report, Res.GetString("77fb090b-da1f-499f-a848-5b6419f1bfe0", "Wrong number of arguments, expected these: {0}", macro));
			}
			else if (int.TryParse(RemoveQuotes(matches[2].Value), out int val) && TryGetTimespan(RemoveQuotes(matches[1].Value), val, out TimeSpan timespan))
			{
				if (DateTime.TryParse(RemoveQuotes(matches[0].Value), out DateTime datetime))
				{
					return new ZDateTime(datetime.Add(timespan));
				}
			}
			else
			{
				ReportMacroError(report, Res.GetString("8905eb85-9a74-4e08-a367-6d943d5fc63d", "Invalid parameters provided: {0}", macro));
			}

			return ZDateTime.Empty;
		}

		string RemoveQuotes(string str)
		{
			return str.Replace("\"", "");
		}

		bool TryGetTimespan(string type, int val, out TimeSpan timespan)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals(type, TimeUnits.Days))
			{
				timespan = TimeSpan.FromDays(val);
			}
			else if (StringComparer.OrdinalIgnoreCase.Equals(type, TimeUnits.Hours))
			{
				timespan = TimeSpan.FromHours(val);
			}
			else if (StringComparer.OrdinalIgnoreCase.Equals(type, TimeUnits.Minutes))
			{
				timespan = TimeSpan.FromMinutes(val);
			}
			else if (StringComparer.OrdinalIgnoreCase.Equals(type, TimeUnits.Seconds))
			{
				timespan = TimeSpan.FromSeconds(val);
			}
			else if (StringComparer.OrdinalIgnoreCase.Equals(type, TimeUnits.Milliseconds))
			{
				timespan = TimeSpan.FromMilliseconds(val);
			}
			else
			{
				timespan = TimeSpan.Zero;
				return false;
			}
			return true;
		}
	}
}
