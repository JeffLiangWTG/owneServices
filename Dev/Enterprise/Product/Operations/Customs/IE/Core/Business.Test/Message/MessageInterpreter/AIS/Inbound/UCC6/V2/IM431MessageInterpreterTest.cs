using System.IO;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM431;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM431MessageInterpreter))]
	class IM431MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM431MessageInterpreter, IM431Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM431;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", @"<q1:IM431 xmlns:q1=""http://www.ros.ie/schemas/customs"">
  <ImportOperation>
    <LRN>LRN001</LRN>
    <MRN>12MRN345CDEFG678R9</MRN>
  </ImportOperation>
  <TimerExpiryForSupplementaryDeclaration>
    <lodgementOfSupplementaryDeclarationStartDate>2023-08-10</lodgementOfSupplementaryDeclarationStartDate>
    <lodgementOfSupplementaryDeclarationExpiryDate>2023-08-11</lodgementOfSupplementaryDeclarationExpiryDate>
    <timerExpiryInformation>Timer Expiry Information</timerExpiryInformation>
  </TimerExpiryForSupplementaryDeclaration>
</q1:IM431>");
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"An Expiration of Timer for Supplementary Declaration (IM431) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Lodgement of Supplementary Declaration Start Date</td><td>10-Aug-23</td></tr><tr><td>Lodgement of Supplementary Declaration Expiry Date</td><td>11-Aug-23</td></tr><tr><td>Timer Expiry Information</td><td>Timer Expiry Information</td></tr></table>";

		protected override IM431Provider GetProvider(TextReader reader) => new IM431Provider(new MailBoxItemProvider<Im431>(reader).Message);
	}
}
