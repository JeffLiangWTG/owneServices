using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Business.Xml.Serializers;
using Enterprise.DataTransfer.Native.Common;

namespace Enterprise.DataTransfer.Native.Business.Xml
{
	public class ActionAttributeGenerator : IXmlGenerator<EntityAction, XAttribute>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public XAttribute Generate(EntityAction source)
		{
			if (source == EntityAction.EMPTY)
			{
				return null;
			}
			return new XAttribute("Action", source.ToString());
		}
	}
}
