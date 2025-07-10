using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class EntityKey : IDataObject
	{
		[MaxLength(128), Mandatory, CandidateKey]
		public ZString? Type { get; set; }
		[MaxLength(1024)]
		public ZString? Key { get; set; }
	}
}