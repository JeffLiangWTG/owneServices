using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Codes.DEMMessageSending,
	Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.DEMMessageSending,
	"DEC",
	typeof(Enterprise.Customs.DE.EMCS.ServiceTasks.DEMCustomsMessageSendingServiceTask),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Germany,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Codes.DEMMessageSending,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.DECustomsEmcsSystem
	},
	"DE Emcs Message Sending"
	)]

namespace Enterprise.Customs.DE.EMCS.ServiceTasks
{
	public class DEMCustomsMessageSendingServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Germany))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() => new DEMOutgoingMessageProcessor(Logger).ProcessMessage(token));
				}
			}
		}
	}
}
