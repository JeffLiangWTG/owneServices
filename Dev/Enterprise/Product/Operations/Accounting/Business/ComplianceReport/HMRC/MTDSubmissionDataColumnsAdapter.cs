using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	public static class MTDSubmissionDataColumnsAdapter
	{
		public static AccTaxReturn LoadAccTaxReturn(this AccComplianceReport complianceReport)
		{
			return complianceReport.Factory.LoadTop1<AccTaxReturn>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, complianceReport.PK));
		}

		public static MTDSubmissionDataColumns LoadMTDSubmissionData(this AccComplianceReport complianceReport)
		{
			var factory = complianceReport.Factory;
			var taxReturn = factory.LoadTop1<AccTaxReturn>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, complianceReport.PK));
			if (taxReturn == null)
			{
				return null;
			}

			var submissionDataColumns = new MTDSubmissionDataColumns(factory, complianceReport);
			using (submissionDataColumns.SuspendSettingHasChangesIncludingChildren())
			{
				submissionDataColumns.Status = taxReturn.ATR_Status;
				if (taxReturn.ATR_Status == AccTaxReturn.Status.Submitted)
				{
					submissionDataColumns.Declaration = true;
					submissionDataColumns.ReceiptReferenceReceivedFromHMRC = taxReturn.ATR_GovtReceiptInformation;
				}

				PopulateMTDSubmissionDataFromAccTaxReturnColumns(taxReturn.Columns, MTDSubmissionDataColumns.ReportPeriodGroup, submissionDataColumns.ComputedByCW1);
				PopulateMTDSubmissionDataFromAccTaxReturnColumns(taxReturn.Columns, MTDSubmissionDataColumns.ErrorsMadeInPreviousPeriodsGroup, submissionDataColumns.UnsubmitedPreviousValues);
				PopulateMTDSubmissionDataFromAccTaxReturnColumns(taxReturn.Columns, MTDSubmissionDataColumns.AdjustedAmountsGroup, submissionDataColumns.Adjustments);
				PopulateMTDSubmissionDataFromAccTaxReturnColumns(taxReturn.Columns, MTDSubmissionDataColumns.GroupMemberCompaniesTotalGroup, submissionDataColumns.GroupMemberTotal);
				PopulateMTDSubmissionDataFromAccTaxReturnColumns(taxReturn.Columns, MTDSubmissionDataColumns.AmountsToBeSubmittedToHMRCGroup, submissionDataColumns.ValuesToSubmitToHMRC);
				PopulateReasonCode(taxReturn.Columns, submissionDataColumns);
			}
			return submissionDataColumns;
		}

		static void PopulateMTDSubmissionDataFromAccTaxReturnColumns(AccTaxReturnColumnCollection taxReturnColumns, string groupCode, MTDSubmissionData submissionData)
		{
			if (submissionData != null && !string.IsNullOrEmpty(groupCode) && taxReturnColumns != null)
			{
				var vatDueSalesColumn = GetATCValue(taxReturnColumns, groupCode, MTDSubmissionDataColumns.VatDueSales);
				submissionData.Box1_VATDue = vatDueSalesColumn?.ATC_Amount ?? ZDecimal.Zero;

				var vatDueAcquisitionsColumn = GetATCValue(taxReturnColumns, groupCode, MTDSubmissionDataColumns.VatDueAcquisitions);
				submissionData.Box2_VATDueReverseChg = vatDueAcquisitionsColumn?.ATC_Amount ?? ZDecimal.Zero;

				var vatReclaimedCurrPeriodColumn = GetATCValue(taxReturnColumns, groupCode, MTDSubmissionDataColumns.VatReclaimedCurrPeriod);
				submissionData.Box4_VATReclaimed = vatReclaimedCurrPeriodColumn?.ATC_Amount ?? ZDecimal.Zero;

				var totalValueSalesExVATColumn = GetATCValue(taxReturnColumns, groupCode, MTDSubmissionDataColumns.TotalValueSalesExVAT);
				submissionData.Box6_TotalSalesExVAT = totalValueSalesExVATColumn?.ATC_Amount ?? ZDecimal.Zero;

				var totalValuePurchasesExVATColumn = GetATCValue(taxReturnColumns, groupCode, MTDSubmissionDataColumns.TotalValuePurchasesExVAT);
				submissionData.Box7_TotalPurchaseExVAT = totalValuePurchasesExVATColumn?.ATC_Amount ?? ZDecimal.Zero;

				var totalValueGoodsSuppliedExVATColumn = GetATCValue(taxReturnColumns, groupCode, MTDSubmissionDataColumns.TotalValueGoodsSuppliedExVAT);
				submissionData.Box8_GoodsSalesECMembersExVAT = totalValueGoodsSuppliedExVATColumn?.ATC_Amount ?? ZDecimal.Zero;

				var totalAcquisitionsExVATColumn = GetATCValue(taxReturnColumns, groupCode, MTDSubmissionDataColumns.TotalAcquisitionsExVAT);
				submissionData.Box9_GoodsPurchaseECMembersExVAT = totalAcquisitionsExVATColumn?.ATC_Amount ?? ZDecimal.Zero;
			}
		}

		static void PopulateReasonCode(AccTaxReturnColumnCollection taxReturnColumns, MTDSubmissionDataColumns submissionDataColumns)
		{
			var column = GetATCValue(taxReturnColumns, MTDSubmissionDataColumns.AdjustedAmountsGroup , MTDSubmissionDataColumns.VatDueSales);
			submissionDataColumns.ReasonHolder1.Code = column.ATC_ReasonCode;
			submissionDataColumns.ReasonHolder1.Reason = column.ATC_Comment;

			column = GetATCValue(taxReturnColumns, MTDSubmissionDataColumns.AdjustedAmountsGroup, MTDSubmissionDataColumns.VatDueAcquisitions);
			submissionDataColumns.ReasonHolder2.Code = column.ATC_ReasonCode;
			submissionDataColumns.ReasonHolder2.Reason = column.ATC_Comment;

			column = GetATCValue(taxReturnColumns, MTDSubmissionDataColumns.AdjustedAmountsGroup, MTDSubmissionDataColumns.VatReclaimedCurrPeriod);
			submissionDataColumns.ReasonHolder4.Code = column.ATC_ReasonCode;
			submissionDataColumns.ReasonHolder4.Reason = column.ATC_Comment;

			column = GetATCValue(taxReturnColumns, MTDSubmissionDataColumns.AdjustedAmountsGroup, MTDSubmissionDataColumns.TotalValueSalesExVAT);
			submissionDataColumns.ReasonHolder6.Code = column.ATC_ReasonCode;
			submissionDataColumns.ReasonHolder6.Reason = column.ATC_Comment;

			column = GetATCValue(taxReturnColumns, MTDSubmissionDataColumns.AdjustedAmountsGroup, MTDSubmissionDataColumns.TotalValuePurchasesExVAT);
			submissionDataColumns.ReasonHolder7.Code = column.ATC_ReasonCode;
			submissionDataColumns.ReasonHolder7.Reason = column.ATC_Comment;

			column = GetATCValue(taxReturnColumns, MTDSubmissionDataColumns.AdjustedAmountsGroup, MTDSubmissionDataColumns.TotalValueGoodsSuppliedExVAT);
			submissionDataColumns.ReasonHolder8.Code = column.ATC_ReasonCode;
			submissionDataColumns.ReasonHolder8.Reason = column.ATC_Comment;

			column = GetATCValue(taxReturnColumns, MTDSubmissionDataColumns.AdjustedAmountsGroup, MTDSubmissionDataColumns.TotalAcquisitionsExVAT);
			submissionDataColumns.ReasonHolder9.Code = column.ATC_ReasonCode;
			submissionDataColumns.ReasonHolder9.Reason = column.ATC_Comment;
		}

		static AccTaxReturnColumn GetATCValue(AccTaxReturnColumnCollection taxReturnColumns, string groupCode, string columnName)
		{
			return taxReturnColumns.Cast<AccTaxReturnColumn>().FirstOrDefault(atc => atc.ATC_GroupCode == groupCode && atc.ATC_ColumnName == columnName);
		}
	}
}
