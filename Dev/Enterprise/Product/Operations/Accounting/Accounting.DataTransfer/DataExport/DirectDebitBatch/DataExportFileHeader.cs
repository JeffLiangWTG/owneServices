using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportFileHeader : DataExportDirectDebitBatch
	{
		public DataExportFileHeader(BusinessObjectFactory factory, DirectDebitBatchHeader header)
			: base(factory, header)
		{
		}
	}
}
