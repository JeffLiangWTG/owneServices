using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Messaging.Business;
using Microsoft.XmlDiffPatch;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Message helper no translation required")]
	public abstract class AmendmentMessageHelper
	{
		public AmendmentMessageHelper(IPointerParser pointerParser)
		{
			Parser = pointerParser;
		}

		protected readonly IPointerParser Parser;

		protected virtual List<XNamespace> NamespacesToKeep => new List<XNamespace>();

		public bool CanBeAmended(WCOJobDeclarationMessageSendingObject objectToSend)
		{
			bool canBeAmended = false;
			var xmlForComparison = GetLatestMessageForComparison(objectToSend);
			EDIMessage newMessageForComparison = null;
			if (!xmlForComparison.IsEmpty)
			{
				newMessageForComparison = CreateNewMessageForComparison(objectToSend);
				var diffGram = GetDiffGram(xmlForComparison, newMessageForComparison?.EM_MessageText ?? ZString.Empty, NamespacesToKeep);
				objectToSend.AmendmentDetails = new AmendmentDetails(diffGram, xmlForComparison, newMessageForComparison.EM_MessageText);

				if (diffGram.HasElements)
				{
					if (CanProcessAmendment(diffGram))
					{
						canBeAmended = true;
						objectToSend.AmendmentDetails.Amendments = GetAmendments(objectToSend);
					}
					else
					{
						objectToSend.ShouldSend = false;
					}
				}
				else
				{
					objectToSend.ShouldSend = false;
				}
			}

			if (!canBeAmended && newMessageForComparison != null)
			{
				objectToSend.Header?.Messages.Remove(newMessageForComparison);
			}

			if (objectToSend.MessageType == WCOEDIMessageTypeList.Codes.ArrivalNotification)
			{
				objectToSend.ShouldSend = true;
			}

			return canBeAmended;
		}

		static bool CanProcessAmendment(XElement diffGram)
		{
			return !diffGram.Descendants("descriptor").Attributes("opid").Any() &&
				!diffGram.Descendants("change").Attributes("name").Any();
		}

		protected abstract EDIMessage CreateNewMessageForComparison(WCOJobDeclarationMessageSendingObject objectToSend);

		internal AmendmentObjectWrapper GetAmendments(WCOJobDeclarationMessageSendingObject objectToSend)
		{
			var differences = GetDifferences(objectToSend);
			if (differences.Any())
			{
				var amendmentXml = GetAmendmentXmlFromDiffGram(objectToSend);

				return new AmendmentObjectWrapper
				{
					Differences = differences,
					Xml = amendmentXml
				};
			}

			return null;
		}

		XElement GetAmendmentXmlFromDiffGram(WCOJobDeclarationMessageSendingObject objectToSend)
		{
			var newXmlDoc = XDocument.Parse(objectToSend.AmendmentDetails?.NewXml);
			var oldXmlDoc = XDocument.Parse(objectToSend.AmendmentDetails?.ComparisonXml);
			var amendmentXML = new XElement(Declaration);

			if (MetaDataDeclarationXPathValue != null)
			{
				var element = objectToSend.AmendmentDetails?.DiffGram.XPathSelectElement(MetaDataDeclarationXPathValue); // Start at MetaData/Declaration level
				var alwaysAddElements = GetAlwaysAddElements(newXmlDoc);

				var xPath = GetXPath(GetNodeHierarchyList(element.Elements().First()));
				var selectedNode = oldXmlDoc.XPathSelectElement(xPath);

				RecursivelySpanDiffGram(element, amendmentXML, newXmlDoc, oldXmlDoc, alwaysAddElements);
				ProcessAlwaysAddNode(amendmentXML, alwaysAddElements, selectedNode, 9999);
			}

			return amendmentXML;
		}

		static XElement RecursivelySpanDiffGram(XElement elements, XElement amendmentXML, XDocument newXml, XDocument oldXml, List<AlwaysAdd> alwaysAddElements)
		{
			foreach (var element in elements.Elements())
			{
				if (IsMatchNode(element))
				{
					ProcessMatchNode(amendmentXML, newXml, oldXml, element, alwaysAddElements);
				}
				else if (IsAddNode(element) && !IsAlreadyInAlwaysAddListForAdd(alwaysAddElements, element, oldXml))
				{
					ProcessAddNode(amendmentXML, oldXml, element, newXml, alwaysAddElements);
				}
				else if (IsChangeNode(element) && !IsAlreadyInAlwaysAddListForChange(alwaysAddElements, element, oldXml))
				{
					ProcessChangeNode(amendmentXML, oldXml, newXml, element);
				}
			}

			return amendmentXML;
		}

		static bool IsAlreadyInAlwaysAddListForAdd(List<AlwaysAdd> alwaysAddElements, XElement element, XDocument oldXml)
		{
			var xPath = GetXPath(GetNodeHierarchyList(element.Parent));
			var selectedNode = oldXml.XPathSelectElement(xPath);

			if (alwaysAddElements.Count > 0 && alwaysAddElements.Any(x => x.RootName.Equals(selectedNode.Name.LocalName) && x.Name.Equals(((XElement)element.FirstNode).Name.LocalName)))
			{
				DecreaseIndexOfElementsStillToAdd(alwaysAddElements, element, oldXml);
				return true;
			}

			return false;
		}

		static bool IsAlreadyInAlwaysAddListForChange(List<AlwaysAdd> alwaysAddElements, XElement element, XDocument oldXml)
		{
			var xPath = GetXPath(GetNodeHierarchyList(element));
			var selectedNode = oldXml.XPathSelectElement(xPath);

			if (alwaysAddElements.Count > 0 && alwaysAddElements.Any(x => x.RootName.Equals(selectedNode.Parent.Name.LocalName) && x.Name.Equals(selectedNode.Name.LocalName)))
			{
				return true;
			}

			return false;
		}

		static void ProcessChangeNode(XElement amendmentXML, XDocument oldXml, XDocument newXml, XElement element)
		{
			var elementToChange = element?.Parent?.Parent;
			var xPath = GetXPath(GetNodeHierarchyList(elementToChange));
			var selectedNode = oldXml.XPathSelectElement(xPath);

			GetAttributes(oldXml, newXml, element).ToList().ForEach(x =>
			{
				if (amendmentXML.Attribute(x.Name) == null)
				{
					amendmentXML.Add(new XAttribute(x.Name, x.Value));
				}
			});

			if (!IsAttributeChange(element))
			{
				AddEmptySiblingNodesForChange(amendmentXML, selectedNode);
				amendmentXML.Value = element.Value;
			}
			else
			{
				amendmentXML.Value = GetValue(oldXml, newXml, element);
			}
		}

		public static ZString GetFullXPathWithNodeNames(XDocument oldXml, ZString xPath)
		{
			var selectedNode = oldXml.XPathSelectElement(xPath);

			var hierarchy = new List<XElement>(new XElement[] { selectedNode });
			var e = selectedNode;
			while (e.Parent != null)
			{
				e = e.Parent;
				if (e.NodeType == XmlNodeType.Document)
				{
					break;
				}
				hierarchy.Add(e);
			}

			var result = @"//*";
			result += string.Join("/*", hierarchy.Select(x => "[local-name() = '" + x.Name.LocalName + "']" + "[" + x.Sequence() + "]").Reverse());

			return result;
		}

		static bool IsAttributeChange(XElement element) => (element.Attribute("match")?.Value ?? ZString.Empty).StartsWith("@", System.StringComparison.OrdinalIgnoreCase);

		static IEnumerable<XAttribute> GetAttributes(XDocument oldXml, XDocument newXml, XElement element)
		{
			var selectedNode = GetNode(oldXml, newXml, element);
			return selectedNode?.Attributes() ?? new List<XAttribute>();
		}

		static ZString GetValue(XDocument oldXml, XDocument newXml, XElement element)
		{
			var selectedNode = GetNode(oldXml, newXml, element);
			return selectedNode?.Value ?? ZString.Empty;
		}

		static XElement GetNode(XDocument oldXml, XDocument newXml, XElement element)
		{
			var parent = element?.Parent;
			var xPath = GetXPath(GetNodeHierarchyList(parent));
			var xPathWithFullNodeNames = GetFullXPathWithNodeNames(oldXml, xPath);
			var selectedNode = newXml.XPathSelectElement(xPathWithFullNodeNames);
			return selectedNode;
		}

		protected static void ProcessAddNode(XElement amendmentXML, XDocument oldXml, XElement element, XDocument newXml, List<AlwaysAdd> alwaysAddElements)
		{
			var emptySiblingNodesAdded = new List<ZString>();
			foreach (var elementToAdd in element.Elements())
			{
				if (HasSequenceNumeric(elementToAdd))
				{
					amendmentXML.Add(elementToAdd);
				}
				else
				{
					var nodeName = elementToAdd.Name.LocalName;

					if (!emptySiblingNodesAdded.Contains(nodeName))
					{
						AddEmptySiblingNodesForAdd(amendmentXML, oldXml, element, elementToAdd);
						emptySiblingNodesAdded.Add(nodeName);
					}
					amendmentXML.Add(elementToAdd);
				}

				DecreaseIndexOfElementsStillToAdd(alwaysAddElements, element, oldXml);
			}
		}

		static void DecreaseIndexOfElementsStillToAdd(List<AlwaysAdd> alwaysAddElements, XElement element, XDocument oldXml)
		{
			if (alwaysAddElements.Count > 0)
			{
				var xPath = GetXPath(GetNodeHierarchyList(element.Parent));
				var selectedNode = oldXml.XPathSelectElement(xPath);

				if (alwaysAddElements.First().RootName.Equals(selectedNode.Name.LocalName))
				{
					foreach (var alwaysAddElement in alwaysAddElements)
					{
						if (alwaysAddElement.Index > -1)
						{
							alwaysAddElement.Index--;
						}
					}
				}
			}
		}

		static void ProcessMatchNode(XElement amendmentXML, XDocument newXML, XDocument oldXml, XElement element, List<AlwaysAdd> alwaysAddElements)
		{
			var xPath = GetXPath(GetNodeHierarchyList(element));
			var selectedNode = oldXml.XPathSelectElement(xPath);
			var positionInTheDocument = Int32.Parse(element.Attribute("match").Value);
			ProcessAlwaysAddNode(amendmentXML, alwaysAddElements, selectedNode, positionInTheDocument);

			if (element.HasElements)
			{
				if (selectedNode != null)
				{
					var newElement = new XElement(selectedNode.Name.LocalName);

					var sequenceNumericNode = selectedNode.RemoveAllNamespaces().Element(SequenceNumeric);
					if (sequenceNumericNode != null)
					{
						newElement.Add(sequenceNumericNode);
					}
					else
					{
						AddEmptySiblingNodesForMatch(amendmentXML, selectedNode);
					}
					amendmentXML.Add(newElement);
					
					RecursivelySpanDiffGram(element, newElement, newXML, oldXml, alwaysAddElements);
				}
			}
		}

		static void ProcessAlwaysAddNode(XElement amendmentXML, List<AlwaysAdd> alwaysAddElements, XElement selectedNode, int positionInTheDocument)
		{
			var processedElements = new List<AlwaysAdd>();

			if (alwaysAddElements.Count != 0)
			{
				if (alwaysAddElements.First().RootName.Equals(selectedNode.Parent.Name.LocalName))
				{
					foreach (var alwaysAddElement in alwaysAddElements)
					{
						if (alwaysAddElement.Index > -1 && alwaysAddElement.Index <= positionInTheDocument)
						{
							amendmentXML.Add(alwaysAddElement.Node);
							processedElements.Add(alwaysAddElement);
						}
					}

					foreach (var processedElement in processedElements)
					{
						processedElement.Index = -1;
					}
				}
			}
		}

		static bool IsAddNode(XElement element) => element.Name.LocalName == Add;

		static bool IsChangeNode(XElement element) => element.Name.LocalName == Change && !string.IsNullOrEmpty(element.Attribute(Match)?.Value);

		static bool IsMatchNode(XElement element) => element.Name.LocalName == Node && !string.IsNullOrEmpty(element.Attribute(Match)?.Value);

		protected static ZString GetXPath(IEnumerable<Node> nodes)
		{
			return string.Format(CultureInfo.InvariantCulture, @"//*{0}", string.Join(@"/*", nodes.Select(x => "[" + x.Index + "]")));
		}

		IEnumerable<Difference> GetDifferences(WCOJobDeclarationMessageSendingObject objectToSend)
		{
			var differences = new List<Difference>();

			AddChangeNodes(objectToSend.AmendmentDetails.ComparisonXml, objectToSend.AmendmentDetails.NewXml, differences, objectToSend.AmendmentDetails.DiffGram.Descendants().Where(e => e.Name.LocalName == Change));
			AddAddNodes(objectToSend.AmendmentDetails.ComparisonXml, objectToSend.AmendmentDetails.NewXml, differences, objectToSend.AmendmentDetails.DiffGram.Descendants().Where(e => e.Name.LocalName == Add));
			AddRemoveNodes(objectToSend.AmendmentDetails.ComparisonXml, objectToSend.AmendmentDetails.NewXml, differences, objectToSend.AmendmentDetails.DiffGram.Descendants().Where(e => e.Name.LocalName == Remove));

			return differences;
		}

		void AddRemoveNodes(ZString comparisonXml, ZString newXml, List<Difference> differences, IEnumerable<XElement> removeNodes)
		{
			foreach (var node in removeNodes)
			{
				var parents = node.AncestorsAndSelf().Where(e => e.Name.LocalName == Node).Select(x => new Node { Index = x.Attribute(Match).Value, NodeName = x.Name.LocalName }).Reverse().ToList();
				var value = node.Attribute(Match).Value;
				var splitValue = value.Split('-');

				foreach (var split in splitValue)
				{
					var splitParents = new List<Node>(parents);
					differences.Add(new Removal(Parser, comparisonXml, newXml, splitParents, split));
				}
			}
		}

		void AddAddNodes(ZString comparisonXml, ZString newXml, List<Difference> differences, IEnumerable<XElement> addNodes)
		{
			foreach (var node in addNodes)
			{
				var parents = node.AncestorsAndSelf().Where(e => e.Name.LocalName == Node).Select(x => new Node { Index = x.Attribute(Match).Value, NodeName = x.Name.LocalName }).Reverse();
				differences.Add(new Addition(Parser, comparisonXml, newXml, parents, node.Elements().ToArray()));
			}
		}

		void AddChangeNodes(ZString comparisonXml, ZString newXml, List<Difference> differences, IEnumerable<XElement> changesNodes)
		{
			foreach (var node in changesNodes)
			{
				var parents = node.AncestorsAndSelf().Where(e => e.Name.LocalName == Node).Select(x => new Node { Index = x.Attribute(Match).Value, NodeName = x.Name.LocalName }).Reverse();
				differences.Add(new Change(Parser, comparisonXml, newXml, parents, node.Value));
			}
		}

		static XmlReader GetXmlReader(ZString xml)
		{
			return XmlReader.Create(new StringReader(xml));
		}

		protected static XElement GetDiffGram(ZString comparisonXml, ZString newXml, List<XNamespace> namespacesToKeep)
		{
			comparisonXml = comparisonXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", "");
			newXml = newXml.Replace("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n", "");

			var comparisonReader = GetXmlReader(comparisonXml);
			var newReader = GetXmlReader(newXml);

			using (var sw = new StringWriter(CultureInfo.InvariantCulture))
			{
				using (var xw = XmlWriter.Create(sw))
				{
					var ignore = XmlDiffOptions.IgnoreComments |
					XmlDiffOptions.IgnoreDtd |
					XmlDiffOptions.IgnorePI |
					XmlDiffOptions.IgnoreXmlDecl |
					XmlDiffOptions.IgnorePrefixes |
					XmlDiffOptions.IgnoreNamespaces;

					var xmldiff = new XmlDiff(ignore);
					xmldiff.Algorithm = XmlDiffAlgorithm.Fast;
					xmldiff.Compare(comparisonReader, newReader, xw);
					xw.Close();
				}

				var result = RemoveNamespacesExcept(XElement.Parse(sw.ToString()), namespacesToKeep);

				return result;
			}
		}

		public static XElement RemoveNamespacesExcept(XElement element, List<XNamespace> namespacesToKeep)
		{
			if (element == null)
			{
				return null;
			}

			var newElement = new XElement(XName.Get(element.Name.LocalName));

			foreach (var attribute in element.Attributes())
			{
				if (namespacesToKeep.Contains(attribute.Name.Namespace) || !attribute.IsNamespaceDeclaration)
				{
					newElement.Add(new XAttribute(attribute.Name.LocalName, attribute.Value));
				}
			}

			if (!element.HasElements && !string.IsNullOrEmpty(element.Value))
			{
				newElement.Value = element.Value;
			}

			foreach (var child in element.Elements())
			{
				var newChild = RemoveNamespacesExcept(child, namespacesToKeep);
				newElement.Add(newChild);
			}

			if (namespacesToKeep.Contains(element.Name.Namespace))
			{
				newElement.Name = XName.Get(newElement.Name.LocalName, element.Name.NamespaceName);
			}

			return newElement;
		}

		static int GetSiblingCount(IEnumerable<XNode> nodes, ZString nodeName)
		{
			return nodes == null ? 0 : nodes.Count(x => ((XElement)x).Name.LocalName == nodeName);
		}

		static int GetSiblingCount(XElement element, ZString nodeName)
		{
			var count = 0;
			var sibling = (element.PreviousNode as XElement);

			while (sibling != null && sibling.Name.LocalName.Equals(nodeName))
			{
				count++;
				sibling = (sibling.PreviousNode as XElement);
			}

			return count;
		}

		static void AddEmptySiblingNodesForMatch(XElement amendmentXML, XElement selectedNode)
		{
			if (selectedNode != null && HaveEmptyNodesBeenAdded(amendmentXML, selectedNode))
			{
				var siblingCount = GetSiblingCount(selectedNode.NodesBeforeSelf(), selectedNode.Name.LocalName);
				var siblingsAlreadyPresentCount = GetSiblingCount(amendmentXML.Elements().Where(e => e.Name.LocalName == selectedNode.Name.LocalName), selectedNode.Name.LocalName);

				if (siblingCount > siblingsAlreadyPresentCount)
				{
					for (int i = siblingsAlreadyPresentCount; i < siblingCount; i++)
					{
						AddEmptySiblingNodeForMatch(amendmentXML, selectedNode);
					}
				}
			}
		}

		static void AddEmptySiblingNodesForChange(XElement amendmentXML, XElement selectedNode)
		{
			if (selectedNode != null && HaveEmptyNodesBeenAdded(amendmentXML, selectedNode))
			{
				var siblingCount = GetSiblingCount(selectedNode.NodesBeforeSelf(), selectedNode.Name.LocalName);
				var siblingsAlreadyPresentCount = GetSiblingCount(amendmentXML.Ancestors().FirstOrDefault(e => e.Name.LocalName == selectedNode.Name.LocalName)?.ElementsBeforeSelf(), selectedNode.Name.LocalName);

				if (siblingCount > siblingsAlreadyPresentCount)
				{
					for (int i = siblingsAlreadyPresentCount; i < siblingCount; i++)
					{
						AddEmptySiblingNodeForChange(amendmentXML, selectedNode.Name.LocalName);
					}
				}
			}
		}

		static void AddEmptySiblingNodesForAdd(XElement amendmentXML, XDocument oldXml, XElement element, XElement elementToAdd)
		{
			var previousNode = (XElement)element.PreviousNode;
			var xPath = GetXPath(GetNodeHierarchyList(previousNode));
			var selectedNode = oldXml.XPathSelectElement(xPath);

			if (selectedNode != null && selectedNode.Name.LocalName.Equals(elementToAdd.Name.LocalName, System.StringComparison.OrdinalIgnoreCase))
			{
				var siblingCount = GetSiblingCount(selectedNode, elementToAdd.Name.LocalName) + 1;
				var siblingsAlreadyPresentCount = amendmentXML.Elements(elementToAdd.Name.LocalName).Count();

				if (siblingCount > siblingsAlreadyPresentCount)
				{
					for (int i = siblingsAlreadyPresentCount; i < siblingCount; i++)
					{
						AddEmptySiblingNodeForAdd(amendmentXML, selectedNode);
					}
				}
			}
		}

		static void AddEmptySiblingNodeForMatch(XElement xml, XElement node)
		{
			var newElement = new XElement(XName.Get(node.Name.LocalName));

			foreach (var attribute in node.Attributes())
			{
				newElement.Add(new XAttribute(attribute.Name.LocalName, attribute.Value));
			}

			xml.Add(newElement);
		}

		static void AddEmptySiblingNodeForChange(XElement xml, string nodeName)
		{
			xml.Parent.AddBeforeSelf(new XElement(nodeName));
		}

		static void AddEmptySiblingNodeForAdd(XElement xml, XElement node)
		{
			var newElement = new XElement(XName.Get(node.Name.LocalName));

			foreach (var attribute in node.Attributes())
			{
				newElement.Add(new XAttribute(attribute.Name.LocalName, attribute.Value));
			}

			xml.Add(newElement);
		}

		public static IEnumerable<Node> GetNodeHierarchyList(XElement element)
		{
			return element?.AncestorsAndSelf().Where(e => e.Name.LocalName == Node).Where(x => x.Name.LocalName != MetaData && x.Name.LocalName != Declaration).Select(x => new Node { Index = x.Attribute(Match).Value, NodeName = x.Name.LocalName }).Reverse() ?? Enumerable.Empty<Node>();
		}

		static bool HaveEmptyNodesBeenAdded(XElement element, XElement nodeToMatch)
		{
			var parent = element.Parent?.Parent ?? element.Parent;
			return !parent?.Elements(nodeToMatch.Name.LocalName).Any(x => !x.Elements().Any()) ?? false;
		}

		static bool HasSequenceNumeric(XElement elementToAdd) => elementToAdd.Elements().FirstOrDefault(e => e.Name.LocalName == SequenceNumeric) != null;

		public static ZString OrderAdditionalInformations(ZString messageText)
		{
			var xmlDoc = XDocument.Parse(messageText);
			var additionalInformations = xmlDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == Declaration)?.Elements().Where(x => x.Name.LocalName == AdditionalInformation) ?? Array.Empty<XElement>().AsEnumerable();
			var orderedAdditionalInformations = additionalInformations.OrderBy(x => x, new AdditionalInformationComparer()).ToList();
			additionalInformations.ToList().ForEach(x => x.Remove());
			xmlDoc.Descendants().FirstOrDefault(x => x.Name.LocalName == Declaration)?.Add(orderedAdditionalInformations);
			return xmlDoc.ToString();
		}

		protected virtual ZString GetLatestMessageForComparison(WCOJobDeclarationMessageSendingObject objectToSend)
		{
			return objectToSend?.Header?.GetLatestMessageForComparison()?.EM_MessageText ?? string.Empty;
		}

		protected virtual List<AlwaysAdd> GetAlwaysAddElements(XDocument newXMLDoc)
		{
			return new List<AlwaysAdd>();
		}

		protected List<AlwaysAdd> GetAlwaysAddElementsFromDocument(List<string> alwaysAddElementNames, XDocument newXMLDoc, string rootName)
		{
			List<AlwaysAdd> alwaysAddElements = new List<AlwaysAdd>();
			var newXMLDocNoAttributes = newXMLDoc;

			foreach (var des in newXMLDocNoAttributes.Descendants())
			{
				des.RemoveAttributes();
				des.RemoveAllNamespaces();
			}

			var declarationNode = newXMLDocNoAttributes.Root.Element(rootName);
			var index = 0;
			foreach (var element in declarationNode.Elements())
			{
				index++;
				if (alwaysAddElementNames.Contains(element.Name.LocalName))
				{
					var alwaysAddNode = new AlwaysAdd(element.Name.LocalName, index, element, rootName);
					alwaysAddElements.Add(alwaysAddNode);
				}
			}

			return alwaysAddElements;
		}

		public const string MetaData = "MetaData";
		public const string Declaration = "Declaration";
		public const string AdditionalInformation = "AdditionalInformation";
		public const string Node = "node";
		public const string Match = "match";
		public const string SequenceNumeric = "SequenceNumeric";
		public const string Add = "add";
		public const string Change = "change";
		public const string Remove = "remove";
		public const string MetaDataDeclarationXPath = "//*[1]/*[1]";
		protected virtual string MetaDataDeclarationXPathValue => MetaDataDeclarationXPath;
	}
}
