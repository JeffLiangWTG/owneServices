using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Design;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.Utilities.Environment;
using Enterprise.ZArchitecture.Web.Utilities.Test;
using Moq;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	[HttpContextEnabledTest]
	public class ZPageTest : WebControlTest
	{
		public void TestDisposeDbConnectionOnUnload()
		{
			var originalProvider = Env.GetCurrentProvider();
			using (var webProvider = new WebServiceEnvironmentProvider())
			{
				webProvider.Enable();
				using (Db.DisposableActionForDbConnection())
				{
					var dbConnectionHashCode = Db.Connection.GetHashCode();
					AssertEquals("Db connection should be active", dbConnectionHashCode, Db.Connection.GetHashCode());

					TestPage.OnUnloadInternal(new EventArgs());

					using (Db.DisposableActionForDbConnection())
					{
						AssertNotEquals("Db connection should get disposed", dbConnectionHashCode, Db.Connection.GetHashCode());
					}
				}
			}
			originalProvider.Enable();
		}

		public void TestProcessRequestUsesDisposableConnection()
		{
			var errorReporterMock = new Mock<IErrorReporter>();
			var page = new ZTestPage();
			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				using (var env = new TestWebDbEnvironment())
				{
					env.SetServingWebBasedApp(true);

					var pageFrameworkInitializeHasBeenCalled = false;
					page.OnTestingDbDisposableConnection = () =>
					{
						pageFrameworkInitializeHasBeenCalled = true;
						Db.Connection.EnsureIsOpen();
					};

					Db.ResetAlreadyReported_ForTest();
					page.ProcessRequest(HttpContext.Current);
					Assert(pageFrameworkInitializeHasBeenCalled);
				}

				errorReporterMock.Verify(
					reporter => reporter.ReportDeveloperExceptionOrHandleSilently(
						It.IsAny<string>(),
						ThreadStaticConnectionFactory.AttemptToUseConnectionWithoutDisposableAction,
						It.IsAny<InvalidOperationException>()
					),
					Times.Never
				);
			}
		}

		public void TestBusinessObjectToValidateIsNull()
		{
			TestPage.fDataSource = null;
			bool saveResult = true;
			AssertNoExceptionThrown("Save without exceptions when BusinessObjectToValidate is null", () => saveResult = TestPage.SaveDataSourceFactory(true));
			Assert("But the saving returns false", !saveResult);
			Assert("Redirected to error page", HttpContext.Current.Response.IsRequestBeingRedirected);
			AssertEquals("Response with code 302", 302, HttpContext.Current.Response.StatusCode);
		}

		#region TestPageName

		public void TestPageName()
		{
			if (!string.IsNullOrEmpty(GetExpectedPageName()))
			{
				Assert(!string.IsNullOrEmpty(TestPage.PageName));
			}
			AssertEquals(GetExpectedPageName(), TestPage.PageName);
		}

		protected virtual string GetExpectedPageName()
		{
			return string.Empty;
		}

		#endregion

		#region TestRenderCustomAlertMessageFromSession

		public void TestRenderCustomAlertMessageFromSession()
		{
			const string alert = "some test alert";

			using (var temp = new TempDirectory())
			{
				TestPage.SetServerMappedPathForTest(temp.DirectoryName);
				TestPage.OnPreRenderInternal(EventArgs.Empty);

				Assert(!TestPage.ZClientScript.IsStartupScriptRegistered(TestPage.GetType(), alert));

				TestPage.Session[ZPage.zPageCustomAlertMessageIndexer] = alert;
				TestPage.OnPreRenderInternal(EventArgs.Empty);

				Assert("Custom alert message must be registered", TestPage.ZClientScript.IsStartupScriptRegistered(TestPage.GetType(), alert));
				AssertNull("Session must be cleaned", TestPage.Session[ZPage.zPageCustomAlertMessageIndexer]);
			}
		}

		#endregion

		#region Constructor Tests

		public void TestDesignTimeHelper()
		{
			AssertEquals("DesignTimeHelper not of expected type", typeof(TopLevelDataSourceTypeHelper), TestPage.topLevelDataSourceTypeHelper.GetType());
		}

		#endregion Constructor Tests

		#region Header Tests

		public virtual void TestSetHeadersAndStyle()
		{
			TestPage.Controls.Add(new HtmlForm());
			AssertNotNull("Form Control should not be null", TestPage.FormControl);
			TestPage.SetHeadersAndStyle();
			Control target = TestPage.FormControl.FindControl(TestPage.NotificationsArea.ID);
			AssertEquals("NavigationControl Div not found", typeof(HtmlGenericControl), target.GetType());
			AssertEquals("NavigationControl Id differs from expected", "ValidationControls", target.ID);
		}

		public void TestPageHeaderControlPath()
		{
			AssertEquals("PathHeaderControlPath differs from expected", ExpectedPageHeaderControlPath, TestPage.PageHeaderControlPathInternal);
		}

		public void TestShowLogOffLinkButton()
		{
			Assert("Default should be true", TestPage.ShowLogOffLinkButtonInternal);
		}

		public void TestShowChangePasswordLinkButton()
		{
			Assert("Default should be true", TestPage.ShowChangePasswordLinkButtonInternal);
		}

		#endregion Header Tests

		#region Footer Tests

		public void TestFooterAddedOnInit()
		{
			using (var temp = new TempDirectory())
			using (var page = new ZTestPage())
			{
				page.SetServerMappedPathForTest(temp.DirectoryName);
				page.Controls.Add(new HtmlForm());

				AssertNull("Precondition", page.FormControl.Controls.Cast<Control>().FirstOrDefault(x => x.ID == "PageFooter"));
				page.OnInitInternal(EventArgs.Empty);
				AssertNotNull("Footer added in OnInit", page.FormControl.Controls.Cast<Control>().FirstOrDefault(x => x.ID == "PageFooter"));
			}
		}

		#endregion

		#region Properties Tests

		public virtual void TestDataSource()
		{
			BusinessObject bizO = new ZPage().DataSource;
			AssertNull("ZPage does not provide a business object. Override this test in derived classes", bizO);
		}

		public virtual Type ExpectedBusinessObjectType
		{
			get { return typeof(DummyBusinessObject); }
		}

		public void TestBrowserType()
		{
			TestPage.fBrowserType = BrowserType.IE;
			AssertEquals("BrowserType differs from expected", BrowserType.IE, TestPage.BrowserType);
			TestPage.fBrowserType = BrowserType.Mozilla;
			AssertEquals("BrowserType differs from expected", BrowserType.Mozilla, TestPage.BrowserType);
		}

		public void TestFactory()
		{
			BusinessObjectFactory pageFactory = TestPage.Factory;
			AssertNotNull("Page factory should never be null", pageFactory);
			AssertEquals("Page should return BusinessObjectFactory", typeof(BusinessObjectFactory), pageFactory.GetType());
			AssertEquals("Page should return the same factory on multiple calls", pageFactory, TestPage.Factory);
		}

		public void TestErrorNotifications()
		{
			AssertEquals("Error Notifications should be displayed by default", true, TestPage.NotificationFlags.DisplayErrors);
			TestPage.NotificationFlags.DisplayErrors = false;
			AssertEquals("Error Notifications should be now suppressed", false, TestPage.NotificationFlags.DisplayErrors);
		}

		public void TestMessageErrorNotifications()
		{
			AssertEquals("Message Error Notifications should be displayed by default", true, TestPage.NotificationFlags.DisplayMessageErrors);
			TestPage.NotificationFlags.DisplayMessageErrors = false;
			AssertEquals("Message Error Notifications should be now suppressed", false, TestPage.NotificationFlags.DisplayMessageErrors);
		}

		public void TestWarningNotifications()
		{
			AssertEquals("Warning Notifications should be displayed by default", true, TestPage.NotificationFlags.DisplayWarnings);
			TestPage.NotificationFlags.DisplayWarnings = false;
			AssertEquals("Warning Notifications should be now suppressed", false, TestPage.NotificationFlags.DisplayWarnings);
		}

		#endregion Properties Tests

		#region PageOverride Tests

		public void TestWebBusinessObjectFactoryCreatedAndDisposed()
		{
			AssertNotNull(TestPage.Factory);
			TestPage.Dispose();
			AssertNull(TestPage.fFactory);
		}

		#endregion PageOverride Tests

		#region Virtual Members

		protected virtual string ExpectedPageHeaderControlPath
		{
			get { return ""; }
		}

		#endregion Virtual Members

		#region ChildControls Test

		public void TestNoScriptBlock()
		{
			AssertNotNull(TestPage.NoScriptBlock);
			AssertEquals("NoScript Tag", "NOSCRIPT", TestPage.NoScriptBlock.TagName.ToUpper());
			AssertEquals("NoScriptBlock Controls", 1, TestPage.NoScriptBlock.Controls.Count);
			Panel noScriptPanel = TestPage.NoScriptBlock.Controls[0] as Panel;
			AssertNotNull("NoScriptPanel", noScriptPanel);
			AssertEquals("Panel Background", nameof(System.Drawing.KnownColor.LightYellow).ToUpper(), noScriptPanel.Style["background"].ToUpper());
			AssertEquals("Panel Border", "SOLID", noScriptPanel.Style["border"].ToUpper());
			AssertEquals("Panel Border Width", "1px", noScriptPanel.Style["border-width"]);
			AssertEquals("Panel Border Color", "BLACK", noScriptPanel.Style["border-color"].ToUpper());
			AssertEquals("Panel Position", "ABSOLUTE", noScriptPanel.Style["position"].ToUpper());
			AssertEquals("Panel Position Top", "0px", noScriptPanel.Style["top"].ToLower());
			AssertEquals("Panel Position Left", "0px", noScriptPanel.Style["left"].ToLower());
			AssertEquals("Panel Controls", 1, noScriptPanel.Controls.Count);
			ZTextLabel label = noScriptPanel.Controls[0] as ZTextLabel;
			string expMessage = @"You do not have javascript enabled. This site requires javascript in order to operate correctly.<br>
Please enable javascript or add this site to the list of trusted sites.<p>
To enable javascript, do the following:<br>
<ol>
<li>Select the Tools Menu in Internet Explorer</li>
<li>Select <b>Internet Options</b></li>
<li>Select the <b>Security</b> tab</li>
<li>Click the <b>Custom Level</b> button</li>
<li>Scroll down to the <b>Scripting</b> heading</li>
<li>Select the <b>enable</b> option of <b>Active Scripting</b></li>
<li>Click <b>OK</b> in all open dialogs</li>
</ol>
For more detailed instructions regarding configuring trusted sites please <a href=http://www.microsoft.com/windows/ie/using/howto/security/settings.mspx>click here.</a>";
			AssertEquals("NoScript Message", expMessage, label.Text);
		}

		#endregion ChildControls Test

		#region DataSource Tests

		public virtual void TestGetNewDataSource()
		{
			BusinessObject bizObject = TestPage.GetNewDataSourceInternal();
			AssertNotNull(bizObject);
			AssertEquals("Test DataSource should be DummyBusinessObject", ExpectedBusinessObjectType, bizObject.GetType());
		}

		#endregion DataSource Tests

		#region TestGetGuidFromParameter

		public void TestGetGuidFromParameter()
		{
			NameValueCollection testParams = new NameValueCollection();
			AssertEquals("No key should return Empty", ZGuid.Empty, TestPage.GetGuidFromParameter("Nothing", testParams));

			testParams.Add("Invalid", "ThisIsNotARealZGuidClintyIsMyHero");
			AssertEquals("Invalid key should return Empty", ZGuid.Empty, TestPage.GetGuidFromParameter("Invalid", testParams));

			ZGuid testKey = ZGuid.NewZGuid();
			testParams.Add("TestKey", testKey.ToString());
			AssertEquals("Valid key should be return", testKey, TestPage.GetGuidFromParameter("TestKey", testParams));
		}

		#endregion

		#region TestGetStringFromParameter

		public void TestGetStringFromParameter()
		{
			NameValueCollection testParams = new NameValueCollection();
			AssertEquals("No key should return Empty", "", TestPage.GetStringFromParameter("Nothing", testParams));

			ZString testKey = "someValue";
			testParams.Add("TestKey", testKey.ToString());
			AssertEquals("Valid key should be return", testKey, TestPage.GetStringFromParameter("TestKey", testParams));
		}

		#endregion

		#region TestAppInstance

		public void TestAppInstance()
		{
			Assert("true by default", TestPage.IsCreateNewAppInstanceIfNullForTest);
			AssertNotNull("not null by default", TestPage.AppInstance);

			TestPage.IsCreateNewAppInstanceIfNullForTest = false;
			AssertNull(TestPage.AppInstance);
		}
		#endregion

		#region TestAdditionalStyleSheetRendering

		public void TestAdditionalStyleSheetRendering()
		{
			StringBuilder renderedControl = new StringBuilder();
			HtmlTextWriter writer = new HtmlTextWriter(new StringWriter(renderedControl));
			Page.AdditionalStyleSheets.Add("AdditionalStyleSheet1.css");
			Page.AdditionalStyleSheets.Add("AdditionalStyleSheet2.css");
			Page.AdditionalStyleSheets.Add("AdditionalStyleSheet3.css");

			Page.PrepareForRendering();
			Page.RenderControl(writer);

			string renderedControlOutput = renderedControl.ToString();

			MatchCollection matches = Regex.Matches(renderedControlOutput, @"<link[^>]* />", RegexOptions.Singleline);
			AssertEquals("Should contain 4 LINKS", 4, matches.Count);
			AssertEquals("First LINK should be to BaseStyle", "<link type=\"text/css\" rel=\"stylesheet\" href=\"/BaseStyle.css\" />", matches[0].Value);
			AssertEquals("Second LINK should be to AdditionalStyleSheet1", "<link type=\"text/css\" rel=\"stylesheet\" href=\"AdditionalStyleSheet1.css\" />", matches[1].Value);
			AssertEquals("Third LINK should be to AdditionalStyleSheet2", "<link type=\"text/css\" rel=\"stylesheet\" href=\"AdditionalStyleSheet2.css\" />", matches[2].Value);
			AssertEquals("Fourth LINK should be to AdditionalStyleSheet3", "<link type=\"text/css\" rel=\"stylesheet\" href=\"AdditionalStyleSheet3.css\" />", matches[3].Value);
		}
		#endregion

		#region SaveDataSourceFactory

		public virtual void TestSaveDataSourceFactoryNoBubble()
		{
			TestPage.LoadOrCreateDataSource();
			TestPage.DataSource.Factory.Saving += new BusinessObjectFactory.SavingEventHandler(DataSource_FactorySaving);
			TestPage.SaveDataSourceFactory(false);
			Assert(true);
			ErrorReporter.Clear();
		}

		#endregion

		#region TestIsChildControl

		public void TestIsChildControl()
		{
			Panel control1 = new Panel();
			Panel control2 = new Panel();
			ZButton child1 = new ZButton();
			ZButton child2 = new ZButton();
			ZButton nonChild = new ZButton();

			control1.Controls.Add(control2);
			control1.Controls.Add(child1);
			control2.Controls.Add(child2);

			AssertEquals("Control2 is child of Control1", true, ZPage.IsChildControl(control1.Controls, control2));
			AssertEquals("Child1 is child of Control1", true, ZPage.IsChildControl(control1.Controls, control2));
			AssertEquals("Child2 is child of Control2", true, ZPage.IsChildControl(control1.Controls, control2));
			AssertEquals("NonChild is not a child of any controls", false, ZPage.IsChildControl(control1.Controls, nonChild));
		}
		#endregion

		#region TestExternallyFiredPostBackHandlers

		public void TestExternallyFiredPostBackHandlers()
		{
			DummyExternallyFiredPostBackHandler handler = new DummyExternallyFiredPostBackHandler();
			handler.OnExternallyFiredPostback += new EventHandler(Handler_OnExternallyFiredPostback);
			TestPage.Controls.Add(handler);

			ZDummyButton targetControl = new ZDummyButton();
			targetControl.ID = "Target";

			TestPage.IsPostBack = true;
			AssertEquals("Page should be doing postback", true, TestPage.IsPostBack);

			TestPage.Controls.Add(targetControl);
			TestPage.RaisePostBackEventInternal(targetControl, "Target");
			AssertEquals("ExternallyFiredPostBackEventHandler should have been fired", true, ExternallyFiredPostbackHandlerCalled);
			AssertEquals("TargetButton should be control that caused postback", targetControl, TargetControlCausingPostback);
			AssertEquals("TargetButton RaisePostBackEvent should have been called", true, targetControl.RaisedPostBackEvent);
		}

		void Handler_OnExternallyFiredPostback(object sender, EventArgs e)
		{
			ExternallyFiredPostbackHandlerCalled = true;
			TargetControlCausingPostback = sender as Control;
		}
		bool ExternallyFiredPostbackHandlerCalled;
		Control TargetControlCausingPostback;

		#endregion

		#region Resources

		public void TestResources()
		{
			bool scriptFileResourceFound = false;
			bool javascriptFrameworkResourceFound = false;
			bool loginStatusControlResourceFound = false;
			bool searchControlResourceFound = false;
			bool commonPageScriptFileResourceFound = false;
			bool screenScriptFileResourceFound = false;

			foreach (ZWebResource resource in TestPage.Resources)
			{
				if (resource == TestPage.ScriptFile)
				{
					scriptFileResourceFound = true;
				}

				if (resource == TestPage.JavascriptFramework)
				{
					javascriptFrameworkResourceFound = true;
				}

				if (resource == TestPage.LoginStatusControl)
				{
					loginStatusControlResourceFound = true;
				}

				if (resource == TestPage.SearchControlResource)
				{
					searchControlResourceFound = true;
				}

				if (resource == TestPage.CommonPageScriptFile)
				{
					commonPageScriptFileResourceFound = true;
				}

				if (resource == TestPage.ScreenScriptFile)
				{
					screenScriptFileResourceFound = true;
				}
			}
			Assert("Script file should be included in the Resources collection", scriptFileResourceFound);
			Assert("Javascript Framework file should be included in the Resources collection", javascriptFrameworkResourceFound);
			Assert("Login status control should be included in the Resources collection", loginStatusControlResourceFound);
			Assert("Search control should be included in the Resources collection", searchControlResourceFound);
			Assert("Common script file should be included into Resources collection", commonPageScriptFileResourceFound);
			Assert("Screen script file should be included into Resources collection", screenScriptFileResourceFound);
		}

		#endregion

		public void TestLoginStatusControl()
		{
			var page = (ZPage)GetNewControl();
			AssertNull(page.GetLoginStatusInternal());

			var form = new HtmlForm();
			var loginStatus = new LoginStatus();

			page.Controls.Add(form);
			page.FormControl.Controls.Add(loginStatus);

			AssertEquals(loginStatus, page.GetLoginStatusInternal());
		}

		public void TestViewStateUserKey()
		{
			using (var temp = new TempDirectory())
			using (var page = new ZTestPage())
			{
				page.SetServerMappedPathForTest(temp.DirectoryName);
				page.Controls.Clear();
				page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				page.OnInitInternal(EventArgs.Empty);

				AssertEquals("ViewStateUserKey should be the session id", page.Session.SessionID, page.ViewStateUserKey);
			}
		}

		public void TestViewStateUserKey_NotAuthenticated()
		{
			Page.OnInitInternal(EventArgs.Empty);

			AssertNull("ViewStateUserKey should not be set for unauthenticated requests", Page.ViewStateUserKey);
		}

		public void TestViewStateUserKey_LoginPage()
		{
			using (var temp = new TempDirectory())
			using (var page = new ZTestLoginPage())
			{
				page.SetServerMappedPathForTest(temp.DirectoryName);
				page.Controls.Clear();
				page.SiteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				page.OnInitInternal(EventArgs.Empty);

				AssertNull("ViewStateUserKey should not be set for the Login page", page.ViewStateUserKey);
			}
		}

		class ZTestLoginPage : ZTestPage
		{
			protected override Uri RequestUrl
			{
				get { return new Uri($"http://www.test.com/ediWeb{AppInstance.LoginPage}"); }
			}
		}

		#region Implementation

		protected override Control GetNewControl()
		{
			return new ZTestPageInternal();
		}

		protected ZPage TestPage
		{
			get { return (ZPage)Control; }
		}

		protected HtmlForm TestForm
		{
			get { return TestPage.Form; }
		}

		void DataSource_FactorySaving(BusinessObjectFactory factory)
		{
			throw new ZSaveConcurrencyException(new ZDataConcurrencyException(new InvalidOperationException("Simulated Concurrency exception"), ((IBusinessObjectInternals)TestBizO).Row, ((IDbConnected)Factory).Connection), Factory);
		}

		#endregion Implementation

		#region MaintainScrollPositionOnPostBack Tests

		public void TestMaintainScrollPositionOnPostBack()
		{
			using (var temp = new TempDirectory())
			using (var page = new ZTestPage())
			{
				page.SetServerMappedPathForTest(temp.DirectoryName);
				page.OnInitInternal(EventArgs.Empty);

				Assert("Page should maintain scroll position on PostBack", page.MaintainScrollPositionOnPostBack);
			}
		}

		#endregion MaintainScrollPositionOnPostBack Tests

		#region RegisterReturnKeyCapture

		void AssertKeyCaptureScript(TextBox tb, string buttonClientId)
		{
			AssertEquals("Scripts does not match", tb.Attributes["onkeydown"], string.Format("return ZPage_ProcessKeyDown(13, '{0}', event)", buttonClientId));
		}

		public void TestRegisterReturnKeyCapture()
		{
			TextBox tb = new TextBox();

			Button button1 = new Button();
			button1.ID = "Button1";
			ZPage.ClientFunctions.RegisterReturnKeyCapture(tb, button1);

			AssertKeyCaptureScript(tb, button1.ClientID);

			HtmlInputButton button2 = new HtmlInputButton();
			button2.ID = "Button2";
			ZPage.ClientFunctions.RegisterReturnKeyCapture(tb, button2);

			AssertKeyCaptureScript(tb, button2.ClientID);
		}

		public void TestRegisterScriptForKeyCapture()
		{
			using (var temp = new TempDirectory())
			using (ZTestPage page = new ZTestPage())
			{
				page.SetServerMappedPathForTest(temp.DirectoryName);
				page.OnPreRenderForTesting();
				string message = string.Format("{0} is not registered", page.CommonPageScriptFile.FileName);
				Assert(message, page.ZClientScript.IsClientScriptIncludeRegistered("ZPage_Scripts"));
			}
		}

		#endregion
	}
}
