using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Enterprise.Customs.GB.ICS
{
	public static class ResponseMessageProcessorHelper
	{
		public static T GetMessageObject<T>(string messageXml)
		{
			var stringReader = new StringReader(messageXml);
			var xmlTextReader = new NamespaceIgnorantXmlTextReader(stringReader);
			var rootElementName = typeof(T).GetCustomAttribute<XmlRootAttribute>().ElementName;
			return (T)new XmlSerializer(typeof(T), new XmlRootAttribute(rootElementName)).Deserialize(xmlTextReader);
		}

		class NamespaceIgnorantXmlTextReader : XmlTextReader
		{
			public NamespaceIgnorantXmlTextReader(TextReader reader) : base(reader)
			{
			}

			public override string NamespaceURI => string.Empty;
		}
	}
}