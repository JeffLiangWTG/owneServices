namespace Enterprise.Customs.CA.Business
{
	using System;
	using System.Collections.Generic;
	using CargoWise.Types;
	using Enterprise.Customs.Common.CA;
	using Enterprise.MasterFiles.Business;

	partial class CusClassification : IDutyAndTaxData
	{
		#region Implementation of IDutyAndTaxData

		JobDeclaration IDutyAndTaxData.Declaration
		{
			get { return null; }
		}
		ZBool IDutyAndTaxData.DDPDeductDutyOnly
		{
			get { return false; }
		}
		ZString IDutyAndTaxData.CalculationMethod
		{
			get { return CalculationMethods.Codes.RateDescCalcOnly; }
		}

		ZDecimal IDutyAndTaxData.ExchangeRate
		{
			get { return 1m; }
		}

		ZDateTime IDutyAndTaxData.EffectiveDutyDate
		{
			get { return ZDateTime.Now; }
		}

		ZString IDutyAndTaxData.ClassificationNumber
		{
			get { return CC_TariffNum; }
		}

		ZString IDutyAndTaxData.TariffCode
		{
			get { return CCA_99TariffCode; }
		}

		ZString IDutyAndTaxData.TreatmentCode
		{
			get
			{
				return CCA_TreatmentCode.IsEmpty ? new ZString(TariffTreatmentCodes.Codes.MostFavouredNation) : CCA_TreatmentCode;
			}
		}

		ZDecimal IDutyAndTaxData.FOBValue
		{
			get { return 1m; }
		}

		ZString IDutyAndTaxData.AdjustmentCode
		{
			get { return ZString.Empty; }
		}

		ZDecimal IDutyAndTaxData.AdjustmentValue
		{
			get { return ZDecimal.Zero; }
		}

		ZDecimal IDutyAndTaxData.ValueForCurrencyConversion
		{
			get { return 1m; }
		}

		ZDecimal IDutyAndTaxData.CustomsValue
		{
			get { return customsValue; }
			set { customsValue = value; }
		}
		ZDecimal customsValue;

		ZBool IDutyAndTaxData.CalculateCustomsValueWithoutDutyIfDutyPaid
		{
			get { return false; }
		}

		ZDecimal IDutyAndTaxData.NormalDutyPaidValue
		{
			get { return 1m; }
		}

		ZDecimal IDutyAndTaxData.NormalValueForTax
		{
			get { return 1m; }
		}

		ZInt IDutyAndTaxData.MonthlyTimeLimit
		{
			get { return ZInt.Zero; }
		}

		ZBool IDutyAndTaxData.IsWarehouseOrSupplementaryEntry
		{
			get { return false; }
		}

		ZBool IDutyAndTaxData.IsB3ValidationRequired
		{
			get { return false; }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity
		{
			get { return 1m; }
		}

		ZString IDutyAndTaxData.CustomsUnits
		{
			get { return ZString.Empty; }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity2
		{
			get { return 1m; }
		}

		ZString IDutyAndTaxData.CustomsUnits2
		{
			get { return ZString.Empty; }
			set { }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity3
		{
			get { return 1m; }
		}

		ZString IDutyAndTaxData.CustomsUnits3
		{
			get { return ZString.Empty; }
			set { }
		}

		ZString IDutyAndTaxData.DefaultGSTStatusCode
		{
			get { return ZString.Empty; }
		}

		ZString IDutyAndTaxData.DefaultETRateCode
		{
			get { return ZString.Empty; }
		}

		ZString IDutyAndTaxData.DefaultETExemptionCode
		{
			get { return ZString.Empty; }
		}

		ZString IDutyAndTaxData.AuthorityNumber
		{
			get { return CCA_AuthorityNumber; }
		}

		ZString IDutyAndTaxData.PreviousTransactionNumber
		{
			get { return ZString.Empty; }
		}

		ZInt IDutyAndTaxData.PreviousLineNumber
		{
			get { return ZInt.Zero; }
		}

		ZString IDutyAndTaxData.CountryOfOrigin
		{
			get { return ZString.Empty; }
		}

		ZString IDutyAndTaxData.CountryOfExport
		{
			get { return ZString.Empty; }
		}

		ZString IDutyAndTaxData.DumpingCaseNumber
		{
			get { return CCA_SIMADumpingNumber; }
		}

		CurrencyConverter IDutyAndTaxData.CurrencyConverter
		{
			get { return null; }
		}

		DutyAndTaxUnitConverter IDutyAndTaxData.UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new DutyAndTaxUnitConverter()); }
		}
		DutyAndTaxUnitConverter fUnitConverter;

		bool IDutyAndTaxData.IsSettingSIMAExemptCodeInProgress { get; set; }

		ZDecimal IDutyAndTaxData.ExchangeRateForSIMA => 1m;

		IEnumerable<DutyAndTax> IDutyAndTaxData.SIMADuties
		{
			get { return DutiesAndTaxes; }
		}

		IEnumerable<CACusRulingConfig> IDutyAndTaxData.RulingConfigs => Array.Empty<CACusRulingConfig>();

		ZDecimal IDutyAndTaxData.ValueForTax
		{
			get;
			set;
		}

		event EventHandler IDutyAndTaxData.QuantityChanged
		{
			remove { }
			add { }
		}

		DutyAndTaxManager IDutyAndTaxData.DutyAndTaxManager
		{
			get { return null; }
		}

		#endregion

	}
}
