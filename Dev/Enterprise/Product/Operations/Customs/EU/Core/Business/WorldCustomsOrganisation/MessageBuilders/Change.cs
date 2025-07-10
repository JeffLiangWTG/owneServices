using System.Collections.Generic;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using Constants = Enterprise.Customs.EU.WorldCustomsOrganisation.Constants;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;

public class Change : Difference
{
	public override ZString Type => Constants.AmendmentType.Change;

	public Change(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, ZString newValue) : base(pointerParser, comparisonXml, newXml, nodeTree)
	{
		NewValue = newValue;
	}

	public ZString NewValue { get; set; }
}
