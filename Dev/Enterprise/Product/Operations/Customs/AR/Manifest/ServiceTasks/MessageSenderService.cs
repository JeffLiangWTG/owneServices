using System.Threading;
using Enterprise.Customs.AR.Manifest.Business;
using Enterprise.Customs.AR.Manifest.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.ARS,
	ServiceTaskApplicationCodeList.Descriptions.ARS,
	ARMessageConstants.MessageServiceTaskCategory,
	typeof(MessageSenderService),
	MinimumPeriod = "60Seconds",
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Argentina,
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskApplicationCodeList.Codes.ARS,
	EDIMessageSchema.Constants.TableName,
	new[] { EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.ARCustoms,
			EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
			EDIMessageSchema.Constants.EM_IsActive + "=Y"
	},
	ServiceTaskApplicationCodeList.Descriptions.ARS)]

namespace Enterprise.Customs.AR.Manifest.ServiceTasks
{
	public class MessageSenderService : MessagingService
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Argentina))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						var log = GetNewLogger();
						new AROutgoingMessageProcessor(log).ProcessMessage(token);
					});
				}
			}
		}

		[HostedServiceRequirement]
		public static string CheckARCompanyHasCredentialsExist() => (MessageHostedServiceRequirement.CheckARCompanyHasCertificate());
	}
}
