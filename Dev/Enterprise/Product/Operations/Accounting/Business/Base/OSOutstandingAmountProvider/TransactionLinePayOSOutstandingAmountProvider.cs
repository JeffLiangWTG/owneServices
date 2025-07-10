using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Matching;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public static class TransactionLinePayOSOutstandingAmountProvider
	{
		public static void SetPaymentAmounts(AccTransactionHeader header, AccTransLinePay linePay, ZDecimal localAmount, ZDecimal osAmount)
		{
			linePay.A7_OSAmount = TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(header)
				? osAmount
				: new ZDecimal(0m);

			linePay.A7_Amount = localAmount;
		}

		public static ZDecimal GetTransLinePaysTotalOSAmount(InvoicingLineBase line)
		{
			ILineMatching lineMatching = line;

			return TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(line.TransactionHeader)
				? (ZDecimal)line.TransLinePays.Sum(x => x.A7_OSAmount)
				: (ZDecimal)Env.CurrentCompany.ExchangeRate.LocalToForeign(line.TransLinePaysTotalAmount, lineMatching.AL_ExchangeRate, lineMatching.AL_RX_NKTransactionCurrency);
		}
	}
}
