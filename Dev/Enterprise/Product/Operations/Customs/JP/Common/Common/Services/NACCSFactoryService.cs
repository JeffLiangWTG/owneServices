using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Common
{
	public static class NACCSFactoryService
	{
		public const string InboundMessageParserKey = "JPNACCSMessaging|InboundMessageParser";
		public const string MessageFlatParserKey = "JPNACCSMessaging|MessageFlatParser";
		public const string MessageVisualBuilderKey = "JPNACCSMessaging|JPMessageVisualBuilder";
		public const string OutboundMessageWriterKey = "JPNACCSMessaging|OutboundMessageWriter";

		public static IJPInboundMessageParser GetInboundMessageParser(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<IJPInboundMessageParser>(InboundMessageParserKey, () => new JPInboundMessageParser());
		}

		public static IJPMessageFlatParser GetMessageFlatParser(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<IJPMessageFlatParser>(MessageFlatParserKey, () => new JPInboundMessageParser());
		}

		public static IJPMessageVisualBuilder GetMessageVisualBuilder(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<IJPMessageVisualBuilder>(MessageVisualBuilderKey, () => new JPInboundMessageParser());
		}

		public static IJPOutboundMessageWriter GetOutboundMessageWriter(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<IJPOutboundMessageWriter>(OutboundMessageWriterKey, () => new JPOutboundMessageWriter());
		}
	}
}
