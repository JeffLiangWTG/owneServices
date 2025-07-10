using System;
using Enterprise.Client.EDI.ServiceTasks.TestDirectXt;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.ServiceTasks;
using Enterprise.ZArchitecture.Schema;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(TestDirectXtOutboundServiceTask.ServiceTaskCode,
	TestDirectXtOutboundServiceTask.ServiceTaskDescription,
	"ESV",
	typeof(TestDirectXtOutboundServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "15minutes",
	ActiveByDefault = true)
]

[assembly: HostedServiceBusinessObjectBinding(TestDirectXtOutboundServiceTask.ServiceTaskCode,
	EDIInterchangeSchema.Constants.TableName,
	new[] {
		EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
		EDIInterchangeSchema.Constants.EI_Status + "=" + EDIMessage.Status.Queued,
		EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
		EDIInterchangeSchema.Constants.EI_TransportType + "=" + EDIInterchange.TransportType.tXT
	},
	"xT Test Message Sending"
)]

namespace Enterprise.Client.EDI.ServiceTasks.TestDirectXt
{
	public class TestDirectXtOutboundServiceTask : TestDirectXtServiceTask
	{
		[HostedServiceRequirement]
		public static string CheckIsClientProductionDatabase() => DirectxTServiceTask.IsProductionDatabase() ? string.Empty : "This is not a production system.";

		public const string ServiceTaskCode = "TXO";
		public const string ServiceTaskDescription = "XT Outbound Test Messages Service Task";

		protected override DateTime OutageStartTime
		{
			get => EDIDataRegistry.Instance.TXOOutageStartTime.Value;
			set => EDIDataRegistry.Instance.TXOOutageStartTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		protected override IInterchangeProcessor GetMessageProcessor()
		{
			return new TestDirectXtOutboundProcessor(ServiceLogger);
		}
	}
}
