using System;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;
using static Enterprise.Accounting.Business.AccountingConstants;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(InvoiceTotalRounding))]
	public class InvoiceTotalRoundingTest : RegistryBusinessObjectTemplateTestCase<InvoiceTotalRounding>
	{
		public void TestValidateCurrency()
		{
			var collection = new InvoiceTotalRoundingCollection();
			var config1 = collection.AddNew();
			config1.Currency = string.Empty;
			config1.ValidateCurrency_ForTestOnly();
			AssertHasError("Must have Currency code", config1.CurrencyInfo, "Please enter a Currency Code.");

			config1.Currency = "AAA";
			config1.ValidateCurrency_ForTestOnly();
			AssertHasError("Currency code must be valid", config1.CurrencyInfo, "Enter a valid selection.");

			config1.Currency = Core.Constants.CurrencyCodes.China;
			AssertNoErrors("Valid Currency code", config1.CurrencyInfo);

			var config2 = collection.AddNew();
			config2.Currency = Core.Constants.CurrencyCodes.China;
			config1.ValidateCurrency_ForTestOnly();
			config2.ValidateCurrency_ForTestOnly();

			AssertHasError("Currencies can't be identical", config1.CurrencyInfo, "Same Currency not allowed more than once, please select another currency.");
			AssertHasError("Currencies can't be identical", config2.CurrencyInfo, "Same Currency not allowed more than once, please select another currency.");

			config2.Currency = Core.Constants.CurrencyCodes.Australia;
			config1.ValidateCurrency_ForTestOnly();
			config2.ValidateCurrency_ForTestOnly();

			AssertNoErrors("Different currency codes", config1.CurrencyInfo);
			AssertNoErrors("Different currency codes", config2.CurrencyInfo);

			config1.Currency = Core.Constants.CurrencyCodes.Afghanistan;
			config1.ValidateCurrency_ForTestOnly();

			AssertHasError("Only currencies with 100 minor units can be selected", config1.CurrencyInfo, "Only currencies with 100 minor units can be selected, please select another currency.");
		}

		public void TestValidateRoundingOption()
		{
			var collection = new InvoiceTotalRoundingCollection();
			var config = collection.AddNew();

			config.RoundingOption = string.Empty;
			config.ValidateRoundingOption_ForTestOnly();
			AssertHasError("Must have Rounding option", config.RoundingOptionInfo, "Please enter a Rounding Option.");

			config.RoundingOption = "AAA";
			config.ValidateRoundingOption_ForTestOnly();
			AssertHasError("Rounding option must be valid", config.RoundingOptionInfo, "Enter a valid selection.");

			config.RoundingOption = AccountingConstants.RoundingOptionsCodes.RoundBasedOnMidpoint;
			config.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.FiftyMinorUnits;
			config.ValidateRoundingOption_ForTestOnly();
			AssertHasError("Rounding option and round to current unit not match when option is 'RBM'", config.RoundingOptionInfo, "The round based on midpoint option can only be used with round to currency unit set to 1.00.");

			config.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.OneMajorUnit;
			config.ValidateRoundingOption_ForTestOnly();
			AssertNoErrors("Valid Rounding option", config.RoundingOptionInfo);

			config.RoundingOption = AccountingConstants.RoundingOptionsCodes.AlwaysRoundDown;
			AssertNoErrors("Valid Rounding option", config.RoundingOptionInfo);
		}

		public void TestRoundToCurrencyUnit()
		{
			var collection = new InvoiceTotalRoundingCollection();
			var config = collection.AddNew();

			config.RoundToCurrencyUnit = string.Empty;
			config.ValidateRoundingOption_ForTestOnly();
			AssertHasError("Must have Round to currency unit", config.RoundToCurrencyUnitInfo, "Please enter a Round To Currency Unit.");

			config.RoundToCurrencyUnit = "AAA";
			config.ValidateRoundingOption_ForTestOnly();
			AssertHasError("Round to currency unit must be valid", config.RoundToCurrencyUnitInfo, "Enter a valid selection.");

			config.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.OneMajorUnit;
			AssertNoErrors("Valid Round to currency unit", config.RoundToCurrencyUnitInfo);
		}

		public void TestConvertRoundingOption()
		{
			var collection = new InvoiceTotalRoundingCollection();
			var invoiceTotalRounding = collection.AddNew();

			foreach (var roundingOption in AccountingConstants.RoundingOptionsList.GetAllCodesZString())
			{
				try
				{
					var roundingOptionEnum = invoiceTotalRounding.ConvertRoundingOption_ForTestOnly(roundingOption);
					AssertType<InvoiceRoundingOption>(roundingOptionEnum);
				}
				catch (Exception)
				{
					Assert("No convert exception should happen", false);
				}
			}
		}

		public void TestConvertInvoiceRoundCurrencyUnit()
		{
			var collection = new InvoiceTotalRoundingCollection();
			var invoiceTotalRounding = collection.AddNew();

			foreach (var roundToCurrencyUnit in AccountingConstants.RoundToCurrencyUnitsList.GetAllCodesZString())
			{
				try
				{
					var roundCurrencyUnitEnum = invoiceTotalRounding.ConvertInvoiceRoundCurrencyUnit_ForTestOnly(roundToCurrencyUnit);
					AssertType<InvoiceRoundCurrencyUnit>(roundCurrencyUnitEnum);
				}
				catch (Exception)
				{
					Assert("No convert exception should happen", false);
				}
			}
		}

		public void TestRoundingOptionEnumExistsInList()
		{
			var invoiceRoundingOptionEnums = Enum.GetNames(typeof(InvoiceRoundingOption));
			foreach (var roundingOption in invoiceRoundingOptionEnums)
			{
				Assert($"Enum {roundingOption} should exist in RoundingOptionsList.", RoundingOptionsList.ContainsCode(roundingOption));
			}
		}

		public void TestInvoiceRoundCurrencyUnitEnumExistsInList()
		{
			var invoiceRoundCurrencyUnitEnums = Enum.GetNames(typeof(InvoiceRoundCurrencyUnit));
			AssertEquals("RoundToCurrencyUnits should have same count between List and Enum.", AccountingConstants.RoundToCurrencyUnitsList.Count, invoiceRoundCurrencyUnitEnums.Length);
		}

		#region Implementation

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => false;

		protected override InvoiceTotalRounding GetBusinessObjectToClone()
		{
			var rule = new InvoiceTotalRounding();
			rule.Currency = Core.Constants.CurrencyCodes.Australia;
			rule.RoundingOption = AccountingConstants.RoundingOptionsCodes.AlwaysRoundDown;
			rule.RoundToCurrencyUnit = AccountingConstants.RoundToCurrencyUnits.OneMajorUnit;
			return rule;
		}

		protected override InvoiceTotalRounding GetBusinessObjectToSerialise() => GetBusinessObjectToClone();

		#endregion
	}
}
