using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HVLVPreScreeningRuleRegistryItem))]
	sealed class HVLVPreScreeningRuleRegistryItemTest : StronglyTypedRegistryItemTestCase<HVLVDetailsPreScreeningConfiguration>
	{
		protected override StronglyTypedRegistryItem<HVLVDetailsPreScreeningConfiguration, HVLVDetailsPreScreeningConfiguration> GetNewRegistryItem()
		{
			return new HVLVPreScreeningRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
