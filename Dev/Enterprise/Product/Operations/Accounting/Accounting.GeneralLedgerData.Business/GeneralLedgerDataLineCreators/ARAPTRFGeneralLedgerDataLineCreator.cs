using System;
using System.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class ARAPTRFGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var controlAccount = GetControlAccount((string)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_Ledger]) ?? throw new MissingGLHeaderException(Res.GetString("8658a1cc-ef33-4f5f-98fe-34088dec791a", "AR/AP Control Account"));

			var transactionCount = (byte)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_TransactionCount];
			var localAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_InvoiceAmount];
			var oSAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_OSTotal];
			var postDate = (DateTime)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_PostDate];
			var ledger = (string)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_Ledger];
			var isFromTransaction = transactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRow ||
									 transactionCount == AccTransactionHeader.TransactionCountConstants.BankTransferFromRowWhenReversing;
			var gLDAccountType = GetGLDAccountType(isFromTransaction, ledger);
			var debitCreditLine = CreateAndPopulateDebitCreditLine(controlAccount.PK, gLDAccountType, localAmount, oSAmount, postDate);

			return new DebitCreditEntryItem[] { debitCreditLine };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnHeader(gLDDataSourceRow);
		}

		string GetGLDAccountType(bool isFromTransaction, ZString ledger)
		{
			string gLDAccountType;
			if (isFromTransaction)
			{
				gLDAccountType = ledger == LedgerTypes.AccountsReceivable ? GLDAccountTypes.ARFromTRFControlAccount : GLDAccountTypes.APFromTRFControlAccount;
			}
			else
			{
				gLDAccountType = ledger == LedgerTypes.AccountsReceivable ? GLDAccountTypes.ARToTRFControlAccount : GLDAccountTypes.APToTRFControlAccount;
			}

			return gLDAccountType;
		}
	}
}
