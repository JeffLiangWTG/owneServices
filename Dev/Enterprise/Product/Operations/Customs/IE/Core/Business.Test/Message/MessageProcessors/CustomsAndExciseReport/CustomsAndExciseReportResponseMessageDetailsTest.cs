using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing;

class CustomsAndExciseReportResponseMessageDetailsTest : TestCaseWithFactory
{
	public void TestGetResponseDetail()
	{
		var detailsPSR = CustomsAndExciseReportResponseMessageDetails.GetResponseDetail(CustomsAndExciseReportTypeList.Codes.PSR, null);
		CombineAssertions("PSR", () =>
		{
			AssertEquals("XmlObjectType", typeof(PSRMessage), detailsPSR.XmlObjectType);
			AssertEquals("ProcessorType", typeof(PSRProcessor), detailsPSR.ProcessorType);
		});

		var rosErrorText = "<ns2:MessageAcknowledgement \r\n\r\nxmlns:ns2=\"http://www.ros.ie/schemas/customs/messageacknowledgement/v1\"> \r\n\r\n<ns2:ErrorReference> \r\n\r\n<ns2:ErrorCode>111008</ns2:ErrorCode> \r\n\r\n</ns2:ErrorReference> \r\n\r\n</ns2:MessageAcknowledgement>";
		var detailsROSError = CustomsAndExciseReportResponseMessageDetails.GetResponseDetail(CustomsAndExciseReportTypeList.Codes.PSR, rosErrorText);
		CombineAssertions("ROS Error", () =>
		{
			AssertEquals("XmlObjectType", typeof(MessageAcknowledgement), detailsROSError.XmlObjectType);
			AssertEquals("ProcessorType", typeof(CustomsAndExciseReportErrorProcessor), detailsROSError.ProcessorType);
		});
	}
}
