using System.IO;
using System.Xml;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.Business.Requests
{
	public static class RequestDeserializerBuilder
	{
		public static BaseRequestDeserializer GetDeserializer(XElement element)
		{
			if (element.Name.LocalName == ReferenceDataXMLForDeSerialize.RootElementName && element.Name.Namespace == NativeXmlInfo.Namespace_2011_11)
			{
				return new RequestDeserializer_Versioned_Native();
			}
			if (element.Name.LocalName == ReferenceDataXMLForDeSerialize.RootElementName && element.Name.Namespace == ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native)
			{
				return new RequestDeserializer_Unversioned_Native();
			}
			if (element.Name.LocalName == ReferenceDataXMLForDeSerialize.RootElementName_Universal && element.Name.Namespace == ReferenceDataXMLForDeSerialize.NameSpace_Universal)
			{
				return new RequestDeserializer_Universal();
			}
			return new OldRequestDeserializer();
		}

		public static BaseRequestDeserializer GetDeserializer(Stream stream)
		{
			using (var reader = XmlReader.Create(stream))
			{
				reader.MoveToContent();
				if (reader.NodeType == XmlNodeType.Element)
				{
					if (reader.LocalName == ReferenceDataXMLForDeSerialize.RootElementName && reader.NamespaceURI == NativeXmlInfo.Namespace_2011_11)
					{
						return new RequestDeserializer_Versioned_Native();
					}
					if (reader.LocalName == ReferenceDataXMLForDeSerialize.RootElementName && reader.NamespaceURI == ReferenceDataXMLForDeSerialize.NameSpace_Unversioned_Native)
					{
						return new RequestDeserializer_Unversioned_Native();
					}
					if (reader.LocalName == ReferenceDataXMLForDeSerialize.RootElementName_Universal && reader.NamespaceURI == ReferenceDataXMLForDeSerialize.NameSpace_Universal)
					{
						return new RequestDeserializer_Universal();
					}
				}
				return new OldRequestDeserializer();
			}
		}
	}
}