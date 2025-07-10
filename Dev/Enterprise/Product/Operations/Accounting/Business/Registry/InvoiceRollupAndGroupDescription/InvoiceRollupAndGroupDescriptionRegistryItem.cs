using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoiceRollupAndGroupDescriptionRegistryItem : TranslatableRegistryItem<InvoiceRollupAndGroupDescriptionCollection, InvoiceRollupAndGroupDescriptionCollection>
	{
		public InvoiceRollupAndGroupDescriptionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, InvoiceRollupAndGroupDescriptionCollection defaultValue)
			: base((new RegistryItemImpl(name, category, caption, hint, new InvoiceRollupAndGroupDescriptionRegistryDataType(), storage, defaultValue)))
		{
			DefaultCollection = defaultValue;
		}

		#region Translation support

		public override bool IsTranslatable => true;
		public override int MaxLength => InvoiceRollupAndGroupDescription.Schema.DescriptionMaxLength;

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (InvoiceRollupAndGroupDescription item in DefaultCollection)
				{
					if (!item.Description.IsEmpty)
					{
						yield return (ResourceString)item.Description;
					}
				}
			}
		}

		public override IEnumerable<string> GetCaptions(InvoiceRollupAndGroupDescriptionCollection value)
		{
			var descriptionValue = value.Cast<InvoiceRollupAndGroupDescription>()
						.Select(i => i.Description.ToString().Trim()).Distinct()
						.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			return descriptionValue;
		}

		protected override InvoiceRollupAndGroupDescriptionCollection Convert(InvoiceRollupAndGroupDescriptionCollection value)
		{
			foreach (InvoiceRollupAndGroupDescription item in value)
			{
				item.Description = GetMultilingualString(item.EnglishDescription);
			}
			return value;
		}

		#endregion

		public MultilingualString GetDescription(ZString descriptionStyle, ZString descriptionGroup)
		{
			return Value.Cast<InvoiceRollupAndGroupDescription>().FirstOrDefault(x => x.Style == descriptionStyle && x.Group == descriptionGroup)?.Description;
		}

		readonly InvoiceRollupAndGroupDescriptionCollection DefaultCollection;
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.InvoiceRollupAndGroupDescriptionRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class InvoiceRollupAndGroupDescriptionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InvoiceRollupAndGroupDescriptionCollection>
	{
	}
}