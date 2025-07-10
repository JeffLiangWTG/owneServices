using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EntityPrecedenceRuleRegistryItem))]
	sealed class EntityPrecedenceRuleRegistryItemTest : StronglyTypedRegistryItemTestCase<EntityPrecedenceRule>
	{
		protected override StronglyTypedRegistryItem<EntityPrecedenceRule, EntityPrecedenceRule> GetNewRegistryItem()
		{
			return new EntityPrecedenceRuleRegistryItem("", null, null, null, RegistryStorageFlags.System, new EntityPrecedenceRuleItemCollection());
		}

		public void TestTranslatableEntityPrecedenceRuleItemDescription()
		{
			using (var mockChs = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				var registryItem = GetNewRegistryItem();

				var entityPrecedenceRule = registryItem.Value;
				var collection = entityPrecedenceRule.DefaultItems;
				var entityPrecedenceRuleItem = collection.AddNew();
				entityPrecedenceRuleItem.Code = "TST";
				entityPrecedenceRuleItem.EnglishDescription = "Test description";
				registryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, entityPrecedenceRule);

				var key = ((ResourceString)registryItem.Value.DefaultItems[0].Description).ResourceKey;
				mockChs.Put(key, new ResourceStringData(key, "测试"));

				AssertNotEquals("Test description", registryItem.Value.DefaultItems[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
				AssertEquals("测试", registryItem.Value.DefaultItems[0].Description.ToString(Core.SharedConstants.Languages.ChineseSimplified));
			}
		}
	}
}
