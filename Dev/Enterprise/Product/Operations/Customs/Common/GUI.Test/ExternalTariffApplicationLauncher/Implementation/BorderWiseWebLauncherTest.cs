using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Common.GUI.Testing
{
	class BorderWiseWebLauncherTest : TestCaseWithFactory
	{
		public void TestLaunchBorderWiseWebExplicitly_ShouldNotShowWaitingForResponseDialog()
		{
			var launcher = new BorderWiseWebLauncher();

			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = "";

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters());
			AssertEquals("https://app.borderwise.com?c=AU", WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_WhenShownForTariffSelection_AndUMPUrlReturned_ShouldAppendParametersCorrectly()
		{
			SetupCurrentUserEmail();

			var launcher = CreateWebUrlLauncher("https://CompuGlobalHyperMegaNet.www/100years.com", "https://api.ump.com");

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters { Tariff = "1234.56.78", Stat = "00.01", ImpExp = "I", ReturnUri = new Uri("edient:whyIsThisTheFormat?") });
			AssertEquals("Should encode URL control characters in the return URL parameter", "https://CompuGlobalHyperMegaNet.www/100years.com?tariff=1234.56.78&stat=00.01&impexp=I&c=AU&ret=edient%3AwhyIsThisTheFormat%3F", WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_WhenShownForTariffSelection_AndUMPUrlReturned_AndTariffIsNull_ShouldAppendParametersCorrectly()
		{
			SetupCurrentUserEmail();

			var launcher = CreateWebUrlLauncher("https://CompuGlobalHyperMegaNet.www/100years.com", "https://api.ump.com");

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters { Tariff = null, ImpExp = "I", ReturnUri = new Uri("edient:whyIsThisTheFormat?") });
			AssertEquals("Should encode URL control characters in the return URL parameter", "https://CompuGlobalHyperMegaNet.www/100years.com?impexp=I&c=AU&ret=edient%3AwhyIsThisTheFormat%3F", WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_WhenShownForTariffSelection_AndUMPUrlReturned_WithUrlParameters_ShouldAppendParametersCorrectly()
		{
			SetupCurrentUserEmail();

			var launcher = CreateWebUrlLauncher("https://CompuGlobalHyperMegaNet.www/100years.com?yes=no&no=yes", "https://api.ump.com");

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters { Tariff = "1234.56.78", Stat = "00.01", ImpExp = "I", ReturnUri = new Uri("edient:whyIsThisTheFormat?") });
			AssertEquals("Should encode URL control characters in the return URL parameter, and use correct parameter concatenation characters", "https://CompuGlobalHyperMegaNet.www/100years.com?yes=no&no=yes&tariff=1234.56.78&stat=00.01&impexp=I&c=AU&ret=edient%3AwhyIsThisTheFormat%3F", WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_WhenShownForTariffSelection_AndUMPUrlNotReturned_ShouldAppendParametersCorrectly()
		{
			var launcher = CreateWebUrlLauncher();

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters { Tariff = "1234.56.78", Stat = "00.01", ImpExp = "I", ReturnUri = new Uri("edient:whyIsThisTheFormat?") });
			AssertEquals("Should encode URL control characters in the return URL parameter", "https://app.borderwise.com?tariff=1234.56.78&stat=00.01&impexp=I&c=AU&ret=edient%3AwhyIsThisTheFormat%3F", WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_WhenRequestIdNotNull_AddCountryParameterIsFalse_ShouldAppendParametersCorrectly()
		{
			var launcher = CreateWebUrlLauncher();

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			var requestId = Guid.NewGuid().ToString();
			launcher.LaunchExternalApplication(new BorderWiseFilters { RequestId = requestId, AddCountryParameter = false });
			AssertEquals("Should show the request-id without country parameter in the return URL parameter", "https://app.borderwise.com?requestId=" + requestId, WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_InvalidData()
		{
			var launcher = CreateWebUrlLauncher();

			var existingBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_IsActive, true));

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = string.Empty;
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				launcher.LaunchExternalApplication(null);
				AssertEquals(BorderWiseWebLauncher.BaseBorderWiseAppPath, WebUrlLauncher.LastUrlLaunched);
			}

			using (Env.SetTemporaryUserContext(Guid.Empty, existingBranch.PK.ToGuid(), Guid.Empty))
			{
				launcher.LaunchExternalApplication(null);
				AssertEquals(BorderWiseWebLauncher.BaseBorderWiseAppPath, WebUrlLauncher.LastUrlLaunched);
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), existingBranch.PK.ToGuid(), Guid.Empty))
			{
				launcher.LaunchExternalApplication(null);
				AssertEquals(BorderWiseWebLauncher.BaseBorderWiseAppPath, WebUrlLauncher.LastUrlLaunched);
			}

			staff.GS_EmailAddress = "aaa@bbb.ccc";
			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), existingBranch.PK.ToGuid(), Guid.Empty))
			{
				launcher.LaunchExternalApplication(null);
				AssertEquals(BorderWiseWebLauncher.BaseBorderWiseAppPath, WebUrlLauncher.LastUrlLaunched);
			}

			launcher.OverriddenResponse_ForTest.Value = null;
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = "DummyUrl";

			AssertNoExceptionThrown("GetAutoLoginUrl should not throw exception when BorderWiseUmpApiBaseAddress is invalid", () =>
			{
				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), existingBranch.PK.ToGuid(), Guid.Empty))
				{
					launcher.LaunchExternalApplication(null);
					Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains($"Invalid URI settings in Registry '{RawDataRegistry.Instance.BorderWiseUmpApiBaseAddress.HumanReadableRegistryPath()}'."));
				}
			});

			launcher.OverriddenResponse_ForTest.Value = new RedirectUrlResponse
			{
				Url = "http://Start",
				TokenExpiry = DateTime.UtcNow.AddHours(10)
			};

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), existingBranch.PK.ToGuid(), Guid.Empty))
			{
				launcher.LaunchExternalApplication(null);
				AssertEquals("http://Start", WebUrlLauncher.LastUrlLaunched);
			}
		}

		public void TestShowBorderWiseWebApp_Should_Use_BorderWiseWebAddress_When_BorderWiseUmpApiBaseAddress_Is_Not_Set()
		{
			var launcher = CreateWebUrlLauncher();

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters());

			AssertEquals(ZArchitecture.Environment.DataRegistry.Instance.BorderWiseWebAddress + DefaultCountryParameter, WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_Should_Return_LoginUrl_From_UmpApi()
		{
			SetupCurrentUserEmail();

			const string umpApiBaseUrl = "https://api.borderwiseUmp.com";
			const string expectedUrl = "https://expectedUrl.com";
			var mockHttpMessageHandler = GetHttpMessageHandler(HttpStatusCode.OK, expectedUrl);

			var launcher = CreateWebUrlLauncher(null, umpApiBaseUrl, mockHttpMessageHandler);

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters());

			AssertEquals(expectedUrl + DefaultCountryParameter, WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_ShouldNotCache_WhenUrlIsNotAuthorized()
		{
			SetupCurrentUserEmail();

			const string umpApiBaseUrl = "https://api.borderwiseUmp.com";
			const string notAuthorizedUrl = "https://api.borderwise.com/login?status=NotAuthorized";
			var mockHttpMessageHandler = GetHttpMessageHandler(HttpStatusCode.OK, notAuthorizedUrl);

			var launcher = CreateWebUrlLauncher(null, umpApiBaseUrl, mockHttpMessageHandler);

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters());

			AssertEquals(BorderWiseWebLauncher.AutoLoginURLs.Count, 0);
		}

		public void TestShowBorderWiseWebApp_Should_Use_BorderWiseWebAddress_If_UmpApiUrl_Is_Invalid()
		{
			const string umpApiBaseUrl = "https://api.borderwiseUmp.com";
			var mockHttpMessageHandler = GetHttpMessageHandler(HttpStatusCode.Forbidden, "invalid");
			var launcher = CreateWebUrlLauncher(null, umpApiBaseUrl, mockHttpMessageHandler);

			AssertNullOrEmpty(WebUrlLauncher.LastUrlLaunched);

			launcher.LaunchExternalApplication(new BorderWiseFilters());

			AssertEquals(ZArchitecture.Environment.DataRegistry.Instance.BorderWiseWebAddress + DefaultCountryParameter, WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_ShouldReturnLoginUrlFromCache_WhenCacheExistsAndNotExpired()
		{
			SetupCurrentUserEmail();
			const string umpApiBaseUrl = "https://api.borderwiseUmp.com";
			var launcher = CreateWebUrlLauncher(null, umpApiBaseUrl);

			var cachedUrl = new RedirectUrlResponse()
			{
				Url = "http://url.com",
				TokenExpiry = DateTime.UtcNow.AddHours(12)
			};
			BorderWiseWebLauncher.AutoLoginURLs.TryAdd(GetUrlCacheKey(), cachedUrl);

			launcher.LaunchExternalApplication(new BorderWiseFilters());

			AssertEquals(cachedUrl.Url + DefaultCountryParameter, WebUrlLauncher.LastUrlLaunched);
		}

		public void TestShowBorderWiseWebApp_ShouldRemoveValueFromCacheOnRequest_WhenItIsExpired()
		{
			SetupCurrentUserEmail();

			var newResponseUrl = "http://newUrl.com";
			const string umpApiBaseUrl = "https://api.borderwiseUmp.com";
			var launcher = CreateWebUrlLauncher(newResponseUrl, umpApiBaseUrl);
			var cachedUrl = new RedirectUrlResponse()
			{
				Url = "http://oldUrl.com",
				TokenExpiry = DateTime.UtcNow.AddHours(-1)
			};
			BorderWiseWebLauncher.AutoLoginURLs.TryAdd(GetUrlCacheKey(), cachedUrl);
			launcher.LaunchExternalApplication(new BorderWiseFilters());

			AssertEquals(newResponseUrl + DefaultCountryParameter, WebUrlLauncher.LastUrlLaunched);
			AssertEquals(BorderWiseWebLauncher.AutoLoginURLs.Count, 1);
			AssertEquals(BorderWiseWebLauncher.AutoLoginURLs.Values.First().Url, newResponseUrl);
			AssertEquals(BorderWiseWebLauncher.AutoLoginURLs.Values.First().TokenExpiry > DateTime.UtcNow, true);
		}

		string GetUrlCacheKey()
		{
			var emailAddress = Env.CurrentUser?.EmailAddress;
			var loggedInOrganisationPK = Env.CurrentCompany?.OrganisationPK;
			var factory = new BusinessObjectFactory { NameForDebugging = nameof(BorderWiseWebLauncher) };
			var loggedInOrgProxy = factory.Load<OrgHeader>(loggedInOrganisationPK.Value);
			var staffCode = "E";
			var databaseNumber = "0";
			var licenseCode = GlbCompany.CurrentCompany.GetLicenceKeyIdentifier("-");
			var versionNumber = ReleaseInfo.Instance.VersionNumber.ToString();
			var releaseRing = ReleaseInfo.Instance.ReleaseRing;

			var secureQueryString = new SecureQueryString
			{
				{ BorderWiseWebLauncher.UserEmailKey, emailAddress },
				{ BorderWiseWebLauncher.OrgCodeKey, loggedInOrgProxy.OH_Code },
				{ BorderWiseWebLauncher.StaffCodeKey, staffCode },
				{ BorderWiseWebLauncher.DatabaseNumberKey, databaseNumber },
				{ BorderWiseWebLauncher.LicenseCodeKey, licenseCode },
				{ BorderWiseWebLauncher.ReleaseRingKey, releaseRing },
				{ BorderWiseWebLauncher.CWVersionNumberKey, versionNumber }
			};
			return secureQueryString.ToString();
		}

		void SetupCurrentUserEmail()
		{
			GlbStaff.GetCurrentUser(Factory).GS_EmailAddress = "dave@FlancrestEnterprises.com";
			Factory.Save();
		}

		BorderWiseWebLauncher CreateWebUrlLauncher(string overriddenResponseString = "", string umpApiBaseUrl = "", HttpMessageHandler httpMessageHandler = null)
		{
			BorderWiseWebLauncher.AutoLoginURLs.Clear();
			var launcher = new BorderWiseWebLauncher();

			if (!string.IsNullOrEmpty(overriddenResponseString))
			{
				launcher.OverriddenResponse_ForTest.Value = new RedirectUrlResponse
				{
					Url = overriddenResponseString,
					TokenExpiry = DateTime.UtcNow.AddHours(12)
				};
			}

			launcher.HttpMessageHandler = httpMessageHandler;
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = umpApiBaseUrl;

			return launcher;
		}

		HttpMessageHandler GetHttpMessageHandler(HttpStatusCode httpStatusCode, string expectedUrl = "")
		{
			var response = new RedirectUrlResponse() { Url = expectedUrl };

			var httpResponse = new HttpResponseMessage(httpStatusCode)
			{
				Content = new StringContent(Utilities.SerializeToJson(response))
				{
					Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
				}
			};

			return new MockHttpHandler(httpResponse);
		}

		protected override void SetUp()
		{
			base.SetUp();
			WebUrlLauncher.ClearLastUrlLaunched();
		}

		protected override void TearDown()
		{
			base.TearDown();
			WebUrlLauncher.ClearLastUrlLaunched();
		}

		const string DefaultCountryParameter = "?c=AU";
	}

	class MockHttpHandler : HttpMessageHandler
	{
		readonly HttpResponseMessage response;
		readonly bool throwException;

		public MockHttpHandler(HttpResponseMessage response, bool throwException = false)
		{
			this.response = response;
			this.throwException = throwException;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			if (throwException)
			{
				throw new Exception("exception");
			}
			return Task.FromResult(response);
		}
	}
}
