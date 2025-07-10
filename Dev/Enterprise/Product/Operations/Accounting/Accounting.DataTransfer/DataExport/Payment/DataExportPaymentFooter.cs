using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public class DataExportPaymentFooter : DataExportPayment
	{
		public DataExportPaymentFooter(BusinessObjectFactory factory, TransactionHeader payment)
			: base(factory, payment)
		{
		}
	}
}
