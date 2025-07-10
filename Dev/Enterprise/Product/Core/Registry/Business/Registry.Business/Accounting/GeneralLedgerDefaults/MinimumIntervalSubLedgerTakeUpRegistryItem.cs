using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class MinimumIntervalSubLedgerTakeUpRegistryItem : StronglyTypedRegistryItem<MinimumIntervalSubLedgerTakeUp>
	{
		public MinimumIntervalSubLedgerTakeUpRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, MinimumIntervalSubLedgerTakeUp defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new MinimumIntervalSubLedgerTakeUpRegistryDataType(), storage, options, defaultValue))
		{
		}

		[RegistryEditor("Enterprise.Registry.GUI.MinimumIntervalSubLedgerTakeUpRegistryItemEditor, Enterprise.Registry.GUI")]
#if DEBUG
		internal
#endif
		class MinimumIntervalSubLedgerTakeUpRegistryDataType : NonPersistentBusinessObjectRegistryDataType<MinimumIntervalSubLedgerTakeUp>
		{
			public MinimumIntervalSubLedgerTakeUpRegistryDataType()
			{
			}
		}
	}
}
