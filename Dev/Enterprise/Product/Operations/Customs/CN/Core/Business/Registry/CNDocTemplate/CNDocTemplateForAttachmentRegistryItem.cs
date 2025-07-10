using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	public class CNDocTemplateForAttachmentRegistryItem : StronglyTypedRegistryItem<CNDocTemplateForAttachmentCollection>
	{
		public CNDocTemplateForAttachmentRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage,
					CNDocTemplateForAttachmentCollection defaultValue)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new CNDocTemplateForAttachmentRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
