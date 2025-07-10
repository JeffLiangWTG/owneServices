#if  DEBUG

using CargoWise.Data;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public partial class ConsolidationBatchExporter
	{
		public DbCommand GetCommandForReadingBatchContents_ExposedForTest()
		{
			return GetCommandForReadingBatchContents();
		}
	}
}

#endif