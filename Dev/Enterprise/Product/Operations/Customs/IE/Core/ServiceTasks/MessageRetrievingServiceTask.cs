using System.Threading;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.ServiceTasks;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using EDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

[assembly: HostedService(
	code: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	description: ServiceTaskApplicationCodeList.Descriptions.IEMessageRetriever,
	category: Enterprise.Customs.IE.ServiceTasks.Constants.ServiceTaskCategory,
	type: typeof(MessageRetrievingServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Ireland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsCommon,
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + CommonInterchangeTypeList.Codes.MailboxRequest
	},
	queueName: "IE Customs Mailbox Request"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsCommon,
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + CommonInterchangeTypeList.Codes.MailboxAcknowledge
	},
	queueName: "IE Customs Mailbox Acknowledge"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsExport,
	},
	queueName: "IE Customs Export Message Acknowledgement"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsImport,
	},
	queueName: "IE Customs Import Message Acknowledgement"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsUCC5Import,
	},
	queueName: "IE Customs Import UCC5 Message Acknowledgement"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsEMCS,
		EDIInterchangeSchema.Constants.EI_InterchangeType + "!=" + CommonInterchangeTypeList.Codes.TransactionID
	},
	queueName: "IE Customs EMCS Message"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsNCTS,
	},
	queueName: "IE Customs NCTS Message Acknowledgement"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsAndExcise,
	},
	queueName: "IE Customs And Excise Report Message"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageRetriever,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsPBN,
	},
	queueName: "IE Customs PBN Message"
)]

namespace Enterprise.Customs.IE.ServiceTasks
{
	public class MessageRetrievingServiceTask : NonBranchSpecificServiceTask
	{
		[HostedServiceRequirement]
		public static string IsRequired() => ServiceTaskHelper.GetCertificateMessageError();

		protected override void RunTaskCore(CancellationToken youMustReactToThisToken)
		{
			ExecuteBatch(new MessageRetrievingProcessor(), youMustReactToThisToken);
			MailboxRequester.Request(ServiceLogger, youMustReactToThisToken);
		}
	}
}
