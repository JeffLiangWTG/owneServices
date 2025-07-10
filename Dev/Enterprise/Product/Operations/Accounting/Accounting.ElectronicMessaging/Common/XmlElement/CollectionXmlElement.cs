using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.Messaging.Integration;

namespace Enterprise.Accounting.ElectronicMessaging.Common
{
	public class CollectionXmlElement<T> : IXmlElement
	{
		public CollectionXmlElement(IEnumerable<T> objectCollection, Func<T, IXmlElement> objectToIXElementConverter)
		{
			ObjectCollection = objectCollection;
			ObjectToIXElementConverter = objectToIXElementConverter;
		}

		IEnumerable<T> ObjectCollection { get; }

		Func<T, IXmlElement> ObjectToIXElementConverter { get; }

		bool IXmlElement.HasValue => Children.Any(c => c.HasValue);

		IEnumerable<XStreamingElement> IXmlElement.ToXElements() => XElements;

		void IXmlElement.WriteToXmlStream(Stream stream, XmlWriterSettings settings)
		{
			settings = new XmlWriterSettings
			{
				ConformanceLevel = ConformanceLevel.Auto,
				OmitXmlDeclaration = false,
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
				Encoding = MessageEncoding.UTF8WithoutBOM,
				NewLineHandling = NewLineHandling.None,
				Indent = true
			};

			var xmlElements = XElements?.ToArray();

			if (xmlElements != null)
			{
				using (var writer = XmlWriter.Create(stream, settings))
				{
					xmlElements.ForEach(e => e.WriteTo(writer));
					writer.Flush();
				}
			}
		}

		IEnumerable<XStreamingElement> XElements => xElements ?? (xElements = GetXElements());
		IEnumerable<XStreamingElement> xElements;

		IEnumerable<XStreamingElement> GetXElements()
		{
			var populatedChildren = Children.Where(c => c.HasValue);
			if (populatedChildren.Any())
			{
				return populatedChildren.SelectMany(pc => pc.ToXElements());
			}
			return null;
		}

		IEnumerable<IXmlElement> Children => ObjectCollection?.Select(o => ObjectToIXElementConverter?.Invoke(o)).ToArray();

		public override string ToString() => string.Join(System.Environment.NewLine, (XElements ?? Enumerable.Empty<XStreamingElement>()).Select(e => e.ToString()).ToArray());
	}
}
