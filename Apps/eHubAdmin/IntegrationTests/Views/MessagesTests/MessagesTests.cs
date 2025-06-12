using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CargoWise.eHub.Selenium.IntegrationTests.Core;
using eServices.eHubAdmin.IntegrationTests.Attributes;
using HtmlAgilityPack;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using TimeZoneConverter;

namespace eServices.eHubAdmin.IntegrationTests
{
	[TestFixture]
	[WithEHubAdminService]
	public class MessagesTests : SeleniumTestBase
	{
		protected override string TestDataLocation => ".Views.MessagesTests.Data.";
		protected override string TestDataSchemaLocation => ".TestBase.Schemas.";

		protected override void SetUpField()
		{
			LinkPaths = new[]
			{
				"/Content/bootstrap-theme.css",
				"/Content/bootstrap-toggle.min.css",
				"/Content/awesome-bootstrap-checkbox.css",
				"/Content/site.css",
				"/Content/themes/base/core.css",
				"/Content/themes/base/resizable.css",
				"/Content/themes/base/selectable.css",
				"/Content/themes/base/accordion.css",
				"/Content/themes/base/autocomplete.css",
				"/Content/themes/base/button.css",
				"/Content/themes/base/dialog.css",
				"/Content/themes/base/slider.css",
				"/Content/themes/base/tabs.css",
				"/Content/themes/base/datepicker.css",
				"/Content/themes/base/progressbar.css",
				"/Content/themes/base/theme.css"
			};
			PreservedNodesXpaths = new[]
			{
				"//div[@id='list']",
				"//div[@id='inbox-message']"
			};
			ReferencePath = "../../../../eHubAdmin";
			SkipComparingAttributes = new Dictionary<string, string>
			{
				{"html", "class"}
			};
			base.SetUpField();
		}

		[Test]
		[Explicit]
		public void Test_Messages()
		{
			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("Messages");
			Driver.Url = endPoint.AbsoluteUri;

			// Transactions
			SearchMessage("MessageTrackingID", "1db032ca-d72e-47ca-8e2c-55f19a8c5ae2", true);
			WaitUntilFindElement(By.XPath("//a[@href='/Messages/Details?EI_PK=dc1f0e01-6907-489b-9df8-6811b8ee370e&Ref=637450560000000000']"));

			var expectedDocument = ReadEmbeddedHtml("Views.MessagesTests.ExpectedPage.Messages_Transactions_FinalPage.html");
			ReplaceLocalTime(expectedDocument, "received-local-time", new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			ReplaceLocalTime(expectedDocument, "delivered-local-time", new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc));
			ReplaceTimeZoneInTableHeader(expectedDocument);

			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			// Accordion
			var accordionButton = Driver.FindElement(By.Id("accordionButton"));
			accordionButton.Click();
			WaitUntilFindElement(By.ClassName("accordionView"));
			Assert.That(Driver.FindElements(By.ClassName("accordionView")), Is.Not.Empty);
			WaitUntilFindElement(By.ClassName("message-details"));
			Assert.That(Driver.FindElements(By.Id("inbox-message")), Is.Not.Empty);
			Assert.That(Driver.FindElements(By.Id("outbox-message")), Is.Not.Empty);
			accordionButton.Click();
			Wait.Until(Driver => Driver.FindElements(By.Id("list-loader-accordion")).Count == 0);

			// Archive
			SearchMessage("MessageTrackingID", "26ce6e6e-d58e-488c-9606-c531400486d6", false);
			WaitUntilFindElement(By.XPath("//a[@href='/Messages/Details?AM_PK=9134b76a-233a-4e8d-9482-54afe0f7a5d9']"));

			expectedDocument = ReadEmbeddedHtml("Views.MessagesTests.ExpectedPage.Messages_Archive_FinalPage.html");
			ReplaceLocalTime(expectedDocument, "received-local-time", new DateTime(2020, 12, 31, 0, 0, 0, DateTimeKind.Utc));
			ReplaceLocalTime(expectedDocument, "delivered-local-time", new DateTime(2020, 12, 31, 0, 0, 10, DateTimeKind.Utc));
			ReplaceTimeZoneInTableHeader(expectedDocument);

			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			// Archive Details
			Driver.FindElement(By.XPath("//a[@href='/Messages/Details?AM_PK=9134b76a-233a-4e8d-9482-54afe0f7a5d9']")).Click();
			while (Driver.WindowHandles.Count < 2)
			{
				Task.Delay(200).Wait();
			}
			Driver.Close();
			Driver.SwitchTo().Window(Driver.WindowHandles[0]);
			WaitUntilFindElement(By.ClassName("received-local-time"));

			expectedDocument = ReadEmbeddedHtml("Views.MessagesTests.ExpectedPage.Messages_Archive_DetailsPage.html");
			ReplaceLocalTime(expectedDocument, "received-local-time", new DateTime(2020, 12, 31, 0, 0, 0, DateTimeKind.Utc), true);

			CompareHtmlDocuments(expectedDocument, SnapShotPage());
		}
		[Test]
		[Explicit]
		public void Test_InitialSubmitQuery()
		{
			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("Messages");
			Driver.Url = endPoint.AbsoluteUri;

			WaitUntilFindElement(By.XPath("//input[@type='submit' and @value='Submit']"));
			var submitButton = Driver.FindElement(By.XPath("//input[@type='submit' and @value='Submit']"));
			submitButton.Click();
			Task.Delay(200).Wait();

			WaitUntilFindElement(By.XPath("//div[@id='collapseQuery' and @class='collapse']"));
			Assert.That(Driver.Url, Contains.Substring("Role=Any"));
		}

