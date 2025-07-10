using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.Ncts.Processors.Phase5;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.CTCMessageRetrieverServiceTask.Code
	, Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.CTCMessageRetrieverServiceTask.FriendlyName
	, Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.CTCMessageRetrieverServiceTask.MessageServiceTaskCategory
	, typeof(Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.CTCMessageRetrieverServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.CTCMessageRetrieverServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsNCTS,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive
	},
	"UK Customs NCTS Phase 5 messages inbound"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.GovernmentGateway.ServiceTasks.CTCMessageRetrieverServiceTask.Code
	, EDIInterchangeSchema.Constants.TableName
	, new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsNCTS
	},
	"UK Customs NCTS Phase 5 interchanges inbound"
)]

namespace Enterprise.Customs.GB.GovernmentGateway.ServiceTasks
{
	public class CTCMessageRetrieverServiceTask : CustomsServiceTask
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
						var logger = GetNewLogger();

						using (var phase5InterchangeProcessor = new NctsInboundInterchangeProcessorPhase5(logger))
						{
							phase5InterchangeProcessor.ExecuteBatch(token);
						}

						using (var phase5MessageProcessor = new NctsResponseMessageProcessorPhase5(ServiceLogger))
						{
							phase5MessageProcessor.ExecuteBatch(token);
						}
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

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			ServiceLogger.Log(logType, log.Trim());
		}
		public const string Code = "GGR";
		public const string FriendlyName = "GB Common transit convention message retriever";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
