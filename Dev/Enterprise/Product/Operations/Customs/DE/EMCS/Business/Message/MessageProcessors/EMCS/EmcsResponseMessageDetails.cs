using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.EMCS.Messaging;
using EMCSVersion2_4 = CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_4;
using EMCSVersion2_5 = CargoWise.Customs.DE.MessageDefinitions.EMCSVersion2_5;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public sealed class EmcsResponseMessageDetails
	{
		EmcsResponseMessageDetails() { }

		public static EmcsResponseMessageDetails Instance => instance ?? (instance = new EmcsResponseMessageDetails());
		[ThreadStatic]
		static EmcsResponseMessageDetails instance;

		public ImmutableDictionary<ZString, ResponseMessageDetails> ResponseMessages
		{
			get
			{
				if (responseMessages == null)
				{
					var combinedDictionary = Version2_5ResponseMessages;
					foreach (var keyValue in Version2_4ResponseMessages)
					{
						if (!combinedDictionary.ContainsKey(keyValue.Key))
						{
							combinedDictionary.Add(keyValue.Key, keyValue.Value);
						}
					}
					responseMessages = combinedDictionary.ToImmutableDictionary();
				}
				return responseMessages;
			}
		}
		ImmutableDictionary<ZString, ResponseMessageDetails> responseMessages;

		internal Dictionary<ZString, ResponseMessageDetails> Version2_4ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>()
		{
			{ nameof(EMCSVersion2_4.ED801D), new ResponseMessageDetails(typeof(EMCSVersion2_4.ED801D), typeof(Messaging.Version2_4.ED801Provider), typeof(EmcsInboundEDIMessage<IED801>)) },
			{ nameof(EMCSVersion2_4.ED802B), new ResponseMessageDetails(typeof(EMCSVersion2_4.ED802B), typeof(Messaging.Version2_4.ED802Provider), typeof(EmcsInboundEDIMessage<IED802>)) },
			{ nameof(EMCSVersion2_4.ED840C), new ResponseMessageDetails(typeof(EMCSVersion2_4.ED840C), typeof(Messaging.Version2_4.ED840Provider), typeof(EmcsInboundEDIMessage<IED840>)) },
			{ nameof(EMCSVersion2_4.ED813E), new ResponseMessageDetails(typeof(EMCSVersion2_4.ED813E), typeof(Messaging.Version2_4.ED813Provider), typeof(EmcsInboundEDIMessage<IED813>)) },
			{ nameof(EMCSVersion2_4.ED818C), new ResponseMessageDetails(typeof(EMCSVersion2_4.ED818C), typeof(Messaging.Version2_4.ED818Provider), typeof(EmcsInboundEDIMessage<IED818>)) },
			{ nameof(EMCSVersion2_4.ED819C), new ResponseMessageDetails(typeof(EMCSVersion2_4.ED819C), typeof(Messaging.Version2_4.ED819Provider), typeof(EmcsInboundEDIMessage<IED819>)) },
			{ nameof(EMCSVersion2_4.ED871C), new ResponseMessageDetails(typeof(EMCSVersion2_4.ED871C), typeof(Messaging.Version2_4.ED871Provider), typeof(EmcsInboundEDIMessage<IED871>)) }
		};

		internal Dictionary<ZString, ResponseMessageDetails> Version2_5ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
			{ nameof(EMCSVersion2_5.ED704C), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED704C), typeof(Messaging.Version2_5.ED704Provider), typeof(EmcsInboundEDIMessage<IED704>)) },
			{ nameof(EMCSVersion2_5.ED801E), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED801E), typeof(Messaging.Version2_5.ED801Provider), typeof(EmcsInboundEDIMessage<IED801>)) },
			{ nameof(EMCSVersion2_5.ED802C), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED802C), typeof(Messaging.Version2_5.ED802Provider), typeof(EmcsInboundEDIMessage<IED802>)) },
			{ nameof(EMCSVersion2_5.ED803B), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED803B), typeof(Messaging.Version2_5.ED803Provider), typeof(EmcsInboundEDIMessage<IED803>)) },
			{ nameof(EMCSVersion2_5.ED807B), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED807B), typeof(Messaging.Version2_5.ED807Provider), typeof(EmcsInboundEDIMessage<IED807>)) },
			{ nameof(EMCSVersion2_5.ED829C), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED829C), typeof(Messaging.Version2_5.ED829Provider), typeof(EmcsInboundEDIMessage<IED829>)) },
			{ nameof(EMCSVersion2_5.ED839C), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED839C), typeof(Messaging.Version2_5.ED839Provider), typeof(EmcsInboundEDIMessage<IED839>)) },
			{ nameof(EMCSVersion2_5.ED840D), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED840D), typeof(Messaging.Version2_5.ED840Provider), typeof(EmcsInboundEDIMessage<IED840>)) },
			{ nameof(EMCSVersion2_5.ED881A), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED881A), typeof(Messaging.Version2_5.ED881Provider), typeof(EmcsInboundEDIMessage<IED881>)) },
			{ nameof(EMCSVersion2_5.ED810C), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED810C), typeof(Messaging.Version2_5.ED810Provider), typeof(EmcsInboundEDIMessage<IED810>)) },
			{ nameof(EMCSVersion2_5.ED813F), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED813F), typeof(Messaging.Version2_5.ED813Provider), typeof(EmcsInboundEDIMessage<IED813>)) },
			{ nameof(EMCSVersion2_5.ED818D), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED818D), typeof(Messaging.Version2_5.ED818Provider), typeof(EmcsInboundEDIMessage<IED818>)) },
			{ nameof(EMCSVersion2_5.ED819D), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED819D), typeof(Messaging.Version2_5.ED819Provider), typeof(EmcsInboundEDIMessage<IED819>)) },
			{ nameof(EMCSVersion2_5.ED871D), new ResponseMessageDetails(typeof(EMCSVersion2_5.ED871D), typeof(Messaging.Version2_5.ED871Provider), typeof(EmcsInboundEDIMessage<IED871>)) }
		};
	}
}
