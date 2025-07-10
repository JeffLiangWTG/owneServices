using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.Types;
using MessageDefinitionsAESVersion3_0 = CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class AesResponseMessageDetails
	{
		AesResponseMessageDetails() { }

		public static AesResponseMessageDetails Instance => instance ?? (instance = new AesResponseMessageDetails());
		[ThreadStatic]
		static AesResponseMessageDetails instance;

		public ImmutableDictionary<ZString, ResponseMessageDetails> ResponseMessages
		{
			get
			{
				if (responseMessages == null)
				{
					var combinedDictionary = AesVersion4_0ResponseMessages;
					foreach (var keyValue in AesVersion3_0ResponseMessages)
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

		internal Dictionary<ZString, ResponseMessageDetails> AesVersion4_0ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
		};

		internal Dictionary<ZString, ResponseMessageDetails> AesVersion3_0ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
			{ nameof(MessageDefinitionsAESVersion3_0.DEXPRF), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXPRF), typeof(Messaging.AESVersion3_0.EXPRELProvider), typeof(AesInboundEDIMessage<IEXPREL>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXPLD), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXPLD), typeof(Messaging.AESVersion3_0.EXPCTLProvider), typeof(AesInboundEDIMessage<IEXPCTL>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXPFE), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXPFE), typeof(Messaging.AESVersion3_0.EXPFUPProvider), typeof(AesInboundEDIMessage<IEXPFUP>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXPNF), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXPNF), typeof(Messaging.AESVersion3_0.EXPNOTProvider), typeof(AesInboundEDIMessage<IEXPNOT>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXPUC), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXPUC), typeof(Messaging.AESVersion3_0.EXPURGProvider), typeof(AesInboundEDIMessage<IEXPURG>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXQSB), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXQSB), typeof(Messaging.AESVersion3_0.EXQSTAProvider), typeof(AesInboundEDIMessage<IEXQSTA>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEERRG), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEERRG), typeof(Messaging.AESVersion3_0.ERRNCKProvider), typeof(AesInboundEDIMessage<IERRNCK>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXPSE), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXPSE), typeof(Messaging.AESVersion3_0.EXPSTAProvider), typeof(AesInboundEDIMessage<IEXPSTA>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXPJE), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXPJE), typeof(Messaging.AESVersion3_0.EXPREJProvider), typeof(AesInboundEDIMessage<IEXPREJ>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXTLF), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXTLF), typeof(Messaging.AESVersion3_0.EXTCTLProvider), typeof(AesInboundEDIMessage<IEXTCTL>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXTJE), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXTJE), typeof(CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXTREJProvider), typeof(AesInboundEDIMessage<IEXTREJ>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXTSE), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXTSE), typeof(Messaging.AESVersion3_0.EXTSTAProvider), typeof(AesInboundEDIMessage<IEXTSTA>)) },
			{ nameof(MessageDefinitionsAESVersion3_0.DEXTDE), new ResponseMessageDetails(typeof(MessageDefinitionsAESVersion3_0.DEXTDE), typeof(CargoWise.Customs.DE.MessageContracts.AESVersion3_0.EXTDATProvider), typeof(AesInboundEDIMessage<IEXTDAT>)) },
		};
	}
}
