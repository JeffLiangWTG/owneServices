using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Enterprise.Customs.AU.Declaration.Business;

[Serializable]
public class NEXDOCAcknowledgeInterchangeBody : IXmlSerializable
{
	public string BodyXml { get; set; }

	XmlSchema IXmlSerializable.GetSchema() { return null; }

	void IXmlSerializable.ReadXml(XmlReader reader)
	{
		BodyXml = reader.ReadInnerXml();
	}

	void IXmlSerializable.WriteXml(XmlWriter writer)
	{
		writer.WriteRaw(BodyXml);
	}
}
