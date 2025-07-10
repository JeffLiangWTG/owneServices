using System.Collections.Generic;
using System.Linq;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class ZeroAmountTaxTypesDescriptionsRegistryItem : TranslatableRegistryItem<ZeroAmountTaxTypesDescriptionsCollection, ZeroAmountTaxTypesDescriptionsCollection>
	{
		public ZeroAmountTaxTypesDescriptionsRegistryItem(
				string name,
				MultilingualString category,
				MultilingualString caption,
				MultilingualString hint,
				RegistryStorageFlags storage,
				ZeroAmountTaxTypesDescriptionsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new ZeroAmountTaxTypesDescriptionsDataType(), storage, defaultValue))
		{
		}

		#region Convert

		protected override ZeroAmountTaxTypesDescriptionsCollection Convert(ZeroAmountTaxTypesDescriptionsCollection value)
		{
			foreach (ZeroAmountTaxTypesDescriptions item in value)
			{
				item.DefaultValue = GetMultilingualString(item.EnglishDefaultValue);
				item.OverrideValue = GetMultilingualString(item.EnglishOverrideValue);
			}
			return value;
		}

		#endregion

		#region TranslatableRegistryItem Members

		public override IEnumerable<ResourceString> DefaultStrings
		{
			get
			{
				return AccountingConstants.ZeroAmountTaxTypesDescriptionList.Cast<IMultilingualDescription>().Select(item => (ResourceString)item.MultilingualDescription);
			}
		}

		public override IEnumerable<string> GetCaptions(ZeroAmountTaxTypesDescriptionsCollection value)
		{
			var defaultValue = value.Cast<ZeroAmountTaxTypesDescriptions>()
						.Select(i => i.DefaultValue.ToString().Trim()).Distinct()
						.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			var overrideValue = value.Cast<ZeroAmountTaxTypesDescriptions>()
									 .Select(i => i.OverrideValue.ToString().Trim()).Distinct()
									 .Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

			return defaultValue.Concat(overrideValue);
		}

		public override bool IsTranslatable
		{
			get { return true; }
		}

		public override int MaxLength
		{
			get { return ZeroAmountTaxTypesDescriptions.Schema.DescriptionMaxLength; }
		}

		#endregion
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.ZeroAmountTaxTypesDescriptionsRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class ZeroAmountTaxTypesDescriptionsDataType : NonPersistentBusinessObjectRegistryDataType<ZeroAmountTaxTypesDescriptionsCollection>
	{
	}
}
