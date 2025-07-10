using System.Linq;
using System.Reflection;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.Customs.IT.ServiceTasks.Testing;

sealed class ServiceTaskArchitectureTest : TestCase
{
	public void TestAllHostedServiceTaskAreSubclassesOfMessagingServiceTask()
	{
		var baseServiceTaskType = typeof(MessagingServiceTask);
		var serviceTasksAssembly = baseServiceTaskType.Assembly;

		var nonCompliantHostedServiceTasks = serviceTasksAssembly
			.GetCustomAttributes<HostedServiceAttribute>()
			.Select(attribute => attribute.Type)
			.Where(hostedServiceTaskType => !hostedServiceTaskType.IsSubclassOf(baseServiceTaskType))
			.Select(hostedServiceTaskType => hostedServiceTaskType.FullName)
			.ToArray();

		if (nonCompliantHostedServiceTasks.Length > 0)
		{
			var failureMessage =
				$"All service tasks must inherit from '{baseServiceTaskType.FullName}' to benefit from the 'IsRequired' [HostedServiceRequirement].\r\n" +
				"The following service tasks do not meet this requirement:\r\n" +
				string.Join("\r\n", nonCompliantHostedServiceTasks);
			Fail(failureMessage);
		}

		Assert(true);
	}
}
