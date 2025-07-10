using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class GreenhouseGas : IDataObject
	{
		public CodeDescriptionPair Type { get; set; }
		public ZDecimal? Value { get; set; }
		public UnitOfWeight Unit { get; set; }
	}
}
