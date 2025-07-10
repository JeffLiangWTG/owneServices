using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	public class TagRuleThrottlingRegistryItem : StronglyTypedRegistryItem<TagRuleThrottlingHeader>
	{
		public TagRuleThrottlingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: this(name, category, caption, hint, storage, new TagRuleThrottlingHeader())
		{
		}

		public TagRuleThrottlingRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryBusinessObjectTemplate defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new TagRuleThrottlingRegistryDataType(), storage, defaultValue))
		{
		}
	}
}
