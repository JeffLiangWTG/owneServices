using System.IO;
using System.Xml;

namespace CargoWise.eHub.Gateway
{
	public class NamespaceIgnorantXmlTextReader : XmlTextReader
	{
		public NamespaceIgnorantXmlTextReader(Stream input)
			: base(input)
		{
		}

		public override string NamespaceURI
		{
			get { return ""; }
		}
	}
}