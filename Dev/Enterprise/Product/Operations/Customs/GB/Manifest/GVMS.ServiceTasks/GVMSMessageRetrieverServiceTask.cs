using System.Threading;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSMessageRetrieverServiceTask.Code
	, Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSMessageRetrieverServiceTask.FriendlyName
	, Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSMessageRetrieverServiceTask.MessageServiceTaskCategory
	, typeof(Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSMessageRetrieverServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "300Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSMessageRetrieverServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsGVMSManifest
	},
	"UK Customs GVMS messages inbound"
)]
[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.GB.GVMS.ServiceTasks.GVMSMessageRetrieverServiceTask.Code
	, EDIInterchangeSchema.Constants.TableName
	, new[]
	{
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsGVMSManifest
	},
	"UK Customs GVMS interchanges inbound"
)]

namespace Enterprise.Customs.GB.GVMS.ServiceTasks
{
	public class GVMSMessageRetrieverServiceTask : Customs.ServiceTasks.CustomsServiceTask
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

						using (var interchangeProcessor = new GVMSInboundInterchangeProcessor(logger))
						{
							interchangeProcessor.ExecuteBatch(token);
						}

						using (var messageProcessor = new GVMSIncomingMessageProcessor(logger))
						{
							messageProcessor.ExecuteBatch(token);
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
		
		public const string Code = "GVM";
		public const string FriendlyName = "GB GVMS Message Retriever";
		public const string MessageServiceTaskCategory = "GBC";
	}
}
