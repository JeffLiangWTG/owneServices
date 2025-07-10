using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.DataExport
{
	public abstract class DataExportPositivePay : AutoDataExportPositivePay
	{
		public DataExportPositivePay(BusinessObjectFactory factory, TransactionHeader[] transactionHeaders, AccBankAccount bankAccount)
			: base(factory)
		{
			TransactionHeaders = transactionHeaders;
			BankAccount = bankAccount;
		}

		public readonly TransactionHeader[] TransactionHeaders;
		public readonly AccBankAccount BankAccount;

		#region Count

		public override ZInt Count
		{
			get { return TransactionHeaders != null ? TransactionHeaders.Length : 0; }
		}

		public override ZDecimal OSTotalAmount
		{
			get { return TransactionHeaders != null ? TransactionHeaders.Sum(x => x.AH_OSTotalAmount) : 0m; }
		}

		public override ZDecimal LocalTotalAmount
		{
			get { return TransactionHeaders != null ? TransactionHeaders.Sum(x => x.AH_LocalTotalAmount) : 0m; }
		}

		#endregion

	}
}
