using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class PurgeSettingsRegistryItem : StronglyTypedRegistryItem<PurgeSettings>
	{
		public PurgeSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, PurgeSettings settings)
			: base(new RegistryItemImpl(name, category, caption, hint, new PurgeSettingsRegistryDataType(), storage, settings))
		{
		}

		public PurgeSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, PurgeSettings settings)
			: base(new RegistryItemImpl(name, category, caption, hint, new PurgeSettingsRegistryDataType(), storage, options, settings))
		{
		}

		protected override object GetValueWithoutFallbackCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var dbsettings = (PurgeSettings)base.GetValueWithoutFallbackCore(companyPK, branchPK, departmentPK);
			dbsettings.SetApplicationCodes(new ApplicationCodeObjCollectionMergeManager().MergeApplicationCodeObjCollections(eHubMessagingRegistry.GetDefaultPurgeSettings().ApplicationCodes, dbsettings.ApplicationCodes));
			return dbsettings;
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.PurgeSettingsRegistryItemEditor, Enterprise.Registry.GUI")]
	public class PurgeSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PurgeSettings>
	{
		public PurgeSettingsRegistryDataType() : base(new PurgeSettings())
		{
		}
	}
}
