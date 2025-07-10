using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

#if DEBUG
using CargoWise.Common.Testing;
#endif

namespace Enterprise.ZArchitecture.Web.GUI
{
	#region SuppressResourceStringsCheckRegion

	/// <summary>
	/// Global settings check - licence, DB connection, etc.
	/// </summary>
	public class ZGlobalConfig
	{
		public ZGlobalConfig()
		{
			ValidationErrors.Add(BranchKey, "A branch is not defined in the registry, see Registry > Web > Web Branch.");
			ValidationErrors.Add(HomePageKey, "A home page is not defined for the branch's company record. Please edit the web address using " + BrandingFactory.Instance.ProductName + " > Maintain > User Admin > Companies > search for your company record > Company Info tab > Web Address\r\nSee the update note for more details: <a href =\"https://wisetechacademy.com/search?quickstart=c9ac1391-baa0-4332-af0a-cbb58c8043b0\">Update Note</a>");
			ValidationErrors.Add(CompanyNameKey, "A company name is not defined for the branch's company record, see the branch's company record > Company Info tab > Name.");
		}

		#region ConfigurationOK

		/// <summary>
		/// Determines if necessary configuration options are set.
		/// </summary>

		public bool ConfigurationOK
		{
			get
			{
				if (!fConfigurationOK)
				{
					fConfigurationOK = true;
					ResetErrors();
					EnsureConfigurationItemData();
					var keys = new ArrayList(ValidationErrors.Keys);
					keys.Sort();
					foreach (string key in keys)
					{
						var item = GetConfigurationItem(key);
						if (item.IsEmpty)
						{
							AddError((string)ValidationErrors[key]);
							fConfigurationOK = false;
						}
					}
				}
				return fConfigurationOK;
			}
		}
		bool fConfigurationOK;

		/// <summary>
		/// Contains all errors 
		/// </summary>
		internal Hashtable ValidationErrors = new Hashtable();

		#endregion

		#region ConfigurationAndLicenceOK

		public bool ConfigurationAndLicenceOK
		{
			get
			{
				if (!fConfigurationAndLicenceOK)
				{
					if (ConfigurationOK)
					{
						OnCheckConfiguration();
						fConfigurationAndLicenceOK = !HasErrors;
					}
				}
				return fConfigurationAndLicenceOK;
			}
		}
		bool fConfigurationAndLicenceOK;

		/// <summary>
		/// Implement your custom configuration check here
		/// </summary>
		protected virtual void OnCheckConfiguration()
		{
		}

		#endregion

		#region Configuration errors

		/// <summary>
		/// The configuration error from the last test
		/// </summary>
		public string ConfigurationError
		{
			get
			{
				var result = new StringBuilder();
				foreach (var str in fConfigurationErrors)
				{
					result.AppendLine(str);
				}
				result.AppendLine();
				result.AppendLine("Current settings:");
				var productKey = ObjectFactory.Get<IProductRegistration>().Key;
				if (productKey != null)
				{
					result.AppendLine(String.Format("EnterpriseCode: {0}", productKey.EnterpriseCode));
					result.AppendLine(String.Format("ServerCode: {0}", productKey.ServerCode));
				}
				result.AppendLine(String.Format("Branch: {0}", Branch));
				result.AppendLine(String.Format("Home page: {0}", HomePage));
				result.AppendLine(String.Format("Company name: {0}", CompanyName));
				return result.ToString();
			}
		}
		internal StringCollection fConfigurationErrors = new StringCollection();

		protected void AddError(string errorMessage)
		{
			if (!fConfigurationErrors.Contains(errorMessage))
			{
				fConfigurationErrors.Add(errorMessage);
			}
		}

		protected bool HasErrors
		{
			get { return fConfigurationErrors.Count > 0; }
		}

		protected internal void Reset()
		{
			ResetErrors();
			fConfigurationOK = false;
			fConfigurationAndLicenceOK = false;
		}

		void ResetErrors()
		{
			fConfigurationErrors.Clear();
		}

		#endregion

		#region Licence checks

		/// <summary>
		/// Licence checkpoint for a particular web project
		/// </summary>
		/// <param name="licences">set of licences for a particular company</param>
		/// <returns>Licence checkpoint for a particular web project for a particular company</returns>
		protected virtual LicenceCheckpoint GetLicenceCheckPoint(Licences licences)
		{
			return null;
		}

		#endregion

		#region Configuration items

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings")]
		protected internal string WebSiteUrlKey
		{
			get { return webSiteUrlKey; }
			set { webSiteUrlKey = value; }
		}
		string webSiteUrlKey;

		internal void EnsureConfigurationItemData()
		{
#if DEBUG
			if (!IsConfigurationItemsForTestingUsed)
#endif
			{
				configurationItemInfo.EnsureConfigured(true);
			}
		}

#if DEBUG
		bool IsConfigurationItemsForTestingUsed
		{
			get { return Globals.IsTest && ConfigurationItemsForTesting != null; }
		}
#endif

		/// <summary>
		/// Gets value from Web.Config's AppSettings section
		/// </summary>
		/// <param name="Name">Parameter name in AppSettings</param>
		/// <returns>value in the corresponding App Settings section</returns>
		protected ZString GetConfigurationItem(string name)
		{
#if DEBUG
			if (IsConfigurationItemsForTestingUsed)
			{
				return new ZString(ConfigurationItemsForTesting[name]);
			}
			else
#endif
			{
				switch (name)
				{
					case BranchKey:
						return configurationItemInfo.Code;
					case HomePageKey:
						return configurationItemInfo.WebAddress;
					case CompanyNameKey:
						return configurationItemInfo.Name;
					default:
						return new ZString(ConfigurationManager.AppSettings[name]);
				}
			}
		}

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static readonly ConfigurationItemInfo configurationItemInfo = new ConfigurationItemInfo();

#if DEBUG
		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		internal static ManualResetEvent Testing_BlockingEvent;
#endif

