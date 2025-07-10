using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(TransactionInfo))]
	class TransactionInfoTest : DataObjectTestCase<TransactionInfo>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(TransactionInfo.Ledger), AccTransactionHeaderSchema.AH_Ledger.MaxLength },
				{ nameof(TransactionInfo.Number), AccTransactionHeaderSchema.AH_TransactionNum.MaxLength },
				{ nameof(TransactionInfo.Description), AccTransactionHeaderSchema.AH_Desc.MaxLength },
				{ nameof(TransactionInfo.Category), AccTransactionHeaderSchema.AH_TransactionCategory.MaxLength },
				{ nameof(TransactionInfo.CheckNumberOrPaymentRef), AccTransactionHeaderSchema.AH_ChequeOrReference.MaxLength },
				{ nameof(TransactionInfo.CheckDrawer), AccTransactionHeaderSchema.AH_ChequeDrawer.MaxLength },
				{ nameof(TransactionInfo.DrawerBank), AccTransactionHeaderSchema.AH_DrawerBank.MaxLength },
				{ nameof(TransactionInfo.DrawerBranch), AccTransactionHeaderSchema.AH_DrawerBranch.MaxLength },
				{ nameof(TransactionInfo.JobInvoiceNumber), AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength },
				{ nameof(TransactionInfo.PlaceOfIssue), Math.Max(50, GlbBranchSchema.GB_City.MaxLength) },
				{ nameof(TransactionInfo.ReceiptOrDirectDebitNumber), AccTransactionHeaderSchema.AH_ReceiptBatchNo.MaxLength },
				{ nameof(TransactionInfo.BankAccount), AccBankAccountSchema.AB_Code.MaxLength },
				{ nameof(TransactionInfo.RequisitionStatus), AccTransactionHeaderSchema.AH_RequisitionStatus.MaxLength },
				{ nameof(TransactionInfo.CreateUser), AccTransactionHeaderSchema.AH_SystemCreateUser.MaxLength },
				{ nameof(TransactionInfo.LastEditUser), AccTransactionHeaderSchema.AH_SystemCreateUser.MaxLength },
				{ nameof(TransactionInfo.ExternalDebtorCode), OrgCompanyDataSchema.OB_ARExternalDebtorCode.MaxLength },
				{ nameof(TransactionInfo.ExternalCreditorCode), OrgCompanyDataSchema.OB_APExternalCreditorCode.MaxLength },
				{ nameof(TransactionInfo.TransactionReference), AccTransactionHeaderSchema.AH_TransactionReference.MaxLength },
				{ nameof(TransactionInfo.ComplianceSubType), AccTransactionHeaderSchema.AH_ComplianceSubType.MaxLength },
				{ nameof(TransactionInfo.AgreedPaymentMethod), AccTransactionHeaderSchema.AH_AgreedPaymentMethodOverride.MaxLength },
				{ nameof(TransactionInfo.DigitalSignature), 200 },	// This is AccTransactionHeaderSchema.AH_DigitalSignature VARBINARY(128) Base64 encoded length rounded to 200
				{ nameof(TransactionInfo.CancelReasonFreeText), 200 },	// Defined on TransactionReasonHolder.Reason as 200 characters. Stored in AH_Desc (128 chars), truncated and appended to any existing data in that field :-/
				{ nameof(TransactionInfo.CheckBookCode), AccChequeBookSchema.AK_Code.MaxLength },
				{ nameof(TransactionInfo.OrganizationsTransactionID), AccTransactionHeaderReferenceSchema.AH1_Reference.MaxLength },
				{ nameof(TransactionInfo.GovernmentAllocatedID), AccTransactionHeaderSchema.AH_GovernmentAllocatedID.MaxLength }
			};
		}

		public void TestPostingJournalCollectionIsNotMandatory()
		{
			var propertyInfo = typeof(TransactionInfo).GetProperty("PostingJournalCollection");

			Assert("PostingJournalCollection should not have mandatory attribute.", propertyInfo.GetCustomAttributes(typeof(MandatoryAttribute), false).Length == 0);
		}
	}
}

