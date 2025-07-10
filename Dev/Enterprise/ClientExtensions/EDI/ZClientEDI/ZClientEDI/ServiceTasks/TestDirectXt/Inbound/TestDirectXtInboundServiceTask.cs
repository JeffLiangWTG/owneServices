using System;
using Enterprise.Client.EDI.ServiceTasks.TestDirectXt;
using Enterprise.xTMessaging.ServiceTasks;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(TestDirectXtInboundServiceTask.ServiceTaskCode,
	TestDirectXtInboundServiceTask.ServiceTaskDescription,
	"ESV",
	typeof(TestDirectXtInboundServiceTask),
	AllowsMultipleInstances = false,
	IsMandatory = true,
	CanRunInAnyBranch = true,
	MinimumPeriod = "5minutes",
	DefaultScheduleRunEvery = "5minutes",
	ActiveByDefault = true)
]

namespace Enterprise.Client.EDI.ServiceTasks.TestDirectXt
{
	public class TestDirectXtInboundServiceTask : TestDirectXtServiceTask
	{
		[HostedServiceRequirement]
		public static string CheckIsClientProductionDatabase() => DirectxTServiceTask.IsProductionDatabase() ? string.Empty : "This is not a production system.";

		public const string ServiceTaskCode = "TXI";
		public const string ServiceTaskDescription = "XT Inbound Test Messages Service Task";

		protected override DateTime OutageStartTime
		{
			get => EDIDataRegistry.Instance.TXIOutageStartTime.Value;
			set => EDIDataRegistry.Instance.TXIOutageStartTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		protected override IInterchangeProcessor GetMessageProcessor()
		{
			return new TestDirectXtInboundProcessor(ServiceLogger);
		}
	}
}
