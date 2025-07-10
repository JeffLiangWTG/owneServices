using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.IL.ServiceTasks.Testing
{
	[TestedType(typeof(MessagingServiceTask))]

	public sealed class MessagingServiceTaskTest : TestCaseWithFactory
	{
		public void TestHostedServiceRequirement()
		{
			var hostedServiceRequirementMethod = typeof(MessagingServiceTask).GetMethods().FirstOrDefault(p => Attribute.IsDefined(p, typeof(HostedServiceRequirementAttribute)));
			AssertNotNull("Method with Attribute HostedServiceRequirement should exist on MessagingServiceTask", hostedServiceRequirementMethod);

			CombineAssertions(() =>
			{
				AssertEquals(string.Empty, hostedServiceRequirementMethod?.Invoke(null, null));
			});
		}
	}
}
