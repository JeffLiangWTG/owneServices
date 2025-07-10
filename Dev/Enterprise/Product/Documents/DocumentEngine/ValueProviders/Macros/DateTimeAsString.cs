using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.MacroValueProviders
{
	class DateTimeAsString : ValueProvider
	{
		protected override ValueProviderDocumenter GetDocumentation()
		{
			return new ValueProviderDocumenter(
				"<DateTimeAsString('{DateValue}', '{DateFormatString}' [,Calendar:{CalendarInfo}] [,{ConvertFromUtcToLocal}])>",
ResString.GetMultilingualString("3d5f3713-f7cf-45bc-b564-ef0bf945ce9c", @"Is used to format a date value using a C# style formatting string. (i.e: {0} etc.)
Most things that work when formatting an excel cell with a customized date format will work as a format string with this function too.

Pre-defined Date Format Strings:
 - {1}
 - {2}
 - {3}

Optional Parameters:
{4} is a Microsoft JScript statement that if is evaluated to True will convert the Date Value from Universal Time to Local Time.",
"dd-MMM-yy hh:mm:ss", "ShortDateFormat", "ShortTimeFormat", "LongTimeFormat", "Calendar:CalendarInfo", "{ConvertFromUtcToLocal}"),
				new List<(string example, object expectedResult)> {
					((NoResString)"<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat', 1==1)>", (NoResString)"08-Aug-07 21:15"),
					((NoResString)"<DateTimeAsString('08/08/2007 11:15:30', 'ShortDateFormat')>", "08-Aug-07"),
					((NoResString)"<DateTimeAsString('<Now>', 'LongTimeFormat')>", (NoResString)"23-Oct-18 10:00"),
					((NoResString)"<DateTimeAsString('19/04/2020 11:15:30', 'ShortDateFormat', 'Calendar:GregorianCalendar')>", "19-Apr-20"),
					((NoResString)"<DateTimeAsString('19 Oct 2004', 'yyyy, MM/dd hh:mm')>", "2004, 10/19 12:00"),
					((NoResString)"<DateTimeAsString('19 Oct 2004', 'yyyy, MM/dd hh:mm zzz')>", "2004, 10/19 12:00 +00:00"),
					((NoResString)"<DateTimeAsString('19 Oct 2004 11:15:30 +10:00', 'yyyy, MM/dd hh:mm zzz')>", "2004, 10/19 11:15 +10:00"),
					((NoResString)"<DateTimeAsString('<DateTimeStart>', 'dd-MMM-yy hh:mm:ss')>", (NoResString)"23-Oct-18 10:00:00") });
		}

		protected override object GetReplacementCore(string macro, Report report)
		{
			var match = regex.Match(macro);
			if (match.Success)
			{
				var value = match.Groups["Value"].Value;
				var format = GetFormat(match.Groups["Format"].Value);
				var calendarInfo = match.Groups["CalendarInfo"];
				DateTimeOffset dateTimeOffset;

				try
				{
					var isValidDate = false;
					using (Culture.SetTemporarily(Culture.Default))
					{
						isValidDate = DateTimeOffset.TryParse(value, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.AssumeUniversal, out dateTimeOffset);
					}
					if (isValidDate)
					{
						var utcToLocalMatch = match.Groups["ConvertToLocal"];
						if (utcToLocalMatch.Success && ExpressionEvaluator.Evaluate(utcToLocalMatch.Value, report.UseJsEvaluator))
						{
							dateTimeOffset = Env.Time.GetLocalTimeFromDateTimeOffset(dateTimeOffset);
						}

						if (calendarInfo.Success)
						{
							using (Culture.SetTemporarilyCalendar(Culture.GetCalendar(calendarInfo.Value)))
							{
								return Env.Time.Format(dateTimeOffset, format);
							}
						}
						return Env.Time.Format(dateTimeOffset, format);
					}
				}
				catch (ArgumentOutOfRangeException)
				{
					ReportMacroError(report, string.Format((NoResString)"({0}) is not supported by the culture info.", calendarInfo));
					return string.Empty;
				}
				catch (FormatException)
				{
					ReportMacroError(report, string.Format((NoResString)"({0}) is not a valid date format string.", format));
					return string.Empty;
				}
			}
			return string.Empty;
		}

		protected override string ReplaceNestedMacro(string macro, IMacroTranslator macroTranslator, Passes currentPass)
		{
			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			using (Culture.SetTemporarily(Culture.Default))
			using (Env.Time.SetTemporaryDateFormat((NoResString)"dd-MMM-yyyy HH:mm:ss"))
			{
				return base.ReplaceNestedMacro(macro, macroTranslator, currentPass);
			}
		}

		string GetFormat(string format)
		{
			if (format.Equals("ShortDateFormat", StringComparison.InvariantCultureIgnoreCase))
			{
				return DateTimeFormatStrings.ShortDateFormat.GetUnresolvedString();
			}
			if (format.Equals("ShortTimeFormat", StringComparison.InvariantCultureIgnoreCase))
			{
				return DateTimeFormatStrings.ShortTimeFormat.GetUnresolvedString();
			}
			if (format.Equals("LongTimeFormat", StringComparison.InvariantCultureIgnoreCase))
			{
				return DateTimeFormatStrings.LongTimeFormat.GetUnresolvedString();
			}

			return format;
		}

		public override Regex Regex
		{
			get { return regex; }
		}
		static readonly Regex regex = new Regex(@"^<[\s]*DateTimeAsString[\s]*\([\s]*'(?<Value>[^']*)'[\s]*,[\s]*'(?<Format>[^']*)'[\s]*(,[\s]*'?Calendar:(?<CalendarInfo>[^')>,]*)'?[\s]*)?(,[\s]*(?<ConvertToLocal>[^)]*)'?[\s]*)?\)[\s]*>$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
