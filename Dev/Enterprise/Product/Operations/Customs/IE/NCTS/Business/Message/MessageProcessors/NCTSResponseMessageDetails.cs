using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.NCTS.Messaging;
using NCTSVersionP5_0 = CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public sealed class NCTSResponseMessageDetails : INCTSResponseMessageDetails
	{
		public ResponseDetail GetResponseDetail(string messageType) => ResponseDetails.TryGetValue(messageType, out var types) ? types : ResponseDetail.Empty;

		public ImmutableDictionary<string, ResponseDetail> ResponseDetails => responseDetails ?? (responseDetails = ImmutableDictionary.CreateRange(new Dictionary<string, ResponseDetail>
		{
			{ NCTSIncomingMessageTypeList.Codes.IE004, new ResponseDetail(typeof(NCTSVersionP5_0.CC004C.Cc004CType), typeof(CC004CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE009, new ResponseDetail(typeof(NCTSVersionP5_0.CC009C.Cc009CType), typeof(CC009CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE019, new ResponseDetail(typeof(NCTSVersionP5_0.CC019C.Cc019CType), typeof(CC019CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE022, new ResponseDetail(typeof(NCTSVersionP5_0.CC022C.Cc022CType), typeof(CC022CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE023, new ResponseDetail(typeof(NCTSVersionP5_0.CC023C.Cc023CType), typeof(CC023CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE025, new ResponseDetail(typeof(NCTSVersionP5_0.CC025C.Cc025CType), typeof(CC025CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE028, new ResponseDetail(typeof(NCTSVersionP5_0.CC028C.Cc028CType), typeof(CC028CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE029, new ResponseDetail(typeof(NCTSVersionP5_0.CC029C.Cc029CType), typeof(CC029CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE035, new ResponseDetail(typeof(NCTSVersionP5_0.CC035C.Cc035CType), typeof(CC035CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE037, new ResponseDetail(typeof(NCTSVersionP5_0.CC037C.Cc037CType), typeof(CC037CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE043, new ResponseDetail(typeof(NCTSVersionP5_0.CC043C.Cc043CType), typeof(CC043CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE045, new ResponseDetail(typeof(NCTSVersionP5_0.CC045C.Cc045CType), typeof(CC045CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE051, new ResponseDetail(typeof(NCTSVersionP5_0.CC051C.Cc051CType), typeof(CC051CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE055, new ResponseDetail(typeof(NCTSVersionP5_0.CC055C.Cc055CType), typeof(CC055CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE056, new ResponseDetail(typeof(NCTSVersionP5_0.CC056C.Cc056CType), typeof(CC056CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE057, new ResponseDetail(typeof(NCTSVersionP5_0.CC057C.Cc057CType), typeof(CC057CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE060, new ResponseDetail(typeof(NCTSVersionP5_0.CC060C.Cc060CType), typeof(CC060CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE140, new ResponseDetail(typeof(NCTSVersionP5_0.CC140C.Cc140CType), typeof(CC140CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE182, new ResponseDetail(typeof(NCTSVersionP5_0.CC182C.Cc182CType), typeof(CC182CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE225, new ResponseDetail(typeof(NCTSVersionP5_0.CC225C.Cc225CType), typeof(CC225CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE228, new ResponseDetail(typeof(NCTSVersionP5_0.CC228C.Cc228CType), typeof(CC228CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE229, new ResponseDetail(typeof(NCTSVersionP5_0.CC229C.Cc229CType), typeof(CC229CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE231, new ResponseDetail(typeof(NCTSVersionP5_0.CC231C.Cc231CType), typeof(CC231CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE917, new ResponseDetail(typeof(NCTSVersionP5_0.CC917C.Cc917CType), typeof(CC917CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.IE928, new ResponseDetail(typeof(NCTSVersionP5_0.CC928C.Cc928CType), typeof(CC928CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR015V, new ResponseDetail(typeof(NCTSVersionP5_0.TR015V.Tr015V), typeof(TR015VProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR054C, new ResponseDetail(typeof(NCTSVersionP5_0.TR054C.Tr054C), typeof(TR054CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR060C, new ResponseDetail(typeof(NCTSVersionP5_0.TR060C.Tr060C), typeof(TR060CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR062C, new ResponseDetail(typeof(NCTSVersionP5_0.TR062C.Tr062C), typeof(TR062CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR064C, new ResponseDetail(typeof(NCTSVersionP5_0.TR064C.Tr064C), typeof(TR064CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR082C, new ResponseDetail(typeof(NCTSVersionP5_0.TR082C.Tr082C), typeof(TR082CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR084C, new ResponseDetail(typeof(NCTSVersionP5_0.TR084C.Tr084C), typeof(TR084CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR862C, new ResponseDetail(typeof(NCTSVersionP5_0.TR862C.Tr862C), typeof(TR862CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR864C, new ResponseDetail(typeof(NCTSVersionP5_0.TR864C.Tr864C), typeof(TR864CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR882C, new ResponseDetail(typeof(NCTSVersionP5_0.TR882C.Tr882C), typeof(TR882CProcessor)) },
			{ NCTSIncomingMessageTypeList.Codes.TR884C, new ResponseDetail(typeof(NCTSVersionP5_0.TR884C.Tr884C), typeof(TR884CProcessor)) },
		}));

		[ThreadStatic]
		static ImmutableDictionary<string, ResponseDetail> responseDetails;

		public ResponseDetail AcknowledgementResponseDetail => new ResponseDetail(xmlObjectType: null, processorType: typeof(MessageAcknowledgementProcessor));

		public int CompareMessageType(ZString messageXType, ZString messageYType)
		{
			var result = 0;
			if (messageXType == messageYType)
			{
				result = 0;
			}

			// IE028 is the logically earliest message
			else if (messageXType == NCTSIncomingMessageTypeList.Codes.IE028)
			{
				result = -1;
			}
			else if (messageYType == NCTSIncomingMessageTypeList.Codes.IE028)
			{
				result = 1;
			}

			// IE029 is the logically latest message.
			else if (messageXType == NCTSIncomingMessageTypeList.Codes.IE029)
			{
				result = 1;
			}
			else if (messageYType == NCTSIncomingMessageTypeList.Codes.IE029)
			{
				result = -1;
			}

			return result;
		}
	}
}
