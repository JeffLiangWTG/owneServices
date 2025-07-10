using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class EntryNumber : IDataObject
	{
		[MaxLength(35), Mandatory, CandidateKey]
		public ZString? Number { get; set; }
		[Mandatory, CandidateKey]
		public EntryType Type { get; set; }
		[MaxLength(50)]
		public ZString? EntryLineReference { get; set; }
		[MaxLength(3)]
		public ZString? Category { get; set; }
		public ZBool? EntryIsSystemGenerated { get; set; }
		public EntryStatus EntryStatus { get; set; }
		public ZDateTime? IssueDate { get; set; }
		public ZDateTime? ExpiryDate { get; set; }
		[CandidateKey]
		public Country CountryOfIssue { get; set; }
	}
}
