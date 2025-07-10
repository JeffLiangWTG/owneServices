using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(VerboseLoggingRegistryItem))]
	class VerboseLoggingRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<VerboseLoggingCollection>
	{
		protected override StronglyTypedRegistryItem<VerboseLoggingCollection, VerboseLoggingCollection> GetNewRegistryItem()
		{
			return new VerboseLoggingRegistryItem(string.Empty,
				null,
				null,
				null,
				RegistryStorageFlags.System,
				RegistryOptions.Default);
		}
	}
}
