using Enterprise.AuditDataServices.Subscription.Testing;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.PAVE.Test
{
	[TestedType(typeof(PAVESubscriberServiceTask))]
	internal class PAVESubscriberServiceTaskTest : AuditSubscriberTaskTestBase<PAVESubscriberServiceTask>
	{
		public override PAVESubscriberServiceTask GenerateServiceTask()
		{
			return new PAVESubscriberServiceTask();
		}

		protected override bool IsClientSpecific => false;

		public override string ServiceTaskName()
		{
			return PAVESubscriberServiceTask.Description;
		}
	}
}
