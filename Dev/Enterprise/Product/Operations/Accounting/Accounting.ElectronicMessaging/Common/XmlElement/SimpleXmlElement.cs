using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class SimpleXmlElement : XmlElementBase
	{
		public SimpleXmlElement(XNamespace documentXmlns, string tagName, object content, bool allowNull = false)
			: base(documentXmlns, tagName, allowNull)
		{
			Content = content;
		}

		object Content { get; }

		protected override IEnumerable<XStreamingElement> ToXElements() => XElement != null ? new XStreamingElement[] { XElement } : null;

		protected override void WriteToXmlStream(Stream stream, XmlWriterSettings settings = null) => WriteXmlElementToStreamWithDocumentConformanceLevel(stream, XElement);

		protected override bool HasValue => !string.IsNullOrEmpty(TagName) && (Content != null || AllowNull);

		XStreamingElement XElement => HasValue ? new XStreamingElement(FullTagName, GetXAttribute(), Content) : null;

		public override string ToString() => XElement?.ToString() ?? string.Empty;
	}
}
