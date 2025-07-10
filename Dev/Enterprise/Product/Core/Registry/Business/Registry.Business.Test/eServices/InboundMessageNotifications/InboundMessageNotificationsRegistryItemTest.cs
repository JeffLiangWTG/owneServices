using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InboundMessageNotificationsRegistryItem))]
	sealed class InboundMessageNotificationsRegistryItemTest : StronglyTypedRegistryItemTestCase<InboundMessageNotificationsRule>
	{
		protected override StronglyTypedRegistryItem<InboundMessageNotificationsRule, InboundMessageNotificationsRule> GetNewRegistryItem()
		{
			return new InboundMessageNotificationsRegistryItem(string.Empty, null, null, null, RegistryStorageFlags.System, new InboundMessageNotificationsRule());
		}
	}
}
