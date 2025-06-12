using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.eHub.Selenium.IntegrationTests.Core;
using eServices.eHubAdmin.IntegrationTests.Attributes;
using HtmlAgilityPack;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace eServices.eHubAdmin.IntegrationTests.Views.MonitorsTest
{
	[TestFixture]
	[WithEHubAdminService]
	public class MonitorsTests : SeleniumTestBase
	{
		protected override string TestDataLocation => ".Views.MonitorsTests.Data.";
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
				"//div[@class='container body-content']"
			};
			ReferencePath = "../../../../eHubAdmin";
			SkipComparingAttributes = new Dictionary<string, string>
			{
				{"html", "class"},
				{"img", "style" }
			};
			base.SetUpField();
		}

		[Test]
		[Explicit]
		public void TestMonitors()
		{
			var endPoint = WithEHubAdminServiceAttribute.Current.GetHttpEndPoint("Monitors");
			Driver.Url = endPoint.AbsoluteUri;
			WaitUntilFindElement(By.XPath("//button[@id='AIR_OUT_CCN_Refresh']"));

			Driver.FindElement(By.Id("refresh-btn-all")).Click();
			var age = SelectDateDiffInMinute("2021-01-01 00:00:00");
			WaitUntilFindElement(By.XPath("//button[@id='refresh-btn-all'][not(@disabled)]"));
			var expectedDocument = ReadEmbeddedHtml("Views.MonitorsTests.ExpectedPage.Monitors_Alert_IndexPage.html");
			TestHelper.ReplaceInnerHtml(expectedDocument, "//td[@class='age']", "[AGE]", age);
			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			Actions ctrlClick = new Actions(Driver);
			var link1 = Driver.FindElement(By.XPath("//a[@href='/Monitors/Details/AIR_OUT_Descartes']"));
			ctrlClick.KeyDown(Keys.Control).Click(link1).KeyUp(Keys.Control).Perform();
			Driver.SwitchTo().Window(Driver.WindowHandles[1]);
			WaitUntilFindElement(By.XPath("//tr[@class='list-detail warning']"));
			expectedDocument = ReadEmbeddedHtml("Views.MonitorsTests.ExpectedPage.Monitors_Alert_DetailPage.html");
			TestHelper.ReplaceInnerHtml(expectedDocument, "//dd", "[AGE]", age);
			WaitUntilFindElement(By.XPath("//button[@id='load-more-btn']"));
			CompareHtmlDocuments(expectedDocument, SnapShotPage());
			Driver.FindElement(By.Id("load-more-btn")).Click();
			expectedDocument = ReadEmbeddedHtml("Views.MonitorsTests.ExpectedPage.Monitors_Alert_DetailPage_NoLoadMore.html");
			TestHelper.ReplaceInnerHtml(expectedDocument, "//dd", "[AGE]", age);
			WaitUntilFindElement(By.XPath("//a[@href='/Messages/Details?EI_PK=f143f1e1-9de5-4635-8711-16509531a309&Ref=637450560000000000']"));
			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			Driver.Close();

			Driver.SwitchTo().Window(Driver.WindowHandles[0]);
			var link2 = Driver.FindElement(By.XPath("//a[@href='/Monitors/Details/AIR_OUT_CCN']"));
			ctrlClick = new Actions(Driver);
			ctrlClick.KeyDown(Keys.Control).Click(link2).KeyUp(Keys.Control).Perform();
			Driver.SwitchTo().Window(Driver.WindowHandles[1]);
			WaitUntilFindElement(By.Id("list"));
			expectedDocument = ReadEmbeddedHtml("Views.MonitorsTests.ExpectedPage.Monitors_Success_DetailPage.html");
			CompareHtmlDocuments(expectedDocument, SnapShotPage());
			Driver.Close();

			Driver.SwitchTo().Window(Driver.WindowHandles[0]);
			UpdateMessageDistributionStatusUsingOutboxPK("AF1DA10C-C61E-45E7-B743-3088F5237C26");
			UpdateMessageDistributionStatusUsingOutboxPK("F8369A85-ED09-47E3-B9AC-B97910B79D2D");
			WaitUntilFindElement(By.XPath("//button[@id='AIR_OUT_Descartes_Refresh']"));
			Driver.FindElement(By.Id("AIR_OUT_Descartes_Refresh")).Click();
			WaitUntilFindElement(By.XPath("//tr[@id='9775d4e4-61ba-436e-8c95-98b307190cd9' and @class='']"));
			expectedDocument = ReadEmbeddedHtml("Views.MonitorsTests.ExpectedPage.Monitors_Success_IndexPage.html");
			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			// test CW1_Download monitor details
			var now = DateTime.UtcNow;
			UpdateInboxMessageInsertDateTime("CE3502AB-09EE-41F0-A434-74819A04CE4F", now.AddHours(-1));
			UpdateOutboxMessageInsertDateTime("22EE7686-5BEC-4F39-BC1F-B7C5792F0E9A", now.AddHours(-2));
			Driver.SwitchTo().Window(Driver.WindowHandles[0]);
			var link3 = Driver.FindElement(By.XPath("//a[@href='/Monitors/Details/CW1_DOWNLOAD']"));
			ctrlClick = new Actions(Driver);
			ctrlClick.KeyDown(Keys.Control).Click(link3).KeyUp(Keys.Control).Perform();
			Driver.SwitchTo().Window(Driver.WindowHandles[1]);
			WaitUntilFindElement(By.Id("SubItem"));
			expectedDocument = ReadEmbeddedHtml("Views.MonitorsTests.ExpectedPage.Monitors_Alert_DetailPage_CW1_Download.html");
			CompareHtmlDocuments(expectedDocument, SnapShotPage());

			Driver.Close();
		}
	}
}
