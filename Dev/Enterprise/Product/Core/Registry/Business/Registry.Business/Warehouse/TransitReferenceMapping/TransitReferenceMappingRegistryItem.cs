using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class TransitReferenceMappingRegistryItem : StronglyTypedRegistryItem<TransitReferenceMappingConfiguration>
	{
		public TransitReferenceMappingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new TransitReferenceMappingRegistryDataType(), storage))
		{
		}
	}
}
