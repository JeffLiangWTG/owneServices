using System.Threading;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

[assembly: HostedService(
	code: ServiceTaskApplicationCodeList.Codes.IEMessageSender,
	description: ServiceTaskApplicationCodeList.Descriptions.IEMessageSender,
	category: "IEC",
	type: typeof(MessageSenderServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Ireland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageSender,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Pending,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsExport,
		EDIMessageSchema.Constants.EM_ApplicationReference + "!="
	},
	queueName: "IE Export Message Sender"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageSender,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Pending,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsImport,
		EDIMessageSchema.Constants.EM_ApplicationReference + "!="
	},
	queueName: "IE Import Message Sender"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageSender,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Pending,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsUCC5Import,
		EDIMessageSchema.Constants.EM_ApplicationReference + "!="
	},
	queueName: "IE Import Message UCC5 Sender"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageSender,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Pending,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsEMCS,
		EDIMessageSchema.Constants.EM_ApplicationReference + "!="
	},
	queueName: "IE EMCS Message Sender"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageSender,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Pending,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsNCTS,
		EDIMessageSchema.Constants.EM_ApplicationReference + "!="
	},
	queueName: "IE NCTS Message Sender"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageSender,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
#pragma warning disable CW1161 // Res.GetString Analyzer
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
#pragma warning restore CW1161 // Res.GetString Analyzer
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsAndExcise,
	},
	queueName: "IE Customs and Excise Reports Message Sender"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageSender,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
#pragma warning disable CW1161 // Res.GetString Analyzer
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
#pragma warning restore CW1161 // Res.GetString Analyzer
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsPBN,
	},
	queueName: "IE Customs PBN Message Sender"
)]

namespace Enterprise.Customs.IE.ServiceTasks
{
	public class MessageSenderServiceTask : CustomsServiceTask
	{
		[HostedServiceRequirement]
		public static string IsRequired() => ServiceTaskHelper.GetCertificateMessageError();

		protected override void RunTaskCore(CancellationToken token)
		{
			var branch = GlbBranch.GetFirstActiveBranch();
			using (Environment.DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				new IEOutgoingMessageProcessor(Logger).ProcessMessage(token);
				new CustomsAndExciseReportMessageProcessor(Logger).ProcessMessage(token);
				new PBNMessageProcessor(Logger).ProcessMessage(token);
			}
		}
	}
}
