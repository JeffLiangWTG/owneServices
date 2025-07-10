using System.Linq;
using Enterprise.AuditDataServices.Subscription.Testing;
using NUnit.Framework;
namespace Enterprise.AuditDataServices.DevTools.Testing
{
	[TestedType(typeof(DevToolsSubscriberServiceTask))]
	class DevToolsSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<DevToolsSubscriberServiceTask>
	{
		public override DevToolsSubscriberServiceTask GenerateServiceTask()
			=> new DevToolsSubscriberServiceTask();

		protected override bool IsClientSpecific => true;

		public override string ServiceTaskName() => "DevTools subscriber service task";

		protected override string[] GetExpectedHostedServiceRequirements()
			=> base.GetExpectedHostedServiceRequirements()
				.Concat(new[] { nameof(DevToolsSubscriberServiceTask.IsEdiClient) })
				.ToArray();
	}
}
