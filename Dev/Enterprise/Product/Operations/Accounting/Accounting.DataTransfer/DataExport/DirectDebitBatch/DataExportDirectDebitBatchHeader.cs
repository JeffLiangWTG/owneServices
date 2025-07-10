using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportDirectDebitBatchHeader : DataExportDirectDebitBatch
	{
		public DataExportDirectDebitBatchHeader(BusinessObjectFactory factory, DirectDebitBatchHeader header)
			: base(factory, header)
		{
		}
	}
}
