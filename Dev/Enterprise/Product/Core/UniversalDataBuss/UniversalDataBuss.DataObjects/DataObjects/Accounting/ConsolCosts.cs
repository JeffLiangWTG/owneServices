using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Accounting
{
	[XsdSchema(Placement.Inner)]
	public partial class ConsolCosts : IDataObject
	{
		public ConsolCosts()
		{
		}

		public ConsolCosts(IDataObjectWriterStrategy strategy)
		{
			SetWriterStrategy(strategy);
		}

		public List<ConsolCostLine> ConsolCostLineCollection { get; private set; }
	}
}
