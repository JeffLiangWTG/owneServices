using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Integration.Customs.BE;

namespace Enterprise.Customs.BE.Business;

public class BECIncomingMessageProcessor : BranchCustomsMessageProcessor
{
	public BECIncomingMessageProcessor() : base(new ZString[] { EDIMessage.ApplicationCodes.BECustoms }, null)
	{ }

	public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
	{
		cachedProcessors = cachedProcessors ?? new Dictionary<string, ApplicationTypeMessageProcessor>();

		var messageType = message.EM_MessageType;
		var messageSubType = message.EM_MessageSubType;
		var key = string.Concat(messageType, messageSubType);

		if (!cachedProcessors.TryGetValue(key, out var processor))
		{
			Type processorType = null;
			switch (messageType)
			{
				case SendMessageTypes.Codes.NCT:
					nctsMessageProcessors.Value.TryGetValue(messageSubType, out processorType);
					break;
				case SendMessageTypes.Codes.AES:
					aesMessageProcessors.Value.TryGetValue(messageSubType, out processorType);
					break;
				case SendMessageTypes.Codes.IMP:
					impMessageProcessors.Value.TryGetValue(messageSubType, out processorType);
					break;
				default:
					break;
			}

			if (processorType != null)
			{
				processor = (BranchCustomsApplicationTypeMessageProcessor)Activator.CreateInstance(processorType, Logger);
				cachedProcessors.Add(key, processor);
			}
		}

		return processor;
	}

	protected override bool MessageShouldBeProcessedInASeparateFactory => true;

	Dictionary<string, ApplicationTypeMessageProcessor> cachedProcessors;

	readonly Lazy<Dictionary<string, Type>> impMessageProcessors = new Lazy<Dictionary<string, Type>>(() => new Dictionary<string, Type>
	{
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.IDMS.NonIDMSType.IE928).Substring(2, 3), typeof(IE928MessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.IDMS.NonIDMSType.IE906).Substring(2, 3), typeof(IE906MessageProcessor) },
	});

	readonly Lazy<Dictionary<string, Type>> aesMessageProcessors = new Lazy<Dictionary<string, Type>>(() => new Dictionary<string, Type>
	{
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC504C.Cc504CType).Substring(2, 3), typeof(CC504CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC509C.Cc509CType).Substring(2, 3), typeof(CC509CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC528C.Cc528CType).Substring(2, 3), typeof(CC528CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC529C.Cc529CType).Substring(2, 3), typeof(CC529CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC551C.Cc551CType).Substring(2, 3), typeof(CC551CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC556C.Cc556CType).Substring(2, 3), typeof(CC556CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC560C.Cc560CType).Substring(2, 3), typeof(CC560CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC599C.Cc599CType).Substring(2, 3), typeof(CC599CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC917C.Cc917CType).Substring(2, 3), typeof(CC917CMessageProcessor) },
		{ nameof(CargoWise.Customs.BE.MessageDefinitions.AESVersion51_8_2.CC928C.Cc928CType).Substring(2, 3), typeof(CC928CMessageProcessor) },
		{ Constants.BECMessageTypes.CustomsServiceErrorUniversalEvent, typeof(CustomsServiceErrorUniversalEventResponseMessageProcessor) },
	});

	readonly Lazy<Dictionary<string, Type>> nctsMessageProcessors = new Lazy<Dictionary<string, Type>>(() => ObjectFactory.Get<INctsMessageProcessorProvider>().NctsMessageProcessors);
}
