using System;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.Customs.BE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.MessageBuilders;

public static class InboundProviderHelper
{
	public static TInboundProvider GetCachedInboundProvider<TXmlObject, TInboundProvider>(this BEMessage message)
		where TInboundProvider : class, CargoWise.Customs.BE.MessageContracts.Interfaces.IInboundProvider
	{
		return message != null && message.EM_ReceiveTransmit == EDIMessage.Direction.Receive
			? message.Factory.GetCachedValue($"{typeof(TInboundProvider)}_{message.PK}", () =>
			{
				var stringReader = new StringReader(message.EM_MessageText);
				var xmlTextReader = new NamespaceIgnorantXmlTextReader(stringReader);
				var rootElementName = typeof(TXmlObject).GetCustomAttribute<XmlRootAttribute>().ElementName;
				var xmlObject = (TXmlObject)new XmlSerializer(typeof(TXmlObject), new XmlRootAttribute(rootElementName)).Deserialize(xmlTextReader);

				return xmlObject != null
					? (TInboundProvider)Activator.CreateInstance(typeof(TInboundProvider), xmlObject)
					: default;
			})
			: null;
	}

	class NamespaceIgnorantXmlTextReader : XmlTextReader
	{
		public NamespaceIgnorantXmlTextReader(TextReader reader) : base(reader) { }

		public override string NamespaceURI
		{
			get { return ""; }
		}
	}
}
