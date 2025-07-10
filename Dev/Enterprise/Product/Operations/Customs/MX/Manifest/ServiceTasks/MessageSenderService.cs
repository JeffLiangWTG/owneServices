using System.Threading;
using Enterprise.Customs.MX.Manifest.Business;
using Enterprise.Customs.MX.Manifest.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.MXS,
	ServiceTaskApplicationCodeList.Descriptions.MXS,
	MXMessageConstants.MessageServiceTaskCategory,
	typeof(MessageSenderService),
	MinimumPeriod = "1Minute",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Mexico,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.MXS,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.MXCustoms,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_HeldUntilDate   + " IS PASTORNULL"
	},
	ServiceTaskApplicationCodeList.Descriptions.MXS)]

namespace Enterprise.Customs.MX.Manifest.ServiceTasks
{
	public class MessageSenderService : MessagingService
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Mexico))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = GetNewLogger();
						new MXCOutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}

		[HostedServiceRequirement]
		public static string CheckMXCompanyHasCredentialsExist() => (MessageHostedServiceRequirement.CheckMXManifestEnabled());
	}
}
