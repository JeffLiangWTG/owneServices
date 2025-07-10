using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class IncoTermChargeCodesRegistryItem : TranslatableRegistryItem<IncoTermChargeCodesCollection, IncoTermChargeCodesCollection>
	{
		public IncoTermChargeCodesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, IncoTermChargeCodesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new IncoTermChargeCodesRegistryDataType(), storage, defaultValue))
		{
			this.defaultValue = defaultValue;
		}

		public IncoTermChargeCodesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, IncoTermChargeCodesCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new IncoTermChargeCodesRegistryDataType(), storage, options, defaultValue))
		{
			this.defaultValue = defaultValue;
		}

		public override bool IsTranslatable => true;

		protected override IncoTermChargeCodesCollection Convert(IncoTermChargeCodesCollection value)
		{
			foreach (IncoTermChargeCodes item in value)
			{
				if (item.IncotermsWithEditableDescriptionsList.Contains(item.IncoTerm))
				{
					item.IncoTermDescriptionMultilingual = GetMultilingualString(item.EnglishIncoTermDescription);
				}
			}
			return value;
		}

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				foreach (IncoTermChargeCodes item in defaultValue)
				{
					if (item.IncotermsWithEditableDescriptionsList.Contains(item.IncoTerm))
					{
						yield return (ResourceString)item.IncoTermDescriptionMultilingual;
					}
				}
			}
		}

		readonly IncoTermChargeCodesCollection defaultValue;

		public override int MaxLength => 256;

		public override IEnumerable<string> GetCaptions(IncoTermChargeCodesCollection value)
		{
			foreach (IncoTermChargeCodes item in value)
			{
				if (item.IncotermsWithEditableDescriptionsList.Contains(item.IncoTerm))
				{
					yield return item.EnglishIncoTermDescription;
				}
			}
		}
	}

	[RegistryEditor("Enterprise.Registry.GUI.IncotermsRegistryItemEditor, Enterprise.Registry.GUI")]
	class IncoTermChargeCodesRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IncoTermChargeCodesCollection>
	{
		public IncoTermChargeCodesRegistryDataType()
		{
		}
	}
}
