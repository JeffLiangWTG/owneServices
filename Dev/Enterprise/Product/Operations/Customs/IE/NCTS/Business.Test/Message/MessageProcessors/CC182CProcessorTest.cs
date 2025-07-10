using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC182C;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.NCTS.Messaging;
using Enterprise.Customs.IE.NCTS.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(CC182CProcessor))]
	class CC182CProcessorTest : NCTSDepartureMessageProcessorAbstractTest<CC182CProcessor, NCTSInboundEDIMessage, NCTSOutboundEDIMessage, CC182CProvider>
	{
		protected override void AssertProcessResultCore(NctsDepartureMovementHeader movementHeader, NCTSInboundEDIMessage incomingMessage)
		{
			AssertEquals("BM_MessageStatus", LogicalStatusList.Codes.Accepted, movementHeader.BM_MessageStatus);
			AssertEquals("BM_CustomsStatus", NCTS5DepartureCustomsStatusList.Codes.IncidentRegistered, movementHeader.BM_CustomsStatus);
			AssertMessageInterpretation(incomingMessage, @"
			A Forwarded Incident Notification Message (IE182) has been received for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>19AA12345678901230</td></tr>
				<tr><td>Incident Notification Date &amp; Time</td><td>30-Jan-23 15:00</td></tr>
			</table><br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>Incident Code</td><td>1</td></tr>
				<tr><td>Incident Text</td><td>Incident</td></tr>
				<tr><td>Endorsement Date</td><td>20-Jan-23</td></tr>
				<tr><td>Endorsement Authority</td><td>A</td></tr>
				<tr><td>Endorsement Place</td><td>Endorsement Place</td></tr>
				<tr><td>Endorsement Country</td><td>IE</td></tr>
				<tr><td>Container Number</td><td>1</td></tr>
				<tr><td>-Number of Seals</td><td>1</td></tr>
				<tr><td>-Seals Identifier</td><td>1</td></tr>
			</table>");
			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Forwarded Incident Notification Message (IE182) has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "CC182C: FORWARDED INCIDENT NOTIFICATION";

		protected override CC182CProcessor Processor => new CC182CProcessor(logger, typeof(Cc182CType));

		protected override ZString MessageType => NCTSIncomingMessageTypeList.Codes.IE182;

		protected override ZString MessageText => InterchangeProcessorTestHelper.GetStandardCC182CText();
	}
}
