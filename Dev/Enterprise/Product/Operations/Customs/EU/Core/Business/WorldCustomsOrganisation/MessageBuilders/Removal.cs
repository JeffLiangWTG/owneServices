using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using Constants = Enterprise.Customs.EU.WorldCustomsOrganisation.Constants;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;

public class Removal : Difference
{
	public Removal(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, ZString removedNode) : base(pointerParser, comparisonXml, newXml, nodeTree)
	{
		RemovedNode = removedNode;
	}

	public override ZString Type => Constants.AmendmentType.Deletion;

	public ZString RemovedNode { get; set; }

	protected override ZString GetHierarchyString()
	{
		var hierarchyString = base.GetHierarchyString();
		var xmlDoc = XDocument.Parse(ComparisonXml);
		var removedNode = xmlDoc.XPathSelectElement(XPath + @"/*" + "[" + RemovedNode + "]")?.RemoveAllNamespaces();

		if (removedNode != null)
		{
			var nodesBefore = removedNode.NodesBeforeSelf().Where(x => ((XElement)x).Name.LocalName == removedNode.Name.LocalName);
			var sequence = nodesBefore.Count() + 1;
			hierarchyString += "//" + removedNode.Name.LocalName + "[" + sequence + "]";
		}

		return hierarchyString;
	}
}
