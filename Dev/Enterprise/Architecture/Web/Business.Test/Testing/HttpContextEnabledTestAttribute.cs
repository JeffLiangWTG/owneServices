using System;
using System.IO;
using System.Reflection;
#if NETFRAMEWORK
using System.Collections;
using System.Collections.Specialized;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using CargoWise.Types;
#elif NET
using Enterprise.MasterFiles.Business;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
#endif
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
#if NETFRAMEWORK
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class HttpContextEnabledTestAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			SetupDomainData();
			SetupHttpContext(testCase);
			SetupHttpSessionStateCollection();
			SetupBrowserCapabilities();
			MakeRequestQueryStringWritableForTest();
			MakeRequestFormCollectionWritableForTest();
		}

		public override void TearDown(TestCase testCase)
		{
			HttpContext.Current.ApplicationInstance.Dispose();

			foreach (DictionaryEntry item in HttpContext.Current.Cache)
			{
				string key = (string)item.Key;
				HttpContext.Current.Cache.Remove(key);
			}

			HttpContext.Current = null;
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomain currentDomain = Thread.GetDomain();
			currentDomain.SetData(".appDomain", originalAppDomain);
			currentDomain.SetData(".domainId", originalDomainId);
			currentDomain.SetData(".appPath", originalAppPath);
			currentDomain.SetData(".appVPath", originalAppVPath);
			currentDomain.SetData(".hostingVirtualPath", originalHostingVirtualPath);
			currentDomain.SetData(".hostingInstallDir", originalHostingInstallDir);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		void SetupDomainData()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			AppDomain currentDomain = Thread.GetDomain();
			originalAppDomain = (string)currentDomain.GetData(".appDomain");
			originalDomainId = (string)currentDomain.GetData(".domainId");
			originalAppPath = (string)currentDomain.GetData(".appPath");
			originalAppVPath = (string)currentDomain.GetData(".appVPath");
			originalHostingInstallDir = (string)currentDomain.GetData(".hostingInstallDir");
			originalHostingVirtualPath = (string)currentDomain.GetData(".hostingVirtualPath");

			currentDomain.SetData(".appDomain", "*");
			currentDomain.SetData(".domainId", ZGuid.NewZGuid().ToString());
			currentDomain.SetData(".appPath", @"C:\inetpub\wwwroot\webapp\"); // This is for test only, doesn't require a real directory
			currentDomain.SetData(".appVPath", "/webapp");
			currentDomain.SetData(".hostingVirtualPath", HttpRuntime.AppDomainAppVirtualPath);
			currentDomain.SetData(".hostingInstallDir", HttpRuntime.AspInstallDirectory);

			HttpRuntime theRuntime = (HttpRuntime)typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
			typeof(HttpRuntime).GetMethod("Init", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(theRuntime, null);
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		void SetupHttpContext(TestCase testCase)
		{
			StringWriter stringWriter = new StringWriter();
			string requestPath = "default.aspx";
			string requestQuery = "";
			if (testCase is IHttpContextEnabledTestWithRequestPath)
			{
				requestPath = ((IHttpContextEnabledTestWithRequestPath)testCase).RequestPath;
				requestQuery = ((IHttpContextEnabledTestWithRequestPath)testCase).QueryString;
			}

			string fullyQualifiedTestName = TestCase.CurrentTestName;
			string[] testNameParts = fullyQualifiedTestName.Split('.');
			string unQualifiedTestName = testNameParts[testNameParts.Length - 1];

			MethodInfo method = testCase.GetType().GetMethod(unQualifiedTestName);
			object[] attributes = method.GetCustomAttributes(typeof(QueryStringAttribute), true);
			if (attributes.Length > 0)
			{
				requestQuery = ((QueryStringAttribute)attributes[0]).QueryString;
			}
			attributes = method.GetCustomAttributes(typeof(RequestStringAttribute), true);
			if (attributes.Length > 0)
			{
				requestPath = ((RequestStringAttribute)attributes[0]).RequestString;
			}

			DummyWorkerRequest workerRequest = (testCase is IHttpContextEnabledTestWithHttps)
				? new SecureDummyWorkerRequest(requestPath, requestQuery, stringWriter)
				: new DummyWorkerRequest(requestPath, requestQuery, stringWriter);
			HttpContext.Current = new HttpContext(workerRequest);

			HttpApplication testApplication = (testCase is IHttpContextEnabledTestWithAppInstance)
				? ((IHttpContextEnabledTestWithAppInstance)testCase).AppInstance
				: new DummyHttpApplication(workerRequest);

			typeof(HttpApplication).InvokeMember("InitInternal",
				BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				testApplication,
				new object[] { HttpContext.Current, testApplication.Application, Array.Empty<MethodInfo>() });
			typeof(HttpApplication).InvokeMember("_context",
				BindingFlags.SetField | BindingFlags.NonPublic | BindingFlags.Instance,
				null,
				testApplication,
				new object[] { HttpContext.Current });

			HttpContext.Current.ApplicationInstance = testApplication;
		}

		void SetupHttpSessionStateCollection()
		{
			HttpSessionStateContainer container = new HttpSessionStateContainer("DummySession", new SessionStateItemCollection(), new HttpStaticObjectsCollection(), 60, true, HttpCookieMode.AutoDetect, SessionStateMode.InProc, false);
			SessionStateUtility.AddHttpSessionStateToContext(HttpContext.Current, container);
		}

		void SetupBrowserCapabilities()
		{
			HttpContext.Current.Request.Browser = new HttpBrowserCapabilities();
			HttpContext.Current.Request.Browser.Capabilities = new HybridDictionary();

			HttpContext.Current.Request.Browser.Capabilities["canInitiateVoiceCall"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["css2"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresFullyQualifiedRedirectUrl"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["requiresAttributeColonSubstitution"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["maximumRenderedPageSize"] = "300000";
			HttpContext.Current.Request.Browser.Capabilities["backgroundsounds"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresUniqueHtmlCheckboxNames"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["rendersBreakBeforeWmlSelectAndInput"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["ecmascriptversion"] = "1.2";
			HttpContext.Current.Request.Browser.Capabilities["supportsXmlHttp"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["isMobileDevice"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["rendersBreaksAfterWmlInput"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["vbscript"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["rendersWmlSelectsAsMenuCards"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsAccesskeyAttribute"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["requiresNoSoftkeyLabels"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["javascript"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsDivNoWrap"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresLeadingPageBreak"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["defaultCharacterHeight"] = "12";
			HttpContext.Current.Request.Browser.Capabilities["requiresPhoneNumbersAsPlainText"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsMaintainScrollPositionOnPostback"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["minorversion"] = ".0";
			HttpContext.Current.Request.Browser.Capabilities["jscriptversion"] = "5.6";
			HttpContext.Current.Request.Browser.Capabilities["defaultScreenCharactersHeight"] = "40";
			HttpContext.Current.Request.Browser.Capabilities["supportsImageSubmit"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresHtmlAdaptiveErrorReporting"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["preferredImageMime"] = "image/gif";
			HttpContext.Current.Request.Browser.Capabilities["supportsVCard"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["screenBitDepth"] = "8";
			HttpContext.Current.Request.Browser.Capabilities["supportsFontSize"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresContentTypeMetaTag"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["w3cdomversion"] = "1.0";
			HttpContext.Current.Request.Browser.Capabilities["gatewayVersion"] = "None";
			HttpContext.Current.Request.Browser.Capabilities["requiresPostRedirectionHandling"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["inputType"] = "keyboard";
			HttpContext.Current.Request.Browser.Capabilities["maximumSoftkeyLabelLength"] = "5";
			HttpContext.Current.Request.Browser.Capabilities["version"] = "6.0";
			HttpContext.Current.Request.Browser.Capabilities["beta"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["canRenderOneventAndPrevElementsTogether"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsIModeSymbols"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["requiresAdaptiveErrorReporting"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["aol"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["crawler"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["requiresSpecialViewStateEncoding"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["majorversion"] = "6";
			HttpContext.Current.Request.Browser.Capabilities["supportsQueryStringInFormAction"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["ExchangeOmaSupported"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["preferredRenderingType"] = "html32";
			HttpContext.Current.Request.Browser.Capabilities["supportsItalic"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresUrlEncodedPostfieldValues"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["requiresNoBreakInFormatting"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["ak"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsCacheControlMetaTag"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["canRenderSetvarZeroWithMultiSelectionList"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsFontColor"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsJPhoneMultiMediaAttributes"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["gatewayMajorVersion"] = "0";
			HttpContext.Current.Request.Browser.Capabilities["msdomversion"] = "6.0";
			HttpContext.Current.Request.Browser.Capabilities["canCombineFormsInDeck"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["defaultScreenPixelsHeight"] = "480";
			HttpContext.Current.Request.Browser.Capabilities["cookies"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["mobileDeviceModel"] = "Unknown";
			HttpContext.Current.Request.Browser.Capabilities["defaultCharacterWidth"] = "8";
			HttpContext.Current.Request.Browser.Capabilities["supportsDivAlign"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["gold"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsCallback"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["javaapplets"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["canRenderAfterInputOrSelectElement"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["extra"] = "; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727; InfoPath.1";
			HttpContext.Current.Request.Browser.Capabilities["supportsSelectMultiple"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["mobileDeviceManufacturer"] = "Unknown";
			HttpContext.Current.Request.Browser.Capabilities["canSendMail"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsFontName"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["tagwriter"] = "System.Web.UI.HtmlTextWriter";
			HttpContext.Current.Request.Browser.Capabilities["xml"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsUncheck"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["platform"] = "WinXP";
			HttpContext.Current.Request.Browser.Capabilities["canRenderPostBackCards"] = "true";
			HttpContext.Current.Request.Browser.Capabilities[""] = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727; InfoPath.1)";
			HttpContext.Current.Request.Browser.Capabilities["supportsEmptyStringInCookieValue"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresUniqueFilePathSuffix"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsCharacterEntityEncoding"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresUniqueHtmlInputNames"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["defaultScreenPixelsWidth"] = "640";
			HttpContext.Current.Request.Browser.Capabilities["cdf"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsFileUpload"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsBodyColor"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["requiresNoescapedPostUrl"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["defaultSubmitButtonLimit"] = "1";
			HttpContext.Current.Request.Browser.Capabilities["canRenderMixedSelects"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["defaultScreenCharactersWidth"] = "80";
			HttpContext.Current.Request.Browser.Capabilities["hasBackButton"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["cachesAllResponsesWithExpires"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["gatewayMinorVersion"] = "0";
			HttpContext.Current.Request.Browser.Capabilities["maximumHrefLength"] = "10000";
			HttpContext.Current.Request.Browser.Capabilities["requiresOutputOptimization"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["requiredMetaTagNameValue"] = "";
			HttpContext.Current.Request.Browser.Capabilities["hidesRightAlignedMultiselectScrollbars"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["type"] = "IE6";
			HttpContext.Current.Request.Browser.Capabilities["tables"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["browser"] = "IE";
			HttpContext.Current.Request.Browser.Capabilities["activexcontrols"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsCss"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsMultilineTextBoxDisplay"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["win32"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["frames"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["preferredRenderingMime"] = "text/html";
			HttpContext.Current.Request.Browser.Capabilities["canRenderInputAndSelectElementsTogether"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["canRenderEmptySelects"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsBold"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["rendersBreaksAfterHtmlLists"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["supportsRedirectWithCookie"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["win16"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsInputMode"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsJPhoneSymbols"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["numberOfSoftkeys"] = "0";
			HttpContext.Current.Request.Browser.Capabilities["sk"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["requiresDBCSCharacter"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["rendersBreaksAfterWmlAnchor"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["authenticodeupdate"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["supportsInputIStyle"] = "false";
			HttpContext.Current.Request.Browser.Capabilities["isColor"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["css1"] = "true";
			HttpContext.Current.Request.Browser.Capabilities["rendersWmlDoAcceptsInline"] = "true";
		}

		void MakeRequestQueryStringWritableForTest()
		{
			MakeRequestNameValueCollectionWritableForTest(HttpContext.Current.Request.QueryString);
		}

		void MakeRequestFormCollectionWritableForTest()
		{
			MakeRequestNameValueCollectionWritableForTest(HttpContext.Current.Request.Form);
		}

		void MakeRequestNameValueCollectionWritableForTest(NameValueCollection collection)
		{
			PropertyInfo isReadOnlyProperty = typeof(NameValueCollection).GetProperty("IsReadOnly", BindingFlags.NonPublic | BindingFlags.Instance);
			isReadOnlyProperty.SetValue(collection, false, null);
		}

		string originalAppDomain;
		string originalDomainId;
		string originalAppPath;
		string originalAppVPath;
		string originalHostingInstallDir;
		string originalHostingVirtualPath;
	}
#elif NET
	[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class HttpContextEnabledTestAttribute : TestSetupAttribute
	{
		public static HttpContext HttpContext;
		public static IMemoryCache MemoryCache;
		public override void SetUp(TestCase testCase)
		{
			var services = new ServiceCollection();
			services.AddLogging();
			services.AddMemoryCache();
			services.AddHttpContextAccessor();
			services.AddSession();

			var serviceProvider = services.BuildServiceProvider();

			var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
			var httpContext = new DefaultHttpContext
			{
				RequestServices = serviceProvider
			};

			var session = new DummySession();
			httpContext.Session = session;

			httpContextAccessor.HttpContext = httpContext;

			HttpContext = httpContext;
			MemoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
			WebEnv.HttpContextAccessor = httpContextAccessor;
			SetupHttpContext(testCase, httpContext);
		}

		public override void TearDown(TestCase testCase)
		{
			HttpContext = null;
			MemoryCache = null;
			WebEnv.HttpContextAccessor = null;
		}

		void SetupHttpContext(TestCase testCase, HttpContext httpContext)
		{
			StringWriter stringWriter = new StringWriter();
			string requestPath = "default.aspx";
			string requestQuery = "";
			if (testCase is IHttpContextEnabledTestWithRequestPath)
			{
				requestPath = ((IHttpContextEnabledTestWithRequestPath)testCase).RequestPath;
				requestQuery = ((IHttpContextEnabledTestWithRequestPath)testCase).QueryString;
			}

			string fullyQualifiedTestName = TestCase.CurrentTestName;
			string[] testNameParts = fullyQualifiedTestName.Split('.');
			string unQualifiedTestName = testNameParts[testNameParts.Length - 1];

			MethodInfo method = testCase.GetType().GetMethod(unQualifiedTestName);
			object[] attributes = method.GetCustomAttributes(typeof(QueryStringAttribute), true);
			if (attributes.Length > 0)
			{
				requestQuery = ((QueryStringAttribute)attributes[0]).QueryString;
			}
			attributes = method.GetCustomAttributes(typeof(RequestStringAttribute), true);
			if (attributes.Length > 0)
			{
				requestPath = ((RequestStringAttribute)attributes[0]).RequestString;
			}

			DummyWorkerRequest workerRequest = (testCase is IHttpContextEnabledTestWithHttps)
				? new SecureDummyWorkerRequest(requestPath, requestQuery, stringWriter, HttpContext)
				: new DummyWorkerRequest(requestPath, requestQuery, stringWriter, HttpContext);

			if (testCase is IHttpContextEnabledTestWithAppInstance)
			{
				HttpContext.Session.SetObject("SiteUser", httpContext.Session?.GetObject<WebUser>("SiteUser"));
			}
			else
			{
				var siteUser = new OrgContactWebUser();
				siteUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				HttpContext.Session.SetObject("SiteUser", (WebUser)siteUser);
			}
		}
	}
#endif
}
