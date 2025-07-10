using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing
{
	internal class EventAndDatabaseTransaction
	{
		internal string AuthRecordType { get; set; }
		internal string CompanyName { get; set; }
		internal Type ParentType { get; set; }
		internal UniversalEvent UniversalEvent { get; set; }
		internal AccEInvoicingBatch Batch { get; set; }
		internal UniversalEventTransactionDataObject EventData { get; set; }
		internal AccEInvoicingTransactionPivot Pivot { get; set; }
		internal InvoicingBase Transaction { get; set; }
		internal AccTransactionHeaderAuthorisationRecord AuthRecord { get; set; }

		string TransactionIdentifier => FormattableString.Invariant($"{Transaction.AH_Ledger} {Transaction.AH_TransactionType} {Transaction.AH_TransactionNum}");  // Concatenation of database fields. Always used in ResString.

		bool IsSingleTransactionBatch => ParentType == typeof(GlobalEInvoicingEventMessageProcessor);
		bool IsMultiTransactionBatch => ParentType == typeof(TransactionBatchEventMessageProcessor);

		string BatchNumber => Batch?.AIB_BatchNumber.ToString() ?? string.Empty;

		internal bool ValidatePivotAndBatch(IXmlSessionTracker logger)
		{
			var batchNumber = Batch.AIB_BatchNumber;

			if (Batch.AIB_Status == EInvoicingBatchState.Discarded)
			{
				return false;
			}

			if (EventData == null)
			{
				var transactionIdentifierSafe = FormattableString.Invariant($"{Transaction?.AH_Ledger} {Transaction?.AH_TransactionType} {Transaction?.AH_TransactionNum}");  // Concatenation of database fields. Used in ResString below.
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|EventDataNotFound", "Universal event was missing transaction in invoice batch {0}, transaction {1}, in {2}. No changes were made to this transaction.", batchNumber, transactionIdentifierSafe, CompanyName));
				return false;
			}
			if (Pivot == null && IsSingleTransactionBatch)
			{
				ReportAndLogError("PivotNotFound", Res.GetString("GlobalEInvoicingEventMessageProcessor|PivotNotFound", "No update performed due to transaction pivot not found for invoice batch {0} in {1}.", batchNumber, CompanyName));
				return false;
			}
			if (Transaction == null && IsSingleTransactionBatch)
			{
				ReportAndLogError("TransactionNotFound", Res.GetString("GlobalEInvoicingEventMessageProcessor|TransactionNotFound", "No update performed due to related transaction was not found for pivot for invoice batch {0} in {1}.", batchNumber, CompanyName));
				return false;
			}
			if ((Pivot == null || Transaction == null) && IsMultiTransactionBatch)
			{
				var transactionIdentifierFromEventData = FormattableString.Invariant($"{EventData.TransactionLedger} {EventData.TransactionType} {EventData.TransactionNumber}");  // Concatenation of database fields. Used in ResString below.
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|PivotOrTransactionMissing", "Universal event contained transaction not in invoice batch {0}, transaction {1}, in {2}. No changes were made to this transaction.", batchNumber, transactionIdentifierFromEventData, CompanyName));
				return false;
			}

			if (Pivot.AIP_Status == EInvoicingPivotState.Succeed
				|| Pivot.AIP_Status == EInvoicingPivotState.Discarded
				|| Pivot.AIP_Status == EInvoicingPivotState.Failed)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|CompletedPivotStatus", "No update performed due to transaction pivot having '{0}' status for invoice batch {1}, transaction {2}, in {3}.", Pivot.AIP_Status, batchNumber, TransactionIdentifier, CompanyName));
				return false;
			}
			if (string.IsNullOrEmpty(EventData.PivotStatus))
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|MissingPivotStatus", "Unable to update transaction as {0} is missing for invoice batch {1}, transaction {2}, in {3}.", EventContextTypeCode.AIP_Status, batchNumber, TransactionIdentifier, CompanyName));
				return false;
			}

			if (IsSingleTransactionBatch)
			{
				var eventType = UniversalEvent.EventType ?? ZString.Empty;
				if ((eventType == AutoEvents.InterchangeAcknowledgedCode && EventData.PivotStatus == EInvoicingPivotState.Failed)
					|| (eventType == AutoEvents.InterchangeRejectedCode && EventData.PivotStatus == EInvoicingPivotState.Succeed))
				{
					logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|InvalidPivotStatus", "No update performed as {0} can't have {1} as '{2}' status for invoice batch {3}, transaction {4}, in {5}.", eventType, EventContextTypeCode.AIP_Status, EventData.PivotStatus, batchNumber, TransactionIdentifier, CompanyName));
					return false;
				}
			}

			return true;

			#region Validation Helper

			void ReportAndLogError(string key, string message)
				=> LoggerWrapper.ReportAndLogError(logger, LogType.Error, message, FormattableString.Invariant($"{ParentType.Name}_{key}"));

			#endregion
		}

		internal void MapGovernmentAllocatedIdToDatabase(IXmlSessionTracker logger)
		{
			if (!Transaction.AH_GovernmentAllocatedID.IsEmpty && !string.IsNullOrEmpty(EventData.GovernmentAllocatedId) && !string.Equals(Transaction.AH_GovernmentAllocatedID, EventData.GovernmentAllocatedId, StringComparison.OrdinalIgnoreCase))
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|GovernmentAllocatedIdOverride", "Government Allocated ID can't be overridden. Existing value: '{0}' & New value: '{1}' for invoice batch {2}, transaction {3}, in {4}.", Transaction.AH_GovernmentAllocatedID, EventData.GovernmentAllocatedId, BatchNumber, TransactionIdentifier, CompanyName));
				return;
			}
			if (!string.IsNullOrEmpty(EventData.GovernmentAllocatedId) && EventData.PivotStatus == EInvoicingPivotState.Failed)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|GovernmentAllocatedIdFailed", "Government Allocated ID can't be updated as {0} is status '{1}' for invoice batch {2}, transaction {3}, in {4}.", EventContextTypeCode.AIP_Status, EInvoicingPivotState.Failed, BatchNumber, TransactionIdentifier, CompanyName));
				return;
			}

			if (!string.IsNullOrEmpty(EventData.GovernmentAllocatedId))
			{
				Transaction.AH_GovernmentAllocatedID = EventData.GovernmentAllocatedId;
			}
		}

		internal void MapComplianceNumberToDatabase(IXmlSessionTracker logger)
		{
			if (!Transaction.AH_TransactionReference.IsEmpty && !string.IsNullOrEmpty(EventData.ComplianceNumber) && !string.Equals(Transaction.AH_TransactionReference, EventData.ComplianceNumber, StringComparison.OrdinalIgnoreCase))
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|ComplianceNumberOverride", "Compliance Number can't be overridden. Existing value: '{0}' & New value: '{1}' for invoice batch {2}, transaction {3}, in {4}.", Transaction.AH_TransactionReference, EventData.ComplianceNumber, BatchNumber, TransactionIdentifier, CompanyName));
				return;
			}
			if (!string.IsNullOrEmpty(EventData.ComplianceNumber) && EventData.PivotStatus == EInvoicingPivotState.Failed)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|ComplianceNumberFailed", "Compliance Number can't be updated as {0} is status '{1}' for invoice batch {2}, transaction {3}, in {4}.", EventContextTypeCode.AIP_Status, EInvoicingPivotState.Failed, BatchNumber, TransactionIdentifier, CompanyName));
				return;
			}

			if (!string.IsNullOrEmpty(EventData.ComplianceNumber))
			{
				Transaction.AH_TransactionReference = EventData.ComplianceNumber;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1122:DoNotUseDateTimeParse", Justification = "Baseline")]
		internal void MapComplianceDateToDatabase(IXmlSessionTracker logger)
		{
			if (!Transaction.AH_ComplianceDocumentDate.IsEmpty && !string.IsNullOrEmpty(EventData.ComplianceDate) && !string.Equals(Transaction.AH_ComplianceDocumentDate.ToString(), EventData.ComplianceDate, StringComparison.OrdinalIgnoreCase))
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|ComplianceDateOverride", "Compliance Date can't be overridden. Existing value: '{0}' & New value: '{1}' for invoice batch {2}, transaction {3}, in {4}.", Transaction.AH_ComplianceDocumentDate, EventData.ComplianceDate, BatchNumber, Transaction.AH_TransactionNum, Transaction.Company.CompanyName));
				return;
			}

			if (!string.IsNullOrEmpty(EventData.ComplianceDate) && EventData.PivotStatus == EInvoicingPivotState.Failed)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|ComplianceDateFailed", "Compliance Date can't be updated as {0} is status '{1}' for invoice batch {2}, transaction {3}, in {4}.", EventContextTypeCode.AIP_Status, EInvoicingPivotState.Failed, BatchNumber, Transaction.AH_TransactionNum, Transaction.Company.CompanyName));
				return;
			}

			if (!string.IsNullOrEmpty(EventData.ComplianceDate))
			{
				var complianceDate = new ZDate(DateTime.Parse(EventData.ComplianceDate, CultureInfo.InvariantCulture));
				Transaction.AH_ComplianceDocumentDate = complianceDate;
			}
		}

		internal void MapComplianceSubTypeToDatabase(IXmlSessionTracker logger)
		{
			var complianceSubType = EventData.ComplianceSubType;
			if (!string.IsNullOrEmpty(complianceSubType) && EventData.PivotStatus == EInvoicingPivotState.Failed)
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|ComplianceSubTypeFailed", "Compliance Sub Type can't be updated as {0} is status '{1}' for invoice batch {2}, transaction {3}, in {4}.", EventContextTypeCode.AIP_Status, EInvoicingPivotState.Failed, BatchNumber, Transaction.AH_TransactionNum, Transaction.Company.CompanyName));
				return;
			}

			if (!string.IsNullOrEmpty(complianceSubType))
			{
				Transaction.AH_ComplianceSubType = complianceSubType;
			}
		}

		internal void MapComplianceDocumentStatusToDatabase()
		{
			var complianceDocumentStatus = EventData.ComplianceDocumentStatus;
			if (!string.IsNullOrEmpty(complianceDocumentStatus))
			{
				var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccTransactionHeaderReferenceTypes.CDS);
				query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, Transaction.PK);
				var reference = Transaction.Factory.LoadTop1<AccTransactionHeaderReference>(query);

				if (reference == null)
				{
					reference = Transaction.Factory.New<AccTransactionHeaderReference>();
					reference.AH1_AH = Transaction.PK;
					reference.AH1_Type = AccTransactionHeaderReferenceTypes.CDS;
				}

				reference.AH1_Reference = complianceDocumentStatus;
			}
		}

		internal void MapVoidedAndCreditedAmountToDatabase()
		{
			var toCheckVoidedAndCreditedAmountStatus = ChinaComplianceInfo.GetNeedCheckVoidedAndCreditedAmountStatusCodes();
			var voidedAndCreditedAmount = EventData.VoidedAndCreditedAmountForCN;
			var complianceDocumentStatus = EventData.ComplianceDocumentStatus;
			if (!string.IsNullOrEmpty(voidedAndCreditedAmount) && toCheckVoidedAndCreditedAmountStatus.Contains(complianceDocumentStatus))
			{
				var query = new ZQuery(AccTransactionHeaderReferenceSchema.AH1_Type, AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.CDS);
				query.AddToFilter(AccTransactionHeaderReferenceSchema.AH1_AH, Transaction.PK);
				var reference = Transaction.Factory.LoadTop1<AccTransactionHeaderReference>(query);

				if (reference != null && toCheckVoidedAndCreditedAmountStatus.Contains(reference.AH1_Reference))
				{
					reference.AH1_Amount = ZDecimal.ParseSafe(voidedAndCreditedAmount, 0);
				}
			}
		}

		internal void MapAuthorisationRecordToDatabase(IXmlSessionTracker logger)
		{
			if (EventData.HasAnyFieldsForAuthorisationRecord && ValidateAuthorisationRecord(logger))
			{
				CreateOrUpdateAuthorisationRecord(logger);
			}
		}

		bool ValidateAuthorisationRecord(IXmlSessionTracker logger)
		{
			if (!string.IsNullOrEmpty(EventData.PublicKey) && !IsValidBase64String(EventData.PublicKey))
			{
				logger.LogBoth(LogType.Warning, InvalidBase64DataResString(EventContextTypeCode.AHF_PublicKey));
				return false;
			}

			if (!string.IsNullOrEmpty(EventData.AuthorisationData) && !IsValidBase64String(EventData.AuthorisationData))
			{
				logger.LogBoth(LogType.Warning, InvalidBase64DataResString(EventContextTypeCode.AHF_AuthorisationData));
				return false;
			}

			if (!string.IsNullOrEmpty(EventData.ITransactionHash) && !IsValidBase64String(EventData.ITransactionHash))
			{
				logger.LogBoth(LogType.Warning, InvalidBase64DataResString(EventContextTypeCode.AHF_ITransactionHash));
				return false;
			}

			if (!string.IsNullOrEmpty(EventData.IssuerCertificateIdentifier) && EventData.IssuerCertificateIdentifier.Length > AutoAccTransactionHeaderAuthorisationRecord.Schema.AHF_IssuerCertificateIdentifierMaxLength)
			{
				logger.LogBoth(LogType.Warning, InvalidLengthString(EventContextTypeCode.AHF_IssuerCertificateIdentifier, AutoAccTransactionHeaderAuthorisationRecord.Schema.AHF_IssuerCertificateIdentifierMaxLength));
				return false;
			}

			if (!string.IsNullOrEmpty(EventData.IssuerAuthorizationData) && !IsValidBase64String(EventData.IssuerAuthorizationData))
			{
				logger.LogBoth(LogType.Warning, InvalidBase64DataResString(EventContextTypeCode.AHF_IssuerAuthorizationData));
				return false;
			}

			if (!string.IsNullOrEmpty(EventData.DebtorNumber) && EventData.DebtorNumber.Length > AutoAccTransactionHeaderAuthorisationRecord.Schema.AHF_DebtorNumberMaxLength)
			{
				logger.LogBoth(LogType.Warning, InvalidLengthString(EventContextTypeCode.AHF_DebtorNumber, AutoAccTransactionHeaderAuthorisationRecord.Schema.AHF_DebtorNumberMaxLength));
				return false;
			}

			if (!string.IsNullOrEmpty(EventData.PlaceOfIssue) && EventData.PlaceOfIssue.Length > AutoAccTransactionHeaderAuthorisationRecord.Schema.AHF_PlaceOfIssueMaxLength)
			{
				logger.LogBoth(LogType.Warning, InvalidLengthString(EventContextTypeCode.AHF_PlaceOfIssue, AutoAccTransactionHeaderAuthorisationRecord.Schema.AHF_PlaceOfIssueMaxLength));
				return false;
			}

			if (!string.IsNullOrEmpty(EventData.DateTime) && !DateTimeOffset.TryParse(EventData.DateTime, out var _))
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|InvalidDateTimeOffset", "Failed to set {0} as '{1}' cannot be converted to date and time for invoice batch {2}, transaction {3}, in {4}.", EventContextTypeCode.AHF_DateTime, EventData.DateTime, Batch?.AIB_BatchNumber, TransactionIdentifier, CompanyName));
				return false;
			}

			var maxLengthValidationResult = ValidateMaxLength();
			if (!maxLengthValidationResult.result)
			{
				logger.LogBoth(LogType.Warning, maxLengthValidationResult.errorMessage);
				return false;
			}

			return true;
		}

		string InvalidBase64DataResString(string fieldName)
			=> Res.GetString("GlobalEInvoicingEventMessageProcessor|InvalidBase64Data", "Failed to set {0} as it is not a valid Base64 string for invoice batch {1}, transaction {2}, in {3}.", fieldName, Batch?.AIB_BatchNumber, TransactionIdentifier, CompanyName);

		string InvalidLengthString(string fieldName, int length)
			=> Res.GetString("GlobalEInvoicingEventMessageProcessor|InvalidLenghtString", "Failed to set {0} as its length more than {4}: invoice batch {1}, transaction {2}, in {3}.", fieldName, Batch?.AIB_BatchNumber, TransactionIdentifier, CompanyName, length);

		void CreateOrUpdateAuthorisationRecord(IXmlSessionTracker logger)
		{
			if (AuthRecord == null)
			{
				var factory = Batch?.Factory ?? Transaction.Factory;
				AuthRecord = factory.New<AccTransactionHeaderAuthorisationRecord>();
				AuthRecord.AHF_RecordType = AuthRecordType;
				AuthRecord.AHF_ParentId = Transaction.PK;
				AuthRecord.AHF_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			}

			var fieldsThatDidNotUpdate = new List<string>();
			TrySetFieldAndTrackFailuresZBlob(EventData.AuthorisationData, AuthRecord.TrySetAuthorisationDataOnce, EventContextTypeCode.AHF_AuthorisationData);
			TrySetFieldAndTrackFailuresZString(EventData.Counter, AuthRecord.TrySetCounterOnce, EventContextTypeCode.AHF_Counter);
			TrySetFieldAndTrackFailuresZDateTime(EventData.DateTime, AuthRecord.TrySetDateTimeOnce, EventContextTypeCode.AHF_DateTime);
			TrySetFieldAndTrackFailuresZString(EventData.IDNumber, AuthRecord.TrySetIDNumberOnce, EventContextTypeCode.AHF_IDNumber);
			TrySetFieldAndTrackFailuresZString(EventData.IDType, AuthRecord.TrySetIDTypeOnce, EventContextTypeCode.AHF_IDType);
			TrySetFieldAndTrackFailuresZBlob(EventData.ITransactionHash, AuthRecord.TrySetITransactionHashOnce, EventContextTypeCode.AHF_ITransactionHash);
			TrySetFieldAndTrackFailuresZString(EventData.Number, AuthRecord.TrySetNumberOnce, EventContextTypeCode.AHF_Number);
			TrySetFieldAndTrackFailuresZBlob(EventData.PublicKey, AuthRecord.TrySetPublicKeyOnce, EventContextTypeCode.AHF_PublicKey);
			TrySetFieldAndTrackFailuresZString(EventData.VerificationUrl, AuthRecord.TrySetVerificationUrlOnce, EventContextTypeCode.AHF_VerificationUrl);
			TrySetFieldAndTrackFailuresZString(EventData.IssuerCertificateIdentifier, AuthRecord.TrySetIssuerCertificateIdentifierOnce, EventContextTypeCode.AHF_IssuerCertificateIdentifier);
			TrySetFieldAndTrackFailuresZBlob(EventData.IssuerAuthorizationData, AuthRecord.TrySetIssuerAuthorizationDataOnce, EventContextTypeCode.AHF_IssuerAuthorizationData);
			TrySetFieldAndTrackFailuresZString(EventData.DebtorNumber, AuthRecord.TrySetDebtorNumberOnce, EventContextTypeCode.AHF_DebtorNumber);
			TrySetFieldAndTrackFailuresZString(EventData.PlaceOfIssue, AuthRecord.TrySetPlaceOfIssueOnce, EventContextTypeCode.AHF_PlaceOfIssue);

			if (fieldsThatDidNotUpdate.Any())
			{
				logger.LogBoth(LogType.Warning, Res.GetString("GlobalEInvoicingEventMessageProcessor|AuthorizationRecordFieldsAlreadySet", "Authorization record field(s) {0} have already been set for invoice batch {1}, transaction {2}, in {3}. You may only set {4} fields once.", string.Join(",", fieldsThatDidNotUpdate), Batch?.AIB_BatchNumber, TransactionIdentifier, CompanyName, nameof(AccTransactionHeaderAuthorisationRecord)));
			}

			#region Local Helpers

			void TrySetFieldAndTrackFailuresZBlob(string rawValue, Func<ZBlob, bool> updater, string field)
			{
				if (!string.IsNullOrEmpty(rawValue) && !updater(Convert.FromBase64String(rawValue)))
				{
					fieldsThatDidNotUpdate.Add(field);
				}
			}

			void TrySetFieldAndTrackFailuresZString(string rawValue, Func<ZString, bool> updater, string field)
			{
				if (!string.IsNullOrEmpty(rawValue) && !updater(rawValue))
				{
					fieldsThatDidNotUpdate.Add(field);
				}
			}

			void TrySetFieldAndTrackFailuresZDateTime(string rawValue, Func<ZDateTimeOffset, bool> updater, string field)
			{
				if (!string.IsNullOrEmpty(rawValue)
					&& DateTimeOffset.TryParse(rawValue, out var dateTime)
					&& !updater(dateTime))
				{
					fieldsThatDidNotUpdate.Add(field);
				}
			}
			#endregion
		}

		(bool result, string errorMessage) ValidateMaxLength()
		{
			var msgBuilder = new StringBuilder();
			foreach (var field in FieldsToCheckForMaximumLength)
			{
				if (!string.IsNullOrEmpty(field.value) && field.value.Length > field.maxLength)
				{
					msgBuilder.AppendLine(Res.GetString("c777fb28-5321-4df4-8474-c3b69e0987c9", "{0}: Maximum Length {1}, but {2} were entered", field.name, field.maxLength, field.value.Length));
				}
			}

			if (msgBuilder.Length > 0)
			{
				return (false, Res.GetString("BBD29409-EB01-4D3A-AF57-ABCADDE437A3", "Invoice batch {0}, transaction {1}, in {2}: The maximum length of following fields has been exceeded\r\n{3}", Batch?.AIB_BatchNumber, TransactionIdentifier, CompanyName, msgBuilder.ToString()));
			}

			return (true, string.Empty);
		}

		(string name, string value, int maxLength)[] FieldsToCheckForMaximumLength
		{
			get
			{
				var fields = new (string name, string value, int maxLength)[]
				{
					(nameof(EventData.Counter), EventData.Counter, AccTransactionHeaderAuthorisationRecordSchema.AHF_Counter.MaxLength)
					, (nameof(EventData.IDNumber), EventData.IDNumber, AccTransactionHeaderAuthorisationRecordSchema.AHF_IDNumber.MaxLength)
					, (nameof(EventData.IDType), EventData.IDType, AccTransactionHeaderAuthorisationRecordSchema.AHF_IDType.MaxLength)
					, (nameof(EventData.Number), EventData.Number, AccTransactionHeaderAuthorisationRecordSchema.AHF_Number.MaxLength)
					, (nameof(EventData.VerificationUrl), EventData.VerificationUrl, AccTransactionHeaderAuthorisationRecordSchema.AHF_VerificationUrl.MaxLength)
					, (nameof(EventData.IssuerCertificateIdentifier), EventData.IssuerCertificateIdentifier, AccTransactionHeaderAuthorisationRecordSchema.AHF_IssuerCertificateIdentifier.MaxLength)
					, (nameof(EventData.DebtorNumber), EventData.DebtorNumber, AccTransactionHeaderAuthorisationRecordSchema.AHF_DebtorNumber.MaxLength)
					, (nameof(EventData.PlaceOfIssue), EventData.PlaceOfIssue, AccTransactionHeaderAuthorisationRecordSchema.AHF_PlaceOfIssue.MaxLength)
				};
				return fields;
			}
		}

		#region Helpers

		static bool IsValidBase64String(string s)
		{
			try
			{
				Convert.FromBase64String(s);
				return true;
			}
			catch (FormatException)
			{
				return false;
			}
		}

		#endregion
	}
}
