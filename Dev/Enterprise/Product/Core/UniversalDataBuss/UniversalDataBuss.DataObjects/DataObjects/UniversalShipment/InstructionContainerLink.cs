using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public class InstructionContainerLink : IConfirmationParentDivot, IContainerLinkParent
	{
		public ZInt? ContainerLink { get; set; }
		public ZInt? Quantity { get; set; }

		public List<Confirmation> ConfirmationCollection { get; set; }
	}
}
