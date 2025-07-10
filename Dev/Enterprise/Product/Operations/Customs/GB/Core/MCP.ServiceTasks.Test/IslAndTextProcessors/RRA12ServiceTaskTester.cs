using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.MCP.RRA12;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MailManager.MailFilters;
using Enterprise.MailManager.MailFilters.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using MailManager;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.GB.MCP.ServiceTasks.RRA12.Testing
{
	[TestedType(typeof(RRA12ServiceTask))]
	public class RRA12ServiceTaskTester : ServiceTaskTestCase<RRA12ServiceTask>
	{
		[TestDate(2012, 05, 03)]
		public void TestRra12ProcessorSelectsCorrectLogoForEmail()
		{
			// Setup has already run - we are currently in DUK (Heathrow) branch

			var branchDC = Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory, "ZDC");
			var branchLC = Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory, "ZLC");

			// Create logos for branches
			var lhrImage = CreateImageAndTagIt("LHR");
			var zdcImage = CreateImageAndTagIt("ZDC");
			var zlcImage = CreateImageAndTagIt("ZLC");
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, lhrImage);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, branchDC.PK.ToGuid(), Guid.Empty, zdcImage);
			SystemDataRegistry.Instance.HtmlEmailBannerImage.SetValue(Guid.Empty, branchLC.PK.ToGuid(), Guid.Empty, zlcImage);

			var rra12String = "=RRA12~1205071310~WXR~84430~1205071255~WXR~S00070282                          ~065000985L030512                   ~Y~84430039400000~GVCU5016120 ~1492006921~00890~ ~N~NNNNYNNNNNN~84430042000000~HMCU9094757 ~1492006921~00890~ ~Y~NNNNNNNNNNN~84430045400000~TRLU7298888 ~1492006921~00890~ ~Y~NNNNNNNNNNN~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~INV~MDB                 ~149200692118                       ~149200692118                       ~149200692118                       }";
			SetUpShared(rra12String, "RRA12 ISL.txt", "065-000985L");
			declaration.JE_GB = branchLC.PK;
			declaration.Factory.Save();

			var serviceTask = new RRA12ServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			var email = Environment.Env.OutgoingCustomsMailManager.EmailsCreated[0];
			var banner = email.Attachments[0];
			var recreatedImage = Image.FromStream(new MemoryStream(banner.Data));
			var pi = recreatedImage.GetPropertyItem(20753);
			var branchCodeExtractedFromImage = Encoding.UTF8.GetString(pi.Value);
			AssertEquals("ZLC", branchCodeExtractedFromImage);

			lhrImage.Dispose();
			zdcImage.Dispose();
			zlcImage.Dispose();
		}

		[TestDate(2012, 05, 03)]
		public void TestRRA12ProcessingStoresHeldChildUcns()
		{
			var rra12String = "=RRA12~1205071310~WXR~84430~1205071255~WXR~S00070282                          ~065000985L030512                   ~Y~84430039400000~GVCU5016120 ~1492006921~00890~ ~N~NNNNYNNNNNN~84430042000000~HMCU9094757 ~1492006921~00890~ ~Y~NNNNNNNNNNN~84430045400000~TRLU7298888 ~1492006921~00890~ ~Y~NNNNNNNNNNN~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~INV~MDB                 ~149200692118                       ~149200692118                       ~149200692118                       }";
			SetUpShared(rra12String, "RRA12 ISL.txt", "065-000985L");
			var oldHeldUcn = cusEntryHeader.MaritimeUcnsThatAreHeld.AddNew();
			oldHeldUcn.Data.NW_UCN = "Old UCN";
			var serviceTask = new RRA12ServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			var entryReloaded = new BusinessObjectFactory().Load<CusEntryHeader>(cusEntryHeader.PK);
			AssertEquals("Old UCN removed, new one added", 1, entryReloaded.MaritimeUcnsThatAreHeld.Count);
			AssertEquals("Old UCN removed, new one added", "84430039400000", entryReloaded.MaritimeUcnsThatAreHeld[0].Data.NW_UCN);
			AssertEquals("Scanner Hold Applied", entryReloaded.MaritimeUcnsThatAreHeld[0].Data.NW_HoldTypeComments);
			AssertEquals("HLD", entryReloaded.CH_EntryStatus);
		}

		public void TestRRA12ProcessingForCDS()
		{
			var rra12String = "=RRA12~2305170947~IWZ~79683~2305170535~IWZ~3GB249915424000-B00319420          ~23GB5EUDD4K6WTEAR6                 ~Y~79683004000000~FBIU0413785 ~MEDUTO8563~00001~ ~Y~NNNNNNNNNNN~79683004100000~FCIU4309488 ~MEDUTO8563~00001~ ~Y~NNNNNNNNNNN~79683004200000~FTAU1173232 ~MEDUTO8563~00001~ ~Y~NNNNNNNNNNN~79683004300000~IPXU3981409 ~MEDUTO8563~00001~ ~Y~NNNNNNNNNNN~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~INV~MDB                 ~MEDUTO856396                       ~MEDUTO856396                       ~MEDUTO856396                       ~MEDUTO856396                       }";
			SetUpShared(rra12String, "RRA12 ISL.txt", "23GB5EUDD4K6WTEAR6");
			var serviceTask = new RRA12ServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			var entryReloaded = new BusinessObjectFactory().Load<CusEntryHeader>(cusEntryHeader.PK);
			AssertContains("Information|Updated CusEntryHeader to CLR: 23GB5EUDD4K6WTEAR6;", log[1]);
			AssertEquals(EntryStatusList.Codes.Clear, entryReloaded.CH_EntryStatus);
		}

		public void TestRra12ServiceTaskEndToEndUsingShyteRRA12()
		{
			// We need to assign the user under which this appears to run, "Developer", an email address, otherwise we cannot send any emails
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "foo@bar.com";
			staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
			GlbGroup postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			staff.Groups.Add(Factory.Load<GlbGroup>(postMasters.PK.ToGuid()));
			Factory.Save();

			var mailItem = Factory.New<MailItem>();
			mailItem.MI_From = "Destin8PRD@destin8.co.uk";
			mailItem.MI_Subject = "Amalgamation Clearance Advice";
			mailItem.MI_Status = MailStatus.Queued;
			mailItem.MI_Direction = MailDirection.Receive;
			mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
			mailItem.MI_ReceivedDateTime = ZDateTime.Now;
			var attachment = mailItem.MailAttachments.AddNew();
			attachment.MA_Data = ZBlob.FromAscii("Complete crap in attachment");
			attachment.MA_FileName = "something.TXT";
			MailFilterLocatorTestHelper.SetApplication(mailItem, MailFilterCodes.GbMcpRra12);

			Factory.Save();

			ErrorReporter.Clear();

			// Here is the main work:
			RRA12ServiceTask serviceTask = new RRA12ServiceTask();
			TestServiceLogger log = InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals("Information|Made EDIMessage from dbo.EDIInterchange for RRA12. Text=Complete crap in attachment", log[0]);

			AssertEquals("Should see a warning to postmaster and a developer exception report", 1, Env.OutgoingCustomsMailManager.EmailsCreated.Count);
			AssertContains("Email to postmaster should say .... ", "could not parse an RRA12 due to it being in an unsupported format.",
							Env.OutgoingCustomsMailManager.EmailsCreated[0].Body);
			AssertContains("Email should have an attachment called Invalid RRA12", "Invalid RRA12", Env.OutgoingCustomsMailManager.EmailsCreated[0].Attachments[0].DisplayName);
		}

		MailItem mailItem;
		MailAttachment attachment;
		JobDeclaration declaration;
		CusEntryHeader cusEntryHeader;

		[TestDate(2010, 10, 21)]
		public void TestRra12IslWithFullBolData2010Format()
		{
			SetUpShared(fullRra12IslIn2010VersionWithFullBolNumber, "RRA12 ISL.txt", "071-000023R");
			RRA12ServiceTask serviceTask = new RRA12ServiceTask();
			TestServiceLogger log = InitialiseAndRunTaskSchedule(serviceTask);
			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			mailItem.Reload();
			AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);
			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("The number of emails created by the Mcp RRA12 processor in the test should be 1.  That is, we should send one email notification to the user ('s group) for the RRA11 that we received.", 1, emails.Count);
			AssertEquals("Subject", "Customs amalgamation clearance advice for  B00009999 / 071-000023R", emails[0].Subject);
			AssertContains(expectedBodyForLongBolsFromIsl, emails[0].Body);

			AssertContains("071-000023R", emails[0].Body);
			AssertContains("Customs clearance", emails[0].Body);
			AssertEquals("foo@bar.com", emails[0].Recipients[0]);

			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			AssertEquals("Checking the CH_EntryStatus flag to see if the status of the entry is now 'cleared'.", "CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals(new ZDateTime(2010, 10, 21, 10, 43, 0), cusEntryHeader.CH_EntryReleaseDate);
			AssertEquals(new ZDateTime(2010, 10, 21, 10, 43, 0), cusEntryHeader.ClearanceDate);
			declaration.Reload();
			((ILogsInternals)declaration.Logs).ReloadFromDB();
			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			AssertEquals(EntryStatusList.Codes.Clear, declaration.JE_EntryStatus);
			AssertEquals(EntryStatusList.Codes.Clear, cusEntryHeader.CH_EntryStatus);
			AssertEquals(MessageStatusList.Codes.OK, cusEntryHeader.CH_Status);

			// The service task takes a peek inside the Messages collection during processing, which caches the collection.  We need to load it to refresh the cached collection otherwise the following assertion fails.
			cusEntryHeader.Messages.Load();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			EDIMessage inboundEdiMessageThatWeMade = cusEntryHeader.Messages.LastIncomingMessage;
			AssertEquals(EDIMessage.Status.Received, inboundEdiMessageThatWeMade.EM_Status);
			AssertEquals(EDIInterchange.Status.Received, inboundEdiMessageThatWeMade.Interchange.EI_Status);
			AssertEquals(bbbBranch.PK, inboundEdiMessageThatWeMade.EM_GB);
			AssertEquals("R12", inboundEdiMessageThatWeMade.EM_MessageType);
			AssertContains(expectedBodyForLongBolsFromIsl, inboundEdiMessageThatWeMade.EM_MessageInterpretation);

			ZQuery query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code);
			query.AddToFilter(StmALogSchema.SL_Parent, declaration.PK);
			AssertEquals("Cleared logs on declaration", 1, Factory.Load<StmALog>(query).Length);
		}

		[TestDate(2009, 10, 23)]
		public void TestRra12IslFormatManyAuthorities()
		{
			SetUpShared(fullRra12ISLManyAuthorities, "RRA12 ISL.txt", "071-001006L");
			RRA12ServiceTask serviceTask = new RRA12ServiceTask();
			TestServiceLogger log = InitialiseAndRunTaskSchedule(serviceTask);
			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			AssertEquals("HLD", cusEntryHeader.CH_EntryStatus);
			mailItem.Reload();
			AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);
			Assert("Should have 'hold' log", (from StmALog l in cusEntryHeader.Logs.GetAllLogs() where l.SL_Reference == "Hold-Multiple" select l).Any());
			Assert("Should have 'clear' log", (from StmALog l in cusEntryHeader.Logs.GetAllLogs() where l.SL_Reference == "Cleared via RRA12" select l).Any());
			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("The number of emails created by the Mcp RRA12 processor in the test should be 1.  That is, we should send one email notification to the user ('s group) for the RRA11 that we received.", 1, emails.Count);
			AssertEquals("Subject", "Customs amalgamation clearance advice for  B00009999 / 071-001006L", emails[0].Subject);
			AssertContains(expectedBodyContainsFromIslManyAuths, emails[0].Body);
		}

		[TestDate(1986, 3, 12)]
		public void TestRra12IslFormatManyEntriesWithSameNumberButOnlyOneWithRightDate()
		{
			SetUpShared(fullRra12ISLManyAuthorities, "RRA12 ISL.txt", "071-001006L");
			var declarationTwo = Factory.New<JobDeclaration>();
			var entryTwo = declarationTwo.CustomsEntryHeaders.AddNew();
			entryTwo.EntryNumber = "071-001006L";
			entryTwo.CusEntryNumber.CE_IssueDate = new ZDateTime(2009, 10, 23); // from ISL message
			Factory.Save();

			var serviceTask = new RRA12ServiceTask();
			InitialiseAndRunTaskSchedule(serviceTask);
			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			entryTwo.Reload();
			AssertEquals("", cusEntryHeader.CH_EntryStatus);
			AssertEquals("HLD", entryTwo.CH_EntryStatus);
		}

		[TestDate(2009, 10, 23)]
		public void TestRra12IslFormatOneAuthority()
		{
			SetUpShared(fullRra12ISLOneAuth, "RRA12 ISL.txt", "071-001006L");
			var serviceTask = new RRA12ServiceTask();
			var log = InitialiseAndRunTaskSchedule(serviceTask);
			cusEntryHeader.Reload();
			((ILogsInternals)cusEntryHeader.Logs).ReloadFromDB();
			AssertEquals("HLD", cusEntryHeader.CH_EntryStatus);
			Assert((from StmALog l in cusEntryHeader.Logs.GetAllLogs() where l.SL_SE_NKEvent == Events.CustomsClearedCode select l).Any());
			mailItem.Reload();
			AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);
			Assert("Should have 'hold' log", (from StmALog l in cusEntryHeader.Logs.GetAllLogs() where l.SL_Reference == "Hold-PortHealthDetention" select l).Any());
			Assert("Should have 'clear' log", (from StmALog l in cusEntryHeader.Logs.GetAllLogs() where l.SL_Reference == "Cleared via RRA12" select l).Any());
			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("The number of emails created by the Mcp RRA12 processor in the test should be 1.  That is, we should send one email notification to the user ('s group) for the RRA11 that we received.", 1, emails.Count);
			AssertEquals("Subject", "Customs amalgamation clearance advice for  B00009999 / 071-001006L", emails[0].Subject);
			AssertContains(expectedBodyContainsFromIslOneAuth, emails[0].Body);

			//Check RRA12 goes to eDocs:
			var docManagerInfo = ((IDocManagerSupport)declaration).DocManagerInfo;
			AssertEquals("Attached Files in eDocs against Declaration", 1, docManagerInfo.Files.Count);
			var eDoc = docManagerInfo.Files[0];
			AssertEquals("eDoc.Description", "Amalgamation clearance advice (RRA12) from Destin8", eDoc.Description);
			AssertEquals("eDoc.DocType", Core.Constants.RefDocTypes.ReleaseRemovalAdvice, eDoc.DocType);
			AssertEquals("eDoc.FileName", "Amalgamation clearance advice 071-001006L.rra12.txt", eDoc.FileName);
			AssertContains("eDoc.ImageData", expectedBodyContainsFromIslOneAuth, eDoc.ImageData.ToAscii());

			CombineAssertions(() =>
			{
				Assert("Log for DDA added", cusEntryHeader.Logs.Find(x => x.SL_SE_NKEvent == Events.DocumentAllocated.Code).Any());
				Assert("Event for DDA added", cusEntryHeader.Logs.Find(x => x.Event.SE_Code == Events.DocumentAllocated.Code).Any());
			});
		}

		[TestDate(2009, 10, 23)]
		public void TestRra12IslServiceTaskEndToEndWithRetardedEntryNumber()
		{
			SetUpShared(fullRra12ISLOneAuth, "RRA12 ISL.txt", "CrapEntryNumberForDB");
			string originalStatus = "ABC";
			declaration.JE_EntryStatus = originalStatus;
			cusEntryHeader.CH_EntryStatus = originalStatus;

			// Here is the main work:
			RRA12ServiceTask serviceTask = new RRA12ServiceTask();
			TestServiceLogger log = InitialiseAndRunTaskSchedule(serviceTask);

			AssertEquals(4, log.Count);
			AssertContains("Made EDIMessage from dbo.EDIInterchange for RRA12. Text==RRA12~0910231654~FEY", log[0]);
			AssertContains("Sent email [Customs amalgamation clearance advice for  071 001006L - entry and job not found] to foo@bar.com", log[3]);

			mailItem.Reload();
			AssertEquals("Received Email Status", MailStatus.Processed, mailItem.MI_Status);

			var emails = Env.OutgoingCustomsMailManager.EmailsCreated;
			AssertEquals("The number of emails created by the Mcp RRA12 processor in the test should be 1 - warning", 1, emails.Count);

			cusEntryHeader.Reload();
			declaration.Reload();
			AssertNotEquals("Clearance status of dec should not be CLR", "CLR", declaration.JE_EntryStatus);
			AssertNotEquals("Clearance status of entry should not be CLR", "CLR", cusEntryHeader.CH_EntryStatus);
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			branchEnvironment = DisposableEnvironment.ForBranch(Business.Testing.DeclarationTestHelper.CreateGbCompanyAndBranchAndSave(Factory).PK.ToGuid());
		}

		IDisposable branchEnvironment;

		protected override void TearDownCore()
		{
			base.TearDownCore();
			branchEnvironment.Dispose();
		}

		void SetUpShared(string textToAdd, string attachmentFileName, string entryNum)
		{
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			bbbBranch = Factory.New<GlbBranch>();
			bbbBranch.GB_Code = "BBB";
			bbbBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(bbbBranch.PK.ToGuid()))
			{
				// We need to assign the user under which this appears to run, "Developer", an email address, otherwise we cannot send any emails
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = "DJC";
				staff.GS_EmailAddress = "foo@bar.com";
				staff.Groups.Add(Factory.Load<GlbGroup>(GBCustomsDataRegistry.Instance.CustomsResponseNotificationsToGroup));
				GlbGroup postMasters = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
				staff.Groups.Add(Factory.Load<GlbGroup>(postMasters.PK.ToGuid()));
				Factory.Save();

				mailItem = Factory.New<MailItem>();
				mailItem.MI_From = "ANYTHING@destin8.co.uk";
				mailItem.MI_Subject = "Amalgamation Clearance Advice";
				mailItem.MI_Status = MailStatus.Queued;
				mailItem.MI_Direction = MailDirection.Receive;
				mailItem.MI_SendDateTime = ZDateTime.Now.AddMinutes(-5);
				mailItem.MI_ReceivedDateTime = ZDateTime.Now;
				attachment = mailItem.MailAttachments.AddNew();
				attachment.MA_Data = ZBlob.FromAscii(textToAdd);
				attachment.MA_FileName = attachmentFileName;
				var filter = new RRA12EmailsToEdiMessagesPoller(null).IncomingInterchangeMailFilter;
				if (filter.CanProcess(mailItem))
				{
					mailItem.MI_Application = filter.Code;
				}

				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				declaration.JE_DeclarationReference = "B00009999";

				cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				cusEntryHeader.CH_EntrySubmittedDate = ZDateTime.Now.AddMinutes(-60);
				cusEntryHeader.EntryNumber = entryNum;
				cusEntryHeader.CusEntryNumber.CE_IssueDate = ZDateTime.Now;

				var mockMessage = Factory.NewMoq<EDIMessageDummyForTest_123>();
				mockMessage.Protected().Setup<string>("GetMessageReferenceNumber").Returns("123");
				EDIMessage outboundMessage = mockMessage.Object;
				cusEntryHeader.Messages.Add(outboundMessage);
				outboundMessage.EM_SystemCreateUser = "DJC";
				outboundMessage.EM_MessageText = " I hate this silly insistence on message number <<MSGNO PLACEHOLDER>>";
				outboundMessage.EM_ReceiveTransmit = "TRX";
				Factory.Save();
				mockMessage.VerifyAll();
			}
		}

		#region RRA12 ISL sample files and outputs

		// Note the short and full bill of lading numbers. Follow me --->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->SHORT--->--->--->--->--->--->--->--->--->--->--->--->--->--->SHORT--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->--->---FULL--->--->--->--->--->--->--->--->FULL
		readonly string fullRra12IslIn2010VersionWithFullBolNumber = @"=RRA12~1010211043~FEY~63083~1010141500~FEY~SLDSSI00895                        ~071000023R211010                   ~Y~63083000800000~YMLU6666666 ~8888      ~00500~ ~Y~NNNNNNNNNNN~63083000900000~ACLU6666666 ~9999      ~00500~ ~Y~NNNNNNNNNNN~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~INV~MDB                 ~full bol for shipment number onexxx~Second BoL also several chars      }";

		readonly string fullRra12ISLManyAuthorities = @"=RRA12~0910231654~FEY~47801~0910201500~FEY~UNIT21                             ~071001006L231009                   ~N~47801001900000~MSKU6236806 ~858315845 ~02969~ ~N~NYNNNNNNNNN~47801002000000~MSKU6687688 ~858317782 ~00067~ ~N~NNNNYNNNNNN~47801002100000~MSKU8692370 ~528315578 ~00418~ ~N~NNNNYNNNNNN~47801002200000~MSKU9672681 ~858321779 ~00036~ ~N~YNNNNNNNNNN~47801002300000~PONU7598055 ~801421143 ~00037~ ~Y~NNNNNNNNNNN~47801002400000~PONU7695292 ~528324869 ~00101~ ~Y~NNNNNNNNNNN~47801002800000~MSKU1234567 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801002900000~MSKU7654321 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003000000~MSKU1122334 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003100000~MSKU2233445 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003200000~MSKU3344556 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003300000~MSKU4455667 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003400000~MSKU5566778 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003500000~MSKU6677889 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003600000~MSKU9988776 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~INV~MDB                 }
