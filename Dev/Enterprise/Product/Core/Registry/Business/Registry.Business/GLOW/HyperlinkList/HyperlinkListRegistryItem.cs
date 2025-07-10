using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class HyperlinkListRegistryItem : StronglyTypedRegistryItem<HyperlinkCollection>
	{
		public HyperlinkListRegistryItem(string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new HyperlinkListRegistryDataType(), storage, options, new HyperlinkCollection()))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.HyperlinkListRegistryItemEditor, Enterprise.Registry.GUI")]
	public class HyperlinkListRegistryDataType : NonPersistentBusinessObjectRegistryDataType<HyperlinkCollection>
	{
	}
}
