using System.Collections.Generic;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.BR.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	MessageInterchangeProcessorService.Code,
	MessageInterchangeProcessorService.FriendlyName,
	MessagingService.MessageServiceTaskCategory,
	typeof(MessageInterchangeProcessorService),
	CanRunInAnyBranch = true,
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Brazil,
	MinimumPeriod = "30seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly:
	HostedServiceBusinessObjectBinding(
	MessageInterchangeProcessorService.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + Enterprise.Messaging.Business.EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.BRCustoms
	},
	MessageInterchangeProcessorService.FriendlyName
)]

namespace Enterprise.Customs.BR.ServiceTasks
{
	public class MessageInterchangeProcessorService : BranchInterchangeProcessorService
	{
		public const string Code = "BRI";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "programmatic constant")]
		public const string FriendlyName = "Brazil Customs Interchange Processor";

		[HostedServiceRequirement]
		public static string IsRequired() => GlbExternalPasswordHelper.CheckAnyStaffHasCCTCertificate();

		protected override Messaging.Business.BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor() => new BRCInboundInterchangeProcessor(ApplicationCodes);

		protected override IEnumerable<string> ApplicationCodes => new string[] { ApplicationCodeList.Codes.BRCustoms };
	}
}
