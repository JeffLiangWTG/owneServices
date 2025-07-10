using System.Globalization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public abstract partial class BaseCharge
	{
		abstract class JobChargeOSGSTCalculationStrategy
		{
			protected JobChargeOSGSTCalculationStrategy(BaseCharge charge)
			{
				this.Charge = charge;
			}
			protected readonly BaseCharge Charge;

			public void ResetOSCostGSTAmount()
			{
				ResetOSCostGSTAmountCore();

				Charge.JR_Cost_LocalGSTAmountInfo.RefreshBinding();
			}

			protected abstract void ResetOSCostGSTAmountCore();

			public void ResetGSTOverriddenFlag()
			{
				if (Charge.JR_E6.IsEmpty && Charge.JR_IsCostTaxAmountOverridden && !Charge.IsCostPosted)
				{
					Charge.JR_IsCostTaxAmountOverridden = false;
				}
			}

			public abstract ZDecimal GetJR_OSCostGSTAmt();

			public abstract void SetJR_OSCostGSTAmt(ZDecimal value);

			protected ZDecimal CalculateOsCostTaxAmount()
			{
				GlbCompany company = null;
				using (Charge?.Factory.AddDiagnosisForFactoryQueryCacheWhenLoadingEnvCurrentBranchOrCompany())
				{
					company = Charge.Company;
				}
				return JobChargeOSGSTCalculationStrategyStaticHelper.CalculateOsCostTaxAmount(Charge.Factory, Charge.JR_OSCostAmt, Charge.JR_RX_NKCostCurrency, Charge.JR_AT_CostGSTRate, Charge.CostGSTRate?.GetRate(Charge.JR_CostTaxDate), Charge.CostGSTRate?.GetEffectiveExtraRate(Charge.JR_CostTaxDate), Charge.JR_AC, Charge.JR_E6, company.GC_RN_NKCountryCode, company.PK, Charge.SetNZEntryFeeChargeTaxAmountSuspender.IsSuspended);
			}
		}

		public static class JobChargeOSGSTCalculationStrategyStaticHelper
		{
			public static ZDecimal CalculateOsCostTaxAmount(BusinessObjectFactory factory, ZDecimal costAmount, ZString costCurrency, ZGuid taxRatePK, ZDate taxDate, ZGuid chargeCodePK, ZGuid jr_E6, string countryCode, ZGuid companyPK, bool isSetNZEntryFeeChargeTaxAmountSuspended = false)
			{
				var chargeCostCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, costCurrency);
				var chargeCostTaxRate = factory.Load<AccTaxRate>(taxRatePK);

				if (chargeCostTaxRate == null)
				{
					return ZDecimal.Zero;
				}

				return CalculateOsCostTaxAmount(factory, costAmount, chargeCostCurrency, chargeCostTaxRate, chargeCostTaxRate.GetRate(taxDate), chargeCostTaxRate.GetEffectiveExtraRate(taxDate), chargeCodePK, jr_E6, countryCode, companyPK, isSetNZEntryFeeChargeTaxAmountSuspended);
			}

			public static ZDecimal CalculateOsCostTaxAmount(BusinessObjectFactory factory, ZDecimal costAmount, ZString costCurrency, ZGuid taxRatePK, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZGuid chargeCodePK, ZGuid jr_E6, string countryCode, ZGuid companyPK, bool isSetNZEntryFeeChargeTaxAmountSuspended = false)
			{
				var chargeCostCurrency = factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, costCurrency);
				var chargeCostTaxRate = factory.Load<AccTaxRate>(taxRatePK);

				if (chargeCostTaxRate == null)
				{
					return ZDecimal.Zero;
				}

				return CalculateOsCostTaxAmount(factory, costAmount, chargeCostCurrency, chargeCostTaxRate, rate, effectiveExtraRate, chargeCodePK, jr_E6, countryCode, companyPK, isSetNZEntryFeeChargeTaxAmountSuspended);
			}

			static ZDecimal CalculateOsCostTaxAmount(BusinessObjectFactory factory, ZDecimal costAmount, RefCurrency costCurrency, AccTaxRate taxID, ZDecimal? rate, ZDecimal? effectiveExtraRate, ZGuid chargeCodePK, ZGuid jr_E6, string countryCode, ZGuid companyPK, bool isSetNZEntryFeeChargeTaxAmountSuspended = false)
			{
				var result = TaxAmountCalculator.GetOSTaxAmount(factory, costAmount, taxID, rate, effectiveExtraRate, costCurrency, companyPK);
				if (!isSetNZEntryFeeChargeTaxAmountSuspended)
				{
					result = NZCustomsEntryFeeTaxCalculator.GetEntryFeeGSTForCostCharge(chargeCodePK, jr_E6, costCurrency?.Code ?? ZString.Empty, costAmount, result, countryCode, companyPK);
				}
				return result;
			}
		}

		class JobChargeOSGSTCalculationWhenNotOverridden : JobChargeOSGSTCalculationStrategy
		{
			public JobChargeOSGSTCalculationWhenNotOverridden(BaseCharge charge)
				: base(charge)
			{
			}

			public override ZDecimal GetJR_OSCostGSTAmt()
			{
				return CalculateOsCostTaxAmount();
			}

			public override void SetJR_OSCostGSTAmt(ZDecimal value)
			{
				var message = string.Format(CultureInfo.CurrentCulture, $@"Trying to set GST amount when JR_IsCostTaxAmountOverridden is false.");
				ErrorReporter.ReportOnce("SettingGSTAmountAtChargeLevelWhenJR_IsCostTaxAmountOverriddenIsFalse_1", message);
			}

			protected override void ResetOSCostGSTAmountCore()
			{
				Charge.JR_OSCostGSTAmt = ZDecimal.Zero;
			}
		}

		class JobChargeOSGSTCalculationWhenOverridden : JobChargeOSGSTCalculationStrategy
		{
			public JobChargeOSGSTCalculationWhenOverridden(BaseCharge charge)
				: base(charge)
			{
			}

			public override ZDecimal GetJR_OSCostGSTAmt()
			{
				return Charge.JR_OSCostGSTAmt;
			}

			public override void SetJR_OSCostGSTAmt(ZDecimal amt)
			{
				if (!Charge.SetNZEntryFeeChargeTaxAmountSuspender.IsSuspended)
				{
					Charge.JR_OSCostGSTAmt = NZCustomsEntryFeeTaxCalculator.GetEntryFeeGSTForCostCharge(Charge.JR_AC, Charge.JR_E6, Charge.JR_RX_NKCostCurrency, Charge.JR_OSCostAmt, amt, Charge.Company.GC_RN_NKCountryCode, Charge.Company.PK);
				}
				else
				{
					Charge.JR_OSCostGSTAmt = amt;
				}
				Charge.JR_OSCostGSTAmt_CalcInfo.RefreshBinding();
			}

			protected override void ResetOSCostGSTAmountCore()
			{
				Charge.JR_OSCostGSTAmt = new JobChargeOSGSTCalculationWhenNotOverridden(Charge).GetJR_OSCostGSTAmt();
				Charge.JR_OSCostGSTAmt_CalcInfo.RefreshBinding();
			}
		}
	}
}
