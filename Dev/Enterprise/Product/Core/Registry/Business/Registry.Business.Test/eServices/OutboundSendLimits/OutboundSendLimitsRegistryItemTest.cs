using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OutboundSendLimitsRegistryItem))]
	sealed class OutboundSendLimitsRegistryItemTest : StronglyTypedRegistryItemTestCase<OutboundSendLimitsRule>
	{
		protected override StronglyTypedRegistryItem<OutboundSendLimitsRule, OutboundSendLimitsRule> GetNewRegistryItem()
		{
			return new OutboundSendLimitsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport);
		}
	}
}
