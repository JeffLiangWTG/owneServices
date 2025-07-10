using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentApprovalItem : AccPaymentApprovalItem
	{
		public PaymentApprovalItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TransactionHeader Header
		{
			get { return Factory.Load<TransactionHeader>(A2_AH); }
		}

		public PaymentApprovalBase Approval
		{
			get
			{
				return Factory.Load<PaymentApprovalBase>(A2_AV);
			}
		}

		[DecimalPlaces(nameof(OSDecimals))]
		public ZDecimal OSAmountPaidThisRun
		{
			get
			{
				if (TransactionHeader == null)
				{
					return 0m;
				}

				if (A2_PaymentThisRun == TransactionHeader.AH_LocalTotal)
				{
					return TransactionHeader.AH_OSTotal;
				}

				if (TransactionHeaderOSOutstandingAmountProvider.IsFeatureEnabled(TransactionHeader))
				{
					return A2_OSPaymentThisRun;
				}

				ZDecimal result = 0M;

				ZDecimal localPayAmount = A2_PaymentThisRun;
				ZDecimal exchangeRate = TransactionHeader.AH_ExchangeRate;
				ZString currencyNK = TransactionHeader.AH_RX_NKTransactionCurrency;

				if (localPayAmount != Header.AH_OutstandingAmount)
				{
					result = Env.CurrentCompany.ExchangeRate.LocalToForeign(localPayAmount, exchangeRate, currencyNK);
				}
				else
				{
					result = TransactionHeaderOSOutstandingAmountProvider.GetHighPrecisionOSOutstandingAmount(Header.AH_LocalTotal, Header.AH_OSTotal, localPayAmount, currencyNK);
				}

				return result;
			}
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal A2_PaymentThisRun
		{
			get => base.A2_PaymentThisRun;
			set => base.A2_PaymentThisRun = value;
		}

		[DecimalPlaces(nameof(OSDecimals))]
		public override ZDecimal A2_OSPaymentThisRun
		{
			get => base.A2_OSPaymentThisRun;
			set => base.A2_OSPaymentThisRun = value;
		}

		public int LocalDecimals => TransactionHeader != null ? TransactionHeader.Company.GetLocalDecimals() : GlbCompany.CurrentCompany.GetLocalDecimals();

		public int OSDecimals => TransactionHeader?.TransactionCurrency != null ? TransactionHeader.TransactionCurrency.Decimals : LocalDecimals;
	}
}
