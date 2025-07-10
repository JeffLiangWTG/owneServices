using System;
using System.IO;
using System.Threading;
using System.Web;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Testing
{
	public class ZGlobalConfigTest : TestCaseWithFactory
	{
		#region Setup 

		protected override BusinessObjectFactory NewFactory()
		{
			return new BusinessObjectFactory();
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestConfig = GetNewConfig();
		}

		protected virtual ZGlobalConfig GetNewConfig()
		{
			return new ZGlobalConfig();
		}

		protected ZGlobalConfig TestConfig;

		#endregion

		IDisposable SetupThreadSafeTesting()
		{
			TestConfig.ConfigurationItemsForTesting = null;
			var cachedBranch = Env.Registry.WebBranch;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BRA";
			Factory.Save();

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "Company Co.";
			company.GC_WebAddress = "company.net";
			branch.GB_GC = company.PK;
			Env.Registry.WebBranch = branch.PK.ToGuid();
			Factory.Save();

			var factory = new BusinessObjectFactory();
			branch = factory.Load<GlbBranch>(Env.Registry.WebBranch);
			AssertNotNull(branch);
			AssertNotNull(branch.Company);

			return new DisposableAction(delegate
			{
				Env.Registry.WebBranch = cachedBranch;
				Factory.Save();
			});
		}

		public void TestIncludeWithOtherFactoriesBooleanIsSetProperly()
		{
			var config = new ZGlobalConfig();
			AssertEquals("Is false when first retrieved", false, ((IBusinessObjectFactoryInternals)config.ReadOnlyFactory).IncludeWithOtherFactoriesForIssueReport);
			AssertEquals("Is still false on subsequent retrievals", false, ((IBusinessObjectFactoryInternals)config.ReadOnlyFactory).IncludeWithOtherFactoriesForIssueReport);
		}

		[ExpectNoExceptions]
		public void TestConfigurationBranchIsThreadSafe()
		{
			TestConfigurationPropertyIsThreadSafe(() => TestConfig.Branch, "BRA");
		}

		[ExpectNoExceptions]
		public void TestConfigurationCompanyNameIsThreadSafe()
		{
			TestConfigurationPropertyIsThreadSafe(() => TestConfig.CompanyName, "Company Co.");
		}

		[ExpectNoExceptions]
		public void TestConfigurationHomePageIsThreadSafe()
		{
			TestConfigurationPropertyIsThreadSafe(() => TestConfig.HomePage, "http://company.net");
		}

		void TestConfigurationPropertyIsThreadSafe(Func<ZString> getValue, string expectedValue)
		{
			using (SetupThreadSafeTesting())
			{
				var isReadingThreadBlocked = false;
				var propertyValue = "N/A";
				Exception exceptionThrownOnThreads = null;
				var reloadingThreadHasStarted = false;
				var readingThreadHasStarted = false;

				ZGlobalConfig.Testing_BlockingEvent = new ManualResetEvent(false);
				var reloadingThread = new Thread(new ThreadStart(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							reloadingThreadHasStarted = true;
							TestConfig.EnsureConfigurationItemData();
						}
					}
					catch (Exception ex)
					{
						exceptionThrownOnThreads = ex;
					}
				}));

				var readingThread = new Thread(() =>
				{
					try
					{
						using (Db.DisposableActionForDbConnection())
						{
							readingThreadHasStarted = true;
							propertyValue = getValue();
						}
					}
					catch (Exception ex)
					{
						exceptionThrownOnThreads = ex;
					}
				});

				reloadingThread.Start();
				while (!reloadingThreadHasStarted && !reloadingThread.Join(TimeSpan.FromSeconds(0.1)))
				{ }

				readingThread.Start();
				while (!readingThreadHasStarted && !reloadingThread.Join(TimeSpan.FromSeconds(0.1)))
				{ }
				readingThread.Join(TimeSpan.FromSeconds(2));
				isReadingThreadBlocked = propertyValue == "N/A";

				ZGlobalConfig.Testing_BlockingEvent.Set();

				reloadingThread.Join(TimeSpan.FromSeconds(2));
				readingThread.Join(TimeSpan.FromSeconds(2));

				if (exceptionThrownOnThreads != null)
				{
					throw exceptionThrownOnThreads;
				}

				Assert("Thread lock is missing. Configuration property thread should be blocked by configuration reloading thread", isReadingThreadBlocked);
				Assert("Thread locking is not working properly. Configuration property thread is locked.", propertyValue != "N/A");
				AssertEquals("Should have the correct value", expectedValue, propertyValue);
			}
		}

		#region ConfigurationOK Tests

		public void TestConfigurationOKBranchCaching()
		{
			TestConfig.ConfigurationItemsForTesting = null;

			var originalCompany = Factory.NewWithValidTestData<GlbCompany>();
			originalCompany.GC_Name = "Original Company";

			var newCompany = Factory.NewWithValidTestData<GlbCompany>();
			newCompany.GC_Name = "New Company";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BRH";
			branch.GB_GC = originalCompany.PK;

			Factory.Save();
			Env.Registry.WebBranch = branch.PK.ToGuid();
			AssertEquals("Invalid configuration expected", false, TestConfig.ConfigurationOK);
			AssertGreaterThan("Should have errors", TestConfig.fConfigurationErrors.Count, 0);
			AssertEquals("Invalid configuration expected (repeat check)", false, TestConfig.ConfigurationOK);

			var remoteBranchUpdateSql = string.Format("Update {0} Set {1}='{2}', GB_SystemLastEditTimeUtc = GetUtcDate(), GB_SystemLastEditUser = 'E' Where {3}='{4}'",
				GlbBranch.Schema.TableName,
				GlbBranch.Schema.GB_GC,
				newCompany.PK,
				GlbBranch.Schema.PK,
				branch.PK);
			Db.Connection.ExecuteNonQuery(remoteBranchUpdateSql);
			AssertEquals("Invalid configuration expected", false, TestConfig.ConfigurationOK);
			AssertGreaterThan("Should have errors", TestConfig.fConfigurationErrors.Count, 0);

			var remoteCompanyUpdateSql = string.Format("Update {0} Set {1}='{2}', GC_SystemLastEditTimeUtc = GetUtcDate(), GC_SystemLastEditUser = 'E' Where {3}='{4}'",
				GlbCompany.Schema.TableName,
				GlbCompany.Schema.GC_WebAddress,
				"www.test.com",
				GlbCompany.Schema.PK,
				newCompany.PK);
			Db.Connection.ExecuteNonQuery(remoteCompanyUpdateSql);
			AssertEquals("Valid configuration expected", true, TestConfig.ConfigurationOK);
			AssertEquals("Should not have errors", 0, TestConfig.fConfigurationErrors.Count);

			originalCompany.GC_WebAddress = string.Empty;
			Factory.Save();
			AssertEquals("Valid configuration expected (cached!)", true, TestConfig.ConfigurationOK);
			AssertEquals("Should not have errors", 0, TestConfig.fConfigurationErrors.Count);
		}

		public void TestConfigurationOKAndCaching()
		{
			AssertEquals("Initially config is not OK - no settings specified", false, TestConfig.ConfigurationOK);
			AssertEquals("Should have 3 errors", 3, TestConfig.fConfigurationErrors.Count);
			AssertEquals("Config should be not OK - checking caching", false, TestConfig.ConfigurationOK);

			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.BranchKey, "ABCD");
			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.HomePageKey, "Url");
			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.CompanyNameKey, "Co");

			AssertEquals("Config should be OK - no caching of invalid configuration results", true, TestConfig.ConfigurationOK);

			TestConfig.ConfigurationItemsForTesting.Remove(ZGlobalConfig.HomePageKey);
			TestConfig.ConfigurationItemsForTesting.Remove(ZGlobalConfig.CompanyNameKey);

			AssertEquals("Config should be OK - using cached results", true, TestConfig.ConfigurationOK);
		}

		public void TestConfigurationOK()
		{
			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.BranchKey, "ABCD");
			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.HomePageKey, "Url");
			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.CompanyNameKey, "Co");

			AssertEquals("Config should be correct", true, TestConfig.ConfigurationOK);
			AssertEquals("Should have no errors", 0, TestConfig.fConfigurationErrors.Count);
		}

		public void TestMultiThreadedPropertyAccess()
		{
			string homepage = string.Empty;
			string companyName = string.Empty;
			string branch = string.Empty;

			ZGlobalConfig config = GetNewConfig();
			config.ConfigurationItemsForTesting = null;

			branch = config.Branch;
			homepage = config.HomePage;
			companyName = config.CompanyName;

			Thread anotherThread = new Thread(
					() =>
					{
						using (Db.DisposableActionForDbConnection())
						{
							branch = config.Branch;
							homepage = config.HomePage;
							companyName = config.CompanyName;
						}
					}
				);

			anotherThread.Start();
			anotherThread.Join();

			AssertEquals("Other thread access property of ZGlobalConfig should not report error", string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestConfigurationAndLicenceOK()
		{
			AssertEquals("Initially config is not OK - no settings specified", false, TestConfig.ConfigurationAndLicenceOK);
			AssertEquals("Should have 3 errors", 3, TestConfig.fConfigurationErrors.Count);
			AssertEquals("Initially config is not OK - no settings specified", false, TestConfig.ConfigurationAndLicenceOK);

			TestConfig.Reset();

			GlbBranch branch = Factory.LoadTop1<GlbBranch>(new ZQuery());

			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.BranchKey, branch.GB_Code);
			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.HomePageKey, "Url");
			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.CompanyNameKey, "Co");

			AssertEquals("Config and license should be OK - no caching of invalid configuration results", true, TestConfig.ConfigurationAndLicenceOK);

			TestConfig.ConfigurationItemsForTesting.Remove(ZGlobalConfig.BranchKey);
			TestConfig.ConfigurationItemsForTesting.Remove(ZGlobalConfig.HomePageKey);
			TestConfig.ConfigurationItemsForTesting.Remove(ZGlobalConfig.CompanyNameKey);

			AssertEquals("Config and license should be OK - using cached results", true, TestConfig.ConfigurationAndLicenceOK);
		}

		public void TestConfigurationError()
		{
			ObjectFactory.Get<IProductRegistration>().KeyForTest.EnterpriseCodeForTest = "CTF";
			ObjectFactory.Get<IProductRegistration>().KeyForTest.ServerCodeForTest = "VAR";
			AssertEquals("Initially config is not OK - no settings specified", false, TestConfig.ConfigurationOK);
			AssertEquals("Should have 3 errors", 3, TestConfig.fConfigurationErrors.Count);
			AssertEquals(@"A branch is not defined in the registry, see Registry > Web > Web Branch.
A company name is not defined for the branch's company record, see the branch's company record > Company Info tab > Name.
A home page is not defined for the branch's company record. Please edit the web address using CargoWise > Maintain > User Admin > Companies > search for your company record > Company Info tab > Web Address
See the update note for more details: <a href =""https://wisetechacademy.com/search?quickstart=c9ac1391-baa0-4332-af0a-cbb58c8043b0"">Update Note</a>

Current settings:
EnterpriseCode: CTF
ServerCode: VAR
Branch: 
Home page: 
Company name: 
", TestConfig.ConfigurationError);
		}

		#endregion

		#region Items in Web.Config

		public void TestBranch()
		{
			AssertEquals("Initially Branch is not set", ZString.Empty, TestConfig.Branch);
			AssertEquals("should have errors", false, TestConfig.ConfigurationOK);
			Assert("Should have Branch error", TestConfig.fConfigurationErrors.Contains((string)TestConfig.ValidationErrors[ZGlobalConfig.BranchKey]));

			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.BranchKey, "BRA");
			TestConfig.Reset();

			AssertEquals("Branch is set", "BRA", TestConfig.Branch);
			AssertEquals("should have errors", false, TestConfig.ConfigurationOK);
			Assert("Should NOT have Branch error", !TestConfig.fConfigurationErrors.Contains((string)TestConfig.ValidationErrors[ZGlobalConfig.BranchKey]));
		}

		public void TestHomePage()
		{
			AssertEquals("Initially HomePageUrl is not set", ZString.Empty, TestConfig.HomePage);
			AssertEquals("should have errors", false, TestConfig.ConfigurationOK);
			Assert("Should have HomePage URL error", TestConfig.fConfigurationErrors.Contains((string)TestConfig.ValidationErrors[ZGlobalConfig.HomePageKey]));

			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.HomePageKey, "web.com");
			TestConfig.Reset();
			AssertEquals("Server name is set, and includes prefix to make it absolute", "http://web.com", TestConfig.HomePage);

			TestConfig.ConfigurationItemsForTesting.Remove(ZGlobalConfig.HomePageKey);
			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.HomePageKey, "http://web2.com");
			TestConfig.Reset();
			AssertEquals("Server name is set correctly", "http://web2.com", TestConfig.HomePage);

			AssertEquals("should have errors", false, TestConfig.ConfigurationOK);
			Assert("Should NOT have HomePage URL error", !TestConfig.fConfigurationErrors.Contains((string)TestConfig.ValidationErrors[ZGlobalConfig.HomePageKey]));
		}

		public void TestHomePageForSettingWebTrackerRegistryWhenAllCompaniesHaveSameURL()
		{
			TestConfig.ConfigurationItemsForTesting = null;

			TestConfig.WebSiteUrlKey = "WebTrackerUrl";
			var stringWriter = new StringWriter();

			var glbCompany1 = Factory.NewWithValidTestData<GlbCompany>();
			var glbCompany2 = Factory.NewWithValidTestData<GlbCompany>();

			glbCompany1.GC_WebAddress = "Company1.com.au";
			glbCompany1.GC_Name = "Company1";

			glbCompany2.GC_WebAddress = "Company2.com.au";
			glbCompany2.GC_Name = "Company2";

			Factory.Save();

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_Code = "BRC";
			branch1.GB_GC = glbCompany1.PK;

			Factory.Save();

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_Code = "BRD";
			branch2.GB_GC = glbCompany2.PK;

			Factory.Save();

			Env.Registry.WebBranch = branch1.PK.ToGuid();
			Factory.Save();

			WebDataRegistry.Instance.WebTrackerUrl.SetValue(glbCompany1.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://test.com/Tracking/");
			WebDataRegistry.Instance.WebTrackerUrl.SetValue(glbCompany2.PK.ToGuid(), Guid.Empty, Guid.Empty, "http://test.com/Tracking/");

			Factory.Save();

			var testContextCompany1 = new HttpContext(new HttpRequest(string.Empty, "http://test.com", string.Empty), new HttpResponse(new StringWriter()));
			HttpContext.Current = testContextCompany1;
			AssertEquals("HomePageUrl is set", "http://Company1.com.au", TestConfig.HomePage);

			var testContextCompany2 = new HttpContext(new HttpRequest(string.Empty, "http://test.com", string.Empty), new HttpResponse(new StringWriter()));
			HttpContext.Current = testContextCompany2;
			AssertEquals("HomePageUrl is set", "http://Company1.com.au", TestConfig.HomePage);
		}

		public void TestHomePageForSettingWebTrackerRegistry()
		{
			AssertHomePageForSettingWebUrlRegistry("WebTrackerUrl", WebDataRegistry.Instance.WebTrackerUrl, "http://test.com/Tracking/", "http://test2.com/Tracking/");
		}

		public void TestHomePageForSettingWebCFSRegistry()
		{
			AssertHomePageForSettingWebUrlRegistry("WebCFSUrl", WebDataRegistry.Instance.WebCFSUrl, "http://test.com/WebCFS/", "http://test2.com/WebCFS/");
		}

		public void TestHomePageForSettingWebCampaignRegistry()
		{
			AssertHomePageForSettingWebUrlRegistry("WebCampaignUrl", WebDataRegistry.Instance.WebCampaignUrl, "http://test.com/WebCampaign/", "http://test2.com/WebCampaign/");
		}

		void AssertHomePageForSettingWebUrlRegistry(string websiteUrlKey, StringRegistryItem registryItem, string company1RegistryValue, string company2RegistryValue)
		{
			TestConfig.WebSiteUrlKey = websiteUrlKey;

			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			company1.GC_WebAddress = "Company1.com.au";
			company1.GC_Name = "Company1";

			company2.GC_WebAddress = "Company2.com.au";
			company2.GC_Name = "Company2";

			registryItem.SetValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, company1RegistryValue);
			registryItem.SetValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, company2RegistryValue);

			Factory.Save();

			HttpContext.Current = new HttpContext(new HttpRequest(string.Empty, company1RegistryValue, string.Empty), new HttpResponse(new StringWriter()));
			AssertEquals("HomePageUrl is set", "http://Company1.com.au", TestConfig.HomePage);

			HttpContext.Current = new HttpContext(new HttpRequest(string.Empty, company2RegistryValue, string.Empty), new HttpResponse(new StringWriter()));
			AssertEquals("HomePageUrl is set", "http://Company2.com.au", TestConfig.HomePage);
		}

		public void TestCompanyName()
		{
			AssertEquals("Initially company name is not set", ZString.Empty, TestConfig.CompanyName);
			AssertEquals("should have errors", false, TestConfig.ConfigurationOK);
			Assert("Should have company name error", TestConfig.fConfigurationErrors.Contains((string)TestConfig.ValidationErrors[ZGlobalConfig.CompanyNameKey]));

			TestConfig.ConfigurationItemsForTesting.Add(ZGlobalConfig.CompanyNameKey, "Company Co.");
			TestConfig.Reset();

			AssertEquals("Server name is set", "Company Co.", TestConfig.CompanyName);
			AssertEquals("should have errors", false, TestConfig.ConfigurationOK);
			Assert("Should NOT have company name error", !TestConfig.fConfigurationErrors.Contains((string)TestConfig.ValidationErrors[ZGlobalConfig.CompanyNameKey]));
		}

		public void TestConfigIsAttainedFromRegistry()
		{
			TestConfig.ConfigurationItemsForTesting = null;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_Code = "BRA";
			branch.Company.GC_Name = "Company Co.";
			branch.Company.GC_WebAddress = "company.net/";
			Factory.Save();

			Env.Registry.WebBranch = branch.PK.ToGuid();
			Factory.Save();

			AssertEquals("BRA", TestConfig.Branch);
			AssertEquals("http://company.net/", TestConfig.HomePage);
			AssertEquals("Company Co.", TestConfig.CompanyName);
		}

		#endregion

		protected ZTestPage Page
		{
			get
			{
				if (fPage == null)
				{
					fPage = new ZTestPage();
				}
				return fPage;
			}
		}
		ZTestPage fPage;
	}
}
