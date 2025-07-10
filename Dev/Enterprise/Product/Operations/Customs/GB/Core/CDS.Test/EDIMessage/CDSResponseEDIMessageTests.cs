using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSResponseEDIMessage))]
	class CDSResponseEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			AssertEquals(CDSEDIMessageTypeList.Codes.Response, message.EM_MessageType);
			AssertEquals(EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
		}

		public void TestEM_MessageInterpretation()
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>8GB123456789000-S0001000</FunctionalReferenceID>
    <ID>18GBJCM3USAFD2WD51</ID>
  </Declaration>
</Response>";

			AssertEquals("<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Declaration has been legally accepted</H3>" +
				"<p><strong>Function Code: </strong>01-ACC<br><strong>Old CHIEF Report Code: </strong>E2<br><strong>MRN: </strong>18GBJCM3USAFD2WD51<br><strong>LRN: </strong>8GB123456789000-S0001000<br><strong>Issued Date: </strong>2018-07-27 12:12</p>", message.EM_MessageInterpretation);
		}
	}
}
