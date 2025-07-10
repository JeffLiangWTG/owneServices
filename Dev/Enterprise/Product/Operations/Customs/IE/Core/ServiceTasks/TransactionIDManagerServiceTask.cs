using System.Threading;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.ServiceTasks;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using EDIInterchange = Enterprise.Messaging.Business.EDIInterchange;

[assembly: HostedService(
	code: ServiceTaskApplicationCodeList.Codes.IETransactionIDManager,
	description: ServiceTaskApplicationCodeList.Descriptions.IETransactionIDManager,
	category: Enterprise.Customs.IE.ServiceTasks.Constants.ServiceTaskCategory,
	type: typeof(TransactionIDManagerServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Ireland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IETransactionIDManager,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIInterchange.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationReference + "=",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsExport,
	},
	queueName: "IE Customs Export TransactionID"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IETransactionIDManager,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIInterchange.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationReference + "=",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsImport,
	},
	queueName: "IE Customs Import TransactionID"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IETransactionIDManager,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIInterchange.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationReference + "=",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsUCC5Import,
	},
	queueName: "IE Customs Import UCC5 TransactionID"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IETransactionIDManager,
	table: EDIInterchangeSchema.Constants.TableName,
	predicates: new[] {
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsCommon,
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchangeTransportTypeList.Codes.xT,
		EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + CommonInterchangeTypeList.Codes.TransactionID
	},
	queueName: "IE Customs TransactionID Request"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IETransactionIDManager,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIInterchange.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIInterchange.Direction.Transmit,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_ApplicationReference + "=",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIInterchange.ApplicationCodes.IECustomsNCTS,
	},
	queueName: "IE Customs NCTS TransactionID"
)]

namespace Enterprise.Customs.IE.ServiceTasks
{
	public class TransactionIDManagerServiceTask : NonBranchSpecificServiceTask
	{
		[HostedServiceRequirement]
		public static string IsRequired() => ServiceTaskHelper.GetCertificateMessageError();

		protected override void RunTaskCore(CancellationToken youMustReactToThisToken)
		{
			TransactionIDManager.New(ServiceLogger, false).Process(youMustReactToThisToken);
		}
	}
}
