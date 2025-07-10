using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class GLJournalVoucherProvider : TransactionWithLinesVoucherProvider
	{
		public GLJournalVoucherProvider(AccTransactionHeader transactionHeader, IControlAccountProvider controlAccount)
			: base(transactionHeader, controlAccount)
		{ }

		public GLJournalVoucherProvider(AccTransactionHeader transactionHeader)
			: base(transactionHeader)
		{ }

		protected override VoucherLine[] GetInvoiceVoucherLines()
		{
			int count = 0;
			count = SetLineDetails(count);

			return (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
		}

		protected override int MaxVoucherLineNo
		{
			get
			{
				fMaxVoucherLineNo = Lines.Count;
				return fMaxVoucherLineNo;
			}
		}
	}
}
