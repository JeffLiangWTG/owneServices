using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class PutawaySequenceRegistryItem : StronglyTypedRegistryItem<PutawaySequence, IPutawaySequence>
	{
		public PutawaySequenceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PutawaySequenceRegistryDataType(), storage))
		{
		}

		public PutawaySequenceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new PutawaySequenceRegistryDataType(), storage, options))
		{
		}
	}
}