=RRA12~0910231654~FEY~47801~0910201500~FEY~UNIT21                             ~071001006L231009                   ~Y~47801003700000~MSKU8877665 ~12345678  ~00100~ ~Y~YYYNNNNNNNN~47801003800000~MSKU7766554 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003900000~MSKU6655443 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801004000000~MSKU5544332 ~12345678  ~00100~ ~N~NNYNNNNNNNN~47801004100000~MSKU4433221 ~12345678  ~00100~ ~N~NNNNYNNNNNN~47801004200000~MSKU0000000 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~INV~MDB                 }
";
		readonly string expectedBodyForLongBolsFromIsl = @"<h2>Customs clearance advice for B00009999 / 071-000023R</h1> <pre>RRA12    Destin8       Amalgamation Clearance Advice          21-Oct-10 10:43:00

-----------------------------------------------------------------------------
UVI: 63083			             Actual Arrival: 14-Oct-10 15:00:00
-----------------------------------------------------------------------------
 Agents Ref : SLDSSI00895                         Entry No : 071 000023R  21/10/10

 Agent : FEY

 UCN		B/L		Unit Id		Pkgs	Devan	Removal	Holds
63083000800000	full bol for shipment number onexxx	YMLU6666666 	500	 	Y	
63083000900000	Second BoL also several chars	ACLU6666666 	500	 	Y	
";

		readonly string expectedBodyContainsFromIslManyAuths = @"<h2>Customs clearance advice for B00009999 / 071-001006L</h1> <pre>RRA12    Destin8       Amalgamation Clearance Advice          23-Oct-09 16:54:00

