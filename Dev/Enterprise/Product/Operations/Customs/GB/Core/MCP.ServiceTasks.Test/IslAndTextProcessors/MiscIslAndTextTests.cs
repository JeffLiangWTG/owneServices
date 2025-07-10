using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Testing;
using Enterprise.Customs.GB.MCP.ServiceTasks.Misc;
using Enterprise.Customs.GB.MCP.ServiceTasks.PHS11.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.IslAndTextProcessors.Testing
{
	[TestedType(typeof(MiscTextAndIslServiceTask))]
	class MiscIslAndTextTests : ServiceTaskTestCase<MiscTextAndIslServiceTask>
	{
		public void TestLUMProcessing()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DJC";
			staff.GS_EmailAddress = "foo@bar.com";
			staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			Factory.Save();

			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "Destin8PRD@destin8.co.uk";
			mailItem.MI_Subject = "LUM01 FEY-FEYM [TXT] Felixstowe weather";
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii(@"LUM01   Destin8         Local Unsolicited Message           31/03/2010  02:55

Reference: Felixstowe weather

Felixstowe Landside Weather update.

Container Yard Operations

Please be advised wind speeds recorded at the Port have exceeded operational 
limits. As a result both Rail and Yard Operations have been affected. For 
the safety of all port users we are currently restricting access to 
operational areas. Vehicles are being marshalled on site and will be 
processed through to operational areas as soon as it is safe to do so.

We will monitor the situation and send a further message when operations 
resume or if an updated forecast is received.

Due to the above some delays to hauliers are likely and customers may wish 
to consider re-scheduling their arrivals.  



This information and updates are available via the Haulier Information Line 
on 01394 604060 (recorded message).


From: SOP - Destin8 System Operator

***LUM01 - End
 
");
			attachment.MA_FileName = "whateverTheWeather.txt";
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbMcpMiscTextAndEmails);

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MasterUCR = "341560045";  // irrelevant
			declaration.JE_DeclarationReference = "B000069";
			CusEntryHeader cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			EDIMessage outboundMessage = mockMessage.Object;
			cusEntryHeader.Messages.Add(outboundMessage);
			cusEntryHeader.EntryNumber = "071-001006L";
			outboundMessage.EM_SystemCreateUser = "DJC";
			outboundMessage.EM_MessageText = " I hate this silly insistence on message number <<MSGNO PLACEHOLDER>>";
			outboundMessage.EM_ReceiveTransmit = "TRX";

			declaration.JE_UCR = "348130347";
			cusEntryHeader.CH_BGMReference = "BGMREF";
			cusEntryHeader.CH_EntrySubmittedDate = ZDateTime.Now.AddMinutes(-60);
			cusEntryHeader.CH_MessageType = ApplicationCodeList.Codes.GbMcpRra01AndRra11;
			Factory.Save();
			AssertEquals("Pre-req: message should have been sent by DJC, not by a built-in account", "DJC", outboundMessage.UserWhoQueuedThisRecord.GS_Code);

			var serviceTask = new MiscTextAndIslServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);

			mailItem.Reload();
			AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);

			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			AssertEquals("Subject", "Miscellaneous advice from MCP", emails[0].Subject);
			AssertContains("<h3>Miscellaneous advice from MCP.</h3> <pre>LUM01   Destin8         Local Unsolicited Message", emails[0].Body);
			AssertEquals("foo@bar.com", emails[0].Recipients[0]);
			mockMessage.VerifyAll();
		}

		void RunTestCSNAndCAUProcessing(string ucn, string fullSubject, string attachmentDataConent)
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DJC";
			staff.GS_EmailAddress = "foo@bar.com";
			staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			Factory.Save();

			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "Destin8PRD@destin8.co.uk";
			mailItem.MI_Subject = fullSubject;
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			var attachment = mailItem.MailAttachments.AddNew();

			attachment.MA_Data = ZBlob.FromAscii(attachmentDataConent);
			attachment.MA_FileName = "whatever.txt";
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbMcpMiscTextAndEmails);

			JobDeclaration declaration = Factory.New<JobDeclaration>();

			declaration.JE_MessageType = "IMP";
			declaration.JE_MasterUCR = ucn;
			declaration.JE_DeclarationReference = "B000069";

			cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
			mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
			EDIMessage outboundMessage = mockMessage.Object;
			cusEntryHeader.Messages.Add(outboundMessage);
			cusEntryHeader.EntryNumber = "071-001006L";
			outboundMessage.EM_SystemCreateUser = "DJC";
			outboundMessage.EM_MessageText = " I hate this silly insistence on message number <<MSGNO PLACEHOLDER>>";
			outboundMessage.EM_ReceiveTransmit = "TRX";

			declaration.JE_UCR = "348130347";
			cusEntryHeader.CH_BGMReference = "BGMREF";
			cusEntryHeader.CH_EntrySubmittedDate = ZDateTime.Now.AddMinutes(-60);
			cusEntryHeader.CH_MessageType = ApplicationCodeList.Codes.GbMcpRra01AndRra11;
			Factory.Save();
			AssertEquals("Pre-req: message should have been sent by DJC, not by a built-in account", "DJC", outboundMessage.UserWhoQueuedThisRecord.GS_Code);
			mockMessage.VerifyAll();
		}

		public void TestCSNTextProcessing_GoodJob()
		{
			RunTestCSNAndCAUProcessing("541601658", "CSN01 FEY-FEYM [TXT] Import Self Nomination Notification = 541601658", csnMessage);
			var serviceTask = new MiscTextAndIslServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);
			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			AssertEquals("Subject", "Self-nomination advice for  B000069 / 071-001006L", emails[0].Subject);
			AssertContains("UVI: 54160   Vessel: Elly Maersk", emails[0].Body);
			var expectedEmailAddresses = new string[] { "foo@bar.com" };
			AssertContainsExactElementsInAnyOrder("Email recipients list", expectedEmailAddresses, emails[0].Recipients.RecipientsAsDelimitedString(",").Split(','));
			cusEntryHeader.Reload();

			// The service task takes a peek inside the Messages collection during processing, which caches the collection.  We need to load it to refresh the cached collection otherwise the following assertion fails.
			cusEntryHeader.Messages.Load();
			EDIMessage inboundEdiMessageThatWeMade = cusEntryHeader.Messages.LastIncomingMessage;
			AssertContains("UVI: 54160   Vessel: Elly Maersk", inboundEdiMessageThatWeMade.EM_MessageInterpretation);
			AssertEquals(EDIMessage.Status.Received, inboundEdiMessageThatWeMade.EM_Status);
			AssertEquals(EDIMessage.Status.Received, inboundEdiMessageThatWeMade.Interchange.EI_Status);
			AssertEquals(2, cusEntryHeader.Messages.Count);
		}

		public void TestCSNTextProcessing_GoodJobNotificationsGoToRecipientForSpecificBranch()
		{
			var dunstableBranch = CuscarInboundParserTests.GetDunstableBranchPkForTest(Factory);
			string notificationEmailAddress = null;

			using (DisposableEnvironment.ForBranch(dunstableBranch.PK.ToGuid()))
			{
				notificationEmailAddress = CuscarInboundParserTests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationMcpRra12, dunstableBranch.PK.ToGuid(), Factory);
				RunTestCSNAndCAUProcessing("541601658", "CSN01 FEY-FEYM [TXT] Import Self Nomination Notification = 541601658", csnMessage);
			}

			var serviceTask = new MiscTextAndIslServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);
			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			var expectedEmailAddresses = new string[] { "foo@bar.com", notificationEmailAddress };
			AssertContainsExactElementsInAnyOrder("Email recipients list for branch", expectedEmailAddresses, emails[0].Recipients.RecipientsAsDelimitedString(",").Split(','));
		}

		public void TestCSNTextProcessing_BadJob()
		{
			var notificationEmailAddress = Phs11Tests.SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationMcpCsn, "CSN", Guid.Empty, Factory);
			RunTestCSNAndCAUProcessing("XXXXXXX", "CSN01 FEY-FEYM [TXT] Import Self Nomination Notification = 541601658", csnMessage);
			var serviceTask = new MiscTextAndIslServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);
			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			AssertEquals("Subject", "Self-nomination advice from MCP", emails[0].Subject);
			AssertContains("UVI: 54160   Vessel: Elly Maersk", emails[0].Body);
			AssertEquals("Self-nomination advice from MCP", notificationEmailAddress, emails[0].Recipients[0].Email);

			cusEntryHeader.Reload();

			// The service task takes a peek inside the Messages collection during processing, which caches the collection.  We need to load it to refresh the cached collection otherwise the following assertion fails.
			cusEntryHeader.Messages.Load();
			AssertEquals(1, cusEntryHeader.Messages.Count);
		}

		const string csnMessage = @"CSN01   Destin8  Import Self Nomination Notification         31/03/10 10:51


