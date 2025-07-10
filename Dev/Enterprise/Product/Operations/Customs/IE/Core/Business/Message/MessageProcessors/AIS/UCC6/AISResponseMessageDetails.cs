using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.RegularExpressions;
using Enterprise.Customs.IE.Messaging;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0;
using AISVersion2_2 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2;

namespace Enterprise.Customs.IE.Business.AIS
{
	public static class AISResponseMessageDetails
	{
		public static ResponseDetail GetResponseDetail(string messageType, string messageText = null)
		{
			if (!string.IsNullOrEmpty(messageText) && Regex.IsMatch(messageText, @"http://www\.ros\.ie/schemas/customs/.+H7"))
			{
				return H7ResponseDetails.TryGetValue(messageType, out var types) ? types : ResponseDetail.Empty;
			}
			else
			{
				return ResponseDetails.TryGetValue(messageType, out var types) ? types : ResponseDetail.Empty;
			}
		}

		public static ImmutableDictionary<string, ResponseDetail> ResponseDetails => responseDetails ?? (responseDetails = ImmutableDictionary.CreateRange(new Dictionary<string, ResponseDetail>
		  {
			 { AISInterchangeTypeList.Codes.IM099, new ResponseDetail(typeof(AISVersion2_0.IM099.Im099), typeof(IM099Processor)) },
			 { AISInterchangeTypeList.Codes.IM404, new ResponseDetail(typeof(AISVersion2_0.IM404.Im404), typeof(IM404Processor)) },
			 { AISInterchangeTypeList.Codes.IM405, new ResponseDetail(typeof(AISVersion2_0.IM405.Im405), typeof(IM405Processor)) },
			 { AISInterchangeTypeList.Codes.IM409, new ResponseDetail(typeof(AISVersion2_0.IM409.Im409), typeof(IM409Processor)) },
			 { AISInterchangeTypeList.Codes.IM410, new ResponseDetail(typeof(AISVersion2_0.IM410.Im410), typeof(IM410Processor)) },
			 { AISInterchangeTypeList.Codes.IM415V, new ResponseDetail(typeof(AISVersion2_0.IM415V.Im415V), typeof(IM415VProcessor)) },
			 { AISInterchangeTypeList.Codes.IM416, new ResponseDetail(typeof(AISVersion2_0.IM416.Im416), typeof(IM416Processor)) },
			 { AISInterchangeTypeList.Codes.IM426, new ResponseDetail(typeof(AISVersion2_0.IM426.Im426), typeof(IM426Processor)) },
			 { AISInterchangeTypeList.Codes.IM428, new ResponseDetail(typeof(AISVersion2_0.IM428.Im428), typeof(IM428Processor)) },
			 { AISInterchangeTypeList.Codes.IM429, new ResponseDetail(typeof(AISVersion2_0.IM429.Im429), typeof(IM429Processor)) },
			 { AISInterchangeTypeList.Codes.IM431, new ResponseDetail(typeof(AISVersion2_0.IM431.Im431), typeof(IM431Processor)) },
			 { AISInterchangeTypeList.Codes.IM438, new ResponseDetail(typeof(AISVersion2_0.IM438.Im438), typeof(IM438Processor)) },
			 { AISInterchangeTypeList.Codes.IM444, new ResponseDetail(typeof(AISVersion2_0.IM444.Im444), typeof(IM444Processor)) },
			 { AISInterchangeTypeList.Codes.IM447, new ResponseDetail(typeof(AISVersion2_0.IM447.Im447), typeof(IM447Processor)) },
			 { AISInterchangeTypeList.Codes.IM451, new ResponseDetail(typeof(AISVersion2_0.IM451.Im451), typeof(IM451Processor)) },
			 { AISInterchangeTypeList.Codes.IM456, new ResponseDetail(typeof(AISVersion2_0.IM456.Im456), typeof(IM456Processor)) },
			 { AISInterchangeTypeList.Codes.IM457, new ResponseDetail(typeof(AISVersion2_0.IM457.Im457), typeof(IM457Processor)) },
			 { AISInterchangeTypeList.Codes.IM460, new ResponseDetail(typeof(AISVersion2_0.IM460.Im460), typeof(IM460Processor)) },
			 { AISInterchangeTypeList.Codes.IM464, new ResponseDetail(typeof(AISVersion2_0.IM464.Im464), typeof(IM464Processor)) },
			 { AISInterchangeTypeList.Codes.IM482, new ResponseDetail(typeof(AISVersion2_0.IM482.Im482), typeof(IM482Processor)) },
			 { AISInterchangeTypeList.Codes.IM484, new ResponseDetail(typeof(AISVersion2_0.IM484.Im484), typeof(IM484Processor)) },
			 { AISInterchangeTypeList.Codes.IM493, new ResponseDetail(typeof(AISVersion2_0.IM493.Im493), typeof(IM493Processor)) },
			 { AISInterchangeTypeList.Codes.IM862, new ResponseDetail(typeof(AISVersion2_0.IM862.Im862), typeof(IM862Processor)) },
			 { AISInterchangeTypeList.Codes.IM864, new ResponseDetail(typeof(AISVersion2_0.IM864.Im864), typeof(IM864Processor)) },
			 { AISInterchangeTypeList.Codes.IM882, new ResponseDetail(typeof(AISVersion2_0.IM882.Im882), typeof(IM882Processor)) },
			 { AISInterchangeTypeList.Codes.IM884, new ResponseDetail(typeof(AISVersion2_0.IM884.Im884), typeof(IM884Processor)) },
			 { AISInterchangeTypeList.Codes.IM917, new ResponseDetail(typeof(AISVersion2_0.IM917.Im917), typeof(IM917Processor)) },
			 { AISInterchangeTypeList.Codes.IM933, new ResponseDetail(typeof(AISVersion2_0.IM933.Im933), typeof(IM933Processor)) },
			 { AISInterchangeTypeList.Codes.IM962, new ResponseDetail(typeof(AISVersion2_0.IM962.Im962), typeof(IM462_IM962Processor)) },
			 { AISInterchangeTypeList.Codes.RD409, new ResponseDetail(typeof(AISVersion2_2.RD409.Rd409), typeof(RD409Processor)) },
			 { AISInterchangeTypeList.Codes.RD416, new ResponseDetail(typeof(AISVersion2_2.RD416.Rd416Type), typeof(RD416Processor)) },
			 { AISInterchangeTypeList.Codes.RF409, new ResponseDetail(typeof(AISVersion2_2.RF409.Rf409Type), typeof(RF409Processor)) },
			 { AISInterchangeTypeList.Codes.RF416, new ResponseDetail(typeof(AISVersion2_2.RF416.Rf416), typeof(RF416Processor)) },
			 { AISInterchangeTypeList.Codes.TS304, new ResponseDetail(typeof(AISVersion2_0.TS304.Ts304), typeof(TS304Processor)) },
			 { AISInterchangeTypeList.Codes.TS305, new ResponseDetail(typeof(AISVersion2_0.TS305.Ts305), typeof(TS305Processor)) },
			 { AISInterchangeTypeList.Codes.TS309, new ResponseDetail(typeof(AISVersion2_0.TS309.Ts309), typeof(TS309Processor)) },
			 { AISInterchangeTypeList.Codes.TS315V, new ResponseDetail(typeof(AISVersion2_0.TS315V.Ts315V), typeof(TS315VProcessor)) },
			 { AISInterchangeTypeList.Codes.TS316, new ResponseDetail(typeof(AISVersion2_0.TS316.Ts316), typeof(TS316Processor)) },
			 { AISInterchangeTypeList.Codes.TS328, new ResponseDetail(typeof(AISVersion2_0.TS328.Ts328), typeof(TS328Processor)) },
			 { AISInterchangeTypeList.Codes.TS333, new ResponseDetail(typeof(AISVersion2_0.TS333.Ts333), typeof(TS333Processor)) },
			 { AISInterchangeTypeList.Codes.TS351, new ResponseDetail(typeof(AISVersion2_0.TS351.Ts351), typeof(TS351Processor)) },
			 { AISInterchangeTypeList.Codes.TS376, new ResponseDetail(typeof(AISVersion2_0.TS376.Ts376), typeof(TS376Processor)) }
		  }));