		[Test]
		[Explicit]
		public void Test_LocalTimeZoneName()
		{
			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("Messages");
			Driver.Url = endPoint.AbsoluteUri;
			WaitUntilFindElement(By.XPath("//th[@id='received-local-time-th']"));
			var websiteDisplayedTimeZone = Driver.FindElement(By.XPath("//th[@id='received-local-time-th']")).Text;

			var jsExecutor = (IJavaScriptExecutor)Driver;
			var timeZoneIdIANA = (string)jsExecutor.ExecuteScript("return Intl.DateTimeFormat().resolvedOptions().timeZone;");
			var FormatedTimeZoneIdIANA = "Received (" + timeZoneIdIANA + ")";

			Assert.That(websiteDisplayedTimeZone, Is.EqualTo(FormatedTimeZoneIdIANA));
		}

		[TestCase("A3B1911C-CE33-4127-8EEC-FE4223F80D8B", "EDIFACT", "Details")]
		[TestCase("A7F7870A-A05F-4216-9D44-FC47A9B71B2C", "X12", "Details")]
		[TestCase("A7F7870A-A05F-4216-9D44-FC47A9B72B3C", "EDIFACT", "Details")]
		[TestCase("C7F7870A-A05F-4216-9D44-FC47A9B71B2C", "X12", "Details")]
		[TestCase("A3B1911C-CE33-4127-8EEC-FE4223F80D8B", "EDIFACT", "Messages")]
		[TestCase("A7F7870A-A05F-4216-9D44-FC47A9B71B2C", "X12", "Messages")]
		[TestCase("A7F7870A-A05F-4216-9D44-FC47A9B72B3C", "EDIFACT", "Messages")]
		[TestCase("C7F7870A-A05F-4216-9D44-FC47A9B71B2C", "X12", "Messages")]
		[Explicit]
		public void Test_PrettyPrintMessages(string messageTrackingId, string messageType, string page)
		{
			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("Messages");
			Driver.Url = endPoint.AbsoluteUri;

			SearchMessage("MessageTrackingID", messageTrackingId, true);
			WaitUntilFindElement(By.XPath("//td[@id='messageDetailsTab']"));

			switch (page)
			{
				case "Messages":
					var accordionButton = Driver.FindElement(By.Id("accordionButton"));
					accordionButton.Click();
					WaitUntilFindElement(By.ClassName("message-details"));
					break;
				case "Details":
					Driver.FindElement(By.ClassName("glyphicon-align-justify")).Click();
					while (Driver.WindowHandles.Count < 2)
					{
						Task.Delay(200).Wait();
					}
					Driver.Close();
					Driver.SwitchTo().Window(Driver.WindowHandles[0]);
					WaitUntilFindElement(By.ClassName("received-local-time"));
					break;
			}
			// Pretty-Print Button
			Assert.That(Driver.FindElements(By.XPath("//input[@id='pretty-print-button']")), Is.Not.Empty);

			// Preview
			Driver.FindElement(By.ClassName("preview-click")).Click();
			WaitUntilFindElement(By.ClassName("popover"));
			var previewPopover = Driver.FindElement(By.ClassName("popover"));
			while (previewPopover.Text.Contains("Retrieving") || previewPopover.Text == "")
			{
				Task.Delay(200).Wait();
			}
			var previewContent = previewPopover.Text;
			Assert.That(IsPrettyPrint(previewContent, messageType), Is.True);

			// View - before Pretty-Print
			Driver.FindElement(By.ClassName("view-click")).Click();
			while (Driver.WindowHandles.Count < 2)
			{
				Task.Delay(200).Wait();
			}
			var window = Driver.SwitchTo().Window(Driver.WindowHandles[1]);
			var viewContent = Driver.FindElement(By.TagName("pre")).Text;
			window.Close();
			Driver.SwitchTo().Window(Driver.WindowHandles[0]);
			Assert.That(IsPrettyPrint(viewContent, messageType), Is.False);

			WaitUntilFindElement(By.XPath("//input[@id='pretty-print-button']"));
			Driver.FindElement(By.Id("pretty-print-button")).Click();

			// View - after Pretty-Print
			Driver.FindElement(By.ClassName("view-click")).Click();
			while (Driver.WindowHandles.Count < 2)
			{
				Task.Delay(200).Wait();
			}
			window = Driver.SwitchTo().Window(Driver.WindowHandles[1]);
			viewContent = Driver.FindElement(By.TagName("pre")).Text;
			window.Close();
			Driver.SwitchTo().Window(Driver.WindowHandles[0]);
			Assert.That(IsPrettyPrint(viewContent, messageType), Is.True);
		}

