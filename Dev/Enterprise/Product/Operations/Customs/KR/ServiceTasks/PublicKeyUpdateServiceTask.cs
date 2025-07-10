using System;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using ServiceManager.Integration.ServiceTasks.CW;
using WTG.Foundation.Http;
using GlbCompanyWrapper = Enterprise.Customs.KR.Business.GlbCompanyWrapper;

[assembly: HostedService(
	Enterprise.Customs.KR.ServiceTasks.PublicKeyUpdateServiceTask.Code,
	"KR Customs Public Key Update",
	Enterprise.Customs.KR.ServiceTasks.PublicKeyUpdateServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.KR.ServiceTasks.PublicKeyUpdateServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.KoreaSouth,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1hour",
	DefaultScheduleRunEvery = "1hour",
	ActiveByDefault = true
	)]

namespace Enterprise.Customs.KR.ServiceTasks
{
	public class PublicKeyUpdateServiceTask : CustomsServiceTask
	{
		public const string MessageServiceTaskCategory = "KRC";
		public const string Code = "KRP";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int DefaultPeriodInMinutes = 1;

		protected override void RunTaskCore(CancellationToken token)
		{
			var company = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.KoreaSouth).Where(x => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(x).IsValidForMessaging).FirstOrDefault();
			if (company != null)
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForCompany(company.GC_Code))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var url = KRCustomsRegistry.Instance.CustomsWebAddress.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
						var existingData = KRCustomsRegistry.Instance.CustomsCertificate.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
						var newPublicKey = GetCustomsPublicKey(url);
						if (!string.IsNullOrEmpty(newPublicKey) && System.Text.Encoding.UTF8.GetString(existingData) != newPublicKey)
						{
							KRCustomsRegistry.Instance.CustomsCertificate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, MessageEncoding.UTF8WithoutBOM.GetBytes(newPublicKey));
							Logger.LogWarning(Res.GetString("BBD47F7C-D49A-4528-81FD-5651D3E94F00", "KR Customs public key has successfully updated."));

							var validityCheckObject = GetCustomsPublicKeyValidityChecker();
							var validityCheckResult = validityCheckObject.CheckCustomsPublicKeyStartAndExiryDates();
							var message = validityCheckResult.Item1;
							var isError = validityCheckResult.Item2;

							if (!string.IsNullOrEmpty(message))
							{
								if (isError)
								{
									Logger.LogError(message);
								}
								else
								{
									Logger.LogWarning(message);
								}
							}
						}
					});
				}
			}
		}

		internal virtual CustomsPublicKeyValidityChecker GetCustomsPublicKeyValidityChecker() => new CustomsPublicKeyValidityChecker();

		internal string GetCustomsPublicKey(string url)
		{
			using var httpClient = ObjectFactory.Get<IHttpClientFactory>().Create();
			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("CW1");
			httpClient.Timeout = TimeSpan.FromSeconds(CustomsDataRegistry.Instance.WebServiceTimeoutInSeconds.Value);
			return httpClient.GetStringAsync(url).GetAwaiter().GetResult();
		}
	}
}
