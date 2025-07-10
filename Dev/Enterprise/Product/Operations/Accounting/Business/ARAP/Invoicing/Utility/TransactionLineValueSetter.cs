using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public static class TransactionLineValueSetter
	{
		public static void SetCostValues(this InvoicingLineBase line, Job job, ChargeWithCost chargeWithCost)
		{
			line.SetConsolID(chargeWithCost.ParentConsolCost);
			line.AL_AC = chargeWithCost.JR_AC; //This must be set before AL_Desc
			line.AL_Desc = chargeWithCost.JR_Desc;
			line.AL_UnitQty = 0;
			line.AL_UnitPrice = 0M;
			line.AL_OSUnitPrice = 0M;
			line.AL_PostPeriod = 0;
			line.AL_PostToGL = "N";
			line.AL_ReversePeriod = 0;
			line.AL_ReverseToGL = "N";
			line.AL_ReverseDate = ZDateTime.Empty;
			line.AL_PreventInvoicePrintGrouping = chargeWithCost.JR_PreventInvoicePrintGrouping;
			line.AL_JH = job != null ? job.PK : ZGuid.Empty;
			line.AL_GB = chargeWithCost.JR_GB;
			line.AL_GE = chargeWithCost.JR_GE;
			line.AL_AG_PercentOf = ZGuid.Empty;
			line.AL_PercentageOfPeriod = 0;

			line.AL_Sequence = (ZShort)line.InvoiceBase.Lines.Count;

			line.AL_OH = chargeWithCost.JR_OH_CostAccount;
			line.AL_SupplyType = chargeWithCost.JR_CostSupplyType;
			if (AccountingMasterFilesUtils.IsTaxBranchApplicable)
			{
				using (line.TaxBranchCalculationSuspender.GetSuspender())
				{
					line.AL_GB_TaxBranch = chargeWithCost.JR_GB_CostTaxBranch;
				}
			}
			line.AL_AT = chargeWithCost.JR_AT_CostGSTRate;
			if (chargeWithCost.JR_AT_CostGSTRate.IsValid && chargeWithCost.JR_CostTaxDate.IsEmpty)
			{
				var taxDate = AccountingUtils.GetChargeTaxDate(chargeWithCost, line.InvoiceBase.InvoiceDate.Date, LedgerTypes.AccountsPayable);
				line.SetTaxDateSafe(taxDate);
				if (chargeWithCost.JR_IsApportioned )
				{
					var consolCost = chargeWithCost.ParentConsolCost;
					if (consolCost.E6_TaxDate.IsEmpty)
					{
						using ((ZBool)consolCost.E6_IsTaxAmountOverriddenInfo.OriginalValue ? consolCost.TaxRecalculationSuspender.GetSuspender() : CargoWise.Common.DisposableAction.NoAction)
						{
							consolCost.E6_TaxDate = taxDate;
						}
					}
				}
				chargeWithCost.SetCostTaxDateSafe(taxDate);
			}
			else
			{
				line.SetTaxDateSafe(chargeWithCost.JR_CostTaxDate);
			}
			line.AL_A9_VATClass = chargeWithCost.JR_A9_CostVATClass;
			line.AL_RX_NKTransactionCurrency = chargeWithCost.JR_RX_NKCostCurrency;
			line.AL_GovtChargeCode = chargeWithCost.JR_CostGovtChargeCode;
			line.AL_PlaceOfSupply = chargeWithCost.JR_CostPlaceOfSupply;
			line.AL_PlaceOfSupplyType = chargeWithCost.JR_CostPlaceOfSupplyType;

			line.CopyExchangeRateAndAmount(chargeWithCost);

			line.AL_AW = chargeWithCost.JR_AW_CostWHTRate;
		}

		public static void SetValues(this InvoicingLineBase line, InvoicingLineBase sourceLine, bool convertAmoutSignsBetweenTransactionTypes)
		{
			using (line.GetValidationSuspender())
			{
				line.SuspendGenericChargeSettingDefaults();
				try
				{
					line.GenericCharge = sourceLine.GenericCharge;
				}
				finally
				{
					line.ResumeGenericChargeSettingDefaults();
				}
				line.AL_AC = sourceLine.AL_AC;

				line.SuspendAL_JHSettingDefaults();
				try
				{
					line.AL_JH = sourceLine.AL_JH;
				}
				finally
				{
					line.ResumeAL_JHSettingDefaults();
				}

				line.AL_AG = sourceLine.AL_AG;
				line.AL_Desc = sourceLine.AL_Desc;

				line.AL_SupplyType = sourceLine.AL_SupplyType;
				line.AL_AT = sourceLine.AL_AT;
				line.SetTaxDateSafe(sourceLine.AL_TaxDate);
				line.AL_A9_VATClass = sourceLine.AL_A9_VATClass;
				line.AL_AW = sourceLine.AL_AW;
				line.AL_GE = sourceLine.AL_GE;
				line.AL_GB = sourceLine.AL_GB;

				line.AL_RX_NKTransactionCurrency = sourceLine.AL_RX_NKTransactionCurrency;
				line.AL_ExchangeRate = sourceLine.AL_ExchangeRate;
				line.AL_PlaceOfSupply = sourceLine.AL_PlaceOfSupply;
				line.AL_PlaceOfSupplyType = sourceLine.AL_PlaceOfSupplyType;

				line.AL_LocalExTaxAmount = sourceLine.AL_LocalExTaxAmount; //This line required to make initial amount setting to be able to compare signs then. Otherwise line amount can be zero.
				var multiplier = convertAmoutSignsBetweenTransactionTypes && Math.Sign(line.AL_LineAmount) != Math.Sign(sourceLine.AL_LineAmount) ? -1 : 1;

				line.AL_OSExTaxAmount = sourceLine.AL_OSExTaxAmount * multiplier;
				line.AL_LocalExTaxAmount = sourceLine.AL_LocalExTaxAmount * multiplier;
				line.AL_OSTaxAmount = sourceLine.AL_OSTaxAmount * multiplier;
				line.AL_LocalTaxAmount = sourceLine.AL_LocalTaxAmount * multiplier;
				line.AL_OSWHTAmount = sourceLine.AL_OSWHTAmount * multiplier;
				line.AL_LocalWHTAmount = sourceLine.AL_LocalWHTAmount * multiplier;
			}
		}

		public static void CopyExchangeRateAndAmount(this InvoicingLineBase line, ChargeWithCost charge)
		{
			line.AL_ExchangeRate = charge.JR_OSCostExRate;
			line.AL_OSExTaxAmount = charge.JR_OSCostAmt;
			((AccountingSuspenders.IRunMethodSuspending)line).RunMethodSuspended = true;
			try
			{
				line.AL_LocalExTaxAmount = charge.JR_LocalCostAmt;
			}
			finally
			{
				((AccountingSuspenders.IRunMethodSuspending)line).RunMethodSuspended = false;
			}
			line.AL_OSTaxAmount = charge.JR_OSCostGSTAmt_Calc;
		}
	}
}
