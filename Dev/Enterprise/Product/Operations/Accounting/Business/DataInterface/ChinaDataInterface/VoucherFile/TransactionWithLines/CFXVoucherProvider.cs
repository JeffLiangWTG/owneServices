using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class CFXVoucherProvider : TransactionWithLinesVoucherProvider
	{
		public CFXVoucherProvider(AccTransactionHeader invoice, IControlAccountProvider controlAccount)
			: base(invoice, controlAccount)
		{ }

		public CFXVoucherProvider(AccTransactionHeader invoice)
			: base(invoice)
		{ }

		protected override VoucherLine[] GetInvoiceVoucherLines()
		{
			int count = 0;
			count = SetCFXLineDetails(count);
			return (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
		}

		protected override int MaxVoucherLineNo
		{
			get
			{
				return Lines.Count * 2;
			}
		}

		protected int SetCFXLineDetails(int count)
		{
			fVoucherLines = new VoucherLine[MaxVoucherLineNo];
			foreach (AccTransactionLines transactionLine in Lines)
			{
				CFXVoucherLineProvider voucherLineProvider = new CFXVoucherLineProvider(transactionLine);
				if (voucherLineProvider != null)
				{
					fVoucherLines[count] = voucherLineProvider.VoucherLine;
					fVoucherLines[count + 1] = voucherLineProvider.VoucherCFXLine;
					count = count + 2;
				}
			}
			return count;
		}
	}
}
