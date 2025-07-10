using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportFileFooter : DataExportDirectDebitBatch
	{
		public DataExportFileFooter(BusinessObjectFactory factory, DirectDebitBatchHeader header)
			: base(factory, header)
		{
		}
	}
}
