using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public abstract class ExxDisOvpVoucherProvider : TransactionWithoutLinesVoucherProvider
	{
		public ExxDisOvpVoucherProvider(AccTransactionHeader transaction, IControlAccountProvider controlAccountProvider)
			: base(transaction, controlAccountProvider)
		{
		}

		public ZGuid GLAccount
		{
			get
			{
				if (!fGLAccount.IsValid)
				{
					fGLAccount = GLAccountCore;
				}
				return fGLAccount;
			}
		}

		protected abstract ZGuid GLAccountCore { get; }
		internal protected ZGuid fGLAccount;

		protected override ZGuid GetGLAccountPKFromTransactionHeader()
		{
			return GLAccount;
		}
	}
}