using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public class EntryNumber : IDataObject
	{
		[CandidateKey, Mandatory]
		public EntryType Type { get; set; }
		[MaxLength(35), Mandatory]
		public ZString? Number { get; set; }
		[MaxLength(50)]
		public ZString? EntryLineReference { get; set; }
		public ZBool? EntryIsSystemGenerated { get; set; }
		public EntryStatus EntryStatus { get; set; }
		public ZDateTime? IssueDate { get; set; }
		public ZDateTime? ExpiryDate { get; set; }
	}
}

