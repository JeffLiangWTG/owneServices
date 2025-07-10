using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Inner)]
	public class EntryReference : IDataObject
	{
		[CandidateKey, Mandatory]
		public ZShort? LineNumber { get; set; }
		[CandidateKey, Mandatory]
		public EntryType Type { get; set; }
		[MaxLength(35), CandidateKey, Mandatory]
		public ZString? Reference { get; set; }
	}
}

