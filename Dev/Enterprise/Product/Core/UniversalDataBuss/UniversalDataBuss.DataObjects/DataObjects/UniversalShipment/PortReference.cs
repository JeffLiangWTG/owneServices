using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class PortReference : IDataObject
	{
		[MaxLength(35), Mandatory, CandidateKey]
		public ZString? Reference { get; set; }
		[Mandatory, CandidateKey]
		public PortReferenceType Type { get; set; }
		public PortReferenceStatus Status { get; set; }
		[CandidateKey]
		public Country Country { get; set; }
	}
}
