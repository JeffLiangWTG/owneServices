using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocMatchLink : DocumentWrapper
	{
		DocMatchLink(AccTransactionMatchLink transactionMatchLink, BusinessObjectFactory factoryToWrap)
			: base(transactionMatchLink, factoryToWrap)
		{
		}

		public static DocMatchLink New(AccTransactionMatchLink transactionMatchLink, BusinessObjectFactory factoryToWrap)
		{
			if (transactionMatchLink == null)
			{
				return null;
			}
			else
			{
				return new DocMatchLink(transactionMatchLink, factoryToWrap);
			}
		}

		AccTransactionMatchLink TransactionMatchLink
		{
			get { return (AccTransactionMatchLink)WrappedObject; }
		}

		public override string ToString()
		{
			return MatchGroupNum;
		}

		public DocARInvoice Invoice
		{
			get
			{
				if (TransactionMatchLink.AP_AH.IsValid)
				{
					var aPInvoice = Factory.Load<APInvoice>(TransactionMatchLink.AP_AH);
					return DocARInvoice.New(aPInvoice, Factory);
				}
				return null;
			}
		}

		public ZDecimal Amount
		{
			get { return TransactionMatchLink.AP_Amount * Invertamount; }
		}

		public ZDecimal MatchAmount
		{
			get { return TransactionMatchLink.AP_Amount; }
		}
		public ZDecimal OSMatchAmount
		{
			get { return GetOSAmount(MatchAmount); }
		}

		public ZDecimal GSTRealised
		{
			get { return TransactionMatchLink.AP_GSTRealised; }
		}

		public ZDateTime MatchDate
		{
			get { return TransactionMatchLink.AP_MatchDate; }
		}

		public ZString MatchGroupNum
		{
			get { return TransactionMatchLink.AP_MatchGroupNum; }
		}

		public ZInt MatchPeriod
		{
			get { return TransactionMatchLink.AP_MatchPeriod; }
		}

		public ZString Reason
		{
			get { return TransactionMatchLink.AP_Reason; }
		}

		public ZString CreatorName
		{
			get { return TransactionMatchLink.Logs.AddedLog != null ? TransactionMatchLink.Logs.AddedLog.User.GS_FullName : ZString.Empty; }
		}

		public ZDecimal InvertedOSAmount
		{
			get { return GetOSAmount(Amount); }
		}

		#region Implementation
		protected ZInt Invertamount
		{
			get
			{
				if (Invoice != null)
				{
					return (Invoice.TransactionType == ZArchitecture.Core.TransactionTypes.Payment && Invoice.Ledger == ZArchitecture.Core.LedgerTypes.AccountsReceivable) ? 1 : -1;
				}
				return 1;
			}
		}
		protected ZDecimal GetOSAmount(ZDecimal localAmountToConvert)
		{
			ZDecimal result = 0;

			if (TransactionMatchLinkOSAmountProvider.IsFeatureEnabled(TransactionMatchLink))
			{
				result = TransactionMatchLink.AP_OSAmount;
			}
			else
			{
				if (Invoice != null && !Invoice.ExchangeRate.IsEmpty && Invoice.Currency != null)
				{
					if (Amount == Invoice.InvoiceAmount + Invoice.GSTAmount && Invoice.OutstandingAmount == 0M)
					{
						result = Invoice.OSTotal;
					}
					else
					{
						RefCurrency oSCurrency = RefCurrency.LoadFromCurrencyCode(Factory, Invoice.Currency.Code);

						if (oSCurrency != null)
						{
							result = Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmountToConvert, Invoice.ExchangeRate, oSCurrency.RX_Code);
						}
					}
				}
			}

			if (SignsAreDifferent(result, localAmountToConvert))
			{
				result *= -1;
			}

			return result;
		}

		bool SignsAreDifferent(ZDecimal result, ZDecimal localAmountToConvert)
		{
			return (result > 0 && localAmountToConvert < 0) ||
				(result < 0 && localAmountToConvert > 0);
		}

		#endregion
	}
}
