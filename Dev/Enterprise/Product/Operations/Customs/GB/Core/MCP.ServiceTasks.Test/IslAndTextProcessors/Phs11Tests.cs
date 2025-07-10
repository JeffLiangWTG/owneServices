using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.PHS11;
using Enterprise.Customs.GB.Registry;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Integration;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.PHS11.Testing
{
	[TestedType(typeof(PHS11ServiceTask))]
	class Phs11Tests : ServiceTaskTestCase<PHS11ServiceTask>
	{
		public void TestSeveralAttachmentsPerEmailIncludingDuffAttachment()
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "Destin8PRD@destin8.co.uk";
			mailItem.MI_Subject = "PHS11 MDT-AJBX [TXT] PORT HEALTH STATUS NOTIFICATION UCN = 227550004, UNITID = BBBB4444444";
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			var attachment1Good = mailItem.MailAttachments.AddNew();
			attachment1Good.MA_Data = ZBlob.FromAscii("=PHS11~1210180930~POF~127550004000~BBBB4444444x~lorry 001~E~X1~LNG~JOHN GOOD SHIPPING marks daniel here 123~KKE~NGBF154846~agents reference djc~00225~00003405~FORKS, PLATE, CUP, NAPKIN,spoon,bib,ribs~PHS~DANIELSxxxReleasedxy~NGBF1548460000000000000000000000001}");
			attachment1Good.MA_FileName = "whatever.txt";
			var attachment2Good = mailItem.MailAttachments.AddNew();
			attachment2Good.MA_Data = ZBlob.FromAscii("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }");
			attachment2Good.MA_FileName = "whatever2.txt";
			var attachmentBad = mailItem.MailAttachments.AddNew();
			attachmentBad.MA_Data = ZBlob.FromAscii("Duffman says oh yeah");
			attachmentBad.MA_FileName = "image.txt";
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbMcpPHS);
			Factory.Save();
			var logger = new TestServiceLogger();
			var pHS11EmailsToMessagesPoller = new PHS11EmailsToEdiMessagesPoller(logger);
			pHS11EmailsToMessagesPoller.ExecuteBatch();
			var interchanges = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("Should only have created 1 interchange", 1, interchanges.Length);
			var messages = interchanges[0].ContainedMessages;
			AssertEquals("Should have 2 messages inside (one was duff)", 2, messages.Count);
			messages.Sort(EDIMessageSchema.EM_MessageText.Name);
			AssertStartsWith("Message 1", "=PHS11~1210180930~POF~127550004000", messages[0].EM_MessageText);
			AssertStartsWith("Message 2", "=PHS11~1210180930~POF~227550004000", messages[1].EM_MessageText);
		}

		public void TestPHS11Processing_SimpleVerbatimMucrAndEntryMatch()
		{
			SetUp("227550004000", true);
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
							, additonalExpectToFindData: new string[] { "2GB123456789000-B0001000/69" }
							, expectedEmailRecipients: new string[] { "on.job@client.com", "in.group@client.com" });
			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			Assert("Should have 'hold' log", (from StmALog l in declaration.Logs.GetAllLogs() where l.SL_Reference == "PortHealth-R" select l).Any());
			AssertNotEquals("R status means NOT held", EntryStatusList.Codes.Hold, declaration.CustomsEntryHeaders[0].CH_EntryStatus);
			AssertEquals("RCV", declaration.CustomsEntryHeaders[0].Messages[0].Interchange.EI_Status);
		}

		public void TestPHS11Processing_DeliveryGroup()
		{
			GBCustomsDataRegistry.Instance.CustomsResponseNotificationsItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Core.Constants.EmailTo.StaffMember);
			SetUp("227550004000", true);
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
							, additonalExpectToFindData: new string[] { "2GB123456789000-B0001000/69" }
							, expectedEmailRecipients: new string[] { "on.job@client.com" }); // staff only
		}

		public void TestPHS11Processing_SimpleVerbatimMucrAndEntryMatch_MaxSizedMessage()
		{
			// Checks full-length of all elements comes out in print
			SetUp("227550004000", true);
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444x~lorry 001~E~X1~LNG~JOHN GOOD SHIPPING marks daniel here 123~KKE~NGBF154846~agents reference djc~00225~00003405~FORKS, PLATE, CUP, NAPKIN,spoon,bib,ribs~PHS~DANIELSxxxReleasedxy~NGBF1548460000000000000000000000001}"
							, additonalExpectToFindData: new string[] {
																		"2GB123456789000-B0001000/69",
																		"BBBB4444444x",
																		"lorry 001",
																		"Shed examination non-animal origin (SXN)",
																		"JOHN GOOD SHIPPING marks daniel here 123",
																		"NGBF154846",
																		"agents reference djc",
																		"FORKS, PLATE, CUP, NAPKIN,spoon,bib,ribs",
																		"DANIELSxxxReleasedxy",
																		"NGBF1548460000000000000000000000001"
																		}
							, expectedEmailRecipients: new string[] { "on.job@client.com", "in.group@client.com" });

			declaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			Assert("Should have 'hold' log", (from StmALog l in declaration.Logs.GetAllLogs() where l.SL_Reference == "PortHealth-E" select l).Any());
			AssertEquals("Non-R status means held", EntryStatusList.Codes.Hold, declaration.CustomsEntryHeaders[0].CH_EntryStatus);
		}

		public void TestPHS11Processing_MucrWithTrailingZeroes()
		{
			SetUp("227550004000", true);
			GBCustomsDataRegistry.Instance.AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Factory.Save();
			var messageContent = "=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }";
			PrepareMailItemAndRunTask(messageContent);
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertNull(printJob);  // unsuccessful when rego forbids truncating
			GBCustomsDataRegistry.Instance.AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			RunPhs11Test(messageContent
								, additonalExpectToFindData: new string[] { "227550004000", "2GB123456789000-B0001000/69" }
								, expectedEmailRecipients: new string[] { "on.job@client.com", "in.group@client.com" }
								, expectedEmailCount: 2);
		}

		public void TestPHS11Processing_MucrWithoutTrailingZeroes()
		{
			SetUp("227550004", true);
			Factory.Save();
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
								, additonalExpectToFindData: new string[] { "MUCR 227550004", "2GB123456789000-B0001000/69" }
								, expectedEmailRecipients: new string[] { "on.job@client.com", "in.group@client.com" });
		}

		public void TestPHS11Processing_MatchUsingHeldUcnFromRra12()
		{
			SetUp("ParentUcnWhatever", true);
			var entry = declaration.CustomsEntryHeaders[0];
			var heldUcnInThisMessage = entry.MaritimeUcnsThatAreHeld.AddNew();
			heldUcnInThisMessage.Data.NW_UCN = "227550004000";
			Factory.Save();
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
								, additonalExpectToFindData: new string[] { "MUCR ParentUcnWhatever", "2GB123456789000-B0001000/69" }
								, expectedEmailRecipients: new string[] { "on.job@client.com", "in.group@client.com" });
		}

		public void TestPHS11Processing_MatchUsingAgentReferenceOverridenByUser()
		{
			SetUp("Irrelevant", true);
			declaration.JE_OwnerRef = "ImAgentsReferenceBox7";
			Factory.Save();
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~ImAgentsReferenceBox7~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
							, additonalExpectToFindData: new string[] { "ImAgentsReferenceBox7", "2GB123456789000-B0001000/69" }
							, expectedEmailRecipients: new string[] { "on.job@client.com", "in.group@client.com" });
		}

		public void TestPHS11Processing_MatchUsingAgentReferenceFallbackFromJobNumber()
		{
			SetUp("Irrelevant", true);
			declaration.JE_OwnerRef = "";  // field labelled "[7] Declarant's reference" on form
			declaration.JE_DeclarationReference = "B00009999";      // the value we send to CHIEF in TRDR-OWN-REF-ENT (box 7) when none given in JE_OwnerRef.... and which is returned in the message
			Factory.Save();
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~B00009999            ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
							, additonalExpectToFindData: new string[] { "B00009999", "2GB123456789000-B0001000/69" }
							, expectedEmailRecipients: new string[] { "on.job@client.com", "in.group@client.com" });
		}

		public void TestPHS11Processing_EntrylessDeclaration()
		{
			SetUp("227550004000", false);
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
						, additonalExpectToFindData: new string[] { "B00009999" }
						, expectedEmailRecipients: new string[] { "on.job@client.com", "in.group@client.com" });
		}

		public void TestPHS11Processing_DeclarationWithoutBrokerInitialsButHasOutboundMessage()
		{
			var emailAddress = SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationMcpPhs11, "PHS11", GlbBranch.CurrentBranch.PK.ToGuid(), Factory);
			SetUp("227550004000", true);
			declaration.JE_GS_NKCusAgent = "";
			var entry = declaration.CustomsEntryHeaders[0];
			var mocker = Factory.NewMoq<GbEDIMessageDummyForTest>();
			mocker.Protected().Setup<string>("GetMessageReferenceNumber").Returns("msgNum");
			var outboundMessage = mocker.Object;
			entry.Messages.Add(outboundMessage);
			outboundMessage.EM_ReceiveTransmit = "TRX";
			outboundMessage.EM_MessageText = GbEDIMessage.MessageNumberPlaceHolder;
			outboundMessage.EM_SystemCreateUser = "DJC";
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
							, additonalExpectToFindData: new string[] { "2GB123456789000-B0001000/69" }
							, expectedEmailRecipients: new string[] { "on.job@client.com", emailAddress });
			mocker.VerifyAll();
		}

		public void TestPHS11Processing_DeclarationWithoutBrokerInitialsOrMessage()
		{
			var emailAddress = SetUpNotificationGroup(GBCustomsDataRegistry.Instance.NotificationMcpPhs11, "PHS11", GlbBranch.CurrentBranch.PK.ToGuid(), Factory);
			SetUp("227550004000", true);
			declaration.JE_GS_NKCusAgent = "";
			RunPhs11Test("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~N/A                  ~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }"
							, additonalExpectToFindData: new string[] { "2GB123456789000-B0001000/69" }
							, expectedEmailRecipients: new string[] { emailAddress });  // group only
		}

		static internal ZString SetUpNotificationGroup(IRegistryItem notificationRegoItem, ZString dtiCode, Guid branchPk, BusinessObjectFactory factory)
		{
			var emailAddress = dtiCode + "notifications@zzz.com";
			var group = factory.New<GlbGroup>();
			group.GG_Code = dtiCode;
			group.GG_Desc = dtiCode + " Group";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "ZZZ";
			staff.GS_LoginName = "ZZZ";
			staff.GS_EmailAddress = emailAddress;
			var companyPk = Guid.Empty;
			if (branchPk == Guid.Empty)
			{
				companyPk = GlbCompany.CurrentCompany.PK.ToGuid();
			}
			notificationRegoItem.SetValue(companyPk, branchPk, Guid.Empty, group.PK.ToGuid());
			factory.Save();
			return emailAddress;
		}

		public void TestPHS11Processing_FailTooManyHitsOnAgentsReference()
		{
			SetUp("Irrelevant", true);
			declaration.JE_OwnerRef = "ImAgentsReferenceBox7";
			var anotherDeclaration = Factory.New<JobDeclaration>();
			anotherDeclaration.JE_OwnerRef = "ImAgentsReferenceBox7";
			anotherDeclaration.JE_MessageType = "IMP";
			anotherDeclaration.JE_GS_NKCusAgent = "DJC";
			Factory.Save();
			RunPhs11Test_Failure("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~ImAgentsReferenceBox7~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }");
		}

		public void TestPHS11Processing_FailNoMatch()
		{
			SetUp("No match", false);
			RunPhs11Test_Failure("=PHS11~1210180930~POF~227550004000~BBBB4444444 ~         ~R~X1~TTY~                                        ~MDT~85        ~Anything~00010~00010000~FROG4                                   ~PHS~ARLENEB             ~85                                 }");
		}

		void RunPhs11Test(string messageContent, string[] additonalExpectToFindData, string[] expectedEmailRecipients, int expectedEmailCount = 1)
		{
			PrepareMailItemAndRunTask(messageContent);
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			var expectToFindData = new List<string> { "BBBB4444444", "PHS11", "Released", "18-Oct-12 09:30" };
			expectToFindData.AddRange(additonalExpectToFindData);
			ExcelContentTest.AssertPrintJobContainsAndNotContainsText(expectToFindData.ToArray(), new string[] { "=PHS11", "NGBF1548460000000000000000000000001}" }, printJob);
			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(expectedEmailCount, emails.Count);
			var email = emails[expectedEmailCount - 1];
			AssertContains("Subject", "Destin8 PHS11 port health status update for UCN 227550004000, declaration B00009999", email.Subject);
			AssertContains("A port health status update message (PHS11) was received from Destin8", email.Body);
			AssertContains("227550004000", email.Body);
			AssertContainsExactElementsInAnyOrder("Email receipients list", expectedEmailRecipients, email.Recipients.RecipientsAsDelimitedString(",").Split(','));
		}

		void RunPhs11Test_Failure(string messageContent)
		{
			PrepareMailItemAndRunTask(messageContent);
			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery());
			AssertNull(printJob);
			if (declaration != null)
			{
				if (declaration.CustomsEntryHeaders.Count != 0)
				{
					AssertEquals(0, declaration.CustomsEntryHeaders[0].Messages.Count);
				}
			}
			var emails = Enterprise.Environment.Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals(1, emails.Count);
			AssertContains("Subject", "Destin8 PHS11 port health status update for UCN 227550004000 could not be matched to a job", emails[0].Subject);
			AssertContains("but no corresponding job could be found in the Broker Software database", emails[0].Body);
			AssertEquals("Failed message should be attached to email", ZBlob.FromAscii(messageContent), emails[0].Attachments[2].Data);
			AssertContains("Released (REL)", emails[0].Body);
			AssertContains("in.group@client.com", emails[0].Recipients.RecipientsAsDelimitedString());
		}

		void PrepareMailItemAndRunTask(string messageContent)
		{
			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "Destin8PRD@destin8.co.uk";
			mailItem.MI_Subject = "PHS11 MDT-AJBX [TXT] PORT HEALTH STATUS NOTIFICATION UCN = 227550004, UNITID = BBBB4444444";
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii(messageContent);
			attachment.MA_FileName = "whatever.txt";
			var filter = new PHS11EmailsToEdiMessagesPoller(null).IncomingInterchangeMailFilter;
			if (filter.CanProcess(mailItem))
			{
				mailItem.MI_Application = filter.Code;
			}
			Factory.Save();
			var serviceTask = new PHS11ServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);
			mailItem.Reload();
			AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);
		}

		void SetUp(string mucrForDeclaration, bool alsoMakeEntry)
		{
			var printer = Factory.New<StmPrintQueue>();
			GBCustomsDataRegistry.Instance.PrinterMcpNonChief.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, printer.PK.ToGuid());
			var staffInGroupButNotOnJob = Factory.New<GlbStaff>();
			staffInGroupButNotOnJob.GS_Code = "XXX";
			staffInGroupButNotOnJob.GS_EmailAddress = "in.group@client.com";
			staffInGroupButNotOnJob.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			staffInGroupButNotOnJob.GS_LoginName = "XXX";
			var staffOnJobButNotInGroup = Factory.New<GlbStaff>();
			staffOnJobButNotInGroup.GS_Code = "DJC";
			staffOnJobButNotInGroup.GS_EmailAddress = "on.job@client.com";
			staffOnJobButNotInGroup.GS_LoginName = "DJC";
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_MasterUCR = mucrForDeclaration;
			declaration.JE_DeclarationReference = "B00009999";
			declaration.JE_GS_NKCusAgent = "DJC";
			if (alsoMakeEntry)
			{
				var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				cusEntryHeader.CH_BGMReference = "2GB123456789000-B0001000/69";
			}

			GBCustomsDataRegistry.Instance.AllowMatchingOfInboundUcnToJobsMucrVerbatimWithoutTruncating.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
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

		JobDeclaration declaration;

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						"Destin8 Port Health Status mail inbound",
						MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbMcpPHS,
						MailDBItemsSchema.Constants.MI_Status + "=" + StatusCodeList.Codes.Queued,
						MailDBItemsSchema.Constants.MI_Direction + "=" + DirectionList.Codes.Receive),
				};
			}
		}
	}

	public class GbEDIMessageDummyForTest : GbEDIMessage
	{
		public GbEDIMessageDummyForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "msgNum";
		}
	}
}
