using System.Threading;
using Enterprise.Customs.CL.Manifest.Business;
using Enterprise.Customs.CL.Manifest.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.CCS,
	ServiceTaskApplicationCodeList.Descriptions.CCS,
	CLMessageConstants.MessageServiceTaskCategory,
	typeof(MessageSenderService),
	MinimumPeriod = "1Minute",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Chile,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)]

[assembly: HostedServiceBusinessObjectBinding(
	ServiceTaskApplicationCodeList.Codes.CCS,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CLCustoms,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL"
	},
	ServiceTaskApplicationCodeList.Descriptions.CCS)]

namespace Enterprise.Customs.CL.Manifest.ServiceTasks
{
	public class MessageSenderService : MessagingService
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Chile))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = GetNewLogger();
						new CHLOutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}

		[HostedServiceRequirement]
		public static string CheckCLCompanyHasSMSSetupExist() => (MessageHostedServiceRequirement.CheckCLSetupSMSMessageSendingConfig());
	}
}
