using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class SealInfo : IDataObject
	{
		public CodeDescriptionPair Type { get; set; }

		public ZInt? Quantity { get; set; }
	}
}
