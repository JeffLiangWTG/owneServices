using System.Threading;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	Enterprise.Customs.BE.ServiceTasks.BECustomsSenderServiceTask.Code,
	Enterprise.Customs.BE.ServiceTasks.BECustomsSenderServiceTask.FriendlyName,
	Enterprise.Customs.BE.ServiceTasks.MessagingServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.BE.ServiceTasks.BECustomsSenderServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Belgium,
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes"
)]

[assembly: HostedServiceBusinessObjectBinding(
	Enterprise.Customs.BE.ServiceTasks.BECustomsSenderServiceTask.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_Status + "=" + EDIInterchange.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.BECustoms,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	"BE Customs Message Sender"
)]

namespace Enterprise.Customs.BE.ServiceTasks;

public class BECustomsSenderServiceTask : MessagingServiceTask
{
	public const string Code = "BES";
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Service Task Name")]
	public const string FriendlyName = "BE Customs Message Sender";

	protected override void RunTaskCore(CancellationToken token)
	{
		foreach (var branch in GlbBranch.GetOneActiveBranchPerCompany(Core.Constants.CountryCodes.Belgium))
		{
			token.ThrowIfCancellationRequested();
			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				RunTaskHandleEmailSendFailure(() =>
				{
					var log = GetNewLogger();
					new BEOutgoingMessageProcessor(log).ProcessMessage(token);
				});
			}
		}
	}

	[HostedServiceRequirement]
	public static string IsRequired() =>
		CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Belgium, PasswordTypesList.Codes.BEC, PasswordStatusList.Codes.PasswordOK)
			? string.Empty : (NoResString)"There is no Credential configured in Belgium."; // information for logging only.
}
