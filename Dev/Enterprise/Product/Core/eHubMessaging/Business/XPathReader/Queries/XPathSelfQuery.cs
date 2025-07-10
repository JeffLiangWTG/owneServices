using System.Xml.XPath;

namespace Enterprise.eHubMessaging.Business
{
	internal sealed class AbsoluteQuery : XPathSelfQuery
	{
	}

	internal class XPathSelfQuery : BaseAxisQuery
	{
		internal XPathSelfQuery()
		{
		}

		internal XPathSelfQuery(Query queryInput, string name, string prefix, XPathNodeType type)
			: base(queryInput, name, prefix, type)
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
			if (reader.HasValue)
			{
				return reader.Value;
			}
			else
			{
				throw new XPathReaderException("Can't get the element value");
			}
		}
	}
}
