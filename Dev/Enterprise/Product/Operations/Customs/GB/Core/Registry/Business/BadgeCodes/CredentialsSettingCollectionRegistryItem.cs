using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	public class CredentialsSettingCollectionRegistryItem : StronglyTypedRegistryItem<CredentialsSettingCollection>
	{
		public CredentialsSettingCollectionRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new CredentialsSettingRegistryDataType(), storage))
		{
		}

		public CredentialsSettingCollectionRegistryItem(string name, MultilingualString category, string caption, string hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, (NoResString)caption, (NoResString)hint, new CredentialsSettingRegistryDataType(), storage, options))
		{
		}

		[RegistryEditor("Enterprise.Customs.GB.GUI.Registry.CredentialsRegistryItemEditor, Enterprise.Customs.GB.GUI")]
		internal class CredentialsSettingRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CredentialsSettingCollection>
		{
			public CredentialsSettingRegistryDataType()
			{
			}
		}
	}
}
