using System.Threading;
using Enterprise.Customs.GB.CDS.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	CDSMessageSenderServiceTask.Code
	, CDSMessageSenderServiceTask.FriendlyName
	, CDSMessageServiceTask.MessageServiceTaskCategory
	, typeof(CDSMessageSenderServiceTask)
	, RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.UnitedKingdom
	, CanRunInAnyBranch = true
	, MinimumPeriod = "60Seconds"
	, DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	CDSMessageSenderServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCustomsDeclarationServices
	},
	"UK Customs CDS messages outbound CDS"
)]

[assembly: HostedServiceBusinessObjectBinding(
	CDSMessageSenderServiceTask.Code
	, EDIMessageSchema.Constants.TableName
	, new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessageStatusList.Codes.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + ReceiveTransmitList.Codes.Transmit,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.GbCDSDISQuery
	},
	"UK Customs CDS messages outbound CDS DIS"
)]

namespace Enterprise.Customs.GB.CDS.ServiceTasks
{
	public class CDSMessageSenderServiceTask : CDSMessageServiceTask
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
						new CDSOutgoingMessageProcessor(log).ProcessMessage(token);
						new CDSDISQueryMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}

		public const string Code = CDSServiceTaskConstants.CDSMessageSenderServiceTaskCode;
		public const string FriendlyName = "GB CDS Message Sender";
	}
}
