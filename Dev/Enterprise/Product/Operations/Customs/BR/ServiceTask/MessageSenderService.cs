using System.Threading;
using Enterprise.Customs.BR.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.BR.ServiceTasks.MessageSenderService.Code,
	Enterprise.Customs.BR.ServiceTasks.MessageSenderService.FriendlyName,
	Enterprise.Customs.BR.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.BR.ServiceTasks.MessageSenderService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Brazil,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly:
	HostedServiceBusinessObjectBinding(
	Enterprise.Customs.BR.ServiceTasks.MessageSenderService.Code,
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.BRCustoms
	},
	Enterprise.Customs.BR.ServiceTasks.MessageSenderService.FriendlyName
)]

namespace Enterprise.Customs.BR.ServiceTasks
{
	public class MessageSenderService : MessagingService
	{
		public const string Code = "BRS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service name")]
		public const string FriendlyName = "Brazil Customs Interchange Sender";

		[HostedServiceRequirement]
		public static string IsRequired() => GlbExternalPasswordHelper.CheckAnyStaffHasCCTCertificate();

		protected override void RunTaskForEachBranch(CancellationToken token) => new BRCOutgoingMessageProcessor(Logger).ProcessMessage(token);
	}
}
