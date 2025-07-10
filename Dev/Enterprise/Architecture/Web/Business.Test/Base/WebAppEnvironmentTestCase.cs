using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
#if NETFRAMEWORK
using System.Reflection;
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
using Moq;
#endif
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class WebAppEnvironmentTestCase : TestCaseWithFactory
	{
		IUserContext initialUserContext;

		protected override void SetUp()
		{
			base.SetUp();
			if (GlbBranch.CurrentBranch.GB_WebAddress.IsEmpty)
			{
				GlbBranch.CurrentBranch.GB_WebAddress = "http://www.wisetechglobal.com/";
				GlbBranch.CurrentBranch.Factory.Save();
			}

			DataRegistry.Instance.WebBranch = GlbBranch.CurrentBranch.PK.ToGuid();
			initialUserContext = Env.CurrentUserContext;
#if NET
			var mockAccessor = new Mock<IHttpContextAccessor>();
			var context = new DefaultHttpContext();
			context.Session = new DummySession();
			mockAccessor.Setup(a => a.HttpContext).Returns(context);
			WebEnv.HttpContextAccessor = mockAccessor.Object;
#endif
		}

#if NETFRAMEWORK
		public void TestSetupBranding_WhenProductivityWiseModeChanges()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				DataRegistry.Instance.ProductivityWiseModeEnabled = true;
				WebAppEnvironment.Setup();
				AssertEquals("ProductivityWise", BrandingFactory.Instance.ProductName);

				DataRegistry.Instance.ProductivityWiseModeEnabled = false;
				WebAppEnvironment.Setup();
				AssertEquals("CargoWise", BrandingFactory.Instance.ProductName);
			}
		}

		public void TestSetup_WithNoDbConnection()
		{
			var thread = new Thread(() =>
			{
				WebAppEnvironment.Setup();
			});

			thread.Start();
			thread.Join();

			AssertEquals("No DbConnnection errors should have been reported", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestSetupEnvironment()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				WebAppEnvironment.Setup();

				GlbBranch expBranch = Factory.LoadTop1<GlbBranch>(new ZQuery());
				GlbDepartment expDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "BRN");

				AssertEquals("Expected WebBranch to be returned", DataRegistry.Instance.WebBranch, GlbBranch.CurrentBranch.PK);
				AssertEquals("Expected default branch [BRN] to be returned", Enterprise.Environment.Env.CurrentDepartment.PK, expDepartment.PK);
			}
		}

		public void TestSetupEnvironmentWithInactiveBranches()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				var initialValue = DataRegistry.Instance.WebBranch;
				var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
				branch.GB_IsActive = false;
				DataRegistry.Instance.WebBranch = branch.PK.ToGuid();
				Factory.Save();

				AssertNoExceptionThrown("Allow inactive branches by default", () => WebAppEnvironment.Setup());
				AssertExceptionThrown("Throw when inactive branches not allowed", typeof(InvalidWebEnvironmentException), () => WebAppEnvironment.Setup(new WebAppEnvironment.Config(activeBranchOnly: true)));

				DataRegistry.Instance.WebBranch = initialValue;
			}
		}

		public void TestSetupEnvironmentWithInactiveDepartments()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				var initialValue = DataRegistry.Instance.WebDepartment;
				var department = Factory.LoadTop1<GlbDepartment>(new ZQuery());
				((ICancellable)department).IsCancelled = true;
				DataRegistry.Instance.WebDepartment = department.PK.ToGuid();
				Factory.Save();

				AssertNoExceptionThrown("Allow inactive departments by default", () => WebAppEnvironment.Setup());
				AssertExceptionThrown("Throw when inactive departments not allowed", typeof(InvalidWebEnvironmentException), () => WebAppEnvironment.Setup(new WebAppEnvironment.Config(activeDepartmentOnly: true)));

				DataRegistry.Instance.WebDepartment = initialValue;
			}
		}

		[HttpContextEnabledTest]
		public void TestSetupLanguage()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
				allowedLanguages.RemoveAll();
				var allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = SharedConstants.Languages.English;
				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

				HttpContext.Current.Request.GetType().GetField("_userLanguages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).SetValue(HttpContext.Current.Request, new string[] { "de", "XX-xx", "yo-NG", "fr-CA" });
				WebAppEnvironment.Setup();
				AssertEquals(Constants.Languages.EnglishAmerican, HttpContext.Current.Session["Language"]);
				AssertEquals(false, Env.Licence.LanguagePackLookup[SharedConstants.Languages.French].IsLoggedIn);
				AssertEquals(false, Env.Licence.LanguagePackLookup[SharedConstants.Languages.German].IsLoggedIn);

				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = SharedConstants.Languages.French;
				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);
				HttpContext.Current.Session.Clear();
				WebAppEnvironment.Setup();
				AssertEquals(SharedConstants.Languages.French, HttpContext.Current.Session["Language"]);
				AssertEquals(true, Env.Licence.LanguagePackLookup[SharedConstants.Languages.French].WebTrackerLanguageCheckpoint.IsLoggedIn);
				AssertEquals(false, Env.Licence.LanguagePackLookup[SharedConstants.Languages.German].WebTrackerLanguageCheckpoint.IsLoggedIn);

				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = SharedConstants.Languages.German;
				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);
				HttpContext.Current.Session.Clear();
				WebAppEnvironment.Setup();
				AssertEquals(SharedConstants.Languages.German, HttpContext.Current.Session["Language"]);
				AssertEquals(true, Env.Licence.LanguagePackLookup[SharedConstants.Languages.German].WebTrackerLanguageCheckpoint.IsLoggedIn);

				Env.Licence.LanguagePackLookup[SharedConstants.Languages.French].ForceLogout();
				HttpContext.Current.Request.Cookies.Add(new HttpCookie("Language", SharedConstants.Languages.French));
				WebAppEnvironment.Setup();
				AssertEquals(SharedConstants.Languages.French, HttpContext.Current.Session["Language"]);
				AssertEquals(true, Env.Licence.LanguagePackLookup[SharedConstants.Languages.French].WebTrackerLanguageCheckpoint.IsLoggedIn);

				AssertLanguageCookieWithOldLanguageCode("ENG");
				AssertLanguageCookieWithOldLanguageCode("AFK");
				AssertLanguageCookieWithOldLanguageCode("JPN");
			}
		}

		void AssertLanguageCookieWithOldLanguageCode(string languageCode)
		{
			HttpContext.Current.Request.Cookies.Remove("Language");
			HttpContext.Current.Session.Clear();
			HttpContext.Current.Request.Cookies.Add(new HttpCookie("Language", languageCode));
			WebAppEnvironment.Setup();
			AssertEquals(DataRegistry.Instance.EnglishSpelling, HttpContext.Current.Session["Language"]);
		}

		[HttpContextEnabledTest]
		public void TestSetupEnglishLanguage()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				HttpContext.Current.Request.GetType().GetField("_userLanguages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).SetValue(HttpContext.Current.Request, new string[] { "en-US" });
				HttpContext.Current.Session.Clear();
				WebAppEnvironment.Setup();
				AssertEquals(Constants.Languages.EnglishAmerican, HttpContext.Current.Session["Language"]);

				HttpContext.Current.Session.Clear();
				HttpContext.Current.Request.GetType().GetField("_userLanguages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).SetValue(HttpContext.Current.Request, new string[] { "en-GB" });
				HttpContext.Current.Session.Clear();
				WebAppEnvironment.Setup();
				AssertEquals(Constants.Languages.EnglishBritish, HttpContext.Current.Session["Language"]);

				HttpContext.Current.Session.Clear();
				HttpContext.Current.Request.GetType().GetField("_userLanguages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).SetValue(HttpContext.Current.Request, new string[] { "en-AU" });
				WebAppEnvironment.Setup();
				AssertEquals(Constants.Languages.EnglishBritish, HttpContext.Current.Session["Language"]);

				RawDataRegistry.Instance.EnglishSpelling.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Languages.EnglishBritish);
				HttpContext.Current.Session.Clear();
				HttpContext.Current.Request.GetType().GetField("_userLanguages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).SetValue(HttpContext.Current.Request, new string[] { "yo-NG" });
				WebAppEnvironment.Setup();
				AssertEquals(Constants.Languages.EnglishBritish, HttpContext.Current.Session["Language"]);

				RawDataRegistry.Instance.EnglishSpelling.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Languages.EnglishAmerican);
				HttpContext.Current.Session.Clear();
				HttpContext.Current.Request.GetType().GetField("_userLanguages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).SetValue(HttpContext.Current.Request, new string[] { "yo-NG" });
				WebAppEnvironment.Setup();
				AssertEquals(Constants.Languages.EnglishAmerican, HttpContext.Current.Session["Language"]);

				HttpContext.Current.Request.GetType().GetField("_userLanguages", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy).SetValue(HttpContext.Current.Request, new string[] { "en" });
				HttpContext.Current.Session.Clear();
				WebAppEnvironment.Setup();
				AssertEquals(Constants.Languages.English, HttpContext.Current.Session["Language"]);
			}
		}

		public void TestSetupEnvironmentWithoutWebUserLoaded()
		{
			var config = new WebAppEnvironment.Config(activeBranchOnly: true);
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				WebAppEnvironment.Setup(config);
				var user = Factory.LoadFromNaturalKey(ObjectFactory.GetType("IGlbStaff"), GlbStaffSchema.GS_LoginName, User.WebUserName);
				try
				{
					user[GlbStaffSchema.GS_LoginName] = "test";
					Factory.Save();

					AssertExceptionThrown(
						message: "Throw when no CWWebUser can be found in DataBase",
						expectedTypeOfException: typeof(InvalidWebEnvironmentException),
						codeToRun: () => WebAppEnvironment.Setup(config));
				}
				finally
				{
					user[GlbStaffSchema.GS_LoginName] = User.WebUserName;
					Factory.Save();
				}
			}
		}
