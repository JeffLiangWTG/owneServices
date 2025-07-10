using System;
using System.Globalization;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	internal class ZTimeParser
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Used in parsing times; not an actual duration")]
		public ZTimeParser(int maximumHours)
		{
			MaximumHours = maximumHours;
		}
		#region Time Edit Helper

		public string GetTextFromTime(ZTime time, bool allowNegative)
		{
			return time.ToString();
		}

		public string GetTextFromTime(ZDateTime time, bool allowNegative)
		{
			return ZDateTimeGeneralHelper.GetTextFromTimeOffset(time, MaximumHours, allowNegative);
		}

		#endregion
		public ZDateTime GetDateTimeFromText(string value, bool allowNegative)
		{
			ZDateTime result;

			if (string.IsNullOrEmpty(value))
			{
				result = ZDateTime.Empty;
			}
			else
			{
				ZInt hour = -1;
				ZInt mins = -1;

				if (!GetHourAndMins(value, allowNegative, out hour, out mins))
				{
					result = ZDateTime.Invalid;
				}
				else
				{
					result = DateTimeFromHoursAndMins(hour, mins, value.StartsWith("-"));
				}
			}

			return result;
		}
		public ZTime GetTimeFromText(string value, bool allowNegative)
		{
			ZTime result;

			if (string.IsNullOrEmpty(value))
			{
				result = ZTime.Empty;
			}
			else
			{
				ZInt hour = -1;
				ZInt mins = -1;

				if (!GetHourAndMins(value, allowNegative, out hour, out mins))
				{
					result = ZTime.Invalid;
				}
				else
				{
					result = new ZTime(hour, mins);
					if (value.StartsWith("-"))
					{
						result = new ZTime(-result.ToTimeSpan());
					}
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Used in parsing times, not an actual duration")]
		protected ZDateTime DateTimeFromHoursAndMins(int hours, int mins, bool negative)
		{
			var result = new ZDateTime(ZDateTime.Today.Year, 1, 1);

			if (hours <= MaximumHours)
			{
				result = result.AddHours(negative ? -hours : hours).AddMinutes(negative ? -mins : mins);
			}

			return result;
		}

#pragma warning disable CW1050 // Use System.TimeSpan Type For A Duration
		protected const int MaximumMinutes = 59;
		protected const int MaximumMinutesLength = 2;
		protected int MaximumHours;

		protected int MaximumHoursLength
		{
			get { return MaximumHours.ToString().Length; }
		}
#pragma warning restore CW1050 // Use System.TimeSpan Type For A Duration

		bool GetHourAndMins(ZString text, bool allowNegative, out ZInt hour, out ZInt mins)
		{
			hour = -1;
			mins = -1;

			ZInt inputHour = -1;
			ZInt inputMins = -1;
			var split = text.Replace("-", "").Split(':');

			if (allowNegative || !text.Contains("-"))
			{
				if (split.Length == 1 && text.TrimStart('-').Length == MaximumHoursLength + 2) // ok, user has entered full number without colon
				{
					inputHour = SafeStringToZInt(text.TrimStart('-').Left(MaximumHoursLength));
					inputMins = SafeStringToZInt(text.Right(MaximumMinutesLength));
				}
				else if (split.Length == 2 && split[0].Length <= MaximumHoursLength + 1 && split[1].Length <= MaximumMinutesLength)
				{
					inputHour = 0;
					inputMins = 0;
					if (!split[0].IsEmpty)
					{
						inputHour = SafeStringToZInt(split[0]);
					}

					if (!split[1].IsEmpty)
					{
						inputMins = SafeStringToZInt(split[1]);
					}
				}
			}

			if (inputHour.IsInRange(0, MaximumHours) && inputMins.IsInRange(0, MaximumMinutes))
			{
				hour = inputHour;
				mins = inputMins;
				if (mins == 60)
				{
					mins = 0;
					hour++;
				}
			}
			return mins != -1;
		}

		static ZInt SafeStringToZInt(string value)
		{
			double junk = 0;
			var actualValue = -1;

			if (double.TryParse(value, NumberStyles.Number ^ NumberStyles.AllowDecimalPoint, null, out junk))
			{
				actualValue = Convert.ToInt32(value);
			}

			return new ZInt(actualValue);
		}
	}
}