-----------------------------------------------------------------------------
UVI: 47801			             Actual Arrival: 20-Oct-09 15:00:00
-----------------------------------------------------------------------------
 Agents Ref : UNIT21                              Entry No : 071 001006L  23/10/09

 Agent : FEY

 UCN		B/L		Unit Id		Pkgs	Devan	Removal	Holds
47801001900000	858315845 	MSKU6236806 	2969	 	N	Local Customs Hold Applied
47801002000000	858317782 	MSKU6687688 	67	 	N	Scanner Hold Applied
47801002100000	528315578 	MSKU8692370 	418	 	N	Scanner Hold Applied
47801002200000	858321779 	MSKU9672681 	36	 	N	Subject to Port Health Detain
47801002300000	801421143 	PONU7598055 	37	 	Y	
47801002400000	528324869 	PONU7695292 	101	 	Y	
47801002800000	12345678  	MSKU1234567 	100	 	Y	
47801002900000	12345678  	MSKU7654321 	100	 	Y	
47801003000000	12345678  	MSKU1122334 	100	 	Y	
47801003100000	12345678  	MSKU2233445 	100	 	Y	
47801003200000	12345678  	MSKU3344556 	100	 	Y	
47801003300000	12345678  	MSKU4455667 	100	 	Y	
47801003400000	12345678  	MSKU5566778 	100	 	Y	
47801003500000	12345678  	MSKU6677889 	100	 	Y	
47801003600000	12345678  	MSKU9988776 	100	 	Y	
47801003700000	12345678  	MSKU8877665 	100	 	Y	Subject to Port Health Detain, Local Customs Hold Applied, DOT Hold Applied
47801003800000	12345678  	MSKU7766554 	100	 	Y	
47801003900000	12345678  	MSKU6655443 	100	 	Y	
47801004000000	12345678  	MSKU5544332 	100	 	N	DOT Hold Applied
47801004100000	12345678  	MSKU4433221 	100	 	N	Scanner Hold Applied
47801004200000	12345678  	MSKU0000000 	100	 	Y	";

		readonly string fullRra12ISLOneAuth = @"=RRA12~0910231654~FEY~47801~0910201500~FEY~UNIT21                             ~071001006L231009                   ~Y~47801003700000~MSKU8877665 ~12345678  ~00100~ ~Y~YNNNNNNNNNN~47801003800000~MSKU7766554 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801003900000~MSKU6655443 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~47801004000000~MSKU5544332 ~12345678  ~00100~ ~N~YNNNNNNNNNN~47801004100000~MSKU4433221 ~12345678  ~00100~ ~N~YNNNNNNNNNN~47801004200000~MSKU0000000 ~12345678  ~00100~ ~Y~NNNNNNNNNNN~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~              ~            ~          ~     ~ ~ ~           ~INV~MDB                 }";

		readonly string expectedBodyContainsFromIslOneAuth =
		@"UVI: 47801			             Actual Arrival: 20-Oct-09 15:00:00
