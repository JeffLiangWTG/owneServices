#if DEBUG

using System.Data;
using CargoWise.Types;

namespace Enterprise.Accounting.ReportTableProviders
{
	public partial class GLAccountDocumentDataProvider
	{
		public int MaximumRowsBeforeException_ForTestOnly => MaximumRowsBeforeException;

		public void SetupParameterValues_ForTestOnly(object[] values)
		{
			SetupParameterValues(values);
		}

		public ZDateTime EndDate_ForTestOnly
		{
			get { return EndDate; }
			set { EndDate = value; }
		}

		public int[] PeriodRange_ForTestOnly
		{
			get { return PeriodRange; }
			set { PeriodRange = value; }
		}

		public Account FRetainedEarningsPreviousYear_ForTestOnly
		{
			get { return fRetainedEarningsPreviousYear; }
			set { fRetainedEarningsPreviousYear = value; }
		}

		public int FindBeginningPeriodOffsetForTheAutoJournal_ForTestOnly(int autoJournalPeriod)
		{
			return FindBeginningPeriodOffsetForTheAutoJournal(autoJournalPeriod);
		}

		public bool FShowHeaderDescription_ForTestOnly
		{
			get { return fShowHeaderDescription; }
			set { fShowHeaderDescription = value; }
		}

		public DataRow AddRow_ForTestOnly(GLAccountListDocumentDataSet.GLAccountListDataSetRow rowToAdd, GLAccountListDocumentDataSet dataSetToProcess) => AddRow(rowToAdd, dataSetToProcess);

		public DataTable GetDataTable_ForTestOnly() => GetDataTable();

		public void SetDatesFromPeriods_ForTestOnly() => SetDatesFromPeriods();
	}
}

#endif
