using System;
using System.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class CBEXXGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var localAmount = (decimal)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_InvoiceAmount];
			var postDate = (DateTime)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_PostDate];
			var glHeaderPK = (Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_AG];
			var bankAccount = ReadOnlyFactory.Load<AccBankAccount>((Guid)gLDDataSourceRow[AccTransactionHeaderSchema.Constants.AH_AB]);
			var line = CreateAndPopulateDebitCreditLine(glHeaderPK, GLDAccountTypes.TransactionHeaderGLAccount, localAmount * (-1), ZDecimal.Zero, postDate);
			var bankAccline = CreateAndPopulateDebitCreditLine(bankAccount.AB_AG, GLDAccountTypes.TransactionBankGLAccount, localAmount, ZDecimal.Zero, postDate);

			return new DebitCreditEntryItem[] { line, bankAccline };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnHeader(gLDDataSourceRow);
		}

		protected override ZDecimal CalculateOSAmount(ZDecimal localAmount)
		{
			return ZDecimal.Zero;
		}
	}
}
