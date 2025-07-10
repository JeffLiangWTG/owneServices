using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.DocumentWrappers
{
	public static class AccountingHelperClass
	{
		#region Tax Description in Document for Tax Amounts

		internal static ZString DescriptionInDocumentsForTaxAmountsRule
		{
			get { return AccountingConfigurationRegistry.Instance.DescriptionInDocumentsForTaxAmountsRule.Value; }
		}

		internal static ZString ZeroAmountTaxTypesDescriptionValue(string taxType)
		{
			ZString result = ZString.Empty;
			var collection = AccountingConfigurationRegistry.Instance.ZeroAmountTaxTypesDescription.Value;
			var taxTypesDescriptions = collection.Cast<ZeroAmountTaxTypesDescriptions>().FirstOrDefault(x => x.TaxType == taxType);
			if (taxTypesDescriptions != null)
			{
				result = taxTypesDescriptions.OverrideValue == null || taxTypesDescriptions.OverrideValue.IsEmpty ? taxTypesDescriptions.DefaultValue : taxTypesDescriptions.OverrideValue;
			}
			return result;
		}

		#endregion
	}
}
