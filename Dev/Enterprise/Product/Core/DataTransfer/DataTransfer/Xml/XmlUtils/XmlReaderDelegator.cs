using System;
using System.Xml;

namespace Enterprise.DataTransfer.Xml
{
	public class XmlReaderDelegator : XmlReader
	{
		public XmlReaderDelegator(XmlReader inner)
		{
			this.Inner = inner;
		}

		public readonly XmlReader Inner;

		public override int AttributeCount
		{
			get { return Inner.AttributeCount; }
		}

		public override int ReadContentAsBase64(byte[] buffer, int index, int count)
		{
			return Inner.ReadContentAsBase64(buffer, index, count);
		}

		public override bool CanReadBinaryContent
		{
			get { return Inner.CanReadBinaryContent; }
		}

		public override bool CanReadValueChunk
		{
			get { return Inner.CanReadValueChunk; }
		}

		public override int ReadElementContentAsBase64(byte[] buffer, int index, int count)
		{
			return Inner.ReadElementContentAsBase64(buffer, index, count);
		}

		public override int ReadValueChunk(char[] buffer, int index, int count)
		{
			return Inner.ReadValueChunk(buffer, index, count);
		}

		public override object ReadContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
		{
			return Inner.ReadContentAs(returnType, namespaceResolver);
		}

		public override int ReadContentAsBinHex(byte[] buffer, int index, int count)
		{
			return Inner.ReadContentAsBinHex(buffer, index, count);
		}

		public override bool ReadContentAsBoolean()
		{
			return Inner.ReadContentAsBoolean();
		}

		public override DateTime ReadContentAsDateTime()
		{
			return Inner.ReadContentAsDateTime();
		}

		public override DateTimeOffset ReadContentAsDateTimeOffset()
		{
			return Inner.ReadContentAsDateTimeOffset();
		}

		public override decimal ReadContentAsDecimal()
		{
			return Inner.ReadContentAsDecimal();
		}

		public override double ReadContentAsDouble()
		{
			return Inner.ReadContentAsDouble();
		}

		public override float ReadContentAsFloat()
		{
			return Inner.ReadContentAsFloat();
		}

		public override int ReadContentAsInt()
		{
			return Inner.ReadContentAsInt();
		}

		public override long ReadContentAsLong()
		{
			return Inner.ReadContentAsLong();
		}

		public override object ReadContentAsObject()
		{
			return Inner.ReadContentAsObject();
		}

		public override string ReadContentAsString()
		{
			return Inner.ReadContentAsString();
		}

		public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver)
		{
			return Inner.ReadElementContentAs(returnType, namespaceResolver);
		}

		public override object ReadElementContentAs(Type returnType, IXmlNamespaceResolver namespaceResolver, string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAs(returnType, namespaceResolver, localName, namespaceURI);
		}

