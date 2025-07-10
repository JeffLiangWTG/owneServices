using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.IL.Business.MessageProcessors;

namespace Enterprise.Customs.IL.Business
{
	public static partial class ILMessageProcessorFactory
	{
		public static IMessageProcessor GetMessageProcessor(ZString messageType, ZString messageSubType, LoggingInformation logger) =>
			messageType.ToString().ToUpperInvariant() switch
			{
				ILMessageTypeList.Codes.DEC => new Message274Processor(logger),
				ILMessageTypeList.Codes.DLO => new Message1220Processor(logger),
				ILMessageTypeList.Codes.DOC => messageSubType.ToString().ToUpperInvariant() switch
					{
						ILEDIMessageSubTypeList.Codes.SupportingDocumentsResponse => ObjectFactory.Get<IMessageProcessor>("IL.Message2716Processor", new object[] { logger }),
						ILEDIMessageSubTypeList.Codes.SupportingDocumentsRqDecisionResponse => ObjectFactory.Get<IMessageProcessor>("IL.Message828Processor", new object[] { logger }),
						_ => new UnsupportedTypeMessageProcessor(logger)
					},
				ILMessageTypeList.Codes.GPM => new Message1035Processor(logger),
				ILMessageTypeList.Codes.MAN => messageSubType.ToString().ToUpperInvariant() switch
					{
						ILEDIMessageSubTypeList.Codes.ForwarderManifestResponse => ObjectFactory.Get<IMessageProcessor>("IL.Message1171Processor", new object[] { logger }),
						ILEDIMessageSubTypeList.Codes.ManifestQueryResponse => ObjectFactory.Get<IMessageProcessor>("IL.Message8241Processor", new object[] { logger }),
						_ => new UnsupportedTypeMessageProcessor(logger)
					},
				ILMessageTypeList.Codes.XER => new XTERRMessageProcessor(logger),
				_ => new UnsupportedTypeMessageProcessor(logger)
			};
	}
}
