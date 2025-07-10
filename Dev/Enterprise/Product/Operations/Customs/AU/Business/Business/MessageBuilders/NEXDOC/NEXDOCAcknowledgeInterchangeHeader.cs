using System;
using System.Xml.Serialization;

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class NEXDOCAcknowledgeInterchangeHeader
{
	[XmlElement(ElementName = "SenderID")]
	public string SenderID { get; set; }

	[XmlElement(ElementName = "RecipientID")]
	public string RecipientID { get; set; }

	[XmlElement(ElementName = "DeliveryMetadata")]
	public NEXDOCAcknowledgeInterchangeDeliveryMetadata DeliveryMetadata { get; set; }
}
