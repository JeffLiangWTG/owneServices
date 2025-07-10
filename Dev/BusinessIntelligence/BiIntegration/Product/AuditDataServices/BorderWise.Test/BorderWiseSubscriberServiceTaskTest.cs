using Enterprise.AuditDataServices.Subscription.Testing;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	[TestedType(typeof(BorderWiseSubscriberServiceTask))]
	class BorderWiseSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<BorderWiseSubscriberServiceTask>
	{
		public override BorderWiseSubscriberServiceTask GenerateServiceTask()
		{
			return new BorderWiseSubscriberServiceTask();
		}

		protected override bool IsClientSpecific => true;

		public override string ServiceTaskName()
		{
			return BorderWiseSubscriberServiceTask.Description;
		}

		protected override string[] GetExpectedHostedServiceRequirements()
		{
			return new[] { "IsEdiClient", "CheckCdcIsEnabled", "IsAuditEnabled", "IsBorderwiseRegistryItemEnabled", "CheckCdcShouldBeDisabled" };
		}

		public override void TestSubscriberServiceTaskAssemblyName()
		{
			var subscriberTask = GenerateServiceTask();

			AssertEquals("Enterprise.AuditDataServices.BorderWise", subscriberTask.AssemblyName);
		}
	}
}
