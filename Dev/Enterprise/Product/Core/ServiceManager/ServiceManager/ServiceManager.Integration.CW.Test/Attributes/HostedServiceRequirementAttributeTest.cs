using System;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW;

namespace Enterprise.ServiceManager.Shared.Testing
{
	class HostedServiceRequirementAttributeTest : TransactionedTestCase
	{
		public void TestCheckValueIsAmong()
		{
			var registryItem = RawDataRegistry.Instance.FaxDestinationOverride;

			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "AAA"))
			{
				var result = HostedServiceRequirementAttribute.CheckValueIsAmong(registryItem, "BBB", "CCC");
				AssertMultilineASCIIEquals("The registry setting 'System -> Testing -> Fax Destination Override' requires one of the following values: 'BBB', 'CCC'.", result);
			}

			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "BBB"))
			{
				var result = HostedServiceRequirementAttribute.CheckValueIsAmong(registryItem, "BBB", "CCC");
				AssertMultilineASCIIEquals(string.Empty, result);
			}

			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "CCC"))
			{
				var result = HostedServiceRequirementAttribute.CheckValueIsAmong(registryItem, "BBB", "CCC");
				AssertMultilineASCIIEquals(string.Empty, result);
			}
		}
	}
}
