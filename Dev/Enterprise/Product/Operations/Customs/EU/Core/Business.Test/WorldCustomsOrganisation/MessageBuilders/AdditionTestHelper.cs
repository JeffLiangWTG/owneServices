using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing;

internal class AdditionTestHelper : Addition
{
	public AdditionTestHelper(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, XElement[] elementsAdded) : base(pointerParser, comparisonXml, newXml, nodeTree, elementsAdded)
	{
	}

	public new ZString[] GetHierarchyStrings() => base.GetHierarchyStrings();
	public new ZString GetHierarchyString() => base.GetHierarchyString();
}
