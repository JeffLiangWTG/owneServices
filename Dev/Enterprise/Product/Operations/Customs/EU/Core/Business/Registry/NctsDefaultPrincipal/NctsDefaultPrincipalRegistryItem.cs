using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.EU.Registry
{
	public class NctsDefaultPrincipalRegistryItem : StronglyTypedRegistryItem<NctsDefaultPrincipal>
	{
		public NctsDefaultPrincipalRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, NctsDefaultPrincipal defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new NctsDefaultPrincipalRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
