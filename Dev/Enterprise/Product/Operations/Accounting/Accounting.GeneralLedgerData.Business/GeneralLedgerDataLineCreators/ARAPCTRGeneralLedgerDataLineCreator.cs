using System;
using System.Data;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class ARAPCTRGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			if (GLControlAccounts.Instance.ARControlAccount == null || GLControlAccounts.Instance.APControlAccount == null)
			{
				throw new MissingGLHeaderException(Res.GetString("8658a1cc-ef33-4f5f-98fe-34088dec791a", "AR/AP Control Account"));
			}

			var localAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_InvoiceAmount];
			var oSAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_OSTotal];
			var postDate = (DateTime)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_PostDate];
			var ledger = (string)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_Ledger];

			DebitCreditEntryItem line;
			if (ledger == LedgerTypes.AccountsPayable)
			{
				line = CreateAndPopulateDebitCreditLine(GLControlAccounts.Instance.APControlAccount.PK, GLDAccountTypes.APControlAccount, localAmount, oSAmount, postDate);
			}
			else
			{
				line = CreateAndPopulateDebitCreditLine(GLControlAccounts.Instance.ARControlAccount.PK, GLDAccountTypes.ARControlAccount, localAmount, oSAmount, postDate);
			}

			return new[] { line };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnHeader(gLDDataSourceRow);
		}
	}
}
