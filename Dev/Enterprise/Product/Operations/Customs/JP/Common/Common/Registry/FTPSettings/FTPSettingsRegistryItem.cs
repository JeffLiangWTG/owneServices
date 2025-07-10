using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common
{
	public class FTPSettingsRegistryItem : StronglyTypedRegistryItem<FTPSettings>
	{
		public FTPSettingsRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage,
					RegistryOptions options)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new FTPSettingsRegistryItemRegistryDataType(), storage, options))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.JP.GUI.FTPSettingsRegistryItemEditor, Enterprise.Customs.JP.GUI")]
	public class FTPSettingsRegistryItemRegistryDataType : NonPersistentBusinessObjectRegistryDataType<FTPSettings>
	{
		public FTPSettingsRegistryItemRegistryDataType()
		{
		}
	}
}
