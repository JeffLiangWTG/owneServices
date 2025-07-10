using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
#if NETFRAMEWORK
using System.Web;
#elif NET
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
#endif
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public static class WebAppEnvironment
	{
		public readonly struct Config : IWebConfig
		{
			public Config(bool activeBranchOnly = false, bool activeDepartmentOnly = false)
			{
				ActiveBranchOnly = activeBranchOnly;
				ActiveDepartmentOnly = activeDepartmentOnly;
			}

			public bool ActiveBranchOnly { get; }
			public bool ActiveDepartmentOnly { get; }
		}

		public static void Setup(IWebConfig config = null, HttpContext context = null)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (config == null)
				{
					config = new Config();
				}
				SetupBranding();
				SetupEnvironment(config.ActiveBranchOnly, config.ActiveDepartmentOnly);
#if NET
				SetupLanguage(context);
#elif NETFRAMEWORK
				SetupLanguage();
#endif
			}
		}

		static void SetupBranding()
		{
			if (DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				BrandingFactory.Configure(BrandingFactory.BrandingType.ProductivityWise);
			}
			else
			{
				if (CWNextFeatureHelper.IsCWNextEnabled())
				{
					BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseNext);
				}
				else
				{
					BrandingFactory.Configure(BrandingFactory.BrandingType.CargoWiseOne);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "A simple user context change is not applicable here . It needs a full environment replacement.")]
		static void SetupEnvironment(bool activeBranchOnly, bool activeDepartmentOnly)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var user = GetWebUserFromDatabase();
				Env.SetUserContext(new UserContext(user, WebBranchPk, WebDepartmentPk));
			}

			var branch = Env.CurrentBranch ?? throw new InvalidWebEnvironmentException("Invalid Branch, you need to provide a valid branch in the System Registry under Web > Web Branch");
			var company = Env.CurrentCompany ?? throw new InvalidWebEnvironmentException($"Unable to load company for branch '{branch.Code}'");
			var department = Env.CurrentDepartment ?? throw new InvalidWebEnvironmentException("Invalid Department, you need to provide a valid department in the System Registry under Web > Web Department");
			var userContext = Env.CurrentUserContext ?? throw new InvalidWebEnvironmentException("Unable to load Web user");

			if (activeBranchOnly && !branch.IsActive)
			{
				throw new InvalidWebEnvironmentException($"Inactive Branch '{branch.Code}', you need to provide an active branch in the System Registry under Web > Web Branch");
			}

			if (activeDepartmentOnly && department is ICancellable cancellable && cancellable.IsCancelled)
			{
				throw new InvalidWebEnvironmentException($"Inactive Department '{department.Code}', you need to provide an active department in the System Registry under Web > Web Department");
			}
		}

		static string WebUserName => User.WebUserName;

		static Guid WebBranchPk => DataRegistry.WebBranch;

		static Guid WebDepartmentPk => DataRegistry.WebDepartment;

		static DataRegistry DataRegistry => DataRegistry.Instance ?? throw new InvalidWebEnvironmentException("Data registry wasn't found.");

#if NET
		public static void Setup(HttpContext context, IWebConfig config = null)
		{
			Setup(config, context);
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Cookie name should not be translated")]
		public static void SetupLanguage(HttpContext httpContext)
		{
			if (httpContext != null && httpContext.Request != null)
			{
				var languageCookie = httpContext.Request.Cookies["Language"];
				if (!string.IsNullOrEmpty(languageCookie))
				{
					if (Culture.LanguageCodeMapping.ContainsKey(languageCookie))
					{
						SetupLanguage(DataRegistry.Instance.EnglishSpelling, httpContext);
						httpContext.Response.Cookies.Append("Language", "", new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddDays(-1) });
						return;
					}

					if (SetupLanguage(languageCookie, httpContext))
					{
						return;
					}

					httpContext.Response.Cookies.Append("Language", "", new CookieOptions() { Expires = DateTimeOffset.UtcNow.AddDays(-1) });
				}

				var userLanguages = httpContext.Request.Headers["Accept-Language"].ToString().Split(',');
				if (userLanguages != null)
				{
					for (int i = 0; i < userLanguages.Length; i++)
					{
						try
						{
							var itemCulture = new CultureInfo(userLanguages[i]);
							var itemLanguage = Culture.GetLanguageForCulture(itemCulture);
							if (!string.IsNullOrEmpty(itemLanguage))
							{
								if (SetupLanguage(itemLanguage, httpContext))
								{
									return;
								}
							}
						}
						catch (CultureNotFoundException)
						{ }
					}
				}
				SetupLanguage(DataRegistry.Instance.EnglishSpelling, httpContext);
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "<Pending>")]
		public static bool SetupLanguage(string language, HttpContext httpContext)
		{
			if (!IsSessionAvailable(httpContext))
			{
				return false;
			}

			if (Res.IsEnglish(language))
			{
				httpContext.Session.SetString("Language", language);
				return true;
			}

			foreach (CodeSelection allowed in WebDataRegistry.Instance.AllowedLanguages.Value)
			{
				if (allowed.Code == language)
				{
					string licencedLanguage = language;
					Env.Licence.UseLanguageLicense(ref licencedLanguage, LanguageUsageType.WebTracker);
					if (licencedLanguage.Equals(language, StringComparison.Ordinal))
					{
						httpContext.Session.SetString("Language", language);
						return true;
					}
				}
			}

			return false;
		}

		public static bool IsSessionAvailable(this HttpContext context)
		{
			return context?.Features.Get<ISessionFeature>()?.Session != null && context?.Session != null;
		}
