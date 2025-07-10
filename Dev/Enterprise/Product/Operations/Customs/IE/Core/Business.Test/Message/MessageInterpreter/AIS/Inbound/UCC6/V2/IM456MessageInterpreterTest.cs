using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM456;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM456MessageInterpreter))]
	class IM456MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM456MessageInterpreter, IM456Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM456;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM456 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <LRN>LRN001</LRN>
    <MRN>12MRN345ABCDE678R9</MRN>
    <customsRegistrationNumber>12CRN345ABCDE678R9</customsRegistrationNumber>
    <businessRejectionType>BRT</businessRejectionType>
    <rejectionDateAndTime>2023-08-11T14:30:45</rejectionDateAndTime>
    <rejectionCode>1</rejectionCode>
    <rejectionReason>Rejection Reason</rejectionReason>
  </ImportOperation>
  <FunctionalError>
    <sequenceNumber>1</sequenceNumber>
    <errorPointer>ErrorPointer001</errorPointer>
    <errorCode>13</errorCode>
    <errorReason>ER1</errorReason>
    <remarks>Functional Error Remarks 1</remarks>
    <originalAttributeValue>Original Attribute Value 1</originalAttributeValue>
  </FunctionalError>
  <FunctionalError>
    <sequenceNumber>2</sequenceNumber>
    <errorPointer>ErrorPointer002</errorPointer>
    <errorCode>52</errorCode>
    <errorReason>ER2</errorReason>
    <remarks>Functional Error Remarks 2</remarks>
    <originalAttributeValue>Original Attribute Value 2</originalAttributeValue>
  </FunctionalError>
</q1:IM456>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Rejection from SCI (IM456) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Customs Registration Number</td><td>12CRN345ABCDE678R9</td></tr><tr><td>Business Rejection Type</td><td>BRT</td></tr><tr><td>Rejection Date and Time</td><td>11-Aug-23 14:30</td></tr><tr><td>Rejection Code</td><td>1</td></tr><tr><td>Rejection Reason</td><td>Rejection Reason</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr><tr><td>Error Code</td><td>52</td></tr><tr><td>Error Code Description</td><td>52</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr></table>";

		protected override IM456Provider GetProvider(TextReader reader) => new IM456Provider(new MailBoxItemProvider<Im456>(reader).Message);
	}
}
