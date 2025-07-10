using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Websocket.Client;

namespace Enterprise.Customs.Common.GUI
{
	public class BorderWiseLauncher
	{
		static string BorderWiseWebAddress => ZArchitecture.Environment.DataRegistry.Instance.BorderWiseWebAddress;

		static string UmpApiBaseAddress => ZArchitecture.Environment.DataRegistry.Instance.BorderWiseUmpApiBaseAddress;

		[SuppressMessage("CargoWiseOne", "CW1021")]
		public static ConcurrentDictionary<string, BatchModeSettings> BatchModeSettingResponse = new ConcurrentDictionary<string, BatchModeSettings>();

		[SuppressMessage("CargoWiseOne", "CW1021")]
		public static ConcurrentDictionary<string, DateTime> CountriesNotToUseBorderWise = new ConcurrentDictionary<string, DateTime>();

		[SuppressMessage("CargoWiseOne", "CW1021")]
		public static readonly IReadOnlyCollection<string> DefaultCountriesToUseBorderWise =
			new ReadOnlyCollection<string>(new[]
			{
				Constants.CountryCodes.Australia,
				Constants.CountryCodes.Canada,
				Constants.CountryCodes.NewZealand,
				Constants.CountryCodes.UnitedStates,
				Constants.CountryCodes.SouthAfrica,
				Constants.CountryCodes.Singapore
			});

		public BorderWiseLauncher(string countryCodeOverride = default)
		{
			this.countryCodeOverride = countryCodeOverride;
		}

		string countryCodeOverride;

		public string CountryCodeOverride
		{
			get
			{
				return string.IsNullOrEmpty(countryCodeOverride) ? Env.CurrentCompany.Country.Code : countryCodeOverride;
			}
			set { countryCodeOverride = value?.Trim(); }
		}

		public IBorderWiseTariffProcessor BorderWiseTariffProcessor { get; set; }

		public IWebsocketClient WebSocketClient { get; set; }

		public HttpMessageHandler HttpMessageHandler { get; set; }

		IExternalTariffApplicationLauncher launcher { get; set; }
		public IExternalTariffApplicationLauncher Launcher
		{
			get
			{
				return launcher ?? GetExternalTariffApplicationLauncher();
			}
			set
			{
				launcher = value;
			}
		}

		public BorderWiseInvoiceLine ClassifySingleCommodityInBorderWise(BorderWiseFilters filters)
		{
			if (Launcher == null)
			{
				return null;
			}

			var borderWiseSingleTariffProcessor = new BorderWiseSyncSingleTariffProcessor();
			var result = borderWiseSingleTariffProcessor.Process(Launcher, filters);

			if (result == null || string.IsNullOrWhiteSpace(result.TariffCode))
			{
				return null;
			}

			return result;
		}

		public bool ClassifyDeclarationJobInBorderWise(Form parentForm, ICommonInvoiceDataProvider baseJobDeclaration)
		{
			if (Launcher == null || !UseBatchMode())
			{
				return false;
			}

			var borderWiseAsyncBatchTariffProcessor = new BorderWiseAsyncBatchTariffProcessor();
			BorderWiseTariffProcessor = borderWiseAsyncBatchTariffProcessor;

			if (WebSocketClient != null)
			{
				borderWiseAsyncBatchTariffProcessor.WebSocketClient = WebSocketClient;
			}

			return borderWiseAsyncBatchTariffProcessor.Process(parentForm, baseJobDeclaration, Launcher);
		}

		public bool ClassifyCommoditiesInBorderWise(IFindBox findBox, Form parentForm, BorderWiseFilters filters)
		{
			if (Launcher == null)
			{
				return false;
			}

			if (UseBatchMode())
			{
				var borderWiseAsyncBatchTariffProcessor = new BorderWiseAsyncBatchTariffProcessor();
				BorderWiseTariffProcessor = borderWiseAsyncBatchTariffProcessor;

				if (WebSocketClient != null)
				{
					borderWiseAsyncBatchTariffProcessor.WebSocketClient = WebSocketClient;
				}

				if (borderWiseAsyncBatchTariffProcessor.IsBatchModeAllowed(findBox, parentForm))
				{
					borderWiseAsyncBatchTariffProcessor.Process(findBox, parentForm, Launcher, filters);
				}
				else
				{
					SingleTariffProcessor(findBox, parentForm, filters);
				}
			}
			else
			{
				SingleTariffProcessor(findBox, parentForm, filters);
			}

			return true;
		}

		public void SendMessageAndDisposeConnectionIfNeeded(ICommonInvoiceDataProvider baseJobDeclaration, Dictionary<ZGuid, string> invoicePksAction, bool isSave = false)
		{
			if (UseBatchMode())
			{
				new BorderWiseAsyncBatchTariffProcessor().SendMessageAndDisposeConnectionIfNeeded(baseJobDeclaration, invoicePksAction, isSave);
			}
		}

		public void LaunchExternalApplication(BorderWiseFilters filters)
		{
			new BorderWiseWebLauncher().LaunchExternalApplication(filters);
		}

