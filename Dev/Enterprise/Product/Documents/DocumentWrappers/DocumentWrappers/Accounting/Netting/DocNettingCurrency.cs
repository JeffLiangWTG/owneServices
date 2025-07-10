using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Netting;

namespace Enterprise.DocumentWrappers
{
	public class DocNettingCurrency : DocBaseWrapper
	{
		protected DocNettingCurrency(NettingCurrency nettingCurrency, BusinessObjectFactory factoryToWrap)
			: base(nettingCurrency, factoryToWrap)
		{
			Argument.NotNull(nettingCurrency, "NettingCurrency");
		}

		public static DocNettingCurrency New(NettingCurrency nettingCurrency, BusinessObjectFactory factoryToWrap)
		{
			return new DocNettingCurrency(nettingCurrency, factoryToWrap);
		}

		public NettingCurrency Currency
		{
			get { return (NettingCurrency)WrappedObject; }
		}

		public ZString NettingCurrency
		{
			get { return Currency.Currency; }
		}

		public ZString TransactionCurrency
		{
			get { return Currency.TransactionCurrency; }
		}

		public ZString ParticipantCurrency
		{
			get { return Currency.ParticipantCurrency; }
		}

		public ZDecimal NettingSystemExchangeRate
		{
			get { return Currency.NettingSystemExchangeRate; }
		}

		public ZDecimal ParticipantExchangeRate
		{
			get { return Currency.ParticipantExchangeRate; }
		}

		public ZDecimal CrossExchangeRate
		{
			get { return Currency.CrossExchangeRate; }
		}

		public ZDecimal ReceivableAmount
		{
			get { return Currency.ReceivableAmount; }
		}

		public ZDecimal PayableAmount
		{
			get { return Currency.PayableAmount; }
		}

		public ZDecimal TotalAmount
		{
			get { return Currency.PayableAmount + Currency.ReceivableAmount; }
		}

		public ZDecimal ExchangeValue
		{
			get { return Currency.ExchangeValue; }
		}
	}
}