		public bool IsPrettyPrint(string currentMessage, string messageType)
		{
			switch (messageType)
			{
				case "EDIFACT":
					Regex edifactRegex = new Regex(@"(?<!\?)\'[^\r\n]");
					return !(edifactRegex.IsMatch(currentMessage));
				case "X12":
					Regex x12Regex = new Regex(@"\~[^\r\n]");
					return !(x12Regex.IsMatch(currentMessage));
				default:
					return true;
			}
		}

		[Test]
		[Explicit]
		public void Test_MessagesLoadMore()
		{
			PreservedNodesXpaths = new[]
			{
				"//div[@id='list']",
				"//div[@id='list-footer']",
				"//div[@id='inbox-message']"
			};
			var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International", true);
			var oldShortDate = registryKey.GetValue("sShortDate");
			var oldShortTime = registryKey.GetValue("sShortTime");
			registryKey.SetValue("sShortDate", "dd/MM/yyyy");
			registryKey.SetValue("sShortTime", "HH:mm");
			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("Messages");
			Driver.Url = endPoint.AbsoluteUri;

			var utcTime = new DateTime(2020, 12, 30, 0, 0, 0);
			SearchMessageByDateRange(utcTime, null, "UTC");
			var transactionsMessageDt = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var expectedDocument = ReadEmbeddedHtml("Views.MessagesTests.ExpectedPage.Messages_HasMoreResult.html");
			var timeZoneIdIANA = ReplaceTimeZoneInTableHeader(expectedDocument);
			TestHelper.ReplaceInnerHtml(expectedDocument, "//td[@class='received-local-time']", "[TRANSACTIONS_RECEIVED_LOCAL]", transactionsMessageDt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
			TestHelper.ReplaceInnerHtml(expectedDocument, "//span[@class='delivered-local-time']", "[TRANSACTIONS_DELIVERED_LOCAL]", transactionsMessageDt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

			var timeZoneInfo = TZConvert.GetTimeZoneInfo(timeZoneIdIANA);
			var offset = timeZoneInfo.GetUtcOffset(transactionsMessageDt);
			var formattedOffset = offset.Hours.ToString("+00;-00") + ":" + Math.Abs(offset.Minutes).ToString("00");
			TestHelper.ReplaceInnerHtml(expectedDocument, "//div[@id='list-footer']/span", "[SEARCH_LIMIT_DATETIME]", $"{transactionsMessageDt.ToLocalTime():yyyy-MM-dd HH:mm:ss} {formattedOffset}");
			WaitUntilFindElement(By.XPath("//button[@id='load-more-btn']"));
			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			Driver.FindElement(By.Id("load-more-btn")).Click();
			WaitUntilFindElement(By.XPath("//a[@href='/Messages/Details?AM_PK=9134b76a-233a-4e8d-9482-54afe0f7a5d9']"));
			expectedDocument = ReadEmbeddedHtml("Views.MessagesTests.ExpectedPage.Messages_NoMoreResult.html");
			ReplaceTimeZoneInTableHeader(expectedDocument);
			TestHelper.ReplaceInnerHtml(expectedDocument, "//td[@class='received-local-time']", "[TRANSACTIONS_RECEIVED_LOCAL]", transactionsMessageDt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
			TestHelper.ReplaceInnerHtml(expectedDocument, "//span[@class='delivered-local-time']", "[TRANSACTIONS_DELIVERED_LOCAL]", transactionsMessageDt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
			var archivedMessageReceivedUtc = new DateTime(2020, 12, 31, 0, 0, 0, DateTimeKind.Utc);
			TestHelper.ReplaceInnerHtml(expectedDocument, "//td[@class='received-local-time']", "[ARCHIVE_RECEIVED_LOCAL]", archivedMessageReceivedUtc.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
			TestHelper.ReplaceInnerHtml(expectedDocument, "//span[@class='delivered-local-time']", "[ARCHIVE_DELIVERED_LOCAL]", archivedMessageReceivedUtc.AddSeconds(10).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));

			offset = timeZoneInfo.GetUtcOffset(archivedMessageReceivedUtc);
			formattedOffset = offset.Hours.ToString("+00;-00") + ":" + Math.Abs(offset.Minutes).ToString("00");
			TestHelper.ReplaceInnerHtml(expectedDocument, "//div[@id='list-footer']/span", "[SEARCH_LIMIT_DATETIME]", $"{archivedMessageReceivedUtc.ToLocalTime():yyyy-MM-dd HH:mm:ss} {formattedOffset}");

			CompareHtmlDocuments(expectedDocument, SnapShotPage());
			registryKey.SetValue("sShortDate", oldShortDate);
			registryKey.SetValue("sShortTime", oldShortTime);
		}

		[Test]
		[Explicit]
		public void TestDateRangeType_DefaultShouldBeLocal()
		{
			var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International", true);
			var oldShortDate = registryKey.GetValue("sShortDate");
			var oldShortTime = registryKey.GetValue("sShortTime");
			registryKey.SetValue("sShortDate", "dd/MM/yyyy");
			registryKey.SetValue("sShortTime", "HH:mm");

			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("Messages");
			Driver.Url = endPoint.AbsoluteUri;

			var utcTime = new DateTime(2021, 1, 1, 0, 0, 0);
			var localTime = utcTime.Add(TimeZoneInfo.Local.GetUtcOffset(utcTime));
			var fromTime = localTime.AddMinutes(-1);
			var toTime = localTime.AddMinutes(1);
			SearchMessageByDateRange(fromTime, toTime, null);
			WaitUntilFindElement(By.XPath("//a[@href='/Messages/Details?EI_PK=dc1f0e01-6907-489b-9df8-6811b8ee370e&Ref=637450560000000000']"));

			var expectedDocument = ReadEmbeddedHtml("Views.MessagesTests.ExpectedPage.Messages_Transactions_FinalPage.html");
			ReplaceLocalTime(expectedDocument, "received-local-time", utcTime);
			ReplaceLocalTime(expectedDocument, "delivered-local-time", utcTime);
			ReplaceTimeZoneInTableHeader(expectedDocument);

			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			registryKey.SetValue("sShortDate", oldShortDate);
			registryKey.SetValue("sShortTime", oldShortTime);
		}

		[Test]
		[Explicit]
		public void TestDateRangeType_Utc()
		{
			var registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\International", true);
			var oldShortDate = registryKey.GetValue("sShortDate");
			var oldShortTime = registryKey.GetValue("sShortTime");
			registryKey.SetValue("sShortDate", "dd/MM/yyyy");
			registryKey.SetValue("sShortTime", "HH:mm");

			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("Messages");
			Driver.Url = endPoint.AbsoluteUri;

			var utcTime = new DateTime(2021, 1, 1, 0, 0, 0);
			var fromTime = utcTime.AddMinutes(-1);
			var toTime = utcTime.AddMinutes(1);
			SearchMessageByDateRange(fromTime, toTime, "UTC");
			WaitUntilFindElement(By.XPath("//a[@href='/Messages/Details?EI_PK=dc1f0e01-6907-489b-9df8-6811b8ee370e&Ref=637450560000000000']"));

			var expectedDocument = ReadEmbeddedHtml("Views.MessagesTests.ExpectedPage.Messages_Transactions_FinalPage.html");
			ReplaceLocalTime(expectedDocument, "received-local-time", utcTime);
			ReplaceLocalTime(expectedDocument, "delivered-local-time", utcTime);
			ReplaceTimeZoneInTableHeader(expectedDocument);

			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			registryKey.SetValue("sShortDate", oldShortDate);
			registryKey.SetValue("sShortTime", oldShortTime);
		}

		void SearchMessage(string keyType, string keyValue, bool isQueryMenuDisplayed)
		{
			WaitUntilFindElement(By.Id("query_button"));
			var queryButton = Driver.FindElement(By.Id("query_button"));
			if(isQueryMenuDisplayed == false)
			{
				queryButton.Click();
			}
			WaitUntilFindElement(By.XPath("//div[@id='collapseQuery' and @class='in']"));
			if (!string.IsNullOrEmpty(keyType))
			{
				var selectOption = new SelectElement(Driver.FindElement(By.Id("KeyType")));
				selectOption.SelectByText(keyType);

				IWebElement keyTextBox = Driver.FindElement(By.Id("Key"));
				keyTextBox.Clear();
				keyTextBox.SendKeys(keyValue);
			}

			IWebElement submitButton = Driver.FindElement(By.XPath("//input[@type='submit' and @value='Submit']"));
			submitButton.Click();
			Task.Delay(200).Wait();

			try
			{
				WaitUntilFindElement(By.XPath("//div[@id='collapseQuery' and @class='collapse']"), 2);
			}
			catch (Exception)
			{
				submitButton.Click();
				WaitUntilFindElement(By.XPath("//div[@id='collapseQuery' and @class='collapse']"), 2);
			}
		}

		void SearchMessageByDateRange(DateTime? fromTime, DateTime? toTime, string dateRangeType)
		{
			WaitUntilFindElement(By.Id("query_button"));
			WaitUntilFindElement(By.XPath("//div[@id='collapseQuery' and @class='in']"));

			Driver.FindElement(By.Id("radioDateRange")).Click();
			if (fromTime != null)
			{
				Driver.FindElement(By.Id("FromDate")).SendKeys(fromTime?.ToString("ddMMyyyy", CultureInfo.InvariantCulture));
				Driver.FindElement(By.Id("FromTime")).SendKeys(fromTime?.ToString("HHmm", CultureInfo.InvariantCulture));
			}

			if (toTime != null)
			{
				Driver.FindElement(By.Id("ToDate")).SendKeys(toTime?.ToString("ddMMyyyy", CultureInfo.InvariantCulture));
				Driver.FindElement(By.Id("ToTime")).SendKeys(toTime?.ToString("HHmm", CultureInfo.InvariantCulture));
			}

			if (dateRangeType != null)
			{
				new SelectElement(Driver.FindElement(By.Id("DateRangeType"))).SelectByText(dateRangeType);
			}

			IWebElement submitButton = Driver.FindElement(By.XPath("//input[@type='submit' and @value='Submit']"));
			submitButton.Click();
			Task.Delay(200).Wait();

			try
			{
				WaitUntilFindElement(By.XPath("//div[@id='collapseQuery' and @class='collapse']"), 2);
			}
			catch (Exception)
			{
				submitButton.Click();
				WaitUntilFindElement(By.XPath("//div[@id='collapseQuery' and @class='collapse']"), 2);
			}
		}

		void ReplaceLocalTime(HtmlDocument htmlDocument, string className, DateTime utcTime, bool includeTimeZone = false)
		{
			var localTimeNodes = htmlDocument.DocumentNode.SelectNodes($"//td[@class='{className}']|//span[@class='{className}']");
			var localTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.Local);
			foreach (var node in localTimeNodes)
			{
				if (includeTimeZone)
				{
					node.InnerHtml = localTime.ToString("yyyy-MM-dd HH:mm:ss zzz");
				}
				else
				{
					node.InnerHtml = localTime.ToString("yyyy-MM-dd HH:mm:ss");
				}
			}
		}

		string ReplaceTimeZoneInTableHeader(HtmlDocument htmlDocument)
		{
			var jsExecutor = (IJavaScriptExecutor)Driver;
			var timeZoneIdIANA = (string)jsExecutor.ExecuteScript("return Intl.DateTimeFormat().resolvedOptions().timeZone;");
			htmlDocument.DocumentNode.SelectSingleNode("//th[@id='received-local-time-th']").InnerHtml = $"Received ({timeZoneIdIANA})";
			return timeZoneIdIANA;
		}
	}
}
