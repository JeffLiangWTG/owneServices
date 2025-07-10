using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportDirectDebitBatchFooter : DataExportDirectDebitBatch
	{
		public DataExportDirectDebitBatchFooter(BusinessObjectFactory factory, DirectDebitBatchHeader header)
			: base(factory, header)
		{
		}
	}
}
