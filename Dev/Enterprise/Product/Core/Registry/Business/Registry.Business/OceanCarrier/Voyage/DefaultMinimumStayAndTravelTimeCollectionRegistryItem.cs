using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class DefaultMinimumStayAndTravelTimeCollectionRegistryItem : StronglyTypedRegistryItem<DefaultMinimumStayAndTravelTimeCollection>
	{
		public DefaultMinimumStayAndTravelTimeCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, DefaultMinimumStayAndTravelTimeCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultMinimumStayAndTravelTimeRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