		void SingleTariffProcessor(IFindBox findBox, Form parentForm, BorderWiseFilters filters)
		{
			var borderWiseSyncSingleTariffProcessor = new BorderWiseSyncSingleTariffProcessor();
			BorderWiseTariffProcessor = borderWiseSyncSingleTariffProcessor;

			if (WebSocketClient != null)
			{
				borderWiseSyncSingleTariffProcessor.WebSocketClient = WebSocketClient;
			}

			borderWiseSyncSingleTariffProcessor.Process(Launcher, filters, findBox, parentForm);
		}

		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		IExternalTariffApplicationLauncher GetExternalTariffApplicationLauncher()
		{
			if ((ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool != ExternalBorderComplianceToolList.Codes.BorderWiseWeb))
			{
				return null;
			}

			var countryCode = CountryCodeOverride;

			var shouldUseBorderWise = DefaultCountriesToUseBorderWise.Contains(countryCode);

			if (!shouldUseBorderWise && !(CountriesNotToUseBorderWise.TryGetValue(countryCode, out var expiryDateTimeUtc) && expiryDateTimeUtc > DateTime.UtcNow))
			{
				shouldUseBorderWise = GetBorderWiseCountriesFromUmp(countryCode);
			}

			launcher = shouldUseBorderWise ? new BorderWiseWebLauncher() : null;
			return launcher;
		}

		[SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		bool GetBorderWiseCountriesFromUmp(string countryCode)
		{
			var result = false;
			if (string.IsNullOrEmpty(UmpApiBaseAddress))
			{
				return result;
			}

			var relativeUri = $"/v1/settings/bww-country?countryCode={countryCode}";
			var requestUri = new Uri(UmpApiBaseAddress.TrimEnd('/') + relativeUri);

			using (var client = HttpMessageHandler == null ? new HttpClient() : new HttpClient(HttpMessageHandler))
			{
				client.Timeout = TimeSpan.FromSeconds(3);

				try
				{
					var response = client.GetAsync(requestUri, CancellationToken.None).GetAwaiter().GetResult();
					var umpResult = response.Content.ReadAsStringAsync().Result;
					if (!response.IsSuccessStatusCode)
					{
						ErrorReporter.ReportOnce($"Get BorderWise country ready for web settings from UMP failed for country: {countryCode}. StatusCode: {response.StatusCode}, Message: {umpResult}");
					}
					else
					{
						result = bool.Parse(umpResult);

						if (!result)
						{
							var expiryDateTimeUtc = DateTime.UtcNow.AddDays(1);
							CountriesNotToUseBorderWise.AddOrUpdate(countryCode, expiryDateTimeUtc, (key, current) => expiryDateTimeUtc);
						}
					}
				}
				catch (Exception exception)
				{
					if (exception is TaskCanceledException)
					{
						return result;
					}

					ErrorReporter.ReportOnce($"Get BorderWise country ready for web settings from UMP failed for country: {countryCode}", exception);
				}
				return result;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "Baseline")]
		bool UseBatchMode()
		{
			if (!Utilities.IsBorderWiseMultiLineClassificationEnabled)
			{
				return false;
			}

			var countryCode = Env.CurrentCompany.Country.Code;
			var emailAddress = Env.CurrentUser?.EmailAddress;

			var versionNumber = ReleaseInfo.Instance.VersionNumber.ToString();
			var releaseRing = ReleaseInfo.Instance.ReleaseRing;

			if (!string.IsNullOrEmpty(countryCode) && !string.IsNullOrEmpty(emailAddress))
			{
				if (BatchModeSettingResponse.TryGetValue(countryCode, out var setting) && setting.ExpiryDateTimeUtc > ZDateTime.UtcNow)
				{
					return setting.UseBatchMode;
				}

				var relativeUri = $"/api/settings/use-batchmode?countryCode={countryCode}&userEmail={emailAddress}&cwVersionNumber={versionNumber}&releaseRing={releaseRing}";
				var requestUri = new Uri(BorderWiseWebAddress.TrimEnd('/') + relativeUri);

				using (var client = HttpMessageHandler == null ? new HttpClient() : new HttpClient(HttpMessageHandler))
				{
					client.Timeout = TimeSpan.FromSeconds(20);

					try
					{
						var task = client.GetAsync(requestUri, CancellationToken.None);

						if (!task.Result.IsSuccessStatusCode)
						{
							ErrorReporter.ReportOnce($"Use BatchMode from BorderWise failed for user email: {emailAddress} country: {countryCode}. StatusCode: {task.Result.StatusCode}");
						}
						else
						{
							var content = task.Result.Content.ReadAsStringAsync().Result;
							var response = Utilities.DeserializeFromJson<BatchModeSettings>(content);
							BatchModeSettingResponse.AddOrUpdate(countryCode, response, (key, current) => response);

							return response.UseBatchMode;
						}
					}
					catch (Exception exception)
					{
						if (exception is TaskCanceledException)
						{
							return false;
						}

						ErrorReporter.ReportOnce($"Use BatchMode from BorderWise failed for user email: {emailAddress} country: {countryCode}", exception);
					}
					return false;
				}
			}
			return false;
		}
	}
}
