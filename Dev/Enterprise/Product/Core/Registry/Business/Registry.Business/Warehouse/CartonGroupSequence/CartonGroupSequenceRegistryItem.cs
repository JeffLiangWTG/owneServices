using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class CartonGroupSequenceRegistryItem : StronglyTypedRegistryItem<CartonGroupSequence>
	{
		public CartonGroupSequenceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new CartonGroupSequenceRegistryDataType(), storage))
		{
		}
	}
}
