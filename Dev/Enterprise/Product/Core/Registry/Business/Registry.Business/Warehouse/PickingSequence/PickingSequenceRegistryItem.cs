using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Warehouse
{
	public class PickingSequenceRegistryItem : StronglyTypedRegistryItem<PickingSequence, IPickingSequence>
	{
		public PickingSequenceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new RegistryItemImpl(name, category, caption, hint, new PickingSequenceRegistryDataType(), storage))
		{
		}

		public PickingSequenceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new PickingSequenceRegistryDataType(), storage, options))
		{
		}
	}
}
