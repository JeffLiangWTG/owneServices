using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class GLGJLNJLGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var postDate = (DateTime)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_PostDate];
			var lineGLAccount = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AG];
			var localAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineAmount];
			var oSAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_OSAmount];
			var line = CreateAndPopulateDebitCreditLine(lineGLAccount, GLDAccountTypes.TransactionLineGLAccount, localAmount, oSAmount, postDate);

			return new DebitCreditEntryItem[] { line };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnLine(gLDDataSourceRow);
		}

		protected override bool NeedCalculateOSAmountWhenEmpty => false;
	}
}
