using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	[TestedType(typeof(CC917CProcessor))]
	class ExitCC917CProcessorTest : CC917CProcessorTest<CusExitReport, CusExitHeader>
	{
		protected override void AssertProcessResultCore(CusExitReport messageAttachee, AESInboundEDIMessage incomingMessage)
		{
			AssertEquals("Not set Entry Status.", string.Empty, messageAttachee.CER_Status);
			AssertEquals("Logical Status", LogicalStatusList.Codes.Error, messageAttachee.CER_MessageStatus);

			MessageProcessorNotificationTestHelper.AssertEmail(
				incomingMessage.MessageTypeWithDescription + " Response (Failure) for E00000003",
				new[] { "A Syntax Error Response message has been received from Customs for Job E00000003 through the IE917 message." },
				new string[] { "staff1@where.com" });
		}

		protected override (CusExitHeader declaration, CusExitReport messageAttachee, EDIMessage outgoingMessage, AESInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var header = Factory.New<CusExitHeader>();
			header.CXH_JobReference = "E00000003";
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
