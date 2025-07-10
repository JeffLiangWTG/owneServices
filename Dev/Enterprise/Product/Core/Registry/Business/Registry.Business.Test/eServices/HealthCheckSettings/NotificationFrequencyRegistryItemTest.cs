using Enterprise.Integration;
using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NotificationFrequencyRegistryItem))]
	sealed class NotificationFrequencyRegistryItemTest : StronglyTypedRegistryItemTestCase<NotificationFrequency>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<NotificationFrequency, NotificationFrequency> GetNewRegistryItem()
		{
			return new NotificationFrequencyRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		#endregion
	}
}
