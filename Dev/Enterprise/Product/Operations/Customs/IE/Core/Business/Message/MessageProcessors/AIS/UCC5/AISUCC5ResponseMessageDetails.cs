using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM099;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM404;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM405;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM409;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM415V;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM416;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM428;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM429;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM451;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM460;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM462;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM464;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM482;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM484;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM862;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM864;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM882;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM884;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM917;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD409;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD416;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RF409;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RF416;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS304;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS305;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS309;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS316;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS328;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS333;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS351;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public static class AISUCC5ResponseMessageDetails
	{
		public static ResponseDetail GetResponseDetail(string messageType) => ResponseDetails.TryGetValue(messageType, out var types) ? types : ResponseDetail.Empty;

		public static ImmutableDictionary<string, ResponseDetail> ResponseDetails => responseDetails ??= ImmutableDictionary.CreateRange(new Dictionary<string, ResponseDetail>
		{
			{ AISInterchangeTypeList.Codes.IM099, new ResponseDetail(typeof(Im099), typeof(IM099Processor)) },
			{ AISInterchangeTypeList.Codes.IM404, new ResponseDetail(typeof(Im404), typeof(IM404Processor)) },
			{ AISInterchangeTypeList.Codes.IM405, new ResponseDetail(typeof(Im405), typeof(IM405Processor)) },
			{ AISInterchangeTypeList.Codes.IM409, new ResponseDetail(typeof(Im409), typeof(IM409Processor)) },
			{ AISInterchangeTypeList.Codes.IM415V, new ResponseDetail(typeof(Im415V), typeof(IM415VProcessor)) },
			{ AISInterchangeTypeList.Codes.IM416, new ResponseDetail(typeof(Im416), typeof(IM416Processor)) },
			{ AISInterchangeTypeList.Codes.IM428, new ResponseDetail(typeof(Im428), typeof(IM428Processor)) },
			{ AISInterchangeTypeList.Codes.IM429, new ResponseDetail(typeof(Im429), typeof(IM429Processor)) },
			{ AISInterchangeTypeList.Codes.IM433, new ResponseDetail(typeof(Im433), typeof(IM433Processor)) },
			{ AISInterchangeTypeList.Codes.IM451, new ResponseDetail(typeof(Im451), typeof(IM451Processor)) },
			{ AISInterchangeTypeList.Codes.IM460, new ResponseDetail(typeof(Im460), typeof(IM460Processor)) },
			{ AISInterchangeTypeList.Codes.IM462, new ResponseDetail(typeof(Im462), typeof(IM462Processor)) },
			{ AISInterchangeTypeList.Codes.IM464, new ResponseDetail(typeof(Im464), typeof(IM464Processor)) },
			{ AISInterchangeTypeList.Codes.IM482, new ResponseDetail(typeof(Im482), typeof(IM482Processor)) },
			{ AISInterchangeTypeList.Codes.IM484, new ResponseDetail(typeof(Im484), typeof(IM484Processor)) },
			{ AISInterchangeTypeList.Codes.IM862, new ResponseDetail(typeof(Im862), typeof(IM862Processor)) },
			{ AISInterchangeTypeList.Codes.IM864, new ResponseDetail(typeof(Im864), typeof(IM864Processor)) },
			{ AISInterchangeTypeList.Codes.IM882, new ResponseDetail(typeof(Im882), typeof(IM882Processor)) },
			{ AISInterchangeTypeList.Codes.IM884, new ResponseDetail(typeof(Im884), typeof(IM884Processor)) },
			{ AISInterchangeTypeList.Codes.IM917, new ResponseDetail(typeof(Im917), typeof(IM917Processor)) },
			{ AISInterchangeTypeList.Codes.RD409, new ResponseDetail(typeof(Rd409), typeof(RD409Processor)) },
			{ AISInterchangeTypeList.Codes.RD416, new ResponseDetail(typeof(Rd416), typeof(RD416Processor)) },
			{ AISInterchangeTypeList.Codes.RF409, new ResponseDetail(typeof(Rf409), typeof(RF409Processor)) },
			{ AISInterchangeTypeList.Codes.RF416, new ResponseDetail(typeof(Rf416), typeof(RF416Processor)) },
			{ AISInterchangeTypeList.Codes.TS304, new ResponseDetail(typeof(Ts304), typeof(TS304Processor)) },
			{ AISInterchangeTypeList.Codes.TS305, new ResponseDetail(typeof(Ts305), typeof(TS305Processor)) },
			{ AISInterchangeTypeList.Codes.TS309, new ResponseDetail(typeof(Ts309), typeof(TS309Processor)) },
			{ AISInterchangeTypeList.Codes.TS315V, new ResponseDetail(typeof(Ts315V), typeof(TS315VProcessor)) },
			{ AISInterchangeTypeList.Codes.TS316, new ResponseDetail(typeof(Ts316), typeof(TS316Processor)) },
			{ AISInterchangeTypeList.Codes.TS328, new ResponseDetail(typeof(Ts328), typeof(TS328Processor)) },
			{ AISInterchangeTypeList.Codes.TS333, new ResponseDetail(typeof(Ts333), typeof(TS333Processor)) },
			{ AISInterchangeTypeList.Codes.TS351, new ResponseDetail(typeof(Ts351), typeof(TS351Processor)) },
		});

		[ThreadStatic]
		static ImmutableDictionary<string, ResponseDetail> responseDetails;
	}
}
