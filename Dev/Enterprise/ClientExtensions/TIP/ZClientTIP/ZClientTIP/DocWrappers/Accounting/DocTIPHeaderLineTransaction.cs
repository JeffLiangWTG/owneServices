using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;

namespace Enterprise.Client.TIP.DocWrappers
{
	public class DocTIPHeaderLineTransaction : DocumentWrapper
	{
		protected DocTIPHeaderLineTransaction(TransactionLine line, BusinessObjectFactory factoryToWrap)
			: base(line, factoryToWrap)
		{
		}

		protected DocTIPHeaderLineTransaction(DocTransactionHeader item, TransactionLine line, BusinessObjectFactory factoryToWrap)
			: this(line, factoryToWrap)
		{
			if (item == null)
			{
				throw new ArgumentNullException(nameof(item), "A null DocPaymentItem has been passed;");
			}
			fPaymentItem = item;
		}

		public static DocTIPHeaderLineTransaction New(TransactionLine transactionLine, BusinessObjectFactory factoryToWrap)
		{
			return new DocTIPHeaderLineTransaction(transactionLine, factoryToWrap);
		}

		public static DocTIPHeaderLineTransaction New(DocTransactionHeader item, TransactionLine transactionLine, BusinessObjectFactory factoryToWrap)
		{
			return new DocTIPHeaderLineTransaction(item, transactionLine, factoryToWrap);
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		TransactionHeader TransactionHeader
		{
			get { return (TransactionHeader)PaymentItem.WrappedObject; }
		}

		TransactionLine TransactionLine
		{
			get { return (WrappedObject != null) ? (TransactionLine)WrappedObject : null; }
		}

		protected DocTransactionHeader PaymentItem
		{
			get { return fPaymentItem; }
		}

		public DocTransactionHeader Invoice
		{
			get
			{
				DocTransactionHeader result = null;
				if (TransactionLine != null)
				{
					result = DocTransactionHeader.New(Factory, TransactionLine.AL_AH);
				}
				else
				{
					result = DocTransactionHeader.New(Factory, TransactionHeader.PK);
				}
				return result;
			}
		}

		#region Header Fields

		public ZString TransactionType
		{
			get { return (PaymentItem != null) ? PaymentItem.TransactionType : ZString.Empty; }
		}

		public ZDateTime InvoiceDate
		{
			get { return (PaymentItem != null) ? PaymentItem.InvoiceDate : ZDateTime.Empty; }
		}

		public ZString TransactionNumber
		{
			get { return (PaymentItem != null) ? PaymentItem.TransactionNumber : ZString.Empty; }
		}

		public ZString Desc
		{
			get { return (PaymentItem != null) ? PaymentItem.Desc : ZString.Empty; }
		}

		public DocCurrency Currency
		{
			get { return (PaymentItem != null) ? PaymentItem.Currency : null; }
		}

		public ZDecimal ExchangeRate
		{
			get { return (PaymentItem != null) ? PaymentItem.ExchangeRate : ZDecimal.Zero; }
		}

		public DocMatchLink MatchLink
		{
			get { return (PaymentItem != null) ? PaymentItem.MatchLink : null; }
		}

		#endregion

		#region Line Fields

		public ZString JobNumber
		{
			get { return (TransactionLine != null) ? TransactionLine.JobNumber : ZString.Empty; }
		}

		public ZString ChargeCode
		{
			get
			{
				ZString result = ZString.Empty;
				if (TransactionLine != null)
				{
					if (TransactionLine.ChargeCode != null)
					{
						result = TransactionLine.ChargeCode.AC_Code;
					}
					else if (TransactionLine.GLHeader != null)
					{
						result = TransactionLine.GLHeader.AG_AccountNum;
					}
				}
				else if (TransactionHeader.GLHeader != null)
				{
					result = TransactionHeader.GLHeader.AG_AccountNum;
				}
				return result;
			}
		}

		public ZString LocalExTaxAmountAsString
		{
			get { return (TransactionLine != null) ? new ZString(ZArchitecture.Core.Utilities.Round(TransactionLine.AL_LocalExTaxAmount, 2).ToString()) : ZString.Empty; }
		}

		public ZString LocalTaxAmountAsString
		{
			get { return (TransactionLine != null) ? new ZString(ZArchitecture.Core.Utilities.Round(TransactionLine.AL_LocalTaxAmount, 2).ToString()) : ZString.Empty; }
		}

		public ZString LocalTotalAmountAsString
		{
			get { return (TransactionLine != null) ? new ZString(ZArchitecture.Core.Utilities.Round(TransactionLine.AL_LocalTotalAmount, 2).ToString()) : ZString.Empty; }
		}

		#endregion

		readonly DocTransactionHeader fPaymentItem;
	}
}
