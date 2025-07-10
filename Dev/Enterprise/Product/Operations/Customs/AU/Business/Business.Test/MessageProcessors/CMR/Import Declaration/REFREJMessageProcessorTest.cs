using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class REFREJMessageProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		public void TestShouldSendAcknowledgementReport()
		{
			AUCustomsDataRegistry.Instance.ThirdPartyRefundRejectionSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.NoEmails);
			Assert(!(new REFREJMessageProcessorForTest(logger).ShouldSendAcknowledgementReportExposed));
		}

		public void TestProcessesUnattachedREFREJMessage()
		{
			var broker1 = CreateStaffMember("broker1", "B1", "Broker 1", "broker1@test1.com");
			var broker2 = CreateStaffMember("broker2", "B2", "Broker 2", "");
			var broker3 = CreateStaffMember("broker3", "B3", "Broker 3", "");
			var postMaster = CreateStaffMember("postmaster", "PM$", "Post Master", "pm@edi.com");

			var notificationGroup = Factory.New<GlbGroup>();
			notificationGroup.GG_Code = "REFREJ";
			GlbGroupLink link1 = Factory.New<GlbGroupLink>();
			link1.GK_GG = notificationGroup.PK;
			link1.GK_GS = broker1.PK;
			GlbGroupLink link2 = Factory.New<GlbGroupLink>();
			link2.GK_GG = notificationGroup.PK;
			link2.GK_GS = postMaster.PK;

			AUCustomsDataRegistry.Instance.ThirdPartyRefundRejectionSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMemberAndNominatedGroup);
			AUCustomsDataRegistry.Instance.ThirdPartyRefundRejectionSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, notificationGroup.PK.ToGuid());

			Factory.Save();
			AssertEquals("Pre-condition", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var incomingMessage = Factory.New<CMRREFREJMessage>();
			incomingMessage.EM_MessageText = unattachedREFREJResponse;
			incomingMessage.EM_MessageSubType = "";

			var processor = (REFREJMessageProcessor)GetMessageProcessor();
			processor.ProcessMessage(incomingMessage);
			AssertEquals("Message sub type should be REJ", "REJ", incomingMessage.EM_MessageSubType);

			var expectedEmailSubject = @"Refund Rejected for Unknown Entry (20000200/1)";
			var expectedEmailContent1 = @"A response message has been received from Customs which does not match an operational customs declaration.";
			var expectedEmailContent2 = @"Shown below is a summary of relevant information received in the message.";
			var expectedEmailContent3 = @"Reference Number:";
			var expectedEmailContent4 = @"Declaration Reference:";
			var expectedEmailContent5 = @"Client Reference:";
			var expectedEmailContent6 = @"Document Reference:";
			var expectedEmailContent7 = @"Document Version:";
			var expectedEmailContent8 = @"Rejection Date:";
			var expectedEmailContent9 = @"Status:";
			var expectedEmailContent10 = @"Errors:";
			var expectedEmailContent11 = @"CERTIFICATE LODGED IS NOT AN AANZ CERTIFICATE AND SIGNATORY NOT RECORDED. REJECT. ERRONEOUS CLAIM (NOT PRESCRIBED).THIS COVERS ALL OTHER REJECTIONS THAT CANNOT BE MORE APPROPRIATELY COVERED ELSEWHERE, EG A PILLAGE OR DAMAGE CLAIM UNSUBSTANTIATED.";
			Factory.Save();
			AssertEquals("1 email has been sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email Subject line", expectedEmailSubject, email.Subject);
			AssertContains(expectedEmailContent1, email.Body);
			AssertContains(expectedEmailContent2, email.Body);
			AssertContains(expectedEmailContent3, email.Body);
			AssertContains(expectedEmailContent4, email.Body);
			AssertContains(expectedEmailContent5, email.Body);
			AssertContains(expectedEmailContent6, email.Body);
			AssertContains(expectedEmailContent7, email.Body);
			AssertContains(expectedEmailContent8, email.Body);
			AssertContains(expectedEmailContent9, email.Body);
			AssertContains(expectedEmailContent10, email.Body);
			AssertContains(expectedEmailContent11, email.Body);
			AssertEquals("Should have been 2 recipients added to this notification", 2, email.Recipients.Count);
			Assert("Group user to be notified", email.Recipients.Contains("broker1@test1.com"));
			Assert("Group user to be notified", email.Recipients.Contains("pm@edi.com"));
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.REFREJ;

		protected override ZString GetExpectedMessageName() => "Refund Rejected Response (REFREJ)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new REFREJMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRREFREJMessage);

		readonly string unattachedREFREJResponse = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::REFREJ+4JG2 19JJ G94:1+11'
DTM+9:20140905:102'
FTX+ACB+++MXINV13001868'
FTX+ACD+++CERTIFICATE LODGED IS NOT AN AANZ CERTIFICATE AND SIGNATORY NOT RECORDED. REJECT.:ERRONEOUS CLAIM (NOT PRESCRIBED).THIS COVERS ALL OTHER REJECTIONS THAT CANNOT BE MORE APPROPRIATELY COVERED ELSEWHERE, EG A PILLAGE OR DAMAGE CLAIM UNSUBSTANTIATED.'
NAD+MR+FFF999P::95'
NAD+VT+AE93YF::95'
NAD+CB++BROKER PTY LIMITED'
RFF+ABO:20000200/1::2'
RFF+ABT:ACXCLRJHF::2'
RFF+ADU:20000200/1'
UNT+12+000001'".Replace("\r\n", "");

		GlbStaff CreateStaffMember(ZString loginName, ZString code, ZString fullName, ZString eMail)
		{
			var result = Factory.New<GlbStaff>();
			result.GS_LoginName = loginName;
			result.GS_Code = code;
			result.GS_FullName = fullName;
			result.GS_EmailAddress = eMail;
			return result;
		}
	}
}
