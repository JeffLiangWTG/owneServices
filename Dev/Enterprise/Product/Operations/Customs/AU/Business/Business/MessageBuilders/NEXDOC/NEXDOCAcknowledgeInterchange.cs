using System;
using System.Xml.Serialization;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[Serializable]
	[XmlRoot("UniversalInterchange", Namespace = "http://www.cargowise.com/Schemas/Universal/2011/11")]
	[XmlSerializerAssembly("Enterprise.Customs.AU.Declaration.Business.XmlSerializers")]
	public class NEXDOCAcknowledgeInterchange
	{
		[XmlElement(ElementName = "Header")]
		public NEXDOCAcknowledgeInterchangeHeader Header { get; set; }

		[XmlElement(ElementName = "Body")]
		public NEXDOCAcknowledgeInterchangeBody Body { get; set; }

		public NEXDOCAcknowledgeInterchange() { }

		public NEXDOCAcknowledgeInterchange(string sender, string recipient, string submitter, string messageType, string rexNumber, string jobNumber, string bodyText)
		{
			Header = new NEXDOCAcknowledgeInterchangeHeader();
			Header.SenderID = sender;
			Header.RecipientID = recipient;
			Header.DeliveryMetadata = new NEXDOCAcknowledgeInterchangeDeliveryMetadata();
			Header.DeliveryMetadata.ValueCollection = new[]
			{
				new NEXDOCInterchangeDeliveryMetadataValue { Name = "Submitter", Type = "String", Data = submitter },
				new NEXDOCInterchangeDeliveryMetadataValue { Name = "MessageType", Type = "String", Data = messageType },
				new NEXDOCInterchangeDeliveryMetadataValue { Name = "RexNumber", Type = "String", Data = rexNumber },
				new NEXDOCInterchangeDeliveryMetadataValue { Name = "JobNumber", Type = "String", Data = jobNumber }
			};

			Body = new NEXDOCAcknowledgeInterchangeBody() { BodyXml = bodyText };
		}
	}
}
