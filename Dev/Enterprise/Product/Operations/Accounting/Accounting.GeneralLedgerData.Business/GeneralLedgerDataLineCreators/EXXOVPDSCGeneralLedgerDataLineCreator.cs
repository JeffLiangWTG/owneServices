using System;
using System.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class EXXOVPDSCGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var glHeaderPk = (Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_AG];
			var localAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_InvoiceAmount];
			var oSAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_OSTotal];
			var postDate = (DateTime)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_PostDate];
			var controlAccount = GetControllAccountIfNullThrowException((string)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_Ledger]);
			var controlAccountType = GetContorlAccountType((string)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_Ledger]);

			var drLine = CreateAndPopulateDebitCreditLine(controlAccount.PK, controlAccountType, localAmount, oSAmount, postDate);
			var crRevLine = CreateAndPopulateDebitCreditLine(glHeaderPk, GLDAccountTypes.TransactionHeaderGLAccount, localAmount * (-1), oSAmount * (-1), postDate);

			return new[] { drLine, crRevLine };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnHeader(gLDDataSourceRow);
		}

		protected ZString GetContorlAccountType(ZString ledger)
		{
			var accountType = ZString.Empty;

			if (ledger.Equals(LedgerTypes.AccountsReceivable))
			{
				accountType = GLDAccountTypes.ARControlAccount;
			}
			else if (ledger.Equals(LedgerTypes.AccountsPayable))
			{
				accountType = GLDAccountTypes.APControlAccount;
			}

			return accountType;
		}

		AccGLHeader GetControllAccountIfNullThrowException(ZString ledger)
		{
			var controlAccount = GetControlAccount(ledger) ?? throw new MissingGLHeaderException(Res.GetString("BDAD60A9-6C8D-4DA9-942C-7D83AFDEC4A6", "AR/AP Control Account"));

			return controlAccount;
		}
	}
}
