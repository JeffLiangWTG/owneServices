using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingCurrency : NonPersistentBusinessObject
	{
		public NettingCurrency(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public ZString Currency { get; set; }
		public ZString TransactionCurrency { get; set; }
		public ZString ParticipantCurrency { get; set; }
		public ZDecimal NettingSystemExchangeRate { get; set; }
		public ZDecimal CrossExchangeRate { get; set; }
		public ZDecimal ParticipantExchangeRate { get; set; }
		public ZDecimal ReceivableAmount { get; set; }
		public ZDecimal PayableAmount { get; set; }
		public ZDecimal ExchangeValue { get; set; }
	}
}
