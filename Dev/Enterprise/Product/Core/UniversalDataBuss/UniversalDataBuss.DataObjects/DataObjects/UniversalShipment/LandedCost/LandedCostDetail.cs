using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public partial class LandedCostDetail : IDataObject
	{
		public LandedCostDetail()
		{
		}

		public LandedCostDetail(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public ZDecimal? CustomsCostPerUnit { get; set; }
		public ZDecimal? TransportAndLogisticsCostPerUnit { get; set; }
		public ZDecimal? GoodsItemCostPerUnit { get; set; }
		public ZDecimal? MarkUp1 { get; set; }
		public ZDecimal? MarkUp2 { get; set; }
		public ZDecimal? MarkUp3 { get; set; }

		public List<LandedLineCostItem> LandedLineCostItemCollection { get; private set; }
	}
}
