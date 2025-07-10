using System;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public struct ResponseMessageDetails
	{
		public ResponseMessageDetails(Type xmlObjectType, Type providerType, Type ediMessageType)
		{
			XmlObjectType = xmlObjectType;
			ProviderType = providerType;
			EDIMessageType = ediMessageType;
		}

		public string XsdSchemaEmbeddedResourceName => XmlObjectType.GetEmbeddedResourcePath();

		public Type XmlObjectType { get; }

		public Type ProviderType { get; }

		public Type EDIMessageType { get; }

		public override bool Equals(object obj) => obj is ResponseMessageDetails details && details == this;

		public override int GetHashCode() => XsdSchemaEmbeddedResourceName.GetHashCode() ^ XmlObjectType.GetHashCode() ^ ProviderType.GetHashCode() ^ EDIMessageType.GetHashCode();

		public static bool operator ==(ResponseMessageDetails x, ResponseMessageDetails y) => x.XsdSchemaEmbeddedResourceName == y.XsdSchemaEmbeddedResourceName && x.XmlObjectType == y.XmlObjectType && x.ProviderType == y.ProviderType && x.EDIMessageType == y.EDIMessageType;

		public static bool operator !=(ResponseMessageDetails x, ResponseMessageDetails y) => !(x == y);
	}
}
