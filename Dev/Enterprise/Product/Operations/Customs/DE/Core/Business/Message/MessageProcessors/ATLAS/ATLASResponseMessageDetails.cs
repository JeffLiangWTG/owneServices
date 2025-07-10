using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using ATLASVersion10_1 = CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1;

namespace Enterprise.Customs.DE.Business
{
	public sealed class ATLASResponseMessageDetails
	{
		ATLASResponseMessageDetails() { }

		public static ATLASResponseMessageDetails Instance => instance ?? (instance = new ATLASResponseMessageDetails());
		[ThreadStatic]
		static ATLASResponseMessageDetails instance;

		public ImmutableDictionary<ZString, ResponseMessageDetails> ResponseMessages
		{
			get
			{
				if (responseMessages == null)
				{
					var combinedDictionary = NctsVersion10_2ResponseMessages;
					TemporaryStorageVersion10_2ResponseMessages.ForEach(x => combinedDictionary.Add(x.Key, x.Value));
					ImportAndCollectiveVersion10_2ResponseMessages.ForEach(x => combinedDictionary.Add(x.Key, x.Value));

					AppendOlderVersionToCombinedDictionary(combinedDictionary, NctsVersion10_1ResponseMessages);
					AppendOlderVersionToCombinedDictionary(combinedDictionary, TemporaryStorageVersion10_1ResponseMessages);
					AppendOlderVersionToCombinedDictionary(combinedDictionary, ImportAndCollectiveVersion10_1ResponseMessages);

					responseMessages = combinedDictionary.ToImmutableDictionary();
				}
				return responseMessages;
			}
		}
		ImmutableDictionary<ZString, ResponseMessageDetails> responseMessages;

