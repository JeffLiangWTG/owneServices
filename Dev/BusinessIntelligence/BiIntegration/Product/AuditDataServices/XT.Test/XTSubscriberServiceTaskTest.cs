using CargoWise.Definitions;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.XT.Test
{
	[TestedType(typeof(XTSubscriberServiceTask))]
	class XTSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<XTSubscriberServiceTask>
	{
		public override XTSubscriberServiceTask GenerateServiceTask()
		{
			return new XTSubscriberServiceTask();
		}

		public override string ServiceTaskName()
		{
			return XTSubscriberServiceTask.Description;
		}

		protected override string[] GetExpectedHostedServiceRequirements()
		{
			return new[] { "IsEdiClient", "CheckCdcIsEnabled", "IsAuditEnabled", "CheckCdcShouldBeDisabled" };
		}

		public override void TestIsLoaded()
		{
			using (ClientHookLoader.Instance.OverrideClientHookForTest(new TestClientOverride(Clients.EDI)))
			{
				base.TestIsLoaded();
			}
		}

		protected override bool IsClientSpecific => true;
	}
}
