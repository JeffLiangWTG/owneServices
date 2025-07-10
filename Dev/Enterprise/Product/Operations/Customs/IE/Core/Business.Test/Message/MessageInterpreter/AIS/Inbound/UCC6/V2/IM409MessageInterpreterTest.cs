using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM409;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM409MessageInterpreter))]
	class IM409MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM409MessageInterpreter, IIM409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM409;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest() => CreateIncomingMessage(true);

		AISInboundEDIMessage CreateIncomingMessage(bool invalidationDecision)
		{
			AISInterchangeProcessorTestHelper.CreateCL180ReferenceTestData(Factory);

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", $@"<q1:IM409 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <MRN>12MRN345CDEFG678R9</MRN>
    <InvalidationDecision>{invalidationDecision.ToString().ToLower()}</InvalidationDecision>
    <InvalidationInitiatedByCustoms>true</InvalidationInitiatedByCustoms>
    <InvalidationJustification>Invalidation Justification</InvalidationJustification>
    <DateOfInvalidationDecision>2023-08-11</DateOfInvalidationDecision>
    <DateOfInvalidationRequest>2023-08-10</DateOfInvalidationRequest>
    <DateOfInvalidation>2023-08-09</DateOfInvalidation>
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
</q1:IM409>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => GetExpectedInterpretation(true);

		protected override IIM409Provider GetProvider(TextReader reader) => new IM409Provider(new MailBoxItemProvider<Im409>(reader).Message);

		public void TestWhenInvalidationDecisionIsFalse()
		{
			var message = CreateIncomingMessage(false);
			message.EM_MessageType = MessageType;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			Factory.Save();
			var interpreter = GetInterpreter(message);
			AssertXMLEquals(GetExpectedInterpretation(false).RemoveLineBreakingsAndIndents(),
				interpreter.GetInterpretation().RemoveLineBreakingsAndIndents()
			);
		}

		internal static string GetExpectedInterpretation(ZBool decision) => $@"[IM409 - Invalidation Decision] has been received and linked to job B00001000. Decision: Invalidation {DecisionWord(decision)}<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
				<tr><td>Invalidation Decision</td><td>{decision}</td></tr>
				<tr><td>Invalidation Initiated by Customs</td><td>Y</td></tr>
				<tr><td>Invalidation Justification</td><td>Invalidation Justification</td></tr>
				<tr><td>Date of Invalidation Decision</td><td>11-Aug-23</td></tr>
				<tr><td>Date of Invalidation Request</td><td>10-Aug-23</td></tr>
				<tr><td>Date of Invalidation</td><td>09-Aug-23</td></tr>
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

		static string DecisionWord(ZBool decision) => decision ? "Rejected" : "Accepted";
	}
}
