using System.Linq;
using Enterprise.AuditDataServices.Subscription.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.AuditDataServices.EServices.Test
{
	[TestedType(typeof(EServicesSubscriberServiceTask))]
	class EServicesSubscriberServiceTaskTest : AuditSubscriberTaskTestBase<EServicesSubscriberServiceTask>
	{
		public override EServicesSubscriberServiceTask GenerateServiceTask()
		{
			return new EServicesSubscriberServiceTask();
		}

		protected override bool IsClientSpecific => true;

		public override string ServiceTaskName()
		{
			return EServicesSubscriberServiceTask.Description;
		}

		public override void TestTaskServiceRequirementsAttribute()
		{
			AssertContainsExactElementsInAnyOrder(
				"Missing HostedServiceRequirement methods",
				new[] { "IsEdiClient", "CheckCdcIsEnabled", "IsAuditEnabled", "CheckCdcShouldBeDisabled" },
				new[]
				{
					typeof(EServicesSubscriberServiceTask).GetMethods(),
					typeof(EServicesSubscriberServiceTask).BaseType.GetMethods(),
					typeof(EServicesSubscriberServiceTask).BaseType.BaseType.GetMethods(),
				}
					.SelectMany(methods => methods.Where(m => m.GetCustomAttributes(typeof(HostedServiceRequirementAttribute), false).Length > 0))
					.Select(m => m.Name));
		}

		public override void TestSubscriberServiceTaskAssemblyName()
		{
			var subscriberTask = GenerateServiceTask();

			AssertEquals("ZClientEDI.Business", subscriberTask.AssemblyName);
		}
	}
}
