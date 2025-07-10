using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class DateTimeFormatStringsTest : TestCase
	{
		[FrequentlyFailing]
		public void TestDateTimeFormatStringsAreValidInAllLanguages()
		{
			var valid = new Regex(@"^[dfFgGhHKmMSstTyz:/%\\""\-\., 年月日]+$", RegexOptions.Compiled);
			CombineAssertions(() =>
				{
					foreach (var language in DataFile.GetAvailableLanguages())
					{
						//the culture for the language Dzongkha is not supported in windows version earlier than windows 10 and windows server 2016, so return default culture for those windows version
						//https://msdn.microsoft.com/en-us/library/cc233982.aspx
						if (language != SharedConstants.Languages.Dzongkha)
						{
							using (Res.TemporarilySwitchLanguage(language))
							using (Culture.SetTemporarily(Culture.GetCultureForLanguage(language)))
							{
								foreach (var dateTimeFormat in DateTimeFormatStrings.AllDateTimeFormats)
								{
									Assert(language + " date format string '" + dateTimeFormat + "' is invalid", valid.IsMatch(dateTimeFormat.ToString()));
									AssertNoExceptionThrown(language + " date format string '" + dateTimeFormat + "' is invalid", delegate
									{
										TimeFactory.Instance.Format(DateTime.Now, dateTimeFormat);
									});
								}
							}
						}
					}
				}
			);
		}

		public void TestLongDateFormatIncludingWeek()
		{
			var date = new DateTime(2015, 11, 22, 10, 20, 1);
			var resKey = ((ResourceString)DateTimeFormatStrings.LongDateFormatIncludingWeek).ResourceKey;

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified)))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "yyyy年M月d日 dddd"));

				AssertEquals("2015年11月22日 星期日", date.ToString(DateTimeFormatStrings.LongDateFormatIncludingWeek));
			}
		}

		public void TestChineseDatesAreNumericOnly()
		{
			var dateTimeFormatsToExcludeFromChecking = new MultilingualString[] { DateTimeFormatStrings.LongTimeFormatIncludingGMT, DateTimeFormatStrings.LongDateFormatIncludingWeek };
			var dateTimeFormatsToCheck = DateTimeFormatStrings.AllDateTimeFormats.Where(f => !dateTimeFormatsToExcludeFromChecking.Contains(f));
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified)))
			{
				foreach (var dateTimeFormat in dateTimeFormatsToCheck)
				{
					var dateString = DateTime.Now.ToString(dateTimeFormat.Replace("t", ""));           // Unit test of translatable date format
					Assert("Date format " + dateTimeFormat.GetUnresolvedString() + " in CHS should be numeric only, but was " + dateString, !dateString.Any(c => char.IsLetter(c)));
				}
			}

			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseTraditional))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Enterprise.Core.SharedConstants.Languages.ChineseTraditional)))
			{
				foreach (var dateTimeFormat in dateTimeFormatsToCheck)
				{
					var dateString = DateTime.Now.ToString(dateTimeFormat.Replace("t", ""));   // Unit test of translatable date format
					Assert("Date format " + dateTimeFormat.GetUnresolvedString() + " in CHT should be numeric only, but was " + dateString, !dateString.Any(c => char.IsLetter(c)));
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestHungarianDateFormats()
		{
			using (Res.TemporarilySwitchLanguage(Constants.Languages.Hungarian))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Constants.Languages.Hungarian)))
			{
				var date = new DateTime(2015, 5, 22, 10, 20, 1);
				CombineAssertions(() =>
				{
					AssertEquals("15. máj. 22", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd-MMM-yy")));
					AssertEquals("15. máj. 22 10:20", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd-MMM-yy HH:mm")));
					AssertEquals("15. máj. 22 10:20:01", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd-MMM-yy HH:mm:ss")));
					AssertEquals("15. 05. 22", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd-MM-yy")));
					AssertEquals("15. máj. 22 10:20 de.", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd-MMM-yy hh:mm tt")));
					AssertEquals("máj. 22 10:20", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd-MMM HH:mm")));
					AssertEquals("2015. máj. 22", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd-MMM-yyyy")));
					AssertEquals("máj. 22", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd-MMM")));
					AssertEquals("2015. 05. 22", date.ToString(DateTimeFormatStrings.GetLocalizedFormatString("dd/MM/yyyy")));

					foreach (var dateTimeFormat in DateTimeFormatStrings.AllDateTimeFormats)
					{
						Assert("Date format " + dateTimeFormat.GetUnresolvedString() + " in Hungarian not have any dashes", !dateTimeFormat.ToString().Contains("-"));
					}
				});
			}
		}
	}
}
