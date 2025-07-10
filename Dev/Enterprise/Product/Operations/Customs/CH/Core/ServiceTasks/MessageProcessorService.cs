using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly:
	HostedService(
		ServiceTaskApplicationCodeList.Codes.MessageProcessor,
		ServiceTaskApplicationCodeList.Descriptions.MessageProcessor,
		BranchMessagingService.MessageServiceTaskCategory,
		typeof(MessageProcessorService),
		RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Switzerland,
		CanRunInAnyBranch = true,
		MinimumPeriod = "1minute",
		DefaultScheduleRunEvery = "15minutes",
		ActiveByDefault = true
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		ServiceTaskApplicationCodeList.Codes.MessageProcessor,
		EDIMessageSchema.Constants.TableName,
		new[] {
			EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CHCustomsEdec,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		},
		ServiceTaskApplicationCodeList.Descriptions.MessageProcessor + " " + EDIMessage.ApplicationCodes.CHCustomsEdec
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		ServiceTaskApplicationCodeList.Codes.MessageProcessor,
		EDIMessageSchema.Constants.TableName,
		new[] {
			EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CHCustomsPassar,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		},
		ServiceTaskApplicationCodeList.Descriptions.MessageProcessor + " " + EDIMessage.ApplicationCodes.CHCustomsPassar
	)]

[assembly:
	HostedServiceBusinessObjectBinding(
		ServiceTaskApplicationCodeList.Codes.MessageProcessor,
		EDIMessageSchema.Constants.TableName,
		new[] {
			EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
			EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
			EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.CHCustomsCharteraOutput,
			EDIMessageSchema.Constants.EM_IsActive + "=Y",
			EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		},
		ServiceTaskApplicationCodeList.Descriptions.MessageProcessor + " " + EDIMessage.ApplicationCodes.CHCustomsCharteraOutput
	)]

namespace Enterprise.Customs.CH.ServiceTasks;

public class MessageProcessorService : BranchMessageProcessorService
{
	[HostedServiceRequirement]
	public static string IsRequired()
	{
		bool required =
			CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Switzerland, PasswordTypesList.Codes.CHT)
			|| CertificateRequirementChecker.ExistsCompanyWithCertificate(CountryCodes.Switzerland, PasswordTypesList.Codes.CHC);
		return required
			? string.Empty
			: (NoResString)"There is no Token Credential / Certificate configured in Switzerland.";
	}

	protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new InboundMessageProcessorHub(ApplicationCodes, MessageTypes);

	protected override IEnumerable<ZString> ApplicationCodes => new ZString[]
	{
			EDIMessage.ApplicationCodes.CHCustomsEdec,
			EDIMessage.ApplicationCodes.CHCustomsPassar,
			EDIMessage.ApplicationCodes.CHCustomsCharteraOutput,
	};

	protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();
}
