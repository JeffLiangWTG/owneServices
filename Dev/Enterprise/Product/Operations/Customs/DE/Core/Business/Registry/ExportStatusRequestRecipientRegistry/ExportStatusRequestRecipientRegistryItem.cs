using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Registry
{
	public class ExportStatusRequestRecipientRegistryItem : StronglyTypedRegistryItem<ExportStatusRequestRecipientRegistryCollection>
	{
		public ExportStatusRequestRecipientRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ExportStatusRequestRecipientRegistryCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ExportStatusRequestRecipientDataType(), storage, RegistryOptions.Default, defaultValue))
		{
		}
	}
}
