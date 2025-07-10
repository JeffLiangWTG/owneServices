using System.Collections.Generic;
using Enterprise.Customs.ES.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(Enterprise.Customs.ES.ServiceTasks.MessageRetrievingService.Code,
	Enterprise.Customs.ES.ServiceTasks.MessageRetrievingService.FriendlyName,
	Enterprise.Customs.ES.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ES.ServiceTasks.MessageRetrievingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Spain,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ES.ServiceTasks.MessageRetrievingService.Code,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + ApplicationCodeList.Codes.ESCustomsMessage
	},
	Enterprise.Customs.ES.ServiceTasks.MessageRetrievingService.FriendlyName
	)]

namespace Enterprise.Customs.ES.ServiceTasks
{
	public class MessageRetrievingService : Customs.ServiceTasks.BranchInterchangeProcessorService
	{
		public const string Code = "ESR";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string FriendlyName = "ES Customs Message Retrieving";

		protected override IEnumerable<string> ApplicationCodes => new string[] { ApplicationCodeList.Codes.ESCustomsMessage };

		protected override BranchInboundInterchangeProcessor GetNewBranchInboundInterchangeProcessor() => new ESCInboundInterchangeProcessor(ApplicationCodes);
	}
}
