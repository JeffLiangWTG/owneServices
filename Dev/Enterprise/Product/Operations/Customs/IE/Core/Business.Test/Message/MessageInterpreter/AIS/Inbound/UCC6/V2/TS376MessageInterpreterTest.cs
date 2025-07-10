using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS376;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS376MessageInterpreter))]
	class TS376MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, TS376MessageInterpreter, TS376Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS376;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:TS376 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <Declaration>
    <MRN>12MRN345ABCDE678R9</MRN>
    <rejectionDate>2023-08-10</rejectionDate>
    <rejectionMotivationText>Rejection Motivation Text</rejectionMotivationText>
  </Declaration>
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
</q1:TS376>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Goods Status Report Declaration (ND4) Rejection (TS376) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Rejection Date</td><td>10-Aug-23</td></tr><tr><td>Rejection Motivation Text</td><td>Rejection Motivation Text</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr><tr><td>Error Code</td><td>52</td></tr><tr><td>Error Code Description</td><td>52</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr></table>";

		protected override TS376Provider GetProvider(TextReader reader) => new TS376Provider(new MailBoxItemProvider<Ts376>(reader).Message);
	}
}
