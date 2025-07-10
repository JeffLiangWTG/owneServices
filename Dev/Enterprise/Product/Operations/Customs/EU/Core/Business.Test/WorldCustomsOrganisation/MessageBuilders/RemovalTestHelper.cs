using System.Collections.Generic;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;

internal class RemovalTestHelper : Removal
{
	public RemovalTestHelper(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, ZString removedNode) : base(pointerParser, comparisonXml, newXml, nodeTree, removedNode)
	{
	}

	public new ZString[] GetHierarchyStrings() => base.GetHierarchyStrings();
	public new ZString GetHierarchyString() => base.GetHierarchyString();
}
