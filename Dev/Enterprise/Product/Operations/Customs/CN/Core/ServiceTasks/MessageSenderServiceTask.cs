using System.Threading;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	code: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageSenderServiceTask,
	description: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Descriptions.CNMessageSenderServiceTask,
	category: "CNC",
	type: typeof(Enterprise.Customs.CN.ServiceTasks.MessageSenderServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.China,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Second",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageSenderServiceTask,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	queueName: Enterprise.Customs.CN.ServiceTasks.ServiceTaskApplicationCodeList.Codes.CNMessageSenderServiceTask
)]

namespace Enterprise.Customs.CN.ServiceTasks
{
	public class MessageSenderServiceTask : CustomsServiceTask
	{
		protected override void RunTaskCore(CancellationToken token)
		{
			foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.China))
			{
				token.ThrowIfCancellationRequested();
				using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
				{
					RunTaskHandleEmailSendFailure(() =>
					{
						new CSWOutgoingMessageProcessor(Logger).ProcessMessage(token);
					});
				}
			}
		}

		[HostedServiceRequirement]
		public static string CheckCNSWClientSetting() => ServiceTaskEnvironmentChecker.CheckCNSWClientSetting();
	}
}
