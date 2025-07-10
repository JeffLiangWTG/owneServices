using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ExchangeRateToleranceRegistryItem : StronglyTypedRegistryItem<ExchangeRateToleranceConfiguration>
	{
		public ExchangeRateToleranceRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ExchangeRateToleranceConfiguration defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExchangeRateToleranceDataType(), storage, defaultValue))
		{
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ExchangeRateToleranceRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class ExchangeRateToleranceDataType : NonPersistentBusinessObjectRegistryDataType<ExchangeRateToleranceConfiguration>
	{
		public ExchangeRateToleranceDataType()
		{
		}
	}
}
