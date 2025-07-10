using System;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	public partial class MTDSubmissionDataColumns
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

		public (string ResponseFromHMRC, string ErrorMessage) TryToSubmitVATDataToHMRC()
		{
			var responseFromHMRC = string.Empty;
			var errorMessage = string.Empty;

			var validationError = ValidateSubmission();
			if (!string.IsNullOrEmpty(validationError))
			{
				errorMessage = validationError;
			}
			else
			{
				var obligations = ComplianceReport.Validation.CheckMTDReportDateRange();
				var accTaxReturn = ComplianceReport.LoadAccTaxReturn();

				//VAT has not been submitted yet.
				if (!string.IsNullOrEmpty(obligations.PeriodKey) && obligations.IsPeriodOpen)
				{
					using (var mutex = GetMutex(obligations.PeriodKey, MTDClient.GetVATRegistrationNumber(ComplianceReport.Company, ComplianceReport.ReportCountryCode)))
					{
						if (mutex.Lock())
						{
							SaveAllAmountsBeforeSubmitting(obligations.PeriodKey);

							var (response, errorSendingSubmitRequest) = SendSubmitVATReturnRequest(obligations.PeriodKey);

							if (response != null && string.IsNullOrEmpty(errorSendingSubmitRequest))
							{
								UpdateReceiptNumberAndStatus(response.ReceiptID);
								responseFromHMRC = Res.GetString("7c06a3a4-e28a-430e-8e26-7ea36cbb9f05", "VAT Return has been submitted successfully.");
							}
							else
							{
								errorMessage = errorSendingSubmitRequest;
							}
						}
						else
						{
							errorMessage = Res.GetString("7cc425d4-ca64-4b8a-8545-360378cdde4a", "Another user is already submitting VAT return to HMRC.");
						}
					}
				}
				//Someoneelse has already submitted VAT return but CW1 does not know about it.
				else if (!string.IsNullOrEmpty(obligations.PeriodKey) && !obligations.IsPeriodOpen && obligations.FulfilledOn.IsValid && !(accTaxReturn?.IsSubmitted ?? false))
				{
					var submittedVATData = GetSubmittedVATDataFromHMRC(ComplianceReport, obligations.PeriodKey);
					errorMessage = Res.GetString("be94e229-192c-44c7-b8ce-10a6293cc675", @"VAT return has already been submitted for this period and HMRC received it on {0}.
{1}"
					, obligations.FulfilledOn.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
					, !string.IsNullOrEmpty(submittedVATData.ResponseFromHMRC) ? submittedVATData.ResponseFromHMRC : submittedVATData.ErrorMessage);
				}
				//all other cases
				else
				{
					errorMessage = obligations.Error;
				}
			}

			return (responseFromHMRC, errorMessage);
		}

		static (string ResponseFromHMRC, string ErrorMessage) GetSubmittedVATDataFromHMRC(AccComplianceReport complianceReport, string periodKey)
		{
			var responseFromHMRC = string.Empty;
			var errorMessage = string.Empty;
			var (response, errorSendingRequest) = SendGetSubmittedVATReturnDataReuqest(complianceReport, periodKey);

			if (response != null && string.IsNullOrEmpty(errorSendingRequest))
			{
				responseFromHMRC = Res.GetString("e85bb64d-8ee5-4f38-b8ae-16e7d18875e8", @"VAT Return data retrieved from HMRC for period {0} to {1}:
{2}", complianceReport.ACR_DateFrom, complianceReport.ACR_DateTo, response);
			}
			else
			{
				errorMessage = Res.GetString("8ac03d39-e62a-45a0-b382-f2849048443b", @"Could not retrieve VAT Return data from HMRC.
{0}", errorSendingRequest);
			}

			return (responseFromHMRC, errorMessage);
		}

		static (MTDVATData Response, string Error) SendGetSubmittedVATReturnDataReuqest(AccComplianceReport complianceReport, string periodKey)
		{
			var errorMessage = string.Empty;
			var client = new MTDClient(complianceReport);
			var request = new MTDGetSubmittedVATDataRequest(client, periodKey);
			var response = client.SendRequest<MTDVATData>(request);

			if (response == null && client.LastErrorInfo != null)
			{
				errorMessage = Res.GetString("00101529-892b-4f80-b319-92d5eada89d2", "Error details:{0}", client.LastErrorInfo.ToString());
			}
			else if (response == null && client.LastErrorInfo == null && client.LastHttpStatusCode.HasValue)
			{
				errorMessage = Res.GetString("23f2a1ba-4cef-4bff-8781-d1d344b17a48", "Status Code:{0}", client.LastHttpStatusCode.Value.ToString());
			}
			else if (response == null && client.LastErrorInfo == null && !client.LastHttpStatusCode.HasValue)
			{
				errorMessage = Res.GetString("83bc753c-82ed-4e4d-b4c3-f782bdd7e814", "An unknown error occurred");
			}

			return (response, errorMessage);
		}

		void SaveAllAmountsBeforeSubmitting(string periodKey)
		{
			using (SaveOnlyAdjustmentData.GetSuspender())
			{
				PeriodKey = periodKey;
				Factory.Save();
			}
		}

		void UpdateReceiptNumberAndStatus(string receiptNumber)
		{
			ReceiptReferenceReceivedFromHMRC = receiptNumber;
			Status = AccTaxReturn.Status.Submitted;
			TaxReturn.ATR_CompanyName = CompanyName;
			TaxReturn.ATR_VATRegNo = GSTRegNo;
			Factory.Save();
		}

		(MTDVATSubmitResponseContent Response, string Error) SendSubmitVATReturnRequest(string periodKey)
		{
			var errorMessage = string.Empty;
			var client = new MTDClient(ComplianceReport);
			var submissionRequest = new MTDPostVATDataRequest(client, GetMTDVATDataForSubmission(periodKey));
			var response = client.SendRequest<MTDVATSubmitResponseContent>(submissionRequest);

			if (response == null && client.LastErrorInfo != null)
			{
				errorMessage = Res.GetString("3c08bc79-c153-44b3-8d59-a499d9bbd3a6", "Could not submit VAT Return. Error details:\r\n{0}", client.LastErrorInfo.ToString());
			}
			else if (response == null && client.LastErrorInfo == null && client.LastHttpStatusCode.HasValue)
			{
				errorMessage = Res.GetString("5c0b458f-9768-40b7-b915-f073afd8d6ac", "Could not submit VAT Return. Status Code:{0}", client.LastHttpStatusCode.Value.ToString());
			}
			else if (response == null && client.LastErrorInfo == null && !client.LastHttpStatusCode.HasValue)
			{
				errorMessage = Res.GetString("62d98642-814d-4a2a-85d6-88ddaade6aae", "Could not submit VAT Return. An unknown error occurred");
			}

			return (response, errorMessage);
		}

		string ValidateGroupMemberSubmission()
		{
			var error = string.Empty;

			if (!IsGroupMemberSubmission)
			{
				ErrorReporter.ReportOnce("MTDSubmissionDataColumns|ValidateGroupMemberSubmission", "'ValidateGroupMemberSubmission' method should not be called for Group Head company.");
				return Res.GetString("9e910990-efd1-4fb4-89a0-a4a7ffb9fd7b", "Cannot submit to the Group on a Group Head company.");
			}
			else if (!CanBeSubmittedToHMRC)
			{
				return Res.GetString("cf49cdbb-b7d1-458a-887e-3c41763c1185", "{0} is in status: {1}. Report must be in {2} status to allow submission of VAT data to Group", ComplianceReport.ACR_Description, ComplianceReport.ACR_Status, AccComplianceReport.Status.ReportFinalised);
			}

			return error;
		}

		string ValidateSubmission()
		{
			var error = string.Empty;

			if (IsGroupMemberSubmission)
			{
				ErrorReporter.ReportOnce("MTDSubmissionDataColumns|ValidateSubmission", "'ValidateSubmission' method should not be called for Group Member company.");
				return Res.GetString("f5341212-f0d6-496f-abf5-e0da71486beb", "Cannot submit to the HMRC on a Group Member company.");
			}
			else if (!CanBeSubmittedToHMRC)
			{
				error = Res.GetString("831789a0-2028-45d5-aa05-3ec937945287", "{0} is in status: {1}. Report must be in {2} status to allow submission of VAT data to HMRC", ComplianceReport.ACR_Description, ComplianceReport.ACR_Status, AccComplianceReport.Status.ReportFinalised);
			}
			else if (!Declaration)
			{
				error = Res.GetString("3ef56724-be9b-4026-83dc-17ebf1cc654d", "Please confirm declaration before continuing.");
			}

			return error;
		}

		MTDVATData GetMTDVATDataForSubmission(string periodKey)
		{
			int decimalPlaces = DecimalPlaces;
			return new MTDVATData()
			{
				finalised = true,
				periodKey = periodKey,
				vatDueSales = Utilities.Round(valuesToSubmitToHMRC.Box1_VATDue, decimalPlaces),
				vatDueAcquisitions = Utilities.Round(valuesToSubmitToHMRC.Box2_VATDueReverseChg, decimalPlaces),
				totalVatDue = Utilities.Round(valuesToSubmitToHMRC.Box3_TotalVATDue, decimalPlaces),
				vatReclaimedCurrPeriod = Utilities.Round(valuesToSubmitToHMRC.Box4_VATReclaimed, decimalPlaces),
				netVatDue = Utilities.Round(valuesToSubmitToHMRC.Box5_NetVAT, decimalPlaces),

				//HMRC does not accept decimal values for the following fields
				totalValueSalesExVAT = Utilities.Round(valuesToSubmitToHMRC.Box6_TotalSalesExVAT, 0),
				totalValuePurchasesExVAT = Utilities.Round(valuesToSubmitToHMRC.Box7_TotalPurchaseExVAT, 0),
				totalValueGoodsSuppliedExVAT = Utilities.Round(valuesToSubmitToHMRC.Box8_GoodsSalesECMembersExVAT, 0),
				totalAcquisitionsExVAT = Utilities.Round(valuesToSubmitToHMRC.Box9_GoodsPurchaseECMembersExVAT, 0)
			};
		}

		static ZGlobalMutex GetMutex(string periodKey, string vrn)
		{
			return new ZGlobalMutex(MutexIDs.MTDVATSubmission, FormattableString.Invariant($"MTD_{periodKey}_{vrn}"));
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

			if (!isAdjustmentOnly)
			{
				CreateOrUpdateAccTaxReturnColumnsFromSubmissionData(ComputedByCW1, ReportPeriodGroup, taxReturn.Columns);
				CreateOrUpdateAccTaxReturnColumnsFromSubmissionData(UnsubmitedPreviousValues, ErrorsMadeInPreviousPeriodsGroup, taxReturn.Columns);
			}

			CreateOrUpdateAccTaxReturnColumnsFromSubmissionData(Adjustments, AdjustedAmountsGroup, taxReturn.Columns);

			if (!isAdjustmentOnly)
			{
				if (!IsGroupMemberSubmission)
				{
					CreateOrUpdateAccTaxReturnColumnsFromSubmissionData(GroupMemberTotal, GroupMemberCompaniesTotalGroup, taxReturn.Columns);
				}

				CreateOrUpdateAccTaxReturnColumnsFromSubmissionData(ValuesToSubmitToHMRC, AmountsToBeSubmittedToHMRCGroup, taxReturn.Columns);
			}
			return taxReturn;
		}

		void CreateOrUpdateAccTaxReturnColumnsFromSubmissionData(MTDSubmissionData submissionData, string groupCode, AccTaxReturnColumnCollection taxReturnColumns)
		{
			if (submissionData != null && !string.IsNullOrEmpty(groupCode) && taxReturnColumns != null)
			{
				var isAdjustmentColumn = groupCode == AdjustedAmountsGroup;

				SetATCValues(taxReturnColumns
					, groupCode
					, VatDueSales
					, submissionData.Box1_VATDue
					, reasonCode: isAdjustmentColumn ? ReasonHolder1.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder1.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, VatDueAcquisitions
					, submissionData.Box2_VATDueReverseChg
					, reasonCode: isAdjustmentColumn ? ReasonHolder2.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder2.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalVatDue
					, submissionData.Box3_TotalVATDue
					, reasonCode: string.Empty
					, comment: string.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, VatReclaimedCurrPeriod
					, submissionData.Box4_VATReclaimed
					, reasonCode: isAdjustmentColumn ? ReasonHolder4.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder4.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, NetVatDue
					, submissionData.Box5_NetVAT
					, reasonCode: string.Empty
					, comment: string.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalValueSalesExVAT
					, submissionData.Box6_TotalSalesExVAT
					, reasonCode: isAdjustmentColumn ? ReasonHolder6.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder6.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalValuePurchasesExVAT
					, submissionData.Box7_TotalPurchaseExVAT
					, reasonCode: isAdjustmentColumn ? ReasonHolder7.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder7.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalValueGoodsSuppliedExVAT
					, submissionData.Box8_GoodsSalesECMembersExVAT
					, reasonCode: isAdjustmentColumn ? ReasonHolder8.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder8.Reason : ZString.Empty);

				SetATCValues(taxReturnColumns
					, groupCode
					, TotalAcquisitionsExVAT
					, submissionData.Box9_GoodsPurchaseECMembersExVAT
					, reasonCode: isAdjustmentColumn ? ReasonHolder9.Code : ZString.Empty
					, comment: isAdjustmentColumn ? ReasonHolder9.Reason : ZString.Empty);
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

		public bool CanBeSubmittedToHMRC
						=> ComplianceReport.IsInDatabase &&
						ComplianceReport.ACR_IsFinalised &&
						!IsSubmitted &&
						(ReturnDueDate.IsValid || IsGroupMemberSubmission) &&
						Env.Security.FinalizeComplianceReport.IsAllowed &&
						(IsGroupMemberSubmission || HaveAllGroupMembersSubmittedReturn);

		public override bool IsInDatabase => TaxReturn?.IsInDatabase ?? false;

		public AccTaxReturn TaxReturn => Factory.LoadTop1<AccTaxReturn>(new ZQuery(AccTaxReturnSchema.ATR_ACR_ComplianceReport, ComplianceReport.PK));

		bool HaveAllGroupMembersSubmittedReturn
		{
			get
			{
				if (!fHaveAllGroupMembersSubmittedReturn.HasValue)
				{
					var helper = new MTDSubmissionDataHelper(ComplianceReport);
					fHaveAllGroupMembersSubmittedReturn = helper.HaveAllGroupMembersSubmittedReturn();
				}
				return fHaveAllGroupMembersSubmittedReturn.Value;
			}
		}
		bool? fHaveAllGroupMembersSubmittedReturn;

		int DecimalPlaces => ComplianceReport.Company.GetLocalDecimals();

		#endregion

		//Column Names - ATC Column Name that corresponds to an amount field submitted to HMRC
		public const string VatDueSales = "B1_VatDueSales";
		public const string VatDueAcquisitions = "B2_VatDueAcquisitions";
		public const string TotalVatDue = "B3_TotalVatDue";
		public const string VatReclaimedCurrPeriod = "B4_VatReclaimedCurrPeriod";
		public const string NetVatDue = "B5_netVatDue";
		public const string TotalValueSalesExVAT = "B6_totalValueSalesExVAT";
		public const string TotalValuePurchasesExVAT = "B7_TotalValuePurchasesExVAT";
		public const string TotalValueGoodsSuppliedExVAT = "B8_TotalValueGoodsSuppliedExVAT";
		public const string TotalAcquisitionsExVAT = "B9_TotalAcquisitionsExVAT";

		//Group Names - ATC Group Name that corresponds to a group of amount fields submitted to HMRC.
		public const string ReportPeriodGroup = "RPR";
		public const string ErrorsMadeInPreviousPeriodsGroup = "ERR";
		public const string AdjustedAmountsGroup = "ADJ";
		public const string GroupMemberCompaniesTotalGroup = "GMC";
		public const string AmountsToBeSubmittedToHMRCGroup = "SUB";
	}
}
