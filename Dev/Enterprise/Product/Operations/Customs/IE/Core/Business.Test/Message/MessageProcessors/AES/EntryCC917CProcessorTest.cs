using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC917CProcessor))]
	class EntryCC917CProcessorTest : CC917CProcessorTest<CusEntryHeader, JobDeclaration>
	{
		public void TestCanProcessCC917CMessageWithoutMessageSender()
		{
			(var declaration, var entry, var outgoingMessage, var incomingMessage) = CreateSetupData(InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, @"<ns3:CC917C xmlns=""http://www.revenue.ie/rcm/"" xmlns:SOAP-ENV=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ns3=""http://ecs.dgtaxud.ec"">
		    <XMLError xmlns="""" xmlns:ns6=""http://www.revenue.ie/rcm/"">
		      <errorLineNumber>1</errorLineNumber>
		      <errorColumnNumber>693</errorColumnNumber>
		      <errorText>cvc-pattern-valid: Value '' is not facet-valid with respect to pattern '.{3}' for type 'SpecificCircumstanceIndicatorContentType'.</errorText>
		    </XMLError>
		  </ns3:CC917C>", includeResponseWrap: false));
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);
				CombineAssertions("Process", () =>
				{
					AssertEquals("EntryHeader Should not change.", string.Empty, entry.CH_EntryStatus);
					AssertEquals("Message should have been set RCV.", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
					AssertEquals("CH_Status should have been set ERR.", LogicalStatusList.Codes.Error, entry.CH_Status);
					MessageProcessorNotificationTestHelper.AssertEmail(
						incomingMessage.MessageTypeWithDescription + " Response (Failure) for B00001000",
						new[] { "A Syntax Error Response message has been received from Customs for Job B00001000 through the IE917 message." },
						new string[] { "staff1@where.com" });
				});
			}
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Not set Entry Status.", string.Empty, entry.CH_EntryStatus);
			AssertEquals("Logical Status", LogicalStatusList.Codes.Error, entry.CH_Status);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response (Failure) for B00001000",
				new[] { "A Syntax Error Response message has been received from Customs for Job B00001000 through the IE917 message." },
				new string[] { "staff1@where.com" });
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001000";
			declaration.JE_GB = Branch.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			entry.Messages.Add(outgoingMessage);
			return (declaration, entry, outgoingMessage, incomingMessage);
		}
	}
}
