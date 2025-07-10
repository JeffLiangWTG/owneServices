#if DEBUG

using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public partial class BaseCharge
	{
		public void OnRevenueExchangeRateChanged_ForTestOnly(object sender, EventArgs e)
		{
			OnRevenueExchangeRateChanged(sender, e);
		}

		public bool JR_GB_InternalBranch_ReadOnly_ForTestOnly => JR_GB_InternalBranch_ReadOnly;

		public bool JR_GE_InternalDept_ReadOnly_ForTestOnly => JR_GE_InternalDept_ReadOnly;

		public bool JR_JH_InternalJob_ReadOnly_ForTestOnly => JR_JH_InternalJob_ReadOnly;

		public bool JR_SellGovtChargeCode_ReadOnly_ForTestOnly => JR_SellGovtChargeCode_ReadOnly;

		public bool JR_AT_SellGSTRate_ReadOnly_ForTestOnly => JR_AT_SellGSTRate_ReadOnly;

		public bool JR_OH_SellAccount_ReadOnly_ForTestOnly => JR_OH_SellAccount_ReadOnly;

		public bool IsSellGSTFieldReadOnly_ForTestOnly => IsSellGSTFieldReadOnly;

		public bool JR_GB_ReadOnly_ForTestOnly => JR_GB_ReadOnly;

		public bool JR_GE_ReadOnly_ForTestOnly => JR_GE_ReadOnly;

		public bool IsMainAllFieldsReadonly_ForTestOnly => IsMainAllFieldsReadonly;

		public bool IsAllCostFieldsReadonly_ForTestOnly => IsAllCostFieldsReadonly;

		public bool IsSellWHTFieldReadOnly_ForTestOnly => IsSellWHTFieldReadOnly;

		public void UpdateJR_AT_SellGSTRateReadOnly_ForTestOnly()
		{
			UpdateJR_AT_SellGSTRateReadOnly();
		}

		public void UpdateJR_AT_CostGSTRateReadOnly_ForTestOnly()
		{
			UpdateJR_AT_CostGSTRateReadOnly();
		}

		public void CreateAccrualCore_ForTestOnly()
		{
			CreateAccrualCore();
		}

		public void CreateWIPCore_ForTestOnly()
		{
			CreateWIPCore();
		}

		public bool JR_CostRatingOverrideComment_ReadOnly_ForTestOnly => JR_CostRatingOverrideComment_ReadOnly;

		public bool JR_SellRatingOverrideComment_ReadOnly_ForTestOnly => JR_SellRatingOverrideComment_ReadOnly;

		public bool JR_CostGovtChargeCode_ReadOnly_ForTestOnly => JR_CostGovtChargeCode_ReadOnly;

		public bool JR_GB_SellTaxBranch_ReadOnly_ForTestOnly => JR_GB_SellTaxBranch_ReadOnly;

		public bool JR_GB_CostTaxBranch_ReadOnly_ForTestOnly => JR_GB_CostTaxBranch_ReadOnly;

		public bool PreventReadOnlyFromChangingValues_ForTestOnly
		{
			get { return PreventReadOnlyFromChangingValues; }
			set { PreventReadOnlyFromChangingValues = value; }
		}

		public ZGuid LocalWHTId_ForTestOnly => LocalWHTId;

		public ZGuid CostWHTId_ForTestOnly => CostWHTId;

		public bool EnableLightValidationIfAvailable_ForTestOnly => EnableLightValidationIfAvailable;

		public ZDecimal JR_Cost_LocalGSTAmountHighPrecision_ForTestOnly => JR_Cost_LocalGSTAmountHighPrecision;

		public bool HasReversedAccrual_ForTestOnly => HasReversedAccrual;

		public bool HasReversedWIP_ForTestOnly => HasReversedWIP;

		public IExchangeRateProvider ExchangeRateProvider_ForTestOnly => ExchangeRateProvider;

		public TransactionHeader APTransactionHeader_ForTestOnly => APTransactionHeader;

		public ZDecimal JR_LocalSellInvoiceAmt_ForTestOnly
		{
			get => JR_LocalSellInvoiceAmt;
			set => JR_LocalSellInvoiceAmt = value;
		}

		public static HashSet<ZString> ForeignKeyColumnsAlreadyFixed_ForTestOnly => ForeignKeyColumnsAlreadyFixed;
	}
}

#endif