-----------------------------------------------------------------------------
 Agents Ref : UNIT21                              Entry No : 071 001006L  23/10/09

 Agent : FEY

 UCN		B/L		Unit Id		Pkgs	Devan	Removal	Holds
47801003700000	12345678  	MSKU8877665 	100	 	Y	Subject to Port Health Detain
47801003800000	12345678  	MSKU7766554 	100	 	Y	
47801003900000	12345678  	MSKU6655443 	100	 	Y	
47801004000000	12345678  	MSKU5544332 	100	 	N	Subject to Port Health Detain
47801004100000	12345678  	MSKU4433221 	100	 	N	Subject to Port Health Detain
47801004200000	12345678  	MSKU0000000 	100	 	Y	";
		#endregion

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						MailDBItemsSchema.Constants.TableName,
						"MCP Destin8 RRA12 mail inbound",
						MailDBItemsSchema.Constants.MI_Application + "=" + MailFilterCodes.GbMcpRra12,
						MailDBItemsSchema.Constants.MI_Status + "=" + StatusCodeList.Codes.Queued,
						MailDBItemsSchema.Constants.MI_Direction + "=" + DirectionList.Codes.Receive
					),
				};
			}
		}

		GlbBranch bbbBranch;

		static Image CreateImageAndTagIt(string branchCode)
		{
			// Must be a JPG otherwise the property item's value gets screwed.
			var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.GB.MCP.ServiceTasks.Test.IslAndTextProcessors.Logo.jpg");
			var image = Image.FromStream(stream);
			var pi = image.GetPropertyItem(20752);
			pi.Id = 20753;
			pi.Type = 1;
			pi.Value = Encoding.UTF8.GetBytes(branchCode);
			pi.Len = pi.Value.Length;
			image.SetPropertyItem(pi);
			return image;
		}
	}

	public class EDIMessageDummyForTest_123 : EDIMessage
	{
		public EDIMessageDummyForTest_123(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
		protected override string GetMessageReferenceNumber()
		{
			return "123";
		}
	}
}
