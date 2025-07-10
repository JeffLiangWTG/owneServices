using System.Threading;
using Enterprise.Customs.CustomsWare.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using ApplicationCodeList = Enterprise.Messaging.Integration.ApplicationCodeList;

[assembly: HostedService(Enterprise.Customs.CustomsWare.ServiceTasks.SubmissionSenderServiceTask.Code,
	"CustomsWare Submission Processor",
	"EUC",
	typeof(Enterprise.Customs.CustomsWare.ServiceTasks.SubmissionSenderServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Switzerland + "," + Enterprise.Core.Constants.CountryCodes.Ireland + "," + Enterprise.Core.Constants.CountryCodes.Netherlands + ","
							+ Enterprise.Core.Constants.CountryCodes.UnitedArabEmirates + "," + Enterprise.Core.Constants.CountryCodes.Belgium + "," + Enterprise.Core.Constants.CountryCodes.Germany,
	CanRunInAnyBranch = true,
	MinimumPeriod = "10Minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.CustomsWare.ServiceTasks.SubmissionSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationCodeList.Codes.CustomsWare,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CustomsWare,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"CustomsWare Submission messages outbound")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.CustomsWare.ServiceTasks.SubmissionSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Pending,
		EDIMessageSchema.Constants.EM_MessageType + "=" + ApplicationCodeList.Codes.CustomsWare,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CustomsWare,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"CustomsWare Submission messages outbound retry")]
namespace Enterprise.Customs.CustomsWare.ServiceTasks
{
	public class SubmissionSenderServiceTask : CustomsServiceTask
	{
		public const string Code = "CWM";

		protected override void RunTaskCore(CancellationToken token)
		{
			using (DisposableEnvironment.ForBranch(Env.CurrentBranchPK))
			{
				RunTaskHandleEmailSendFailure(() =>
				{
					new SubmissionMessageBatchProcessor(Logger).ProcessMessage(token);
				});
			}
		}
	}
}
