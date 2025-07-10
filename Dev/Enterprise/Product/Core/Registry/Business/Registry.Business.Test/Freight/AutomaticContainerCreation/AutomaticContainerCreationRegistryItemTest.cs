using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AutomaticContainerCreationRegistryItem))]
	sealed class AutomaticContainerCreationRegistryItemTest : StronglyTypedRegistryItemTestCase<AutomaticContainerCreation>
	{
		protected override StronglyTypedRegistryItem<AutomaticContainerCreation, AutomaticContainerCreation> GetNewRegistryItem()
		{
			return new AutomaticContainerCreationRegistryItem("", null, null, null);
		}
	}
}
