using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class RatingTokenAuthenticationRegistryItem : StronglyTypedRegistryItem<RatingTokenAuthenticationCollection>
	{
		public RatingTokenAuthenticationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, RatingTokenAuthenticationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new RatingTokenAuthenticationCollectionRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.RatingTokenAuthenticationRegistryItemEditor, Enterprise.Registry.GUI")]
	public class RatingTokenAuthenticationCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<RatingTokenAuthenticationCollection>
	{
		public RatingTokenAuthenticationCollectionRegistryDataType()
		{
		}
	}
}
