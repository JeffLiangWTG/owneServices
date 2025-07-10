using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class ContactSalutationRegistryItem : TranslatableRegistryItem<ContactSalutationCollection, ContactSalutationCollection>
	{
		public ContactSalutationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, ContactSalutationCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ContactSalutationTypesDataType(), storage, RegistryOptions.Default, defaultValue))
		{
			this.defaultValue = defaultValue;
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		protected override ContactSalutationCollection Convert(ContactSalutationCollection value)
		{
			foreach (ContactSalutation item in value)
			{
				item.Salutation = GetMultilingualString(item.AnnotatedEnglishSalutation);
			}
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (ContactSalutation item in defaultValue)
				{
					yield return (ResourceString)item.RawSalutation;
				}
			}
		}

		public override IEnumerable<string> GetCaptions(ContactSalutationCollection value)
		{
			foreach (ContactSalutation item in value)
			{
				yield return item.AnnotatedEnglishSalutation;
			}
		}

		protected override IEnumerable<MultilingualString> GetMultilingualCaptions(ContactSalutationCollection value)
		{
			foreach (ContactSalutation item in value)
			{
				yield return item.RawSalutation;
			}
		}

		public override int MaxLength
		{
			get { return ContactSalutation.Schema.SalutationMaxLength; }
		}

		readonly ContactSalutationCollection defaultValue;
	}

	[RegistryEditor("Enterprise.Registry.GUI.ContactSalutationRegistryItemEditor, Enterprise.Registry.GUI")]
	public class ContactSalutationTypesDataType : NonPersistentBusinessObjectRegistryDataType<ContactSalutationCollection>
	{
	}
}
