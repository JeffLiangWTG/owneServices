using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile
{
	public class VoucherDescriptionLookUp
	{
		public VoucherDescriptionLookUp(AccTransactionHeader transaction)
		{
			this.Transaction = transaction;
		}

		protected AccTransactionHeader Transaction;

		public ZString GetDescription()
		{
			ZString description = "";

#pragma warning disable CW1161 // strings are already localised
			if (DataInterfaceUtils.GetLocalLanguage() == Core.Constants.Languages.ChineseSimplified)
			{
				switch (Transaction.AH_TransactionType)
				{
					case TransactionTypes.AdjustmentNote:
					case TransactionTypes.Invoice:
					case TransactionTypes.CreditNote:
						if (IsAR())
						{
							description = "营业收入";
						}
						else if (IsAP())
						{
							description = "营业成本";
						}
						break;
					case TransactionTypes.Contra:
						description = "应收/应付抵账";
						break;
					case TransactionTypes.DirectPayment:
						description = "直接付款单";
						break;
					case TransactionTypes.DirectReceipt:
						description = "直接收款单";
						break;
					case TransactionTypes.Discount:
						if (IsAR())
						{
							description = "应收回扣";
						}
						else if (IsAP())
						{
							description = "应付回扣";
						}
						break;
					case TransactionTypes.ExchangeDifference:
						if (IsAR())
						{
							description = "应收利息差异";
						}
						else if (IsAP())
						{
							description = "应付利息差异";
						}
						else if (IsCB())
						{
							description = "银行利息差异";
						}
						break;
					case TransactionTypes.GLStandardJournal:
						description = "总账凭证";
						break;
					case TransactionTypes.Journal:
						if (IsAR())
						{
							description = "应收凭证";
						}
						else if (IsAP())
						{
							description = "应付凭证";
						}
						else if (IsJobCosting())
						{
							description = "营业凭证";
						}
						break;
					case TransactionTypes.Overpayment:
						if (IsAR())
						{
							description = "应收预付账款";
						}
						else if (IsAP())
						{
							description = "应付预付账款";
						}
						break;
					case TransactionTypes.Payment:
						description = "银行支出";
						break;
					case TransactionTypes.Receipt:
						description = "银行收入";
						break;
					case TransactionTypes.Transfer:
						if (IsAR())
						{
							description = "应收转账";
						}
						else if (IsAP())
						{
							description = "应付转账";
						}
						else if (IsCB())
						{
							description = "银行转账";
						}
						break;
					case TransactionTypes.JobRevenueJournal:
						description = "营业凭证";
						break;
				}
			}
			else
			{
				description = Transaction.AH_Desc;
			}
#pragma warning restore CW1161

			return description;
		}

		protected bool IsAR()
		{
			return Transaction.AH_Ledger == LedgerTypes.AccountsReceivable;
		}

		protected bool IsAP()
		{
			return Transaction.AH_Ledger == LedgerTypes.AccountsPayable;
		}

		protected bool IsCB()
		{
			return Transaction.AH_Ledger == LedgerTypes.CashBook;
		}

		protected bool IsJobCosting()
		{
			return Transaction.AH_Ledger == LedgerTypes.JobCosting;
		}
	}
}
