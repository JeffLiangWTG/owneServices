using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class DutyAndTaxManager : IDisposable
	{
		internal DutyAndTaxManager(DutyAndTaxCollection lines, IDutyAndTaxData data)
		{
			this.data = data;
			dutiesAndTaxes = lines;
			dutiesAndTaxes.ElementOverrideValueChanged += ElementOverrideValueChanged;
			dutiesAndTaxes.ElementTaxTypeValueChanged += ElementTaxTypeValueChanged;
			dutiesAndTaxes.ElementCodeValueChanged += ElementCodeValueChanged;
			data.QuantityChanged += ElementQuantityValueChanged;
			dutiesAndTaxes.CountChanged += DutiesAndTaxes_CountChanged;
			factory = dutiesAndTaxes.Factory;
		}

		void IDisposable.Dispose()
		{
			dutiesAndTaxes.ElementOverrideValueChanged -= ElementOverrideValueChanged;
			dutiesAndTaxes.ElementTaxTypeValueChanged -= ElementTaxTypeValueChanged;
			dutiesAndTaxes.ElementCodeValueChanged -= ElementCodeValueChanged;
			data.QuantityChanged -= ElementQuantityValueChanged;
			dutiesAndTaxes.CountChanged -= DutiesAndTaxes_CountChanged;
#if DEBUG
			isDisposed = true;
#endif
		}

#if DEBUG
		internal bool isDisposed;
#endif

		#region Properties

		#region DeliveredDutyPaidFOBValue

		internal ZDecimal GetDeliveredDutyPaidFOBValue()
		{
			var result = ZDecimal.Zero;
			if (data.CalculationMethod == CalculationMethods.Codes.DeliveredDutyPaid)
			{
				switch (data.AdjustmentCode)
				{
					case AmountTypes.Codes.Dollar:
						result = data.ValueForCurrencyConversion - data.AdjustmentValue;
						break;
					case AmountTypes.Codes.Percent:
						result = Utilities.Round(data.ValueForCurrencyConversion / (1m + data.AdjustmentValue / 100m), 2);
						break;
					default:
						result = data.ValueForCurrencyConversion;
						break;
				}
			}
			return result;
		}

		#endregion

		#region ValueForCurrencyConversion

		internal ZDecimal GetValueForCurrencyConversion()
		{
			return data.CalculationMethod == CalculationMethods.Codes.DeliveredDutyPaid && customsValue.HasValue
					? GetValueForCurrencyConversionFromCalculatedCustomsValue() : GetValueForCurrencyConversionFromFOB();
		}

		internal ZDecimal GetValueForCurrencyConversionFromFOB()
		{
			ZDecimal result;
			switch (data.AdjustmentCode)
			{
				case AmountTypes.Codes.Dollar:
					result = data.FOBValue + data.AdjustmentValue;
					break;
				case AmountTypes.Codes.Percent:
					result = data.FOBValue * (1m + data.AdjustmentValue / 100m);
					break;
				default:
					result = data.FOBValue;
					break;
			}
			return result;
		}

		internal ZDecimal GetValueForCurrencyConversionFromCalculatedCustomsValue()
		{
			var value = data.CalculateCustomsValueWithoutDutyIfDutyPaid ? customsValue.GetValueOrDefault() : data.CustomsValue;
			var result = data.ExchangeRate.IsEmpty ? 0m : value / data.ExchangeRate;
			return Utilities.Round(result, 2);
		}

		#endregion

		#region CustomsValueForDuty

		internal ZDecimal GetCustomsValueForDuty(decimal? alternativeExchangeRate = null)
		{
			if (data.CalculationMethod != CalculationMethods.Codes.DeliveredDutyPaid || !customsValue.HasValue)
			{
				ZDecimal exchangeRateForCurrencyConversion = alternativeExchangeRate ?? data.ExchangeRate;
				return Utilities.Round(exchangeRateForCurrencyConversion.IsEmpty ? 0m : data.ValueForCurrencyConversion * exchangeRateForCurrencyConversion, 2);
			}
			return customsValue.GetValueOrDefault();
		}
		ZDecimal? customsValue;

		#endregion

		#region NormalValueForTax

		internal ZDecimal NormalValueForTax
		{
			get { return normalValueForTax ?? (normalValueForTax = NormalDutyPaidValue + ExciseTaxesTotalAmount).Value; }
		}

		ZDecimal? normalValueForTax;

		#endregion

		#region CalculatedValueForTax

		internal ZDecimal CalculatedValueForTax
		{
			get
			{
				return calculatedValueForTax ?? (calculatedValueForTax = CalculatedValueForTaxCore()).Value;
			}
		}
		ZDecimal? calculatedValueForTax;

		internal ZDecimal CalculatedValueForTaxCore()
		{
			if (!IsCalculated)
			{
				CalculateCustomsValueWithoutDutyIfDutyPaid();
			}

			return customsValue.GetValueOrDefault() + DutiesTotalAmount + SIMAPayableAmount + ExciseTaxesTotalAmount;
		}

		#endregion

		#region NormalDutyPaidValue

		internal ZDecimal NormalDutyPaidValue
		{
			get { return customsValue.GetValueOrDefault() + DutiesTotalAmount + SIMAPayableAmount; }
		}

		#region ExciseTaxesTotalAmount

		internal ZDecimal ExciseTaxesTotalAmount
		{
			get { return (exciseTaxesTotalAmount ?? (exciseTaxesTotalAmount = GetTotalAmount(ExciseTaxes))).Value; }
		}

		ZDecimal? exciseTaxesTotalAmount;

		#endregion

		#region DutiesTotalAmount

		internal ZDecimal DutiesTotalAmount
		{
			get { return (dutiesTotalAmount ?? (dutiesTotalAmount = GetTotalAmount(Duties))).Value; }
		}

		ZDecimal? dutiesTotalAmount;

		#endregion

		#region SIMAPayableAmount

		public ZDecimal SIMAPayableAmount
		{
			get { return SIMADuties.Sum(duty => IDutyAndTaxDataExtensions.IsSimaAmountPayable(duty.C1_ExemptCode) || duty.IsSurtax ? duty.C1_Amount : ZDecimal.Zero); }
		}

		#endregion

		#region GSTTaxesTotalAmount

		internal ZDecimal GSTTaxesTotalAmount
		{
			get { return (gstTaxesTotalAmount ?? (gstTaxesTotalAmount = GetTotalAmount(GSTaxes))).Value; }
		}

		ZDecimal? gstTaxesTotalAmount;

		#endregion

		#endregion

		#region Unit Converter

		DutyAndTaxUnitConverter UnitConverter
		{
			get { return fUnitConverter ?? (fUnitConverter = new DutyAndTaxUnitConverter()); }
		}
		DutyAndTaxUnitConverter fUnitConverter;

		#endregion

		#endregion

		#region Get Amount/Rate/Code/UOM

		internal ZDecimal GetTotalAmount(ZString dutyOrTaxType)
		{
			return GetTotalAmount(GetDutiesOrTaxesByType(dutyOrTaxType));
		}

		static ZDecimal GetTotalAmount(IEnumerable<DutyAndTax> dutiesOrTaxes)
		{
			return dutiesOrTaxes.Sum(dutyOrTax => dutyOrTax.C1_Amount);
		}

		internal ZDecimal GetAmount(ZString dutyOrTaxType, int? index = null)
		{
			return index.HasValue ? GetValue(dutyOrTaxType, d => d.C1_Amount, index) : GetTotalAmount(dutyOrTaxType);
		}

		internal ZString GetExemptCode(ZString dutyOrTaxType, int? index = null, bool getSpecifiedTypeForSIMA = false)
		{
			return GetValue(dutyOrTaxType, d => d.C1_ExemptCode, index, getSpecifiedTypeForSIMA);
		}

		internal ZDecimal GetRate(ZString dutyOrTaxType, int? index = null)
		{
			return GetValue(dutyOrTaxType, d => d.C1_Rate, index);
		}

		internal ZString GetRateType(ZString dutyOrTaxType, int? index = null)
		{
			return GetValue(dutyOrTaxType, d => d.C1_RateType, index);
		}

		internal ZString GetUnitOfMeasure(ZString dutyOrTaxType, int? index = null, bool getSpecifiedTypeForSIMA = false)
		{
			return GetValue(dutyOrTaxType, d => d.C1_UnitOfMeasure, index, getSpecifiedTypeForSIMA);
		}

		internal ZString GetCode(ZString dutyOrTaxType, int? index = null, bool getSpecifiedTypeForSIMA = false)
		{
			return GetValue(dutyOrTaxType, d => d.C1_Code, index, getSpecifiedTypeForSIMA);
		}

		internal ZBool GetIsOverride(ZString dutyOrTaxType, int? index = null, bool getSpecifiedTypeForSIMA = false)
		{
			return GetValue(dutyOrTaxType, d => d.C1_Override, index, getSpecifiedTypeForSIMA);
		}

		T GetValue<T>(ZString dutyOrTaxType, Func<DutyAndTax, T> getValue, int? index = null, bool getSpecifiedTypeForSIMA = false) where T : IZType
		{
			var dutiesOrTaxes = GetDutiesOrTaxesByType(dutyOrTaxType, getSpecifiedTypeForSIMA);
			if (index.HasValue)
			{
				var dutyOrTax = dutiesOrTaxes.ElementAtOrDefault(index.GetValueOrDefault());
				return dutyOrTax != null ? getValue(dutyOrTax) : default(T);
			}
			return (from dutyOrTax in dutiesOrTaxes let value = getValue(dutyOrTax) where !value.IsEmpty select value).FirstOrDefault();
		}

		internal IEnumerable<DutyAndTax> GetDutiesOrTaxesByType(ZString dutyOrTaxType, bool getSpecifiedTypeForSIMA = false)
		{
			switch (dutyOrTaxType)
			{
				case DutyAndTaxTypes.Codes.CustomsDuty:
					return Duties;
				case DutyAndTaxTypes.Codes.SIMADuty:
				case DutyAndTaxTypes.Codes.ADD:
				case DutyAndTaxTypes.Codes.CVD:
				case DutyAndTaxTypes.Codes.SUR:
					if (getSpecifiedTypeForSIMA)
					{
						return GetDutiesOrTaxesOrdered(dutyOrTaxType);
					}
					else
					{
						return SIMADuties;
					}
				case DutyAndTaxTypes.Codes.ExciseTax:
					return ExciseTaxes;
				case DutyAndTaxTypes.Codes.GST:
					return GSTaxes;
				case DutyAndTaxTypes.Codes.CPT:
					return CPTTaxes;
				case DutyAndTaxTypes.Codes.CTA:
					return CTATaxes;
				default:
					return GetDutiesOrTaxesOrdered(dutyOrTaxType);
			}
		}

		#endregion

		#region PopulateDutiesAndTaxes

		internal ZString DeriveDutyRateShortDescription()
		{
			var builder = new ZStringBuilder();
			using (var calcMarker = new CalculatingMarker(this))
			{
				PopulateNormalDutiesIfRequired();
				foreach (var duty in calculatingDutiesAndTaxes)
				{
					if (duty.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty)
					{
						builder.Append(duty.AmountDescription);
					}
				}
			}
			var result = builder.ToStringWithDelimiterBetweenAppends(" + ");
			return string.IsNullOrEmpty(result) ? Res.GetString("c50b562d-852a-4c8d-aab6-f25da7f0694f", "NO RATE") : result;
		}

		internal ZString DeriveTaxRateShortDescription()
		{
			var builder = new ZStringBuilder();
			using (var calcMarker = new CalculatingMarker(this))
			{
				PopulateDutiesAndTaxesFromGlobalTariff(true);
				PopulateDutiesOrTaxesFromClassificationNumber(PopulateNormalTaxes);

				foreach (var duty in calculatingDutiesAndTaxes)
				{
					if (duty.IsGST && !duty.AmountDescription.IsEmpty)
					{
						builder.Append(Res.GetString("6c35053a-5113-42f9-9c6e-0a503af042a7", "GST: {0}", duty.AmountDescription));
					}
					else if (duty.IsExciseTax)
					{
						builder.Append(Res.GetString("5a4bff58-5786-47b8-b926-cdb4d5a3a14e", "EXT: {0}", duty.AmountDescription));
					}
				}
			}
			var result = builder.ToStringWithDelimiterBetweenAppends(" + ");
			return string.IsNullOrEmpty(result) ? Res.GetString("c50b562d-852a-4c8d-aab6-f25da7f0694f", "NO RATE") : result;
		}

		internal void PopulateDutiesAndTaxes()
		{
			using (var calcMarker = new CalculatingMarker(this))
			{
				IsCalculated = true;
				FireOnStartCalculation();
				RemoveNotOverriddenDuties();
				RemoveNotOverriddenCasualImportProvincialTaxesForNormalBills();
				RefreshDataAfterCalculated();
				AddSIMADutyIfNotExists();
				PopulateNormalDutiesIfRequired();
				PopulateDutiesAndTaxesFromGlobalTariff(false);
				PopulateDutiesOrTaxesFromClassificationNumber(PopulateNormalTaxes);
				RecalculateRemissions();
				PopulateCasualImportProvincialTaxesIfRequired();
				data.ValueForTax = this.CalculatedValueForTax;
				FireRunValidateDutiesAndTaxes();
				FireOnFinishCalculation();
			}
		}

		void RemoveCustomsDutyAndExciseTax()
		{
			var dutiesAndTaxesToRemove = calculatingDutiesAndTaxes.Where(dutyOrTax => !dutyOrTax.C1_Override && dutyOrTax.IsExciseTax).ToList();
			dutiesAndTaxesToRemove.ForEach(tax => dutiesAndTaxes.Delete(tax));
		}

		DutyAndTax AddNewOrReuseOldDutyOrTax(ZString taxType, ZString code, ZString exemptCode, ZString rateType, ZString unitOfMeasure, bool isInRefFiles = false)
		{
			DutyAndTax result = null;
			var previousTranNumber = ZString.Empty;
			var previousTranLine = ZInt.Zero;

			if (taxType == DutyAndTaxTypes.Codes.CustomsDuty && !Duties.Any())
			{
				previousTranNumber = data.PreviousTransactionNumber;
				previousTranLine = data.PreviousLineNumber;
			}

			if (IsCaculatingDutyAndTax)
			{
				result = dutiesAndTaxes.FirstOrDefault(tax => tax.IsOldRow
															&& tax.C1_TaxType == taxType
															&& tax.C1_Code == code
															&& tax.C1_ExemptCode == exemptCode
															&& tax.C1_RateType == rateType
															&& tax.C1_UnitOfMeasure == unitOfMeasure
															&& tax.C1_PreviousTranNumber == previousTranNumber
															&& tax.C1_PreviousTranLine == previousTranLine);
			}
			if (result == null)
			{
				result = dutiesAndTaxes.AddNew(taxType);
				result.IsInRefFiles = isInRefFiles;
				result.C1_Code = code;
				result.C1_ExemptCode = exemptCode;
				result.C1_RateType = rateType;
				result.C1_UnitOfMeasure = unitOfMeasure;
				result.C1_PreviousTranNumber = previousTranNumber;
				result.C1_PreviousTranLine = previousTranLine;
			}
			else
			{
				result.IsOldRow = false;
				result.IsInRefFiles = isInRefFiles;
			}
			return result;
		}

		void ClearOldDutiesAndTaxes()
		{
			var dutiesAndTaxesToRemove = dutiesAndTaxes.Where(t => t.IsOldRow).ToList();
			dutiesAndTaxesToRemove.ForEach(tax => dutiesAndTaxes.Delete(tax));
		}

		void DeleteDutyAndTaxOrSetAsOldRow(DutyAndTax dutyAndTax)
		{
			if (IsCaculatingDutyAndTax)
			{
				dutyAndTax.IsOldRow = true;
				dutyAndTax.ClearFieldsForRecycle();
			}
			else
			{
				dutiesAndTaxes.Delete(dutyAndTax);
			}
		}

		internal void PopulateDummyLineGSTTax(ZDecimal taxAmount)
		{
			using (data.Declaration.SuspendMarkApportionmentDirty())
			{
				var tax = dutiesAndTaxes.AddNew(DutyAndTaxTypes.Codes.GST);
				tax.ReadOnly = true;
				tax.C1_Code = NormalGSTRefNum;
				tax.C1_Override = true;
				tax.C1_Rate = NormalGSTRate;
				tax.C1_Amount = Utilities.Round(taxAmount, 2);
			}
		}

		#region RemoveNotOverriddenDutiesAndTaxes

		internal void RemoveNotOverriddenDutiesAndTaxes()
		{
			var dutiesAndTaxesToRemove = calculatingDutiesAndTaxes.Where(dutyOrTax => !dutyOrTax.C1_Override && (!DutyAndTaxTypes.IsSIMATaxCode(dutyOrTax.C1_TaxType) || !data.IsSIMADutyRequired)).ToList();
			dutiesAndTaxesToRemove.ForEach(tax => DeleteDutyAndTaxOrSetAsOldRow(tax));
		}

		void RemoveNotOverriddenDuties()
		{
			var dutiesAndTaxesToRemove = calculatingDutiesAndTaxes.Where(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && !dutyOrTax.C1_Override && !dutyOrTax.IsWarehouseOrSupplementaryEntryHasValues).ToList();
			dutiesAndTaxesToRemove.ForEach(tax => DeleteDutyAndTaxOrSetAsOldRow(tax));
		}

		internal void RemoveNotOverriddenSIMADuties()
		{
			var simaDutiesToRemove = calculatingDutiesAndTaxes.Where(dutyOrTax => DutyAndTaxTypes.IsSIMATaxCode(dutyOrTax.C1_TaxType) && !dutyOrTax.C1_Override).ToList();
			simaDutiesToRemove.ForEach(tax => DeleteDutyAndTaxOrSetAsOldRow(tax));
		}

		void RemoveNotOverriddenCasualImportProvincialTaxesForNormalBills()
		{
			var casualImportTaxData = data as ICasualImportTaxData;

			if (casualImportTaxData == null || !casualImportTaxData.IsEffectiveCasualImport)
			{
				RemoveExistingNotOverridenLinesAndCacheExcemptionCodeByTaxType(DutyAndTaxTypes.Codes.CPT);
				RemoveExistingNotOverridenLinesAndCacheExcemptionCodeByTaxType(DutyAndTaxTypes.Codes.CTA);
			}
		}

		#endregion

		#region CalculateCustomsValueWithoutDutyIfDutyPaid

		void CalculateCustomsValueWithoutDutyIfDutyPaid()
		{
			customsValue = null;
			customsValue = data.CustomsValue;

			if (data.CalculationMethod == CalculationMethods.Codes.DeliveredDutyPaid &&
					data.CalculateCustomsValueWithoutDutyIfDutyPaid)
			{
				customsValue = null;
				var valueWithDuty = GetCustomsValueForDuty();
				var tempManager = GetTempManagerWithPopulatedDuties(valueWithDuty, true);

				ZDecimal totalSpecificTax = 0;
				ZDecimal sumOfAdValoremTaxRates = 0;

				if (!data.DDPDeductDutyOnly)
				{
					foreach (var dutyOrTax in tempManager.dutiesAndTaxes.Where(d => NeedToBeDeductedWhenDDPDeductDutyOnlyIsFalse(d.C1_TaxType)))
					{
						if (dutyOrTax.C1_ExemptCode.IsEmpty)
						{
							if (dutyOrTax.C1_RateType == RateTypes.Codes.AdValorem && !dutyOrTax.C1_Override)
							{
								sumOfAdValoremTaxRates += dutyOrTax.C1_Rate;
							}
							else
							{
								totalSpecificTax += dutyOrTax.C1_Amount;
							}
						}
					}
				}
				valueWithDuty = 100 * (valueWithDuty - totalSpecificTax) / (100 + sumOfAdValoremTaxRates);

				totalSpecificTax = 0;
				sumOfAdValoremTaxRates = 0;
				foreach (var exciseTax in tempManager.dutiesAndTaxes.Where(d => d.IsExciseTax))
				{
					if (exciseTax.C1_ExemptCode.IsEmpty)
					{
						if (exciseTax.C1_RateType == RateTypes.Codes.AdValorem && !exciseTax.C1_Override)
						{
							sumOfAdValoremTaxRates += exciseTax.C1_Rate;
						}
						else
						{
							totalSpecificTax += exciseTax.C1_Amount;
						}
					}
				}
				valueWithDuty = 100 * (valueWithDuty - totalSpecificTax) / (100 + sumOfAdValoremTaxRates);

				ZDecimal sumOfOtherDuties = 0;
				ZDecimal sumOfAdValoremDutyRates = 0;

				foreach (var duty in tempManager.dutiesAndTaxes.Where(d => d.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty || DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(d.C1_TaxType)))
				{
					if (!DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(duty.C1_TaxType) || IDutyAndTaxDataExtensions.IsSimaAmountPayable(duty.C1_ExemptCode))
					{
						if (duty.C1_RateType == RateTypes.Codes.AdValorem && !duty.C1_Override)
						{
							sumOfAdValoremDutyRates += duty.C1_Rate;
						}
						else
						{
							sumOfOtherDuties += duty.C1_Amount;
						}
					}
				}

				customsValue = !tempManager.adValoremDuties.Any()
					? CalculateCustomsValue(valueWithDuty, sumOfOtherDuties, sumOfAdValoremDutyRates)
					: CalculateCustomsValue(valueWithDuty, sumOfOtherDuties, sumOfAdValoremDutyRates, tempManager.adValoremDuties);
				customsValueInvalid = customsValue.GetValueOrDefault() == 0 && valueWithDuty > 0;
			}
		}

		bool NeedToBeDeductedWhenDDPDeductDutyOnlyIsFalse(ZString taxType)
		{
			return taxType == DutyAndTaxTypes.Codes.GST || taxType == DutyAndTaxTypes.Codes.CPT || taxType == DutyAndTaxTypes.Codes.CTA;
		}

		void RefreshDataAfterCalculated()
		{
			CalculateCustomsValueWithoutDutyIfDutyPaid();

			if (data.CalculationMethod == CalculationMethods.Codes.DeliveredDutyPaid && data.CalculateCustomsValueWithoutDutyIfDutyPaid)
			{
				var saveIgnoreValidationSuspended = data.Declaration.IgnoreValidationSuspended;
				try
				{
					data.Declaration.IgnoreValidationSuspended = false;
					customsValue = Utilities.Round(GetValueForCurrencyConversionFromCalculatedCustomsValue() * data.ExchangeRate, 2);
					data.CustomsValue = customsValue.GetValueOrDefault();
				}
				finally
				{
					data.Declaration.IgnoreValidationSuspended = saveIgnoreValidationSuspended;
				}
			}
			else
			{
				data.CustomsValue = Utilities.Round(customsValue.GetValueOrDefault(), 2);
			}
		}

		ZDecimal CalculateCustomsValue(ZDecimal value, ZDecimal sumOfOtherDuties, ZDecimal sumOfAdValoremDutyRates, IEnumerable<CombinedDuty> combinedDuties)
		{
			var singleCombinedDuty = combinedDuties.Take(2).Count() == 1;
			var first = combinedDuties.First();
			foreach (var combinedDuty in first.AsEnumerable())
			{
				var adValorem = sumOfAdValoremDutyRates;
				var other = sumOfOtherDuties;
				foreach (var regular in combinedDuty.Regulars)
				{
					if (regular.RateType == RateTypes.Codes.AdValorem)
					{
						adValorem += regular.Rate;
					}
					else
					{
						other += regular.Amount;
					}
				}

				var result = singleCombinedDuty
								? CalculateCustomsValue(value, other, adValorem)
								: CalculateCustomsValue(value, other, adValorem, combinedDuties.Where(d => d != first));
				if (result > ZDecimal.Zero)
				{
					return result;
				}
			}
			return ZDecimal.Zero;
		}

		ZDecimal CalculateCustomsValue(ZDecimal value, ZDecimal sumOfOtherDuties, ZDecimal sumOfAdValoremDutyRates)
		{
			ZDecimal result = 100 * (value - sumOfOtherDuties) / (100 + sumOfAdValoremDutyRates);
			var tempManager = GetTempManagerWithPopulatedDuties(result, false);
			return Math.Abs(tempManager.NormalDutyPaidValue - value) <= 0.02m ? result : 0;
		}

		DutyAndTaxManager GetTempManagerWithPopulatedDuties(ZDecimal value, bool customsValueCalculation)
		{
			var tempDuties = new DutyAndTaxCollection(NewTempFactory(), dutiesAndTaxes.DutyAndTaxDataMaster);
			var tempManager = new DutyAndTaxManager(tempDuties, data) { customsValue = value };

			using (var calcMarker = new CalculatingMarker(tempManager))
			{
				foreach (var duty in Duties.Concat(SIMADuties))
				{
					tempManager.AddApplicableDuty(duty, duty.AmountDescription);
				}

				tempManager.calculatingCustomsValueForDuty = customsValueCalculation;
				tempManager.AddSIMADutyIfNotExists();
				tempManager.PopulateNormalDutiesIfRequired();
				if (customsValueCalculation)
				{
					tempManager.PopulateDutiesOrTaxesFromClassificationNumber(tempManager.PopulateNormalTaxes);
					foreach (var existingTax in GSTaxes)
					{
						SetAnyExistingTaxCodeAndExemption(existingTax, tempManager.GSTaxes, tempManager);
					}
					foreach (var existingTax in ExciseTaxes)
					{
						SetAnyExistingTaxCodeAndExemption(existingTax, tempManager.ExciseTaxes, tempManager);
					}

					tempManager.PopulateCasualImportProvincialTaxesIfRequired();
				}
			}
			tempManager.calculatingIndex++;
			return tempManager;
		}

		void SetAnyExistingTaxCodeAndExemption(DutyAndTax existingTax, IEnumerable<DutyAndTax> tempTaxes, DutyAndTaxManager tempManager)
		{
			var taxTypeCode = existingTax.C1_TaxType;
			var tempTax = tempTaxes.FirstOrDefault(x => x.C1_TaxType == taxTypeCode);
			if (tempTax != null)
			{
				tempTax.C1_Code = existingTax.C1_Code;
				tempTax.C1_ExemptCode = existingTax.C1_ExemptCode;
				tempManager.UpdateTaxRate(tempTax);
			}
		}

		bool calculatingCustomsValueForDuty;

		#endregion

		#region AddSIMADutyIfNotExists

		internal void AddSIMADutyIfNotExists()
		{
			if (data.IsSIMADutyRequired)
			{
				using (var calcMarker = new CalculatingMarker(this))
				{
					var tariffViewForSurtax = TariffView;
					if (tariffViewForSurtax != null)
					{
						AddSIMADutyIfNotExistsOrCalculateAmount(tariffViewForSurtax, DutyAndTaxTypes.Codes.SUR);
					}

					var tariffViewForAntiDumping = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, SIMATariffType, data.DumpingCaseNumber, data.EffectiveDutyDate, data.ClassificationNumber);
					if (tariffViewForAntiDumping != null)
					{
						AddSIMADutyIfNotExistsOrCalculateAmount(tariffViewForAntiDumping, DutyAndTaxTypes.Codes.ADD);
						AddSIMADutyIfNotExistsOrCalculateAmount(tariffViewForAntiDumping, DutyAndTaxTypes.Codes.CVD);
					}
				}
			}
		}
		public const string SIMATariffType = "SIMA";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void AddSIMADutyIfNotExistsOrCalculateAmount(TariffView tariffViewForSIMA, ZString simaDutyCode)
		{
			DutyAndTax simaDuty = null;
			if (simaDutyCode == DutyAndTaxTypes.Codes.SUR)
			{
				simaDuty = dutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == simaDutyCode);
			}

			var cusDutyRate = GetRateByTradeGroup(tariffViewForSIMA, data.EffectiveDutyDate, new ZString[] { simaDutyCode }, data.CountryOfOrigin, simaDuty?.C1_Code ?? ZString.Empty);
			if (cusDutyRate == null && (simaDutyCode == DutyAndTaxTypes.Codes.ADD || simaDutyCode == DutyAndTaxTypes.Codes.CVD) && !data.CountryOfExport.IsEmpty)
			{
				cusDutyRate = GetRateByTradeGroup(tariffViewForSIMA, data.EffectiveDutyDate, new ZString[] { simaDutyCode }, data.CountryOfExport);
			}

			if (cusDutyRate != null)
			{
				if (simaDuty == null)
				{
					if (data is CusEntryLine)
					{
						simaDuty = dutiesAndTaxes.FirstOrDefault(x => DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(x.C1_TaxType));
					}
					else
					{
						simaDuty = dutiesAndTaxes.FirstOrDefault(x => x.C1_TaxType == simaDutyCode);
					}

					if (simaDuty == null)
					{
						simaDuty = dutiesAndTaxes.AddNew();
						simaDuty.C1_TaxType = simaDutyCode;
					}
				}

				var exchangeRate = (simaDutyCode == DutyAndTaxTypes.Codes.SUR) ? data.ExchangeRate : data.ExchangeRateForSIMA;
				SetSIMADutyPropertiesFromCusDutyRate(factory, simaDuty, cusDutyRate, data, GetCustomsValueForDuty(exchangeRate));

				if (simaDuty.C1_ExemptCode.IsEmpty)
				{
					var simaDutyHasExemptCode = dutiesAndTaxes.FirstOrDefault(d => DutyAndTaxTypes.IsSIMATaxCode(d.C1_TaxType) && !d.C1_ExemptCode.IsEmpty);
					if (simaDutyHasExemptCode != null)
					{
						simaDuty.C1_ExemptCode = simaDutyHasExemptCode.C1_ExemptCode;
					}
				}
			}
		}
		public const string VFDInFormula = "VFD";

		static void SetSIMADutyPropertiesFromCusDutyRate(BusinessObjectFactory factory, DutyAndTax simaDuty, RateView cusDutyRate, IDutyAndTaxData data, ZDecimal customsValue)
		{
			if (!simaDuty.C1_Override)
			{
				simaDuty.C1_NormalValuePerUnit = ZDecimal.Zero;
				simaDuty.C1_NormalValueCurrency = ZString.Empty;
				simaDuty.C1_ForeignRate = ZDecimal.Zero;
				simaDuty.C1_ForeignCurrency = ZString.Empty;
				simaDuty.C1_RateType = ZString.Empty;
				simaDuty.C1_Rate = ZDecimal.Zero;
				simaDuty.C1_UnitOfMeasure = ZString.Empty;
				simaDuty.C1_Amount = ZDecimal.Zero;

				if (simaDuty.C1_TaxType == DutyAndTaxTypes.Codes.SUR && simaDuty.C1_Code.IsEmpty)
				{
					simaDuty.C1_Code = cusDutyRate.FilteredRateApplicabilities.FirstOrDefault(x => x.IsApplicable(data.CountryOfOrigin, data.EffectiveDutyDate))?.ZZT_AdditionalCode ?? String.Empty;
				}

				var rateAndUnit = cusDutyRate.ZZ2_RateFormula.KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.*").Split('*').OrderBy(x => x).ToArray();
				if (rateAndUnit.Length == 2)
				{
					var rate = ZDecimal.ParseSafe(rateAndUnit[0], 0);
					var rateType = ZString.Empty;
					var unitOfMeasure = ZString.Empty;
					var foreignCurrency = ZString.Empty;
					if (rateAndUnit[1] == DutyAndTaxManager.VFDInFormula)
					{
						rate = rate * 100;
						rateType = RateTypes.Codes.AdValorem;
					}
					else
					{
						rateType = RateTypes.Codes.Specific;
						unitOfMeasure = rateAndUnit[1];

						if (!cusDutyRate.ZZ2_RX_NKCurrencyOverride.IsEmpty)
						{
							foreignCurrency = cusDutyRate.ZZ2_RX_NKCurrencyOverride;
						}
					}

					if (foreignCurrency.IsEmpty)
					{
						simaDuty.C1_Rate = rate;
					}
					else
					{
						simaDuty.C1_ForeignCurrency = foreignCurrency;
						simaDuty.C1_ForeignRate = rate;
					}

					simaDuty.C1_RateType = rateType;
					simaDuty.C1_UnitOfMeasure = unitOfMeasure;

					if (!unitOfMeasure.IsEmpty && data != null)
					{
						if (data.CustomsUnits.IsEmpty || data.CustomsUnits != unitOfMeasure)
						{
							if (data.CustomsUnits2.IsEmpty)
							{
								if (data.CustomsQuantity2.IsEmpty)
								{
									data.CustomsUnits2 = unitOfMeasure;
								}
							}
							else if (data.CustomsUnits2 != unitOfMeasure && data.CustomsUnits3.IsEmpty)
							{
								if (data.CustomsQuantity3.IsEmpty)
								{
									data.CustomsUnits3 = unitOfMeasure;
								}
							}
						}
					}
				}

				if (!simaDuty.C1_ExemptCode.IsEmpty && simaDuty.IsAmountNotRequiredForSIMA)
				{
					simaDuty.C1_RateType = RateTypes.Codes.Exempt;
				}
				simaDuty.AmountDescription = ZString.Empty;
				DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(simaDuty, customsValue);
			}
		}

		internal static void ResetSIMADutiesWhenNotOverridden(BusinessObjectFactory factory, DutyAndTax dutyAndTax, IDutyAndTaxData data)
		{
			if (data != null)
			{
				var taxTypeCode = dutyAndTax?.C1_TaxType ?? ZString.Empty;
				if (taxTypeCode == DutyAndTaxTypes.Codes.SUR)
				{
					var tariffViewForSurtax = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, data.ClassificationNumber, data.EffectiveDutyDate);
					if (tariffViewForSurtax != null)
					{
						ResetSIMADutyFromCusDutyRate(factory, tariffViewForSurtax, dutyAndTax, data, taxTypeCode);
					}
				}
				else if (taxTypeCode == DutyAndTaxTypes.Codes.ADD || taxTypeCode == DutyAndTaxTypes.Codes.CVD)
				{
					var tariffViewForAntiDumping = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, SIMATariffType, data.DumpingCaseNumber, data.EffectiveDutyDate, data.ClassificationNumber);
					if (tariffViewForAntiDumping != null)
					{
						ResetSIMADutyFromCusDutyRate(factory, tariffViewForAntiDumping, dutyAndTax, data, taxTypeCode);
					}
				}
			}
		}

		static void ResetSIMADutyFromCusDutyRate(BusinessObjectFactory factory, TariffView tariffViewForSIMA, DutyAndTax dutyAndTax, IDutyAndTaxData data, ZString simaDutyCode)
		{
			if (data != null)
			{
				var cusDutyRate = GetRateByTradeGroup(tariffViewForSIMA, data.EffectiveDutyDate, new ZString[] { simaDutyCode }, data.CountryOfOrigin);
				if (cusDutyRate == null && (simaDutyCode == DutyAndTaxTypes.Codes.ADD || simaDutyCode == DutyAndTaxTypes.Codes.CVD) && !data.CountryOfExport.IsEmpty)
				{
					cusDutyRate = GetRateByTradeGroup(tariffViewForSIMA, data.EffectiveDutyDate, new ZString[] { simaDutyCode }, data.CountryOfExport);
				}
				if (cusDutyRate != null)
				{
					var exchangeRate = (simaDutyCode == DutyAndTaxTypes.Codes.SUR) ? data.ExchangeRate : data.ExchangeRateForSIMA;
					SetSIMADutyPropertiesFromCusDutyRate(factory, dutyAndTax, cusDutyRate, data, Utilities.Round(data.ValueForCurrencyConversion * exchangeRate, 2));
				}
			}
		}

		internal static RateView GetRateByTradeGroup(TariffView tariffView, ZDateTime dateOfValuation, ZString[] rateTypes, ZString tradeGroup, string rateCode = "")
		{
			var availableRates = tariffView.Rates.Where(d => d.ZZ2_StartDate <= dateOfValuation && d.ZZ2_EndDate >= dateOfValuation && rateTypes.Contains(d.RateCode));
			return availableRates.FirstOrDefault(
				rate => rate.RateApplicabilities.Any(
					applicabilitiy => applicabilitiy.TradeGroup is CusRefTradeGroupView appTradeGroup && applicabilitiy.IsApplicable(tradeGroup, dateOfValuation)
									&& appTradeGroup.ZZA_ZZZ_NKDataGrouping == Core.Constants.CountryCodes.Canada
									&& (string.IsNullOrEmpty(rateCode) || applicabilitiy.ZZT_AdditionalCode == rateCode)
				)
			);
		}

		internal static ZBool IsSIMADutyRequired(BusinessObjectFactory factory, ZBool isImport, ZString tariffNumber, ZString countryCode, ZString dumpingNumber, ZDateTime effectiveDate, ZBool shouldCheckSurTax)
		{
			return factory.GetCachedValue<ZBool>("IsSIMADutyRequired" + isImport + tariffNumber + countryCode + dumpingNumber + effectiveDate + shouldCheckSurTax, () =>
			{
				var result = isImport && !tariffNumber.IsEmpty && !countryCode.IsEmpty;
				if (result)
				{
					result = false;
					if (shouldCheckSurTax)
					{
						var tariffViewForSurTax = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, tariffNumber, effectiveDate);
						result = tariffViewForSurTax != null && DutyAndTaxManager.GetRateByTradeGroup(tariffViewForSurTax, effectiveDate, new ZString[] { DutyAndTaxTypes.Codes.SUR }, countryCode) != null;
					}

					if (!result && !dumpingNumber.IsEmpty)
					{
						var tariffViewForDumping = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, SIMATariffType, dumpingNumber, effectiveDate, tariffNumber);
						if (tariffViewForDumping != null)
						{
							result = DutyAndTaxManager.GetRateByTradeGroup(tariffViewForDumping, effectiveDate, new ZString[] { DutyAndTaxTypes.Codes.ADD }, countryCode) != null;
							if (!result)
							{
								result = DutyAndTaxManager.GetRateByTradeGroup(tariffViewForDumping, effectiveDate, new ZString[] { DutyAndTaxTypes.Codes.CVD }, countryCode) != null;
							}
						}
					}
				}
				return result;
			});
		}

		internal static ZString GetSIMADumpingDescription(BusinessObjectFactory factory, ZString tariffNumber, ZString dumpingNumber, ZDateTime effectiveDate)
		{
			return factory.GetCachedValue("CA_SIMADumpingDesc" + tariffNumber + dumpingNumber + effectiveDate, () =>
			{
				var result = ZString.Empty;
				if (!tariffNumber.IsEmpty && !dumpingNumber.IsEmpty)
				{
					var tariffViewForDumping = new TariffView.Loader(factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, SIMATariffType, dumpingNumber, effectiveDate, tariffNumber);
					if (tariffViewForDumping != null)
					{
						result = tariffViewForDumping.ZZ1_Description;
					}
				}

				return result;
			});
		}

		internal static List<Tuple<ZString, ZString>> GetSIMADumpingNumbersFromTariffAndCountry(BusinessObjectFactory factory, ZString tariffNumber, ZString countryCode, ZDateTime effectiveDate)
		{
			return factory.GetCachedValue("DumpingNumbers" + tariffNumber + countryCode + effectiveDate, () =>
			{
				var result = new List<Tuple<ZString, ZString>>();
				var allTariffs = new TariffView.Loader(factory).GetEffectiveChildTariffs(Core.Constants.CountryCodes.Canada, Universal.Constants.TariffTypes.HarmonizedSystem, tariffNumber, effectiveDate, DutyAndTaxManager.SIMATariffType);
				if (allTariffs != null && allTariffs.Any())
				{
					allTariffs.ForEach(x =>
					{
						var antiDumpingRate = DutyAndTaxManager.GetRateByTradeGroup(x, effectiveDate, new ZString[] { DutyAndTaxTypes.Codes.ADD, DutyAndTaxTypes.Codes.CVD }, countryCode);
						if (antiDumpingRate != null)
						{
							result.Add(new Tuple<ZString, ZString>(x.ZZ1_TariffCode, x.ZZ1_Description));
						}
					});
				}
				return result;
			});
		}

		#endregion

		#region PopulateNormalDuties

		void PopulateNormalDutiesIfRequired()
		{
			var duties = Duties;

			if (!duties.Any() || !duties.Any(x => x.C1_Override || x.IsWarehouseOrSupplementaryEntryHasValues))
			{
				duties.ToList().ForEach(duty => DeleteDutyAndTaxOrSetAsOldRow(duty));

				PopulateNormalDutiesFromTariffCode();
				if (data.TariffCode.IsEmpty || !tariffRateFoundLastTime)
				{
					PopulateDutiesOrTaxesFromClassificationNumber(PopulateClassificationDutyRates);
				}
				PopulateDutiesOrTaxesFromClassificationNumber(PopulateExciseDutyRates);

				ResetDutyAmountsForRegularRemission();
			}

			ClearCachedValues();
		}

		#region PopulateNormalDutiesFromClassificationNumber

		void PopulateDutiesOrTaxesFromClassificationNumber(PopulateDutiesOrTaxesDelegate populateDutiesOrTaxes)
		{
			if (!data.ClassificationNumber.IsEmpty)
			{
				var classHeader = data.GetClassHeader();
				classHeaderFoundLastTime = classHeader != null;
				if (classHeaderFoundLastTime)
				{
					populateDutiesOrTaxes(classHeader);
				}
				else
				{
					RemoveNotOverriddenDutiesAndTaxes();
				}
			}
			else
			{
				classHeaderFoundLastTime = true;
			}
		}

		internal void PopulateDutiesAndTaxesFromGlobalTariff(bool forceDefaultCode)
		{
			var tariffView = TariffView;
			if (tariffView != null)
			{
				var exciseTaxRateCode = CACustomsDataRegistry.Instance.DefaultToThisExciseTaxRateCodeWhenApplicable.Value;
				var defaultRateCode = data.DefaultETRateCode.IsEmpty ? exciseTaxRateCode : data.DefaultETRateCode.ToString();
				var isDefaultCode = !string.IsNullOrEmpty(defaultRateCode) && GetRateViewWithRateCode(tariffView, Universal.Constants.RateTypes.ExciseTax, defaultRateCode, data.EffectiveDutyDate) != null;

				var hasExcise = false;
				var exciseRates = tariffView.Rates.Where(d => d.ZZ2_StartDate <= data.EffectiveDutyDate && d.ZZ2_EndDate >= data.EffectiveDutyDate && d.ZZ2_ZZR_RateTypeCode == Universal.Constants.RateTypes.ExciseTax);

				if (exciseRates.Any())
				{
					hasExcise = true;
					AddNewOrReuseOldDutyOrTax(DutyAndTaxTypes.Codes.ExciseTax, ZString.Empty, ZString.Empty, ZDecimal.Zero, isDefaultCode ? exciseTaxRateCode : ZString.Empty);
					if (!data.DefaultETRateCode.IsEmpty)
					{
						PopulateTaxRate(data.DefaultETRateCode, DutyAndTaxTypes.Codes.ExciseTax, data.DefaultETExemptionCode, updateC1Code: true);
					}
					else if (!CACustomsDataRegistry.Instance.DefaultExciseTaxFromCustomsTariff.Value)
					{
						PopulateTaxRate(exciseRates.First().RateCode, DutyAndTaxTypes.Codes.ExciseTax, data.DefaultETExemptionCode, addWithBlankRate: true, updateC1Code: forceDefaultCode);
					}
					else if (exciseRates.Any(x => x.RateCode == CigarRateCode))
					{
						PopulateHighestExciseTax(exciseRates.Select(x => x.RateCode));
					}
					else
					{
						var refNumber = DutyAndTaxTypes.Constant.NO;
						var refNumberFromHeader = exciseRates.First().RateCode;
						var rate = CACTaxRate.Load(factory, refNumberFromHeader, CACTaxRate.TaxType.Excise, data.EffectiveDutyDate);
						if (rate != null)
						{
							refNumber = refNumberFromHeader;
						}
						PopulateTaxRate(refNumber, DutyAndTaxTypes.Codes.ExciseTax, data.DefaultETExemptionCode, updateC1Code: forceDefaultCode);
					}
				}
				if (!hasExcise)
				{
					RemoveCustomsDutyAndExciseTax();
				}
			}
		}

		public const string CigarRateCode = "E01";

		void PopulateHighestExciseTax(IEnumerable<ZString> refNums)
		{
			var tax = GetDutiesOrTaxesByType(DutyAndTaxTypes.Codes.ExciseTax).FirstOrDefault();
			var code = DutyAndTax.GetExciseTaxCodeYieldHighestTax(dutiesAndTaxes.Factory, dutiesAndTaxes.DutyAndTaxDataMaster, refNums);
			if (tax == null && refNums.Any(x => !x.IsEmpty))
			{
				tax = AddNewOrReuseOldDutyOrTax(DutyAndTaxTypes.Codes.ExciseTax, code, data.DefaultETExemptionCode, ZString.Empty, ZString.Empty);
			}
			if (tax != null)
			{
				if (!tax.C1_Override)
				{
					tax.C1_Code = code;
				}
				UpdateTaxRate(tax);
			}
		}

		void ParseRateFormula(ZString rateFormula, out ZDecimal rate, out ZString rateType, out ZString unitOfMeasure)
		{
			rate = ZDecimal.Zero;
			rateType = ZString.Empty;
			unitOfMeasure = ZString.Empty;

			var formulas = rateFormula.Split('*');
			if (formulas.Length == 2)
			{
				rate = ZDecimal.Parse(formulas[0]);
				if (formulas[1] == VFDInFormula)
				{
					rateType = RateTypes.Codes.AdValorem;
					rate *= 100;
				}
				else
				{
					rateType = RateTypes.Codes.Specific;
					unitOfMeasure = formulas[1].Trim('[', ']');
				}
			}
		}

		void AddNewOrReuseOldDutyOrTax(ZString taxType, ZString rateType, ZString unitOfMeasure, ZDecimal rate, ZString code)
		{
			DutyAndTax dutyAndTax = null;
			if (taxType == DutyAndTaxTypes.Codes.ExciseTax)
			{
				dutyAndTax = dutiesAndTaxes.FirstOrDefault(tax => tax.C1_TaxType == taxType);
			}

			if (dutyAndTax == null)
			{
				dutyAndTax = dutiesAndTaxes.AddNew(taxType);
				dutyAndTax.C1_RateType = rateType;
				dutyAndTax.C1_Rate = rate;
				dutyAndTax.C1_UnitOfMeasure = unitOfMeasure;
				dutyAndTax.C1_Code = code;
				DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(dutyAndTax, GetValueForDutyAndTax(dutyAndTax.C1_TaxType));
			}
			else
			{
				dutyAndTax.IsOldRow = false;
			}
		}

		void PopulateClassificationDutyRates(CACClassHeader classHeader)
		{
			var ratesFound = false;
			var classRateHeaders = CACRateHeader.LoadEffectiveRateHeaders(classHeader, data.EffectiveDutyDate, CACRateHeader.RateType.ClassificationRate);
			if (classRateHeaders.Any())
			{
				if (classRateHeaders.All(x => x.ZB_FreeInd))
				{
					AddFreeClassificationDutyRate();
					ratesFound = true;
				}
				else
				{
					var classRate = CACRate.Load(classHeader.ZA_ClassificationNumber, data.TreatmentCode, data.EffectiveDutyDate, data.Factory);
					var rateLines = classRate?.RateLines;
					if (rateLines != null && rateLines.Count > 0)
					{
						AddDutyRatesFromRateLines(rateLines, data.CalculationMethod == CalculationMethods.Codes.RateDescCalcOnly ? classRate.EffectiveUnitOfMeasure : ZString.Empty, CombinedDuty.Type.Classification);
						ratesFound = true;
					}
					else
					{
						classRate = CACRate.Load(classHeader.ZA_ClassificationNumber, TariffTreatmentCodes.Codes.General, data.EffectiveDutyDate, data.Factory);
						rateLines = classRate?.RateLines;
						if (rateLines != null && rateLines.Count > 0)
						{
							AddDutyRatesFromRateLines(rateLines, data.CalculationMethod == CalculationMethods.Codes.RateDescCalcOnly ? classRate.EffectiveUnitOfMeasure : ZString.Empty, CombinedDuty.Type.Classification);
							ratesFound = true;
						}
					}
				}
			}

			if (!ratesFound)
			{
				AddGeneralClassificationDutyRate();
			}
		}

		void PopulateExciseDutyRates(CACClassHeader classHeader)
		{
			var exciseRate = CACRateHeader.Load(classHeader, data.EffectiveDutyDate, CACRateHeader.RateType.ExciseDutyRate);
			if (exciseRate != null && exciseRate.Rates.Count > 0)
			{
				AddDutyRatesFromRateLines(exciseRate.Rates[0].RateLines, exciseRate.ZB_UnitOfMeasure, CombinedDuty.Type.Excise);
			}
		}

		#endregion

		#region PopulateNormalDutiesFromTariffCode

		void PopulateNormalDutiesFromTariffCode()
		{
			if (!data.TariffCode.IsEmpty)
			{
				tariffRateFoundLastTime = false;
				var tariffHeader = CACTariffHeader.Load(factory, data.EffectiveDutyDate, data.TariffCode);
				if (tariffHeader != null)
				{
					tariffRateFoundLastTime = tariffHeader.ZF_FreeInd || !(tariffHeader.ZF_RateEffectiveDate.Date <= data.EffectiveDutyDate.Date && data.EffectiveDutyDate.Date <= tariffHeader.ZF_RateExpiryDate.Date);
					if (!tariffRateFoundLastTime)
					{
						var tariffRate =  tariffHeader.Rates.OfType<CACRate>().FirstOrDefault(x => !x.ZC_Inactive && x.ZC_TreatmentCode == data.TreatmentCode);
						if (tariffRate != null)
						{
							AddDutyRatesFromRateLines(tariffRate.RateLines, ZString.Empty, CombinedDuty.Type.Tariff);
							tariffRateFoundLastTime = true;
						}
					}
				}
			}
			else
			{
				tariffRateFoundLastTime = true;
			}
		}

		#endregion

		#region Add Duty Rates

		void AddFreeClassificationDutyRate()
		{
			var duty = AddNewOrReuseOldDutyOrTax(DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, ZString.Empty, RateTypes.Codes.Free, ZString.Empty);
			duty.AmountDescription = data.CalculationMethod == CalculationMethods.Codes.RateDescCalcOnly ?
				Res.GetString("81e847c1-535f-4b50-aa6c-7b12b9785ea1", "Free") : Res.GetString("37ef78ab-4d61-417e-8c2d-03ada5ea94f3", "Free Classification duty rate.");
		}

		void AddGeneralClassificationDutyRate()
		{
			var rate = CACustomsDataRegistry.Instance.DefaultGeneralRateOfDuty.Value.EffectiveValue(data.EffectiveDutyDate.Date);
			if (calculatingCustomsValueForDuty)
			{
				adValoremDuties.Add(new CombinedDuty(NewTempDuty(RateTypes.Codes.AdValorem, rate, ZString.Empty, CombinedDuty.Type.Empty)));
			}
			else
			{
				var duty = AddNewOrReuseOldDutyOrTax(DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, ZString.Empty, RateTypes.Codes.AdValorem, ZString.Empty, false);
				duty.C1_Rate = rate;
				DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(duty, customsValue.GetValueOrDefault());
				duty.AmountDescription = data.CalculationMethod == CalculationMethods.Codes.RateDescCalcOnly ?
					DutyAndTaxAmountDescriptor.GetRateString(duty) : Res.GetString("adec5ca4-ab8d-46e1-8cbd-940a243e9757", "General Classification duty rate.") + " " + duty.AmountDescription;
			}
		}

		#region AddDutyRatesFromRateLines

		void AddDutyRatesFromRateLines(CACRateLineCollection rateLines, ZString unitOfMeasure, CombinedDuty.Type dutyType)
		{
			var combinedDuty = new CombinedDuty(dutyType);

			foreach (CACRateLine rateLine in rateLines)
			{
				if (rateLine.ZR_DutyRateMin > ZDecimal.Zero)
				{
					combinedDuty.Min = NewTempDuty(rateLine.ZR_DutyRateType, rateLine.ZR_DutyRateMin, unitOfMeasure, dutyType);
				}
				else if (rateLine.ZR_DutyRateMax > ZDecimal.Zero)
				{
					combinedDuty.Max = NewTempDuty(rateLine.ZR_DutyRateType, rateLine.ZR_DutyRateMax, unitOfMeasure, dutyType);
				}
				else
				{
					combinedDuty.Regulars.Add(NewTempDuty(rateLine.ZR_DutyRateType, rateLine.ZR_DutyRateRegular, unitOfMeasure, dutyType));
				}
			}

			if (combinedDuty.Regulars.Count > 0)
			{
				if (data.CalculationMethod == CalculationMethods.Codes.RateDescCalcOnly)
				{
					var duty = AddNewOrReuseOldDutyOrTax(DutyAndTaxTypes.Codes.CustomsDuty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, true);
					duty.AmountDescription = DutyAndTaxAmountDescriptor.GetCombinedShortDutyDescription(combinedDuty);
				}
				else
				{
					if (calculatingCustomsValueForDuty && combinedDuty.HasAdValorem)
					{
						adValoremDuties.Add(combinedDuty);
					}
					else
					{
						var description = DutyAndTaxAmountDescriptor.GetCombinedDutyDescription(combinedDuty);
						var regularTotalAmount = combinedDuty.Regulars.Sum(duty => duty.Amount);

						if (combinedDuty.Min != null && combinedDuty.Min.Amount > regularTotalAmount)
						{
							AddApplicableDuty(combinedDuty.Min, description);
						}
						else if (combinedDuty.Max != null && combinedDuty.Max.Amount < regularTotalAmount)
						{
							AddApplicableDuty(combinedDuty.Max, description);
						}
						else
						{
							combinedDuty.Regulars.ForEach(duty => AddApplicableDuty(duty, description));
						}
					}
				}
			}
		}

		void AddApplicableDuty(IDutyAndTaxDataForCalculation tempDuty, ZString description)
		{
			var exemptCode = DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(tempDuty.TaxType) ? tempDuty.ExemptCode : ZString.Empty;
			var duty = AddNewOrReuseOldDutyOrTax(tempDuty.TaxType, ZString.Empty, exemptCode, tempDuty.RateType, tempDuty.UnitOfMeasure, true);
			duty.C1_Rate = tempDuty.Rate;
			duty.C1_Amount = tempDuty.Amount;
			duty.C1_ValueForCalculation = tempDuty.ValueForCalculation;
			duty.C1_DutyType = tempDuty.DutyType;
			duty.C1_Override = tempDuty.Override;

			if (duty.Parent != null)
			{
				duty.AmountDescription = description;
			}
		}

		#region NewTempDuty

		IDutyAndTaxDataForCalculation NewTempDuty(ZString rateType, ZDecimal rate, ZString unitOfMeasure, CombinedDuty.Type dutyType)
		{
			var duty = new TempDutyAndTax(dutiesAndTaxes.DutyAndTaxDataMaster);
			duty.RateType = rateType;
			duty.Rate = rate;
			duty.UnitOfMeasure = unitOfMeasure;
			duty.DutyType = CombinedDuty.GetDutyTypeCode(dutyType);
			DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(duty, customsValue.GetValueOrDefault());
			return duty;
		}

		BusinessObjectFactory NewTempFactory()
		{
			return factory.GetCachedReadOnlyFactory();
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region PopulateNormalTaxes

		void PopulateNormalTaxes(CACClassHeader classHeader)
		{
			PopulateTaxRate(NormalGSTRefNum, DutyAndTaxTypes.Codes.GST, data.DefaultGSTStatusCode);
		}

		public const string NormalGSTRefNum = "001";
		public const decimal NormalGSTRate = 39m;

		void PopulateTaxRate(ZString refNumber, ZString taxType, ZString defaultCode, bool addWithBlankRate = false, bool updateC1Code = false)
		{
			var tax = calculatingCustomsValueForDuty ? null : GetDutiesOrTaxesByType(taxType).FirstOrDefault();
			var code = addWithBlankRate ? ZString.Empty : refNumber;
			if (tax == null && !refNumber.IsEmpty)
			{
				tax = AddNewOrReuseOldDutyOrTax(taxType, code, defaultCode, ZString.Empty, ZString.Empty);
			}

			if (tax != null)
			{
				if (!defaultCode.IsEmpty && !tax.C1_Override)
				{
					tax.C1_ExemptCode = defaultCode;
				}

				if (updateC1Code && !code.IsEmpty && taxType == DutyAndTaxTypes.Codes.ExciseTax)
				{
					tax.C1_Code = code;
				}
			}

			if (tax != null)
			{
				UpdateTaxRate(tax);
			}
		}

		void UpdateTaxRate(DutyAndTax tax)
		{
			if (!tax.C1_Override)
			{
				var shouldUpdateOtherField = false;
				if (!tax.IsGST)
				{
					shouldUpdateOtherField = UpdateTaxAccordingToCACTaxRate(tax);
				}
				else
				{
					shouldUpdateOtherField = UpdateTaxAccordingToSRDB(tax);
				}

				if (shouldUpdateOtherField)
				{
					if (tax.C1_ExemptCode.IsEmpty)
					{
						DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(tax, GetValueForDutyAndTax(tax.C1_TaxType));
					}

					if (data.CalculationMethod == CalculationMethods.Codes.RateDescCalcOnly)
					{
						if (!tax.C1_ExemptCode.IsEmpty)
						{
							tax.C1_RateType = RateTypes.Codes.Exempt;
						}

						tax.AmountDescription = DutyAndTaxAmountDescriptor.GetShortDutyDescription(tax);
					}
					else
					{
						if (data.CalculationMethod == CalculationMethods.Codes.RegularRemission && data.AuthorityNumber == TaxRemittedOICNumber)
						{
							tax.C1_Amount = ZDecimal.Zero;
						}
						tax.AmountDescription = ZString.Empty;
					}
				}
				else
				{
					tax.C1_RateType = ZString.Empty;
					tax.C1_Rate = ZDecimal.Zero;
					tax.C1_UnitOfMeasure = ZString.Empty;
					tax.C1_Amount = ZDecimal.Zero;
					tax.AmountDescription = ZString.Empty;
				}
			}
		}

		bool UpdateTaxAccordingToCACTaxRate(DutyAndTax tax)
		{
			var taxRate = CACTaxRate.Load(factory, tax.C1_Code, tax.C1_TaxType, data.EffectiveDutyDate);
			if (taxRate != null)
			{
				tax.C1_RateType = taxRate.ZH_RateType;
				tax.C1_Rate = taxRate.ZH_Rate;
				tax.C1_UnitOfMeasure = taxRate.ZH_UnitOfMeasure;
				return true;
			}
			else
			{
				return false;
			}
		}

		bool UpdateTaxAccordingToSRDB(DutyAndTax tax)
		{
			var taxRate = tax.CAGSTRateCode;
			if (taxRate != null)
			{
				var rateType = taxRate.Attributes.GetAttributeValue(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_RateType);
				ZDecimal.TryParse(taxRate.Attributes.GetAttributeValue(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.CAGST_Rate), out var rate);

				tax.C1_RateType = rateType;
				tax.C1_Rate = rate;
				return true;
			}
			else
			{
				return false;
			}
		}

		void UpdateExciseTax(DutyAndTax tax)
		{
			var tariffView = TariffView;
			if (tariffView != null)
			{
				var taxRate = GetRateViewWithRateCode(tariffView, tax.C1_TaxType, tax.C1_Code, data.EffectiveDutyDate);
				if (taxRate != null)
				{
					ParseRateFormula(taxRate.ZZ2_RateFormula, out var rate, out var rateType, out var unitOfMeasure);
					tax.C1_RateType = rateType;
					tax.C1_Rate = rate;
					tax.C1_UnitOfMeasure = unitOfMeasure;
					tax.C1_Override = true;
					DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(tax, GetValueForDutyAndTax(tax.C1_TaxType));
				}
			}
		}

		RateView GetRateViewWithRateCode(TariffView tariffView, ZString rateType, ZString rateCode, ZDateTime effectiveDate)
		{
			var taxRate = tariffView.Rates.FirstOrDefault(d => d.ZZ2_StartDate <= effectiveDate && d.ZZ2_EndDate >= effectiveDate && d.ZZ2_ZZR_RateTypeCode == rateType && d.RateCode == rateCode);
			return taxRate;
		}

		ZDecimal GetValueForDutyAndTax(ZString taxType)
		{
			return taxType == DutyAndTaxTypes.Codes.CustomsDuty || DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(taxType)
				? customsValue.GetValueOrDefault()
				: taxType == DutyAndTaxTypes.Codes.ExciseTax ? NormalDutyPaidValue : NormalValueForTax;
		}

		#endregion

		#region Populate Casual Import Provincial Taxes
		void PopulateCasualImportProvincialTaxesIfRequired()
		{
			var casualImportTaxData = data as ICasualImportTaxData;

			if (casualImportTaxData != null && casualImportTaxData.IsEffectiveCasualImport)
			{
				var provinceOfDest = casualImportTaxData.EffectiveCasualImportDestinationProvince;
				var provinceOfClearance = casualImportTaxData.EffectiveCasualImportClearanceProvince;
				var commodity = casualImportTaxData.EffectiveCasualImportCommodity;
				var dateForDutyRate = casualImportTaxData.EffectiveDate;

				var exemptCode = RemoveExistingNotOverridenLinesAndCacheExcemptionCodeByTaxType(DutyAndTaxTypes.Codes.CPT);
				RemoveExistingNotOverridenLinesAndCacheExcemptionCodeByTaxType(DutyAndTaxTypes.Codes.CTA);

				CACCasualImpRates hstRates = null;
				var isHSTApplicable = ZBool.False;
				if (!casualImportTaxData.IsExempt)
				{
					hstRates = CACCasualImpRates.Load(factory, provinceOfDest, CasualImportConstants.CasualImpRatesCommodityType.CommodityTypeHSTCode);
					isHSTApplicable = hstRates != null;
				}
				var overridenCptTax = calculatingDutiesAndTaxes.FirstOrDefault(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT && dutyOrTax.C1_Override);

				var isCADEnabled = data is JobComInvoiceLine invoiceLine && (invoiceLine.Declaration?.IsCADEnabled ?? false);
				if (isHSTApplicable && !casualImportTaxData.IsExempt)
				{
					if (overridenCptTax != null)
					{
						HSTTaxAmount = overridenCptTax.C1_Amount;
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(exemptCode))
						{
							PopulateCasualImportExemptLine(exemptCode, isHSTApplicable);
						}
						else
						{
							if (isCADEnabled && casualImportTaxData.IsEffectiveCasualImport)
							{
								hstRates.ResetCasualImpRateSpecList();
								var gstRate = GSTaxes.FirstOrDefault(x => x.C1_TaxType == DutyAndTaxTypes.Codes.GST && x.C1_ExemptCode != ExciseTaxExemptionCodes.Codes.C99 && !x.C1_Override);
								var cptRate = hstRates.CasualImpRateSpecList.FirstOrDefault(x => x.RateType == (gstRate?.C1_RateType ?? ZString.Empty));
								if (gstRate != null && cptRate != null && cptRate.RegularRate >= gstRate.C1_Rate)
								{
									cptRate.RegularRate -= gstRate.C1_Rate;
								}
							}
							PopulateCasualImportHSTTax(hstRates, exemptCode, provinceOfDest, commodity);
						}
					}
				}

				if (isHSTApplicable && !HSTTaxAmount.IsEmpty && !isCADEnabled)
				{
					UpdateGSTExempt(ExciseTaxExemptionCodes.Codes.C99);
				}

				PopulateCasualImportCTATax(provinceOfClearance, commodity);

				if (!isHSTApplicable && !casualImportTaxData.IsExempt)
				{
					if (overridenCptTax != null)
					{
						HSTTaxAmount = overridenCptTax.C1_Amount;
					}
					else
					{
						if (!string.IsNullOrWhiteSpace(exemptCode))
						{
							PopulateCasualImportExemptLine(exemptCode, isHSTApplicable);
						}
						else
						{
							PopulateCasualImportPSTTax(provinceOfDest, commodity, dateForDutyRate);
						}
					}
				}
			}
		}

		void PopulateCasualImportExemptLine(ZString exemptCode, ZBool isHST)
		{
			var tax = AddNewOrReuseOldDutyOrTax(DutyAndTaxTypes.Codes.CPT, ZString.Empty, exemptCode, ZString.Empty, ZString.Empty);
			tax.C1_Amount = ZDecimal.Zero;
			if (isHST)
			{
				HSTTaxAmount = ZDecimal.Zero;
			}
			else
			{
				PSTTaxAmount = ZDecimal.Zero;
			}
		}

		void PopulateCasualImportHSTTax(CACCasualImpRates hstRates, ZString exemptCode, ZString provinceOfDest, ZString commodity)
		{
			if (string.IsNullOrWhiteSpace(exemptCode))
			{
				HSTTaxAmount = PopulateCasualImportProvincialTax(hstRates, DutyAndTaxTypes.Codes.CPT);
			}
			else
			{
				var tax = AddNewOrReuseOldDutyOrTax(DutyAndTaxTypes.Codes.CPT, ZString.Empty, exemptCode, ZString.Empty, ZString.Empty);
				HSTTaxAmount = tax.C1_Amount = ZDecimal.Zero;
			}
		}

		void PopulateCasualImportCTATax(ZString provinceOfClearance, ZString commodity)
		{
			var ctaTax = calculatingDutiesAndTaxes.FirstOrDefault(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CTA && dutyOrTax.C1_Override);
			if (ctaTax != null)
			{
				CTATaxAmount = ctaTax.C1_Amount;
			}
			else
			{
				var ctaRates = CACCasualImpRates.Load(factory, provinceOfClearance, commodity);

				if (ctaRates != null && ctaRates.IR_RateType1 == RateTypes.Codes.AcceptX)
				{
					var taxInfo = new TaxInfo(DutyAndTaxTypes.Codes.CTA, RateTypes.Codes.AcceptX, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero);
					BindTaxInfoIntoTax(taxInfo);
					CTATaxAmount = taxInfo.Amount;
				}
				else
				{
					CTATaxAmount = PopulateCasualImportProvincialTax(ctaRates, DutyAndTaxTypes.Codes.CTA);
				}
			}
		}

		void PopulateCasualImportPSTTax(ZString provinceOfDest, ZString commodity, ZDate dateForDutyRate)
		{
			var pstTax = calculatingDutiesAndTaxes.FirstOrDefault(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT && dutyOrTax.C1_Override);
			if (pstTax != null)
			{
				PSTTaxAmount = pstTax.C1_Amount;
				UpdateGSTExempt(ZString.Empty);
			}
			else
			{
				var pstRates = CACCasualImpRates.LoadPSTRates(factory, provinceOfDest, commodity, dateForDutyRate);
				PSTTaxAmount = PopulateCasualImportProvincialTax(pstRates, DutyAndTaxTypes.Codes.CPT);
				if (pstRates != null && !PSTTaxAmount.IsEmpty)
				{
					UpdateGSTExempt(ZString.Empty);
				}
			}
		}

		ZString RemoveExistingNotOverridenLinesAndCacheExcemptionCodeByTaxType(ZString taxType)
		{
			var exemptCode = ZString.Empty;
			var dutiesAndTaxesToRemove = calculatingDutiesAndTaxes.Where(dutyOrTax => dutyOrTax.C1_TaxType == taxType && !dutyOrTax.C1_Override).ToList();
			var dutiesAndTaxesContainExemptCode = dutiesAndTaxesToRemove.FirstOrDefault(dutyOrTax => !string.IsNullOrWhiteSpace(dutyOrTax.C1_ExemptCode));
			if (dutiesAndTaxesContainExemptCode != null)
			{
				exemptCode = dutiesAndTaxesContainExemptCode.C1_ExemptCode;
			}

			dutiesAndTaxesToRemove.ForEach(tax => DeleteDutyAndTaxOrSetAsOldRow(tax));
			return exemptCode;
		}

		ZDecimal PopulateCasualImportProvincialTax(CACCasualImpRates rates, ZString taxType)
		{
			var sumAmount = ZDecimal.Zero;

			if (rates != null)
			{
				int rateSpecListCount = rates.CasualImpRateSpecList.Count;
				TripleTaxInfo[] tripleTaxInfoArray = new TripleTaxInfo[rateSpecListCount];
				TaxInfo emptyTaxInfo = new TaxInfo(ZString.Empty, ZString.Empty, ZDecimal.Zero, ZDecimal.Zero, ZString.Empty, ZDecimal.Zero);

				for (int i = 0; i < rateSpecListCount; i++)
				{
					var casualImpRateSpec = rates.CasualImpRateSpecList[i];
					if (casualImpRateSpec.RateType == RateTypes.Codes.AdValorem)
					{
						tripleTaxInfoArray[i] = CalculateCasualImportProvincialTaxByType(casualImpRateSpec, taxType, rates.IR_AdValoremBasis);
					}
					else if (casualImpRateSpec.RateType == RateTypes.Codes.Specific)
					{
						tripleTaxInfoArray[i] = CalculateCasualImportProvincialTaxByType(casualImpRateSpec, taxType, casualImpRateSpec.Units);
					}
				}

				if (!tripleTaxInfoArray.Any(tripleTaxInfo => tripleTaxInfo != null && tripleTaxInfo.IsValid))
				{
					var applicableTaxInfo = CalculateApplicableTaxInfo(emptyTaxInfo, tripleTaxInfoArray);
					if (applicableTaxInfo != null && !applicableTaxInfo.Amount.IsEmpty)
					{
						BindTaxInfoIntoTax(applicableTaxInfo);
						sumAmount += applicableTaxInfo.Amount;
					}
				}
				else
				{
					for (int i = 0; i < rateSpecListCount; i++)
					{
						if (tripleTaxInfoArray[i] != null && tripleTaxInfoArray[i].IsValid)
						{
							var applicableTaxInfo = CalculateApplicableTaxInfo(tripleTaxInfoArray[i].RegularAmountTax, tripleTaxInfoArray);
							if (applicableTaxInfo != null && !applicableTaxInfo.Amount.IsEmpty)
							{
								BindTaxInfoIntoTax(applicableTaxInfo);
								sumAmount += applicableTaxInfo.Amount;
							}
						}
					}
				}
			}

			return sumAmount;
		}

		TripleTaxInfo CalculateCasualImportProvincialTaxByType(CasualImpRateSpec casualImpRateSpec, ZString taxType, ZString rateBasisInputParameter)
		{
			TripleTaxInfo tripleTaxInfo = null;

			if (!calculatingDutiesAndTaxes.Any(dutyOrTax => dutyOrTax.C1_TaxType == taxType && dutyOrTax.C1_RateType == casualImpRateSpec.RateType && dutyOrTax.C1_Override))
			{
				tripleTaxInfo = new TripleTaxInfo();
				var rateBasis = CalCulateRateBasis(casualImpRateSpec.RateType, rateBasisInputParameter);
				tripleTaxInfo.RegularAmountTax = CreateTaxInfo(taxType, casualImpRateSpec, rateBasis, casualImpRateSpec.RegularRate);
				tripleTaxInfo.MinimumAmountTax = CreateTaxInfo(taxType, casualImpRateSpec, rateBasis, casualImpRateSpec.MinimumRate);
				tripleTaxInfo.MaximumAmountTax = CreateTaxInfo(taxType, casualImpRateSpec, rateBasis, casualImpRateSpec.MaximumRate);
			}

			return tripleTaxInfo;
		}

		TaxInfo CreateTaxInfo(ZString taxType, CasualImpRateSpec casualImpRateSpec, ZDecimal rateBasis, ZDecimal taxRate)
		{
			return new TaxInfo(taxType,
				casualImpRateSpec.RateType,
				taxRate,
				casualImpRateSpec.RateType == RateTypes.Codes.AdValorem ? ZDecimal.Zero : rateBasis,
				casualImpRateSpec.Units,
				rateBasis * NormalizeRateByType(casualImpRateSpec.RateType, taxRate));
		}

		ZDecimal NormalizeRateByType(ZString rateType, ZDecimal rate)
		{
			return rateType.Trim() == RateTypes.Codes.AdValorem ? new ZDecimal(rate / 100) : rate;
		}

		#region Inner Class Definition

		class TaxInfo
		{
			public TaxInfo(ZString taxType, ZString rateType, ZDecimal rate, ZDecimal quantity, ZString uom, ZDecimal amount)
			{
				this.TaxType = taxType;
				this.RateType = rateType;
				this.Rate = rate;
				this.Quantity = quantity;
				this.UOM = uom;
				this.Amount = amount;
			}

			public ZString TaxType { get; set; }
			public ZString RateType { get; set; }
			public ZDecimal Rate { get; set; }
			public ZDecimal Quantity { get; set; }
			public ZString UOM { get; set; }
			public ZDecimal Amount { get; set; }
		}

		class TripleTaxInfo
		{
			public TaxInfo RegularAmountTax { get; set; }
			public TaxInfo MinimumAmountTax { get; set; }
			public TaxInfo MaximumAmountTax { get; set; }

			public ZBool IsValid
			{
				get
				{
					return this.RegularAmountTax != null && !this.RegularAmountTax.Rate.IsEmpty;
				}
			}
		}

		#endregion

		#region Calculate Rate Basis

		ZDecimal CalCulateRateBasis(ZString rateType, ZString inputParameter)
		{
			switch (rateType)
			{
				case RateTypes.Codes.AdValorem:
					return CalculateCasualImportAdValoremBasisValue(inputParameter);
				case RateTypes.Codes.Specific:
					return FindCasualImportQuantity(inputParameter);
				default:
					return ZDecimal.Zero;
			}
		}

		ZDecimal CalculateCasualImportAdValoremBasisValue(ZString adValoremBasis)
		{
			var result = ZDecimal.Zero;

			if (!string.IsNullOrWhiteSpace(adValoremBasis))
			{
				switch (adValoremBasis.Trim())
				{
					case AdValoremBasisTypes.Codes.VFT:
						result = NormalValueForTax;
						break;
					case AdValoremBasisTypes.Codes.HST:
						result = NormalValueForTax + HSTTaxAmount;
						break;
					case AdValoremBasisTypes.Codes.GST:
						var gst = GSTaxes.FirstOrDefault();
						result = NormalValueForTax + (gst == null ? ZDecimal.Zero : gst.C1_Amount);
						break;
					case AdValoremBasisTypes.Codes.PST:
						result = NormalValueForTax + CTATaxAmount;
						break;
				}
			}
			return result;
		}

		ZDecimal FindCasualImportQuantity(ZString unit)
		{
			var result = ZDecimal.Zero;

			if (!string.IsNullOrWhiteSpace(unit))
			{
				switch (unit.Trim())
				{
					case CustomsUnitOfMeasureList.Codes.Gram:
						result = DutyAndTaxUnitConverter.GetConvertedQuantity(data.CustomsQuantity, data.CustomsUnits, unit);
						if (result.IsEmpty)
						{
							result = DutyAndTaxUnitConverter.GetConvertedQuantity(data.CustomsQuantity2, data.CustomsUnits2, unit);
							if (result.IsEmpty)
							{
								result = DutyAndTaxUnitConverter.GetConvertedQuantity(data.CustomsQuantity3, data.CustomsUnits3, unit);
							}
						}
						break;
					case CustomsUnitOfMeasureList.Codes.Number:
						result = FindCompatibleQuantityFrom3Quantities(UnitConverter.ConvertSafeToNumber);
						break;
					case CustomsUnitOfMeasureList.Codes.Litre:
						result = FindCompatibleQuantityFrom3Quantities(UnitConverter.ConvertSafeToLitre);
						break;
					case Core.Constants.Weight.Ounces:
						result = FindCompatibleQuantityFrom3Quantities(UnitConverter.ConvertSafeToOunce);
						break;
				}
			}

			return result;
		}

		delegate ZDecimal convertSafeDelegate(ZDecimal sourceValue, ZString sourceUnitCode);

		ZDecimal FindCompatibleQuantityFrom3Quantities(convertSafeDelegate convertSafeMethod)
		{
			var result = convertSafeMethod(data.CustomsQuantity, data.CustomsUnits);
			if (result.IsEmpty)
			{
				result = convertSafeMethod(data.CustomsQuantity2, data.CustomsUnits2);
				if (result.IsEmpty)
				{
					result = convertSafeMethod(data.CustomsQuantity3, data.CustomsUnits3);
				}
			}
			return result;
		}

		#endregion

		#region BindApplicableTaxInfoIntoTax

		TaxInfo CalculateApplicableTaxInfo(TaxInfo regularTaxInfo, TripleTaxInfo[] tripleTaxInfoArray)
		{
			TaxInfo applicableTaxInfo = regularTaxInfo;

			foreach (var tripleTaxInfo in tripleTaxInfoArray)
			{
				if (tripleTaxInfo != null)
				{
					if (tripleTaxInfo.MaximumAmountTax != null && !tripleTaxInfo.MaximumAmountTax.Amount.IsEmpty && tripleTaxInfo.MaximumAmountTax.Amount < applicableTaxInfo.Amount ||
						applicableTaxInfo.Rate.IsEmpty && !tripleTaxInfo.MaximumAmountTax.Rate.IsEmpty)
					{
						applicableTaxInfo = tripleTaxInfo.MaximumAmountTax;
					}
					else if (tripleTaxInfo.MinimumAmountTax != null && tripleTaxInfo.MinimumAmountTax.Amount > applicableTaxInfo.Amount ||
						applicableTaxInfo.Rate.IsEmpty && !tripleTaxInfo.MinimumAmountTax.Rate.IsEmpty)
					{
						applicableTaxInfo = tripleTaxInfo.MinimumAmountTax;
					}
				}
			}

			return applicableTaxInfo;
		}

		void BindTaxInfoIntoTax(TaxInfo taxInfo)
		{
			DutyAndTax tax = null;
			if (taxInfo != null)
			{
				tax = AddNewOrReuseOldDutyOrTax(taxInfo.TaxType, ZString.Empty, ZString.Empty, taxInfo.RateType, taxInfo.UOM);
				tax.C1_Rate = taxInfo.Rate;
				tax.Quantity = taxInfo.Quantity;
				tax.C1_Amount = Utilities.Round(taxInfo.Amount, 2);
			}
		}

		#endregion

		void UpdateGSTExempt(ZString exemptCode)
		{
			if (calculatingDutiesAndTaxes.Any(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CPT && dutyOrTax.C1_Amount > ZDecimal.Zero))
			{
				var gstLine = GSTaxes.FirstOrDefault();
				if (gstLine != null)
				{
					gstLine.C1_ExemptCode = exemptCode;
				}
				if (exemptCode.IsEmpty)
				{
					PopulateTaxRate(NormalGSTRefNum, DutyAndTaxTypes.Codes.GST, data.DefaultGSTStatusCode);
				}
			}
		}

		#endregion

		#region RecalculateRemissions

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void RecalculateRemissions()
		{
			var notOverriddenDutiesOrTaxes = calculatingDutiesAndTaxes.Where(dutyOrTax => !dutyOrTax.C1_Override).ToList();
			switch (data.CalculationMethod)
			{
				case CalculationMethods.Codes.OneSixtiethRemission:
					notOverriddenDutiesOrTaxes.ForEach(dutyOrTax => SetAmountForRemission(dutyOrTax, Utilities.Round(dutyOrTax.C1_Amount * data.MonthlyTimeLimit / 60, 2)));
					break;
				case CalculationMethods.Codes.OneOneTwentiethRemission:
					notOverriddenDutiesOrTaxes.ForEach(dutyOrTax => SetAmountForRemission(dutyOrTax, Utilities.Round(dutyOrTax.C1_Amount * data.MonthlyTimeLimit / 120, 2)));
					break;
				case CalculationMethods.Codes.RepairsRemission:
				case CalculationMethods.Codes.WarrantyRepairsRemission:
				case CalculationMethods.Codes.SoftwareRemission:
					notOverriddenDutiesOrTaxes.ForEach(dutyOrTax => dutyOrTax.C1_Amount = ZDecimal.Zero);
					ClearCachedValues();
					break;
				case CalculationMethods.Codes.DutyDeferral:
					notOverriddenDutiesOrTaxes.Where(x => x.C1_TaxType != DutyAndTaxTypes.Codes.GST).ForEach(x => SetAmountForRemission(x, ZDecimal.Zero));
					break;
				case CalculationMethods.Codes.GiftsUpTo60:
					RecalculateRemissionGiftsUpTo60(notOverriddenDutiesOrTaxes);
					break;
				case CalculationMethods.Codes.SpiritsRemission:
					var exciseDuty = notOverriddenDutiesOrTaxes.FirstOrDefault(x => x.IsExciseTax);
					if (exciseDuty != null)
					{
						exciseDuty.C1_OriginalAmount = exciseDuty.C1_Amount;
						exciseDuty.C1_ExemptCode = ExciseTaxExemptionCodes.Codes.C94;
					}
					break;
				case CalculationMethods.Codes.RegularRemission:
					RecalculateRegularRemissions();
					break;
				default:
					notOverriddenDutiesOrTaxes.ForEach(x => x.C1_OriginalAmount = x.C1_Amount);
					break;
			}
		}

		void ResetDutyAmountsForRegularRemission()
		{
			if (data.CalculationMethod == CalculationMethods.Codes.RegularRemission)
			{
				if (!data.RulingConfigs.Any())
				{
					foreach (var duty in Duties)
					{
						SetAmountForRemission(duty, ZDecimal.Zero);
					}
				}
				else
				{
					ApplyApplicableTaxesFromRemissionConfigs(SIMADuties, RefCusRulingConfigCategories.Codes.SIM);
					ApplyApplicableTaxesFromRemissionConfigs(ExciseTaxes, RefCusRulingConfigCategories.Codes.EXC);
					ApplyApplicableTaxesFromRemissionConfigs(Duties, RefCusRulingConfigCategories.Codes.DTY);
					ApplyApplicableTaxesFromRemissionConfigs(Duties, RefCusRulingConfigCategories.Codes.EXD);
				}
			}
		}

		void RecalculateRemissionGiftsUpTo60(List<DutyAndTax> dutyAndTaxes)
		{
			dutyAndTaxes.Sort((x, y) => GetTaxRank(x.C1_TaxType).CompareTo(GetTaxRank(y.C1_TaxType)));
			ClearCachedValues();

			foreach (var dutyAndTax in dutyAndTaxes)
			{
				dutyAndTax.C1_OriginalAmount = dutyAndTax.C1_Amount;
				DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(dutyAndTax, Math.Max(GetValueForDutyAndTax(dutyAndTax.C1_TaxType) - 60, 0));
			}
		}

		int GetTaxRank(string taxType)
		{
			switch (taxType)
			{
				case DutyAndTaxTypes.Codes.CustomsDuty:
					return 0;
				case DutyAndTaxTypes.Codes.ExciseTax:
					return 2;
				case DutyAndTaxTypes.Codes.GST:
					return 3;
				default:
					return DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(taxType) ? 1 : 4;
			}
		}

		void RecalculateRegularRemissions()
		{
			var rulingConfigs = data.RulingConfigs;
			if (rulingConfigs.Any())
			{
				ApplyApplicableTaxesFromRemissionConfigs(Duties, RefCusRulingConfigCategories.Codes.DTY);
				ApplyApplicableTaxesFromRemissionConfigs(SIMADuties, RefCusRulingConfigCategories.Codes.SIM);
				ApplyApplicableTaxesFromRemissionConfigs(ExciseTaxes, RefCusRulingConfigCategories.Codes.EXC);
				ApplyApplicableTaxesFromRemissionConfigs(GSTaxes, RefCusRulingConfigCategories.Codes.GST);
				ApplyApplicableTaxesFromRemissionConfigs(Duties, RefCusRulingConfigCategories.Codes.EXD);
			}
		}

		void ApplyApplicableTaxesFromRemissionConfigs(IEnumerable<DutyAndTax> dutyAndTaxes, ZString configType)
		{
			var dutyAndTaxesForRemission = dutyAndTaxes.Where(d => !d.C1_Override).ToArray();
			if (configType == RefCusRulingConfigCategories.Codes.EXD)
			{
				dutyAndTaxesForRemission = dutyAndTaxesForRemission.Where(d => d.IsEXDDuty).ToArray();
			}

			if (dutyAndTaxesForRemission.Any())
			{
				ClearCachedValues();

				var rulingConfigs = data.RulingConfigs;
				var acceptAmountConfig = GetRulingConfig(rulingConfigs, configType, RefCusRulingConfigTypes.Codes.AcceptAmount);
				var exemptCodeConfig = GetRulingConfig(rulingConfigs, configType, RefCusRulingConfigTypes.Codes.ExemptCode);
				var noneFreeConfig = GetRulingConfig(rulingConfigs, configType, RefCusRulingConfigTypes.Codes.NoneFree);
				var maxConfig = GetRulingConfig(rulingConfigs, configType, RefCusRulingConfigTypes.Codes.Maximum);
				var minConfig = GetRulingConfig(rulingConfigs, configType, RefCusRulingConfigTypes.Codes.Minimum);

				if (acceptAmountConfig != null)
				{
					ApplyAcceptAmountFromRulingConfig(acceptAmountConfig, dutyAndTaxesForRemission);
				}
				else if (noneFreeConfig != null)
				{
					ApplyNoneFeeFromRulingConfig(dutyAndTaxesForRemission);
				}
				else if (exemptCodeConfig != null)
				{
					ApplyExemptCodeFromRulingConfig(exemptCodeConfig, dutyAndTaxesForRemission);
				}
				else
				{
					ApplyRateFromRulingConfig(dutyAndTaxesForRemission, configType);
				}

				if (maxConfig != null || minConfig != null)
				{
					ApplyMaxAndMinFromRulingConfig(maxConfig, minConfig, dutyAndTaxesForRemission);
				}
			}
		}

		void ApplyRateFromRulingConfig(IEnumerable<DutyAndTax> dutiesForRemission, ZString configType)
		{
			var rulingConfigs = data.RulingConfigs;
			var adConfig = GetRulingConfig(rulingConfigs, configType, RefCusRulingConfigTypes.Codes.AdValorem);
			var specificConfig = GetRulingConfig(rulingConfigs, configType, RefCusRulingConfigTypes.Codes.Specific);
			ZDecimal? rate = null;
			var rateType = string.Empty;
			var rulingConfigType = ZString.Empty;

			if (adConfig != null)
			{
				rate = adConfig.ZZY_Rate;
				rateType = RateTypes.Codes.AdValorem;
				rulingConfigType = adConfig.ZZY_Type;
			}
			else if (specificConfig != null)
			{
				rate = specificConfig.ZZY_Rate;
				rateType = RateTypes.Codes.Specific;
				rulingConfigType = specificConfig.ZZY_Type;
			}

			if (rate.HasValue)
			{
				var rulingConfigTypeDesc = rulingConfigType.IsEmpty ? string.Empty : RefCusRulingConfigTypes.GetUppercaseCodeList(factory).GetDescriptionFromCode(rulingConfigType.ToUpperInvariant());
				foreach (var duty in dutiesForRemission.Where(d => d.C1_RateType == rateType))
				{
					duty.C1_OriginalAmount = duty.C1_Amount;
					duty.C1_Rate = rate.Value;
					DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(duty, GetValueForDutyAndTax(duty.C1_TaxType));
					duty.AmountDescription = Res.GetString("83776874-d19f-4097-92d0-7568a7cd9053", "Remission configuration duty rate. {0}% for {1}.", rate.Value, rulingConfigTypeDesc);
				}
			}
		}

		void ApplyMaxAndMinFromRulingConfig(CusRulingConfigCombined maxConfig, CusRulingConfigCombined minConfig, IEnumerable<DutyAndTax> dutyAndTaxes)
		{
			foreach (var dutyAndTax in dutyAndTaxes)
			{
				var valueForDutyAndTax = GetValueForDutyAndTax(dutyAndTax.C1_TaxType);
				var maxAmount = GetAmountViaRemissionConfig(maxConfig, valueForDutyAndTax);
				var minAmount = GetAmountViaRemissionConfig(minConfig, valueForDutyAndTax);

				ApplyDutyForMinAndMaxRulingConfigs(minAmount, maxAmount, dutyAndTax);
			}
		}

		void ApplyDutyForMinAndMaxRulingConfigs(ZDecimal? minAmount, ZDecimal? maxAmount, DutyAndTax duty)
		{
			var hasMinAmount = minAmount != null && duty.C1_Amount < minAmount.Value;
			var hasMaxAmount = maxAmount != null && duty.C1_Amount > maxAmount.Value;

			if (hasMinAmount || hasMaxAmount)
			{
				ZDecimal amount;
				var builder = new ZStringBuilder(Res.GetString("62fb11c2-92bd-4858-b330-2912a0b21028", "{0} duty amount.", RemissionConfigurationPrefix));

				if (hasMinAmount)
				{
					amount = minAmount.Value;
					builder.Append(Res.GetString("059cdeed-6547-4ca4-bf63-108edf990633", "Not less than {0}.", amount));
				}
				else
				{
					amount = maxAmount.Value;
					builder.Append(Res.GetString("32df1c33-53ba-466c-967b-0d0f8ef01f50", "Not more than {0}.", amount));
				}

				if (!duty.AmountDescription.StartsWith(RemissionConfigurationPrefix, StringComparison.Ordinal))
				{
					duty.C1_OriginalAmount = duty.C1_Amount;
				}

				duty.C1_Amount = amount;
				duty.AmountDescription = builder.ToStringWithDelimiterBetweenAppends(DutyAndTaxAmountDescriptor.Delimiter);
			}
		}

		void ApplyAcceptAmountFromRulingConfig(CusRulingConfigCombined config, IEnumerable<DutyAndTax> dutyAndTaxes)
		{
			if (ZDecimal.TryParse(config.ZZY_Value, out var acceptAmount))
			{
				foreach (var dutyAndTax in dutyAndTaxes)
				{
					SetAmountForRemission(dutyAndTax, acceptAmount);
					dutyAndTax.AmountDescription = Res.GetString("b19d50b6-a64e-44ea-aea8-19d5b82da9fc", "{0} duty amount. {1} for Accept Amount.", RemissionConfigurationPrefix, acceptAmount);
				}
			}
		}

		void ApplyExemptCodeFromRulingConfig(CusRulingConfigCombined config, IEnumerable<DutyAndTax> dutyAndTaxes)
		{
			foreach (var dutyAndTax in dutyAndTaxes)
			{
				dutyAndTax.C1_OriginalAmount = dutyAndTax.C1_Amount;
				dutyAndTax.C1_ExemptCode = config.ZZY_Value.Left(CADutyAndTaxAddInfo.Schema.C1_ExemptCodeMaxLength);
				dutyAndTax.C1_Amount = ZDecimal.Zero;
				dutyAndTax.AmountDescription = Res.GetString("f9cb1184-d593-4608-a09f-e71353be6b82", "{0} duty amount. 0 for Exempt Code: {1}.", RemissionConfigurationPrefix, config.ZZY_Value);
			}
		}

		void ApplyNoneFeeFromRulingConfig(IEnumerable<DutyAndTax> dutyAndTaxes)
		{
			foreach (var dutyAndTax in dutyAndTaxes)
			{
				SetAmountForRemission(dutyAndTax, ZDecimal.Zero);
				dutyAndTax.AmountDescription = Res.GetString("5a95df48-15a0-46e5-b6d2-e6343fd788c1", "{0} duty amount. 0 for None/Fee.", RemissionConfigurationPrefix);
			}
		}

		ZDecimal? GetAmountViaRemissionConfig(CusRulingConfigCombined rulingConfig, ZDecimal? originalValue)
		{
			if (rulingConfig != null && originalValue.HasValue)
			{
				if (rulingConfig.ZZY_Rate > 0)
				{
					return Utilities.Round(originalValue.Value * rulingConfig.ZZY_Rate / 100m, 2);
				}
				if (ZDecimal.TryParse(rulingConfig.ZZY_Value, out var amount))
				{
					return amount;
				}
			}

			return null;
		}

		CusRulingConfigCombined GetRulingConfig(IEnumerable<CusRulingConfigCombined> rulingConfigs, string category, string type)
		{
			return rulingConfigs.FirstOrDefault(x => x.ZZY_Category == category && x.ZZY_Type.EqualsIgnoringCase(type));
		}

		void SetAmountForRemission(DutyAndTax dutyAndTax, ZDecimal newAmount)
		{
			dutyAndTax.C1_OriginalAmount = dutyAndTax.C1_Amount;
			dutyAndTax.C1_Amount = newAmount;
		}

		const string RemissionConfigurationPrefix = "Remission configuration";
		public const string TaxRemittedOICNumber = "85-2955";
		public const string TaxRemittedOICNumber1 = "85-2955-1";
		public const string TaxRemittedOICNumber2 = "85-2955-2";
		public const string TaxRemittedOICNumber3 = "85-2955-3";
		public const string A99TariffCode0017 = "0017";

		public void ClearCachedValues()
		{
			dutiesTotalAmount = null;
			exciseTaxesTotalAmount = null;
			gstTaxesTotalAmount = null;
			normalValueForTax = null;
			calculatedValueForTax = null;
		}

		#endregion

		#endregion

		#region UpdateDutiesAmountDescriptions

		internal void UpdateDutiesAmountDescriptions()
		{
			if(!IsCaculatingDutyAndTax)
			{
				using (var calcMarker = new CalculatingMarker(this))
				{
					if (Duties.Any() && !Duties.First().C1_Override)
					{
						var tempDuties = new DutyAndTaxCollection(NewTempFactory(), dutiesAndTaxes.DutyAndTaxDataMaster);
						var tempManager = new DutyAndTaxManager(tempDuties, data) { customsValue = data.CustomsValue };
						using (var tempCalcMarker = new CalculatingMarker(tempManager))
						{
							tempManager.PopulateNormalDutiesIfRequired();
							tempManager.RecalculateRemissions();

							foreach (var duty in Duties)
							{
								UpdateAmountDescription(duty, tempDuties);
							}
						}
					}
				}
			}
		}

		static void UpdateAmountDescription(DutyAndTax duty, DutyAndTaxCollection tempDuties)
		{
			Func<DutyAndTax, bool> func =
				duty1 => duty.C1_RateType == duty1.C1_RateType
						 && duty.C1_Rate == duty1.C1_Rate
						 && duty.C1_UnitOfMeasure == duty1.C1_UnitOfMeasure
						 && duty.C1_Amount == duty1.C1_Amount;

			var tempDuty = tempDuties.FirstOrDefault(func);
			if (tempDuty != null)
			{
				using (duty.SuspendSettingHasChanges())
				{
					duty.AmountDescription = tempDuty.AmountDescription;
					tempDuties.Delete(tempDuty);
				}
			}
		}

		#endregion

		#region Validation

		#region ValidateDetailsWereFound

		internal void ValidateDetailsWereFound(ZPropertyInfo notificationInfo)
		{
			if (IsCalculated)
			{
				if (!tariffRateFoundLastTime && !calculatingDutiesAndTaxes.Any(dutyOrTax => dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty && dutyOrTax.C1_Override))
				{
					AddNoDetailsWereFoundMessageError(notificationInfo, Res.GetString("de335b5c-bb5f-4a00-9f56-96e10b6a700c", "Tariff"));
				}

				if (!classHeaderFoundLastTime)
				{
					AddNoDetailsWereFoundMessageError(notificationInfo, Res.GetString("476a6ceb-8046-4e04-b0ee-5c2477639dcc", "Classification"));
				}

				if (data.CalculationMethod == CalculationMethods.Codes.DeliveredDutyPaid && data.Declaration != null)
				{
					if (customsValueInvalid || !isCalculatedDDPValueValid())
					{
						notificationInfo.AddError(Res.GetString("1157544F-17A6-4365-85D2-0F24F9834AC8", "The system was unable to calculate the DDP Customs Value. Please run Brokerage->Perform Apportionment->, if this does not fix the problem then override and calculate manually."));
					}
				}
			}
		}

		bool isCalculatedDDPValueValid()
		{
			ZDecimal originalValueWithDutyAndTax;

			using (data.Declaration.SuspendMarkApportionmentDirty())
			{
				var saveCalculatedCustomsValue = customsValue;
				try
				{
					customsValue = null;
					originalValueWithDutyAndTax = GetCustomsValueForDuty();
				}
				finally
				{
					customsValue = saveCalculatedCustomsValue;
				}
			}

			return Math.Abs(originalValueWithDutyAndTax - data.CustomsValue - calculatingDutiesAndTaxes.Where(d => !data.DDPDeductDutyOnly || !NeedToBeDeductedWhenDDPDeductDutyOnlyIsFalse(d.C1_TaxType)).Sum(dutyOrTax => dutyOrTax.C1_Amount)) <= 0.02m;
		}

		static void AddNoDetailsWereFoundMessageError(ZPropertyInfo notificationInfo, string numberType)
		{
			notificationInfo.AddMessageError(Res.GetString("de232eb2-2ad8-4d68-bcfc-4f619e488952", "No details were found for the {0} Number.\r\nPlease check the number and, if necessary, request a Classification/Tariff Query from the Brokerage > Messages menu.", numberType));
		}

		#endregion

		#region ValidateDutyOrTaxCount

		internal void ValidateDutyOrTaxCount(DutyAndTax dutyOrTax)
		{
			if (!IsCaculatingDutyAndTax)
			{
				var dutiesOrTaxes = GetDutiesOrTaxesByType(dutyOrTax.C1_TaxType);
				var count = dutyOrTax.C1_TaxType == DutyAndTaxTypes.Codes.CustomsDuty || DutyAndTaxTypes.IsSIMATaxCode(dutyOrTax.C1_TaxType) ? 3 : 1;
				if (dutiesOrTaxes.Count() > count)
				{
					var codeInMessage = DutyAndTaxTypes.IsSIMATaxCode(dutyOrTax.C1_TaxType) ? DutyAndTaxTypes.Codes.SIMADuty : (string)dutyOrTax.C1_TaxType;
					dutyOrTax.C1_TaxTypeInfo.AddMessageError(Res.GetString("bf5d82b3-c81e-4408-b7c4-f88d69119dd0", "More than {0} {1} rates apply to this Classification Number", count, new DutyAndTaxTypes().GetDescriptionFromCode(codeInMessage)));
				}

				ValidateDutyOrTaxCountForAllExcept(dutiesOrTaxes, dutyOrTax.PK);
			}
		}

		void ValidateDutyOrTaxCountForAllExcept(IEnumerable<DutyAndTax> dutiesOrTaxes, ZGuid exceptPK)
		{
			if (!validating)
			{
				validating = true;
				foreach (var dutyOrTax in from dutyOrTax in dutiesOrTaxes where dutyOrTax.PK != exceptPK select dutyOrTax)
				{
					dutyOrTax.AddInfoValidation.ValidateC1_TaxType();
				}
				validating = false;
			}
		}

		bool validating;

		internal static void AddNoDutyRateFoundMessageError(ZPropertyInfo notificationInfo, string classTariff, string treatmentCode, ZDateTime effectiveDate)
		{
			notificationInfo.AddMessageError(Res.GetString("8FBC0907-99C6-42B9-8599-BE037F1232B5", @"No duty rate has been found for classification tariff {0} with treatment code {1} for effective date {2}.
Please check the entered details and, if necessary, request a Classification/Tariff Query from the Brokerage > Messages menu.", classTariff, treatmentCode, effectiveDate.ToShortDateString()));
		}

		#endregion

		#endregion

		#region Implementation

		internal static void ApportionTotalValueOverLinesIfDifferentToLinesTotal<T>(ZDecimal total, IEnumerable<T> lines, Func<T, decimal> getLineValue, Action<T, decimal> addCentsToLine)
		{
			if (total > 0)
			{
				lines = lines.OrderByDescending(getLineValue);
				var linesTotal = lines.Sum(getLineValue);
				var centsToApportion = (int)((total - linesTotal) * 100);

				if (centsToApportion != 0 && lines.Any())
				{
					int remainder;
					var centsPerLine = Math.DivRem(centsToApportion, lines.Count(), out remainder);
					remainder = Math.Abs(remainder);

					foreach (var line in lines)
					{
						if (centsToApportion == 0)
						{
							break;
						}

						var centsToAdd = centsPerLine + (remainder-- > 0 ? Math.Sign(centsToApportion) : 0);
						addCentsToLine(line, ((decimal)centsToAdd / 100));
						centsToApportion -= centsToAdd;
					}
				}
			}
		}

		#region Duties & Taxes

		internal IEnumerable<DutyAndTax> Duties
		{
			get { return duties == null || IsCaculatingDutyAndTax ? (duties = GetDutiesOrTaxesOrdered(DutyAndTaxTypes.Codes.CustomsDuty)) : duties; }
		}
		IEnumerable<DutyAndTax> duties;

		internal IEnumerable<DutyAndTax> SIMADuties
		{
			get { return simaDuties == null || IsCaculatingDutyAndTax ? (simaDuties = GetDutiesOrTaxesOrdered(DutyAndTaxTypes.Codes.SIMADuty)) : simaDuties; }
		}
		IEnumerable<DutyAndTax> simaDuties;

		internal IEnumerable<DutyAndTax> ExciseTaxes
		{
			get { return exciseTaxes == null || IsCaculatingDutyAndTax ? (exciseTaxes = GetDutiesOrTaxesOrdered(DutyAndTaxTypes.Codes.ExciseTax)) : exciseTaxes; }
		}
		IEnumerable<DutyAndTax> exciseTaxes;

		internal IEnumerable<DutyAndTax> GSTaxes
		{
			get { return gsts == null || IsCaculatingDutyAndTax ? (gsts = GetDutiesOrTaxesOrdered(DutyAndTaxTypes.Codes.GST)) : gsts; }
		}
		IEnumerable<DutyAndTax> gsts;

		internal IEnumerable<DutyAndTax> CPTTaxes
		{
			get { return cptTaxes == null || IsCaculatingDutyAndTax ? (cptTaxes = GetDutiesOrTaxesOrdered(DutyAndTaxTypes.Codes.CPT)) : cptTaxes; }
		}
		IEnumerable<DutyAndTax> cptTaxes;

		internal IEnumerable<DutyAndTax> CTATaxes
		{
			get { return ctaTaxes == null || IsCaculatingDutyAndTax ? (ctaTaxes = GetDutiesOrTaxesOrdered(DutyAndTaxTypes.Codes.CTA)) : ctaTaxes; }
		}
		IEnumerable<DutyAndTax> ctaTaxes;

		internal ZDecimal HSTTaxAmount { get; set; }
		internal ZDecimal CTATaxAmount { get; set; }
		internal ZDecimal PSTTaxAmount { get; set; }

		IEnumerable<DutyAndTax> GetDutiesOrTaxesOrdered(string type)
		{
			if (type == DutyAndTaxTypes.Codes.SIMADuty)
			{
				return calculatingDutiesAndTaxes.Where(d => DutyAndTaxTypes.IsSIMATaxCodeIncludingSIMAType(d.C1_TaxType)).OrderBy(a => a, new DutyAndTaxComparer());
			}
			else
			{
				return calculatingDutiesAndTaxes.Where(d => d.C1_TaxType == type).OrderBy(a => a, new DutyAndTaxComparer());
			}
		}

		void ResetDutiesAndTaxes()
		{
			duties = null;
			simaDuties = null;
			exciseTaxes = null;
			gsts = null;
			HSTTaxAmount = ZDecimal.Zero;
			CTATaxAmount = ZDecimal.Zero;
			PSTTaxAmount = ZDecimal.Zero;
		}

		#endregion

		#region Event Handlers

		void DutiesAndTaxes_CountChanged(object sender, EventArgs e)
		{
			if (!IsCaculatingDutyAndTax)
			{
				ResetDutiesAndTaxes();
				FireRunValidateDutiesAndTaxes();
				ValidateDutyOrTaxCountForAllExcept(calculatingDutiesAndTaxes, ZGuid.Empty);
			}
		}

		void ElementTaxTypeValueChanged(DutyAndTax dutyAndTax)
		{
			if (!IsCaculatingDutyAndTax)
			{
				using (var calcMarker = new CalculatingMarker(this))
				{
					dutyAndTax.C1_Override = dutyAndTax.C1_TaxType != DutyAndTaxTypes.Codes.CustomsDuty || Duties.Take(2).Count() == 1 || Duties.First().C1_Override;
					dutyAndTax.C1_ExemptCode = ZString.Empty;
					ResetDutiesAndTaxes();
					FireRunValidateDutiesAndTaxes();
				}
			}
		}

		void ElementOverrideValueChanged(DutyAndTax dutyOrTax)
		{
			if (!IsCaculatingDutyAndTax)
			{
				using (var calcMarker = new CalculatingMarker(this))
				{
					switch (dutyOrTax.C1_TaxType)
					{
						case DutyAndTaxTypes.Codes.CustomsDuty:
							foreach (var duty in Duties)
							{
								if (duty.PK != dutyOrTax.PK)
								{
									duty.C1_Override = dutyOrTax.C1_Override;
									duty.AddInfoValidation.ValidateOnOverrideValueChanged();
								}
							}
							break;
						case DutyAndTaxTypes.Codes.ExciseTax:
						case DutyAndTaxTypes.Codes.GST:
							UpdateTaxRate(dutyOrTax);
							break;
					}
					dutyOrTax.AddInfoValidation.ValidateOnOverrideValueChanged();
				}
			}
		}

		void ElementCodeValueChanged(DutyAndTax dutyOrTax)
		{
			if (!IsCaculatingDutyAndTax)
			{
				using (var calcMarker = new CalculatingMarker(this))
				{
					if (dutyOrTax.IsTax)
					{
						if (dutyOrTax.C1_ExemptCode.IsEmpty && dutyOrTax.IsExciseTax)
						{
							UpdateExciseTax(dutyOrTax);
						}
						if (dutyOrTax.C1_Override && dutyOrTax.IsExciseTax && data is JobComInvoiceLine invoiceLine && invoiceLine.IsLuxuryTaxInvoiceLine)
						{
							var taxRate = CACTaxRate.Load(factory, dutyOrTax.C1_Code, dutyOrTax.C1_TaxType, data.EffectiveDutyDate);
							if (taxRate != null)
							{
								dutyOrTax.C1_RateType = taxRate.ZH_RateType;
								dutyOrTax.C1_Rate = taxRate.ZH_Rate;
								dutyOrTax.C1_UnitOfMeasure = taxRate.ZH_UnitOfMeasure;
							}
						}
						else
						{
							UpdateTaxRate(dutyOrTax);
						}
					}
				}
			}
		}

		void ElementQuantityValueChanged(object sender, EventArgs e)
		{
			foreach (var duty in GetDutiesOrTaxesByType(DutyAndTaxTypes.Codes.SIMADuty))
			{
				if (!duty.IsAmountNotRequiredForSIMA)
				{
					DutyAndTaxAmountCalculator.CalculateDutyAndTaxAmount(duty, data.CustomsValue);
				}
			}
		}

		void FireRunValidateDutiesAndTaxes()
		{
			if (RunValidateDutiesAndTaxes != null)
			{
				RunValidateDutiesAndTaxes();
			}
		}

		void FireOnStartCalculation()
		{
			if (OnStartCalculation != null)
			{
				OnStartCalculation(this, new EventArgs());
			}
		}

		void FireOnFinishCalculation()
		{
			if (OnFinishCalculation != null)
			{
				OnFinishCalculation(this, new EventArgs());
			}
		}

		#endregion

		#region CombinedDuty

		internal class CombinedDuty
		{
			internal enum Type { Classification, Excise, Tariff, Empty }

			internal CombinedDuty(IDutyAndTaxDataForCalculation duty)
			{
				Regulars = new List<IDutyAndTaxDataForCalculation>();
				if (duty != null)
				{
					Regulars.Add(duty);
				}
			}

			internal CombinedDuty(Type dutyType)
			{
				DutyType = dutyType;
				Regulars = new List<IDutyAndTaxDataForCalculation>();
			}

			CombinedDuty(IEnumerable<IDutyAndTaxDataForCalculation> regulars)
			{
				Regulars = new List<IDutyAndTaxDataForCalculation>(regulars);
			}

			internal bool HasAdValorem
			{
				get
				{
					return Regulars.Any(duty => duty.RateType == RateTypes.Codes.AdValorem)
							 || (Min != null && Min.RateType == RateTypes.Codes.AdValorem)
							 || (Min != null && Min.RateType == RateTypes.Codes.AdValorem);
				}
			}

			internal bool HasMinOrMax
			{
				get { return Min != null || Max != null; }
			}

			internal IEnumerable<CombinedDuty> AsEnumerable()
			{
				yield return new CombinedDuty(Regulars);
				yield return new CombinedDuty(Min);
				yield return new CombinedDuty(Max);
			}

			internal Type DutyType { get; private set; }
			internal IDutyAndTaxDataForCalculation Min { get; set; }
			internal IDutyAndTaxDataForCalculation Max { get; set; }
			internal List<IDutyAndTaxDataForCalculation> Regulars { get; private set; }

			internal static ZString GetDutyTypeCode(Type type)
			{
				var result = ZString.Empty;
				switch (type)
				{
					case Type.Classification:
						result = Classification;
						break;
					case Type.Excise:
						result = Excise;
						break;
					case Type.Tariff:
						result = Tariff;
						break;
					default:
						break;
				}
				return result;
			}

			internal const string Classification = "CLS";
			internal const string Excise = "EXC";
			internal const string Tariff = "TRF";
		}

		#endregion

		IEnumerable<DutyAndTax> calculatingDutiesAndTaxes
		{
			get { return IsCaculatingDutyAndTax ? dutiesAndTaxes.Where(t => !t.IsOldRow) : dutiesAndTaxes; }
		}

		TariffView TariffView => new TariffView.Loader(data.Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Canada, Constants.TariffTypes.HarmonizedSystem, data.ClassificationNumber, data.EffectiveDutyDate);

		bool IsCaculatingDutyAndTax
		{
			get { return calculatingIndex > 0; }
		}

		class CalculatingMarker : IDisposable
		{
			public CalculatingMarker(DutyAndTaxManager manager)
			{
				dutyAndTaxManager = manager;
				dutyAndTaxManager.calculatingIndex++;
			}
			readonly DutyAndTaxManager dutyAndTaxManager;

			public void Dispose()
			{
				dutyAndTaxManager.ClearOldDutiesAndTaxes();
				dutyAndTaxManager.calculatingIndex--;
			}
		}

		readonly IDutyAndTaxData data;
		readonly DutyAndTaxCollection dutiesAndTaxes;
		readonly BusinessObjectFactory factory;
		readonly List<CombinedDuty> adValoremDuties = new List<CombinedDuty>();
		bool tariffRateFoundLastTime;
		bool classHeaderFoundLastTime;
		internal bool IsCalculated { get; set; }
		int calculatingIndex;
		bool customsValueInvalid;
		internal event RunValidationInvoker RunValidateDutiesAndTaxes;
		internal event EventHandler OnStartCalculation;
		internal event EventHandler OnFinishCalculation;
		delegate void PopulateDutiesOrTaxesDelegate(CACClassHeader classHeader);

		#endregion
	}
}
