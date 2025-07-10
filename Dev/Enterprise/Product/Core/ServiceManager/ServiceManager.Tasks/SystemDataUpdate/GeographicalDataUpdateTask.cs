using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.AddressCleansing.Common;
using WTG.Foundation.Http;
using IHttpClientFactory = WTG.Foundation.Http.IHttpClientFactory;

[assembly: HostedService(
	"GDU", "Geographical Data Update Service", "SYS",
	typeof(Enterprise.ServiceManager.Tasks.SystemDataUpdate.GeographicalDataUpdateTask),
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1week",
	DefaultScheduleRunEvery = "4weeks",
	DefaultScheduleDaysOfWeek = new DayOfWeek[] { DayOfWeek.Sunday },
	DefaultScheduleStartAtLocal = "0seconds",
	ActiveByDefault = true
	)
]

namespace Enterprise.ServiceManager.Tasks.SystemDataUpdate
{
	class GeographicalDataUpdateTask : ServiceProviderImpl
	{
		const string GeographicalDataServiceEntry = "getcountrydataavailability";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		public override void RunTask(CancellationToken token)
		{
			var avsUri = AddressValidationServiceUrisProvider.GetAddressValidationServiceUris().Primary;

			if (!string.IsNullOrEmpty(avsUri.Uri))
			{
				ServiceLogger.Log(LogType.Information, "Starting Geographical Data Update Service...");

				var urlToUse = avsUri.Uri + GeographicalDataServiceEntry;
				if (Uri.IsWellFormedUriString(urlToUse, UriKind.Absolute))
				{
					var uriFull = new Uri(urlToUse);
					using (var client = GetHttpClient(avsUri))
					{
						try
						{
							var response = client.GetAsync(uriFull).Result;
							if (response.IsSuccessStatusCode)
							{
								var countryDataAvailabilities = response.Content.ReadAsAsync<List<CountryDataAvailability>>().Result;
								if (countryDataAvailabilities != null && countryDataAvailabilities.Count > 0)
								{
									UpdateCountryData(countryDataAvailabilities, token);
								}
								else
								{
									ServiceLogger.Log(LogType.Warning, "The Geographical web service returns no data.");
								}
							}
							else
							{
								var requestDetails = string.Join(System.Environment.NewLine, response.RequestMessage.ToString().Split(','));
								var errorMessage = string.Format(CultureInfo.CurrentCulture, "REQUEST:\n{0}\n\nERROR:\n{1}", requestDetails, response.ReasonPhrase);

								ServiceLogger.Log(LogType.Error, "The web service request failed" + errorMessage);
							}
						}
						catch (AggregateException ex)
						{
							var timeoutException = ex.InnerException as TaskCanceledException;
							if (timeoutException != null)
							{
								ServiceLogger.Log(LogType.Error, "Web service times out");
							}
							else
							{
								ServiceLogger.Log(LogType.Error, "Web service failed" + System.Environment.NewLine + ex.Message);
							}
						}
					}
				}
				else
				{
					ServiceLogger.Log(LogType.Error, $"Invalid URL: [{avsUri.Uri}]");
				}

				ServiceLogger.Log(LogType.Information, "Geographical Data Update Service finished.");
			}
		}

		HttpClient GetHttpClient(AddressValidationServiceUri avsUri)
		{
			var httpClientFactory = ObjectFactory.Get<IHttpClientFactory>();
			var httpClient = httpClientFactory
				.CreateNew(
					new HttpClientHandlerWithDiagnostics(new CookieContainer()),
					TimeSpan.FromSeconds(Env.Instance.Registry.AddressValidationWebServiceTimeout));
			var helper = new AddressValidationServiceHelper();
			httpClient.DefaultRequestHeaders.Authorization = helper.GetAuthenticationHeaderValue(avsUri.EnableS2STAuth);
			return httpClient;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider")]
		void UpdateCountryData(List<CountryDataAvailability> countryDataAvailabilities, CancellationToken token)
		{
			var countries = new List<Tuple<string, string, string>>();
			string selectSql = string.Format("SELECT {0}, {1}, {2} FROM {3}", RefCountrySchema.RN_Code.Name,
				RefCountrySchema.RN_Desc.Name, RefCountrySchema.RN_ValidationStatus.Name, RefCountrySchema.Constants.TableName);

			using (var cmd = Db.Connection.Command(selectSql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					countries.Add(new Tuple<string, string, string>(reader[0].ToString(), reader[1].ToString(), reader[2].ToString()));
				}
			}

			string updateSql = string.Empty;
			foreach (var countryData in countryDataAvailabilities)
			{
				token.ThrowIfCancellationRequested();
				var oldCountry = countries.FirstOrDefault(c => c.Item1 == countryData.CountryCode);
				if (oldCountry != null && oldCountry.Item3 != countryData.AvailableData)
				{
					updateSql += string.Format("UPDATE {0} SET {1} = '{2}' WHERE {3} = '{4}'   ", RefCountrySchema.Constants.TableName,
						RefCountrySchema.RN_ValidationStatus.Name, countryData.AvailableData, RefCountrySchema.RN_Code.Name, countryData.CountryCode);
					ServiceLogger.Log(LogType.Information, string.Format("Updating Validation rule for {0} from {1} to {2}", oldCountry.Item2, oldCountry.Item3, countryData.AvailableData));
				}
			}

			if (!string.IsNullOrEmpty(updateSql))
			{
				var updateCommand = Db.Connection.Command(updateSql);
				updateCommand.ExecuteNonQuery();
			}
		}
	}
}
