using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM451;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM451MessageInterpreter))]
	class IM451MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM451MessageInterpreter, IIM451Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM451;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland);
			helper.CreateNewOrGetExistingCusCodeType("CL716", "Control Type");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, "CL716", "Red", "Physical Control - R", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeType("CL740", "Risk Area Code");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "CL740", "100000", "Cash control", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM451 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <MRN>12MRN345ABCDE678R9</MRN>
    <LRN>LRN001</LRN>
    <declarationType>CO</declarationType>
    <additionalDeclarationType>A</additionalDeclarationType>
    <decisionDate>2023-08-11</decisionDate>
    <decisionReason>Decision Reason</decisionReason>
    <PreferredPaymentMethod>A</PreferredPaymentMethod>
    <Remarks>Remarks001</Remarks>
  </ImportOperation>
  <ControlResults>
    <sequenceNumber>1</sequenceNumber>
    <declarationGoodsItemNumber>10001</declarationGoodsItemNumber>
    <controlResultCode>A4</controlResultCode>
    <ResultsOfControl>
      <sequenceNumber>1</sequenceNumber>
      <riskAreaCode>100000</riskAreaCode>
      <controlType>Red</controlType>
      <controlDate>2023-08-09</controlDate>
      <remarks>Control Results Remarks 011</remarks>
      <ControlDetails>
        <sequenceNumber>1</sequenceNumber>
        <typeOfDiscrepancies>TD</typeOfDiscrepancies>
        <attributePointer>Attribute Pointer 111</attributePointer>
        <correctedValue>Corrected Value 111</correctedValue>
        <remarks>Control Details Remarks 111</remarks>
      </ControlDetails>
      <ControlDetails>
        <sequenceNumber>2</sequenceNumber>
        <typeOfDiscrepancies>TE</typeOfDiscrepancies>
        <attributePointer>Attribute Pointer 112</attributePointer>
        <correctedValue>Corrected Value 112</correctedValue>
        <remarks>Control Details Remarks 112</remarks>
      </ControlDetails>
    </ResultsOfControl>
    <ResultsOfControl>
      <sequenceNumber>2</sequenceNumber>
      <riskAreaCode>100000</riskAreaCode>
      <controlType>Red</controlType>
      <controlDate>2023-08-10</controlDate>
      <remarks>Control Results Remarks 012</remarks>
      <ControlDetails>
        <sequenceNumber>1</sequenceNumber>
        <typeOfDiscrepancies>TF</typeOfDiscrepancies>
        <attributePointer>Attribute Pointer 121</attributePointer>
        <correctedValue>Corrected Value 111</correctedValue>
        <remarks>Control Details Remarks 121</remarks>
      </ControlDetails>
      <ControlDetails>
        <sequenceNumber>2</sequenceNumber>
        <typeOfDiscrepancies>TG</typeOfDiscrepancies>
        <attributePointer>Attribute Pointer 122</attributePointer>
        <correctedValue>Corrected Value 112</correctedValue>
        <remarks>Control Details Remarks 122</remarks>
      </ControlDetails>
    </ResultsOfControl>
  </ControlResults>
  <ControlResults>
    <sequenceNumber>2</sequenceNumber>
    <declarationGoodsItemNumber>20001</declarationGoodsItemNumber>
    <controlResultCode>A4</controlResultCode>
    <ResultsOfControl>
      <sequenceNumber>1</sequenceNumber>
      <riskAreaCode>100000</riskAreaCode>
      <controlType>Red</controlType>
      <controlDate>2023-08-11</controlDate>
      <remarks>Control Results Remarks 021</remarks>
      <ControlDetails>
        <sequenceNumber>1</sequenceNumber>
        <typeOfDiscrepancies>TH</typeOfDiscrepancies>
        <attributePointer>Attribute Pointer 211</attributePointer>
        <correctedValue>Corrected Value 211</correctedValue>
        <remarks>Control Details Remarks 211</remarks>
      </ControlDetails>
    </ResultsOfControl>
  </ControlResults>
</q1:IM451>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A No Release (IM451) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Declaration Type</td><td>CO</td></tr><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>Decision Date</td><td>11-Aug-23</td></tr><tr><td>Decision Reason</td><td>Decision Reason</td></tr><tr><td>Preferred Payment Method</td><td>A</td></tr><tr><td>Remarks</td><td>Remarks001</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Results</td><td>1</td></tr><tr><td>-Declaration Goods Item Number</td><td>10001</td></tr><tr><td>-Control Result Code</td><td>A4</td></tr><tr><td>-Results of Control</td><td>1</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>09-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 011</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TD</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 111</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 111</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 111</td></tr><tr><td>--Control Details</td><td>2</td></tr><tr><td>---Type of Discrepancies</td><td>TE</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 112</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 112</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 112</td></tr><tr><td>-Results of Control</td><td>2</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>10-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 012</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TF</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 121</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 111</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 121</td></tr><tr><td>--Control Details</td><td>2</td></tr><tr><td>---Type of Discrepancies</td><td>TG</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 122</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 112</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 122</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Control Results</td><td>2</td></tr><tr><td>-Declaration Goods Item Number</td><td>20001</td></tr><tr><td>-Control Result Code</td><td>A4</td></tr><tr><td>-Results of Control</td><td>1</td></tr><tr><td>--Risk Area Code</td><td>100000</td></tr><tr><td>--Risk Area Code Description</td><td>Cash control</td></tr><tr><td>--Control Type</td><td>Red</td></tr><tr><td>--Control Type Description</td><td>Physical Control - R</td></tr><tr><td>--Control Date</td><td>11-Aug-23</td></tr><tr><td>--Remarks</td><td>Control Results Remarks 021</td></tr><tr><td>--Control Details</td><td>1</td></tr><tr><td>---Type of Discrepancies</td><td>TH</td></tr><tr><td>---Attribute Pointer</td><td>Attribute Pointer 211</td></tr><tr><td>---Corrected Value</td><td>Corrected Value 211</td></tr><tr><td>---Remarks</td><td>Control Details Remarks 211</td></tr></table>";

		protected override IIM451Provider GetProvider(TextReader reader) => new IM451Provider(new MailBoxItemProvider<Im451>(reader).Message);
	}
}
