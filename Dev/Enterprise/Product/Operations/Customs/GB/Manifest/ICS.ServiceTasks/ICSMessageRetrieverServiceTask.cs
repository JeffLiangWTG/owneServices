using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageRetrieverServiceTask.Code
	, Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageRetrieverServiceTask.FriendlyName
	, Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageRetrieverServiceTask.MessageServiceTaskCategory
	, typeof(Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageRetrieverServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageRetrieverServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbMessageICSGreatBritain
	},
	"UK Customs ICS messages inbound"
)]
[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.ICS.ServiceTasks.ICSMessageRetrieverServiceTask.Code
	, EDIInterchangeSchema.Constants.TableName
	, new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbMessageICSGreatBritain
	},
	"UK Customs ICS interchanges inbound"
)]

namespace Enterprise.Customs.GB.ICS.ServiceTasks
{
	public class ICSMessageRetrieverServiceTask : Customs.ServiceTasks.CustomsServiceTask
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

						using (var interchangeProcessor = new ICSInboundInterchangeProcessor(logger))
						{
							interchangeProcessor.ExecuteBatch(token);
						}

						using (var processor = new ICSIncomingMessageProcessor(logger))
						{
							processor.ExecuteBatch(token);
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

		void Logger_OnLogInfoAdded(string log, LogType logType) => ServiceLogger.Log(logType, log.Trim());

		public const string Code = "ICR";
		public const string FriendlyName = "GB ICS Message Retriever";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
