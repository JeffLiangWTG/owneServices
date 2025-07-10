using System;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using EDIMessage = Enterprise.Customs.KR.Business.EDIMessage;
using GlbCompanyWrapper = Enterprise.Customs.KR.Business.GlbCompanyWrapper;

[assembly: HostedService(
	Enterprise.Customs.KR.ServiceTasks.OutBoundServiceTask.Code,
	"KR Customs Message Sender",
	Enterprise.Customs.KR.ServiceTasks.OutBoundServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.KR.ServiceTasks.OutBoundServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.KoreaSouth,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.KR.ServiceTasks.OutBoundServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
				EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
				EDIMessageSchema.Constants.EM_IsActive + "=Y",
				EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
				EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms
	},
	"KR Customs Messages Outbound"
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.KR.ServiceTasks.OutBoundServiceTask.Code,
	CusPollingTransactionSchema.Constants.TableName,
	new[] {
		CusPollingTransactionSchema.Constants.CPT_Status + "=" + Enterprise.Customs.KR.Messaging.CusPollingTransactionStatusList.Codes.Opened,
		CusPollingTransactionSchema.Constants.CPT_ApplicationCode + "=" + EDIMessage.ApplicationCodes.KRCustoms
	},
	"KR CusPollingTransaction Outbound"
	)]

namespace Enterprise.Customs.KR.ServiceTasks
{
	public class OutBoundServiceTask : CustomsServiceTask
	{
		public const string MessageServiceTaskCategory = "KRC";
		public const string Code = "KRO";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public const int DefaultPeriodInMinutes = 1;

		protected override void RunTaskCore(CancellationToken token)
		{
			var activeKoreanCompaniesForMessaging = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.KoreaSouth).Where(x => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(x).IsValidForMessaging).ToArray();
			if (activeKoreanCompaniesForMessaging.Length > 0)
			{
				if (CheckMessagingPrerequisites())
				{
					foreach (var company in activeKoreanCompaniesForMessaging)
					{
						token.ThrowIfCancellationRequested();
						using (DisposableEnvironment.ForCompany(company.GC_Code))
						{
							RunTaskHandleEmailSendFailure(() =>
							{
								new KRCOutgoingMessageProcessor(Logger).ProcessMessage(token);
								new CusPollingTransactionMessageGenerator(Logger, new BusinessObjectFactory()).Create(token, company.PK);
							});
						}
					}
				}
			}
		}

		bool CheckMessagingPrerequisites()
		{
			var result = true;
			var krCustomsCertificate = KRCustomsRegistry.Instance.CustomsCertificate.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (krCustomsCertificate.Length == 0)
			{
				Logger.LogError(Res.GetString("096A0918-556B-44AB-A647-8AC74C51388E", "KR Customs public key has never been set. Please check if the service task 'KRP' is up and running. This service task stops now."));
				result = false;
			}
			else
			{
				var validityCheckResult = new CustomsPublicKeyValidityChecker().CheckCustomsPublicKeyStartAndExiryDates();

				var message = validityCheckResult.Item1;
				var isError = validityCheckResult.Item2;

				if (!string.IsNullOrEmpty(message))
				{
					if (isError)
					{
						Logger.LogError(message);
						result = false;
					}
					else
					{
						Logger.LogWarning(message);
					}
				}
			}
			return result;
		}
	}
}
