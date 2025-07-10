using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.Customs.IE.Messaging;
using AESVersion1_0 = CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0;

namespace Enterprise.Customs.IE.Business.AES
{
	public static class AESResponseMessageDetails
	{
		public static ResponseDetail GetResponseDetail(string messageType) => ResponseDetails.TryGetValue(messageType, out var types) ? types : ResponseDetail.Empty;

		public static ImmutableDictionary<string, ResponseDetail> ResponseDetails => responseDetails ?? (responseDetails = ImmutableDictionary.CreateRange(new Dictionary<string, ResponseDetail>
			{
				//TODO: Uncomment the below as we developer their corresponding processor
				{ AESIncomingMessageTypeList.Codes.EX515V, new ResponseDetail(typeof(AESVersion1_0.EX515V.Ex515V), typeof(EX515VProcessor)) },
				{ AESIncomingMessageTypeList.Codes.EX562, new ResponseDetail(typeof(AESVersion1_0.EX562.Ex562), typeof(EX562Processor)) },
				{ AESIncomingMessageTypeList.Codes.EX564, new ResponseDetail(typeof(AESVersion1_0.EX564.Ex564), typeof(EX564Processor)) },
				{ AESIncomingMessageTypeList.Codes.EX582, new ResponseDetail(typeof(AESVersion1_0.EX582.Ex582), typeof(EX582Processor)) },
				//{ AESIncomingMessageTypeList.Codes.EX583, new ResponseDetail(typeof(AESVersion1_0.EX583.EX583), typeof(EX583Processor), typeof(EX583Provider)) },
				{ AESIncomingMessageTypeList.Codes.EX584, new ResponseDetail(typeof(AESVersion1_0.EX584.Ex584), typeof(EX584Processor)) },
				{ AESIncomingMessageTypeList.Codes.EX862, new ResponseDetail(typeof(AESVersion1_0.EX862.Ex862), typeof(EX862Processor)) },
				{ AESIncomingMessageTypeList.Codes.EX864, new ResponseDetail(typeof(AESVersion1_0.EX864.Ex864), typeof(EX864Processor)) },
				{ AESIncomingMessageTypeList.Codes.EX882, new ResponseDetail(typeof(AESVersion1_0.EX882.Ex882), typeof(EX882Processor)) },
				{ AESIncomingMessageTypeList.Codes.EX884, new ResponseDetail(typeof(AESVersion1_0.EX884.Ex884), typeof(EX884Processor)) },
				{ AESIncomingMessageTypeList.Codes.IE504, new ResponseDetail(typeof(AESVersion1_0.CC504C.Cc504C), typeof(CC504CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE509, new ResponseDetail(typeof(AESVersion1_0.CC509C.Cc509C), typeof(CC509CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE521, new ResponseDetail(typeof(AESVersion1_0.CC521C.Cc521C), typeof(CC521CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE522, new ResponseDetail(typeof(AESVersion1_0.CC522C.Cc522C), typeof(CC522CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE525, new ResponseDetail(typeof(AESVersion1_0.CC525C.Cc525C), typeof(CC525CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE528, new ResponseDetail(typeof(AESVersion1_0.CC528C.Cc528C), typeof(CC528CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE529, new ResponseDetail(typeof(AESVersion1_0.CC529C.Cc529C), typeof(CC529CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE531, new ResponseDetail(typeof(AESVersion1_0.CC531C.Cc531C), typeof(CC531CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE551, new ResponseDetail(typeof(AESVersion1_0.CC551C.Cc551C), typeof(CC551CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE556, new ResponseDetail(typeof(AESVersion1_0.CC556C.Cc556C), typeof(CC556CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE557, new ResponseDetail(typeof(AESVersion1_0.CC557C.Cc557C), typeof(CC557CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE560, new ResponseDetail(typeof(AESVersion1_0.CC560C.Cc560C), typeof(CC560CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE561, new ResponseDetail(typeof(AESVersion1_0.CC561C.Cc561C), typeof(CC561CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE571, new ResponseDetail(typeof(AESVersion1_0.CC571C.Cc571C), typeof(CC571CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE574, new ResponseDetail(typeof(AESVersion1_0.CC574C.Cc574C), typeof(CC574CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE582, new ResponseDetail(typeof(AESVersion1_0.CC582C.Cc582C), typeof(CC582CProcessor)) },
				//{ AESIncomingMessageTypeList.Codes.IE590, new ResponseDetail(typeof(AESVersion1_0.CC590C.CC590C), typeof(CC590CProcessor), typeof(CC590CProvider)) },
				{ AESIncomingMessageTypeList.Codes.IE599, new ResponseDetail(typeof(AESVersion1_0.CC599C.Cc599C), typeof(CC599CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE604, new ResponseDetail(typeof(AESVersion1_0.CC604C.Cc604C), typeof(CC604CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE609, new ResponseDetail(typeof(AESVersion1_0.CC609C.Cc609C), typeof(CC609CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE628, new ResponseDetail(typeof(AESVersion1_0.CC628C.Cc628C), typeof(CC628CProcessor)) },
				{ AESIncomingMessageTypeList.Codes.IE917, new ResponseDetail(typeof(AESVersion1_0.CC917C.Cc917C), typeof(CC917CProcessor)) },
			}));

		[ThreadStatic]
		static ImmutableDictionary<string, ResponseDetail> responseDetails;
	}
}
