using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.ServiceTasks;
using Enterprise.Customs.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;
using Constants = Enterprise.Customs.IE.ServiceTasks.Constants;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;

[assembly: HostedService(
	code: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	description: ServiceTaskApplicationCodeList.Descriptions.IEMessageProcessor,
	category: Constants.ServiceTaskCategory,
	type: typeof(MessageProcessingServiceTask),
	RequiresCompanyInCountry = Enterprise.Core.Constants.CountryCodes.Ireland,
	CanRunInAnyBranch = true,
	MinimumPeriod = "60Seconds",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsExport
	},
	queueName: "IE Export Message Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsExport
	},
	queueName: "IE Export Message Pre-Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsImport
	},
	queueName: "IE Import Message Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsImport
	},
	queueName: "IE Import Message Pre-Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsUCC5Import
	},
	queueName: "IE Import UCC5 Message Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsUCC5Import
	},
	queueName: "IE Import UCC5 Message Pre-Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsEMCS
	},
	queueName: "IE EMCS Message Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsEMCS
	},
	queueName: "IE EMCS Message Pre-Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsNCTS
	},
	queueName: "IE NCTS Message Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsNCTS
	},
	queueName: "IE NCTS Message Pre-Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsAndExcise
	},
	queueName: "IE Customs And Excise Report Message Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsAndExcise
	},
	queueName: "IE Customs And Excise Report Message Pre-Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsPBN
	},
	queueName: "IE PBN Message Processing"
)]

[assembly: HostedServiceBusinessObjectBinding(
	serviceTaskCode: ServiceTaskApplicationCodeList.Codes.IEMessageProcessor,
	table: EDIMessageSchema.Constants.TableName,
	predicates: new[] {
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.PreProcessedOK,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_IsActive + "=Y",
		EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL",
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.IECustomsPBN
	},
	queueName: "IE PBN Message Pre-Processing"
)]

namespace Enterprise.Customs.IE.ServiceTasks
{
	public class MessageProcessingServiceTask : BranchMessageProcessorService
	{
		[HostedServiceRequirement]
		public static string IsRequired() => ServiceTaskHelper.GetCertificateMessageError();

		protected override IEnumerable<ZString> MessageTypes => Enumerable.Empty<ZString>();

		protected override IEnumerable<ZString> ApplicationCodes
		{
			get
			{
				yield return EDIMessage.ApplicationCodes.IECustomsExport;
				yield return EDIMessage.ApplicationCodes.IECustomsImport;
				yield return EDIMessage.ApplicationCodes.IECustomsUCC5Import;
				yield return EDIMessage.ApplicationCodes.IECustomsEMCS;
				yield return EDIMessage.ApplicationCodes.IECustomsNCTS;
				yield return EDIMessage.ApplicationCodes.IECustomsAndExcise;
				yield return EDIMessage.ApplicationCodes.IECustomsPBN;
			}
		}

		protected override BranchCustomsMessageProcessor GetNewBranchCustomsMessageProcessor() => new Business.BranchMessageProcessor();
	}
}
