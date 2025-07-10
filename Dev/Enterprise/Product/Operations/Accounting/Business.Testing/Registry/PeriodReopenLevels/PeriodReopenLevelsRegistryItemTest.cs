using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(PeriodReopenLevelsRegistryItem))]
	class PeriodReopenLevelsRegistryItemTest : StronglyTypedRegistryItemTestCase<PeriodReopenLevelsCollection>
	{
		protected override StronglyTypedRegistryItem<PeriodReopenLevelsCollection, PeriodReopenLevelsCollection> GetNewRegistryItem()
		{
			return new PeriodReopenLevelsRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default);
		}
	}
}
