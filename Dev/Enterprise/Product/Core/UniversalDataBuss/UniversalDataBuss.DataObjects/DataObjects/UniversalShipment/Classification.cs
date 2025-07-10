using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class Classification : ICodeDescriptionDataObject
	{
		[MaxLength(15), CandidateKey, Mandatory]
		public ZString? Code { get; set; }

		[MaxLength(35)]
		public ZString? Description { get; set; }

		[CandidateKey, Mandatory]
		public Country Country { get; set; }

		[CandidateKey, Mandatory]
		public CodeDescriptionPair Type { get; set; }
	}
}