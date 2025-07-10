using System;
using System.Globalization;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(DateTimeAsString))]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Baseline")]
	sealed class DateTimeAsStringTest : ValueProviderTest
	{
		public void TestTwoDigitYearMax()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
				$@"{{A}}-[#Config]
{{A}}-[DataContext=.DummyBODocSupportable]
{{A}}-[Name=Test]
{{A}}-[#ConfigurableSection:GEN, Test Date]
{{B}}-[<DateTimeAsString('<Z0_Date>', 'yyyy-MM-dd')>]
{{A}}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			var twoDigitYearMax = CultureInfo.CurrentCulture.Calendar.TwoDigitYearMax;
			dummy.Z0_Date = new ZDateTime(twoDigitYearMax, 1, 1);

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Test Date"));

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
				AssertMultilineASCIIEquals("Date should be formatted correctly.", "{B}-" + $"[{twoDigitYearMax}-01-01]", excelInterface.WorkSheets[0].ToString().ToUpper());
			}

			dummy.Z0_Date = new ZDateTime(twoDigitYearMax + 1, 1, 1);
			printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
				AssertMultilineASCIIEquals("Date should be formatted correctly.", "{B}-" + $"[{twoDigitYearMax + 1}-01-01]", excelInterface.WorkSheets[0].ToString().ToUpper());
			}
		}

		public void TestShortDateFormatInSomeOtherLanguage()
		{
			using (var englishCache = Res.UseMockData())
			using (var chineseCache = Res.GetLanguageInstance(Enterprise.Core.Constants.Languages.ChineseSimplified).UseMockData())
			{
				englishCache.Put(
					"DateTimeFormat|ShortDateFormat",
					new ResourceStringData("DateTimeFormat|ShortDateFormat", "dd-MMM-yy"));

				chineseCache.Put(
					"DateTimeFormat|ShortDateFormat",
					new ResourceStringData("DateTimeFormat|ShortDateFormat", "yyyy-MM-dd"));

				AssertEquals("19-Oct-04", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat')>", Report));

				using (Res.TemporarilySwitchLanguage(Enterprise.Core.Constants.Languages.ChineseSimplified))
				{
					AssertEquals("2004-10-19", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat')>", Report));
				}
			}
		}

		[TestUtcOffset(08, 0, 0)]
		public void TestConvertFromUtcToLocal()
		{
			Assert("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat', 1==1)>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat', 1==1)>", Passes.FirstPass));
			AssertEquals("08-Aug-07 19:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat', 1==1)>", Report));

			Assert("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat', 1==2)>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat', 1==2)>", Passes.FirstPass));
			AssertEquals("08-Aug-07 11:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat', 1==2)>", Report));
		}

		[TestUtcOffset(08, 0, 0)]
		public void TestConvertFromUtcToLocal_OffsetInput()
		{
			Assert("<DateTimeAsString('08/08/2007 11:15:30 +00:00', 'LongTimeFormat', 1==1)>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30 +00:00', 'LongTimeFormat', 1==1)>", Passes.FirstPass));
			AssertEquals("08-Aug-07 19:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30 +00:00', 'LongTimeFormat', 1==1)>", Report));

			Assert("<DateTimeAsString('08/08/2007 11:15:30 -02:00', 'LongTimeFormat', 1==1)>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30 -02:00', 'LongTimeFormat', 1==1)>", Passes.FirstPass));
			AssertEquals("08-Aug-07 21:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30 -02:00', 'LongTimeFormat', 1==1)>", Report));

			Assert("<DateTimeAsString('08/08/2007 11:15:30 +00:00', 'LongTimeFormat', 1==2)>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30 +00:00', 'LongTimeFormat', 1==2)>", Passes.FirstPass));
			AssertEquals("08-Aug-07 11:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30 +00:00', 'LongTimeFormat', 1==2)>", Report));

			Assert("<DateTimeAsString('08/08/2007 11:15:30 -02:00', 'LongTimeFormat', 1==2)>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30 -02:00', 'LongTimeFormat', 1==2)>", Passes.FirstPass));
			AssertEquals("08-Aug-07 11:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30 -02:00', 'LongTimeFormat', 1==2)>", Report));
		}

		public void TestShortDateFormat()
		{
			Assert("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat')>", Passes.FirstPass));
			AssertEquals("19-Oct-04", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat')>", Report));

			Assert("<DateTimeAsString('08/08/2007 11:15:30', 'ShortDateFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30', 'ShortDateFormat')>", Passes.FirstPass));
			AssertEquals("08-Aug-07", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30', 'ShortDateFormat')>", Report));
		}

		public void TestShortDateFormat_OffsetInput()
		{
			Assert("<DateTimeAsString('08/08/2007 20:15:30 -08:00', 'ShortDateFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 20:15:30 -08:00', 'ShortDateFormat')>", Passes.FirstPass));
			AssertEquals("08-Aug-07", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 20:15:30 -08:00', 'ShortDateFormat')>", Report));
		}

		public void TestShortTimeFormat()
		{
			Assert("<DateTimeAsString('19 Oct 2004', 'ShortTimeFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('19 Oct 2004', 'ShortTimeFormat')>", Passes.FirstPass));
			AssertEquals("00:00", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortTimeFormat')>", Report));

			Assert("<DateTimeAsString('08/08/2007 11:15:30', 'ShortTimeFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30', 'ShortTimeFormat')>", Passes.FirstPass));
			AssertEquals("11:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30', 'ShortTimeFormat')>", Report));
		}

		public void TestShortTimeFormat_OffsetInput()
		{
			Assert("<DateTimeAsString('08/08/2007 11:15:30 -08:00', 'ShortTimeFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30 -08:00', 'ShortTimeFormat')>", Passes.FirstPass));
			AssertEquals("11:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30 -08:00', 'ShortTimeFormat')>", Report));
		}

		public void TestLongTimeFormat()
		{
			Assert("<DateTimeAsString('19 Oct 2004', 'LongTimeFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('19 Oct 2004', 'LongTimeFormat')>", Passes.FirstPass));
			AssertEquals("19-Oct-04 00:00", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'LongTimeFormat')>", Report));

			Assert("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat')>", Passes.FirstPass));
			AssertEquals("08-Aug-07 11:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30', 'LongTimeFormat')>", Report));
		}

		public void TestLongTimeFormat_OffsetInput()
		{
			Assert("<DateTimeAsString('08/08/2007 11:15:30 +10:00', 'LongTimeFormat')>", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('08/08/2007 11:15:30 +10:00', 'LongTimeFormat')>", Passes.FirstPass));
			AssertEquals("08-Aug-07 11:15", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30 +10:00', 'LongTimeFormat')>", Report));
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString(AField)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<dattimeasstring>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   date time as string  >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   datetimeasstring somefield   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   DateTimeAsString  \t  (     'fld' , 'dd' )   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   DateTimeAsString('<AField>', '<ANotherField>')   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<   DateTimeAsString('<A Field>', '<ANother Field>')   >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("<DateTimeAsString('19 Oct 2004', 'yyyy, MM/dd hh:mm')>", Passes.FirstPass));
		}

		public void TestDateTimeCalendar()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.SaudiArabia))
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Enterprise.Core.Constants.Languages.Arabic)))
			using (var arabicCache = Res.GetLanguageInstance(Enterprise.Core.Constants.Languages.Arabic).UseMockData())
			{
				AssertEquals("Default is Gregorian", "19-أكتوبر-04", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat')>", Report));
				AssertEquals("06-رمضان-25", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat', 'Calendar:HijriCalendar', 1==1)>", Report));
				AssertEquals("19-أكتوبر-04", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat', 'Calendar:GregorianCalendar')>", Report));
				AssertEquals("19-أكتوبر-04", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat', 'Calendar:GregorianCalendar', 1==2)>", Report));
				AssertEquals("Return to default Gregorian", "10-أكتوبر-18", ValueProviderToTest.GetReplacement("<DateTimeAsString('10 Oct 2018', 'ShortDateFormat')>", Report));
			}
		}

		public void TestDateTimeReportIfCultureInfoDoesNotSupportCalendar()
		{
			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'ShortDateFormat', 'Calendar:HijriCalendar', 1==1)>", Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in DateTimeAsString Macro: (HijriCalendar) is not supported by the culture info.]",
	Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestReplacement()
		{
			AssertEquals("2004-10-19", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'yyyy-MM-dd')>", Report));
			AssertEquals("19-Oct-04", ValueProviderToTest.GetReplacement("<DateTimeAsString('19 Oct 2004', 'dd-MMM-yy')>", Report));
			AssertEquals("08-Aug-07 11:15 AM", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30', 'dd-MMM-yy hh:mm tt')>", Report));
			AssertEquals("08-Aug-07 11:15 +10:00", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30 +10:00', 'dd-MMM-yy hh:mm zzz')>", Report));
			AssertEquals("08-Aug-07 11:15 -02:00", ValueProviderToTest.GetReplacement("<DateTimeAsString('08/08/2007 11:15:30 -02:00', 'dd-MMM-yy hh:mm zzz')>", Report));
		}

		public void TestStringFormat()
		{
			DateTime date = DateTime.UtcNow;
			var formatError = "z";

			AssertEquals(string.Empty, ValueProviderToTest.GetReplacement(string.Format("<DateTimeAsString('19 Oct 2004', '{0}')>", formatError), Report));
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in DateTimeAsString Macro: (z) is not a valid date format string.]",
				Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));

			Report.ErrorManager.ClearErrors();
		}

		public void TestReplacementIsLocalized()
		{
			AssertEquals("25-Jan-11 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-01-25 12:27', 'dd-MMM-yy HH:mm')>", Report));
			AssertEquals("25-Jan-11 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-01-25 12:27', 'LongTimeFormat')>", Report));
			AssertEquals("25-Jan-11", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-01-25 12:27', 'dd-MMM-yy')>", Report));
			AssertEquals("25-Jan-11", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-01-25 12:27', 'ShortDateFormat')>", Report));

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var temporaryCulture = new TemporaryValueSetter<CultureInfo>((culture) => Culture.Set(culture), Culture.Current))
			{
				temporaryCulture.Set(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.ChineseSimplified));
				AssertEquals("2011-5-25 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'dd-MMM-yy HH:mm')>", Report));
				AssertEquals("2011-5-25 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'LongTimeFormat')>", Report));
				AssertEquals("2011-5-25", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'dd-MMM-yy')>", Report));
				AssertEquals("2011-5-25", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'ShortDateFormat')>", Report));
				AssertEquals("2012-10-3 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2012-10-03 12:27', 'dd-MMM-yy HH:mm')>", Report));
				AssertEquals("2012-10-3 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2012-10-03 12:27', 'LongTimeFormat')>", Report));
				AssertEquals("2012-10-3", ValueProviderToTest.GetReplacement("<DateTimeAsString('2012-10-03 12:27', 'dd-MMM-yy')>", Report));
				AssertEquals("2012-10-3", ValueProviderToTest.GetReplacement("<DateTimeAsString('2012-10-03 12:27', 'ShortDateFormat')>", Report));
				AssertEquals("2011-5-25 12:27 下午", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'dd-MMM-yy hh:mm tt')>", Report));
				AssertEquals("2011-5-25", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'dd-MM-yy')>", Report));
				AssertEquals("5-25 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'dd-MMM HH:mm')>", Report));
			}

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Japanese))
			using (var temporaryCulture = new TemporaryValueSetter<CultureInfo>((culture) => Culture.Set(culture), Culture.Current))
			{
				temporaryCulture.Set(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.Japanese));
				AssertEquals("2011-6-23 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-06-23 12:27', 'dd-MMM-yy HH:mm')>", Report));
				AssertEquals("2011-6-23 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-06-23 12:27', 'LongTimeFormat')>", Report));
				AssertEquals("2011-6-23", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-06-23 12:27', 'dd-MMM-yy')>", Report));
				AssertEquals("2011-6-23", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-06-23 12:27', 'ShortDateFormat')>", Report));
				AssertEquals("2011-5-25 12:27 午後", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'dd-MMM-yy hh:mm tt')>", Report));
			}

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			using (var temporaryCulture = new TemporaryValueSetter<CultureInfo>((culture) => Culture.Set(culture), Culture.Current))
			{
				temporaryCulture.Set(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.German));
				AssertEquals("25-Mai-11 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'dd-MMM-yy HH:mm')>", Report));
				AssertEquals("25-Mai-11 12:27", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'LongTimeFormat')>", Report));
				AssertEquals("25-Mai-11", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'dd-MMM-yy')>", Report));
				AssertEquals("25-Mai-11", ValueProviderToTest.GetReplacement("<DateTimeAsString('2011-05-25 12:27', 'ShortDateFormat')>", Report));
			}

			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.Hungarian))
			using (var temporaryCulture = new TemporaryValueSetter<CultureInfo>((culture) => Culture.Set(culture), Culture.Current))
			{
				temporaryCulture.Set(Culture.GetCultureForLanguage(Core.Constants.Languages.Hungarian));
				AssertEquals("2015. 05. 22", ValueProviderToTest.GetReplacement("<DateTimeAsString('2015-05-22 12:27', 'yyyy-MM-dd')>", Report));
			}
		}

		public void TestFormatStringsAreValidInAllLangauges()
		{
			string[] acceptedShortDateFormats = new string[] { "dd-MMM-yy", "yy-MMM-dd", "yyyy-MM-dd", "yyyy-M-d", "yyyy年M月d日", "yy. MMM dd" };
			string[] acceptedShortTimeFormats = new string[] { "HH:mm" };
			string[] acceptedLongTimeFormats = new string[] { "dd-MMM-yy HH:mm", "yy-MMM-dd HH:mm", "yyyy-MM-dd HH:mm", "yyyy-M-d HH:mm", "yyyy年M月d日 HH:mm", "yy. MMM dd HH:mm" };

			string docLabelKeyShortDateFormat = DocBuilderResourceStrings.GetKey(null, "dd-MMM-yy");
			string docLabelKeyShortTimeFormat = DocBuilderResourceStrings.GetKey(null, "HH:mm");
			string docLabelKeyLongtTimeFormat = DocBuilderResourceStrings.GetKey(null, "dd-MMM-yy HH:mm");

			CombineAssertions(delegate
			{
				foreach (var language in DataFile.GetAvailableLanguages())
				{
					using (Res.TemporarilySwitchLanguage(language))
					{
						AssertNotNull(string.Format("Invalid {0} ShortDateFormat string '{1}'", language, DateTimeFormatStrings.ShortDateFormat), Array.Find(acceptedShortDateFormats, item => item == DateTimeFormatStrings.ShortDateFormat));
						AssertNotNull(string.Format("Invalid {0} ShortTimeFormat string '{1}'", language, DateTimeFormatStrings.ShortTimeFormat), Array.Find(acceptedShortTimeFormats, item => item == DateTimeFormatStrings.ShortTimeFormat));
						AssertNotNull(string.Format("Invalid {0} LongTimeFormat string '{1}'", language, DateTimeFormatStrings.LongTimeFormat), Array.Find(acceptedLongTimeFormats, item => item == DateTimeFormatStrings.LongTimeFormat));
						AssertNotNull(string.Format("Invalid {0} DocLabel ShortDateFormat string '{1}'", language, "dd-MMM-yy"), Array.Find(acceptedShortDateFormats, item => item == "dd-MMM-yy"));
						AssertNotNull(string.Format("Invalid {0} DocLabel ShortTimeFormat string '{1}'", language, "HH:mm"), Array.Find(acceptedShortTimeFormats, item => item == "HH:mm"));
						AssertNotNull(string.Format("Invalid {0} DocLabel LongTimeFormat string '{1}'", language, "dd-MMM-yy HH:mm"), Array.Find(acceptedLongTimeFormats, item => item == "dd-MMM-yy HH:mm"));
					}
				}
			});
		}

		[TestDate(2018, 10, 23)]
		public void TestDateInLithuanian()
		{
			AssertDateFormatCorrectInSpecificLanguage(Core.SharedConstants.Languages.Lithuanian, "yyyy.MM.dd", "2018.10.23");
		}

		[TestDate(2018, 10, 23)]
		public void TestDateInPolish()
		{
			AssertDateFormatCorrectInSpecificLanguage(Core.SharedConstants.Languages.Polish, "dd-MMM-yy", "23-PAŹ-18");
		}

		[TestDate(2018, 10, 23)]
		public void TestDateInArabic()
		{
			AssertDateFormatCorrectInSpecificLanguage(Core.SharedConstants.Languages.Arabic, "dd-MMM-yy", "‏23-أكتوبر-18");
		}

		[TestDate(2018, 10, 23)]
		public void TestDateWithIfExpression()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
