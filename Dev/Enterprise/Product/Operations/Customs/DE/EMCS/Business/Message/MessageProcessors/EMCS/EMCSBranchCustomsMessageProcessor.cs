using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class EMCSBranchCustomsMessageProcessor : BranchCustomsMessageProcessor
	{
		public EMCSBranchCustomsMessageProcessor()
			: base(new ZString[] { EDIMessage.ApplicationCodes.DECustomsEmcsSystem }, null)
		{
		}

		public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message)
		{
			BranchCustomsApplicationTypeMessageProcessor result = null;

			var applicationReference = message.EM_ApplicationReference;
			var key = applicationReference.SubstringSafe(0, 5);
			if (EmcsMessageProcessors.TryGetValue(key, out var processorType))
			{
				var obj = Activator.CreateInstance(processorType, Logger);
				if (obj is IDEBranchCustomsApplicationTypeMessageProcessorProvider provider)
				{
					result = provider.GetProcessor(message);
				}
				else
				{
					result = (BranchCustomsApplicationTypeMessageProcessor)obj;
				}
			}
			return result;
		}

		ImmutableDictionary<string, Type> emcsMessageProcessors;
		ImmutableDictionary<string, Type> EmcsMessageProcessors
		{
			get
			{
				if (emcsMessageProcessors == null)
				{
					emcsMessageProcessors = new Dictionary<string, Type>
					{
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED704C).Substring(0, 5), typeof(ED704MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED801E).Substring(0, 5), typeof(ED801MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED802B).Substring(0, 5), typeof(ED802MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED803B).Substring(0, 5), typeof(ED803MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED807B).Substring(0, 5), typeof(ED807MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED810C).Substring(0, 5), typeof(ED810MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED813F).Substring(0, 5), typeof(ED813MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED829C).Substring(0, 5), typeof(ED829MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED839C).Substring(0, 5), typeof(ED839MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED840D).Substring(0, 5), typeof(ED840MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED871D).Substring(0, 5), typeof(ED871MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4.ED881A).Substring(0, 5), typeof(ED881MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED819D).Substring(0, 5), typeof(ED819MessageProcessor) },
						{ nameof(CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5.ED818D).Substring(0, 5), typeof(ED818MessageProcessor) },
					}.ToImmutableDictionary();
				}
				return emcsMessageProcessors;
			}
		}
	}
}
