using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.Business;

public sealed class EdecBordereauConfigRegistryItem : StronglyTypedRegistryItem<EdecBordereauConfig>
{
	public EdecBordereauConfigRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
	: base(new RegistryItemImpl(name, category, caption, hint, new EdecBordereauConfigRegistryDataType(), storage, new EdecBordereauConfig()))
	{
	}
}
