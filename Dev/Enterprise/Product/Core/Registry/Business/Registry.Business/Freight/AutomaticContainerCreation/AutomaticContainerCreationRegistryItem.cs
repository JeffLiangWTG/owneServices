using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AutomaticContainerCreationRegistryItem : StronglyTypedRegistryItem<AutomaticContainerCreation>
	{
		public AutomaticContainerCreationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(name, category, caption, hint, new AutomaticContainerCreationRegistryDataType(), RegistryStorageFlags.System, RegistryOptions.Default))
		{
		}
	}
}
