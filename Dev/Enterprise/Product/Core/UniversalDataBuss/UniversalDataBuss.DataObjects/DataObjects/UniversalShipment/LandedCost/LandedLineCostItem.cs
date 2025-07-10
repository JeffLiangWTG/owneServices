using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class LandedLineCostItem : IDataObject
	{
		[Mandatory]
		public CodeDescriptionPair CostType { get; set; }
		public ZDecimal? CostAmount { get; set; }
	}
}
