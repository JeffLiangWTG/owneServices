using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs.US
{
	public class ExportEntryFilerIDRegistryItem : StronglyTypedRegistryItem<ExportEntryFilerID>
	{
		public ExportEntryFilerIDRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExportEntryFilerIDRegistryDataType(), RegistryStorageFlags.Company | RegistryStorageFlags.Branch, RegistryOptions.Default))
		{
		}
	}
}
