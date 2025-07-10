using Enterprise.AuditDataServices.Subscription.Testing;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.Telematics.Test
{ 
	[TestedType(typeof(TelematicsSubscriberServiceTask))]
	internal class TelematicsSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<TelematicsSubscriberServiceTask>
	{
		public override TelematicsSubscriberServiceTask GenerateServiceTask()
		{
			return new TelematicsSubscriberServiceTask();
		}

		protected override bool IsClientSpecific => false;

		public override string ServiceTaskName()
		{
			return TelematicsSubscriberServiceTask.Description;
		}
	}
}
