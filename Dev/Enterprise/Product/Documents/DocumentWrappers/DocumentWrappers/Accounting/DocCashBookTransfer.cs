using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.CashBook.Transfer;

namespace Enterprise.DocumentWrappers
{
	public class DocCashBookTransfer : DocTransactionHeader
	{
		DocCashBookTransfer(BankTransferRow bankTransferRow, BusinessObjectFactory factoryToWrap)
			: base(bankTransferRow, factoryToWrap)
		{
		}

		public static DocCashBookTransfer New(BankTransferRow bankTransferRow, BusinessObjectFactory factoryToWrap)
		{
			if (bankTransferRow == null)
			{
				return null;
			}
			else
			{
				return new DocCashBookTransfer(bankTransferRow, factoryToWrap);
			}
		}
	}
}
