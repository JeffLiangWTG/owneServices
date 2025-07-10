using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class InvoicingBase
	{
		#region Original Invoice Reference

		public bool ShouldShowOriginalInvoiceReferenceFields
		{
			get
			{
				var countrySpecificOriginalInvoiceReference = ObjectFactory.Get<ICountryComplianceFactory>()?.GetIOriginalInvoiceReference(Company.GC_RN_NKCountryCode);
				if (countrySpecificOriginalInvoiceReference != null)
				{
					return countrySpecificOriginalInvoiceReference.ShouldShowOriginalInvoiceReferenceFields(AH_Ledger, AH_TransactionType);
				}
				else
				{
					// default behaviour: we show Original Invoice Reference fields in Credit Note form
					return (AH_Ledger == LedgerTypes.AccountsReceivable || AH_Ledger == LedgerTypes.AccountsPayable) && (AH_TransactionType == TransactionTypes.CreditNote || (AH_TransactionType == TransactionTypes.Invoice && IsAmendingTransaction));
				}
			}
		}

		public bool ShouldShowOriginalInvoiceReferenceDatesFields
		{
			get
			{
				var countrySpecificOriginalInvoiceReference = ObjectFactory.Get<ICountryComplianceFactory>()?.GetIOriginalInvoiceReference(Company.GC_RN_NKCountryCode);
				if (countrySpecificOriginalInvoiceReference != null)
				{
					return countrySpecificOriginalInvoiceReference.ShouldShowOriginalInvoiceReferenceDatesFields(AH_Ledger, AH_TransactionType);
				}
				else
				{
					// default behaviour: we don't show "Reference Date From" and "Reference Date To" fields
					return false;
				}
			}
		}

		public bool ShouldShowOriginalInvoiceReferenceReasonFields
		{
			get
			{
				var countrySpecificOriginalInvoiceReference = ObjectFactory.Get<ICountryComplianceFactory>()?.GetIOriginalInvoiceReference(Company.GC_RN_NKCountryCode);
				if (countrySpecificOriginalInvoiceReference != null)
				{
					return countrySpecificOriginalInvoiceReference.ShouldShowOriginalInvoiceReferenceReasonFields(AH_Ledger, AH_TransactionType);
				}
				else
				{
					// default behaviour: we show Reason Code and Description fields in AR Credit Note form
					return AH_Ledger == LedgerTypes.AccountsReceivable && (AH_TransactionType == TransactionTypes.CreditNote || (AH_TransactionType == TransactionTypes.Invoice && IsAmendingTransaction));
				}
			}
		}

		public bool AreAllOriginalInvoiceReferenceFieldsEnabled
		{
			get
			{
				var countrySpecificOriginalInvoiceReference = ObjectFactory.Get<ICountryComplianceFactory>()?.GetIOriginalInvoiceReference(Company.GC_RN_NKCountryCode);
				if (countrySpecificOriginalInvoiceReference != null)
				{
					return countrySpecificOriginalInvoiceReference.GetAreAllOriginalInvoiceReferenceFieldsEnabled(AH_Ledger, AH_TransactionType, AH_ComplianceSubType);
				}
				else
				{
					// default behaviour: When Original Invoice Reference related fields are shown, they are enabled (not read only)
					return true;
				}
			}
		}

		public bool AreOriginalTransactionReferenceFieldsMandatory
		{
			get
			{
				var countrySpecificOriginalInvoiceReference = ObjectFactory.Get<ICountryComplianceFactory>()?.GetIOriginalInvoiceReference(Company.GC_RN_NKCountryCode);
				if (countrySpecificOriginalInvoiceReference != null)
				{
					return AreAllOriginalInvoiceReferenceFieldsEnabled && countrySpecificOriginalInvoiceReference.GetAreOriginalTransactionReferenceFieldsMandatory(AH_Ledger, AH_TransactionType);
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region Original Reference fields

		[ReadOnlyMember(nameof(AreOriginalReferenceNumberAndDateFieldsReadOnly))]
		public override ZString AH_OriginalTransactionNum
		{
			get => OriginalTransactionIsSet ? OriginalReferenceTransaction.AH_TransactionNum : base.AH_OriginalTransactionNum;
			set => base.AH_OriginalTransactionNum = value;
		}

		[ReadOnlyMember(nameof(AreOriginalReferenceNumberAndDateFieldsReadOnly))]
		public override ZDate AH_OriginalInvoiceDate
		{
			get => OriginalTransactionIsSet ? OriginalReferenceTransaction.AH_InvoiceDate.Date : base.AH_OriginalInvoiceDate;
			set => base.AH_OriginalInvoiceDate = value;
		}

		[ReadOnlyMember(nameof(AreOriginalReferenceFieldsReadOnly))]
		public override ZDate AH_OriginalReferenceStartDate
		{
			get => base.AH_OriginalReferenceStartDate;
			set => base.AH_OriginalReferenceStartDate = value;
		}

		[ReadOnlyMember(nameof(AreOriginalReferenceFieldsReadOnly))]
		public override ZDate AH_OriginalReferenceEndDate
		{
			get => base.AH_OriginalReferenceEndDate;
			set => base.AH_OriginalReferenceEndDate = value;
		}

		protected bool AreOriginalReferenceFieldsReadOnly => !ShouldShowOriginalInvoiceReferenceFields || !AreAllOriginalInvoiceReferenceFieldsEnabled || ReadOnlyForAssociatedDraftInvoice;
		protected bool AreOriginalReferenceNumberAndDateFieldsReadOnly => !AreAllOriginalInvoiceReferenceFieldsEnabled || OriginalTransactionIsSet || ReadOnlyForAssociatedDraftInvoice;

		#endregion

		#region Reason Code And Description

		public ReadOnlyCodeDescriptionPairList ReasonCodes => reasonCodes ?? (reasonCodes = AccountingMasterFilesRegistry.Instance.AmendmentReasonCodesList.Value);
		ReadOnlyCodeDescriptionPairList reasonCodes;

		[ReadOnlyMember(nameof(IsReasonCodeReadOnly))]
		[List(nameof(ReasonCodes))]
		[MaxLength(3)]
		public ZString ReasonCode
		{
			get => ShouldShowOriginalInvoiceReferenceReasonFields && IsInDatabase ? (reasonCode = GetReasonCodeFromAddOnColumnData()) : reasonCode;
			set
			{
				CheckMaximumLength(ReasonCodeInfo, value);
				SetNonPersistentPropertyValue(ReasonCodeInfo, ref reasonCode, value);
				if (!IsValidationSuspended && Validation is InvoiceBaseValidation invoiceBaseValidation)
				{
					invoiceBaseValidation.ValidateReasonCode();
				}

				var reasonDescription = ReasonCodes.GetDescriptionFromCode(ReasonCode);
				ReasonDescription = reasonDescription != null ? ((ZString)reasonDescription).Left(ReasonDescriptionInfo.MaxLength) : null;
			}
		}
		ZString reasonCode;

		public ZPropertyInfo ReasonCodeInfo => GetZPropertyInfo(nameof(ReasonCode));

		[ReadOnlyMember(nameof(IsReasonDescriptionReadOnly))]
		[MaxLength(50)]
		public ZString ReasonDescription
		{
			get => ShouldShowOriginalInvoiceReferenceReasonFields && IsInDatabase ? (reasonDescription = GetReasonDescriptionFromAddOnColumnData()) : reasonDescription;
			set
			{
				CheckMaximumLength(ReasonDescriptionInfo, value);
				SetNonPersistentPropertyValue(ReasonDescriptionInfo, ref reasonDescription, value);
				if (!IsValidationSuspended && Validation is InvoiceBaseValidation invoiceBaseValidation)
				{
					invoiceBaseValidation.ValidateReasonDescription();
				}
			}
		}
		ZString reasonDescription;

		public ZPropertyInfo ReasonDescriptionInfo => GetZPropertyInfo(nameof(ReasonDescription));

		string GetReasonCodeFromAddOnColumnData() => GetReasonAddOnColumnData(0);
		string GetReasonDescriptionFromAddOnColumnData() => GetReasonAddOnColumnData(1);

		ZString GetReasonAddOnColumnData(int index)
		{
			var reasonAddOnColumnDataParts = ReasonAddOnColumn?.XA_Data.Split(reasonCodeDescriptionSeparator);
			return reasonAddOnColumnDataParts != null && reasonAddOnColumnDataParts.Length == 2 ? reasonAddOnColumnDataParts[index] : ZString.Empty;
		}

		public bool OriginalTransactionIsSet => OriginalTransactionReference.IsValid && OriginalReferenceTransaction != null;

		bool AreReasonFieldsEnabled => AreAllOriginalInvoiceReferenceFieldsEnabled
			&& ShouldShowOriginalInvoiceReferenceReasonFields
			&& OriginalTransactionIsSet;

		public const string GenAddOnColumnReasonName = "CreditNoteReasonDescription";

		protected bool IsReasonCodeReadOnly => !OriginalTransactionIsSet;

		protected bool IsReasonDescriptionReadOnly => IsReasonCodeReadOnly || ReasonCode != AccountingMasterFilesConstants.ReasonFreeTextCode.Code;

		GenAddOnColumn ReasonAddOnColumn => reasonAddOnColumn ?? (reasonAddOnColumn = LoadReasonAddOnColumn());
		GenAddOnColumn reasonAddOnColumn;
		internal string reasonCodeDescriptionSeparator = "|";

		void SaveReasonAddOnColumn()
		{
			if (!IsInDatabase
				&& AreReasonFieldsEnabled
				&& !ReasonCode.IsEmpty
				&& !ReasonDescription.IsEmpty)
			{
				reasonAddOnColumn = CreateReasonAddOnColumn();
				reasonAddOnColumn.XA_Data = ReasonCode + reasonCodeDescriptionSeparator + ReasonDescription;
			}
		}

		GenAddOnColumn LoadReasonAddOnColumn()
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, PK);
			query.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, AccTransactionHeaderSchema.Constants.Prefix);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, GenAddOnColumnReasonName);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			return Factory.LoadTop1<GenAddOnColumn>(query);
		}

		GenAddOnColumn CreateReasonAddOnColumn()
		{
			var addOnColumn = Factory.New<GenAddOnColumn>();
			addOnColumn.XA_ParentID = PK;
			addOnColumn.XA_ParentTableCode = AccTransactionHeaderSchema.Constants.Prefix;
			addOnColumn.XA_Name = GenAddOnColumnReasonName;
			addOnColumn.XA_Type = AddOnColumnDataType.Codes.String;

			return addOnColumn;
		}

		#endregion
	}
}
