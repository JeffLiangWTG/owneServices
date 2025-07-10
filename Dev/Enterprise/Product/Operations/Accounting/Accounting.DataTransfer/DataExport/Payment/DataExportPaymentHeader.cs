using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportPaymentHeader : DataExportPayment
	{
		public DataExportPaymentHeader(BusinessObjectFactory factory, TransactionHeader payment)
			: base(factory, payment)
		{
		}
	}
}
