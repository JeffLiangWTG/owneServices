using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class ControlAccountProvider : IControlAccountProvider
	{
		public ControlAccountProvider()
		{
		}

		#region PK

		public ZGuid PK
		{
			get
			{
				if (fPK.IsEmpty)
				{
					if (Transaction.AH_Ledger == LedgerTypes.AccountsPayable)
					{
						fPK = AccountingConfigurationRegistry.Instance.APControlAccount.Value;
					}
					else if (Transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						fPK = AccountingConfigurationRegistry.Instance.ARControlAccount.Value;
					}
					else if (Transaction.AH_Ledger == LedgerTypes.CashBook)
					{
						switch (Transaction.AH_TransactionType)
						{
							case TransactionTypes.ExchangeDifference:
								fPK = Transaction.AH_AG;
								break;
						}
					}
				}
				return fPK;
			}
		}

		ZGuid fPK;

		#endregion

		#region GST

		public ZGuid GST
		{
			get
			{
				if (fGST.IsEmpty)
				{
					if (Transaction.AH_Ledger == LedgerTypes.AccountsPayable)
					{
						fGST = AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value;
					}
					else if (Transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
					{
						fGST = AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value;
					}
					else if (Transaction.AH_Ledger == LedgerTypes.CashBook)
					{
						if (Transaction.AH_TransactionType == TransactionTypes.DirectPayment)
						{
							fGST = AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value;
						}
						else if (Transaction.AH_TransactionType == TransactionTypes.DirectReceipt)
						{
							fGST = AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value;
						}
					}
				}
				return fGST;
			}
		}

		ZGuid fGST;

		#endregion

		public void SetTransaction(AccTransactionHeader transaction)
		{
			this.Transaction = transaction;
		}

		protected AccTransactionHeader Transaction;
	}
}
