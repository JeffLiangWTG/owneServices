using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.Business
{
	public class DEAESBranchCustomsMessageProcessor : BranchCustomsMessageProcessor
	{
		public DEAESBranchCustomsMessageProcessor()
			: base(new ZString[] { EDIMessage.ApplicationCodes.DECustomsAesSystem }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			BranchCustomsApplicationTypeMessageProcessor result = null;
			IDictionary<string, Type> messageProcessors;
			var applicationReference = message.EM_ApplicationReference;

			if (message.EM_MessageSubType == ExportMessageSubTypeList.Codes.EXT)
			{
				messageProcessors = ObjectFactory.Get<Integration.Customs.DE.IExitControlMessageProcessor>().ExitControlMessageProcessors;
			}
			else
			{
				messageProcessors = AesMessageProcessors;
			}

			var key = applicationReference.SubstringSafe(0, 5);
			if (messageProcessors.TryGetValue(key, out var processorType))
			{
				var obj = Activator.CreateInstance(processorType, Logger);
				switch (obj)
				{
					case BranchCustomsApplicationTypeMessageProcessor messageProcessor:
						result = messageProcessor;
						break;
					case IDEBranchCustomsApplicationTypeMessageProcessorProvider provider:
						result = provider.GetProcessor(message);
						break;
				}
			}
			return result;
		}

		ImmutableDictionary<string, Type> aesMessageProcessors;
		ImmutableDictionary<string, Type> AesMessageProcessors
		{
			get
			{
				if (aesMessageProcessors == null)
				{
					aesMessageProcessors = new Dictionary<string, Type>
					{
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEERRG).Substring(0, 5), typeof(AesERRNCKMessageProcessorProvider) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPFE).Substring(0, 5), typeof(ExportEXPFUPMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPJE).Substring(0, 5), typeof(ExportEXPREJMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPLD).Substring(0, 5), typeof(ExportEXPCTLMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPNF).Substring(0, 5), typeof(ExportEXPNOTMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPRF).Substring(0, 5), typeof(ExportEXPRELMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPSE).Substring(0, 5), typeof(ExportEXPSTAMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPUC).Substring(0, 5), typeof(ExportEXPURGMessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXQSB).Substring(0, 5), typeof(ExportEXQSTAMessageProcessor) },
					}.ToImmutableDictionary();
				}
				return aesMessageProcessors;
			}
		}
	}
}
