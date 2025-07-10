using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class Sea : IDataObject
	{
		[MaxLength(35)]
		[Mandatory]
		public Vessel Vessel { get; set; }

		[MaxLength(10)]
		[Mandatory]
		public ZString? VoyageNumber { get; set; }

		public CodeDescriptionPair VoyageType { get; set; }
	}
}
