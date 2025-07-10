using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public interface IXmlElement
	{
		string ToString();

		void WriteToXmlStream(Stream stream, XmlWriterSettings settings = null);

		IEnumerable<XStreamingElement> ToXElements();

		bool HasValue { get; }
	}

	public abstract class XmlElementBase : IXmlElement
	{
		protected XmlElementBase(XNamespace documentXmlns, string tagName)
			: this(documentXmlns, tagName, false)
		{
		}

		protected XmlElementBase(XNamespace documentXmlns, string tagName, bool allowNull)
		{
			Argument.NotNull(documentXmlns, nameof(documentXmlns));
			DocumentXmlns = documentXmlns;
			TagName = tagName;
			AllowNull = allowNull;
		}

		IEnumerable<XStreamingElement> IXmlElement.ToXElements() => ToXElements();

		void IXmlElement.WriteToXmlStream(Stream stream, XmlWriterSettings settings) => WriteToXmlStream(stream, settings);

		bool IXmlElement.HasValue => HasValue;

		public List<XmlElementAttribute> Attributes => attributes ?? (attributes = new List<XmlElementAttribute>());
		List<XmlElementAttribute> attributes;

		public bool AllowNull { get; }

		protected string TagName { get; }

		protected XName FullTagName => DocumentXmlns + TagName;

		protected XNamespace DocumentXmlns { get; }

		protected abstract IEnumerable<XStreamingElement> ToXElements();

		protected abstract bool HasValue { get; }

		protected abstract void WriteToXmlStream(Stream stream, XmlWriterSettings settings = null);

		protected void WriteXmlElementToStreamWithDocumentConformanceLevel(Stream stream, XStreamingElement xElement, XmlWriterSettings settings = null)
		{
			if (settings == null)
			{
				settings = new XmlWriterSettings
				{
					ConformanceLevel = ConformanceLevel.Document,
					OmitXmlDeclaration = false,
					NamespaceHandling = NamespaceHandling.OmitDuplicates,
					Encoding = MessageEncoding.UTF8WithoutBOM,
					NewLineHandling = NewLineHandling.None,
					Indent = true
				};
			}

			if (xElement != null)
			{
				using (var writer = XmlWriter.Create(stream, settings))
				{
					writer.WriteStartDocument();
					xElement.WriteTo(writer);
					writer.WriteEndDocument();
					writer.Flush();
				}
			}
		}

		protected IEnumerable<XAttribute> GetXAttribute() =>
			Attributes.Where(a => a.HasValue).Select(a => a.ToXAttribute());
	}

	public class XmlElementAttribute
	{
		public XmlElementAttribute(string attributeName, object content)
		{
			Name = Argument.NotNullOrEmpty(attributeName, nameof(attributeName));
			Value = content;
		}

		public XmlElementAttribute(XName attributeXName, object content)
		{
			XName = Argument.NotNull(attributeXName, nameof(attributeXName));
			Value = content;
		}

		public string Name { get; }

		public XName XName { get; }

		public object Value { get; }

		public bool HasValue => Value != null;

		public XAttribute ToXAttribute()
		{
			return HasValue
						? !string.IsNullOrEmpty(Name)
							? new XAttribute(Name, Value)
							: new XAttribute(XName, Value)
						: null;
		}
	}
}
