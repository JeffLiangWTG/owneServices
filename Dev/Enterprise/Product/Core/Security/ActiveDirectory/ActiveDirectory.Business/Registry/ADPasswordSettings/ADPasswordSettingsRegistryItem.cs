using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	public class ADPasswordSettingsRegistryItem : RegistryItemWrapper
	{
		public ADPasswordSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new ADPasswordSettingsRegistryDataType(new ADPasswordSettingsRegistryBusinessObject()), storage, options))
		{
		}
	}
}
