using System.Xml;
using System.Xml.XPath;

namespace Enterprise.eHubMessaging.Business
{
	internal class BaseAxisQuery : Query
	{
		#region .ctor

		Query queryInput;
		readonly string name = string.Empty;
		readonly string prefix = string.Empty;
		readonly XPathNodeType nodeType;
		int expectedDepth = -1;

		internal BaseAxisQuery()
		{
		}

		internal BaseAxisQuery(Query queryInput)
		{
			this.queryInput = queryInput;
		}

		internal BaseAxisQuery(Query queryInput, string name, string prefix, XPathNodeType nodeType)
		{
			this.prefix = prefix;
			this.queryInput = queryInput;
			this.name = name;
			this.nodeType = nodeType;
		}

		#endregion

		internal override XPathResultType ReturnType()
		{
			return XPathResultType.NodeSet;
		}

		internal string Name
		{
			get { return this.name; }
		}

		internal string Prefix
		{
			get { return this.prefix; }
		}

		internal XPathNodeType NodeType
		{
			get { return this.nodeType; }
		}

		internal int Depth
		{
			get { return this.expectedDepth; }
			set { this.expectedDepth = value; }
		}

		internal Query QueryInput
		{
			get { return this.queryInput; }
			set { this.queryInput = value; }
		}

		internal bool MatchType(XPathNodeType xType, XmlNodeType type)
		{
			bool ret = false;

			switch (xType)
			{
				case XPathNodeType.Element:
					if (type == XmlNodeType.Element || type == XmlNodeType.EndElement)
					{
						ret = true;
					}
					break;

				case XPathNodeType.Attribute:
					if (type == XmlNodeType.Attribute)
					{
						ret = true;
					}
					break;

				case XPathNodeType.Text:
					if (type == XmlNodeType.Text)
					{
						ret = true;
					}
					break;

				case XPathNodeType.ProcessingInstruction:
					if (type == XmlNodeType.ProcessingInstruction)
					{
						ret = true;
					}
					break;

				case XPathNodeType.Comment:
					if (type == XmlNodeType.Comment)
					{
						ret = true;
					}
					break;

				default:
					throw new XPathReaderException("Unknown nodeType");
			}
			return ret;
		}
	}

	internal sealed class NullQuery : BaseAxisQuery
	{
		internal override bool MatchNode(XPathReader reader)
		{
			return false;
		}
	}
}
