using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch
{
	public interface IDirectDebitBatchTransaction : IBusiness, IDirectDebitBatchComponent
	{
		OrgHeader Header { get; }
		OrgHeaderCollection Headers { get; }
		AccBankAccount BankAccount { get; }

		ZString Code { get; }
		ZPropertyInfo CodeInfo { get; }

		ZString AH_TransactionNum { get; }
		ZPropertyInfo AH_TransactionNumInfo { get; }

		ZString AH_Ledger { get; }
		ZPropertyInfo AH_LedgerInfo { get; }

		ZString AH_TransactionType { get; }
		ZPropertyInfo AH_TransactionTypeInfo { get; }

		ZString PayeeBankAccountNumber { get; }
		ZPropertyInfo PayeeBankAccountNumberInfo { get; }

		ZString PayeeBankBranchName { get; }
		ZPropertyInfo PayeeBankBranchNameInfo { get; }

		ZString PayeeBankAddress1 { get; }
		ZPropertyInfo PayeeBankAddress1Info { get; }

		ZString PayeeBankAddress2 { get; }
		ZPropertyInfo PayeeBankAddress2Info { get; }

		ZString PayeeBankAddress3 { get; }
		ZPropertyInfo PayeeBankAddress3Info { get; }

		ZString AccountTitle { get; }
		ZPropertyInfo AccountTitleInfo { get; }

		ZString PayeeBankBSB { get; }
		ZPropertyInfo PayeeBankBSBInfo { get; }

		ZString AccountCurrency { get; }
		ZPropertyInfo AccountCurrencyInfo { get; }

		ZString PayeeBankName { get; }
		ZPropertyInfo PayeeBankNameInfo { get; }

		ZString PayeeBankSwift { get; }
		ZPropertyInfo PayeeBankSwiftInfo { get; }

		ZString PayeeIBANNumber { get; }
		ZPropertyInfo PayeeIBANNumberInfo { get; }

		ZString PayeeCountryCode { get; }
		ZPropertyInfo PayeeCountryCodeInfo { get; }

		ZBool IncludeInTheBatch { get; set; }
		ZPropertyInfo IncludeInTheBatchInfo { get; }

		ZDecimal AH_OSExTaxAmount { get; }
		ZPropertyInfo AH_OSExTaxAmountInfo { get; }

		ZDecimal AH_OSTaxAmount { get; }
		ZPropertyInfo AH_OSTaxAmountInfo { get; }

		ZDecimal AH_OSTotalAmount { get; }
		ZPropertyInfo AH_OSTotalAmountInfo { get; }

		ZDecimal AH_LocalExTaxAmount { get; }
		ZPropertyInfo AH_LocalExTaxAmountInfo { get; }

		ZDecimal AH_LocalTaxAmount { get; }
		ZPropertyInfo AH_LocalTaxAmountInfo { get; }

		ZDecimal AH_LocalTotalAmount { get; }
		ZPropertyInfo AH_LocalTotalAmountInfo { get; }

		[List("Lookups.Headers")]
		ZGuid AH_OH { get; }
		ZPropertyInfo AH_OHInfo { get; }

		ZBool AllowAutoDDR { get; }
		ZPropertyInfo AllowAutoDDRInfo { get; }

		ZString AH_ReceiptBatchNo { get; set; }
		ZPropertyInfo AH_ReceiptBatchNoInfo { get; }

		ZString AH_ReceiptType { get; set; }
		ZPropertyInfo AH_ReceiptTypeInfo { get; }

		ZDateTime AH_PostDate { get; set; }
		ZPropertyInfo AH_PostDateInfo { get; }

		ZGuid AH_AB { get; set; }
		ZPropertyInfo AH_ABInfo { get; }

		ZString BankAccountNumber { get; }

		ZString BankCreateUser { get; }
		ZPropertyInfo BankCreateUserInfo { get; }

		ZDateTime BankCreateTimeLocal { get; }
		ZPropertyInfo BankCreateTimeLocalInfo { get; }

		ZString BankLastEditUser { get; }
		ZPropertyInfo BankLastEditUserInfo { get; }

		ZDateTime BankLastEditTimeLocal { get; }
		ZPropertyInfo BankLastEditTimeLocalInfo { get; }

		// Validation Related Interface Members
		void ValidateAutoDDR();
		void ValidateBankBSB();
		void ValidateBankAccountNumber();

		DirectDebitBatchLineCollection DDRCollection { get; }
	}
}
