using System;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Business;
using Enterprise.xTMessaging.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(ServiceTaskCodeList.Codes.XTO,
	ServiceTaskCodeList.Descriptions.XTO,
	"ESV",
	typeof(OutboundServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "1minute",
	DefaultScheduleRunEvery = "15minutes")
]

[assembly: HostedServiceBusinessObjectBinding(ServiceTaskCodeList.Codes.XTO,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessage.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchange.TransportType.xT
	},
	"xT Message Sending"
)]

namespace Enterprise.xTMessaging.ServiceTasks
{
	public class OutboundServiceTask : DirectxTServiceTask
	{
		protected override IInterchangeProcessor GetMessageProcessor()
		{
			return new OutboundInterchangeProcessor(ServiceLogger);
		}

		protected override DateTime OutageStartTime
		{
			get => DirectxTMessagingRegistry.Instance.XTOOutageStartTime.Value;
			set => DirectxTMessagingRegistry.Instance.XTOOutageStartTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}
	}
}
