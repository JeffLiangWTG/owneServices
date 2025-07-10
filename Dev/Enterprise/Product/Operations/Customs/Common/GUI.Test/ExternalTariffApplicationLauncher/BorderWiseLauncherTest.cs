using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common.GUI.Testing
{
	class BorderWiseLauncherTest : TestCaseWithFactory
	{
		readonly string defaultCountryCode = Core.Constants.CountryCodes.Egypt;
		const string umpApiBaseUrl = "https://test.borderwiseUmp.com";

		public void TestSelectTariffFromExternalApplication_ShouldCreateBorderWiseWebLauncher_WhenCountryCodeOnDefaultCountryList()
		{
			var countryCodes = BorderWiseLauncher.DefaultCountriesToUseBorderWise;
			BorderWiseLauncher.CountriesNotToUseBorderWise.Clear();
			foreach (var countryCode in countryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				{
					var borderWiseLauncher = new BorderWiseLauncher();
					AssertEquals(borderWiseLauncher.Launcher.GetType(), typeof(BorderWiseWebLauncher));
				}
			}
		}

		public void TestSelectTariffFromExternalApplication_ShouldCreateBorderWiseLauncher_WhenUmpReturnsCountrySettingsIsReadyToUseWeb()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(defaultCountryCode))
			{
				var borderWiseLauncher = ArrangeUmpApiReturnCountrySettings(true);
				AssertEquals(typeof(BorderWiseWebLauncher), borderWiseLauncher.Launcher.GetType());
				AssertEquals(0, BorderWiseLauncher.CountriesNotToUseBorderWise.Count);
			}
		}

		public void TestSelectTariffFromExternalApplication_ShouldReturnNull_WhenUmpReturnsCountrySettingsIsNotReadyToUseWeb()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(defaultCountryCode))
			{
				var borderWiseLauncher = ArrangeUmpApiReturnCountrySettings(false);
				AssertNull(borderWiseLauncher.Launcher);
				AssertEquals(1, BorderWiseLauncher.CountriesNotToUseBorderWise.Count);
				var cacheAdded = BorderWiseLauncher.CountriesNotToUseBorderWise.TryGetValue(defaultCountryCode, out DateTime expiryDateTimeUtc);
				AssertEquals(true, cacheAdded);
				AssertEquals(true, expiryDateTimeUtc > DateTime.UtcNow);
			}
		}

		public void TestSelectTariffFromExternalApplication_ShouldReturnNull_WhenUmpReturnsError()
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = umpApiBaseUrl;
			BorderWiseLauncher.CountriesNotToUseBorderWise.Clear();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(defaultCountryCode))
			{
				var borderWiseLauncher = new BorderWiseLauncher();
				const string resultFromUmp = "error";
				borderWiseLauncher.HttpMessageHandler = GetHttpMessageHandler(HttpStatusCode.InternalServerError, resultFromUmp);

				AssertNull(borderWiseLauncher.Launcher);
				AssertEquals(
					$"Get BorderWise country ready for web settings from UMP failed for country: {Env.CurrentCompany.Country.Code}. StatusCode: {HttpStatusCode.InternalServerError}, Message: {resultFromUmp}",
					ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		public void TestSelectTariffFromExternalApplication_ShouldReturnNull_WhenUmpApiBaseUrlIsEmpty()
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = "";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(defaultCountryCode))
			{
				var borderWiseLauncher = new BorderWiseLauncher();

				AssertNull(borderWiseLauncher.Launcher);
			}
		}

		public void TestSelectTariffFromExternalApplication_ShouldReturnNull_WhenCountriesNotToUseBorderWiseCacheExists()
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = umpApiBaseUrl;
			BorderWiseLauncher.CountriesNotToUseBorderWise.Clear();
			BorderWiseLauncher.CountriesNotToUseBorderWise.TryAdd(defaultCountryCode, DateTime.UtcNow.AddHours(1));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(defaultCountryCode))
			{
				var borderWiseLauncher = new BorderWiseLauncher();
				AssertNull(borderWiseLauncher.Launcher);
			}
		}

		public void TestSelectTariffFromExternalApplication_ShouldCallUmpApi_WhenCountriesNotToUseBorderWiseCacheExpired()
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = umpApiBaseUrl;
			BorderWiseLauncher.CountriesNotToUseBorderWise.Clear();
			BorderWiseLauncher.CountriesNotToUseBorderWise.TryAdd(defaultCountryCode, DateTime.UtcNow.AddHours(-1));
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(defaultCountryCode))
			{
				var borderWiseLauncher = new BorderWiseLauncher
				{
					HttpMessageHandler = GetHttpMessageHandler(HttpStatusCode.OK, "false")
				};

				AssertNull(borderWiseLauncher.Launcher);
				var cacheAdded = BorderWiseLauncher.CountriesNotToUseBorderWise.TryGetValue(defaultCountryCode, out DateTime expiryDateTimeUtc);
				AssertEquals(true, expiryDateTimeUtc > DateTime.UtcNow);
			}
		}

		public void TestSelectTariffFromExternalApplication_ShouldReturnNull_WhenCountryCodeOverrideNotToUseBorderWise()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var borderWiseLauncher = ArrangeUmpApiReturnCountrySettings(false);
				borderWiseLauncher.CountryCodeOverride = defaultCountryCode;
				AssertNull(borderWiseLauncher.Launcher);
			}
		}

		HttpMessageHandler GetHttpMessageHandler(HttpStatusCode httpStatusCode, string resultFromUmp = "", bool throwException = false)
		{
			var httpResponse = new HttpResponseMessage(httpStatusCode)
			{
				Content = new StringContent(resultFromUmp)
				{
					Headers = { ContentType = new MediaTypeHeaderValue("application/json") }
				}
			};

			return new MockHttpHandler(httpResponse, throwException);
		}

		BorderWiseLauncher ArrangeUmpApiReturnCountrySettings(bool resultFromUmp)
		{
			ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress = umpApiBaseUrl;
			BorderWiseLauncher.CountriesNotToUseBorderWise.Clear();

			var borderWiseLauncher = new BorderWiseLauncher
			{
				HttpMessageHandler = GetHttpMessageHandler(HttpStatusCode.OK, resultFromUmp.ToString())
			};
			return borderWiseLauncher;
		}
	}
}
