using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class CalculationTrigger
	{
		public CalculationTrigger(ChargeWithCost charge)
		{
			this.Charge = charge;
		}

		#region Initialisation

		protected ChargeWithCost Charge;
		protected bool fIsProcessing;

		bool Enabled => !isCaclualtionsSwitchedOff && !CalculationSuspender.IsSuspended;

#if DEBUG
		public bool Enabled_ForTestOnly => Enabled;
#endif

#if DEBUG
		internal bool IsProcessingForTest
		{
			get { return fIsProcessing; }
			set { fIsProcessing = value; }
		}
#endif

		public void SwitchOffCalculations() => isCaclualtionsSwitchedOff = true;
		bool isCaclualtionsSwitchedOff;

		public IDisposable SuspendCalculations() => CalculationSuspender.GetSuspender();

		public IDisposable SuspendLocalToForeignOrForeginToLocalCostAmountConversionCalculations() => LocalToForeignOrForeginToLocalCostAmountCalculationSuspender.GetSuspender();

		public IDisposable SuspendLocalToForeignOrForeginToLocalSellAmountConversionCalculations() => LocalToForeignOrForeginToLocalSellAmountCalculationSuspender.GetSuspender();

		FunctionalitySuspender CalculationSuspender
		{
			get
			{
				if (calculationSuspender == null)
				{
					calculationSuspender = new FunctionalitySuspender
						(
							onResumeAction: () => FunctionalitySuspender.ResumeCollection(calculationTriggerSuspender_AdditionalSuspenders),
							onSuspendAction: () => calculationTriggerSuspender_AdditionalSuspenders = new[]
								{
									Charge.CostRoundingErrorReproterFunctionalitySuspender.GetSuspender(),
									Charge.SellRoundingErrorReproterFunctionalitySuspender.GetSuspender()
								}
						);
				}

				return calculationSuspender;
			}
		}
		FunctionalitySuspender calculationSuspender;
		IEnumerable<IDisposable> calculationTriggerSuspender_AdditionalSuspenders;

		FunctionalitySuspender LocalToForeignOrForeginToLocalCostAmountCalculationSuspender
		{
			get
			{
				if (localToForeignOrForeginToLocalCostAmountCalculationSuspender == null)
				{
					localToForeignOrForeginToLocalCostAmountCalculationSuspender = new FunctionalitySuspender
						(
							onResumeAction: () => FunctionalitySuspender.ResumeCollection(costAmountConversionCalculationSuspender_AdditionalSuspenders),
							onSuspendAction: () => costAmountConversionCalculationSuspender_AdditionalSuspenders = new[]
								{
									Charge.CostRoundingErrorReproterFunctionalitySuspender.GetSuspender(),
								}
						);
				}

				return localToForeignOrForeginToLocalCostAmountCalculationSuspender;
			}
		}
		FunctionalitySuspender localToForeignOrForeginToLocalCostAmountCalculationSuspender;
		IEnumerable<IDisposable> costAmountConversionCalculationSuspender_AdditionalSuspenders;

		FunctionalitySuspender LocalToForeignOrForeginToLocalSellAmountCalculationSuspender
		{
			get
			{
				if (localToForeignOrForeginToLocalSellAmountCalculationSuspender == null)
				{
					localToForeignOrForeginToLocalSellAmountCalculationSuspender = new FunctionalitySuspender
						(
							onResumeAction: () => FunctionalitySuspender.ResumeCollection(sellAmountConversionCalculationSuspender_AdditionalSuspenders),
							onSuspendAction: () => sellAmountConversionCalculationSuspender_AdditionalSuspenders = new[]
								{
									Charge.SellRoundingErrorReproterFunctionalitySuspender.GetSuspender()
								}
						);
				}

				return localToForeignOrForeginToLocalSellAmountCalculationSuspender;
			}
		}
		FunctionalitySuspender localToForeignOrForeginToLocalSellAmountCalculationSuspender;
		IEnumerable<IDisposable> sellAmountConversionCalculationSuspender_AdditionalSuspenders;

		protected bool IsProcessing
		{
			get { return fIsProcessing; }
		}

		#endregion

		ZArchitecture.Environment.ExchangeRate ExchangeRate => (Charge.Company ?? Env.CurrentCompany).ExchangeRate;

		#region Cost Fields Triggers

		public void CostCurrencyChange(string prevCurrencyCode, string newCurrencyCode)
		{
			if (IsProcessing || !Enabled)
			{
				if (IsProcessing && AccountingValuesRoundingHelper.HasChangeInDecimalPlaces(Charge.Factory, prevCurrencyCode, newCurrencyCode))
				{
					SetForeignCostAmountFromLocal();
				}
				return;
			}

			try
			{
				fIsProcessing = true;
				SetForeignCostAmountFromLocal();
				UpdateRevenueBasedOnCost();
			}
			finally
			{
				fIsProcessing = false;
			}
		}

		public void CostExRateChanged()
		{
			if (IsProcessing || !Enabled)
			{
				return;
			}

			try
			{
				fIsProcessing = true;
				SetLocalCostAmountFromForeign();
				if (Charge.IsDisbursementCharge)
				{
					UpdateDSBChargeAmountsFromCost();
				}
			}
			finally
			{
				fIsProcessing = false;
			}
		}

		public void LocalCostAmountChange()
		{
			if (IsProcessing || !Enabled)
			{
				return;
			}

			try
			{
				fIsProcessing = true;

				if (!Charge.JR_IsApportioned)
				{
					SetForeignCostAmountFromLocal();
				}

				UpdateRevenueBasedOnCost();
			}
			finally
			{
				fIsProcessing = false;
			}
		}

		public void ForeignCostAmountChange()
		{
			if (IsProcessing || !Enabled)
			{
				return;
			}

			try
			{
				fIsProcessing = true;
				SetLocalCostAmountFromForeign();
				UpdateRevenueBasedOnCost();
			}
			finally
			{
				fIsProcessing = false;
			}
		}

		#region Helpers

		bool cleanupMode;

		public void UpdateRevenueBasedOnCostForCleanup()
		{
			try
			{
				cleanupMode = true;
				UpdateRevenueBasedOnCost();
			}
			finally
			{
				cleanupMode = false;
			}
		}

		public void UpdateCostBasedOnRevenueForCleanup()
		{
			try
			{
				cleanupMode = true;
				UpdateCostForMarginCode();
			}
			finally
			{
				cleanupMode = false;
			}
		}

		/// <summary>
		/// Updates Revenue amount based on cost Amount if Revenue is not posted
		/// - if it's a DSB charge and the cost Amount is 0, then Revenue must be set to 0
		/// - if Revenue is 0 and Cost is not 0, then populate it based on cost
		/// </summary> 
		internal void UpdateRevenueBasedOnCost()
		{
			using (Charge.SuppressAutoRatingOverride())
			{
				if (!Charge.IsRevenuePosted && !Charge.IsManualJobAccrualCharge && (!Charge.JR_SellRated || Charge.IsDisbursementCharge))
				{
					if (Charge.CostCurrency != null && (Charge.JR_OSSellAmt == 0 || cleanupMode) && Charge.JR_OSCostAmt != 0)
					{
						if (Charge.JR_RX_NKSellCurrency.IsEmpty || Charge.SellAccount == null || Charge.SellAccount.CompanyData.OB_RX_NKARDDefltCurrency.IsEmpty || Charge.JR_RX_NKSellCurrency != Charge.SellAccount.CompanyData.OB_RX_NKARDDefltCurrency)
						{
							Charge.JR_RX_NKSellCurrency = Charge.JR_RX_NKCostCurrency;
						}
						Charge.UpdateInvoiceType();
						if (!Charge.IsCreateProfitShareCharges)
						{
							if (Charge.JR_RX_NKSellCurrency == Charge.JR_RX_NKCostCurrency)
							{
								Charge.JR_OSSellAmt = Charge.GetRevenueAmountBasedOnCost();
							}
							else
							{
								SetForeignSellAmountWhenCostAndSellCurrenciesAreDifferent();
							}
						}
						SetLocalSellAmountFromForeignAmount();
					}
					if (Charge.IsDisbursementCharge)
					{
						UpdateDSBChargeAmountsFromCost();
					}
				}
			}
		}

		void SetForeignSellAmountWhenCostAndSellCurrenciesAreDifferent()
		{
			var exRateFromCharge = Charge.RevenueExchangeRate;
			var exRate = exRateFromCharge != null ? exRateFromCharge.Rate : Charge.JR_OSSellExRate;
			var localAmount = Charge.GetLocalRevenueAmountBasedOnCost();
			Charge.JR_OSSellAmt = ExchangeRate.LocalToForeign(localAmount, exRate, Charge.JR_RX_NKSellCurrency);
		}

		/// <summary>
		/// Calculates Local Cost Amount based on Cost Foreign amount
		/// </summary>
		protected void SetLocalCostAmountFromForeign()
		{
			if (!LocalToForeignOrForeginToLocalCostAmountCalculationSuspender.IsSuspended)
			{
				using (Charge.SuppressAutoRatingOverride())
				{
					Charge.JR_LocalCostAmt = ExchangeRate.ForeignToLocal(Charge.JR_OSCostAmt, Charge.JR_OSCostExRate);
				}
			}
		}

		/// <summary>
		/// Calculates Foreign Cost Amount based on Cost Local amount
		/// </summary>
		protected void SetForeignCostAmountFromLocal()
		{
			if (Charge.CostCurrency != null && !LocalToForeignOrForeginToLocalCostAmountCalculationSuspender.IsSuspended)
			{
				Charge.JR_OSCostAmt = ExchangeRate.LocalToForeign(Charge.JR_LocalCostAmt, Charge.JR_OSCostExRate, Charge.JR_RX_NKCostCurrency);
			}
		}

		#endregion

		#endregion

		#region Revenue Fields Triggers

		public void SellCurrencyChange(string prevCurrencyCode, string newCurrencyCode)
		{
			if (IsProcessing || !Enabled)
			{
				if (IsProcessing && AccountingValuesRoundingHelper.HasChangeInDecimalPlaces(Charge.Factory, prevCurrencyCode, newCurrencyCode))
				{
					SetForeignSellAmountFromLocalAmount();
				}
				CollectOSSellAmtNotEqualLocalSellAmtInfoIfRequired();
				return;
			}

			try
			{
				fIsProcessing = true;
				DoSellCurrencyChange();
				CollectOSSellAmtNotEqualLocalSellAmtInfoIfRequired();
			}
			finally
			{
				fIsProcessing = false;
			}
		}

		void CollectOSSellAmtNotEqualLocalSellAmtInfoIfRequired()
		{
			var localCurrency = Charge.Company?.GC_RX_NKLocalCurrency ?? ZString.Empty;
			if (Charge.JR_RX_NKSellCurrency == localCurrency && Charge.JR_OSSellExRate == 1m && Charge.JR_OSSellAmt != Charge.JR_LocalSellAmt)
			{
				CriticalValidationInfoCollectorService.GetOrCreateService(Charge.Factory).AddLastInfoWhenAllowed(Charge.PK,
					CriticalValidationInfoCollectorServiceKeyType.JobChargeOSSellAmountNotRecalculatedWhenSellCurrencyIsSetToLocalCurrency,
					() => FormattableString.Invariant($"\r\nStack Trace: {System.Environment.StackTrace}\r\n\r\n{Charge.GetJobChargeInfo()}\r\n\r\n{GetFlagInformation()}"));
			}
		}

		string GetFlagInformation()
		{
			var messageBuilder = new ZStringBuilder();
			messageBuilder.AppendLine($"Charge Calculation Trigger Flag Values:");
			messageBuilder.AppendLine($"IsProcessing :{IsProcessing.ToYesNoString()}");
			messageBuilder.AppendLine($"isCaclualtionsSwitchedOff :{isCaclualtionsSwitchedOff.ToYesNoString()}");
			messageBuilder.AppendLine($"CalculationSuspender.IsSuspended :{CalculationSuspender.IsSuspended.ToYesNoString()}");
			messageBuilder.AppendLine($"LocalToForeignOrForeginToLocalSellAmountCalculationSuspender.IsSuspended :{LocalToForeignOrForeginToLocalSellAmountCalculationSuspender.IsSuspended.ToYesNoString()}");
			return messageBuilder.ToString();
		}

		public void SellExRateChanged()
		{
			if (IsProcessing || !Enabled)
			{
				return;
			}

			try
			{
				fIsProcessing = true;
				if (Charge.IsDisbursementCharge)
				{
					UpdateDSBChargeAmountsFromCost();
				}
				else
				{
					SetLocalSellAmountFromForeignAmount(isSettingExRate: true);
				}
			}
			finally
			{
				fIsProcessing = false;
			}
		}

		public void ForeignSellAmountChanged()
		{
			if (IsProcessing || !Enabled)
			{
				return;
			}

			try
			{
				fIsProcessing = true;
				SetLocalSellAmountFromForeignAmount();
				UpdateMarginOrDisbursementAmounts();
			}
			finally
			{
				fIsProcessing = false;
			}
		}

		public void LocalSellAmountChanged()
		{
			if (IsProcessing || !Enabled)
			{
				return;
			}

			try
			{
				fIsProcessing = true;
				SetForeignSellAmountFromLocalAmount();
				UpdateMarginOrDisbursementAmounts();
			}
			finally
			{
				fIsProcessing = false;
			}
		}

		void UpdateMarginOrDisbursementAmounts()
		{
			if (Charge.IsMarginCharge)
			{
				UpdateCostForMarginCode();
			}
			else if (Charge.IsDisbursementCharge)
			{
				UpdateDSBCostFromSell();
			}
		}

		#region Helper methods

		protected void SetLocalSellAmountFromForeignAmount(bool isSettingExRate = false)
		{
			if (!LocalToForeignOrForeginToLocalSellAmountCalculationSuspender.IsSuspended && !Charge.HasContext(JobInvoicingBusinessContext.SuspendJobChargeCalculationTrigger))
			{
				using (Charge.SuppressAutoRatingOverride())
				{
					if (!isSettingExRate && Charge.RevenueExchangeRate != null)
					{
						Charge.UpdateSellExRateWithBaseRate(Charge.RevenueExchangeRate.Rate);
					}
					Charge.JR_LocalSellAmt = ExchangeRate.ForeignToLocal(Charge.JR_OSSellAmt, Charge.JR_OSSellExRate);
				}
			}
		}

		protected void SetForeignSellAmountFromLocalAmount()
		{
			if (Charge.SellCurrency != null && !LocalToForeignOrForeginToLocalSellAmountCalculationSuspender.IsSuspended
				&& !Charge.HasContext(JobInvoicingBusinessContext.SuspendJobChargeCalculationTrigger))
			{
				if (Charge.RevenueExchangeRate != null)
				{
					Charge.UpdateSellExRateWithBaseRateFromLocalAmt(Charge.RevenueExchangeRate.Rate);
				}
				Charge.JR_OSSellAmt = ExchangeRate.LocalToForeign(Charge.JR_LocalSellAmt, Charge.JR_OSSellExRate, Charge.SellCurrency.RX_Code);
			}
		}

		/// <summary>
		/// Updates Cost Amount for a Margin Charge code if it's 0
		/// </summary>
		protected void UpdateCostForMarginCode()
		{
			if (!Charge.IsCostPosted && !Charge.JR_IsApportioned && !Charge.JR_CostRated)
			{
				if ((Charge.JR_LocalCostAmt == 0 || cleanupMode) && Charge.JR_OSSellAmt != 0 && Charge.SellCurrency != null && Charge.IsMarginCharge)
				{
					using (Charge.SuppressAutoRatingOverride())
					{
						if (Charge.JR_RX_NKCostCurrency.IsEmpty || Charge.CostAccount == null || Charge.CostAccount.CompanyData.OB_RX_NKAPDefltCurrency.IsEmpty || Charge.JR_RX_NKCostCurrency != Charge.CostAccount.CompanyData.OB_RX_NKAPDefltCurrency)
						{
							Charge.JR_RX_NKCostCurrency = Charge.JR_RX_NKSellCurrency;
						}

						if (!Charge.IsCreateProfitShareCharges)
						{
							if (Charge.JR_RX_NKSellCurrency == Charge.JR_RX_NKCostCurrency)
							{
								Charge.JR_OSCostAmt = Charge.GetMarginAmountDown(Charge.JR_OSSellAmt, Charge.SellCurrency.RX_Code);
								SetLocalCostAmountFromForeign();
							}
							else
							{
								var exRateFromCharge = Charge.CostExchangeRate;
								var exRate = exRateFromCharge != null ? exRateFromCharge.Rate : Charge.JR_OSCostExRate;
								var osCostAmount = ExchangeRate.LocalToForeign(Charge.JR_LocalSellAmt, exRate, Charge.JR_RX_NKCostCurrency);

								Charge.JR_OSCostAmt = Charge.GetMarginAmountDown(osCostAmount, Charge.JR_RX_NKCostCurrency);
								SetLocalCostAmountFromForeign();
							}
						}
					}
				}
			}
		}

		protected void DoSellCurrencyChange()
		{
			Charge.UpdateInvoiceType();
			if (Charge.IsDisbursementCharge)
			{
				UpdateDSBChargeAmountsFromCost();
			}
			else
			{
				SetForeignSellAmountFromLocalAmount();
			}
		}

		protected void UpdateDSBChargeAmountsFromCost()
		{
			if (!Charge.IsRevenuePosted)
			{
				using (Charge.SuppressAutoRatingOverride())
				{
					if (Charge.IsCostForeign && Charge.IsSellLocal)
					{
						Charge.JR_OSSellAmt = Charge.JR_LocalSellAmt = Charge.JR_LocalCostAmt;
					}
					else if (Charge.IsCostForeign && Charge.IsSellForeign)
					{
						if ((Charge.RevenueExchangeRate?.CFXPercent ?? 0m) == 0m)
						{
							if (Charge.JR_RX_NKCostCurrency == Charge.JR_RX_NKSellCurrency)
							{
								var osSellAmtToBeSet = Charge.JR_OSCostAmt;
								if (Charge.JR_OSSellExRate != Charge.JR_OSCostExRate)
								{
									Charge.JR_LocalSellAmt = ExchangeRate.ForeignToLocal(osSellAmtToBeSet, Charge.JR_OSSellExRate);
								}
								else
								{
									Charge.JR_LocalSellAmt = Charge.JR_LocalCostAmt;
								}
								Charge.JR_OSSellAmt = osSellAmtToBeSet;
							}
							else
							{
								Charge.JR_LocalSellAmt = Charge.JR_LocalCostAmt;
								SetSellAmountFromCostWhenSellAndCostAreForeign();
							}
						}
						else
						{
							if (Charge.JR_RX_NKCostCurrency == Charge.JR_RX_NKSellCurrency)
							{
								SetSellAmountFromCostWhenSellAndCostAreForeign();
								SetLocalSellAmountFromForeignAmount();
							}
							else
							{
								SetForeignSellAmountWhenCostAndSellCurrenciesAreDifferent();
								SetLocalSellAmountFromForeignAmount();
							}
						}
					}
					else if (Charge.IsCostLocal && Charge.IsSellForeign && !(Charge.RevenueExchangeRate?.CFXPercent ?? ZDecimal.Zero).IsEmpty)
					{
						SetForeignSellAmountWhenCostAndSellCurrenciesAreDifferent();
						SetLocalSellAmountFromForeignAmount();
					}
					else
					{
						Charge.JR_LocalSellAmt = Charge.JR_LocalCostAmt;
						SetForeignSellAmountFromLocalAmount();
					}
				}
			}
		}

		void SetSellAmountFromCostWhenSellAndCostAreForeign()
		{
			var sellCurrencyCode = Charge.SellCurrency != null ? Charge.SellCurrency.RX_Code : ZString.Empty;
			Charge.JR_OSSellAmt = sellCurrencyCode == Charge.JR_RX_NKCostCurrency ? Charge.JR_OSCostAmt :
				new ZDecimal(GetConvertedForeignAmount(Charge.JR_OSCostAmt,
					Charge.CostExchangeRate?.Rate ?? (Charge.JR_IsApportioned || Charge.IsCostPosted ? Charge.JR_OSCostExRate : new ZDecimal(1m)),
					sellCurrencyCode, Charge.RevenueExchangeRate?.SellRate ?? 1m));
		}

		decimal GetConvertedForeignAmount(decimal foreignAmountToConvert, decimal foreignAmountLocalConversionRate, ZString currencyToConvertTo, decimal exchangeRateToConvertTo)
		{
			decimal result = foreignAmountToConvert;
			if (currencyToConvertTo.IsValid)
			{
				decimal unRoundedLocalValue = ExchangeRate.ForeignToLocalWithoutRounding(foreignAmountToConvert, foreignAmountLocalConversionRate);
				result = ExchangeRate.LocalToForeign(unRoundedLocalValue, exchangeRateToConvertTo, currencyToConvertTo);
			}
			return result;
		}

		protected void UpdateDSBCostFromSell()
		{
			if (!Charge.IsCostPosted && !Charge.JR_IsApportioned && Charge.IsDisbursementCharge)
			{
				if (Charge.IsSellForeign)
				{
					if (Charge.IsCostLocal)
					{
						Charge.JR_RX_NKCostCurrency = Charge.JR_RX_NKSellCurrency;
					}
					Charge.JR_OSCostAmt = GetConvertedForeignAmount(Charge.JR_OSSellAmt,
							Charge.RevenueExchangeRate?.Rate ?? 1m,
							(Charge.JR_RX_NKCostCurrency),
							Charge.CostExchangeRate?.Rate ?? 1m);
					SetLocalCostAmountFromForeign();
				}
				else
				{
					Charge.JR_LocalCostAmt = Charge.JR_LocalSellAmt;
					SetForeignCostAmountFromLocal();
				}
			}
		}

		#endregion

		#endregion
	}
}
