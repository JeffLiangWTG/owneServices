using System;
using System.Data;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class ARAPJNLGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var ledger = (string)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_Ledger];
			var controlAccount = GetControlAccount(ledger) ?? throw new MissingGLHeaderException(Res.GetString("8658a1cc-ef33-4f5f-98fe-34088dec791a", "AR/AP Control Account"));

			var localAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_InvoiceAmount];
			var oSAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_OSTotal];
			var postDate = (DateTime)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_PostDate];

			var drLine = CreateAndPopulateDebitCreditLine(controlAccount.PK, ledger == LedgerTypes.AccountsPayable ? GLDAccountTypes.APControlAccount : GLDAccountTypes.ARControlAccount, localAmount, oSAmount, postDate);
			var crLine = CreateAndPopulateDebitCreditLine((Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_AG], GLDAccountTypes.TransactionHeaderGLAccount, localAmount * (-1), oSAmount * (-1), postDate);

			return new DebitCreditEntryItem[] { drLine, crLine };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnHeader(gLDDataSourceRow);
		}
	}
}
