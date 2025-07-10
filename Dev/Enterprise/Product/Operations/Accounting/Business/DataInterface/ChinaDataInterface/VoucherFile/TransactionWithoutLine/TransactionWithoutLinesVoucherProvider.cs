using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class TransactionWithoutLinesVoucherProvider : VoucherProvider
	{
		public TransactionWithoutLinesVoucherProvider(AccTransactionHeader transaction, IControlAccountProvider controlAccountProvider)
			: base(transaction, controlAccountProvider)
		{
		}

		public override VoucherLine[] VoucherLines
		{
			get
			{
				if (fVoucherLines == null)
				{
					fVoucherLines = new VoucherLine[MaxVoucherLineNo];

					fVoucherLines[0] = new VoucherLine(Transaction);
					fVoucherLines[1] = new VoucherLine(Transaction);

					SetOriginalVoucherLine(fVoucherLines[0]);
					SetControlAccountVoucherLine(fVoucherLines[1]);
				}
				return (fVoucherLines.OrderBy(x => x.CreditAmount != 0)).ToArray();
			}
#if DEBUG
			set
			{
				fVoucherLines = value;
			}
#endif
		}

		protected override int MaxVoucherLineNo
		{
			get { return 2; }
		}
	}
}
