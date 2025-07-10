using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class LandedCostingPreferencesRegistryItem : StronglyTypedRegistryItem<LandedCostingGroupCollection>
	{
		public LandedCostingPreferencesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, LandedCostingGroupCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new LandedCostingPreferencesRegistryDataType(), storage, defaultValue))
		{
		}

		public LandedCostingPreferencesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, LandedCostingGroupCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new LandedCostingPreferencesRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.LandedCostingPreferencesRegistryItemEditor, Enterprise.Registry.GUI")]
	class LandedCostingPreferencesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<LandedCostingGroupCollection>
	{
		public LandedCostingPreferencesRegistryDataType()
		{
		}

		protected override LandedCostingGroupCollection CloneValue(LandedCostingGroupCollection value)
		{
			return (LandedCostingGroupCollection)value.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), null);
		}
	}
}
