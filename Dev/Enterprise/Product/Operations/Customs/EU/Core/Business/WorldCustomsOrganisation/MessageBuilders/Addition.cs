using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;
using Constants = Enterprise.Customs.EU.WorldCustomsOrganisation.Constants;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;

public class Addition : Difference
{
	public Addition(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree, XElement[] elementsAdded) : base(pointerParser, comparisonXml, newXml, nodeTree)
	{
		ElementsAdded = elementsAdded;
	}

	public override ZString Type => Constants.AmendmentType.Addition;

	public IReadOnlyList<XElement> ElementsAdded { get; set; }

	protected override ZString[] GetHierarchyStrings()
	{
		var hierarchyStrings = new List<ZString>();
		foreach (var elementAdd in ElementsAdded)
		{
			var elements = elementAdd.Elements();
			var descendantsToMatch = elementAdd.Descendants();

			if (descendantsToMatch.Any())
			{
				foreach (var element in elements)
				{
					var hierarchyString = GetHierarchyString();
					var sequence = GetSequence(elementAdd, descendantsToMatch);
					hierarchyString += "//" + elementAdd.Name.LocalName + GetSequenceString(sequence, elementAdd);
					sequence = GetSequence(element, element.Descendants());
					hierarchyString += "//" + element.Name.LocalName + GetSequenceString(sequence, element);
					hierarchyString = BuildHierarchyString(hierarchyString, element.Elements());
					hierarchyStrings.Add(hierarchyString);
				}
			}
			else
			{
				var hierarchyString = GetHierarchyString();
				var tag = elementAdd.Name.LocalName;
				hierarchyStrings.Add(hierarchyString + "//" + tag);
			}
		}

		return hierarchyStrings.ToArray();
	}

	ZString BuildHierarchyString(ZString hierarchyString, IEnumerable<XElement> elements)
	{
		foreach (var element in elements)
		{
			var sequence = GetSequence(element, element.Descendants());

			hierarchyString += "//" + element.Name.LocalName + GetSequenceString(sequence, element);

			hierarchyString = BuildHierarchyString(hierarchyString, element.Elements());
		}

		return hierarchyString;
	}

	string GetSequenceString(int sequence, XElement element)
	{
		return ((sequence > 0 && element.HasElements) ? "[" + sequence + "]" : string.Empty);
	}

	int GetSequence(XElement elementAdd, IEnumerable<XElement> elementsToMatch)
	{
		var sequence = 0;
		var xmlDoc = XDocument.Parse(NewXml);
		xmlDoc.Descendants().Attributes().Where(a => a.IsNamespaceDeclaration).Remove();
		var parentNode = xmlDoc.XPathSelectElement(FullXPathWithNodeNames).RemoveAllNamespaces();

		var matchString = string.Join("|", elementsToMatch.Select(y => y.Name.LocalName + "=" + y.Value));

		var descendants = parentNode.Elements(elementAdd.Name.LocalName);

		var match = descendants.FirstOrDefault(x => string.Join("|", x.Descendants().Select(z => z.Name.LocalName + "=" + z.Value)) == matchString);

		if (match == null)
		{
			sequence = 1;
		}
		else
		{
			var nodesBeforeCount = match?.NodesBeforeSelf().Count(x => ((XElement)x).Name.LocalName == elementAdd.Name.LocalName) ?? 0;
			sequence = nodesBeforeCount + 1;
		}
		return sequence;
	}
}
