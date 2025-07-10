using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class PackingLink : IDataObject
	{
		[Mandatory]
		public ZInt? PackingLineLink { get; set; }
		public ZDecimal? PackedQuantity { get; set; }
		public ZBool? IsContainer { get; set; }
	}
}
