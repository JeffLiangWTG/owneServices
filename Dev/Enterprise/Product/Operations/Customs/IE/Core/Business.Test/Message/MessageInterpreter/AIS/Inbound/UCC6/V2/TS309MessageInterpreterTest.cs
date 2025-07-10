using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS309;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS309MessageInterpreter))]
	class TS309MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, TS309MessageInterpreter, TS309Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS309;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL180", "Error Code Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL180", "13", "Error Code Item13", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:TS309 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <Declaration>
    <MRN>12MRN345ABCDE678R9</MRN>
    <invalidationDecision>true</invalidationDecision>
    <invalidationInitiatedByCustoms>true</invalidationInitiatedByCustoms>
	<invalidationJustification>Invalidation Justification Text</invalidationJustification>
	<dateOfInvalidationDecision>2023-08-10</dateOfInvalidationDecision>
	<dateOfInvalidationRequest>2023-08-11</dateOfInvalidationRequest>
	<dateOfInvalidation>2023-08-12</dateOfInvalidation>
  </Declaration>
  <FunctionalError>
    <sequenceNumber>0</sequenceNumber>
    <errorPointer>ErrorPointer001</errorPointer>
    <errorCode>13</errorCode>
    <errorReason>ER1</errorReason>
    <remarks>Functional Error Remarks 1</remarks>
    <originalAttributeValue>Original Attribute Value 1</originalAttributeValue>
  </FunctionalError>
  <FunctionalError>
    <sequenceNumber>1</sequenceNumber>
    <errorPointer>ErrorPointer001</errorPointer>
    <errorCode>13</errorCode>
    <errorReason>ER1</errorReason>
    <remarks>Functional Error Remarks 1</remarks>
    <originalAttributeValue>Original Attribute Value 1</originalAttributeValue>
  </FunctionalError>
</q1:TS309>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message)
		{
			return @"Temporary Storage Declaration Invalidation Decision (TS309) has been received and linked to job B00000012. Decision: Invalidation Request Accepted.<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Invalidation Decision</td><td>Y</td></tr><tr><td>Invalidation Initiated by Customs</td><td>Y</td></tr><tr><td>Invalidation Justification</td><td>Invalidation Justification Text</td></tr><tr><td>Date of Invalidation Decision</td><td>10-Aug-23</td></tr><tr><td>Date of Invalidation Request</td><td>11-Aug-23</td></tr><tr><td>Date of Invalidation</td><td>12-Aug-23</td></tr></table><br /><br />Functional Error: 1<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>Error Code Item13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br /><br />Functional Error: 2<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>Error Code Item13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table>";
		}

		protected override TS309Provider GetProvider(TextReader reader) => new TS309Provider(new MailBoxItemProvider<Ts309>(reader).Message);
	}
}
