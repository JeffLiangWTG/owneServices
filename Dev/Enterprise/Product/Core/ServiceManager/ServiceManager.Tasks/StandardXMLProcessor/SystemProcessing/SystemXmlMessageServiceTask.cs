using System;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ServiceManager.Tasks.StandardXMLProcessor;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService("SYS", "System Message Service", "SYS", typeof(SystemXmlMessageServiceTask),
	IsMandatory = true,
	MaximumPeriod = "24hours",
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes")
]

[assembly: HostedServiceBusinessObjectBinding("SYS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType  + "=" + SystemMessageList.Codes.CustomerServiceResponse
	},
	SystemMessageList.Descriptions.CustomerServiceResponse)]

[assembly: HostedServiceBusinessObjectBinding("SYS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType  + "=" + SystemMessageList.Codes.ReferenceDataUpdate,
	},
	SystemMessageList.Descriptions.ReferenceDataUpdate)]

[assembly: HostedServiceBusinessObjectBinding("SYS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType  + "=" + SystemMessageList.Codes.TranslationFeedbackEntry
	},
	SystemMessageList.Descriptions.TranslationFeedbackEntry
)]

[assembly: HostedServiceBusinessObjectBinding("SYS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType  + "=" + SystemMessageList.Codes.TranslationFeedbackUpdate
	},
	SystemMessageList.Descriptions.TranslationFeedbackUpdate)]

[assembly: HostedServiceBusinessObjectBinding("SYS",
	EDIMessageSchema.Constants.TableName,
	new[]
	{
		EDIMessageSchema.Constants.EM_ApplicationCode + "=" + ApplicationCodeList.Codes.SYS,
		EDIMessageSchema.Constants.EM_Status          + "=" + EDIMessage.Status.Queued,
		EDIMessageSchema.Constants.EM_IsActive        + "=Y",
		EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Receive,
		EDIMessageSchema.Constants.EM_MessageSubType  + "=" + SystemMessageList.Codes.LinkTrack
	},
	SystemMessageList.Descriptions.LinkTrack)]

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor
{
	public class SystemXmlMessageServiceTask : ServiceProviderImpl
	{
		public override void RunTask(CancellationToken token)
		{
			CreateProcessor().Process(ServiceLogger.GetTaskNotificationSubscriber(), token);
		}

		SystemXmlMessageProcessor CreateProcessor()
		{
			Type type = typeof(SystemXmlMessageProcessor);
			type = TypeDecider.GetTypeForBinding(type);
			return (SystemXmlMessageProcessor)Activator.CreateInstance(type);
		}
	}
}
