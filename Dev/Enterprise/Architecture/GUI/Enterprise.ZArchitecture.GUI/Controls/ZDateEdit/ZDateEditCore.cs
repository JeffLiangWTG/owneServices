using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	/// <summary>
	/// Helper class for DateEdit to re-use date time parsing logic
	/// Tested via ZDateEdit
	/// </summary>
	public class ZDateEditCore : IDateEditCore
	{
		public ZDateEditCore(IDateInputControl control)
		{
			dateControl = control;
		}

		public object ConvertTextToDateTime(string text)
		{
			object result = DBNull.Value;

			if (!string.IsNullOrEmpty(text))
			{
				var convertedValue = GetDateTimeAsObject(text, EnvProxy.Instance.CurrentCompany.DateTimeFormat);
				result = (convertedValue is DateTime) ? (DateTime)convertedValue : DateTime.MinValue;
			}

			return result;
		}

		public object GetDateTimeAsObject(string text, CountryDateTimeFormat format)
		{
			conversionStartIndex = 0;
			var relativeDateTriggers = new string[] { "T", "Y", "E", "S", "DE", "DS" };
			var trigger = relativeDateTriggers.FirstOrDefault(triggerItem => text.StartsWith(triggerItem, StringComparison.OrdinalIgnoreCase));

			return trigger != null ? GetRelativeDate(text, trigger) : GetAbsoluteDate(text, format);
		}

		public virtual string DateToString(object value, string inputText)
		{
			var currentDate = (value != null) ? (ZDateTime)value : ZDateTime.Empty;
			return currentDate.ToString(DateControl.FormatString);
		}

		public virtual ZDateTime StringToDate(string inputText)
		{
			ZDateTime dateTime;

			if (!TryParseDateTime(inputText, out dateTime, DateControl.FormatString))
			{
				if (IsAttemptedIsoDateEntry(inputText))
				{
					dateTime = ZDateTime.FromIsoWeekDateString(inputText);
				}
				else if (IsAttemptedMonthYearDateEntry(inputText))
				{
					dateTime = FromMonthYearDateString(inputText);
				}
				else
				{
					var result = ConvertTextToDateTime(inputText);
					var value = new ZDateTime(result);

					if (value.IsEmpty)
					{
						dateTime = ZDateTime.Empty;
					}
					else if (!value.IsValid || !value.IsValidSmallDateTime)
					{
						dateTime = ZDateTime.Invalid;
					}
					else
					{
						dateTime = value;
					}
				}
			}

			return dateTime;
		}

		bool TryParseDateTime(string inputText, out ZDateTime dateTime, string format)
		{
			dateTime = ZDateTime.Empty;

			var possibleFormats = new List<string>();

			possibleFormats.Add(format);
			if (format == DateTimeFormatStrings.LongTimeIncludingSecondsFormat)
			{
				possibleFormats.Add(DateTimeFormatStrings.LongTimeIncludingSecondsFormat.Replace(CultureInfo.CurrentUICulture.DateTimeFormat.TimeSeparator, ""));
				possibleFormats.Add(DateTimeFormatStrings.LongTimeFormat);
			}
			if (format == DateTimeFormatStrings.LongTimeFormat || format == DateTimeFormatStrings.LongTimeIncludingSecondsFormat)
			{
				possibleFormats.Add(DateTimeFormatStrings.LongTimeFormat.Replace(CultureInfo.CurrentUICulture.DateTimeFormat.TimeSeparator, ""));
				possibleFormats.Add(DateTimeFormatStrings.LongTimeFormatIncludingGMT);
				possibleFormats.Add(DateTimeFormatStrings.LongTimeFormatIncludingGMT.Replace(CultureInfo.CurrentUICulture.DateTimeFormat.TimeSeparator, ""));
				possibleFormats.Add(DateTimeFormatStrings.ShortDateFormat);

				possibleFormats.Add(GetSimplifiedLongTimeFormat());
			}

			for (var i = 0; i < possibleFormats.Count; i++)
			{
				if (ZDateTimeOffset.TryParseExact(inputText, out var dateTimeOffset, possibleFormats[i]))
				{
					dateTime = dateTimeOffset.ToZDateTime();
					return true;
				}
			}

			return false;
		}

		[ThreadSafe]
		static readonly ConcurrentDictionary<string, string> SimplifiedLongTimeFormatCache = new ConcurrentDictionary<string, string>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "These are all used as input parameters for regular expressions and date format specifiers.")]
		string GetSimplifiedLongTimeFormat()
		{
			return SimplifiedLongTimeFormatCache.GetOrAdd(
					DateTimeFormatStrings.LongTimeFormat,
					(longTimeFormat) =>
					{
						var pattern = "(?<!d)dd(?!d)|(?<!H)HH(?!H)|(?<!m)mm(?!m)|(?<!y)yy(?!y)";
						return Regex.Replace(longTimeFormat, pattern, m =>
						{
							return m.Value switch
							{
								"dd" => "d",
								"HH" => "H",
								"mm" => "m",
								"yy" => "y",
								_ => m.Value
							};
						});
					});
		}

		public ZDateTimeOffset StringToDateTimeOffset(string inputText)
		{
			TimeSpan? offset;
			ZDateTime dateTime = StringToDate(inputText);

			if (dateTime.IsEmpty)
			{
				return ZDateTimeOffset.Empty;
			}

			if (!TryParseOffset(inputText, out offset))
			{
				return new ZDateTimeOffset(dateTime);
			}

			if (dateTime == ZDateTime.Invalid || !IsValidOffset((TimeSpan)offset))
			{
				return ZDateTimeOffset.Invalid;
			}
			return new ZDateTimeOffset(dateTime, (TimeSpan)offset);
		}

		bool TryParseOffset(string inputText, out TimeSpan? offset)
		{
			var regex = new Regex((NoResString)@"GMT([+-])(\d{2}):(\d{2})", RegexOptions.IgnoreCase);
			var match = regex.Match(inputText);

			if (match.Success)
			{
				int sign = match.Groups[1].Value.Equals("+") ? 1 : -1;
				int hours = int.Parse(match.Groups[2].Value) * sign;
				int minutes = int.Parse(match.Groups[3].Value) * sign;
				offset = new TimeSpan(hours, minutes, 0);
				return true;
			}

			offset = null;
			return false;
		}

		bool IsValidOffset(TimeSpan offset)
		{
			var validOffsets = Enterprise.Environment.Env.Time.TimeZoneOffsets;
			return validOffsets.Contains(offset);
		}

		protected IDateInputControl DateControl
		{
			get { return dateControl; }
		}

		static int CurrentMonth
		{
			get { return EnvProxy.Instance.Time.CurrentLocalDate.Month; }
		}

		object GetAbsoluteDate(string text, CountryDateTimeFormat format)
		{
			object result;

			var hour = 0;
			var minute = 0;

			if (IsTimeOnly(text))
			{
				var timeText = GetTimePortion(text);
				hour = GetHour(timeText);
				minute = GetMinute(timeText);

				if (hour != -1 && minute != -1)
				{
					var today = EnvProxy.Instance.Time.CurrentLocalDateTime;
					result = new DateTime(today.Year, today.Month, today.Day, hour, minute, 0);
				}
				else
				{
					result = text;
				}
			}
			else
			{
				int day;
				int month;
				int year;

				switch (format)
				{
					case CountryDateTimeFormat.US:

						if (IsStartsFromLetter(text) || !IsContainsLetter(text, 3))
						{
							month = GetMonth(text);
							day = GetDay(text);
							year = GetYear(text);
						}
						else
						{
							day = GetDay(text);
							month = GetMonth(text);
							year = GetYear(text);
						}

						break;

					case CountryDateTimeFormat.Japan:
						year = GetYear(text);
						month = GetMonth(text);
						day = GetDay(text);
						break;

					default:
						day = GetDay(text);
						month = GetMonth(text);
						year = GetYear(text);
						break;
				}

				var validMonthAndDayOnly =
					format != CountryDateTimeFormat.Japan && conversionStartIndex == text.Length &&
					month != -1 && day != -1 && year == -1;

				if (dateControl.AutoCompleteYear && validMonthAndDayOnly)
				{
					year = AutoCompletedYearForMonth(month);
					if (day <= DateTime.DaysInMonth(year, month))
					{
						result = new DateTime(year, month, day);
					}
					else
					{
						result = text;
					}
				}
				else
				{
					if (conversionStartIndex < text.Length)
					{
						hour = GetHour(text);
						minute = GetMinute(text);
					}

					if ((year != -1 && month != -1 && day != -1 && hour != -1 && minute != -1) &&
						(year > 0 && year <= 9999 && day <= DateTime.DaysInMonth(year, month)))
					{
						result = new DateTime(year, month, day, hour, minute, 0);
					}
					else
					{
						result = text;
					}
				}
			}

			return result;
		}

		static bool IsStartsFromLetter(string text)
		{
			return IsContainsLetter(text, 0);
		}

		static bool IsContainsLetter(string text, int position)
		{
			return position < text.Length && char.IsLetter(text[position]);
		}

		int AutoCompletedYearForMonth(int month)
		{
			var result = EnvProxy.Instance.Time.CurrentLocalDate.Year;

			var currentMonthPlusThresholdExceedsDecember = (CurrentMonth + dateControl.AutoCompleteMonthThreshold > 12);
			if (month < CurrentMonth && currentMonthPlusThresholdExceedsDecember)
			{
				var monthsToMoveToNextYear = ((CurrentMonth + dateControl.AutoCompleteMonthThreshold) % 12);
				if (month <= monthsToMoveToNextYear)
				{
					result++;
				}
			}
			else
			{
				var currentMonthMinusThresholdIsBeforeJanuary = (CurrentMonth - dateControl.AutoCompleteMonthThreshold < 1);
				if (month > CurrentMonth && currentMonthMinusThresholdIsBeforeJanuary)
				{
					var monthsToMoveToPreviousNear = CurrentMonth - dateControl.AutoCompleteMonthThreshold;
					if (monthsToMoveToPreviousNear <= 0)
					{
						monthsToMoveToPreviousNear += 12;
					}

					if (month >= monthsToMoveToPreviousNear)
					{
						result--;
					}
				}
			}

			return result;
		}

		bool IsTimeOnly(string text)
		{
			var checkText = text.Trim().Replace(" ", "");
			var maxLength = Regex.IsMatch(checkText, @"(PM|P|A|AM)$", RegexOptions.IgnoreCase) ? 7 : 5; // HH:mm(AM or PM) or HH:mm
			var matchString = @"^([0-2])?[0-9]([\.:])"; // Regular Expression
			matchString += (dateControl.FormatString == DateTimeFormatStrings.ShortTimeFormat) ? "?" : "";      // To distinguish between a string '1201'=1 Dec for a date to '1201'=12:01 for a time
			matchString += @"[0-5][0-9](PM|P|A|AM)?$"; // Regular Expression

			return checkText.Length <= maxLength && Regex.IsMatch(checkText, matchString, RegexOptions.IgnoreCase);
		}

		static string GetTimePortion(string text)
		{
			var checkText = text.Trim().Replace(" ", "").ToUpper();
			var suffix = "";
			if (checkText.EndsWith("P") || checkText.EndsWith("PM"))
			{
				suffix = "PM";
			}
			else if (checkText.EndsWith("A") || checkText.EndsWith("AM"))
			{
				suffix = "AM";
			}

			var result = "";
			foreach (var c in checkText)
			{
				if (char.IsDigit(c))
				{
					result += c.ToString();
				}
			}

			if (result.Length == 3)
			{
				result = "0" + result; // Make it HHmm format
			}

			return result + suffix;
		}

		static bool IsAttemptedIsoDateEntry(ZString text)
		{
			return text.StartsWith("WC") || text.StartsWith("WE");
		}

		#region Parse Relative Dates

		object GetRelativeDate(string text, string trigger)
		{
			var offset = 0;

			if (text.Length > trigger.Length)
			{
				if (text[trigger.Length] != '+' && text[trigger.Length] != '-')
				{
					return text;
				}

				if (!int.TryParse(text.Substring(trigger.Length), NumberStyles.Number, null, out offset))
				{
					return text;
				}
			}

			if (trigger == "Y" && offset != 0)
			{
				return text;
			}

			var actions = new Dictionary<string, Func<object>>
			{
				{ "T", () => GetTodayWithoutSeconds().AddDays(offset) },
				{ "Y", () => GetTodayWithoutSeconds().AddDays(-1) },
				{ "E", () => WorkingDaysHelper.GetRelativeTime(offset, WorkingDaysHelper.TimeOfDay.End, WorkingDaysHelper.DataSourceOption.Staff) },
				{ "S", () => WorkingDaysHelper.GetRelativeTime(offset, WorkingDaysHelper.TimeOfDay.Start, WorkingDaysHelper.DataSourceOption.Staff) },
				{ "DE", () => WorkingDaysHelper.GetRelativeTime(offset, WorkingDaysHelper.TimeOfDay.End, WorkingDaysHelper.DataSourceOption.Department) },
				{ "DS", () => WorkingDaysHelper.GetRelativeTime(offset, WorkingDaysHelper.TimeOfDay.Start, WorkingDaysHelper.DataSourceOption.Department) },
			};

			if (!actions.TryGetValue(trigger, out var action))
			{
				return text;
			}

			return action();
		}

		DateTime GetTodayWithoutSeconds()
		{
			var today = EnvProxy.Instance.Time.CurrentLocalDateTime;

			return (dateControl.FormatString == DateTimeFormatStrings.ShortDateFormat)
				? new DateTime(today.Year, today.Month, today.Day)
				: new DateTime(today.Year, today.Month, today.Day, today.Hour, today.Minute, 0);
		}

		#endregion

		#region Parse Year

		int GetYear(string text)
		{
			if (conversionStartIndex < text.Length)
			{
				if (char.IsDigit(text[conversionStartIndex]))
				{
					if (IsNotDelimitedDate(text))
					{
						return GetYearFromNonDelimitedText(text);
					}

					var yearCandidate = "";
					while ((conversionStartIndex < text.Length) && (char.IsDigit(text[conversionStartIndex])))
					{
						yearCandidate += text[conversionStartIndex];
						conversionStartIndex++;
					}

					var yearAsInt = ParseInt(yearCandidate);
					if (yearAsInt != -1)
					{
						if (yearAsInt.ToString().Length <= 2)
						{
							yearAsInt = Get4DigitYearFrom2DigitYear(yearAsInt);
						}

						return yearAsInt;
					}
				}
				else if (!char.IsLetter(text[conversionStartIndex]))
				{
					conversionStartIndex++;
					return GetYear(text);
				}
			}

			return -1;
		}

		/// <summary>
		/// Converts a 2-digit date to a 4-digit date.
		/// </summary>
		protected virtual int Get4DigitYearFrom2DigitYear(int year)
		{
			return ZDateTime.Get4DigitYearFrom2DigitYear(year);
		}

		int GetYearFromNonDelimitedText(string text)
		{
			var date = GetNonDelimitedDate(text);

			if (IsValidDateLength(date))
			{
				string yearCandidate = null;
				if (date.Length == 5 || date.Length == 6 || date.Length == 7)
				{
					if ((date.Length == 5 && conversionStartIndex < 5) || (date.Length == 6 && conversionStartIndex == 5)) // handle 1 digit years (ie 2jul4 and 02jul4)
					{
						yearCandidate = date.Substring(conversionStartIndex, 1);
					}
					else
					{
						if (conversionStartIndex < date.Length && char.IsLetter(date[conversionStartIndex + 1]))
						{
							yearCandidate = date.Substring(conversionStartIndex, 1); // to handle 1 digit year in JP format (2jul09)
						}
						else if (conversionStartIndex < date.Length - 1)
						{
							yearCandidate = date.Substring(conversionStartIndex, 2);
						}
					}
				}
				else if (date.Length == 8 || date.Length == 9)
				{
					// get year from the current position to the position a non-digit is reached or a maximal of 4 digits
					var i = 0;
					for (i = 0; i < 4 && conversionStartIndex + i < date.Length; i++)
					{
						if (!char.IsDigit(date[conversionStartIndex + i]))
						{
							break;
						}
					}
					yearCandidate = date.Substring(conversionStartIndex, i);
				}

				if (IsYear(yearCandidate))
				{
					var yearAsInt = ParseInt(yearCandidate);
					if (yearAsInt != -1)
					{
						if (yearAsInt < 50)
						{
							yearAsInt += 2000;
						}
						else if (yearAsInt < 100)
						{
							yearAsInt += 1900;
						}

						conversionStartIndex += yearCandidate.Length;
						return yearAsInt;
					}
				}
			}

			return -1;
		}

		static bool IsValidDateLength(string date)
		{
			var validLetterLength = date.Length == 5 || date.Length == 6 || date.Length == 7 || date.Length == 9;
			var validDigitLength = date.Length == 6 || date.Length == 8;
			var hasLetters = Regex.IsMatch(date, "[a-zA-Z]");

			return hasLetters ? validLetterLength : validDigitLength;
		}

		static bool IsYear(string year)
		{
			return !string.IsNullOrEmpty(year) && ParseInt(year) != -1;
		}

		static bool IsNotDelimitedDate(string text)
		{
			var result = true;

			if (text.Length >= 5)
			{
				for (var i = 0; i < 5; i++)
				{
					result &= char.IsLetterOrDigit(text[i]);
					if (!result)
					{
						break;
					}
				}
			}

			return result;
		}

		static string GetNonDelimitedDate(string text)
		{
			var date = "";
			var letterCount = 0;
			foreach (var currChar in text)
			{
				if (char.IsDigit(currChar))
				{
					date += currChar;
				}
				else if (char.IsLetter(currChar))
				{
					if ((currChar.ToString().ToUpper() == "T") && (letterCount == 0 || letterCount == 3))
					{
						break;
					}

					letterCount++;
					date += currChar;
				}
				else
				{
					break;
				}
			}

			return (letterCount == 3 || letterCount == 0) ? date : "";
		}

		#endregion

		#region Parse Day

		int GetDay(string text)
		{
			var isDelimited = !IsNotDelimitedDate(text);

			if (conversionStartIndex < text.Length)
			{
				if (char.IsDigit(text[conversionStartIndex]))
				{
					string dayCandidate;
					if (isDelimited)
					// add logic for delimited date, in that case should use delimiter to figure out if the day part has been retrieved other than simply using the length
					{
						var m = Regex.Match(text.Substring(conversionStartIndex), @"[0-9]+");
						dayCandidate = m.Value;
						conversionStartIndex += dayCandidate.Length;
					}
					else
					{
						dayCandidate = text[conversionStartIndex].ToString();
						conversionStartIndex++;

						if ((conversionStartIndex < text.Length) && (char.IsDigit(text[conversionStartIndex])))
						{
							dayCandidate += text[conversionStartIndex].ToString();
							conversionStartIndex++;
						}
					}
					if (IsDay(dayCandidate))
					{
						return ParseInt(dayCandidate);
					}
				}
				else
				{
					conversionStartIndex++;
					return GetDay(text);
				}
			}

			return -1;
		}

		static bool IsDay(string day)
		{
			var dayAsInt = ParseInt(day);
			return ((dayAsInt >= 1) && (dayAsInt <= 31));
		}

		#endregion

		#region Parse Month

		int GetMonth(string text)
		{
			if (conversionStartIndex < text.Length)
			{
				if (char.IsLetter(text[conversionStartIndex]))
				{
					if (text.Length >= conversionStartIndex + 3)
					{
						var monthCandidate = text.Substring(conversionStartIndex, 3);
						if (IsMonth(monthCandidate))
						{
							conversionStartIndex += 3;
							return (Array.IndexOf(months, monthCandidate.ToUpper()) + 1);
						}
					}
				}
				else if (char.IsDigit(text[conversionStartIndex]))
				{
					if ((conversionStartIndex + 1 < text.Length) && char.IsDigit(text[conversionStartIndex + 1]))
					{
						var monthCandidate = text.Substring(conversionStartIndex, 2);
						if (IsMonth(monthCandidate))
						{
							conversionStartIndex += 2;
							return ParseInt(monthCandidate);
						}
					}
					else
					{
						var monthCandidate = text.Substring(conversionStartIndex, 1);
						if (IsMonth(monthCandidate))
						{
							conversionStartIndex++;
							return ParseInt(monthCandidate);
						}
					}
				}
				else
				{
					conversionStartIndex++;
					return GetMonth(text);
				}
			}

			return -1;
		}

		bool IsMonth(string month)
		{
			if (Array.IndexOf(months, month.ToUpper()) > -1)
			{
				return true;
			}

			var monthAsInt = ParseInt(month);
			return ((monthAsInt <= 12) && (monthAsInt >= 1));
		}

		readonly string[] months = { "JAN", "FEB", "MAR", "APR", "MAY", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC" };

		#endregion

		#region Parse Time

		int GetHour(string text)
		{
			if (conversionStartIndex + 1 < text.Length)
			{
				if (char.IsDigit(text[conversionStartIndex]))
				{
					var candidateHour = text[conversionStartIndex].ToString();
					conversionStartIndex++;

					if (char.IsDigit(text[conversionStartIndex]))
					{
						candidateHour += text[conversionStartIndex];
						conversionStartIndex++;
					}

					if (IsHour(candidateHour))
					{
						var hourAsInt = ParseInt(candidateHour);
						if ((text.EndsWith("PM") || text.EndsWith("P")) && hourAsInt < 12)
						{
							hourAsInt += 12;
						}

						return hourAsInt;
					}
				}
				else
				{
					conversionStartIndex++;
					return GetHour(text);
				}
			}

			return -1;
		}

		static bool IsHour(string hour)
		{
			var hourAsInt = ParseInt(hour);
			return ((hourAsInt >= 0) && (hourAsInt <= 23));
		}

		int GetMinute(string text)
		{
			if (conversionStartIndex < text.Length)
			{
				if (char.IsDigit(text[conversionStartIndex]))
				{
					var candidateMinute = text[conversionStartIndex].ToString();
					conversionStartIndex++;

					if ((conversionStartIndex < text.Length) && char.IsDigit(text[conversionStartIndex]))
					{
						candidateMinute += text[conversionStartIndex];
						conversionStartIndex++;
					}

					if (IsMinute(candidateMinute))
					{
						return ParseInt(candidateMinute);
					}
				}
				else
				{
					conversionStartIndex++;
					return GetMinute(text);
				}
			}

			return -1;
		}

		static bool IsMinute(string minute)
		{
			var minuteAsInt = ParseInt(minute);
			return ((minuteAsInt >= 0) && (minuteAsInt <= 59));
		}

		static int ParseInt(string intCandidate)
		{
			int result;
			int.TryParse(intCandidate, out result);
			return result;
		}

		#endregion

		#region Date from Month-Year String

		static bool IsAttemptedMonthYearDateEntry(ZString text)
		{
			return text.StartsWith("MC") || text.StartsWith("ME");
		}

		ZDateTime FromMonthYearDateString(ZString text)
		{
			var result = ZDateTime.Invalid;

			if (text.Length == 5 /* "MC104" */ || text.Length == 6 /* "MC1204" */)
			{
				var monthAndYear = text.SubstringSafe(2);

				if (monthAndYear.IsNumbersOnlyOrEmpty)
				{
					var yearString = text.SubstringSafe(text.Length - 2); // last 2 digits
					var monthString = text.Substring(2, text.Length - yearString.Length - 2); // inbetween "MC" and year

					var year = Get4DigitYearFrom2DigitYear(int.Parse(yearString));
					var month = int.Parse(monthString);

					if (month >= 1 && month <= 12)
					{
						var monthCommencing = text.StartsWith("MC");
						result = monthCommencing ? new ZDateTime(year, month, 1) : new ZDateTime(year, month, 1).AddMonths(1).AddSeconds(-1);
					}
				}
			}

			return result;
		}

		#endregion

		readonly IDateInputControl dateControl;
		int conversionStartIndex;
	}
}
