using System.Threading;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using static Enterprise.Core.Constants;

[assembly: HostedService(
	ServiceTaskApplicationCodeList.Codes.MessageSender,
	ServiceTaskApplicationCodeList.Descriptions.MessageSender,
	BranchMessagingService.MessageServiceTaskCategory,
	typeof(MessageSendingService),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Switzerland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
	)]

[assembly: HostedServiceBusinessObjectBinding(
	ServiceTaskApplicationCodeList.Codes.MessageSender,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CHCustomsEdec,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
	},
	ServiceTaskApplicationCodeList.Descriptions.MessageSender + " " + EDIMessage.ApplicationCodes.CHCustomsEdec
	)]

[assembly: HostedServiceBusinessObjectBinding(
	ServiceTaskApplicationCodeList.Codes.MessageSender,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CHCustomsPassar,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
	},
	ServiceTaskApplicationCodeList.Descriptions.MessageSender + " " + EDIMessage.ApplicationCodes.CHCustomsPassar
	)]

[assembly: HostedServiceBusinessObjectBinding(
	ServiceTaskApplicationCodeList.Codes.MessageSender,
	EDIMessageSchema.Constants.TableName,
	new[] {
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.CHCustomsCharteraOutput,
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
	},
	ServiceTaskApplicationCodeList.Descriptions.MessageSender + " " + EDIMessage.ApplicationCodes.CHCustomsCharteraOutput
	)]

namespace Enterprise.Customs.CH.ServiceTasks;

public class MessageSendingService : BranchMessagingService
{
	protected override void RunTaskForEachBranch(CancellationToken token)
	{
		OutboundMessageProcessor.ProcessMessages(Logger, token);
	}

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
}
