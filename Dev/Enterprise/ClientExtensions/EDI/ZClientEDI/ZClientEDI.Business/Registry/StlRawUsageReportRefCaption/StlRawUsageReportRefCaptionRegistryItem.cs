using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class StlRawUsageReportRefCaptionRegistryItem : StronglyTypedRegistryItem<StlRawUsageReportRefCaptionCollection>
	{
		public StlRawUsageReportRefCaptionRegistryItem(string itemName, MultilingualString category, MultilingualString caption, MultilingualString hint)
			: base(new RegistryItemImpl(
				itemName,
				category,
				caption,
				hint,
				new StlRawUsageReportRefCaptionDataType(),
				RegistryStorageFlags.System,
				RegistryOptions.Default))
		{
		}

		public StlRawUsageReportRefCaptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, StlRawUsageReportRefCaptionCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new StlRawUsageReportRefCaptionDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}

