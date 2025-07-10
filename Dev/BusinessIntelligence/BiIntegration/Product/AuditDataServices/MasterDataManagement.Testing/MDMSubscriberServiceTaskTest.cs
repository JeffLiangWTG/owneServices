using Enterprise.AuditDataServices.Subscription.Testing;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(MDMSubscriberServiceTask))]
	internal class MDMSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<MDMSubscriberServiceTask>
	{
		public override MDMSubscriberServiceTask GenerateServiceTask()
		{
			return new MDMSubscriberServiceTask();
		}

		protected override bool IsClientSpecific => false;

		public override string ServiceTaskName()
		{
			return MDMSubscriberServiceTask.Description;
		}
	}
}
