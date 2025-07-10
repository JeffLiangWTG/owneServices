using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.Registry
{
	public class FallbackSettingsRegistryItem : StronglyTypedRegistryItem<FallbackSettings>
	{
		public FallbackSettingsRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new FallbackSettingsRegistryDataType(), storage))
		{
		}

		public FallbackSettingsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, FallbackSettings defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new FallbackSettingsRegistryDataType(), RegistryStorageFlags.Company, RegistryOptions.Default, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Customs.FR.GUI.Registry.FallbackRegistryItemEditor, Enterprise.Customs.FR.GUI")]
		public class FallbackSettingsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FallbackSettings>
		{
			public FallbackSettingsRegistryDataType()
			{
			}
		}
	}
}
