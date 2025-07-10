using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC599CProcessor))]
	class ExitCC599CProcessorTest : CC599CProcessorTest<CusExitReport, CusExitHeader>
	{
		protected override void AssertProcessResultCore(CusExitReport messageAttachee, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("EntryHeader should have been set Exported.", "EXP", messageAttachee.CER_Status);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CER_MessageStatus);

			var jobNumber = messageAttachee.Header.CXH_JobReference;
			var messageInterpretation = $@"An Export Notification message has been received from Customs for Job {jobNumber} through the IE599 message.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Status</td><td>EXP</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>MRN</td><td>21IEDUB11A782454R2</td></tr><tr><td>Exit Control Code</td><td>A1</td></tr><tr><td>Exit Date</td><td>22-Feb-22</td></tr><tr><td>Exit Stopped Date</td><td>01-Jul-22</td></tr><tr><td>State of Seals</td><td>0</td></tr></table>";
			AssertMessageInterpretation(incomingMessage, messageInterpretation);
			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for " + jobNumber, new[] { messageInterpretation }, new string[] { "staff1@where.com" });
		}

		protected override (CusExitHeader declaration, CusExitReport messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var header = Factory.New<CusExitHeader>();
			header.CXH_JobReference = "E00000004";
			header.CXH_GB_Branch = Branch.PK;
			var report = header.CusExitReports.AddNew();

			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			var outgoingMessage = Factory.New<AESOutboundEDIMessage>();
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_GB = Branch.PK;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			report.Messages.Add(outgoingMessage);
			return (header, report, outgoingMessage, incomingMessage);
		}
	}
}
