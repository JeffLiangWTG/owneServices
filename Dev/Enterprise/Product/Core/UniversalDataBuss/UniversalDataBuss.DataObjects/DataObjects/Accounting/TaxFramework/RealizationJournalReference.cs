using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting.TaxFramework
{
	[XsdSchema(Placement.Outer)]
	public class RealizationJournalReference : IDataObject
	{
		[MaxLength(2)]
		public ZString? Ledger { get; set; } //AH_Ledger
		public TransactionType? TransactionType { get; set; }
		[MaxLength(38)]
		public ZString? TransactionNumber { get; set; } //AH_TransactionNum
		[MaxLength(3)]
		public ZString? Category { get; set; } //AH_TransactionCategory
		[MaxLength(128)]
		public ZString? Description { get; set; }
	}
}
