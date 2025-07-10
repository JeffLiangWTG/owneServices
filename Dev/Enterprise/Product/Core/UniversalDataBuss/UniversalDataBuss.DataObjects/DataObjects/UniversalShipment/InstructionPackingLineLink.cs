using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class InstructionPackingLineLink : IConfirmationParentDivot
	{
		public ZInt? PackingLineLink { get; set; }
		public ZInt? Quantity { get; set; }

		public List<Confirmation> ConfirmationCollection { get; set; }
	}
}
