using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.Shared;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.Web.GUI.Res;

namespace Enterprise.ZArchitecture.Web.ServerServices.Testing
{
	public class DateFormatterWebServiceMethodTest : WebServiceMethodTest<DateFormatterWebServiceMethod>
	{
		#region Test Cases

		public override void TestExecute()
		{
			AssertNotNull(TestMethod);
			string cachedRegistryValue = WebDataRegistry.Instance.DateFormat.Value;
			var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
			for (int i = 0; i < 2; i++)
			{
				if (i == 0)
				{
					WebDataRegistry.Instance.DateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WebRegistryDateFormats.Standard);
				}
				else
				{
					WebDataRegistry.Instance.DateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WebRegistryDateFormats.WebUserLocale);
				}
				foreach (CultureInfo testCulture in allCultures)
				{
					SetLanguage(testCulture.Name);
					var parametersAndResponseTokens = MethodParametersAndExpectedResponseTokens;
					foreach (string testMethodParameters in parametersAndResponseTokens.Keys)
					{
						string expectedResponse = parametersAndResponseTokens[testMethodParameters];
						string actualResponse = TestMethod.Execute(testMethodParameters);
						AssertEquals(testMethodParameters, expectedResponse, actualResponse);
					}
				}
			}
			WebDataRegistry.Instance.DateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, cachedRegistryValue);
		}

		[TestDate(2016, 10, 13)]
		public void TestExecute_OctoberKenya()
		{
			//TestExecute can fail in October for culture Kikuyu (Kenya)
			SetLanguage("ki-KE");

			AssertNotNull(TestMethod);
			for (int i = 0; i < 2; i++)
			{
				if (i == 0)
				{
					WebDataRegistry.Instance.DateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WebRegistryDateFormats.Standard);
				}
				else
				{
					WebDataRegistry.Instance.DateFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, WebRegistryDateFormats.WebUserLocale);
				}

				var parametersAndResponseTokens = MethodParametersAndExpectedResponseTokens;
				foreach (string testMethodParameters in parametersAndResponseTokens.Keys)
				{
					string expectedResponse = parametersAndResponseTokens[testMethodParameters];
					string actualResponse = TestMethod.Execute(testMethodParameters);
					AssertEquals(testMethodParameters, expectedResponse, actualResponse);
				}
			}
		}

		public void TestExecute_InvalidDate()
		{
			var parameters = new DateFormatterParameters
			{
				DateControlID = "TestID",
				DateFormatType = "Short",
				DateValue = "2/31",
				TimeControlID = "TimeID",
				TimeValue = string.Empty
			};

			var expectedResponse = new WebServiceResponse
			{
				new SetFocusResponseToken("TestID"),
				new ShowErrorResponseToken("Please enter valid date!")
			};

			var actualResponse = TestMethod.Execute(parameters.ToString());

			AssertEquals("Invalid date was not handled", expectedResponse.ToString(), actualResponse.ToString());
		}

		[ExpectNoExceptions]
		[TestDate(2016, 10, 13)]
		public void TestExecuteWithUnknownDateType_ShouldDefaultToShortDateFormat()
		{
			var parameters = new DateFormatterParameters
			{
				DateControlID = "TestID",
				DateFormatType = "Corrupted",
				DateValue = "2/28",
				TimeControlID = "TimeID",
				TimeValue = string.Empty
			};

			var expectedResponse = new WebServiceResponse
			{
				new UpdateValueResponseToken("TestID", WebDateTimeFormatter.GetFormattedDate(new ZDateTime(ZDateTime.Now.Year, 2, 28), ZDateTimePickerFormat.Short)),
				new UpdateValueResponseToken("TimeID", "00:00")
			};

			var actualResponse = TestMethod.Execute(parameters.ToString());
			AssertEquals(expectedResponse.ToString(), actualResponse.ToString());
		}

		public void TestExecute_ZDateTimeArgumentException()
		{
			var parameters = new DateFormatterParameters
			{
				DateControlID = "TestID",
				DateFormatType = "Short",
				DateValue = "9999",
				TimeControlID = "TimeID",
				TimeValue = string.Empty
			};

			var expectedResponse = new WebServiceResponse
			{
				new SetFocusResponseToken("TestID"),
				new ShowErrorResponseToken("Please enter valid date!")
			};

			var actualResponse = TestMethod.Execute(parameters.ToString());

			AssertEquals("Invalid date was not handled", expectedResponse.ToString(), actualResponse.ToString());
		}

		#endregion

		#region Implementation

		string GetExpectedMonthAbbreviation(int month, CultureInfo parsingCulture, CultureInfo formattingCulture)
		{
			var monthAbbreviation = formattingCulture.DateTimeFormat.AbbreviatedMonthNames[month - 1];
			if (parsingCulture.Name != formattingCulture.Name)
			{
				var parsingMonthAbbreviation = parsingCulture.DateTimeFormat.AbbreviatedMonthNames.FirstOrDefault(m => m.Equals(monthAbbreviation, StringComparison.InvariantCultureIgnoreCase));
				if (!string.IsNullOrEmpty(parsingMonthAbbreviation))
				{
					var monthIndex = Array.IndexOf(parsingCulture.DateTimeFormat.AbbreviatedMonthNames, parsingMonthAbbreviation);
					monthAbbreviation = formattingCulture.DateTimeFormat.AbbreviatedMonthNames[monthIndex];
				}
			}
			return monthAbbreviation;
		}

		protected override void SetMethodParametersAndExpectedResponseTokens(Dictionary<string, WebServiceResponse> setting)
		{
			DateFormatterParameters testParameters = new DateFormatterParameters();
			testParameters.DateControlID = "TestID";
			testParameters.DateFormatType = "Short";
			testParameters.DateValue = "";
			testParameters.TimeControlID = "TimeID";
			testParameters.TimeValue = "";
			WebServiceResponse expectedResponse = new WebServiceResponse();
			setting.Add(testParameters.ToString(), expectedResponse);

			var allCultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
			foreach (CultureInfo testCulture in allCultures)
			{
				if (HttpContext.Current != null &&
										 HttpContext.Current.Request != null &&
										 HttpContext.Current.Request.UserLanguages != null &&
										 HttpContext.Current.Request.UserLanguages.Length > 0
										 && HttpContext.Current.Request.UserLanguages[0] == testCulture.Name)
				{
					CultureInfo clientCulture = testCulture;
					try
					{
						clientCulture = CultureInfo.CreateSpecificCulture(testCulture.Name);
					}
					catch (Exception) { }
					CultureInfo parsingCulture = clientCulture;
					if (parsingCulture.IsNeutralCulture)
					{
						parsingCulture = DefaultCulture;
					}
					try
					{
						if (string.IsNullOrEmpty(parsingCulture.DateTimeFormat.ShortDatePattern))
						{
							parsingCulture = DefaultCulture;
						}
					}
					catch (Exception)
					{
						parsingCulture = DefaultCulture;
					}
					bool isMonthFirstPattern = parsingCulture.DateTimeFormat.ShortDatePattern.StartsWith("m", StringComparison.InvariantCultureIgnoreCase);
					bool isYearFirstPattern = parsingCulture.DateTimeFormat.ShortDatePattern.StartsWith("y", StringComparison.InvariantCultureIgnoreCase);

					try
					{
						if (!WebDateTimeParser.IsValidCultureForDatesParsingAndDisplay(parsingCulture))
						{
							parsingCulture = DefaultCulture;
						}
					}
					catch (Exception)
					{
						parsingCulture = DefaultCulture;
					}
					CultureInfo formattingCulture = parsingCulture;
					if (WebDataRegistry.Instance.DateFormat.Value == WebRegistryDateFormats.Standard)
					{
						formattingCulture = DefaultCulture;
					}

					int day = 2;
					int month = 5;
					int year = 10;
					string monthString = month.ToString("00");
					string dayString = day.ToString("00");
					string dateControlID = string.Format("TestID{0}", clientCulture.EnglishName);
					string expectedUpdateValue = string.Empty;

					expectedUpdateValue = string.Format("{0}-{1}-{2}", dayString, GetExpectedMonthAbbreviation(month, parsingCulture, formattingCulture), year);
					testParameters.DateControlID = dateControlID;
					testParameters.DateValue = string.Format("{0}-{1}-{2}", dayString, formattingCulture.DateTimeFormat.AbbreviatedMonthNames[month - 1], year);
					testParameters.TimeValue = "";
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					expectedResponse.Add(new UpdateValueResponseToken("TimeID", "00:00"));
					setting.Add(testParameters.ToString(), expectedResponse);

					testParameters.DateValue = string.Format("{0}-{1}-{2}", monthString, dayString, year);
					testParameters.TimeValue = "13:10";
					expectedResponse = new WebServiceResponse();
					if (isMonthFirstPattern)
					{
						expectedUpdateValue = string.Format("{0}-{1}-{2}", dayString, GetExpectedMonthAbbreviation(month, parsingCulture, formattingCulture), year);
					}
					else if (isYearFirstPattern)
					{
						expectedUpdateValue = string.Format("{0}-{1}-{2}", year, GetExpectedMonthAbbreviation(day, parsingCulture, formattingCulture), monthString);
					}
					else
					{
						expectedUpdateValue = string.Format("{0}-{1}-{2}", monthString, GetExpectedMonthAbbreviation(day, parsingCulture, formattingCulture), year);
					}

					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					testParameters.TimeControlID = "";
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					testParameters.DateValue = string.Format("{0}/{1}/{2}", monthString, dayString, year);
					testParameters.TimeControlID = "TimeID";
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					testParameters.DateValue = string.Format("{0}.{1}.{2}", monthString, dayString, year);
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					testParameters.DateValue = string.Format("{0}{1}{2}", monthString, dayString, year);
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					year = int.Parse(ZDateTime.Now.ToString("yy"));
					if (isMonthFirstPattern || isYearFirstPattern)
					{
						expectedUpdateValue = string.Format("{0}-{1}-{2}", dayString, GetExpectedMonthAbbreviation(month, parsingCulture, formattingCulture), year);
					}
					else
					{
						expectedUpdateValue = string.Format("{0}-{1}-{2}", monthString, GetExpectedMonthAbbreviation(day, parsingCulture, formattingCulture), year);
					}

					testParameters.DateValue = string.Format("{0}-{1}", monthString, dayString);
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					testParameters.DateValue = string.Format("{0}/{1}", monthString, dayString);
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					testParameters.DateValue = string.Format("{0}.{1}", monthString, dayString);
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					testParameters.DateValue = string.Format("{0}{1}", monthString, dayString);
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					month = ZDateTime.Now.Month;
					monthString = month.ToString("00");
					expectedUpdateValue = string.Format("{0}-{1}-{2}", dayString, formattingCulture.DateTimeFormat.AbbreviatedMonthNames[month - 1], year);

					testParameters.DateValue = string.Format("{0}", dayString);
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					day = ZDateTime.Now.Day;
					dayString = day.ToString("00");
					expectedUpdateValue = string.Format("{0}-{1}-{2}", dayString, formattingCulture.DateTimeFormat.AbbreviatedMonthNames[month - 1], year);
					testParameters.DateValue = "T";
					testParameters.TimeValue = string.Empty;
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					expectedResponse.Add(new UpdateValueResponseToken("TimeID", "00:00"));
					setting.Add(testParameters.ToString(), expectedResponse);

					ZDateTime yesterdayDate = ZDateTime.Now.AddDays(-1);
					day = yesterdayDate.Day;
					dayString = day.ToString("00");
					month = yesterdayDate.Month;
					monthString = month.ToString("00");
					year = int.Parse(yesterdayDate.ToString("yy"));
					expectedUpdateValue = string.Format("{0}-{1}-{2}", dayString, formattingCulture.DateTimeFormat.AbbreviatedMonthNames[month - 1], year);
					testParameters.DateValue = "Y";
					testParameters.TimeValue = "13:10";
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
					setting.Add(testParameters.ToString(), expectedResponse);

					for (int f = 1; f < 365; f += 10)
					{
						ZDateTime futureDate = ZDateTime.Now.AddDays(f);
						ZDateTime pastDate = ZDateTime.Now.AddDays(f * (-1));

						day = futureDate.Day;
						dayString = day.ToString("00");
						month = futureDate.Month;
						monthString = month.ToString("00");
						year = int.Parse(futureDate.ToString("yy"));
						expectedUpdateValue = string.Format("{0}-{1}-{2}", dayString, formattingCulture.DateTimeFormat.AbbreviatedMonthNames[month - 1], year);
						testParameters.DateValue = string.Format("T+{0}", f);
						expectedResponse = new WebServiceResponse();
						expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
						setting.Add(testParameters.ToString(), expectedResponse);

						day = pastDate.Day;
						dayString = day.ToString("00");
						month = pastDate.Month;
						monthString = month.ToString("00");
						year = int.Parse(pastDate.ToString("yy"));
						expectedUpdateValue = string.Format("{0}-{1}-{2}", dayString, formattingCulture.DateTimeFormat.AbbreviatedMonthNames[month - 1], year);
						testParameters.DateValue = string.Format("T-{0}", f);
						expectedResponse = new WebServiceResponse();
						expectedResponse.Add(new UpdateValueResponseToken(dateControlID, expectedUpdateValue));
						setting.Add(testParameters.ToString(), expectedResponse);
					}

					testParameters.DateValue = "Invalid";
					testParameters.TimeValue = string.Empty;
					expectedResponse = new WebServiceResponse();
					expectedResponse.Add(new SetFocusResponseToken(dateControlID));
					expectedResponse.Add(new ShowErrorResponseToken(Res.GetString("4603575B-50FE-4138-8BAB-52026440BFF9", "Please enter valid date!")));
					setting.Add(testParameters.ToString(), expectedResponse);
				}
			}
		}

		CultureInfo DefaultCulture
		{
			get
			{
				return ObjectCache.CultureProvider.Culture;
			}
		}

		void SetLanguage(string language)
		{
			if (HttpContext.Current.Request.UserLanguages != null && HttpContext.Current.Request.UserLanguages.Length > 0)
			{
				HttpContext.Current.Request.UserLanguages[0] = language;
			}
			else
			{
				DummyHttpApplication dummyApplication = (DummyHttpApplication)HttpContext.Current.ApplicationInstance;
				DummyWorkerRequest dummyWorkerRequest = dummyApplication.WorkerRequest;
				dummyWorkerRequest.ClearUserLanguages();
				dummyWorkerRequest.SetUserLanguagesSeparatedByComma(language);
			}
		}

		protected override string GetExpectedMethodName()
		{
			return "FormatDate";
		}

		protected override DateFormatterWebServiceMethod GetNewWebServiceMethod()
		{
			return new DateFormatterWebServiceMethod();
		}

		protected override void TestExecuteSetup()
		{
			base.TestExecuteSetup();
			SetLanguage("en-US");
		}

		#endregion
	}
}
