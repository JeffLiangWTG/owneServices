using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Registry
{
	public class AutomatedModificationRegistryItem : StronglyTypedRegistryItem<AutomatedModification>
	{
		public AutomatedModificationRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new AutomatedModificationRegistryDataType(), storage))
		{
		}

		public AutomatedModificationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AutomatedModification defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new AutomatedModificationRegistryDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}
