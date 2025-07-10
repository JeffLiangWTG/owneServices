using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class DiscountVoucherProvider : ExxDisOvpVoucherProvider
	{
		public DiscountVoucherProvider(AccTransactionHeader transaction)
			: base(transaction, new ControlAccountProvider())
		{
		}

		protected override ZGuid GLAccountCore => Transaction.AH_AG;
	}
}