$@"{{A}}-[#Config]
{{A}}-[DataContext=.DummyBODocSupportable]
{{A}}-[Name=Test]
{{A}}-[#ConfigurableSection:GEN, Test Date]
{{B}}-[<DateTimeAsString('<Now>', '<If(""<Z0_VarCharMax>""==""SEA"", ""dd-MMM-yy"", ""yyyy-MM-dd"")>')>]
{{A}}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			dummy.Z0_VarCharMax = "SEA";

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Test Date"));

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
				AssertMultilineASCIIEquals("Date should be formatted correctly.", "{B}-[23-OCT-18]", excelInterface.WorkSheets[0].ToString().ToUpper());
			}

			dummy.Z0_VarCharMax = "AIR";
			printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
				AssertMultilineASCIIEquals("Date should be formatted correctly.", "{B}-[2018-10-23]", excelInterface.WorkSheets[0].ToString().ToUpper());
			}
		}

		void AssertDateFormatCorrectInSpecificLanguage(string language, string dateFormat, string expectedResult)
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
$@"{{A}}-[#Config]
{{A}}-[DataContext=.DummyBODocSupportable]
{{A}}-[Name=Test]
{{A}}-[#ConfigurableSection:GEN, Test Date]
{{B}}-[<DateTimeAsString('<Now>', '{dateFormat}')>]
{{A}}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Test Date"));

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand, language);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);
				AssertMultilineASCIIEquals("Date should be formatted correctly.", $"{{B}}-[{expectedResult}]", excelInterface.WorkSheets[0].ToString().ToUpper());
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new DateTimeAsString();
		}

		[TestDate(2018, 10, 23)]
		[TestUtcOffset(10, 0, 0)]
		public override void TestDocumentation()
		{
			Report.SetStartTime();
			base.TestDocumentation();
		}
	}
}