		class ConfigurationItemInfo
		{
			internal void EnsureConfigured(bool forceReload = false)
			{
				lock (syncLock)
				{
#if DEBUG
					Testing_BlockingEvent?.WaitOne();
#endif
					ReloadConfigurationIfInvalid(forceReload);
				}
			}

			void ReloadConfigurationIfInvalid(bool forceReload = false)
			{
				if (forceReload || branchId != Env.Registry.WebBranch)
				{
					code = string.Empty;
					name = string.Empty;
					webAddress = string.Empty;

					using (Db.DisposableActionForDbConnection())
					{
						var factory = new BusinessObjectFactory();
						var branch = factory.Load<GlbBranch>(Env.Registry.WebBranch);

						if (branch != null)
						{
							branch.Reload();
							code = branch.GB_Code;

							var company = branch.Company;
							if (company != null)
							{
								company.Reload();
								webAddress = company.GC_WebAddress;
								name = company.CompanyName;
								branchId = branch.PK.ToGuid();
							}
						}
					}
				}
			}

			readonly object syncLock = new object();

			internal string Code
			{
				get
				{
					lock (syncLock)
					{
						ReloadConfigurationIfInvalid();

						return code;
					}
				}
			}
			internal string WebAddress
			{
				get
				{
					lock (syncLock)
					{
						ReloadConfigurationIfInvalid();

						return webAddress;
					}
				}
			}
			internal string Name
			{
				get
				{
					lock (syncLock)
					{
						ReloadConfigurationIfInvalid();

						return name;
					}
				}
			}

			string code;
			string webAddress;
			string name;
			Guid branchId;
		}

#if DEBUG
		[SuppressWeaklyTypedCollectionMessage]
		public Hashtable ConfigurationItemsForTesting = new Hashtable();
#endif

		public const string BranchKey = "Branch";
		public ZString Branch
		{
			get { return GetConfigurationItem(BranchKey); }
		}

		ReadOnlyBusinessObjectFactory readOnlyFactory;
		internal ReadOnlyBusinessObjectFactory ReadOnlyFactory
		{
			get
			{
				if (readOnlyFactory == null)
				{
					readOnlyFactory = new ReadOnlyBusinessObjectFactory();
					((IBusinessObjectFactoryInternals)readOnlyFactory).IncludeWithOtherFactoriesForIssueReport = false;
				}
				return readOnlyFactory;
			}
		}

		protected StmDataCollection stmDataList;
		StmDataCollection StmDataList
		{
			get
			{
				if (stmDataList == null)
				{
					ZQuery filter = new ZQuery(StmDataSchema.SD_Name, WebSiteUrlKey);
					stmDataList = new StmDataCollection(ReadOnlyFactory, filter);
				}
				return stmDataList;
			}
		}

		GlbCompany RelatedCompany
		{
			get
			{
				if (HttpContext.Current != null)
				{
					var serverName = HttpContext.Current.Request.Url.Host.ToLower(CultureInfo.CurrentCulture);
					var matchedOnes = StmDataList.Where(item => Encoding.Unicode.GetString(item.SD_BinaryValue).ToLower(CultureInfo.CurrentCulture).Contains(serverName));
					if (matchedOnes.Count() == 1)
					{
						var uniqueStmData = matchedOnes.Single();
						return ReadOnlyFactory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.Equal, uniqueStmData.SD_Owner));
					}
					else
					{
						return null;
					}
				}
				return null;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings")]
		public string GetCompanyNavigateUrlAndName(string name)
		{
			if (RelatedCompany != null)
			{
				switch (name)
				{
					case HomePageKey:
						var companyNavigateUrl = RelatedCompany.GC_WebAddress;
						if (!companyNavigateUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !companyNavigateUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && !String.IsNullOrEmpty(companyNavigateUrl))
						{
							companyNavigateUrl = "http://" + companyNavigateUrl;
						}
						return companyNavigateUrl;
					case CompanyNameKey:
						return RelatedCompany.GC_Name;
				}
			}

			return string.Empty;
		}

		protected internal const string HomePageKey = "HomePageURL";
		public ZString HomePage
		{
			get
			{
				var companyNavigateUrl = string.Empty;
				if (!string.IsNullOrEmpty(WebSiteUrlKey))
				{
					companyNavigateUrl = GetCompanyNavigateUrlAndName(HomePageKey);
				}

				if (String.IsNullOrEmpty(companyNavigateUrl))
				{
					companyNavigateUrl = GetConfigurationItem(HomePageKey);
					if (!companyNavigateUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !companyNavigateUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) && !String.IsNullOrEmpty(companyNavigateUrl))
					{
						companyNavigateUrl = "http://" + companyNavigateUrl;
					}
				}
				return companyNavigateUrl;
			}
		}

		protected internal const string CompanyNameKey = "CompanyName";
		public ZString CompanyName
		{
			get
			{
				var companyHint = string.Empty;
				if (!string.IsNullOrEmpty(WebSiteUrlKey))
				{
					companyHint = GetCompanyNavigateUrlAndName(CompanyNameKey);
				}
				if (String.IsNullOrEmpty(companyHint))
				{
					companyHint = GetConfigurationItem(CompanyNameKey);
				}
				return companyHint;
			}
		}

#if DEBUG
		public const string ClientKey = "Client";
		public ZString Client
		{
			get { return GetConfigurationItem(ClientKey); }
		}
#endif

		#endregion
	}

	#endregion
}
