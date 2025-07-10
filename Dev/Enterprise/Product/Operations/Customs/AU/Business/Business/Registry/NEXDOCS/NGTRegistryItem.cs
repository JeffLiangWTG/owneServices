using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NGTRegistryItem : StronglyTypedRegistryItem<NGT>
	{
		public NGTRegistryItem(ZString name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(name, category, caption, hint, new NGTRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.IsOnlyForController))
		{
		}
	}
}
