using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.CTCMessageSenderServiceTask.Code
	, Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.CTCMessageSenderServiceTask.FriendlyName
	, Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.CTCMessageSenderServiceTask.MessageServiceTaskCategory
	, typeof(Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.CTCMessageSenderServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.CTCMessageSenderServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCommonTransitConvention,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit
	},
	"UK Customs CTC NCTS messages outbound (GCT)"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.CTCMessageSenderServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsNCTS,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit
	},
	"UK Customs CTC NCTS messages outbound (GBN)"
)]

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts
{
	public class CTCMessageSenderServiceTask : CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.UnitedKingdom))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = GetNewLogger();
						new CTCOutgoingMessageProcessor(log).ProcessMessage(token);
						new NctsPhase5OutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}

		protected LoggingInformation GetNewLogger()
		{
			var result = new LoggingInformation();
			result.OnLogInfoAdded += Logger_OnLogInfoAdded;
			return result;
		}

		void Logger_OnLogInfoAdded(string log, Integration.LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}
		public const string Code = "GGS";
		public const string FriendlyName = "GB Common transit convention message sender";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
