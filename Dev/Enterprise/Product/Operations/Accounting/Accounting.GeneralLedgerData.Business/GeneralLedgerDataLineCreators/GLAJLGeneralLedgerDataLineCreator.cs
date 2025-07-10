using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GeneralLedgerData.Business
{
	public class GLAJLGeneralLedgerDataLineCreator : GeneralLedgerDataLineCreatorBase
	{
		protected override DebitCreditEntryItem[] CreateDRCREntriesCore(DataRow gLDDataSourceRow)
		{
			var postDate = new ZDateTime(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_PostDate]);
			var reverseDate = new ZDateTime(gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_ReverseDate]);
			var startPeriod = PeriodCalculator.GetPeriodFromDate(postDate);
			var endPeriod = PeriodCalculator.GetPeriodFromDate(reverseDate);
			var lineGLAccount = (Guid)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_AG];
			var localAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_LineAmount];
			var oSAmount = (decimal)gLDDataSourceRow[AccTransactionLinesSchema.Constants.AL_OSAmount];
			var currentPeriod = startPeriod;

			var result = new List<DebitCreditEntryItem>();
			while (currentPeriod <= endPeriod && currentPeriod != 0)
			{
				var line = CreateAndPopulateDebitCreditLine(lineGLAccount, GLDAccountTypes.TransactionLineGLAccount, localAmount, oSAmount, postDate);
				line.JournalDate = PeriodCalculator.GetLastDayForPeriod(currentPeriod);
				line.Period = currentPeriod;
				result.Add(line);
				currentPeriod = PeriodCalculator.GetNextPeriod(currentPeriod);
			}

			return result.ToArray();
		}

		protected override bool ShouldSetPeriod => false;

		protected override DebitCreditEntry CreateDebitCreditEntry(DataRow gLDDataSourceRow)
		{
			return CreateGeneralLedgerDataBasicBasedOnLine(gLDDataSourceRow);
		}
	}
}
