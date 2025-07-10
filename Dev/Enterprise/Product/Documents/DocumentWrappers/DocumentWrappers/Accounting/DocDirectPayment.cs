using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocDirectPayment : DocTransactionHeader, IPayment
	{
		DocDirectPayment(DirectPayment directPayment, BusinessObjectFactory factoryToWrap)
			: base(directPayment, factoryToWrap)
		{
		}

		public static DocDirectPayment New(DirectPayment directPayment, BusinessObjectFactory factoryToWrap)
		{
			return (directPayment == null) ? null : new DocDirectPayment(directPayment, factoryToWrap);
		}

		DirectPayment DirectPayment
		{
			get { return (DirectPayment)WrappedObject; }
		}

		public ZDecimal InvoiceAmountAndTax
		{
			get { return DirectPayment.AH_LocalTotalAmount; }
		}

		#region Remittance Advice, Payment Voucher and Cheque Fields

		protected override ZDecimal GetPaymentVoucherOSTotalCore()
		{
			return OSTotalForRemittanceAdvice;
		}

		protected override ZDecimal GetExchangeRateForPaymentVoucherCore()
		{
			return RemittanceExchangeRate;
		}

		protected override ZDecimal GetTotalPaidAsForPaymentVoucherCore()
		{
			return OSTotalForRemittanceAdvice;
		}

		protected override ZDecimal GetTotalPaymentForPaymentVoucherCore()
		{
			return InvoiceAmountForPaymentVoucher;
		}

		protected override ZString GetRemittanceAdviceContactCore()
		{
			return RemittanceAdviceContact;
		}

		protected override ZDecimal GetOSTotalForRemittanceAdviceCore()
		{
			return OSTotalForRemittanceAdvice;
		}

		protected override ZDecimal GetSummaryTotalForRemittanceAdviceCore()
		{
			return OSTotalForRemittanceAdvice;
		}

		protected override ZString GetPaymentCurrencyCodeCore()
		{
			return PaymentCurrency == null ? ZString.Empty : PaymentCurrency.Code;
		}

		protected override ZBool GetShowOriginalAmountCore()
		{
			return ShowOriginalAmount;
		}

		protected override ZBool GetShowOriginalAmountForDPYCore()
		{
			return ShowOriginalAmountForDPY;
		}

		protected override DocGenericTransactionLineCollection GetPaidLinesCore()
		{
			var result = new DocGenericTransactionLineCollection(DirectPayment.Factory);

			foreach (DirectPaymentLine line in DirectPayment.Lines)
			{
				result.Add(DocGenericTransactionLine.New(line, Factory));
			}
			return result;
		}

		protected DocTransactionLineCollection fDirectPayLines;
		public DocTransactionLineCollection DirectPayLines
		{
			get
			{
				if (fDirectPayLines == null)
				{
					fDirectPayLines = new DocTransactionLineCollection(DirectPayment.Factory);
					foreach (DirectPaymentLine line in DirectPayment.Lines)
					{
						fDirectPayLines.Add(DocTransactionLine.New(line, Factory));
					}
				}
				return fDirectPayLines;
			}
		}

		protected DocTransactionLineCollection fFirstPageDPYLines;
		public DocTransactionLineCollection FirstPageDPYLines
		{
			get
			{
				if (fFirstPageDPYLines == null)
				{
					fFirstPageDPYLines = new DocTransactionLineCollection(DirectPayment.Factory);
					ZInt count = 1;
					foreach (DocTransactionLine dPYLine in DirectPayLines)
					{
						if (count <= NumberOfTransactionLines - (NumberOfDifferentCurrencies * 2))
						{
							fFirstPageDPYLines.Add(dPYLine);
							count++;
						}
					}
				}
				return fFirstPageDPYLines;
			}
		}

		public ZInt FirstPageDPYLinesRowCount
		{
			get { return FirstPageDPYLines.Count; }
		}

		public ZBool ShowOriginalAmountForDPY
		{
			get
			{
				ZBool result = ZBool.False;
				if (Currency != null)
				{
					foreach (DocTransactionLine invLine in DirectPayLines)
					{
						if (invLine.Currency != null && Currency.Code != invLine.Currency.Code)
						{
							result = ZBool.True;
							break;
						}
					}
				}
				return result;
			}
		}

		public virtual ZDecimal NumberOfTransactionsAndTotalsOnCheque
		{
			get
			{
				return DirectPayLines.Count + 2; // Two lines reserved for total
			}
		}

		#region IPayment Members

		public ZDecimal OSTotalForRemittanceAdvice
		{
			get
			{
				ZDecimal result = 0M;
				foreach (DocTransactionLine line in DirectPayLines)
				{
					result += line.DPYOSAmount;
				}
				return result;
			}
		}

		public ZDecimal InvoiceAmountForPaymentVoucher
		{
			get { return new ZDecimal((InvoiceAmount + GSTAmount) * -1); }
		}

		public ZDecimal RemittanceExchangeRate
		{
			get
			{
				foreach (DocTransactionLine line in DirectPayLines)
				{
					return line.ExchangeRate;
				}
				return ExchangeRate;
			}
		}

		public ZInt NumberOfDifferentCurrencies
		{
			get
			{
				ZInt result = 0;
				ArrayList currencyTable = new ArrayList();
				foreach (DocTransactionLine dPYLine in DirectPayLines)
				{
					if (dPYLine.Currency != null)
					{
						if (!currencyTable.Contains(dPYLine.Currency.Code))
						{
							result++;
							currencyTable.Add(dPYLine.Currency.Code);
						}
					}
				}
				return result;
			}
		}

		public ZString RemittanceAdviceContact
		{
			get { return ChequeDrawer; }
		}

		public ZBool IsReversal
		{
			get { return (IsCancelled && InvoiceAmount < 0); }
		}

		public ZBool ShowOriginalAmount
		{
			get { return ShowOriginalAmountForDPY; }
		}

		public ZBool PrintRemittanceOnCheque
		{
			get
			{
				if (DirectPayLines.Count + 2 > NumberOfTransactionLines)
				{
					return ZBool.False;
				}
				return ZBool.True;
			}
		}

		public DocCurrency PaymentCurrency
		{
			get
			{
				foreach (DocTransactionLine line in DirectPayLines)
				{
					return line.Currency;
				}
				return null;
			}
		}

		protected DocCheque fCheque;
		public DocCheque Cheque
		{
			get
			{
				if (fCheque == null)
				{
					fCheque = new DocCheque(ChequePayTo, OSTotalForRemittanceAdvice, PaymentCurrency, FirstLinePaymentWidth);
				}
				return fCheque;
			}
		}

		public ZString ChequePayTo
		{
			get { return ChequeDrawer; }
		}

		public ZString PaymentTransactionSummary
		{
			get { return (NoResString)"PLEASE REFER TO REMITTANCE ADVICE FOR DETAILED LIST OF TRANSACTIONS"; }
		}

		public ZString ChequePayToWithAddress
		{
			get { return DirectPayment.AH_ChequeDrawer; }
		}

		public ZString MICRNumber
		{
			get
			{
				return BuildMICRNumber(DirectPayment.AH_ChequeOrReference, RoutingTransitNumber, BankAccount);
			}
		}

		#endregion

		#endregion
	}
}
