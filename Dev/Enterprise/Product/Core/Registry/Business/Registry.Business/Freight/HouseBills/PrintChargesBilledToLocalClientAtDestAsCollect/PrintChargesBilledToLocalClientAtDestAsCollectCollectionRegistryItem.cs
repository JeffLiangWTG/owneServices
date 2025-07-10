using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public sealed class PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItem : StronglyTypedRegistryItem<PrintChargesBilledToLocalClientAtDestAsCollectCollection>
	{
		public PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions option)
			: base(new PrintChargesBilledToLocalClientAtDestAsCollectRegistryImpl(name, category, caption, hint, new PrintChargesBilledToLocalClientAtDestAsCollectCollectionDataType(), storage, option))
		{
		}

		public PrintChargesBilledToLocalClientAtDestAsCollectCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new PrintChargesBilledToLocalClientAtDestAsCollectRegistryImpl(name, category, caption, hint, new PrintChargesBilledToLocalClientAtDestAsCollectCollectionDataType(), storage))
		{
		}
	}
}
