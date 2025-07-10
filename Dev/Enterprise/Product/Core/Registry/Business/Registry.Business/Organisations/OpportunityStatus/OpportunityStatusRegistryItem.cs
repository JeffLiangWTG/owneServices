using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class OpportunityStatusRegistryItem : TranslatableRegistryBusinessItemCollectionRegistryItem<OpportunityStatusCollection, OpportunityStatusCollection>
	{
		public OpportunityStatusRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, OpportunityStatusCollection defaultValue)
			: this(name, category, caption, hint, storage, RegistryOptions.Default, defaultValue)
		{
		}

		public OpportunityStatusRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, OpportunityStatusCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new OpportunityStatusRegistryDataType(defaultValue), storage, options, defaultValue))
		{
		}

		public override int MaxLength
		{
			get { return 256; }
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.OpportunityStatusRegistryItemEditor, Enterprise.Registry.GUI")]
	public class OpportunityStatusRegistryDataType : NonPersistentBusinessObjectRegistryDataType<OpportunityStatusCollection>
	{
		public OpportunityStatusRegistryDataType(OpportunityStatusCollection defaultValue)
			: base(defaultValue)
		{
		}
	}
}
