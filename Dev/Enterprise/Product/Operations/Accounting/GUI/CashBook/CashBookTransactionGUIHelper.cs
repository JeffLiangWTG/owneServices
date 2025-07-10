using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.GUI.CashBook
{
	public static class CashBookTransactionGUIHelper
	{
		public static void ConfigureColumns(ZGrid grid, ZString transactionType, DirectTransactionHeaderBase directTransactionHeaderBase)
		{
			RemoveCommonColumns(grid);
			RemoveTaxColumns(grid, transactionType, directTransactionHeaderBase);
			ConfigureExtraTaxColumnsCaption(grid);
		}

		static void RemoveCommonColumns(ZGrid grid)
		{
			if (!AccountingMasterFilesRegistry.Instance.EnableGovernmentChargeCode.Value)
			{
				grid.RemoveFromAvailableColumns(DirectTransactionLineBase.Schema.AL_GovtChargeCode);
			}

			if (!PlaceOfSupplyListProvider.IsPlaceOfSupplyApplicable(GlbCompany.CurrentCompany))
			{
				grid.RemoveFromAvailableColumns(DirectTransactionLineBase.Schema.AL_PlaceOfSupply);
			}

			if (!AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value)
			{
				grid.RemoveFromAvailableColumns(DirectTransactionLineBase.Schema.AL_SupplyType);
			}
		}

		static void RemoveTaxColumns(ZGrid grid, ZString transactionType, DirectTransactionHeaderBase directTransactionHeaderBase)
		{
			var shouldKeepTaxColumns = (directTransactionHeaderBase != null && directTransactionHeaderBase.IsTaxed)
				|| GlbCompany.CurrentCompany.GC_IsGSTRegistered;

			if (transactionType != TransactionTypes.DirectPayment || !shouldKeepTaxColumns)
			{
				grid.RemoveFromAvailableColumns(DirectTransactionLineBase.Schema.AL_Calc_InputGSTVATRecoverablePercentage,
												DirectTransactionLineBase.Schema.AL_OSTaxAmount_Recoverable,
												DirectTransactionLineBase.Schema.AL_OSTaxAmount_NotRecoverable,
												DirectTransactionLineBase.Schema.AL_LocalTaxAmount_Recoverable,
												DirectTransactionLineBase.Schema.AL_LocalTaxAmount_NotRecoverable);
			}

			if (!shouldKeepTaxColumns)
			{
				grid.RemoveFromAvailableColumns(DirectTransactionLineBase.Schema.AL_AT,
												DirectTransactionLineBase.Schema.AL_TaxDate,
												DirectTransactionLineBase.Schema.AL_A9_VATClass,
												DirectTransactionLineBase.Schema.AL_OSTaxAmount,
												DirectTransactionLineBase.Schema.AL_LocalTaxAmount);
			}

			if (!GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				grid.RemoveFromAvailableColumns(DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount,
												DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount,
												DirectTransactionLineBase.Schema.AL_OSGSTAmount,
												DirectTransactionLineBase.Schema.AL_LocalGSTAmount);
			}
		}

		static void ConfigureExtraTaxColumnsCaption(ZGrid grid)
		{
			if (GlbCompany.CurrentCompany.IsExtraTaxApplicable())
			{
				grid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_OSExtraTaxAmount).CaptionResourceString = AccountingCaptionHelper.OSExtraTaxAmountCaption;
				grid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_LocalExtraTaxAmount).CaptionResourceString = AccountingCaptionHelper.LocalExtraTaxAmountCaption;
				grid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_LocalGSTAmount).CaptionResourceString = AccountingCaptionHelper.LocalTaxAmountCaption;
				grid.GetColumnStyle(DirectTransactionLineBase.Schema.AL_OSGSTAmount).CaptionResourceString = AccountingCaptionHelper.OSTaxAmountCaption;
			}
		}
	}
}