		internal Dictionary<ZString, ResponseMessageDetails> NctsVersion10_1ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
			{ nameof(ATLASVersion10_1.DETQSC), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETQSC), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.TRQSTAProvider), typeof(AtlasInboundEDIMessage<ITRQSTA>)) },
			{ nameof(ATLASVersion10_1.DETSSB), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETSSB), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESSTAProvider), typeof(AtlasInboundEDIMessage<IDESSTA>)) },
			{ nameof(ATLASVersion10_1.DETPIA), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETPIA), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPINCProvider), typeof(AtlasInboundEDIMessage<IDEPINC>)) },
			{ nameof(ATLASVersion10_1.DEERRG), new ResponseMessageDetails(typeof(ATLASVersion10_1.DEERRG), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.ERRNCKGProvider), typeof(AtlasInboundEDIMessage<IERRNCK>)) },
			{ nameof(ATLASVersion10_1.DETPRH), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETPRH), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPRELProvider), typeof(AtlasInboundEDIMessage<IDEPREL>)) },
			{ nameof(ATLASVersion10_1.DETPJF), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETPJF), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPREJProvider), typeof(AtlasInboundEDIMessage<IDEPREJ>)) },
			{ nameof(ATLASVersion10_1.DETPSF), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETPSF), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DEPSTAProvider), typeof(AtlasInboundEDIMessage<IDEPSTA>)) },
			{ nameof(ATLASVersion10_1.DETSPC), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETSPC), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESPERProvider), typeof(AtlasInboundEDIMessage<IDESPER>)) },
			{ nameof(ATLASVersion10_1.DETSJB), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETSJB), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.DESREJProvider), typeof(AtlasInboundEDIMessage<IDESREJ>)) },
			{ nameof(ATLASVersion10_1.DETGAE), new ResponseMessageDetails(typeof(ATLASVersion10_1.DETGAE), typeof(CargoWise.Customs.DE.MessageContracts.ATLASVersion10_1.GUAACKProvider), typeof(AtlasInboundEDIMessage<IGUAACK>)) },
		};

		internal Dictionary<ZString, ResponseMessageDetails> TemporaryStorageVersion10_1ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
			{ nameof(ATLASVersion10_1.SCCANE), new ResponseMessageDetails(typeof(ATLASVersion10_1.SCCANE), typeof(Messaging.ATLASVersion10_1.CUSCANProvider), typeof(AtlasInboundEDIMessage<ICUSCAN>)) },
			{ nameof(ATLASVersion10_1.SCFING), new ResponseMessageDetails(typeof(ATLASVersion10_1.SCFING), typeof(Messaging.ATLASVersion10_1.CUSFINProvider), typeof(AtlasInboundEDIMessage<ICUSFIN>)) },
			{ nameof(ATLASVersion10_1.SCFSTF), new ResponseMessageDetails(typeof(ATLASVersion10_1.SCFSTF), typeof(Messaging.ATLASVersion10_1.CUSFSTProvider), typeof(AtlasInboundEDIMessage<IUnderCustomsControl>)) },
			{ nameof(ATLASVersion10_1.SCSTAB), new ResponseMessageDetails(typeof(ATLASVersion10_1.SCSTAB), typeof(Messaging.ATLASVersion10_1.CUSSTAProvider), typeof(AtlasInboundEDIMessage<ICUSSTA>)) },
			{ nameof(ATLASVersion10_1.SCSTPC), new ResponseMessageDetails(typeof(ATLASVersion10_1.SCSTPC), typeof(Messaging.ATLASVersion10_1.CUSSTPProvider), typeof(AtlasInboundEDIMessage<ICUSSTP>)) },
			{ nameof(ATLASVersion10_1.SCTSTJ), new ResponseMessageDetails(typeof(ATLASVersion10_1.SCTSTJ), typeof(Messaging.ATLASVersion10_1.CUSTSTProvider), typeof(AtlasInboundEDIMessage<ICUSTST>)) },
			{ nameof(ATLASVersion10_1.DEIACA), new ResponseMessageDetails(typeof(ATLASVersion10_1.DEIACA), typeof(Messaging.ATLASVersion10_1.ENSCTLProvider), typeof(AtlasInboundEDIMessage<IENSCTL>)) },
		};

		internal Dictionary<ZString, ResponseMessageDetails> ImportAndCollectiveVersion10_1ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
			// collective messages
			{ nameof(ATLASVersion10_1.DEERRF), new ResponseMessageDetails(typeof(ATLASVersion10_1.DEERRF), typeof(Messaging.ATLASVersion10_1.ERRNCKProvider), typeof(AtlasInboundEDIMessage<IERRNCK>)) },
			{ nameof(ATLASVersion10_1.GCRELG), new ResponseMessageDetails(typeof(ATLASVersion10_1.GCRELG), typeof(Messaging.ATLASVersion10_1.CURRELProvider), typeof(AtlasInboundEDIMessage<ICURREL>)) },
			{ nameof(ATLASVersion10_1.GCTAXM), new ResponseMessageDetails(typeof(ATLASVersion10_1.GCTAXM), typeof(Messaging.ATLASVersion10_1.CUSTAXProvider), typeof(AtlasInboundEDIMessage<ICUSTAX>)) },
			{ nameof(ATLASVersion10_1.GCTRAG), new ResponseMessageDetails(typeof(ATLASVersion10_1.GCTRAG), typeof(Messaging.ATLASVersion10_1.CUSTRAProvider), typeof(AtlasInboundEDIMessage<ICUSTRA>)) },
			{ nameof(ATLASVersion10_1.GCNOAD), new ResponseMessageDetails(typeof(ATLASVersion10_1.GCNOAD), typeof(Messaging.ATLASVersion10_1.CUSNOAProvider), typeof(AtlasInboundEDIMessage<ICUSNOA>)) },
			{ nameof(ATLASVersion10_1.GCRECF), new ResponseMessageDetails(typeof(ATLASVersion10_1.GCRECF), typeof(Messaging.ATLASVersion10_1.CUSRECProvider), typeof(AtlasInboundEDIMessage<ICUSREC>)) },
			{ nameof(ATLASVersion10_1.GNTAXK), new ResponseMessageDetails(typeof(ATLASVersion10_1.GNTAXK), typeof(Messaging.ATLASVersion10_1.NFFTAXProvider), typeof(AtlasInboundEDIMessage<INFFTAX>)) },

			// import
			{ nameof(ATLASVersion10_1.FCREVH), new ResponseMessageDetails(typeof(ATLASVersion10_1.FCREVH), typeof(Messaging.ATLASVersion10_1.CUSREVProvider), typeof(AtlasInboundEDIMessage<ICUSREV>)) },
			{ nameof(ATLASVersion10_1.FFTAXE), new ResponseMessageDetails(typeof(ATLASVersion10_1.FFTAXE), typeof(Messaging.ATLASVersion10_1.FINTAXProvider), typeof(AtlasInboundEDIMessage<IFINTAX>)) },
			{ nameof(ATLASVersion10_1.LECWIF), new ResponseMessageDetails(typeof(ATLASVersion10_1.LECWIF), typeof(Messaging.ATLASVersion10_1.ECWINFProvider), typeof(AtlasInboundEDIMessage<IECWINF>)) },
			{ nameof(ATLASVersion10_1.NSREVC), new ResponseMessageDetails(typeof(ATLASVersion10_1.NSREVC), typeof(Messaging.ATLASVersion10_1.SRAREVProvider), typeof(AtlasInboundEDIMessage<ISRAREV>)) },
			{ nameof(ATLASVersion10_1.NSTAXK), new ResponseMessageDetails(typeof(ATLASVersion10_1.NSTAXK), typeof(Messaging.ATLASVersion10_1.SRATAXProvider), typeof(AtlasInboundEDIMessage<ISRATAX>)) },
			{ nameof(ATLASVersion10_1.LSCWIF), new ResponseMessageDetails(typeof(ATLASVersion10_1.LSCWIF), typeof(Messaging.ATLASVersion10_1.SCWINFProvider), typeof(AtlasInboundEDIMessage<ISCWINF>)) },
			{ nameof(ATLASVersion10_1.LCWSIE), new ResponseMessageDetails(typeof(ATLASVersion10_1.LCWSIE), typeof(Messaging.ATLASVersion10_1.CWSINFProvider), typeof(AtlasInboundEDIMessage<ICWSINF>)) },
		};

		internal Dictionary<ZString, ResponseMessageDetails> NctsVersion10_2ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
		};

		internal Dictionary<ZString, ResponseMessageDetails> TemporaryStorageVersion10_2ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
		};

		internal Dictionary<ZString, ResponseMessageDetails> ImportAndCollectiveVersion10_2ResponseMessages => new Dictionary<ZString, ResponseMessageDetails>
		{
		};

		void AppendOlderVersionToCombinedDictionary(Dictionary<ZString, ResponseMessageDetails> combinedDictionary, Dictionary<ZString, ResponseMessageDetails> olderMessageVersionModuleDictionary)
		{
			foreach (var keyValue in olderMessageVersionModuleDictionary)
			{
				if (!combinedDictionary.ContainsKey(keyValue.Key))
				{
					combinedDictionary.Add(keyValue.Key, keyValue.Value);
				}
			}
		}
	}
}
