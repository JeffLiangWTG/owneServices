using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class AdditionalAddressInfo : IDataObject
	{
		[MaxLength(40), Mandatory]
		public ZString? AddressType { get; set; }
		[Mandatory]
		public CodeDescriptionPair TransportMode { get; set; }
	}
}

