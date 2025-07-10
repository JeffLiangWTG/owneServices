using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA
{
	public partial class LIQSubmissionDataColumns
	{
		#region VAT return Submission Functions

		public (string SuccessMessage, string ErrorMessage) SubmitVATDataToGroup()
		{
			var successMessage = string.Empty;
			var errorMessage = ValidateGroupMemberSubmission();

			if (string.IsNullOrEmpty(errorMessage))
			{
				Status = AccTaxReturn.Status.Submitted;
				Factory.Save();
				successMessage = Res.GetString("f4d0145f-947a-44be-a271-bdbf35ba4592", "VAT Return has been submitted successfully to Group.");
			}
			return (successMessage, errorMessage);
		}

		string ValidateGroupMemberSubmission()
		{
			var error = string.Empty;

			if (!IsGroupMemberSubmission)
			{
				ErrorReporter.ReportOnce("LIQSubmissionDataColumns|ValidateGroupMemberSubmission", "'ValidateGroupMemberSubmission' method should not be called for Group Head company.");
				return Res.GetString("DE6E78D0-7333-40E7-AC96-855A6EA4916F", "Cannot submit to the Group on a Group Head company.");
			}

			return error;
		}

		#endregion

		#region AccTaxReturn Creation/Update

		AccTaxReturn CreateOrUpdateTaxReturn(bool isAdjustmentOnly)
		{
			if (ComplianceReport == null)
			{
				return null;
			}

			var taxReturn = TaxReturn ?? Factory.New<AccTaxReturn>();

			taxReturn.ATR_ACR_ComplianceReport = ComplianceReport.PK;
			taxReturn.ATR_GovtReceiptInformation = ReceiptReferenceReceivedFromHMRC;
			taxReturn.ATR_GovtReturnIdentifier = PeriodKey;
			taxReturn.ATR_Status = Status;
			taxReturn.ATR_ReturnType = AccTaxReturn.ReturnType.LIQ;

			taxReturn.ATR_PageFromAP = PageFromAP;
			taxReturn.ATR_PageFromAR = PageFromAR;
			taxReturn.ATR_PageFromLiq = PageFromLiquidazione;
			taxReturn.ATR_PageToAP = PageToAP;
			taxReturn.ATR_PageToAR = PageToAR;
			taxReturn.ATR_PageToLiq = PageToLiquidazione;

			CreateOrUpdateAccVatReturnColumnsFromSubmissionData(Adjustments, AdjustedAmountsGroup, taxReturn.Columns);
			CreateOrUpdateAccVatReturnColumnsFromSubmissionData(ComputedByCW1, AmountsPeriodGroup, taxReturn.Columns);

			return taxReturn;
		}

		void CreateOrUpdateAccVatReturnColumnsFromSubmissionData(LIQSubmissionData submissionData, string groupCode, AccTaxReturnColumnCollection taxReturnColumns)
		{
			if (submissionData != null && !string.IsNullOrEmpty(groupCode) && taxReturnColumns != null)
			{
				var isAdjustmentColumn = groupCode == AdjustedAmountsGroup;

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalVatBaseReceivables
					, submissionData.Box1_TotalVatBaseReceivables
					, reasonCode: isAdjustmentColumn ? ReasonHolder1.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder1.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalVatReceivables
					, submissionData.Box2_TotalVatReceivables
					, reasonCode: isAdjustmentColumn ? ReasonHolder2.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder2.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalVatBasePayables
					, submissionData.Box3_TotalVatBasePayables
					, reasonCode: isAdjustmentColumn ? ReasonHolder3.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder3.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalVatPayablesRecoverable
					, submissionData.Box4_TotalVatPayablesRecoverable
					, reasonCode: isAdjustmentColumn ? ReasonHolder4.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder4.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalVatPayablesNotRecoverable
					, submissionData.Box5_TotalVatPayablesNotRecoverable
					, reasonCode: isAdjustmentColumn ? ReasonHolder5.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder5.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, VatBalanceReceivablesAndPayables
					, submissionData.Box6_VatBalanceReceivablesAndPayables
					, reasonCode: string.Empty
					, comment: string.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, BalancePreviousPeriod
					, submissionData.Box7_BalancePreviousPeriod
					, reasonCode: isAdjustmentColumn ? ReasonHolder7.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder7.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalBalance
					, submissionData.Box8_TotalBalance
					, reasonCode: string.Empty
					, comment: string.Empty);
			}
		}

		static void SetATCValues(AccTaxReturnColumnCollection taxReturnColumns, string groupCode, string columnName, ZDecimal amount, ZString reasonCode, ZString comment)
		{
			var column = GetOrCreateColumn(taxReturnColumns, groupCode, columnName);
			if (column != null)
			{
				column.ATC_Amount = amount;
				column.ATC_Comment = comment;
				column.ATC_ReasonCode = reasonCode;
			}
		}

		static AccTaxReturnColumn GetOrCreateColumn(AccTaxReturnColumnCollection taxReturnColumns, string groupCode, string columnName)
		{
			var atColumn = taxReturnColumns.Cast<AccTaxReturnColumn>().FirstOrDefault(atc => atc.ATC_GroupCode == groupCode && atc.ATC_ColumnName == columnName);
			if (atColumn == null)
			{
				atColumn = taxReturnColumns.AddNew();
				atColumn.ATC_ColumnName = columnName;
				atColumn.ATC_GroupCode = groupCode;
			}
			return atColumn;
		}

		#endregion

		#region Properties 

		public bool IsGroupMemberSubmission => AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany.Value != Guid.Empty;

		public bool IsSubmitted => IsInDatabase && Status == AccTaxReturn.Status.Submitted;

		public override bool IsInDatabase => TaxReturn?.IsInDatabase ?? false;

		public AccTaxReturn TaxReturn => Factory.LoadTop1<AccTaxReturn>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, ComplianceReport.PK));

		#endregion

		//Column Names - ATC Column Name that corresponds to an amount field.
		public const string TotalVatBaseReceivables = "B1_TotalVatBaseReceivables";
		public const string TotalVatReceivables = "B2_TotalVatReceivables";
		public const string TotalVatBasePayables = "B3_TotalVatBasePayables";
		public const string TotalVatPayablesRecoverable = "B4_TotalVatPayablesRecoverable";
		public const string TotalVatPayablesNotRecoverable = "B5_TotalVatPayablesNotRecoverable";
		public const string VatBalanceReceivablesAndPayables = "B6_VatBalanceReceivablesAndPayables";
		public const string BalancePreviousPeriod = "B7_BalancePreviousPeriod";
		public const string TotalBalance = "B8_TotalBalance";

		//Group Names - ATC Column Name that corresponds to an amount field.
		public const string AmountsPeriodGroup = "APR";
		public const string AdjustedAmountsGroup = "ADJ";
		public const string AmountsTotalsGroup = "TOT";
	}
}
