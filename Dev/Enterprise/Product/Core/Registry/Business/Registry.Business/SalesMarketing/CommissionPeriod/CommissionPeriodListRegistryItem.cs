using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class CommissionPeriodListRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<CommissionPeriodCollection, CommissionPeriodCollection>
	{
		public CommissionPeriodListRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				CommissionPeriodListRegistryEditorInfo editorInfo,
				CommissionPeriodCollection defaultValue
			)
			: base(new RegistryItemImpl(name, category, caption, hint, new CommissionPeriodListRegistryDataType(), storage, defaultValue), editorInfo)
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	public class CommissionPeriodListRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CommissionPeriodCollection>
	{
		public CommissionPeriodListRegistryDataType()
		{
		}
	}
}
