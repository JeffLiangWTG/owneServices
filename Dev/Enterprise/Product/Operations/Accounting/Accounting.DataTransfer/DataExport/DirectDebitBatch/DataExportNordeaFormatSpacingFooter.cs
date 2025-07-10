using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportNordeaFormatSpacingFooter : DataExportDirectDebitBatch
	{
		public DataExportNordeaFormatSpacingFooter(BusinessObjectFactory factory, DirectDebitBatchHeader header)
			: base(factory, header)
		{
		}
	}
}
