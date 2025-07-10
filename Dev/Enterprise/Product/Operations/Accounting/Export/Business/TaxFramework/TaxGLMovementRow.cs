using CargoWise.Types;

namespace Enterprise.Accounting.Export.Business.TaxFramework
{
	public class TaxGLMovementRow
	{
		public ZGuid TaxTransactionPK { get; set; }
		public ZString? DebitAccountNumber { get; set; }
		public ZString? DebitAccountDescription { get; set; }
		public ZString? CreditAccountNumber { get; set; }
		public ZString? CreditAccountDescription { get; set; }
		public ZDecimal? PostingAmount { get; set; }
		public ZDate? PostingDate { get; set; }
		public ZInt? PostingPeriod { get; set; }
		public ZString? PostingCurrencyCode { get; set; }
		public ZString? PostingCurrencyName { get; set; }
	}
}