#elif NETFRAMEWORK
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Cookie name should not be translated")]
		public static void SetupLanguage()
		{
			if (HttpContext.Current != null && HttpContext.Current.Request != null)
			{
				var languageCookie = HttpContext.Current.Request.Cookies["Language"];
				if (languageCookie != null && !string.IsNullOrEmpty(languageCookie.Value))
				{
					if (Culture.LanguageCodeMapping.ContainsKey(languageCookie.Value))
					{
						SetupLanguage(DataRegistry.Instance.EnglishSpelling);
						HttpContext.Current.Response.SetCookie(new HttpCookie("Language") { Expires = DateTime.UtcNow.AddDays(-1D) });
						return;
					}

					if (SetupLanguage(languageCookie.Value))
					{
						return;
					}

					HttpContext.Current.Response.SetCookie(new HttpCookie("Language") { Expires = DateTime.UtcNow.AddDays(-1D) });
				}

				if (HttpContext.Current.Request.UserLanguages != null)
				{
					for (int i = 0; i < HttpContext.Current.Request.UserLanguages.Length; i++)
					{
						try
						{
							var itemCulture = new CultureInfo(HttpContext.Current.Request.UserLanguages[i]);
							var itemLanguage = Culture.GetLanguageForCulture(itemCulture);
							if (!string.IsNullOrEmpty(itemLanguage))
							{
								if (SetupLanguage(itemLanguage))
								{
									return;
								}
							}
						}
						catch (CultureNotFoundException)
						{ }
					}
				}
				SetupLanguage(DataRegistry.Instance.EnglishSpelling);
			}
		}

		public static bool SetupLanguage(string language)
		{
			if (HttpContext.Current.Session != null)
			{
				if (Res.IsEnglish(language))
				{
					HttpContext.Current.Session["Language"] = language;
					return true;
				}

				foreach (CodeSelection allowed in WebDataRegistry.Instance.AllowedLanguages.Value)
				{
					if (allowed.Code == language)
					{
						string licencedLanguage = language;
						Env.Licence.UseLanguageLicense(ref licencedLanguage, LanguageUsageType.WebTracker);
						if (licencedLanguage.Equals(language,StringComparison.Ordinal))
						{
							HttpContext.Current.Session["Language"] = language;
							return true;
						}
					}
				}
			}

			return false;
		}
#endif

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "It's an error string., A simple user context change is not applicable here . It needs a full environment replacement.")]
		public static GlbStaff GetWebUserFromDatabase()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = false };
			factory.SuspendValidation();

			var type = ObjectFactory.GetType("IGlbStaff");
			var column = GlbStaffSchema.GS_LoginName;
			var userName = WebUserName;
			var user = (GlbStaff)factory.LoadFromNaturalKey(type, column, userName);
			if (user == null)
			{
				var query = new ZQuery(column, userName);
				query.ReLoadExistingRows = true;
				query.IgnoreDbQueryCache = true;
				user = (GlbStaff)factory.LoadTop1(type, query);
				if (user == null)
				{
					var rowFactory = ((IBusinessObjectFactoryInternals)factory).RowFactory;
					var table = rowFactory.GetTable(GlbStaffSchema.Constants.TableName, false);
					var has = new Func<bool, string>(f => f ? "Yes" : "No");
					var connection = Db.Connection ?? throw new InvalidWebEnvironmentException("No connection to the database found.");

					throw new InvalidWebEnvironmentException($@"Unable to load user {user} for type {type} and column {column.Name} from database.
	Additional information:
	Has Table: {has(table != null)}
	Current Db: {connection.CurrentDatabase}
	Throwing?: {has(connection.DatabaseUpgradedExceptionHasBeenThrown)}
	Impersonated: {connection.ImpersonatedLogin}");
				}
				else
				{
					ErrorReporter.ReportOnce("Loaded GlbStaff row correctly on retry. Uber factory is corrupt somehow?");
				}
			}
			return user;
		}
	}
}
