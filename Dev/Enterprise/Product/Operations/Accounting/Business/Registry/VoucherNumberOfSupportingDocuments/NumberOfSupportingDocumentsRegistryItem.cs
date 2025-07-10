using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class DefaultNumberOfSupportingDocumentsRegistryItem : TranslatableRegistryItem<DefaultNumberOfSupportingDocumentsCollection, DefaultNumberOfSupportingDocumentsCollection>
	{
		public DefaultNumberOfSupportingDocumentsRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DefaultNumberOfSupportingDocumentsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DefaultNumberOfSupportingDocumentsRegistryDataType(), storage, options, defaultValue))
		{
			this.DefaultCollection = defaultValue;
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override int MaxLength
		{
			get { return DefaultNumberOfSupportingDocuments.Schema.DescriptionMaxLength; }
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (DefaultNumberOfSupportingDocuments item in DefaultCollection)
				{
					if (!item.Description.IsEmpty)
					{
						yield return (ResourceString)item.Description;
					}
				}
			}
		}

		public override IEnumerable<string> GetCaptions(DefaultNumberOfSupportingDocumentsCollection value)
		{
			var descriptionValue = value.Cast<DefaultNumberOfSupportingDocuments>()
						.Select(i => i.Description.ToString().Trim()).Distinct()
						.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			return descriptionValue;
		}

		protected override DefaultNumberOfSupportingDocumentsCollection Convert(DefaultNumberOfSupportingDocumentsCollection value)
		{
			foreach (DefaultNumberOfSupportingDocuments item in value)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
			}
			return value;
		}

		readonly DefaultNumberOfSupportingDocumentsCollection DefaultCollection;
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.DefaultNumberOfSupportingDocumentsRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class DefaultNumberOfSupportingDocumentsRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultNumberOfSupportingDocumentsCollection>
	{
		public DefaultNumberOfSupportingDocumentsRegistryDataType()
		{
		}
	}
}