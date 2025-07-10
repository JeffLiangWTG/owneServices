using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.StabilityChecker
{
	sealed class StabilityResultsRegistryItem : StronglyTypedRegistryItem<StabilityResults>
	{
		public StabilityResultsRegistryItem(string name)
			: base(new RegistryItemImpl(name, null, null, null, new StabilityResultsRegistryDataType(), RegistryStorageFlags.System, RegistryOptions.IsHidden))
		{ }
	}
}
