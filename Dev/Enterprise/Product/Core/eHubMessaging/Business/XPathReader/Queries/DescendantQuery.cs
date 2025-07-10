using System;
using System.Xml.XPath;

namespace Enterprise.eHubMessaging.Business
{
	internal class DescendantQuery : BaseAxisQuery
	{
		public DescendantQuery(Query qyInput, String name, String prefix, XPathNodeType type)
			: base(qyInput, name, prefix, type)
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
				else if (base.Name != null && (base.Name != reader.Name || base.Prefix != reader.Prefix))
				{
					ret = false;
				}
			}

			return ret;
		}

		internal override object GetValue(XPathReader reader)
		{
			throw new XPathReaderException("Can't get the decendent nodes value");
		}
	}
}
