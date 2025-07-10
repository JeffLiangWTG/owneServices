using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	Enterprise.Customs.ES.ServiceTasks.MessageProcessorService.Code,
	Enterprise.Customs.ES.ServiceTasks.MessageProcessorService.FriendlyName,
	Enterprise.Customs.ES.ServiceTasks.MessagingService.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.ES.ServiceTasks.MessageProcessorService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Spain,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ES.ServiceTasks.MessageProcessorService.Code,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.ESCustomsMessage,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
	},
	Enterprise.Customs.ES.ServiceTasks.MessageProcessorService.FriendlyName + " QUE")]

[assembly: HostedServiceBusinessObjectBinding(Enterprise.Customs.ES.ServiceTasks.MessageProcessorService.Code,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + Enterprise.Messaging.Business.EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_Status + "=" + Enterprise.Messaging.Business.EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.ESCustomsMessage,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
	},
	Enterprise.Customs.ES.ServiceTasks.MessageProcessorService.FriendlyName + " PPS")]

namespace Enterprise.Customs.ES.ServiceTasks
{
	public class MessageProcessorService : Customs.ServiceTasks.BranchMessageProcessorService
	{
		public const string Code = "PES";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public const string FriendlyName = "ES Customs Message Processing";

		protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();

		protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.ESCustomsMessage };

		protected sealed override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new ESBranchCustomsMessageProcessor();
	}
}
