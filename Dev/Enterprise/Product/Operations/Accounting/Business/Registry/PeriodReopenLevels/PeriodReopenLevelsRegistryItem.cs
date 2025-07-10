using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class PeriodReopenLevelsRegistryItem : StronglyTypedRegistryItem<PeriodReopenLevelsCollection>
	{
		public PeriodReopenLevelsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions regOptions)
			: base(new RegistryItemImpl(name, category, caption, hint, new PeriodReopenLevelsRegistryDataType(), storage, regOptions))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.PeriodReopenLevelsRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class PeriodReopenLevelsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PeriodReopenLevelsCollection>
	{
		public PeriodReopenLevelsRegistryDataType()
		{
		}
	}
}
