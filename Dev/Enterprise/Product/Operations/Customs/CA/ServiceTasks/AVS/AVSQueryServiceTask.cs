using System;
using System.Globalization;
using System.Threading;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.CA.ServiceTasks.AVSQueryServiceTask.Code,
	Enterprise.Customs.CA.ServiceTasks.AVSQueryServiceTask.FriendlyName,
	"CAC",
	typeof(Enterprise.Customs.CA.ServiceTasks.AVSQueryServiceTask),
	MinimumPeriod = "1Minutes",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Canada,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.CA.ServiceTasks.AVSQueryServiceTask.Code,
EDIMessageSchema.Constants.TableName,
new[] { EDIMessageSchema.Constants.EM_IsActive + "=Y",
	EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
	EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
	EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CFIAQuery,
	EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	"CA Customs CFIA message outbound")]

namespace Enterprise.Customs.CA.ServiceTasks
{
	public class AVSQueryServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		public const string Code = "AVS";
		public const string FriendlyName = "Canadian AIRS Validation Query";

		protected override void RunTaskCore(CancellationToken token)
		{
			var processor = new AVSQueryMessageProcessor(ServiceLogger);
			foreach (var company in GlbCompany.GetActiveCompanies(company => company.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada))
			{
				token.ThrowIfCancellationRequested();
				if (ValidateEnvironment(company))
				{
					using (DisposableEnvironment.ForCompany(company.GC_Code))
					{
						RunTaskHandleEmailSendFailure(processor.Process);
					}
				}
			}
		}

		bool ValidateEnvironment(GlbCompany company)
		{
			var result = true;
			if (string.IsNullOrEmpty(CACustomsDataRegistry.Instance.AIRSValidationKey.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty)))
			{
				ServiceLogger.Log(LogType.Warning, string.Format(CultureInfo.CurrentCulture, "AIRS Validation Key hasn't been setup for Company {0}. The key is allocated by CFIA. Please set it up in Registry -> {1}",
						company.GC_Code, ((IRegistryItemInternals)CACustomsDataRegistry.Instance.AIRSValidationKey).Location));
				result = false;
			}

			return result;
		}
	}
}
