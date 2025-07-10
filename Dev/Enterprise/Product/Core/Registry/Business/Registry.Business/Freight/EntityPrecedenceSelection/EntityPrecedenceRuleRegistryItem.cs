using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class EntityPrecedenceRuleRegistryItem : TranslatableRegistryItem<EntityPrecedenceRule, EntityPrecedenceRule>
	{
		public EntityPrecedenceRuleRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, EntityPrecedenceRuleItemCollection defaultValues)
			: base(new RegistryItemImpl(name, category, caption, hint, new EntityPrecedenceRuleRegistryDataType(), storage, GetDefaultEntityPrecedenceRule(defaultValues)))
		{
			ruleItemCollction = defaultValues;
		}

		public EntityPrecedenceRuleRegistryItem(string name, MultilingualString[] categories, MultilingualString caption, MultilingualString hint, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, EntityPrecedenceRuleItemCollection defaultValues)
			: base(new RegistryItemImpl(name, caption, hint, new EntityPrecedenceRuleRegistryDataType(), editorInfo, storage, options, GetDefaultEntityPrecedenceRule(defaultValues), false, categories))
		{
			ruleItemCollction = defaultValues;
		}

		readonly EntityPrecedenceRuleItemCollection ruleItemCollction;

		static EntityPrecedenceRule GetDefaultEntityPrecedenceRule(EntityPrecedenceRuleItemCollection defaultValues)
		{
			var entityPrecedence = new EntityPrecedenceRule();
			entityPrecedence.DefaultItems.AddRange(defaultValues);
			entityPrecedence.ResetItems();

			return entityPrecedence;
		}

		protected override EntityPrecedenceRule Convert(EntityPrecedenceRule value)
		{
			ConvertEntityPrecedenceRuleItemCollectionDescription(value.DefaultItems);
			ConvertEntityPrecedenceRuleItemCollectionDescription(value.SelectedItems);
			ConvertEntityPrecedenceRuleItemCollectionDescription(value.AvailableItems);
			return value;
		}

		void ConvertEntityPrecedenceRuleItemCollectionDescription(EntityPrecedenceRuleItemCollection collection)
		{
			foreach (EntityPrecedenceRuleItem item in collection)
			{
				using (item.SuspendSettingHasChanges())
				{
					item.Description = GetMultilingualString(item.EnglishDescription);
				}
			}
		}

		public override IEnumerable<string> GetCaptions(EntityPrecedenceRule value)
		{
			foreach (EntityPrecedenceRuleItem item in value.DefaultItems)
			{
				yield return item.EnglishDescription;
			}
		}

		public override bool IsTranslatable => true;

		public override int MaxLength => 256;

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (EntityPrecedenceRuleItem item in ruleItemCollction)
				{
					yield return (ResourceString)item.Description;
				}
			}
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.EntityPrecedenceRuleEditor, Enterprise.Registry.GUI")]
	class EntityPrecedenceRuleRegistryDataType : NonPersistentBusinessObjectRegistryDataType<EntityPrecedenceRule>
	{
		public EntityPrecedenceRuleRegistryDataType()
		{
		}
	}
}
