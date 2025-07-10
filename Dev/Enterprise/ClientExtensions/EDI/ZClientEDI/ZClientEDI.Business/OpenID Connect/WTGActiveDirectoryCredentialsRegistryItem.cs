using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace ZClientEDI.Business
{
	public class WTGActiveDirectoryCredentialsRegistryItem : StronglyTypedRegistryItem<WTGActiveDirectoryCredentials>
	{
		public WTGActiveDirectoryCredentialsRegistryItem(
			string name,
			MultilingualString category,
			MultilingualString caption,
			MultilingualString hint,
			RegistryStorageFlags storage,
			RegistryOptions options,
			WTGActiveDirectoryCredentials defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new WTGActiveDirectoryCredentialsRegistryDataType(defaultValue), storage, options, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Client.EDI.Registry.GUI.WTGActiveDirectoryCredentialsEditor, ZClientEDI")]
		public class WTGActiveDirectoryCredentialsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<WTGActiveDirectoryCredentials>
		{
			public WTGActiveDirectoryCredentialsRegistryDataType(WTGActiveDirectoryCredentials defaultValue) : base(defaultValue)
			{
			}
		}
	}
}



