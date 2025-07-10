using System;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Web.Shared
{
	public static class WebDateTimeParser
	{
		#region Static

		public static bool IsValidCultureForDatesParsingAndDisplay(CultureInfo culture)
		{
			foreach (string abbreviatedMonth in culture.DateTimeFormat.AbbreviatedMonthNames)
			{
				if (!string.IsNullOrEmpty(abbreviatedMonth.Trim()))
				{
					if (abbreviatedMonth.Length != 3)
					{
						return false;
					}
				}
			}
			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Date time format string")]
		public static ZDateTime ParseDate(string dateString)
		{
			CultureInfo currentCulture = null;
			string dayString = string.Empty;
			string monthString = string.Empty;
			string yearString = string.Empty;
			try
			{
				if (!string.IsNullOrEmpty(WebUserCulture.DateTimeFormat.ShortDatePattern))
				{
					currentCulture = WebUserCulture;
				}
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				try
				{
					if (!string.IsNullOrEmpty(DefaultCulture.DateTimeFormat.ShortDatePattern))
					{
						currentCulture = DefaultCulture;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException()) { }
			}
			if (currentCulture != null)
			{
				bool isMonthFirstPattern = currentCulture.DateTimeFormat.ShortDatePattern.StartsWith("m", StringComparison.InvariantCultureIgnoreCase);
				bool isYearFirstPattern = currentCulture.DateTimeFormat.ShortDatePattern.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);
				for (int i = 0; i < 2; i++)
				{
					if (!IsValidCultureForDatesParsingAndDisplay(currentCulture))
					{
						currentCulture = DefaultCulture;
					}
					if (Regex.IsMatch(dateString, "^\\d{1,2}$"))
					{
						dayString = dateString;
					}
					else if (Regex.IsMatch(dateString, "^\\d{3,4}$"))
					{
						if (isMonthFirstPattern || isYearFirstPattern)
						{
							dayString = dateString.Substring(dateString.Length - 2);
							monthString = dateString.Substring(0, dateString.Length - 2);
						}
						else
						{
							dayString = dateString.Substring(0, dateString.Length - 2);
							monthString = dateString.Substring(dateString.Length - 2);
						}
					}
					else if (Regex.IsMatch(dateString, "^\\d{1,2}[-/.]\\d{2}$"))
					{
						if (isMonthFirstPattern || isYearFirstPattern)
						{
							dayString = dateString.Substring(dateString.Length - 2);
							monthString = dateString.Substring(0, dateString.Length - 3);
						}
						else
						{
							dayString = dateString.Substring(0, dateString.Length - 3);
							monthString = dateString.Substring(dateString.Length - 2);
						}
					}
					else if (Regex.IsMatch(dateString, "^\\d{5,6}$"))
					{
						if (isMonthFirstPattern)
						{
							monthString = dateString.Substring(0, dateString.Length - 4);
							dayString = dateString.Substring(dateString.Length - 4, 2);
							yearString = dateString.Substring(dateString.Length - 2);
						}
						else if (isYearFirstPattern)
						{
							yearString = dateString.Substring(0, dateString.Length - 4);
							monthString = dateString.Substring(dateString.Length - 4, 2);
							dayString = dateString.Substring(dateString.Length - 2);
						}
						else
						{
							dayString = dateString.Substring(0, dateString.Length - 4);
							monthString = dateString.Substring(dateString.Length - 4, 2);
							yearString = dateString.Substring(dateString.Length - 2);
						}
					}
					else if (Regex.IsMatch(dateString, "^\\d{1,2}[-/.]\\d{2}[-/.]\\d{2}$"))
					{
						if (isMonthFirstPattern)
						{
							monthString = dateString.Substring(0, 2);
							dayString = dateString.Substring(3, 2);
							yearString = dateString.Substring(dateString.Length - 2);
						}
						else if (isYearFirstPattern)
						{
							yearString = dateString.Substring(0, 2);
							monthString = dateString.Substring(3, 2);
							dayString = dateString.Substring(dateString.Length - 2);
						}
						else
						{
							dayString = dateString.Substring(0, 2);
							monthString = dateString.Substring(3, 2);
							yearString = dateString.Substring(dateString.Length - 2);
						}
					}

					else if (Regex.IsMatch(dateString, "^\\d{1,2}\\D{3}$"))
					{
						dayString = dateString.Substring(0, dateString.Length - 3);
						monthString = dateString.Substring(dateString.Length - 3);
					}
					else if (Regex.IsMatch(dateString, "^\\d{1,2}\\D{3}\\d{2}$"))
					{
						dayString = dateString.Substring(0, 2);
						monthString = dateString.Substring(2, 3);
						yearString = dateString.Substring(dateString.Length - 2);
					}
					else if (Regex.IsMatch(dateString, "^\\d{1,2}[-/.]\\D{3}$"))
					{
						dayString = dateString.Substring(0, dateString.Length - 4);
						monthString = dateString.Substring(dateString.Length - 3);
					}
					else if (Regex.IsMatch(dateString, "^\\d{1,2}[-/.]\\D{3}[-/.]\\d{2}$"))
					{
						dayString = dateString.Substring(0, 2);
						monthString = dateString.Substring(3, 3);
						yearString = dateString.Substring(dateString.Length - 2);
					}
					dayString = dayString.Trim();
					monthString = monthString.Trim();

					if (!string.IsNullOrEmpty(dayString))
					{
						int day = -1;
						int month = -1;
						int year = -1;
						if (int.TryParse(dayString, out day))
						{
							if (!string.IsNullOrEmpty(monthString))
							{
								if (!int.TryParse(monthString, out month))
								{
									month = MonthNumber(currentCulture, monthString);
									if (month == -1)
									{
										month = MonthNumber(DefaultCulture, monthString);
									}
								}
							}
							else
							{
								month = ZDateTime.Now.Month;
							}
							if (int.TryParse(yearString, out year))
							{
								if (yearString.Length < 4)
								{
									year += (year < 59 ? 2000 : 1900);
								}
							}
							else
							{
								year = ZDateTime.Now.Year;
							}
							if (month > 12 && day < 13)
							{
								var temp = month;
								month = day;
								day = temp;
							}
							try
							{
								return new ZDateTime(year, month, day);
							}
							catch (ZTypeValueException) { }
							catch (ArgumentOutOfRangeException) { }
							catch (ArgumentException) { }
						}
					}
					if (currentCulture == DefaultCulture)
					{
						break;
					}
					currentCulture = DefaultCulture;
				}
			}
			return ZDateTime.Invalid;
		}

		static int MonthNumber(CultureInfo culture, string abbreviatedMonthName)
		{
			if (!string.IsNullOrEmpty(abbreviatedMonthName.Trim()))
			{
				try
				{
					for (int i = 0; i < culture.DateTimeFormat.AbbreviatedMonthNames.Length; i++)
					{
						if (!string.IsNullOrEmpty(culture.DateTimeFormat.AbbreviatedMonthNames[i].Trim()))
						{
							if (culture.DateTimeFormat.AbbreviatedMonthNames[i].Equals(abbreviatedMonthName, StringComparison.InvariantCultureIgnoreCase))
							{
								return i + 1;
							}
						}
					}
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
			}
			return -1;
		}

		internal static CultureInfo WebUserCulture
		{
			get
			{
				CultureInfo result = DefaultCulture;
				try
				{
					result = WebEnvShared.ClientCulture;
				}
				catch (Exception e) when (!e.IsCriticalException()) { }
				return result;
			}
		}

		internal static CultureInfo DefaultCulture
		{
			get
			{
				return ObjectCache.CultureProvider.Culture;
			}
		}

		#endregion

	}
}
