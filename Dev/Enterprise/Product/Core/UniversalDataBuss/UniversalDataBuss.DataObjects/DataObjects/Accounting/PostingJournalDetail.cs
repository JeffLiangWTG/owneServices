using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Outer)]
	public class PostingJournalDetail : IDataObject
	{
		[MaxLength(10)]
		public GLAccount DebitGLAccount { get; set; }
		[MaxLength(10)]
		public GLAccount CreditGLAccount { get; set; }
		public ZDateTime? PostingDate { get; set; }
		public ZInt? PostingPeriod { get; set; }
		public Currency PostingCurrency { get; set; }
		public ZDecimal? PostingAmount { get; set; }
	}
}
