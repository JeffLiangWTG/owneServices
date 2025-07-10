using Enterprise.AuditDataServices.Subscription.Testing;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Glow.Test
{
	[TestedType(typeof(GlowSubscriberServiceTask))]
	internal class GlowSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<GlowSubscriberServiceTask>
	{
		public override GlowSubscriberServiceTask GenerateServiceTask()
		{
			return new GlowSubscriberServiceTask();
		}

		protected override bool IsClientSpecific => false;

		public override string ServiceTaskName()
		{
			return GlowSubscriberServiceTask.Description;
		}
	}
}
