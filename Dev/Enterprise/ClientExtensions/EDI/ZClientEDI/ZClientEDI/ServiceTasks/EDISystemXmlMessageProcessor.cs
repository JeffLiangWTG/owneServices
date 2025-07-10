using CargoWise.Definitions;
using Enterprise.Client.EDI.IncidentManager.BatchProcessor;
using Enterprise.Client.EDI.LogsReporting.BatchProcessor;
using Enterprise.Client.EDI.VersionReporting.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedServiceBusinessObjectBinding("SYS", EDIMessageSchema.Constants.TableName,
	new string[] {
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.CurrentVersionReport
	}, null, ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("SYS", EDIMessageSchema.Constants.TableName,
	new string[] {
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.DeliveredVersionReport
	}, null, ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("SYS", EDIMessageSchema.Constants.TableName,
	new string[] {
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.LogsReport
	}, null, ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("SYS", EDIMessageSchema.Constants.TableName,
	new string[] {
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.ERequestDocument
	}, null, ClientSpecificCode = Clients.EDI)]

[assembly: HostedServiceBusinessObjectBinding("SYS", EDIMessageSchema.Constants.TableName,
	new string[] {
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType + "=" + SystemMessageList.Codes.UserAccountReport
	}, null, ClientSpecificCode = Clients.EDI)]

namespace Enterprise.Client.EDI.ServiceTasks
{
	public class EDISystemXmlMessageProcessor : SystemXmlMessageProcessor
	{
		public EDISystemXmlMessageProcessor()
		{
			AddSupportedMessage(SystemMessageList.Codes.CurrentVersionReport, typeof(CurrentVersionReportMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.DeliveredVersionReport, typeof(DeliveredVersionReportMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.LogsReport, typeof(LogsReportMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.ERequestDocument, typeof(ERequestDocumentMessageAction));
			AddSupportedMessage(SystemMessageList.Codes.UserAccountReport, typeof(UserAccountReportMessageAction));
		}
	}
}
