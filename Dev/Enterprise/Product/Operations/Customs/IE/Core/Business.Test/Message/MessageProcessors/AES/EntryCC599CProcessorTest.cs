using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC599CProcessor))]
	class EntryCC599CProcessorTest : CC599CProcessorTest<CusEntryHeader, JobDeclaration>
	{
		public void TestProcessMessage_Exported()
		{
			(var _, var entry, var _, var incomingMessage) = CreateSetupData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertProcessResultCore(entry, incomingMessage);
				});
			}
		}

		public void TestProcessMessage_Export_Refused()
		{
			(var _, var entry, var _, var incomingMessage) = CreateRefusedData();
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertEquals("EntryHeader should have been set Refused.", "REF", entry.CH_EntryStatus);
					AssertEquals("Message should have been set PRS.", EDIMessageStatusList.Codes.ProcessedOK, incomingMessage.EM_Status);
					AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
					var jobNumber = entry.Declaration.JE_DeclarationReference;
					var messageInterpretation = @"An Export Notification message has been received from Customs for Job B00001000 through the IE599 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>REF</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Exit Control Code</td><td>B1</td></tr><tr><td>Exit Date</td><td>22-Feb-22</td></tr><tr><td>Exit Stopped Date</td><td>01-Jul-22</td></tr><tr><td>State of Seals</td><td>0</td></tr></table>";
					AssertMessageInterpretation(incomingMessage, messageInterpretation);
					MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
				});
			}
		}

		protected override void AssertProcessResultCore(CusEntryHeader entry, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("EntryHeader should have been set Exported.", "EXP", entry.CH_EntryStatus);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);

			var jobNumber = entry.Declaration.JE_DeclarationReference;
			var messageInterpretation = @"An Export Notification message has been received from Customs for Job B00001000 through the IE599 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>EXP</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Exit Control Code</td><td>A1</td></tr><tr><td>Exit Date</td><td>22-Feb-22</td></tr><tr><td>Exit Stopped Date</td><td>01-Jul-22</td></tr><tr><td>State of Seals</td><td>0</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		(JobDeclaration declaration, CusEntryHeader entry, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateRefusedData()
		{
			var stuff = MessageProcessorNotificationTestHelper.SetupStaffData(Factory);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GB = Branch.PK;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var incomingMessage = CreateRefusedIncomingMessgae();
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = stuff.GS_Code;
			entry.Messages.Add(outgoingMessage);
			Factory.Save();
			return (declaration, entry, outgoingMessage, incomingMessage);
		}

		AESInboundEDIMessage CreateRefusedIncomingMessgae()
		{
			var message = Factory.New<AESInboundEDIMessage>();
			message.EM_ApplicationReference = TransactionID;
			message.EM_MessageType = MessageType;
			message.EM_MessageText = AESInterchangeProcessorTestHelper.GetStandardCC599CMailboxItemText(TransactionID, "B1", includeResponseWrap: false);
			return message;
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
