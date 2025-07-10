using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class ComplexXmlElement : XmlElementBase
	{
		public ComplexXmlElement(XNamespace documentXmlns, string tagName, params IXmlElement[] childElements)
			: this(documentXmlns, tagName, false, childElements)
		{
		}

		public ComplexXmlElement(XNamespace documentXmlns, string tagName, bool allowNull, params IXmlElement[] childElements)
			: this(documentXmlns, tagName, allowNull, childElements?.Where(c => c != null)
																	.Select((c, i) => new IXmlElementWithSequence(c, i))
																	.ToArray())
		{
		}

		public ComplexXmlElement(XNamespace documentXmlns, string tagName, params IXmlElementWithSequence[] childElements)
			: this(documentXmlns, tagName, false, childElements)
		{
		}

		public ComplexXmlElement(XNamespace documentXmlns, string tagName, bool allowNull, params IXmlElementWithSequence[] childElements)
			: base(documentXmlns, tagName, allowNull)
		{
			Argument.NotNull(childElements, nameof(childElements));
			Children = childElements.ToList();
		}

		public List<IXmlElementWithSequence> Children { get; }

		protected override IEnumerable<XStreamingElement> ToXElements() => XElement != null ? new XStreamingElement[] { XElement } : null;

		protected override bool HasValue => AllowNull || Children.Any(c => c?.XmlElement.HasValue ?? false);

		protected override void WriteToXmlStream(Stream stream, XmlWriterSettings settings = null) => WriteXmlElementToStreamWithDocumentConformanceLevel(stream, XElement, settings);

		XStreamingElement XElement
		{
			get
			{
				XStreamingElement element = null;

				if (HasValue)
				{
					var populatedChildren = Children.Where(c => c?.XmlElement.HasValue ?? false).ToArray();
					if (populatedChildren.Any())
					{
						element = new XStreamingElement(FullTagName, GetXAttribute(), populatedChildren.OrderBy(pc => pc.Sequence)
																									.Select(pc => pc.XmlElement.ToXElements()));
					}
					else if (AllowNull)
					{
						element = new XStreamingElement(FullTagName, GetXAttribute(), null);
					}
				}

				return element;
			}
		}

		public override string ToString() => XElement?.ToString() ?? string.Empty;
	}

	public class IXmlElementWithSequence
	{
		public IXmlElementWithSequence(IXmlElement xmlElement, int sequence)
		{
			Argument.NotNull(xmlElement, nameof(xmlElement));
			XmlElement = xmlElement;
			Sequence = sequence;
		}

		public IXmlElement XmlElement { get; }
		public int Sequence { get; }
	}
}