		public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count)
		{
			return Inner.ReadElementContentAsBinHex(buffer, index, count);
		}

		public override bool ReadElementContentAsBoolean()
		{
			return Inner.ReadElementContentAsBoolean();
		}

		public override bool ReadElementContentAsBoolean(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsBoolean(localName, namespaceURI);
		}

		public override DateTime ReadElementContentAsDateTime()
		{
			return Inner.ReadElementContentAsDateTime();
		}

		public override DateTime ReadElementContentAsDateTime(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsDateTime(localName, namespaceURI);
		}

		public override decimal ReadElementContentAsDecimal()
		{
			return Inner.ReadElementContentAsDecimal();
		}

		public override decimal ReadElementContentAsDecimal(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsDecimal(localName, namespaceURI);
		}

		public override double ReadElementContentAsDouble()
		{
			return Inner.ReadElementContentAsDouble();
		}

		public override double ReadElementContentAsDouble(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsDouble(localName, namespaceURI);
		}

		public override float ReadElementContentAsFloat()
		{
			return Inner.ReadElementContentAsFloat();
		}

		public override float ReadElementContentAsFloat(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsFloat(localName, namespaceURI);
		}

		public override int ReadElementContentAsInt()
		{
			return Inner.ReadElementContentAsInt();
		}

		public override int ReadElementContentAsInt(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsInt(localName, namespaceURI);
		}

		public override long ReadElementContentAsLong()
		{
			return Inner.ReadElementContentAsLong();
		}

		public override long ReadElementContentAsLong(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsLong(localName, namespaceURI);
		}

		public override object ReadElementContentAsObject()
		{
			return Inner.ReadElementContentAsObject();
		}

		public override object ReadElementContentAsObject(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsObject(localName, namespaceURI);
		}

		public override string ReadElementContentAsString()
		{
			return Inner.ReadElementContentAsString();
		}

		public override string ReadElementContentAsString(string localName, string namespaceURI)
		{
			return Inner.ReadElementContentAsString(localName, namespaceURI);
		}

		public override XmlReader ReadSubtree()
		{
			return Inner.ReadSubtree();
		}

		public override bool ReadToDescendant(string localName, string namespaceURI)
		{
			return Inner.ReadToDescendant(localName, namespaceURI);
		}

		public override bool ReadToDescendant(string name)
		{
			return Inner.ReadToDescendant(name);
		}

		public override bool ReadToFollowing(string localName, string namespaceURI)
		{
			return Inner.ReadToFollowing(localName, namespaceURI);
		}

		public override bool ReadToFollowing(string name)
		{
			return Inner.ReadToFollowing(name);
		}

		public override bool ReadToNextSibling(string localName, string namespaceURI)
		{
			return Inner.ReadToNextSibling(localName, namespaceURI);
		}

		public override bool ReadToNextSibling(string name)
		{
			return Inner.ReadToNextSibling(name);
		}

		public override System.Xml.Schema.IXmlSchemaInfo SchemaInfo
		{
			get { return Inner.SchemaInfo; }
		}

		public override XmlReaderSettings Settings
		{
			get { return Inner.Settings; }
		}

		public override Type ValueType
		{
			get { return Inner.ValueType; }
		}

		public override string BaseURI
		{
			get { return Inner.BaseURI; }
		}

		public override bool CanResolveEntity
		{
			get { return Inner.CanResolveEntity; }
		}

		public override void Close()
		{
			Inner.Close();
		}

		public override int Depth
		{
			get { return Inner.Depth; }
		}

		public override bool EOF
		{
			get { return Inner.EOF; }
		}

		public override string GetAttribute(int i)
		{
			return Inner.GetAttribute(i);
		}

		public override string GetAttribute(string name)
		{
			return Inner.GetAttribute(name);
		}

		public override string GetAttribute(string name, string namespaceURI)
		{
			return Inner.GetAttribute(name, namespaceURI);
		}

		public override bool HasAttributes
		{
			get { return Inner.HasAttributes; }
		}

		public override bool HasValue
		{
			get { return Inner.HasValue; }
		}

		public override bool IsDefault
		{
			get { return Inner.IsDefault; }
		}

		public override bool IsEmptyElement
		{
			get { return Inner.IsEmptyElement; }
		}

		public override bool IsStartElement()
		{
			return Inner.IsStartElement();
		}

		public override bool IsStartElement(string localname, string ns)
		{
			return Inner.IsStartElement(localname, ns);
		}

		public override bool IsStartElement(string name)
		{
			return Inner.IsStartElement(name);
		}

		public override string LocalName
		{
			get { return Inner.LocalName; }
		}

		public override string LookupNamespace(string prefix)
		{
			return Inner.LookupNamespace(prefix);
		}

		public override void MoveToAttribute(int i)
		{
			Inner.MoveToAttribute(i);
		}

		public override bool MoveToAttribute(string name)
		{
			return Inner.MoveToAttribute(name);
		}

		public override bool MoveToAttribute(string name, string ns)
		{
			return Inner.MoveToAttribute(name, ns);
		}

		public override XmlNodeType MoveToContent()
		{
			return Inner.MoveToContent();
		}

		public override bool MoveToElement()
		{
			return Inner.MoveToElement();
		}

		public override bool MoveToFirstAttribute()
		{
			return Inner.MoveToFirstAttribute();
		}

		public override bool MoveToNextAttribute()
		{
			return Inner.MoveToNextAttribute();
		}

		public override string Name
		{
			get { return Inner.Name; }
		}

		public override string NamespaceURI
		{
			get { return Inner.NamespaceURI; }
		}

		public override XmlNameTable NameTable
		{
			get { return Inner.NameTable; }
		}

		public override XmlNodeType NodeType
		{
			get { return Inner.NodeType; }
		}

		public override string Prefix
		{
			get { return Inner.Prefix; }
		}

		public override char QuoteChar
		{
			get { return Inner.QuoteChar; }
		}

		public override bool Read()
		{
			return Inner.Read();
		}

		public override bool ReadAttributeValue()
		{
			return Inner.ReadAttributeValue();
		}

		public override string ReadElementString()
		{
			return Inner.ReadElementString();
		}

		public override string ReadElementString(string localname, string ns)
		{
			return Inner.ReadElementString(localname, ns);
		}

		public override string ReadElementString(string name)
		{
			return Inner.ReadElementString(name);
		}

		public override void ReadEndElement()
		{
			Inner.ReadEndElement();
		}

		public override string ReadInnerXml()
		{
			return Inner.ReadInnerXml();
		}

		public override string ReadOuterXml()
		{
			return Inner.ReadOuterXml();
		}

		public override void ReadStartElement()
		{
			Inner.ReadStartElement();
		}

		public override void ReadStartElement(string localname, string ns)
		{
			Inner.ReadStartElement(localname, ns);
		}

		public override void ReadStartElement(string name)
		{
			Inner.ReadStartElement(name);
		}

		public override ReadState ReadState
		{
			get { return Inner.ReadState; }
		}

		public override string ReadString()
		{
			return Inner.ReadString();
		}

		public override void ResolveEntity()
		{
			Inner.ResolveEntity();
		}

		public override void Skip()
		{
			Inner.Skip();
		}

		public override string this[int i]
		{
			get { return Inner[i]; }
		}

		public override string this[string name, string namespaceURI]
		{
			get { return Inner[name, namespaceURI]; }
		}

		public override string this[string name]
		{
			get { return Inner[name]; }
		}

		public override string Value
		{
			get { return Inner.Value; }
		}

		public override string XmlLang
		{
			get { return Inner.XmlLang; }
		}

		public override XmlSpace XmlSpace
		{
			get { return Inner.XmlSpace; }
		}
	}
}
