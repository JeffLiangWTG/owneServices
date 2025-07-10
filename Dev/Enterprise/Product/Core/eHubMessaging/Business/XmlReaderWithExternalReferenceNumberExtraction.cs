using System.Xml;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.eHubMessaging.Business
{
	public class XmlReaderWithExternalReferenceNumberExtraction : XmlReader
	{
		readonly XmlReader InnerReader;

		public string ExternalReferenceNumber {  get; private set; }

		public XmlReaderWithExternalReferenceNumberExtraction(XmlReader innerReader)
		{
			InnerReader = innerReader;
			ExternalReferenceNumber = string.Empty;
		}

		public override int AttributeCount => InnerReader.AttributeCount;
		public override string BaseURI => InnerReader.BaseURI;
		public override int Depth => InnerReader.Depth;
		public override bool EOF => InnerReader.EOF;
		public override bool IsEmptyElement => InnerReader.IsEmptyElement;
		public override string LocalName => InnerReader.LocalName;
		public override string NamespaceURI => InnerReader.NamespaceURI;
		public override XmlNameTable NameTable => InnerReader.NameTable;
		public override XmlNodeType NodeType => InnerReader.NodeType;
		public override string Prefix => InnerReader.Prefix;
		public override ReadState ReadState => InnerReader.ReadState;
		public override string Value => InnerReader.Value;

		public override string GetAttribute(int i) => InnerReader.GetAttribute(i);
		public override string GetAttribute(string name) => InnerReader.GetAttribute(name);
		public override string GetAttribute(string name, string namespaceURI) => InnerReader.GetAttribute(name, namespaceURI);
		public override string LookupNamespace(string prefix) => InnerReader.LookupNamespace(prefix);
		public override bool MoveToAttribute(string name) => InnerReader.MoveToAttribute(name);
		public override bool MoveToAttribute(string name, string ns) => InnerReader.MoveToAttribute(name, ns);
		public override bool MoveToElement() => InnerReader.MoveToElement();
		public override bool MoveToFirstAttribute() => InnerReader.MoveToFirstAttribute();
		public override bool MoveToNextAttribute() => InnerReader.MoveToNextAttribute();
		public override bool Read()
		{
			var read = InnerReader.Read();
			ExtractExternalReferenceNumber();
			return read;
		}
		public override bool ReadAttributeValue() => InnerReader.ReadAttributeValue();
		public override void ResolveEntity() => InnerReader.ResolveEntity();
		void ExtractExternalReferenceNumber()
		{
			if (CanSetExternalReferenceNumber && InnerReader.NodeType == XmlNodeType.Text)
			{
				ExternalReferenceNumber = InnerReader.Value;
				CanSetExternalReferenceNumber = false;
			}

			if (IsInMessageNumberCollectionElement && IsInMessageNumberElement() && IsTypeExternal())
			{
				CanSetExternalReferenceNumber = true;
			}

			SetIsInMessageNumberCollectionElement();
		}

		void SetIsInMessageNumberCollectionElement()
		{
			if (InnerReader.LocalName == nameof(ITopLevelDataObject.MessageNumberCollection))
			{
				if (InnerReader.NodeType == XmlNodeType.Element)
				{
					IsInMessageNumberCollectionElement = true;
				}
				else if (InnerReader.NodeType == XmlNodeType.EndElement)
				{
					IsInMessageNumberCollectionElement = false;
				}
			}
		}

		const string MessageNumberElementName = "MessageNumber";

		bool IsInMessageNumberCollectionElement { get; set; }
		bool CanSetExternalReferenceNumber { get; set; }
		bool IsInMessageNumberElement()
		{
			return InnerReader.NodeType == XmlNodeType.Element && InnerReader.LocalName == MessageNumberElementName;
		}
		bool IsTypeExternal()
		{
			return (InnerReader.GetAttribute(nameof(IMessageNumber.Type)) ?? ZString.Empty).Equals(nameof(MessageNumberType.External));
		}
	}
}
