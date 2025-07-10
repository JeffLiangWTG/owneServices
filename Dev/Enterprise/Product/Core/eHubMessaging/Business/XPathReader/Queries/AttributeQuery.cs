using System;
using System.Xml;
using System.Xml.XPath;

namespace Enterprise.eHubMessaging.Business
{
	internal sealed class AttributeQuery : BaseAxisQuery
	{
		internal AttributeQuery(Query qyParent, String name, String prefix, XPathNodeType type)
			: base(qyParent, name, prefix, type)
		{
		}

		internal override bool MatchNode(XPathReader reader)
		{
			bool ret = true;
			if (base.NodeType != XPathNodeType.All)
			{
				if (!MatchType(base.NodeType, reader.NodeType))
				{
					ret = false;
				}
				else if (!string.IsNullOrEmpty(Name) && (Name != reader.Name || Prefix != reader.Prefix))
				{
					ret = false;
				}
			}

			return ret;
		}

		internal override object GetValue(XPathReader reader)
		{
			XmlReader baseReader = reader.BaseReader;

			object ret = null;

			if (baseReader.MoveToAttribute(base.Name))
			{
				ret = reader.Value;
			}

			baseReader.MoveToElement();
			return ret;
		}
	}
}
