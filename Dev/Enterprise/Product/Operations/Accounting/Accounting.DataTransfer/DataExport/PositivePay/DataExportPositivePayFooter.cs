using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportPositivePayFooter : DataExportPositivePay
	{
		public DataExportPositivePayFooter(BusinessObjectFactory factory, TransactionHeader[] transactionHeaders, AccBankAccount bankAccount)
			: base(factory, transactionHeaders, bankAccount)
		{
		}
	}
}
