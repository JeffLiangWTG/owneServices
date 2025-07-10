using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class TransportLogisticsCost : IDataObject
	{
		public Accounting.ChargeCode ChargeCode { get; set; }
		[MaxLength(35)]
		public ZString? ChargeDescription { get; set; }
		public CodeDescriptionPair LandedCostGroup { get; set; }
		public CodeDescriptionPair DistributeCostBy { get; set; }
		public ZDecimal? CostAmount { get; set; }
		public Currency CostCurrency { get; set; }
		public ZDecimal? ServiceExRate { get; set; }
	}
}
