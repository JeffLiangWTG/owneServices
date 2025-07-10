using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class AviationSecurityTrainingRestrictionRegistryItem : StronglyTypedRegistryItem<AviationSecurityTrainingRestriction>
	{
		public AviationSecurityTrainingRestrictionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AviationSecurityTrainingRestriction defaultValue)
			: base(new AviationSecurityTrainingRestrictionRegistryItemImpl(name, category, caption, hint, storage, defaultValue))
		{
		}
	}

	public class AviationSecurityTrainingRestrictionRegistryItemImpl : RegistryItemImpl
	{
		public AviationSecurityTrainingRestrictionRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, AviationSecurityTrainingRestriction defaultValue)
			: base(name, category, caption, hint, new AviationSecurityTrainingRestrictionDataType(), storage, defaultValue)
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.AviationSecurityTrainingRestrictionRegistryItemEditor, Enterprise.Registry.GUI")]
	public class AviationSecurityTrainingRestrictionDataType : NonPersistentBusinessObjectRegistryDataType<AviationSecurityTrainingRestriction>
	{
	}
}
