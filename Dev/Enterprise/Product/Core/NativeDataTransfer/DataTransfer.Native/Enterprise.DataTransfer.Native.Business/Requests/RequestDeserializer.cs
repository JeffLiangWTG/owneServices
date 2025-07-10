using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public abstract class RequestDeserializer : BaseRequestDeserializer
	{
		protected abstract string RootElementName { get; }
		protected abstract ZXmlSerializer GetNewHeaderDeSerializer();

		const string HeaderElementName = ReferenceDataXMLForDeSerialize.HeaderElementName;
		const string BodyElementName = ReferenceDataXMLForDeSerialize.BodyElementName;

		public override Request Deserialize(XElement element)
		{
			var stream = new MemoryStream();

			using (var xmlWriter = NativeXmlWriter.Create(stream))
			using (var cleanWriter = new CleanXmlWriter(xmlWriter))
			{
				element.WriteTo(cleanWriter);
			}
			return Deserialize(stream);
		}

		public override Request Deserialize(Stream stream)
		{
			var request = new Request();
			request.Settings = DeserializeHeaderElement(stream);
			request.EntitySets = DeserializeEntitySetElements(stream);

			return request;
		}

		HeaderData DeserializeHeaderElement(Stream stream)
		{
			stream.Position = 0;

			var headerSerializer = GetNewHeaderDeSerializer();
			var readerSettings = new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Document, ValidationType = ValidationType.Schema };

			using (var reader = XmlReader.Create(stream, readerSettings))
			{
				reader.Read();
				reader.ReadStartElement(RootElementName, NameSpace);

				HeaderData result = null;

				if (reader.IsStartElement(HeaderElementName, NameSpace))
				{
					var headerWithoutUniversalNamespace =
						reader.ReadOuterXml().Replace(string.Format("xmlns=\"{0}\"", UniversalXmlInfo.Namespace_2011_11), string.Empty);
					var headerXmlReader = new XmlTextReader(headerWithoutUniversalNamespace, XmlNodeType.Element, null);
					result = (HeaderData)headerSerializer.Deserialize(headerXmlReader);
				}

				return result;
			}
		}

		IEnumerable<XElement> DeserializeEntitySetElements(Stream stream)
		{
			stream.Position = 0;
			var reader = new XmlTextReader(stream);
			reader.ReadToDescendant(BodyElementName, NameSpace);
			if (!reader.IsStartElement(BodyElementName, NameSpace))
			{
				throw new NativeXMLUserVisibleException("Could not find Body Element in XML");
			}

			if (reader.IsEmptyElement)
			{
				yield break;
			}

			//GOTO next element
			var innerReader = reader.ReadSubtree();
			innerReader.MoveToContent();
			innerReader.Read();

			while (innerReader.IsStartElement())
			{
				if (XNode.ReadFrom(innerReader) is XElement element)
				{
					yield return element; // Might cause effiency problem if it is not yield return
				}
			}
		}
	}
}