#elif NET
		public void TestSetupBranding_WhenProductivityWiseModeChanges()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				DataRegistry.Instance.ProductivityWiseModeEnabled = true;
				WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext);
				AssertEquals("ProductivityWise", BrandingFactory.Instance.ProductName);

				DataRegistry.Instance.ProductivityWiseModeEnabled = false;
				WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext);
				AssertEquals("CargoWise", BrandingFactory.Instance.ProductName);
			}
		}

		public void TestSetup_WithNoDbConnection()
		{
			var thread = new Thread(() =>
			{
				WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext);
			});

			thread.Start();
			thread.Join();

			AssertEquals("No DbConnnection errors should have been reported", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestSetupEnvironment()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext);

				GlbBranch expBranch = Factory.LoadTop1<GlbBranch>(new ZQuery());
				GlbDepartment expDepartment = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "BRN");

				AssertEquals("Expected WebBranch to be returned", DataRegistry.Instance.WebBranch, GlbBranch.CurrentBranch.PK);
				AssertEquals("Expected default branch [BRN] to be returned", Enterprise.Environment.Env.CurrentDepartment.PK, expDepartment.PK);
			}
		}

		public void TestSetupEnvironmentWithInactiveBranches()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				var initialValue = DataRegistry.Instance.WebBranch;
				var branch = Factory.LoadTop1<GlbBranch>(new ZQuery());
				branch.GB_IsActive = false;
				DataRegistry.Instance.WebBranch = branch.PK.ToGuid();
				Factory.Save();

				AssertNoExceptionThrown("Allow inactive branches by default", () => WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext));
				AssertExceptionThrown("Throw when inactive branches not allowed", typeof(InvalidWebEnvironmentException), () => WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext, new WebAppEnvironment.Config(activeBranchOnly: true)));

				DataRegistry.Instance.WebBranch = initialValue;
			}
		}

		public void TestSetupEnvironmentWithInactiveDepartments()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				var initialValue = DataRegistry.Instance.WebDepartment;
				var department = Factory.LoadTop1<GlbDepartment>(new ZQuery());
				((ICancellable)department).IsCancelled = true;
				DataRegistry.Instance.WebDepartment = department.PK.ToGuid();
				Factory.Save();

				AssertNoExceptionThrown("Allow inactive departments by default", () => WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext));
				AssertExceptionThrown("Throw when inactive departments not allowed", typeof(InvalidWebEnvironmentException), () => WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext, new WebAppEnvironment.Config(activeDepartmentOnly: true)));

				DataRegistry.Instance.WebDepartment = initialValue;
			}
		}

		[HttpContextEnabledTest]
		public void TestSetupLanguage()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				var httpContext = HttpContextEnabledTestAttribute.HttpContext;
				var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
				allowedLanguages.RemoveAll();
				var allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = SharedConstants.Languages.English;
				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

				httpContext.Request.Headers["Accept-Language"] = "de,XX-xx,yo-NG,fr-CA";
				WebAppEnvironment.Setup(httpContext);
				AssertEquals(Constants.Languages.EnglishAmerican, httpContext.Session.GetString("Language"));
				AssertEquals(false, Env.Licence.LanguagePackLookup[SharedConstants.Languages.French].IsLoggedIn);
				AssertEquals(false, Env.Licence.LanguagePackLookup[SharedConstants.Languages.German].IsLoggedIn);

				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = SharedConstants.Languages.French;
				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);
				httpContext.Session.Clear();
				WebAppEnvironment.Setup(httpContext);
				AssertEquals(SharedConstants.Languages.French, httpContext.Session.GetString("Language"));
				AssertEquals(true, Env.Licence.LanguagePackLookup[SharedConstants.Languages.French].WebTrackerLanguageCheckpoint.IsLoggedIn);
				AssertEquals(false, Env.Licence.LanguagePackLookup[SharedConstants.Languages.German].WebTrackerLanguageCheckpoint.IsLoggedIn);

				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = SharedConstants.Languages.German;
				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);
				httpContext.Session.Clear();
				WebAppEnvironment.Setup(httpContext);
				AssertEquals(SharedConstants.Languages.German, httpContext.Session.GetString("Language"));
				AssertEquals(true, Env.Licence.LanguagePackLookup[SharedConstants.Languages.German].WebTrackerLanguageCheckpoint.IsLoggedIn);

				Env.Licence.LanguagePackLookup[SharedConstants.Languages.French].ForceLogout();
				SetRequestCookie(httpContext, "Language", SharedConstants.Languages.French);
				WebAppEnvironment.Setup(httpContext);
				AssertEquals(SharedConstants.Languages.French, httpContext.Session.GetString("Language"));
				AssertEquals(true, Env.Licence.LanguagePackLookup[SharedConstants.Languages.French].WebTrackerLanguageCheckpoint.IsLoggedIn);

				AssertLanguageCookieWithOldLanguageCode("ENG");
				AssertLanguageCookieWithOldLanguageCode("AFK");
				AssertLanguageCookieWithOldLanguageCode("JPN");
			}
		}

		void AssertLanguageCookieWithOldLanguageCode(string languageCode)
		{
			var httpContext = HttpContextEnabledTestAttribute.HttpContext;
			httpContext.Session.Clear();
			SetRequestCookie(httpContext, "Language", languageCode);
			WebAppEnvironment.Setup(httpContext);
			AssertEquals(DataRegistry.Instance.EnglishSpelling, httpContext.Session.GetString("Language"));
		}

		void SetRequestCookie(HttpContext context, string key, string value)
		{
			string encoded = Uri.EscapeDataString(value);
			context.Request.Headers["Cookie"] = $"{key}={encoded}";
		}

		[HttpContextEnabledTest]
		public void TestSetupEnglishLanguage()
		{
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				var httpContext = HttpContextEnabledTestAttribute.HttpContext;

				void RunLanguageTest(string acceptLanguage, string expectedLanguage)
				{
					httpContext.Request.Headers["Accept-Language"] = acceptLanguage;
					httpContext.Session.Clear();
					WebAppEnvironment.Setup(httpContext);
					AssertEquals(expectedLanguage, httpContext.Session.GetString("Language"));
				}

				RunLanguageTest("en-US", Constants.Languages.EnglishAmerican);
				RunLanguageTest("en-GB", Constants.Languages.EnglishBritish);
				RunLanguageTest("en-AU", Constants.Languages.EnglishBritish);

				RawDataRegistry.Instance.EnglishSpelling.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Languages.EnglishBritish);
				RunLanguageTest("yo-NG", Constants.Languages.EnglishBritish);

				RawDataRegistry.Instance.EnglishSpelling.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Constants.Languages.EnglishAmerican);
				RunLanguageTest("yo-NG", Constants.Languages.EnglishAmerican);

				RunLanguageTest("en", Constants.Languages.English);
			}
		}

		public void TestSetupEnvironmentWithoutWebUserLoaded()
		{
			var config = new WebAppEnvironment.Config(activeBranchOnly: true);
			using (Env.SetTemporaryUserContext(new UserContext("", Guid.Empty, Guid.Empty)))
			{
				WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext, config);
				var user = Factory.LoadFromNaturalKey(ObjectFactory.GetType("IGlbStaff"), GlbStaffSchema.GS_LoginName, User.WebUserName);
				try
				{
					user[GlbStaffSchema.GS_LoginName] = "test";
					Factory.Save();

					AssertExceptionThrown(
						message: "Throw when no CWWebUser can be found in DataBase",
						expectedTypeOfException: typeof(InvalidWebEnvironmentException),
						codeToRun: () => WebAppEnvironment.Setup(WebEnv.HttpContextAccessor.HttpContext, config));
				}
				finally
				{
					user[GlbStaffSchema.GS_LoginName] = User.WebUserName;
					Factory.Save();
				}
			}
		}
#endif

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		protected override void TearDown()
		{
			using (Env.Instance.SuppressSwitchContextCheck(ensureContextIsRestoredAfterSuppression: false))
			{
				Env.SetUserContext(initialUserContext);
			}
			base.TearDown();
#if NET
			WebEnv.HttpContextAccessor = null;
#endif
		}
	}
}
