using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class Reference : IDataObject
	{
		[CandidateKey, Mandatory]
		public EntryType Type { get; set; }
		[CandidateKey, MaxLength(35)]
		public ZString? ReferenceNumber { get; set; }
	}
}