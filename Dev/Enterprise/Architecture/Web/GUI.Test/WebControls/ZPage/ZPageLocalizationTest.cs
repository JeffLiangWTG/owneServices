using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Moq;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZPageLocalizationTest : ZPageTestCase
	{
		string previousLanguage;

		protected override ZPage GetNewZPage()
		{
			return new ZTestPage();
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousLanguage = Res.CurrentLanguage;
		}

		protected override void TearDown()
		{
			ObjectFactory.Get<IResourceStrings>().CurrentLanguage = previousLanguage;
			base.TearDown();
		}

		public void TestDisposableActionForDbConnectionForInitializeCulture()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "Test Company";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_Email = "test@test.com";
			contact.OC_IsActive = true;
			contact.OC_WebAccessEnabled = true;
			contact.SetHashedPassword("test");
			Factory.Save();

			Page.SiteUser.LoginForTest(orgHeader.OH_Code, "test@test.com", "test");

			var errorReporterMock = new Mock<IErrorReporter>();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				using (var env = new Web.Utilities.Test.TestWebDbEnvironment())
				{
					env.SetServingWebBasedApp(true);
					Page.Session["Language"] = Enterprise.Core.SharedConstants.Languages.German;
					Db.ResetAlreadyReported_ForTest();
					Page.InitializeCultureInternal();
					AssertEquals(Enterprise.Core.SharedConstants.Languages.German, Res.CurrentLanguage);
				}

				errorReporterMock.Verify(reporter => reporter.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction, It.IsAny<InvalidOperationException>()), Times.Never);
			}
		}

		public void TestLanguage()
		{
			var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
			allowedLanguages.RemoveAll();
			var allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.English;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.German;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.French;
			WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

			AssertType(typeof(LanguageSelectionControl), Page.Footer.Controls.Cast<Control>().FirstOrDefault(control => control.ID == "LanguageSelection"));

			Page.Session["Language"] = Enterprise.Core.SharedConstants.Languages.German;
			Page.InitializeCultureInternal();
			AssertEquals(Enterprise.Core.SharedConstants.Languages.German, Res.CurrentLanguage);

			Page.Request.Cookies.Add(new HttpCookie("Language", Enterprise.Core.SharedConstants.Languages.French));
			Page.InitializeCultureInternal();
			AssertEquals(Enterprise.Core.SharedConstants.Languages.French, Res.CurrentLanguage);
		}

		public void TestLanguage_SessionLost()
		{
			var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
			allowedLanguages.RemoveAll();
			var allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.English;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.German;
			allowedItem = allowedLanguages.AddNew();
			allowedItem.Code = Enterprise.Core.SharedConstants.Languages.French;
			WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

			AssertType(typeof(LanguageSelectionControl), Page.Footer.Controls.Cast<Control>().FirstOrDefault(control => control.ID == "LanguageSelection"));

			Page.Session["Language"] = Enterprise.Core.SharedConstants.Languages.German;
			Page.InitializeCultureInternal();
			AssertEquals(Enterprise.Core.SharedConstants.Languages.German, Res.CurrentLanguage);

			Page.Request.Cookies.Add(new HttpCookie("Language", Enterprise.Core.SharedConstants.Languages.French));
			Env.ClearUserContext();
			AssertEquals(null, Env.CurrentUser);
			Page.InitializeCultureInternal();
			AssertEquals(true, Page.Response.IsRequestBeingRedirected);
			Assert(Page.Response.RedirectLocation.StartsWith("/Error.aspx?data="));
			var qs = new SecureQueryString(WebUtility.UrlDecode(Page.Response.RedirectLocation.Substring(17)));
			AssertEquals("title", "Missing session", qs["title"]);
			AssertEquals("message", "The session of current page has been lost, please try again.", qs["message"]);
		}

		public void TestLocalize()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.German))
			using (var mock = Res.UseMockData())
			{
				mock.Put("wc|Enterprise.ZArchitecture.Web.GUI|VGhlIFRpdGxl", new ResourceStringData("wc|Enterprise.ZArchitecture.Web.GUI|VGhlIFRpdGxl", "Eltit Eht"));
				mock.Put("wc|Enterprise.ZArchitecture.Web.GUI|VGhlIExhYmVs", new ResourceStringData("wc|Enterprise.ZArchitecture.Web.GUI|VGhlIExhYmVs", "Lebal Eht"));
				mock.Put("wc|Enterprise.ZArchitecture.Web.GUI|UmVmZXJlbmNl", new ResourceStringData("wc|Enterprise.ZArchitecture.Web.GUI|VGhlIExhYmVs", "Ecnerefer"));
				mock.Put("wc|Enterprise.ZArchitecture.Web.GUI|UmVjZWl2ZWQ=", new ResourceStringData("wc|Enterprise.ZArchitecture.Web.GUI|UmVjZWl2ZWQ=", "Deviecer"));
				mock.Put("wc|Enterprise.ZArchitecture.Web.GUI|UHJpbnRlZA==", new ResourceStringData("wc|Enterprise.ZArchitecture.Web.GUI|UHJpbnRlZA==", "Detnirp"));
				var head = new LiteralControl("<head><title>The Title</title></head>");
				var div = new ZDiv();
				var label = new ZTextLabel("The Label");
				var whitespace = new ZTextLabelWhiteSpace(5);
				var hyperlink = new HyperLink() { Text = "Reference" };
				var checkBox = new ZCheckBox() { Text = "Received" };
				var button = new ZButton() { Text = "Printed" };
				var linkbutton = new ZLinkButton() { Text = "Reference" };
				var grid = new ZDataGrid() { Caption = "The Label" };
				var panel = new ZCollapsablePanel { Label = "The Label" };
				Page.Controls.Add(head);
				Page.Controls.Add(div);
				div.Controls.Add(label);
				div.Controls.Add(whitespace);
				div.Controls.Add(hyperlink);
				div.Controls.Add(checkBox);
				div.Controls.Add(button);
				div.Controls.Add(linkbutton);
				div.Controls.Add(grid);
				div.Controls.Add(panel);
				Page.OnPreRenderInternal(EventArgs.Empty);
				AssertEquals("<head><title>Eltit Eht</title></head>", head.Text);
				AssertEquals("Lebal Eht", label.Text);
				AssertEquals("Ecnerefer", hyperlink.Text);
				AssertEquals("Deviecer", checkBox.Text);
				AssertEquals("Detnirp", button.Text);
				AssertEquals("Ecnerefer", linkbutton.Text);
				AssertEquals("Lebal Eht", grid.Caption);
				AssertEquals("Lebal Eht", panel.Label);
			}
		}

		public void TestLocalizationAsmid()
		{
			AssertEquals("Should get same asmid in any plantform", 39354, Page.LocalizationAsmid);
		}
	}
}
