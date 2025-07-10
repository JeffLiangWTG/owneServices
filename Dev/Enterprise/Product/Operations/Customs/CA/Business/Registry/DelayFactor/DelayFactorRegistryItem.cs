using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Registry
{
	public class DelayFactorRegistryItem : StronglyTypedRegistryItem<DelayFactorRegistryBusinessObject>
	{
		public DelayFactorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, DelayFactorRegistryBusinessObject defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelayFactorRegistryDataType(), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default, defaultValue))
		{
		}

		public DelayFactorRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryOptions options, DelayFactorRegistryBusinessObject defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DelayFactorRegistryDataType(), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.CA.GUI.DelayFactorRegistryItemEditor, Enterprise.Customs.CA.GUI")]
	class DelayFactorRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DelayFactorRegistryBusinessObject>
	{
	}
}
