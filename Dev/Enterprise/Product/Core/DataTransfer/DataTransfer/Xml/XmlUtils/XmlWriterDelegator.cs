using System;
using System.Xml;

namespace Enterprise.DataTransfer.Xml
{
	public abstract class XmlWriterDelegator : XmlWriter
	{
		protected XmlWriterDelegator(XmlWriter inner)
		{
			this.Inner = inner;
		}

		protected readonly XmlWriter Inner;

		public override void Close()
		{
			Inner.Close();
		}

		public override void Flush()
		{
			Inner.Flush();
		}

		public override void WriteNode(System.Xml.XPath.XPathNavigator navigator, bool defattr)
		{
			Inner.WriteNode(navigator, defattr);
		}

		public override void WriteValue(bool value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(decimal value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(double value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(float value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(int value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(long value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(object value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(string value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(DateTime value)
		{
			Inner.WriteValue(value);
		}

		public override void WriteValue(DateTimeOffset value)
		{
			Inner.WriteValue(value);
		}

		public override string LookupPrefix(string ns)
		{
			return Inner.LookupPrefix(ns);
		}

		public override void WriteAttributes(XmlReader reader, bool defattr)
		{
			Inner.WriteAttributes(reader, defattr);
		}

		public override void WriteBase64(byte[] buffer, int index, int count)
		{
			Inner.WriteBase64(buffer, index, count);
		}

		public override void WriteBinHex(byte[] buffer, int index, int count)
		{
			Inner.WriteBinHex(buffer, index, count);
		}

		public override void WriteCData(string text)
		{
			Inner.WriteCData(text);
		}

		public override void WriteCharEntity(char ch)
		{
			Inner.WriteCharEntity(ch);
		}

		public override void WriteChars(char[] buffer, int index, int count)
		{
			Inner.WriteChars(buffer, index, count);
		}

		public override void WriteComment(string text)
		{
			Inner.WriteComment(text);
		}

		public override void WriteDocType(string name, string pubid, string sysid, string subset)
		{
			Inner.WriteDocType(name, pubid, sysid, subset);
		}

		public override void WriteEndAttribute()
		{
			Inner.WriteEndAttribute();
		}

		public override void WriteEndDocument()
		{
			Inner.WriteEndDocument();
		}

		public override void WriteEndElement()
		{
			Inner.WriteEndElement();
		}

		public override void WriteEntityRef(string name)
		{
			Inner.WriteEntityRef(name);
		}

		public override void WriteFullEndElement()
		{
			Inner.WriteFullEndElement();
		}

		public override void WriteName(string name)
		{
			Inner.WriteName(name);
		}

		public override void WriteNmToken(string name)
		{
			Inner.WriteNmToken(name);
		}

		public override void WriteNode(XmlReader reader, bool defattr)
		{
			Inner.WriteNode(reader, defattr);
		}

		public override void WriteProcessingInstruction(string name, string text)
		{
			Inner.WriteProcessingInstruction(name, text);
		}

		public override void WriteQualifiedName(string localName, string ns)
		{
			Inner.WriteQualifiedName(localName, ns);
		}

		public override void WriteRaw(char[] buffer, int index, int count)
		{
			Inner.WriteRaw(buffer, index, count);
		}

		public override void WriteRaw(string data)
		{
			Inner.WriteRaw(data);
		}

		public override void WriteStartAttribute(string prefix, string localName, string ns)
		{
			Inner.WriteStartAttribute(prefix, localName, ns);
		}

		public override void WriteStartDocument()
		{
			Inner.WriteStartDocument();
		}

		public override void WriteStartDocument(bool standalone)
		{
			Inner.WriteStartDocument(standalone);
		}

		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			Inner.WriteStartElement(prefix, localName, ns);
		}

		public override WriteState WriteState
		{
			get { return Inner.WriteState; }
		}

		public override void WriteString(string text)
		{
			Inner.WriteString(text);
		}

		public override void WriteSurrogateCharEntity(char lowChar, char highChar)
		{
			Inner.WriteSurrogateCharEntity(lowChar, highChar);
		}

		public override void WriteWhitespace(string ws)
		{
			Inner.WriteWhitespace(ws);
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
