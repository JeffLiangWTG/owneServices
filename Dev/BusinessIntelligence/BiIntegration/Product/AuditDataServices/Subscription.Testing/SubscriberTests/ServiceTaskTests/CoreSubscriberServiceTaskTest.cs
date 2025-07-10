using Enterprise.AuditDataServices.Core;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Subscription.Testing
{
	[TestedType(typeof(CoreSubscriberServiceTask))]
	class CoreSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<CoreSubscriberServiceTask>
	{
		public override CoreSubscriberServiceTask GenerateServiceTask()
		{
			return new CoreSubscriberServiceTask();
		}

		public override string ServiceTaskName()
		{
			return CoreSubscriberServiceTask.Description;
		}

		protected override bool IsClientSpecific => false;
	}
}
