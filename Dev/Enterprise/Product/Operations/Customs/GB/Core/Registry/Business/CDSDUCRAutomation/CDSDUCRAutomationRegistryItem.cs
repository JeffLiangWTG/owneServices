using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	public class CDSDUCRAutomationRegistryItem : StronglyTypedRegistryItem<CDSDUCRAutomationSettings>
	{
		public CDSDUCRAutomationRegistryItem(IRegistryItem inner) : base(inner)
		{
		}

		public CDSDUCRAutomationRegistryItem(string name, MultilingualString category,
			MultilingualString caption, MultilingualString hint, RegistryStorageFlags flags, RegistryOptions options,
			CDSDUCRAutomationSettings defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint,
				new CDSDUCRAutomationRegistryDataType(),
				flags, options, defaultValue))
		{
		}

		public CDSDUCRAutomationRegistryItem(string name, MultilingualString category,
			MultilingualString caption, MultilingualString hint, RegistryStorageFlags flags,
			RegistryOptions options
			)
			: base(new RegistryItemImpl(name, category, caption, hint,
				new CDSDUCRAutomationRegistryDataType(),
				flags,
				options))
		{
		}
	}
}
