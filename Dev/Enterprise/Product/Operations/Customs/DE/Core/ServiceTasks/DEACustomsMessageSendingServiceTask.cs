using System.Threading;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Codes.DEAMessageSending,
	Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.DEAMessageSending,
	"DEC",
	typeof(Enterprise.Customs.DE.ServiceTasks.DEACustomsMessageSendingServiceTask),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Germany,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.DE.ServiceTasks.ServiceTaskApplicationCodeList.Codes.DEAMessageSending,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.DECustomsAtlasSystem,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"DE Atlas Message Sending"
	)]

namespace Enterprise.Customs.DE.ServiceTasks
{
	public class DEACustomsMessageSendingServiceTask : Customs.ServiceTasks.CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Germany))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() => new DEAOutgoingMessageProcessor(Logger).ProcessMessage(token));
				}
			}
		}
	}
}
