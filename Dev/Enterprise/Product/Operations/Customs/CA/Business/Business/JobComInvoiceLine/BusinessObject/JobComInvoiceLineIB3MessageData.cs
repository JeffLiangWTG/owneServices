using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	partial class JobComInvoiceLine : IDutyAndTaxData, IClassificationTariffLine
	{
		#region IDutyAndTaxData Members

		JobDeclaration IDutyAndTaxData.Declaration
		{
			get { return Declaration; }
		}

		ZBool IDutyAndTaxData.DDPDeductDutyOnly
		{
			get { return InvoiceHeader?.CA_DDPDeductDutyOnly ?? false; }
		}

		ZString IDutyAndTaxData.CalculationMethod
		{
			get { return (IsRepairLine || IsDutyDeferralLineParent || IsSoftwareRemissionLine) ? (ZString)CalculationMethods.Codes.NoRemission : CA_CalculationMethod; }
		}

		ZDecimal IDutyAndTaxData.ExchangeRate
		{
			get { return InvoiceHeader?.JZ_InvoiceCurrExRate ?? ZDecimal.Zero; }
		}

		ZDateTime IDutyAndTaxData.EffectiveDutyDate
		{
			get
			{
				var declaration = Declaration != null && Declaration.IsLVS && InvoiceHeader != null && InvoiceHeader.JZ_JE != Declaration.PK
					? Factory.Load<JobDeclaration>(InvoiceHeader.JZ_JE) : Declaration;
				return declaration?.EffectiveDutyDate ?? ZDateTime.Today;
			}
		}

		ZString IDutyAndTaxData.ClassificationNumber
		{
			get { return JI_Tariff; }
		}

		ZString IDutyAndTaxData.TariffCode
		{
			get { return CA_99TariffCode; }
		}

		ZString IDutyAndTaxData.TreatmentCode
		{
			get { return EffectiveTreatmentCode; }
		}

		ZDecimal IDutyAndTaxData.FOBValue
		{
			get { return base.GetJI_Calc_FOB(); } //NOTE: Should get BASE value here
		}

		ZString IDutyAndTaxData.AdjustmentCode
		{
			get { return CA_ADJCode; }
		}

		ZDecimal IDutyAndTaxData.AdjustmentValue
		{
			get { return CA_ADJValue; }
		}

		ZDecimal IDutyAndTaxData.ValueForCurrencyConversion
		{
			get { return CA_CVforCurrConv; }
		}

		ZDecimal IDutyAndTaxData.CustomsValue
		{
			get { return CA_CustomsValueOvr ? CA_CustomsValue : DutyAndTaxManager.GetCustomsValueForDuty(); }
			set
			{
				if (!CA_CustomsValueOvr)
				{
					using (GetValidationSuspender())
					{
						CA_CustomsValue = value;
					}
				}
			}
		}

		ZBool IDutyAndTaxData.CalculateCustomsValueWithoutDutyIfDutyPaid
		{
			get { return !CA_CustomsValueOvr; }
		}

		ZDecimal IDutyAndTaxData.NormalDutyPaidValue
		{
			get { return DutyAndTaxManager.NormalDutyPaidValue; }
		}

		ZDecimal IDutyAndTaxData.NormalValueForTax
		{
			get { return DutyAndTaxManager.NormalValueForTax; }
		}

		ZInt IDutyAndTaxData.MonthlyTimeLimit
		{
			get { return InvoiceHeader?.CA_TimeLimit ?? ZInt.Zero; }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity
		{
			get { return JI_CustomsQuantity; }
		}

		ZString IDutyAndTaxData.CustomsUnits
		{
			get { return JI_CustomsUnitQty; }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity2
		{
			get { return JI_CustomsSecondQuantity; }
		}

		ZString IDutyAndTaxData.CustomsUnits2
		{
			get { return JI_CustomsSecondUnitQty; }
			set { JI_CustomsSecondUnitQty = value; }
		}

		ZDecimal IDutyAndTaxData.CustomsQuantity3
		{
			get { return JI_CustomsThirdQuantity; }
		}

		ZString IDutyAndTaxData.CustomsUnits3
		{
			get { return JI_CustomsThirdUnitQty; }
			set { JI_CustomsThirdUnitQty = value; }
		}

		ZString IDutyAndTaxData.DefaultGSTStatusCode
		{
			get
			{
				if (CA_CalculationMethod == CalculationMethods.Codes.WarrantyRepairsRemission)
				{
					return (ZString)GSTStatusCodes.Codes.C66;
				}
				if (Pivot is CusClassPartPivot pivot)
				{
					return !pivot.CCA_GSTStatusCode.IsEmpty ? pivot.CCA_GSTStatusCode : pivot.CI_CC_CA_GSTStatusCode;
				}
				if (Classification is CusClassification classification)
				{
					return classification.CCA_GSTStatusCode;
				}
				return ZString.Empty;
			}
		}

		ZString IDutyAndTaxData.DefaultETRateCode
		{
			get
			{
				if (Pivot is CusClassPartPivot pivot)
				{
					return !pivot.CCA_ETRateCode.IsEmpty ? pivot.CCA_ETRateCode : pivot.CI_CC_CA_ETRateCode;
				}
				if (Classification is CusClassification classification)
				{
					return classification.CCA_ETRateCode;
				}
				return ZString.Empty;
			}
		}

		ZString IDutyAndTaxData.DefaultETExemptionCode
		{
			get
			{
				if (Pivot is CusClassPartPivot pivot)
				{
					return !pivot.CCA_ETExemption.IsEmpty ? pivot.CCA_ETExemption : pivot.CI_CC_CA_ETExemption;
				}
				if (Classification is CusClassification classification)
				{
					return classification.CCA_ETExemption;
				}
				return ZString.Empty;
			}
		}

		ZBool IDutyAndTaxData.IsWarehouseOrSupplementaryEntry
		{
			get { return Declaration?.IsWarehouseOrSupplementaryEntry ?? ZBool.False; }
		}

		ZBool IDutyAndTaxData.IsB3ValidationRequired
		{
			get { return Declaration?.IsB3ValidationRequired ?? ZBool.False; }
		}

		ZString IDutyAndTaxData.AuthorityNumber
		{
			get { return CA_AuthorityNumber; }
		}

		ZString IDutyAndTaxData.PreviousTransactionNumber
		{
			get { return JI_PreviousEntryNumber; }
		}

		ZInt IDutyAndTaxData.PreviousLineNumber
		{
			get { return JI_PreviousEntryLineNumber; }
		}

		ZString IDutyAndTaxData.CountryOfOrigin
		{
			get { return JI_CountryOfOrigin; }
		}

		ZString IDutyAndTaxData.CountryOfExport
		{
			get { return CA_RN_NKExport.IsEmpty && InvoiceHeader != null ? InvoiceHeader.CA_RN_NKExport : CA_RN_NKExport; }
		}

		ZString IDutyAndTaxData.DumpingCaseNumber
		{
			get { return CA_SIMADumpingNum; }
		}

		CurrencyConverter IDutyAndTaxData.CurrencyConverter
		{
			get { return fCurrencyConverter ?? (fCurrencyConverter = new CurrencyConverterWithFixedExchangeRatesDataProvider(Factory, new DutyAndTaxCurrencyConverter(InvoiceHeader))); }
		}
		CurrencyConverter fCurrencyConverter;

		DutyAndTaxUnitConverter IDutyAndTaxData.UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new DutyAndTaxUnitConverter()); }
		}
		DutyAndTaxUnitConverter fUnitConverter;

		bool IDutyAndTaxData.IsSettingSIMAExemptCodeInProgress { get; set; }

		ZDecimal IDutyAndTaxData.ExchangeRateForSIMA
		{
			get
			{
				if (exchangeRateForSIMA == null)
				{
					exchangeRateForSIMA = new CachedProperty<ZDecimal>(Factory, delegate
					{
						var result = ZDecimal.Zero;

						if (((IDutyAndTaxData)this).CurrencyConverter is CurrencyConverter currencyConverter && InvoiceHeader?.Invoice_Currency is RefCurrency currency)
						{
							result = currencyConverter.GetExchangeRate(currency);
						}

						if (result.IsEmpty)
						{
							result = ((IDutyAndTaxData)this).ExchangeRate;
						}
						return result;
					});
				}
				return exchangeRateForSIMA.Value;
			}
		}
		CachedProperty<ZDecimal> exchangeRateForSIMA;

		IEnumerable<DutyAndTax> IDutyAndTaxData.SIMADuties
		{
			get { return DutiesAndTaxes.ToArray().Where(x => DutyAndTaxTypes.IsSIMATaxCode(x.C1_TaxType)); }
		}

		IEnumerable<CACusRulingConfig> IDutyAndTaxData.RulingConfigs => RulingConfigurations.Cast<CACusRulingConfig>();

		ZDecimal IDutyAndTaxData.ValueForTax
		{
			get => CA_ValueForTax;
			set => CA_ValueForTax = value;
		}
		#endregion

		#region IClassificationTariffLine Members

		ZString IClassificationTariffLine.ClassificationNumber
		{
			get { return JI_Tariff; }
		}

		ZDateTime IClassificationTariffLine.ClassificationDate
		{
			get { return ((IDutyAndTaxData)this).EffectiveDutyDate; }
		}

		ZString IClassificationTariffLine.TariffCode
		{
			get { return CA_99TariffCode; }
		}

		ZDateTime IClassificationTariffLine.TariffDate
		{
			get { return ((IDutyAndTaxData)this).EffectiveDutyDate; }
		}

		#endregion

		#region Implementation

		internal bool IsDutiesAndTaxesOverriden
		{
			get
			{
				var overridenDutiesOrTaxes = from DutyAndTax dutyOrTax in DutiesAndTaxes
											 where dutyOrTax.C1_Override && !DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(dutyOrTax.C1_TaxType)
											 select dutyOrTax;
				return CA_CustomsValueOvr || overridenDutiesOrTaxes.Any();
			}
		}

		#region JI_Calc_CIF

		protected override ZDecimal GetJI_Calc_CIF()
		{
			if (IsImport && CA_CalculationMethod == CalculationMethods.Codes.DeliveredDutyPaid)
			{
				var result = JI_Calc_FOB;
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					var invoiceCurrency = invoiceHeader.Invoice_Currency;
					if (invoiceCurrency != null)
					{
						result -= ValuationCalculator.GetAmountToAddToITOTForDutiable(invoiceCurrency);
						result += ValuationCalculator.GetAmountToAddToITOTForVatableGstable(invoiceCurrency);
					}
				}
				return result;
			}

			return base.GetJI_Calc_CIF();
		}

		#endregion

		#region JI_Calc_FOB

		protected override ZDecimal GetJI_Calc_FOB()
		{
			return IsImport && CA_CalculationMethod == CalculationMethods.Codes.DeliveredDutyPaid
							 ? DutyAndTaxManager.GetDeliveredDutyPaidFOBValue() : base.GetJI_Calc_FOB();
		}

		#endregion

		#region CA_CVforCurrConv

		[ReadOnlyMember(nameof(CA_CVforCurrConv_ReadOnly))]
		[ResourceStringData("CAAddInfo|CA_CVforCurrConv", Caption = "Value for Currency Conversion", ShortCaption = "Conv.Value", MediumCaption = "Conversion Value", FullDescription = "The value in the currency of the invoice that will be converted to CAD for duty calculation.")]
		public override ZDecimal CA_CVforCurrConv
		{
			get
			{
				if (!CA_CVforCurrConvOvr
					&& (CA_CalculationMethod != CalculationMethods.Codes.DeliveredDutyPaid
						|| DutyAndTaxManager.IsCalculated))
				{
					using (((ISingleElementListInternal)this).SuspendListChanged())
					using (GetAddInfo().SuspendSettingHasChanges())
					using (GetValidationSuspender())
					using (Declaration?.SuspendMarkApportionmentDirty() ?? DisposableAction.NoAction)
					{
						base.CA_CVforCurrConv = DutyAndTaxManager.GetValueForCurrencyConversion();
					}
				}
				return base.CA_CVforCurrConv;
			}
			set { base.CA_CVforCurrConv = value; }
		}

		#endregion

		#region DutyAndTaxManager

		public DutyAndTaxManager DutyAndTaxManager
		{
			get
			{
				if (dutyAndTaxManager == null)
				{
					if (dutiesAndTaxes == null)
					{
						SetDutiesAndTaxes();
					}
					else
					{
						dutyAndTaxManager = new DutyAndTaxManager(dutiesAndTaxes, this);
						dutyAndTaxManager.RunValidateDutiesAndTaxes += () => AddInfoValidation.ValidateCA_CustomsValue();
						dutyAndTaxManager.OnStartCalculation += (x, y) => SuspendMarkApportionmentDirtyForDutiesAndTaxes();
						dutyAndTaxManager.OnFinishCalculation += (x, y) => RefreshCalculatedFields();
						dutyAndTaxManager.OnFinishCalculation += (x, y) => ResumeMarkApportionmentDirtyForDutiesAndTaxes();
					}
				}
				return dutyAndTaxManager;
			}
		}

		public void ResetDutyAndTaxManager()
		{
			dutyAndTaxManager = null;
			dutiesAndTaxes = null;
		}

		protected override void BeforeSuccessfulDelete()
		{
			base.BeforeSuccessfulDelete();
			if (dutyAndTaxManager != null)
			{
				((IDisposable)dutyAndTaxManager).Dispose();
			}
		}

		public void SuspendMarkApportionmentDirtyForDutiesAndTaxes()
		{
			OnMarkApportionmentDirty -= JobComInvoiceLine_OnMarkApportionmentDirty;
			DutiesAndTaxes.HasChangesChanged -= JobComInvoiceLine_OnMarkApportionmentDirty;
		}

		public void ResumeMarkApportionmentDirtyForDutiesAndTaxes()
		{
			OnMarkApportionmentDirty += JobComInvoiceLine_OnMarkApportionmentDirty;
			DutiesAndTaxes.HasChangesChanged += JobComInvoiceLine_OnMarkApportionmentDirty;
		}

		void RefreshCalculatedFields()
		{
			CA_CustomsValueInfo.RefreshBinding();
			CA_CVforCurrConvInfo.RefreshBinding();
			JI_Calc_FOBInfo.RefreshBinding();
			JI_Calc_CIFInfo.RefreshBinding();
			RefreshAggregatedFields();
		}

		internal void RefreshAggregatedFields()
		{
			AggregatedFieldsCalculator.CalculateAggregatedFields(this);
		}

		DutyAndTaxManager dutyAndTaxManager;

		#endregion

		#region B3SubHeader Line Level

		public ZInt B3SubHeaderNumberForLVX { get; set; }

		internal ZDecimal FreightCharges
		{
			get
			{
				var chargeKey = IncoTermAndChargeFactory.GetCharge(CustomsChargeTypeList.Codes.OverseasFreight)?.ChargeCodeChargeKey;
				var overseasFreightCharge = chargeKey == null ? Money.Empty : CurrencyConverter.Add(Charges.GetCharge(chargeKey), ApportionedCharges.GetCharge(chargeKey));
				return InvoiceHeader?.GetAmountInCanadianDollars(overseasFreightCharge, false) ?? ZDecimal.Zero;
			}
		}

		public ZString EffectiveCountryAndStateOfOrigin
		{
			get { return GetCountryAndState(EffectiveCountryOfOrigin, EffectiveProvinceOfOrigin); }
		}

		public ZString CountryAndStateOfOrigin
		{
			get { return GetCountryAndState(JI_CountryOfOrigin, JI_StateOrRegionOfOrigin); }
		}

		internal ZString EffectiveCountryAndStateOfExport
		{
			get { return GetCountryAndState(EffectiveCountryOfExport, EffectiveStateOfExport); }
		}

		public ZString CountryAndStateOfExport
		{
			get { return GetCountryAndState(CA_RN_NKExport, CA_USStateOfExport); }
		}

		internal ZString EffectiveCountryOfExport
		{
			get { return CA_RN_NKExport.IsEmpty ? (InvoiceHeader?.CA_RN_NKExport ?? ZString.Empty) : CA_RN_NKExport; }
		}

		ZString EffectiveStateOfExport
		{
			get { return CA_USStateOfExport.IsEmpty ? (InvoiceHeader?.CA_USStateOfExport ?? ZString.Empty) : CA_USStateOfExport; }
		}

		public ZString EffectiveTreatmentCode
		{
			get { return CA_TreatmentCode.IsEmpty ? (InvoiceHeader?.CA_TreatmentCode ?? ZString.Empty) : CA_TreatmentCode; }
		}

		static ZString GetCountryAndState(ZString country, ZString state)
		{
			return country == Core.Constants.CountryCodes.UnitedStates ? (ZString)("U" + state) : country;
		}

		#endregion

		internal ZInt InvoiceCrossReferencePageLineNumber { get; set; }

		#endregion
	}
}
