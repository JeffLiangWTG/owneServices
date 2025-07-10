using System;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;
using AISVersion2_0 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0;
using AISVersion2_2 = CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class AISResponseMessageDetailsTest : TestCase
	{
		public void TestIM099()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM099), typeof(AISVersion2_0.IM099.Im099), typeof(IM099Processor));
		}

		public void TestIM404()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM404), typeof(AISVersion2_0.IM404.Im404), typeof(IM404Processor));
		}

		public void TestIM405()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM405), typeof(AISVersion2_0.IM405.Im405), typeof(IM405Processor));
		}

		public void TestIM409()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM409), typeof(AISVersion2_0.IM409.Im409), typeof(IM409Processor));
		}

		public void TestIM410()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM410), typeof(AISVersion2_0.IM410.Im410), typeof(IM410Processor));
		}

		public void TestIM415V()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM415V), typeof(AISVersion2_0.IM415V.Im415V), typeof(IM415VProcessor));
		}

		public void TestIM416()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM416), typeof(AISVersion2_0.IM416.Im416), typeof(IM416Processor));
		}

		public void TestIM426()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM426), typeof(AISVersion2_0.IM426.Im426), typeof(IM426Processor));
		}

		public void TestIM428()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM428), typeof(AISVersion2_0.IM428.Im428), typeof(IM428Processor));
		}

		public void TestIM429()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM429), typeof(AISVersion2_0.IM429.Im429), typeof(IM429Processor));
		}

		public void TestIM431()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM431), typeof(AISVersion2_0.IM431.Im431), typeof(IM431Processor));
		}

		public void TestIM438()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM438), typeof(AISVersion2_0.IM438.Im438), typeof(IM438Processor));
		}

		public void TestIM444()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM444), typeof(AISVersion2_0.IM444.Im444), typeof(IM444Processor));
		}

		public void TestIM447()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM447), typeof(AISVersion2_0.IM447.Im447), typeof(IM447Processor));
		}

		public void TestIM451()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM451), typeof(AISVersion2_0.IM451.Im451), typeof(IM451Processor));
		}

		public void TestIM456()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM456), typeof(AISVersion2_0.IM456.Im456), typeof(IM456Processor));
		}

		public void TestIM457()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM457), typeof(AISVersion2_0.IM457.Im457), typeof(IM457Processor));
		}

		public void TestIM460()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM460), typeof(AISVersion2_0.IM460.Im460), typeof(IM460Processor));
		}

		public void TestIM464()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM464), typeof(AISVersion2_0.IM464.Im464), typeof(IM464Processor));
		}

		public void TestIM482()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM482), typeof(AISVersion2_0.IM482.Im482), typeof(IM482Processor));
		}

		public void TestIM484()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM484), typeof(AISVersion2_0.IM484.Im484), typeof(IM484Processor));
		}

		public void TestIM493()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM493), typeof(AISVersion2_0.IM493.Im493), typeof(IM493Processor));
		}

		public void TestIM862()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM862), typeof(AISVersion2_0.IM862.Im862), typeof(IM862Processor));
		}

		public void TestIM864()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM864), typeof(AISVersion2_0.IM864.Im864), typeof(IM864Processor));
		}

		public void TestIM882()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM882), typeof(AISVersion2_0.IM882.Im882), typeof(IM882Processor));
		}

		public void TestIM884()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM884), typeof(AISVersion2_0.IM884.Im884), typeof(IM884Processor));
		}

		public void TestIM917()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM917), typeof(AISVersion2_0.IM917.Im917), typeof(IM917Processor));
		}

		public void TestIM933()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM933), typeof(AISVersion2_0.IM933.Im933), typeof(IM933Processor));
		}

		public void TestIM962()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM962), typeof(AISVersion2_0.IM962.Im962), typeof(IM462_IM962Processor));
		}

		public void TestRD409()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.RD409), typeof(AISVersion2_2.RD409.Rd409), typeof(RD409Processor));
		}

		public void TestRD416()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.RD416), typeof(AISVersion2_2.RD416.Rd416Type), typeof(RD416Processor));
		}

		public void TestRF409()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.RF409), typeof(AISVersion2_2.RF409.Rf409Type), typeof(RF409Processor));
		}

		public void TestRF416()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.RF416), typeof(AISVersion2_2.RF416.Rf416), typeof(RF416Processor));
		}

		public void TestTS304()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS304), typeof(AISVersion2_0.TS304.Ts304), typeof(TS304Processor));
		}

		public void TestTS305()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS305), typeof(AISVersion2_0.TS305.Ts305), typeof(TS305Processor));
		}

		public void TestTS309()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS309), typeof(AISVersion2_0.TS309.Ts309), typeof(TS309Processor));
		}

		public void TestTS315V()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS315V), typeof(AISVersion2_0.TS315V.Ts315V), typeof(TS315VProcessor));
		}

		public void TestTS316()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS316), typeof(AISVersion2_0.TS316.Ts316), typeof(TS316Processor));
		}

		public void TestTS328()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS328), typeof(AISVersion2_0.TS328.Ts328), typeof(TS328Processor));
		}

		public void TestTS333()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS333), typeof(AISVersion2_0.TS333.Ts333), typeof(TS333Processor));
		}

		public void TestTS351()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS351), typeof(AISVersion2_0.TS351.Ts351), typeof(TS351Processor));
		}

		public void TestTS376()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.TS376), typeof(AISVersion2_0.TS376.Ts376), typeof(TS376Processor));
		}

		public void TestGetResponseDetail_InvalidMessageType()
		{
			AssertEquals(ResponseDetail.Empty, AISResponseMessageDetails.GetResponseDetail("!@#"));
		}

		void AssertResponseDetail(ResponseDetail detail, Type xmlObjectType, Type processorType)
		{
			AssertionWithHtml.CombineAssertions(delegate
			{
				Assertion.AssertEquals(nameof(detail.XmlObjectType), xmlObjectType, detail.XmlObjectType);
				Assertion.AssertEquals(nameof(detail.ProcessorType), processorType, detail.ProcessorType);
			});
		}
	}
}
