using System.Linq;
using System.Xml;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DataTransfer.Native.Utils
{
	/// <summary>
	/// A Wrapper class to remove the XML Illegal charactors in XML
	/// </summary>
	public class CleanXmlWriter : XmlWriter
	{
		readonly XmlWriter wrapped;

		public CleanXmlWriter(XmlWriter wrapped)
		{
			this.wrapped = wrapped;
		}

		#region Wrap Writer Method

		public override void WriteStartDocument()
		{
			wrapped.WriteStartDocument();
		}

		public override void WriteStartDocument(bool standalone)
		{
			wrapped.WriteStartDocument(standalone);
		}

		public override void WriteEndDocument()
		{
			wrapped.WriteEndDocument();
		}

		public override void WriteDocType(string name, string pubid, string sysid, string subset)
		{
			wrapped.WriteDocType(name, pubid, sysid, subset);
		}

		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			wrapped.WriteStartElement(prefix, localName, ns);
		}

		public override void WriteEndElement()
		{
			wrapped.WriteEndElement();
		}

		public override void WriteFullEndElement()
		{
			wrapped.WriteFullEndElement();
		}

		public override void WriteStartAttribute(string prefix, string localName, string ns)
		{
			wrapped.WriteStartAttribute(prefix, localName, ns);
		}

		public override void WriteEndAttribute()
		{
			wrapped.WriteEndAttribute();
		}

		public override void WriteCData(string text)
		{
			wrapped.WriteCData(text);
		}

		public override void WriteComment(string text)
		{
			wrapped.WriteComment(text);
		}

		public override void WriteProcessingInstruction(string name, string text)
		{
			wrapped.WriteProcessingInstruction(name, text);
		}

		public override void WriteEntityRef(string name)
		{
			wrapped.WriteEntityRef(name);
		}

		public override void WriteCharEntity(char ch)
		{
			wrapped.WriteCharEntity(ch);
		}

		public override void WriteWhitespace(string ws)
		{
			wrapped.WriteWhitespace(ws);
		}

		public override void WriteSurrogateCharEntity(char lowChar, char highChar)
		{
			wrapped.WriteSurrogateCharEntity(lowChar, highChar);
		}

		public override void WriteChars(char[] buffer, int index, int count)
		{
			wrapped.WriteChars(buffer, index, count);
		}

		public override void WriteRaw(char[] buffer, int index, int count)
		{
			wrapped.WriteRaw(buffer, index, count);
		}

		public override void WriteRaw(string data)
		{
			wrapped.WriteRaw(data);
		}

		public override void WriteBase64(byte[] buffer, int index, int count)
		{
			wrapped.WriteBase64(buffer, index, count);
		}

		public override void Close()
		{
			wrapped.Close();
		}

		public override void Flush()
		{
			wrapped.Flush();
		}

		public override string LookupPrefix(string ns)
		{
			return wrapped.LookupPrefix(ns);
		}

		public override WriteState WriteState
		{
			get { return wrapped.WriteState; }
		}

		#endregion

		public override void WriteString(string text)
		{
			if (text.All(c => ZXmlValidation.IsLegalXmlCharacter(c)))
			{
				wrapped.WriteString(text);
			}
			else
			{
				wrapped.WriteString(ZXmlValidation.EscapeInvalidXmlCharacters(text));
			}
		}
	}
}
