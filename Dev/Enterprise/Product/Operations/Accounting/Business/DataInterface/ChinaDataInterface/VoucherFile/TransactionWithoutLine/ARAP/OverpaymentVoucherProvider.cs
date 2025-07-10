using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class OverpaymentVoucherProvider : ExxDisOvpVoucherProvider
	{
		public OverpaymentVoucherProvider(AccTransactionHeader transaction)
			: base(transaction, new ControlAccountProvider())
		{
		}

		protected override ZGuid GLAccountCore
		{
			get { return AccountingConfigurationRegistry.Instance.OverpaymentsAccount.Value; }
		}
	}
}
