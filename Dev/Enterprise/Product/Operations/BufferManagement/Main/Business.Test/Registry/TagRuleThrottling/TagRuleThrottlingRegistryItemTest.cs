using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TagRuleThrottlingRegistryItem))]
	class TagRuleThrottlingRegistryItemTest : StronglyTypedRegistryItemTestCase<TagRuleThrottlingHeader>
	{
		protected override StronglyTypedRegistryItem<TagRuleThrottlingHeader, TagRuleThrottlingHeader> GetNewRegistryItem()
		{
			return new TagRuleThrottlingRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}
	}
}
