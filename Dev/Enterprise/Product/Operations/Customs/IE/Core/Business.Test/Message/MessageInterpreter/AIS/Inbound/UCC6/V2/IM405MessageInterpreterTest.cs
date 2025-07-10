using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM405MessageInterpreter))]
	class IM405MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM405MessageInterpreter, IIM405Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM405;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			AISInterchangeProcessorTestHelper.CreateCL180ReferenceTestData(Factory);

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", @"<q1:IM405 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <MRN>12MRN345CDEFG678R9</MRN>
    <AmendmentRejectionDate>2023-08-10</AmendmentRejectionDate>
    <AmendmentRejectionMotivationText>Amendment Rejection Motivation Text</AmendmentRejectionMotivationText>
    <Remarks>Remarks001</Remarks>
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
</q1:IM405>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => ExpectedInterpretation;

		protected override IIM405Provider GetProvider(TextReader reader) => new IM405Provider(new MailBoxItemProvider<Im405>(reader).Message);

		internal const string ExpectedInterpretation = @"An Amendment Request Rejection (IM405) message has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
				<tr><td>Amendment Rejection Date</td><td>10-Aug-23</td></tr>
				<tr><td>Amendment Rejection Motivation Text</td><td>Amendment Rejection Motivation Text</td></tr>
				<tr><td>Remarks</td><td>Remarks001</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Functional Error</td><td>1</td></tr>
				<tr><td>Error Pointer</td><td>ErrorPointer001</td></tr>
				<tr><td>Error Code</td><td>13</td></tr>
				<tr><td>Error Code Description</td><td>13 - Condition violation (Missing)</td></tr>
				<tr><td>Error Reason</td><td>ER1</td></tr>
				<tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr>
				<tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Functional Error</td><td>2</td></tr>
				<tr><td>Error Pointer</td><td>ErrorPointer002</td></tr>
				<tr><td>Error Code</td><td>52</td></tr>
				<tr><td>Error Code Description</td><td>52 - Functional violation post downgrade</td></tr>
				<tr><td>Error Reason</td><td>ER2</td></tr>
				<tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr>
				<tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr>
			</table>";
	}
}
