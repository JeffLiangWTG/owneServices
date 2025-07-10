using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class InvoiceCopiesRegistryItem : TranslatableRegistryItem<InvoiceCopyCollection, InvoiceCopyCollection>
	{
		public InvoiceCopiesRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				InvoiceCopyCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new InvoiceCopiesRegistryDataType(), storage, defaultValue))
		{
		}

		#region Convert

		protected override InvoiceCopyCollection Convert(InvoiceCopyCollection value)
		{
			foreach (InvoiceCopy item in value)
			{
				item.Name = GetMultilingualString(item.EnglishName);
			}

			UpdateMissingInvoiceCopyOrder(value);

			return value;
		}

		void UpdateMissingInvoiceCopyOrder(InvoiceCopyCollection value)
		{
			var invoiceCopies = value.Cast<InvoiceCopy>();
			if (invoiceCopies.Any(x => x.Order == 0))
			{
				int counter = 2;
				foreach (var invoiceCopy in invoiceCopies)
				{
					if (invoiceCopy.IsOriginal)
					{
						invoiceCopy.Order = 1;
					}
					else
					{
						invoiceCopy.Order = counter++;
					}
				}
			}
		}

		#endregion

		#region TranslatableRegistryItem Members

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				return System.Array.Empty<ResourceString>();
			}
		}

		public override IEnumerable<string> GetCaptions(InvoiceCopyCollection value)
		{
			var nameValue = value.Cast<InvoiceCopy>()
						.Select(i => i.Name.ToString().Trim()).Distinct()
						.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			return nameValue;
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override int MaxLength
		{
			get { return InvoiceCopy.Schema.NameMaxLength; }
		}
		#endregion

	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.InvoiceCopiesRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class InvoiceCopiesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<InvoiceCopyCollection>
	{
		public InvoiceCopiesRegistryDataType()
		{
		}
	}
}
