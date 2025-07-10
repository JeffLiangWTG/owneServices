using System;
using System.Xml;

namespace Enterprise.eHubMessaging.Business
{
	class FilterQuery : BaseAxisQuery
	{
		readonly Query predicate;
		int matchCount;
		readonly Query axis;

		internal BaseAxisQuery Axis
		{
			get { return (BaseAxisQuery)axis; }
		}

		internal FilterQuery(Query axisQuery, Query predicate)
		{
			base.QueryInput = ((BaseAxisQuery)axisQuery).QueryInput;
			this.predicate = predicate;
			this.axis = axisQuery;
		}

		internal override bool MatchNode(XPathReader reader)
		{
			bool ret = false;

			if (this.axis.MatchNode(reader))
			{
				if (reader.NodeType != XmlNodeType.EndElement && reader.ReadMethod != ReadMethods.MoveToElement)
				{
					++matchCount;
					this.predicate.PositionCount = matchCount;
				}

				object obj = this.predicate.GetValue(reader);
				if (obj is System.Boolean && Convert.ToBoolean(obj))
				{
					ret = true;
				}
				else if (obj is System.Double && Convert.ToDouble(obj) == matchCount)
				{
					ret = true;
				}
				else if (obj != null && !(obj is System.Double || obj is System.Boolean))
				{
					ret = true;
				}
			}
			return ret;
		}

		internal override object GetValue(XPathReader reader)
		{
			throw new XPathReaderException("Can't get value");
		}
	}
}
