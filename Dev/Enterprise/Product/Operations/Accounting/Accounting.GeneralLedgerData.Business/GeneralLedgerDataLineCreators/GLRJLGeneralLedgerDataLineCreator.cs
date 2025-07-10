using System;
using System.Data;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GeneralLedgerData
{
	public class GLRJLGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var postDate = (DateTime)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_PostDate];
			var reverseDate = (DateTime)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_ReverseDate];
			var lineGLAccount = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AG];
			var localAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineAmount];
			var oSAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_OSAmount];
			var glLine = CreateAndPopulateDebitCreditLine(lineGLAccount, GLDAccountTypes.TransactionLineGLAccount, localAmount, oSAmount, postDate);
			var glRevLine = CreateAndPopulateDebitCreditLine(lineGLAccount, GLDAccountTypes.TransactionLineGLAccountForReverse, localAmount * (-1), oSAmount * (-1), reverseDate);

			return new[] { glLine, glRevLine };
		}

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			var line = CreateGeneralLedgerDataBasicBasedOnLine(gLDDataSourceRow);
			var transactionCompany = ReadOnlyFactory.Load<GlbCompany>((Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GC]);
			line.ExchangeRate = ((ICompany)transactionCompany).ExchangeRate.GetRate((decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineAmount] + (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_GSTVAT], (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_OSAmount], AccTransactionLinesSchema.AL_ExchangeRate.Scale);
			return line;
		}
	}
}
