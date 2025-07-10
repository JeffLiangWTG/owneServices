using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	public class MessageVersionRegistryItem : StronglyTypedRegistryItem<MessageVersionRegistryCollection>
	{
		public MessageVersionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, MessageVersionRegistryCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new MessageVersionDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}
