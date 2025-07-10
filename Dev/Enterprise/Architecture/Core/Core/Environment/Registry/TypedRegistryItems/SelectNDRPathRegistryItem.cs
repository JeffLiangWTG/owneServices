using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class SelectNDRPathRegistryItem : StronglyTypedRegistryItem<string>
	{
		public SelectNDRPathRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new SelectNDRPathRegistryDataType(lookUpList), null, storage, options, defaultValue, false))
		{
		}
	}
}
