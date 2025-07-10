using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class PeriodClosureConfigurationRegistryItem : StronglyTypedRegistryItem<PeriodClosureConfiguration>
	{
		public PeriodClosureConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, PeriodClosureConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new PeriodClosureConfigurationRegistryDataType(), storage, options, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.PeriodClosureConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class PeriodClosureConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<PeriodClosureConfiguration>
	{
		public PeriodClosureConfigurationRegistryDataType()
		{
		}
	}
}
