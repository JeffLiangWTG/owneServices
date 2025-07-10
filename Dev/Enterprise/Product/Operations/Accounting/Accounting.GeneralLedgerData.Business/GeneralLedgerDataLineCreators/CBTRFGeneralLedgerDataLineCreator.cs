using System;
using System.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class CBTRFGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var transactionCount = (byte)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_TransactionCount];
			var isFromTransaction = transactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRow || transactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing;
			var bankAccountPK = (Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_AB];
			var bankAccount = ReadOnlyFactory.Load<AccBankAccount>(bankAccountPK);

			var gLDAccountType = isFromTransaction ? GLDAccountTypes.FromTRFBankControlAccount : GLDAccountTypes.ToTRFBankControlAccount;
			var line = CreateAndPopulateDebitCreditLine(bankAccount.AB_AG, gLDAccountType, (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_InvoiceAmount], (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_OSTotal], (DateTime)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_PostDate]);

			return new DebitCreditEntryItem[] { line };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnHeader(gLDDataSourceRow);
		}
	}
}
