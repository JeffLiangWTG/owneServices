using System.Threading;
using Enterprise.Customs.BR.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.BR.ServiceTasks.MessageRetrieverService.Code,
	Enterprise.Customs.BR.ServiceTasks.MessageRetrieverService.FriendlyName,
	Enterprise.Customs.BR.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.BR.ServiceTasks.MessageRetrieverService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Brazil,
	CanRunInAnyBranch = true,
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.BR.ServiceTasks.MessageRetrieverService.Code,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Integration.ReceiveTransmitList.Codes.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.BRCustoms },
	"BR Customs messages inbound"
)]

namespace Enterprise.Customs.BR.ServiceTasks
{
	public class MessageRetrieverService : MessagingService
	{
		public const string Code = "BRP";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public const string FriendlyName = "Brazil Customs Message Processor";

		[HostedServiceRequirement]
		public static string IsRequired() => GlbExternalPasswordHelper.CheckAnyStaffHasCCTCertificate();

		protected override void RunTaskForEachBranch(CancellationToken token)
		{
			using (var messageProcessor = new BRCIncomingMessageProcessor(Logger))
			{
				messageProcessor.ExecuteBatch(token);
			}
		}
	}
}
