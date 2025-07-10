using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OrgBarcodeMaskRegistryItem : StronglyTypedRegistryItem<OrgBarcodeMaskCollection>
	{
		public OrgBarcodeMaskRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new OrgBarcodeMaskDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.OrgBarcodeMaskRegistryItemEditor, Enterprise.Registry.GUI")]
	class OrgBarcodeMaskDataType : NonPersistentBusinessObjectRegistryDataType<OrgBarcodeMaskCollection>
	{
		public OrgBarcodeMaskDataType()
		{
		}
	}
}
