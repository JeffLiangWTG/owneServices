using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class WIPAccrualControlAccountProvider : IControlAccountProvider
	{
		protected readonly string LineTypes;

		public WIPAccrualControlAccountProvider(string lineTypes)
		{
			this.LineTypes = lineTypes;
		}

		protected ZGuid fPK;
		public ZGuid PK
		{
			get
			{
				if (!fPK.IsValid)
				{
					switch (LineTypes)
					{
						case TransactionLineTypes.Accrual:
							fPK = AccountingConfigurationRegistry.Instance.AccruedCostControlAccount.Value;
							break;
						case TransactionLineTypes.WIP:
							fPK = AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount.Value;
							break;
					}
				}
				return fPK;
			}
		}

		public ZGuid GST
		{
			get
			{
				return ZGuid.Empty;
			}
		}

		public void SetTransaction(AccTransactionHeader transaction)
		{
		}
	}
}
