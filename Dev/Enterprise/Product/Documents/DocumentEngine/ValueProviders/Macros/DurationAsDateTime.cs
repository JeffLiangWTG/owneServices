using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Types;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DurationAsDateTime : ValueProvider
	{
		public override Regex Regex => regex;

		static readonly Regex regex = new Regex(@"^<(?:[\s]*)DurationAsDateTime(?:[\s]*)\((?:[\s]*)""([^:]*)(:?)([^:]*)""(?:[\s]*)\)(?:[\s]*)>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter("<DurationAsDateTime({duration})>",
			ResString.GetMultilingualString("82E938E7-A085-42F9-80A7-85441279F9BD", "Takes a duration and returns a DateTime in the format required to set duration fields. Expected Input format: \"HHH:MM\" where MM < 60."),
			new List<(string example, object expectedResult)> { ("<DurationAsDateTime(\"2:45\")>", ZDateTime.DefaultDurationEpoch.AddHours(2).AddMinutes(45)) });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var matches = regex.Match(macro).Groups;

			if (matches.Count == 4)
			{
				string hourString = matches[1].Value;
				hourString = hourString.Length == 0 ? "0" : hourString;

				string minuteString = matches[3].Value;
				minuteString = minuteString.Length == 0 ? "0" : minuteString;

				if (matches[2].Value.Length == 1 && int.TryParse(hourString, out int hours) && int.TryParse(minuteString, out int minutes) && hours >= 0 && minutes >= 0 && hours < 1000 && minutes < 60)
				{
					return (ZDateTime)new TimeSpan(hours, minutes, 0);
				}
			}

			ReportMacroError(report, Res.GetString("83428B49-4144-4394-8839-DB1918168BB3", "Incorrect format, should be <DurationAsDateTime(\"HHH:MM\")>"));

			return ZDateTime.Empty;
		}
	}
}
