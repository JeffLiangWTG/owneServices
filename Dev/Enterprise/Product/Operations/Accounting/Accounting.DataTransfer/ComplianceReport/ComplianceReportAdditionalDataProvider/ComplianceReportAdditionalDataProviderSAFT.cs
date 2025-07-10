using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.DataTransfer.ComplianceReport
{
	class ComplianceReportAdditionalDataProviderSAFT : IComplianceReportAdditionalDataHeaderDetailsProvider, IComplianceReportAdditionalDataLineDetailsProvider, IComplianceReportAdditionalDataProvider
	{
		public (string extraColumns, string extraConditions) GetHeaderExtraColumnsAndConditions(ComplianceReportDataCollectionMode mode)
		{
			var ledgerOfSAFT = GetSAFTLedger(mode);

			var extraColumns = @",
	addEvent.SL_EventTime HighPrecisionAddDateTime, header.AH_InvoiceTerm, header.AH_InvoiceTermDays, headerReference.AH1_Reference, XA_Data, header.AH_OriginalReferenceStartDate, header.AH_OriginalReferenceEndDate,
header.AH_AgreedPaymentMethodOverride, header.AH_DueDate, originalHeaderReference.AH1_Reference AS OriginalReferenceSourceReference, originalTransaction.AH_ComplianceSubType AS OriginalReferenceComplianceSubType";    // Hardcoded part of SQL statement

			var extraConditions = $@"	LEFT JOIN dbo.AccTransactionHeaderReference headerReference ON header.AH_PK = headerReference.AH1_AH AND headerReference.AH1_Type = '{AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.PIR}'
	LEFT JOIN dbo.AccTransactionHeaderReference originalHeaderReference ON originalTransaction.AH_PK = originalHeaderReference.AH1_AH AND originalHeaderReference.AH1_Type = '{AccountingMasterFilesConstants.AccTransactionHeaderReferenceTypes.PIR}'
	LEFT JOIN dbo.StmALog addEvent ON addEvent.SL_Parent = header.AH_PK AND addEvent.SL_SE_NKEvent = 'ADD'
	LEFT JOIN dbo.GenAddOnColumn ON XA_ParentID = header.AH_PK AND XA_ParentTableCode = '{AccTransactionHeaderSchema.Constants.Prefix}' AND XA_Name = '{InvoicingBase.GenAddOnColumnReasonName}'
WHERE header.AH_Ledger = '{ledgerOfSAFT}' AND
	header.AH_TransactionType IN ('{TransactionTypes.Invoice}', '{TransactionTypes.CreditNote}')";

			if (mode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				extraConditions += FormattableString.Invariant($@" AND
	header.AH_TransactionCategory = '{TransactionCategory.Codes.SelfBilling}' AND
	header.AH_OH = @SelectedSupplierPK");
			}

			return (extraColumns, extraConditions);
		}

		public (string extraJoin, string extraConditions) GetLineExtraJoinsAndConditions(ComplianceReportDataCollectionMode mode)
		{
			var ledgerOfSAFT = GetSAFTLedger(mode);
			var extraJoin = "JOIN dbo.AccTransactionHeader ON AH_PK = AL_AH";
			var extraCondition = FormattableString.Invariant($@"AND AH_Ledger = '{ledgerOfSAFT}'
AND AH_TransactionType IN('{TransactionTypes.Invoice}', '{TransactionTypes.CreditNote}')");  // Hardcoded part of SQL statement

			if (mode == ComplianceReportDataCollectionMode.SAFT)
			{
				extraCondition += FormattableString.Invariant($"AND AL_LineType  = '{TransactionLineTypes.Revenue}'");    // Hardcoded part of SQL statement
			}
			else if (mode == ComplianceReportDataCollectionMode.SAFTSelfBilling)
			{
				extraCondition += FormattableString.Invariant($@"AND AL_LineType  = '{TransactionLineTypes.Cost}' 
AND AH_OH = @SelectedSupplierPK
AND AH_TransactionCategory = '{TransactionCategory.Codes.SelfBilling}'");    // Hardcoded part of SQL statement
			}

			return (extraJoin, extraCondition);
		}

		string GetSAFTLedger(ComplianceReportDataCollectionMode mode)
		{
			return mode == ComplianceReportDataCollectionMode.SAFT
						? LedgerTypes.AccountsReceivable
						: mode == ComplianceReportDataCollectionMode.SAFTSelfBilling
								? LedgerTypes.AccountsPayable
								: string.Empty;
		}

		bool IComplianceReportAdditionalDataProvider.GetIsValidForCustomers(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.AccountsReceivable && (transactionType == TransactionTypes.Invoice || transactionType == TransactionTypes.CreditNote);
		}

		bool IComplianceReportAdditionalDataProvider.GetIsValidForSuppliers(string ledger, string transactionType)
		{
			return ledger == LedgerTypes.AccountsPayable || (ledger == LedgerTypes.JobCosting && transactionType == TransactionLineTypes.Accrual);
		}
	}
}
