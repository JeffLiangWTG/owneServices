using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA
{
	public static class LIQSubmissionDataColumnsAdapter
	{
		public static AccTaxReturn LoadAccTaxReturn(this AccComplianceReport complianceReport)
		{
			return complianceReport.Factory.LoadTop1<AccTaxReturn>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, complianceReport.PK));
		}

		public static LIQSubmissionDataColumns LoadLIQSubmissionData(this AccComplianceReport complianceReport)
		{
			var factory = complianceReport.Factory;
			var taxReturn = LoadAccTaxReturn(complianceReport);
			if (taxReturn == null)
			{
				return null;
			}

			var submissionDataColumns = new LIQSubmissionDataColumns(factory, complianceReport);

			using (submissionDataColumns.SuspendSettingHasChangesIncludingChildren())
			{
				submissionDataColumns.PageFromAP = taxReturn.ATR_PageFromAP;
				submissionDataColumns.PageFromAR = taxReturn.ATR_PageFromAR;
				submissionDataColumns.PageFromLiquidazione = taxReturn.ATR_PageFromLiq;
				submissionDataColumns.PageToAP = taxReturn.ATR_PageToAP;
				submissionDataColumns.PageToAR = taxReturn.ATR_PageToAR;
				submissionDataColumns.PageToLiquidazione = taxReturn.ATR_PageToLiq;

				PopulateLIQSubmissionDataFromAccTaxReturnColumns(taxReturn.Columns, LIQSubmissionDataColumns.AmountsPeriodGroup, submissionDataColumns.ComputedByCW1);
				PopulateLIQSubmissionDataFromAccTaxReturnColumns(taxReturn.Columns, LIQSubmissionDataColumns.AdjustedAmountsGroup, submissionDataColumns.Adjustments);
				PopulateReasonCode(taxReturn.Columns, submissionDataColumns);
			}
			return submissionDataColumns;
		}

		static void PopulateLIQSubmissionDataFromAccTaxReturnColumns(AccTaxReturnColumnCollection taxReturnColumns, string groupCode, LIQSubmissionData submissionData)
		{
			if (submissionData != null && !string.IsNullOrEmpty(groupCode) && taxReturnColumns != null)
			{
				var vatDueSalesColumn = GetATCColumn(taxReturnColumns, groupCode, LIQSubmissionDataColumns.TotalVatBaseReceivables);
				submissionData.Box1_TotalVatBaseReceivables = vatDueSalesColumn?.ATC_Amount ?? ZDecimal.Zero;

				var vatDueAcquisitionsColumn = GetATCColumn(taxReturnColumns, groupCode, LIQSubmissionDataColumns.TotalVatReceivables);
				submissionData.Box2_TotalVatReceivables = vatDueAcquisitionsColumn?.ATC_Amount ?? ZDecimal.Zero;

				var vatReclaimedCurrPeriodColumn = GetATCColumn(taxReturnColumns, groupCode, LIQSubmissionDataColumns.TotalVatBasePayables);
				submissionData.Box3_TotalVatBasePayables = vatReclaimedCurrPeriodColumn?.ATC_Amount ?? ZDecimal.Zero;

				var totalValueSalesExVATColumn = GetATCColumn(taxReturnColumns, groupCode, LIQSubmissionDataColumns.TotalVatPayablesRecoverable);
				submissionData.Box4_TotalVatPayablesRecoverable = totalValueSalesExVATColumn?.ATC_Amount ?? ZDecimal.Zero;

				var totalValuePurchasesExVATColumn = GetATCColumn(taxReturnColumns, groupCode, LIQSubmissionDataColumns.TotalVatPayablesNotRecoverable);
				submissionData.Box5_TotalVatPayablesNotRecoverable = totalValuePurchasesExVATColumn?.ATC_Amount ?? ZDecimal.Zero;

				var totalValueGoodsSuppliedExVATColumn = GetATCColumn(taxReturnColumns, groupCode, LIQSubmissionDataColumns.BalancePreviousPeriod);
				submissionData.Box7_BalancePreviousPeriod = totalValueGoodsSuppliedExVATColumn?.ATC_Amount ?? ZDecimal.Zero;
			}
		}

		static void PopulateReasonCode(AccTaxReturnColumnCollection taxReturnColumns, LIQSubmissionDataColumns submissionDataColumns)
		{
			if (submissionDataColumns != null && taxReturnColumns != null)
			{
				var column = GetATCColumn(taxReturnColumns, LIQSubmissionDataColumns.AdjustedAmountsGroup, LIQSubmissionDataColumns.TotalVatBaseReceivables);
				submissionDataColumns.ReasonHolder1.Code = column?.ATC_ReasonCode ?? ZString.Empty;
				submissionDataColumns.ReasonHolder1.Reason = column?.ATC_Comment ?? ZString.Empty;

				column = GetATCColumn(taxReturnColumns, LIQSubmissionDataColumns.AdjustedAmountsGroup, LIQSubmissionDataColumns.TotalVatReceivables);
				submissionDataColumns.ReasonHolder2.Code = column?.ATC_ReasonCode ?? ZString.Empty;
				submissionDataColumns.ReasonHolder2.Reason = column?.ATC_Comment ?? ZString.Empty;

				column = GetATCColumn(taxReturnColumns, LIQSubmissionDataColumns.AdjustedAmountsGroup, LIQSubmissionDataColumns.TotalVatBasePayables);
				submissionDataColumns.ReasonHolder3.Code = column?.ATC_ReasonCode ?? ZString.Empty;
				submissionDataColumns.ReasonHolder3.Reason = column?.ATC_Comment ?? ZString.Empty;

				column = GetATCColumn(taxReturnColumns, LIQSubmissionDataColumns.AdjustedAmountsGroup, LIQSubmissionDataColumns.TotalVatPayablesRecoverable);
				submissionDataColumns.ReasonHolder4.Code = column?.ATC_ReasonCode ?? ZString.Empty;
				submissionDataColumns.ReasonHolder4.Reason = column?.ATC_Comment ?? ZString.Empty;

				column = GetATCColumn(taxReturnColumns, LIQSubmissionDataColumns.AdjustedAmountsGroup, LIQSubmissionDataColumns.TotalVatPayablesNotRecoverable);
				submissionDataColumns.ReasonHolder5.Code = column?.ATC_ReasonCode ?? ZString.Empty;
				submissionDataColumns.ReasonHolder5.Reason = column?.ATC_Comment ?? ZString.Empty;

				column = GetATCColumn(taxReturnColumns, LIQSubmissionDataColumns.AdjustedAmountsGroup, LIQSubmissionDataColumns.BalancePreviousPeriod);
				submissionDataColumns.ReasonHolder7.Code = column?.ATC_ReasonCode ?? ZString.Empty;
				submissionDataColumns.ReasonHolder7.Reason = column?.ATC_Comment ?? ZString.Empty;
			}
		}

		static AccTaxReturnColumn GetATCColumn(AccTaxReturnColumnCollection taxReturnColumns, string groupCode, string columnName)
		{
			return taxReturnColumns.Cast<AccTaxReturnColumn>().FirstOrDefault(atc => atc.ATC_GroupCode == groupCode && atc.ATC_ColumnName == columnName);
		}
	}
}
