using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Customs.GB.EMCS.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	EMCSMessageRetrieverServiceTask.Code
	, EMCSMessageRetrieverServiceTask.FriendlyName
	, EMCSMessageRetrieverServiceTask.MessageServiceTaskCategory
	, typeof(EMCSMessageRetrieverServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	EMCSMessageRetrieverServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsEMCS
	},
	"UK Customs EMCS messages inbound"
)]
[assembly: HostedServiceBusinessObjectBinding(
	EMCSMessageRetrieverServiceTask.Code
	, EDIInterchangeSchema.Constants.TableName
	, new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsEMCS
	},
	"UK Customs EMCS interchanges inbound"
)]

namespace Enterprise.Customs.GB.EMCS.ServiceTasks
{
	public class EMCSMessageRetrieverServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			ServiceTaskHelper.LogTaskStarting(ServiceLogger, Code, FriendlyName);
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.UnitedKingdom))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var logger = GetNewLogger();

						using (var interchangeProcessor = new EMCSInboundInterchangeProcessor(logger))
						{
							interchangeProcessor.ExecuteBatch(token);
						}
						using (var messageProcessor = new EMCSInboundBranchMessageProcessor(logger))
						{
							messageProcessor.ExecuteBatch(token);
						}
					});
				}
			}
			ServiceTaskHelper.LogTaskFinished(ServiceLogger, Code, FriendlyName);
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

		public const string Code = "EMC";
		public const string FriendlyName = "GB EMCS Message Retriever";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
