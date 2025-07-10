using System;
using Enterprise.Customs.IE.Business.AIS;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;
using AIS_H7_Version1_0 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0;

namespace Enterprise.Customs.IE.Business.UCC6.V1.Testing
{
	sealed class AISResponseMessageDetailsTest : TestCase
	{
		public void TestIM099()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM099, messageText), typeof(AIS_H7_Version1_0.IM099.Im099), typeof(IM099Processor));
		}

		public void TestIM404()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM404, messageText), typeof(AIS_H7_Version1_0.IM404.Im404), typeof(IM404Processor));
		}

		public void TestIM405()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM405, messageText), typeof(AIS_H7_Version1_0.IM405.Im405), typeof(IM405Processor));
		}

		public void TestIM409()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM409, messageText), typeof(AIS_H7_Version1_0.IM409.Im409), typeof(IM409Processor));
		}

		public void TestIM415V()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM415V, messageText), typeof(AIS_H7_Version1_0.IM415V.Im415V), typeof(IM415VProcessor));
		}

		public void TestIM460()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM460, messageText), typeof(AIS_H7_Version1_0.IM460.Im460), typeof(IM460Processor));
		}

		public void TestIM464()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM464, messageText), typeof(AIS_H7_Version1_0.IM464.Im464), typeof(IM464Processor));
		}

		public void TestIM482()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM482, messageText), typeof(AIS_H7_Version1_0.IM482.Im482), typeof(IM482Processor));
		}

		public void TestIM484()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM484, messageText), typeof(AIS_H7_Version1_0.IM484.Im484), typeof(IM484Processor));
		}

		public void TestIM862()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM862, messageText), typeof(AIS_H7_Version1_0.IM862.Im862), typeof(IM862Processor));
		}

		public void TestIM864()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM864, messageText), typeof(AIS_H7_Version1_0.IM864.Im864), typeof(IM864Processor));
		}

		public void TestIM416()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM416, messageText), typeof(AIS_H7_Version1_0.IM416.Im416), typeof(IM416Processor));
		}

		public void TestIM428()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM428, messageText), typeof(AIS_H7_Version1_0.IM428.Im428), typeof(IM428Processor));
		}

		public void TestIM429()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM429, messageText), typeof(AIS_H7_Version1_0.IM429.Im429), typeof(IM429Processor));
		}

		public void TestIM433()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM433, messageText), typeof(AIS_H7_Version1_0.IM433.Im433), typeof(IM933Processor));
		}

		public void TestIM451()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM451, messageText), typeof(AIS_H7_Version1_0.IM451.Im451), typeof(IM451Processor));
		}

		public void TestIM882()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM882, messageText), typeof(AIS_H7_Version1_0.IM882.Im882), typeof(IM882Processor));
		}

		public void TestIM884()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM884, messageText), typeof(AIS_H7_Version1_0.IM884.Im884), typeof(IM884Processor));
		}

		public void TestIM917()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM917, messageText), typeof(AIS_H7_Version1_0.IM917.Im917), typeof(IM917Processor));
		}

		public void TestIM462()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.IM462, messageText), typeof(AIS_H7_Version1_0.IM462.Im462), typeof(IM462_IM962Processor));
		}

		public void TestRF409()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.RF409, messageText), typeof(AIS_H7_Version1_0.RF409.Rf409), typeof(AIS.UCC6.V1.RF409Processor));
		}

		public void TestRF416()
		{
			AssertResponseDetail(AISResponseMessageDetails.GetResponseDetail(AISInterchangeTypeList.Codes.RF416, messageText), typeof(AIS_H7_Version1_0.RF416.Rf416), typeof(AIS.UCC6.V1.RF416Processor));
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

		readonly string messageText = "<IM099 xmlns=\"http://www.ros.ie/schemas/customs/IM099H7\">";
	}
}