-----------------------------------------------------------------------------
UVI: 54160   Vessel: Elly Maersk                   

Estimated Arrival: 03/04/10 19:00

Estimated Departure: 05/04/10 19:00

Berth: TTY - Trinity Terminal

	   Cargo Broker: MSM - Maersk Line

	   Nominated Agent: FEY - Elite Group Logistics  Ltd
-----------------------------------------------------------------------------

UCN     Unit-id      Pkgs  Weight  B/Lading   POO

1658    TCKU9181385  1574  15338   860026639  CNYTN 


*** CSN01 - End                                                     Page - 1
 ";

		const string csnIslMessage = "=CSN01~78121000100000~FAJ~KKK~TEST1234567 ~MESSAGE TE~1609140917~CSN~GARYS               ~MESSAGE TESTER                     ~                    ~PHILIPP                            ~1609141400~ ~N~N~N~N~N~N~N~N~TTY~1100 ~42000~ESACE~4510~N}";

		public void TestCsnIslProcessing()
		{
			RunTestCSNAndCAUProcessing("781210001", "CSN01 FAJ-MCP0 [ISL] IMPORT SELF NOMINATION NOTIFICATION = 781210001", csnIslMessage);
			var serviceTask = new MiscTextAndIslServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);
			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			AssertEquals("Subject", "Self-nomination advice for  B000069 / 071-001006L", emails[0].Subject);
			AssertContains("<td>New Nominated Agent</td><td>FAJ</td></tr><tr><td>Cargo Broker</td><td>KKK</td>", emails[0].Body);
			var expectedEmailAddresses = new string[] { "foo@bar.com" };
			AssertContainsExactElementsInAnyOrder("Email recipients list", expectedEmailAddresses, emails[0].Recipients.RecipientsAsDelimitedString(",").Split(','));
			cusEntryHeader.Reload();
		}

		const string cauIslMessage = "=CAU01~781210001     ~FVH~FAJ~TEST1234567 ~MESSAGE TE~1609140921~CAU~GARYS               ~MESSAGE TESTER                     ~4510~42000~TTY~N}";

		public void TestCauIslProcessing()
		{
			RunTestCSNAndCAUProcessing("781210001", "CAU01 FVH-MCP0 [ISL] NOMINATED AGENT RE-NOMINATION = 781210001", cauIslMessage);
			var serviceTask = new MiscTextAndIslServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);
			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			AssertEquals("Subject", "Re-nomination advice for  B000069 / 071-001006L", emails[0].Subject);
			AssertContains("<tr><td>New Nominated Agent</td><td>FVH</td></tr><tr><td>Old Nominated Agent</td><td>FAJ</td>", emails[0].Body);
			var expectedEmailAddresses = new string[] { "foo@bar.com" };
			AssertContainsExactElementsInAnyOrder("Email recipients list", expectedEmailAddresses, emails[0].Recipients.RecipientsAsDelimitedString(",").Split(','));
			cusEntryHeader.Reload();
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			branchEnvironment = Enterprise.Environment.DisposableEnvironment.ForBranch(Enterprise.Customs.GB.Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDownCore()
		{
			base.TearDownCore();
			branchEnvironment.Dispose();
		}

		CusEntryHeader cusEntryHeader;
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						"Destin8 Misc mail inbound",
						MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbMcpMiscTextAndEmails,
						MailDBItemsSchema.Constants.MI_Status + "=" + StatusCodeList.Codes.Queued,
						MailDBItemsSchema.Constants.MI_Direction + "=" + DirectionList.Codes.Receive),
				};
			}
		}
	}
}
