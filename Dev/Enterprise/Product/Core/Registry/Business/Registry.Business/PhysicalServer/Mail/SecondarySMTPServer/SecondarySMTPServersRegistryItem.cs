using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SecondarySMTPServersRegistryItem : StronglyTypedRegistryItem<SecondarySMTPServerCollection>
	{
		public SecondarySMTPServersRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions regOptions)
			: base(new RegistryItemImpl(name, category, caption, hint, new SecondarySMTPServersRegistryDataType(), storage, regOptions))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.SecondarySMTPServerRegistryItemEditor, Enterprise.Registry.GUI")]
	class SecondarySMTPServersRegistryDataType : NonPersistentBusinessObjectRegistryDataType<SecondarySMTPServerCollection>
	{
	}
}