		public static ImmutableDictionary<string, ResponseDetail> H7ResponseDetails => h7ResponseDetails ?? (h7ResponseDetails = ImmutableDictionary.CreateRange(new Dictionary<string, ResponseDetail>
		  {
			 { AISInterchangeTypeList.Codes.IM099, new ResponseDetail(typeof(AIS_H7_Version1_0.IM099.Im099), typeof(IM099Processor)) },
			 { AISInterchangeTypeList.Codes.IM404, new ResponseDetail(typeof(AIS_H7_Version1_0.IM404.Im404), typeof(IM404Processor)) },
			 { AISInterchangeTypeList.Codes.IM405, new ResponseDetail(typeof(AIS_H7_Version1_0.IM405.Im405), typeof(IM405Processor)) },
			 { AISInterchangeTypeList.Codes.IM409, new ResponseDetail(typeof(AIS_H7_Version1_0.IM409.Im409), typeof(IM409Processor)) },
			 { AISInterchangeTypeList.Codes.IM415V, new ResponseDetail(typeof(AIS_H7_Version1_0.IM415V.Im415V), typeof(IM415VProcessor)) },
			 { AISInterchangeTypeList.Codes.IM460, new ResponseDetail(typeof(AIS_H7_Version1_0.IM460.Im460), typeof(IM460Processor)) },
			 { AISInterchangeTypeList.Codes.IM464, new ResponseDetail(typeof(AIS_H7_Version1_0.IM464.Im464), typeof(IM464Processor)) },
			 { AISInterchangeTypeList.Codes.IM482, new ResponseDetail(typeof(AIS_H7_Version1_0.IM482.Im482), typeof(IM482Processor)) },
			 { AISInterchangeTypeList.Codes.IM484, new ResponseDetail(typeof(AIS_H7_Version1_0.IM484.Im484), typeof(IM484Processor)) },
			 { AISInterchangeTypeList.Codes.IM862, new ResponseDetail(typeof(AIS_H7_Version1_0.IM862.Im862), typeof(IM862Processor)) },
			 { AISInterchangeTypeList.Codes.IM864, new ResponseDetail(typeof(AIS_H7_Version1_0.IM864.Im864), typeof(IM864Processor)) },
			 { AISInterchangeTypeList.Codes.IM416, new ResponseDetail(typeof(AIS_H7_Version1_0.IM416.Im416), typeof(IM416Processor)) },
			 { AISInterchangeTypeList.Codes.IM428, new ResponseDetail(typeof(AIS_H7_Version1_0.IM428.Im428), typeof(IM428Processor)) },
			 { AISInterchangeTypeList.Codes.IM429, new ResponseDetail(typeof(AIS_H7_Version1_0.IM429.Im429), typeof(IM429Processor)) },
			 { AISInterchangeTypeList.Codes.IM433, new ResponseDetail(typeof(AIS_H7_Version1_0.IM433.Im433), typeof(IM933Processor)) },
			 { AISInterchangeTypeList.Codes.IM451, new ResponseDetail(typeof(AIS_H7_Version1_0.IM451.Im451), typeof(IM451Processor)) },
			 { AISInterchangeTypeList.Codes.IM882, new ResponseDetail(typeof(AIS_H7_Version1_0.IM882.Im882), typeof(IM882Processor)) },
			 { AISInterchangeTypeList.Codes.IM884, new ResponseDetail(typeof(AIS_H7_Version1_0.IM884.Im884), typeof(IM884Processor)) },
			 { AISInterchangeTypeList.Codes.IM917, new ResponseDetail(typeof(AIS_H7_Version1_0.IM917.Im917), typeof(IM917Processor)) },
			 { AISInterchangeTypeList.Codes.IM462, new ResponseDetail(typeof(AIS_H7_Version1_0.IM462.Im462), typeof(IM462_IM962Processor)) },
			 { AISInterchangeTypeList.Codes.RF409, new ResponseDetail(typeof(AIS_H7_Version1_0.RF409.Rf409), typeof(UCC6.V1.RF409Processor)) },
			 { AISInterchangeTypeList.Codes.RF416, new ResponseDetail(typeof(AIS_H7_Version1_0.RF416.Rf416), typeof(UCC6.V1.RF416Processor)) },
		  }));

		[ThreadStatic]
		static ImmutableDictionary<string, ResponseDetail> responseDetails;

		[ThreadStatic]
		static ImmutableDictionary<string, ResponseDetail> h7ResponseDetails;
	}
}
