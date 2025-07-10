using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RatesPrioritiesRegistryItem))]
	sealed class RatesPrioritiesRegistryItemTest : StronglyTypedRegistryItemTestCase<RatesPrioritiesCollection>
	{
		#region Implementation

		protected override StronglyTypedRegistryItem<RatesPrioritiesCollection, RatesPrioritiesCollection> GetNewRegistryItem()
		{
			return new RatesPrioritiesRegistryItem("", null, null, null, RegistryStorageFlags.System, new RatesPrioritiesCollection());
		}

		#endregion
	}
}
