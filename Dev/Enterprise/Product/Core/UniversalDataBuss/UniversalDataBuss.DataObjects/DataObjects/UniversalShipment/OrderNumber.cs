using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class OrderNumber : IDataObject
	{
		[MaxLength(35), CandidateKey]
		public ZString? OrderReference { get; set; }
		public ZShort? Sequence { get; set; }
	}
}

