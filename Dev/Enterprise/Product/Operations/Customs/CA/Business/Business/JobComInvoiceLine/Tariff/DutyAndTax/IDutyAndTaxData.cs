using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public interface IDutyAndTaxData : IFactoryProvider
	{
		JobDeclaration Declaration { get; }
		ZBool DDPDeductDutyOnly { get; }
		ZString CalculationMethod { get; }
		ZDecimal ExchangeRate { get; }
		ZDateTime EffectiveDutyDate { get; }
		ZString ClassificationNumber { get; }
		ZString TariffCode { get; }
		ZString TreatmentCode { get; }
		ZDecimal FOBValue { get; }
		ZString AdjustmentCode { get; }
		ZDecimal AdjustmentValue { get; }
		ZDecimal ValueForCurrencyConversion { get; }
		ZDecimal CustomsValue { get; set; }
		ZBool CalculateCustomsValueWithoutDutyIfDutyPaid { get; }
		ZDecimal NormalDutyPaidValue { get; }
		ZDecimal NormalValueForTax { get; }
		ZInt MonthlyTimeLimit { get; }
		ZBool IsWarehouseOrSupplementaryEntry { get; }
		ZBool IsB3ValidationRequired { get; }
		ZBool IsSIMADutyRequired { get; }

		ZDecimal CustomsQuantity { get; }
		ZString CustomsUnits { get; }
		ZDecimal CustomsQuantity2 { get; }
		ZString CustomsUnits2 { get; set; }
		ZDecimal CustomsQuantity3 { get; }
		ZString CustomsUnits3 { get; set; }
		ZDecimal ValueForTax { get; set; }

		ZString DefaultGSTStatusCode { get; }
		ZString DefaultETRateCode { get; }
		ZString DefaultETExemptionCode { get; }
		ZString AuthorityNumber { get; }
		ZString PreviousTransactionNumber { get; }
		ZInt PreviousLineNumber { get; }
		ZString CountryOfOrigin { get; }
		ZString CountryOfExport { get; }
		ZString DumpingCaseNumber { get; }
		CurrencyConverter CurrencyConverter { get; }
		DutyAndTaxUnitConverter UnitConverter { get; }

		bool IsSettingSIMAExemptCodeInProgress { get; set; }
		ZDecimal ExchangeRateForSIMA { get; }
		IEnumerable<DutyAndTax> SIMADuties { get; }
		IEnumerable<CACusRulingConfig> RulingConfigs { get; }

		event EventHandler QuantityChanged;

		DutyAndTaxManager DutyAndTaxManager { get; }
	}

	public interface IDutyAndTaxDataForCalculation
	{
		IDutyAndTaxData Parent { get; }
		ZString TaxType { get; }
		ZString ExemptCode { get; }
		ZString RateType { get; }
		ZDecimal Rate { get; }
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }
		ZDecimal ValueForCalculation { get; set; }
		ZDecimal NormalValuePerUnit { get; }
		ZString NormalValueCurrency { get; }
		ZDecimal Amount { get; set; }
		ZString AmountDescription { get; }
		ZBool IsInRefFiles { get; }
		ZBool Override { get; }
		ZString DutyType { get; }
	}
}
