using System.Threading;
using Enterprise.Customs.GB.CDS.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CDSMessageRetrieverServiceTask.Code
	, CDSMessageRetrieverServiceTask.FriendlyName
	, CDSMessageServiceTask.MessageServiceTaskCategory
	, typeof(CDSMessageRetrieverServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "60Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	CDSMessageRetrieverServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	,
	[
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsDeclarationServices
	],
	"UK Customs CDS messages inbound"
)]
[assembly: HostedServiceBusinessObjectBinding(
	CDSMessageRetrieverServiceTask.Code
	, EDIInterchangeSchema.Constants.TableName
	,
	[
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.GbCustomsDeclarationServices
	],
	"UK Customs CDS interchanges inbound"
)]

namespace Enterprise.Customs.GB.CDS.ServiceTasks
{
	public class CDSMessageRetrieverServiceTask : CDSMessageServiceTask
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

						using (var processor = new CDSInboundInterchangeProcessor(logger))
						{
							processor.ExecuteBatch(token);
						}

						using (var processor = new CDSIncomingMessageProcessor(logger))
						{
							processor.ExecuteBatch(token);
						}
					});
				}
			}
		}

		public const string Code = CDSServiceTaskConstants.CDSMessageRetrieverServiceTaskCode;
		public const string FriendlyName = "GB CDS Message Retriever";
	}
}
