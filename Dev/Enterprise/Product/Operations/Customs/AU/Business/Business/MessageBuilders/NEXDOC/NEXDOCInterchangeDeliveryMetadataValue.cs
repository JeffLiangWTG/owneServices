using System;
using System.Xml.Serialization;

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class NEXDOCInterchangeDeliveryMetadataValue
{
	[XmlElement(ElementName = "Name")]
	public string Name { get; set; }

	[XmlElement(ElementName = "Type")]
	public string Type { get; set; }

	[XmlElement(ElementName = "Data")]
	public string Data { get; set; }
}
