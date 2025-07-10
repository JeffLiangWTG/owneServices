using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	BEMessageServiceTypeList.Codes.BeCustomsMessagesInbound,
	BEMessageServiceTypeList.Descriptions.BeCustomsMessagesInbound,
	Enterprise.Customs.BE.ServiceTasks.MessagingServiceTask.MessageServiceTaskCategory,
	typeof(Enterprise.Customs.BE.ServiceTasks.MessageProcessingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Belgium,
	MinimumPeriod = "60Seconds",
	CanRunInAnyBranch = true,
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(BEMessageServiceTypeList.Codes.BeCustomsMessagesInbound,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.BECustoms,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL" },
	BEMessageServiceTypeList.Descriptions.BeCustomsMessagesInbound
	)]

[assembly: HostedServiceBusinessObjectBinding(
	BEMessageServiceTypeList.Codes.BeCustomsMessagesInbound,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.BECustoms,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"
	},
	queueName: "BE Customs Inbound Message Processing"
)]

namespace Enterprise.Customs.BE.ServiceTasks;

public class MessageProcessingService : BranchMessageProcessorService
{
	protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();

	protected override IEnumerable<ZString> ApplicationCodes => new ZString[] { EDIMessage.ApplicationCodes.BECustoms };

	protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new BECIncomingMessageProcessor();

	[HostedServiceRequirement]
	public static string IsRequired() => CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Belgium, PasswordTypesList.Codes.BEC, PasswordStatusList.Codes.PasswordOK)
		? string.Empty : (NoResString)"There is no Credential configured in Belgium."; // information for logging only.
}
