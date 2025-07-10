using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class ExchangeDiffVoucherProvider : ExxDisOvpVoucherProvider
	{
		public ExchangeDiffVoucherProvider(AccTransactionHeader transaction)
			: base(transaction, new ControlAccountProvider())
		{
		}

		protected override ZGuid GLAccountCore
		{
			get { return Transaction.AH_AG; }
		}
	}
}
