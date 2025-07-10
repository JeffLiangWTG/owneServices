using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public class BankTransferCashbookExchangeDiffCreator
	{
		public void Fill(BankTransfer bankTransfer, CashbookExchangeDiff exchangeDiff)
		{
			exchangeDiff.AH_TransactionCategory = TransactionCategory.Codes.RealizedExchangeGainLoss;
			exchangeDiff.AH_InvoiceDate = bankTransfer.TransactionDate;
			exchangeDiff.AH_PostDate = bankTransfer.AH_PostDate;
			exchangeDiff.AH_Desc = GetDescription(bankTransfer, exchangeDiff);
			exchangeDiff.AH_AG = GetGLAccount(bankTransfer);
			exchangeDiff.AH_AB = exchangeDiff.IsReverseTransaction ? bankTransfer.BankTransferFromPK : bankTransfer.BankTransferToPK;
			exchangeDiff.AH_RX_NKTransactionCurrency = bankTransfer.LocalCurrency;
			exchangeDiff.AH_ExchangeRate = 1;
			exchangeDiff.AH_InvoiceAmount = bankTransfer.ExRateGainLoss;
			if (exchangeDiff.BankAccount.AB_RX_NKAccountCurrency == bankTransfer.LocalCurrency)
			{
				exchangeDiff.AH_OSTotal = bankTransfer.ExRateGainLoss;
			}
		}

		ZGuid GetGLAccount(BankTransfer bankTransfer)
		{
			if (bankTransfer.IsReverseTransaction && bankTransfer.OriginalTransaction != null)
			{
				return bankTransfer.OriginalTransaction.ExchangeDiff.AH_AG;
			}

			return bankTransfer.ExRateGainLoss < 0 ?
				AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount.Value
				: AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount.Value;
		}

		string GetDescription(BankTransfer bankTransfer, CashbookExchangeDiff exchangeDiff)
		{
			if (bankTransfer.IsReverseTransaction)
			{
				return string.Format(AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.ReversalRelated,
					Res.GetString("754e5d67-6ea0-4aff-bcbb-6b2eb0d4ffcb", "Reversal related to")) + " {0}", exchangeDiff.OriginalTransactionNumber);
			}
			return bankTransfer.ExRateGainLoss < 0 ?
				Res.GetString("7C9EC349-4DF0-4A53-B44D-77F5C67287EE", "Exchange Loss on Bank Transfer {0} / {1}", bankTransfer.TransactionNumber, bankTransfer.Reference)
				: Res.GetString("AC7000A3-BDA0-4F3C-A993-248DF297B44A", "Exchange Gain on Bank Transfer {0} / {1}", bankTransfer.TransactionNumber, bankTransfer.Reference);
		}
	}
}
