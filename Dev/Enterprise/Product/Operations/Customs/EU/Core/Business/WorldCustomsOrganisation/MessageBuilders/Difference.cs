using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.Customs.Shared.WorldCustomsOrganisation;
using CargoWise.Customs.Shared.WorldCustomsOrganisation.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders
{
	public abstract class Difference
	{
		protected Difference(IPointerParser pointerParser, ZString comparisonXml, ZString newXml, IEnumerable<Node> nodeTree)
		{
			ComparisonXml = comparisonXml;
			NewXml = newXml;
			NodeTree = nodeTree;
			parser = pointerParser;
		}

		public abstract ZString Type { get; }
		protected ZString ComparisonXml { get; set; }
		protected ZString NewXml { get; set; }
		public IEnumerable<Node> NodeTree { get; set; }
		public virtual ZString XPath
		{
			get
			{
				return string.Format(CultureInfo.InvariantCulture, @"//*{0}", string.Join(@"/*", NodeTree.Select(x => "[" + x.Index + "]")));
			}
		}

		public ZString FullXPathWithNodeNames
		{
			get
			{
				var xmlDoc = XDocument.Parse(ComparisonXml);
				return AmendmentMessageHelper.GetFullXPathWithNodeNames(xmlDoc, XPath);
			}
		}

		protected virtual ZString[] GetHierarchyStrings()
		{
			return new ZString[] { GetHierarchyString() };
		}

		protected virtual ZString GetHierarchyString()
		{
			var xmlDoc = XDocument.Parse(ComparisonXml);
			var selectedNode = xmlDoc.XPathSelectElement(XPath);

			var hierarchy = new List<XElement>(new XElement[] { selectedNode });
			var e = selectedNode;
			while (e.Parent != null)
			{
				e = e.Parent;
				if (e.NodeType == XmlNodeType.Document || e.Name.LocalName == AmendmentMessageHelper.MetaData)
				{
					break;
				}
				hierarchy.Add(e);
			}

			var fullHierachyString = string.Join("//", hierarchy.Select(x => x.Name.LocalName + "[" + x.Sequence() + "]").Reverse());

			return fullHierachyString;
		}

		ZString RemoveLastPointer(ZString path) => removeLastPointer = path.SubstringSafe(0, path.LastIndexOf("/", System.StringComparison.OrdinalIgnoreCase));
		public ZString removeLastPointer;
		public virtual IEnumerable<ZString> WCOIDPointers => GetHierarchyStrings().Select(x => (ZString)parser.GetFullPointersFromNames(x));
		public IEnumerable<ZString> WCOIDPointersWithoutLastPointer => GetHierarchyStrings().Select(x => (ZString)parser.GetFullPointersFromNames(RemoveLastPointer(x)));

		readonly IPointerParser parser;
	}
}
