using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SupportEdwDataSourceReportRegistryItem : StronglyTypedRegistryItem<SupportEdwDataSourceReportCollection>
	{
		public SupportEdwDataSourceReportRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, SupportEdwDataSourceReportCollection defaultValue)
		: base(new RegistryItemImpl(name, category, caption, hint, new SupportEdwDataSourceReportRegistryDataType(), storage, options, defaultValue))
		{
		}
	}
}
