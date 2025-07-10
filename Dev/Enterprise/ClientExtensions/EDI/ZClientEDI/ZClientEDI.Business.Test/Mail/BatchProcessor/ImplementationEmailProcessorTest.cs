using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.EConversation.ServiceTasks;
using Enterprise.EConversation.Testing;
using Enterprise.Environment;
using Enterprise.MailManager;
using Enterprise.MailManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	internal class ImplementationEmailProcessorTest : BusinessObjectEmailProcessorTestCase<ImplementationEmailProcessor, EDIProject>
	{
		public void TestProcessNoSubject()
		{
			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(true, processor.CreateAndProcessMailItem(MakeEmail("").GetEmail(), LoggerForTest));
		}

		public void TestProcessNoProjectReference()
		{
			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(true, processor.CreateAndProcessMailItem(MakeEmail("this subject has no project reference").GetEmail(), LoggerForTest));
			AssertNoEmailAttachedLogEntry();
		}

		public void TestProcessInvalidProjectReference()
		{
			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(true, processor.CreateAndProcessMailItem(MakeEmail("this subject has an project reference PRJ98765432 that doesn't actually exist").GetEmail(), LoggerForTest));
			AssertNoEmailAttachedLogEntry();
		}

		public void TestProcessProjectReferenceAtEnd()
		{
			MakeAndSaveProject("PRJ00000001");

			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(true, processor.CreateAndProcessMailItem(MakeEmail("this subject has a valid project reference at the end PRJ00000001").GetEmail(), LoggerForTest));
			AssertEmailAttachedLogEntry("PRJ00000001");
		}

		public void TestProcessProjectReferenceAtBeginning()
		{
			MakeAndSaveProject("PRJ00000002");

			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(true, processor.CreateAndProcessMailItem(MakeEmail("PRJ00000002 this subject has a valid project reference at the beginning ").GetEmail(), LoggerForTest));
			AssertEmailAttachedLogEntry("PRJ00000002");
		}

		public void TestProcessProjectReferenceInMiddle()
		{
			MakeAndSaveProject("PRJ00000003");

			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(true, processor.CreateAndProcessMailItem(MakeEmail("this subject has a valid project reference in PRJ00000003 the middle").GetEmail(), LoggerForTest));
			AssertEmailAttachedLogEntry("PRJ00000003");
		}

		public void TestProcessAttachAndSendEmailToProject()
		{
			EDIProject project = MakeAndSaveProject("PRJ00000004", "Support@enterprisedevelopment.cargowise.com");

			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			Email email = MakeEmail("this should be attached to PRJ00000004").GetEmail();
			ZQuery query = new ZQuery(MailDBItemsSchema.MI_Subject, "FW: this should be attached to PRJ00000004");
			AssertEquals("Precondition", 0, Factory.GetDatabaseCount(typeof(MailItem), query));

			AssertEquals(true, processor.CreateAndProcessMailItem(email, LoggerForTest));
			AssertEmailAttachedLogEntry("PRJ00000004");
			AssertEquals(1, ((IDocManagerSupport)project).DocManagerInfo.Files.Count);
			AssertEquals("this should be attached to PRJ00000004.eml", ((IDocManagerSupport)project).DocManagerInfo.Files[0].FileName);

			MailItem[] outgoingMails = Factory.Load<MailItem>(query);
			AssertEquals("1 email", 1, outgoingMails.Length);
			AssertEquals("1 or 2 recipients", 1 + (ShouldForwardProcessedEmailToInstallationManager(project) ? 1 : 0), outgoingMails[0].MailRecipients.Count);
			AssertEquals(Env.Registry.MailboxEmailAddress, TrimEmail(outgoingMails[0].MI_From));
			AssertEquals("Support@enterprisedevelopment.cargowise.com", TrimEmail(outgoingMails[0].MailRecipients[0].EmailAddress));
		}

		public void TestProcessAttachAndSendEmailToProjectWithNoProjectOrGroupManager()
		{
			EDIProject project = MakeAndSaveProject("PRJ00000012", "", ProductTypes.Codes.Enterprise);
			GlbGroup group = Factory.Load<GlbGroup>(EDIDataRegistry.Instance.IncidentInstallationsGroupENT.Value);
			GlbStaff groupManager = project.GetGroupManager(group);
			groupManager.GS_EmailAddress = "";
			Factory.Save();

			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			Email email = MakeEmail("Attach to PRJ00000012").GetEmail();
			ZQuery query = new ZQuery(MailDBItemsSchema.MI_Subject, "FW:Attach to PRJ00000012");
			AssertEquals("Precondition", 0, Factory.GetDatabaseCount(typeof(MailItem), query));

			AssertEquals(true, processor.CreateAndProcessMailItem(email, LoggerForTest));
			AssertEmailAttachedLogEntry("PRJ00000012");
			AssertEquals(1, ((IDocManagerSupport)project).DocManagerInfo.Files.Count);
			AssertEquals("Attach to PRJ00000012.eml", ((IDocManagerSupport)project).DocManagerInfo.Files[0].FileName);

			MailItem[] outgoingMails = Factory.Load<MailItem>(query);
			AssertEquals("0 emails", 0, outgoingMails.Length);
		}

		public void TestForwardProcessedEmailToProjectAssignee()
		{
			ZDateTime testStarted = ZDateTime.UtcNow;

			EDIProject project = MakeAndSaveProject("PRJ00000005", "newuser@cargowise.com");
			Email email = MakeEmail("PRJ00000005 - Enquiry")
				.Cc("Cc Recipient", "mycc@cargowise.com")
				.Bcc("BCc Recipient", "mybcc@cargowise.com")
				.GetEmail();

			ZQuery query = new ZQuery(MailDBItemsSchema.MI_Subject, "FW: PRJ00000005 - Enquiry");
			query.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Transmit);
			MailItem[] mailItems = Factory.Load<MailItem>(query);
			AssertEquals("Precondition", 0, mailItems.Length);

			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(true, processor.CreateAndProcessMailItem(email, LoggerForTest));

			mailItems = Factory.Load<MailItem>(query);
			AssertEquals("1 email", 1, mailItems.Length);
			MailItem emailSent = mailItems[0];
			AssertEquals("1 or 2 recipients", 1 + (ShouldForwardProcessedEmailToInstallationManager(project) ? 1 : 0), emailSent.MailRecipients.Count);
			Assert(emailSent.IsInDatabase && !emailSent.HasChanges);

			AssertEquals("From:", Env.Registry.MailboxEmailAddress, TrimEmail(emailSent.MI_From));
			AssertContains("To:", "newuser@cargowise.com", TrimEmail(emailSent.GetHeaderItem("to")));
			AssertEquals("Cc:", ZString.Empty, emailSent.GetHeaderItem("cc"));
			AssertEquals("Bcc:", ZString.Empty, emailSent.GetHeaderItem("bcc"));
			AssertEquals("testing ImplementationEmailProcessor\r\n", emailSent.MI_Body);
			Assert("SendDateTime is in correct range", emailSent.MI_SendDateTime > testStarted && emailSent.MI_SendDateTime < ZDateTime.UtcNow);
		}

		static string TrimEmail(string emailAddress)
		{
			return emailAddress.Substring(emailAddress.IndexOf('<') + 1).TrimEnd('>');
		}

		[TestDate(2008, 9, 26, 10, 0, 0)]
		public void TestForwardProcessedEmailToInstallationManagerIfAttendeeUnavailable()
		{
			//Assigned Staff has no email address
			EDIProject project1 = MakeAndSaveProject("PRJ00000001", string.Empty, ProductTypes.Codes.Enterprise);
			AssertForwardProcessedEmailToInstallationManagerIfAttendeeUnavailable(project1, 1, 0);

			//Assigned Staff is absent
			EDIProject project2 = MakeAndSaveProject("PRJ00000002", string.Empty, ProductTypes.Codes.Enterprise);
			project2.WKP_GS_NKProjectManager = ZString.Empty;
			Factory.Save();
			AssertForwardProcessedEmailToInstallationManagerIfAttendeeUnavailable(project2, 1, 0);

			//Assigned Staff is not working today
			EDIProject project3 = MakeAndSaveProject("PRJ00000003", "ted.burhan@testcargowise.com", ProductTypes.Codes.Enterprise);
			TestDateAttribute.Date = new DateTime(2012, 1, 1); //New Year - Sunday
			AssertForwardProcessedEmailToInstallationManagerIfAttendeeUnavailable(project3, 2, 1);
		}

		public void TestShouldNotForwardEmailIfRecipientIsTheMailboxItself()
		{
			EDIProject project = MakeAndSaveProject("PRJ001", "", ProductTypes.Codes.Enterprise);
			GlbGroup group = Factory.Load<GlbGroup>(EDIDataRegistry.Instance.IncidentInstallationsGroupENT.Value);
			GlbStaff groupManager = project.GetGroupManager(group);
			groupManager.GS_EmailAddress = EDIDataRegistry.Instance.ImplementationDefaultFromEmailAddress.Value;
			Factory.Save();

			Email email = MakeEmail("Enquiry for PRJ001").GetEmail();
			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			Assert("Should be processed OK", processor.CreateAndProcessMailItem(email, LoggerForTest));

			ZQuery query = new ZQuery(MailDBItemsSchema.MI_Subject, SQLComparisonOperator.Contains, "PRJ001");
			MailItem[] mailItems = Factory.Load<MailItem>(query);
			CombineAssertions(delegate
			{
				AssertEquals("Should not be forwarded - only the original email is in the DB", 1, mailItems.Length);
				AssertEquals("Original subject", "Enquiry for PRJ001", mailItems[0].MI_Subject);
			});
		}

		void AssertForwardProcessedEmailToInstallationManagerIfAttendeeUnavailable(EDIProject project, int recipientCount, int managerRecipientIndex)
		{
			Email email = MakeEmail(project.WKP_ProjectNumber + " - Enquiry")
				.Cc("Cc Recipient", "mycc@cargowise.com")
				.Bcc("BCc Recipient", "mybcc@cargowise.com")
				.GetEmail();

			ZQuery query = new ZQuery(MailDBItemsSchema.MI_Subject, "FW: " + project.WKP_ProjectNumber + " - Enquiry");
			query.AddToFilter(MailDBItemsSchema.MI_Direction, MailDirection.Transmit);
			MailItem[] mailItems = Factory.Load<MailItem>(query);
			AssertEquals("Precondition", 0, mailItems.Length);

			ImplementationEmailProcessor processor = GetNewBusinessObjectEmailProcessor();
			AssertEquals(true, processor.CreateAndProcessMailItem(email, LoggerForTest));

			mailItems = Factory.Load<MailItem>(query);
			AssertEquals(1, mailItems.Length);

			MailItem emailSent = mailItems[0];
			Assert(emailSent.IsInDatabase && !emailSent.HasChanges);

			AssertEquals("From:", Env.Registry.MailboxEmailAddress, TrimEmail(emailSent.MI_From));
			AssertEquals(recipientCount, emailSent.MailRecipients.Count);
			AssertEquals("To:", "InstallationManager" + project.WKP_SubType + "@testcargowise.com", TrimEmail(emailSent.MailRecipients[managerRecipientIndex].EmailAddress));
			AssertEquals("Cc:", ZString.Empty, emailSent.GetHeaderItem("cc"));
			AssertEquals("Bcc:", ZString.Empty, emailSent.GetHeaderItem("bcc"));
			AssertEquals("testing ImplementationEmailProcessor\r\n", emailSent.MI_Body);
		}

		bool ShouldForwardProcessedEmailToInstallationManager(EDIProject project)
		{
			GlbStaff projectManager = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, project.WKP_GS_NKProjectManager);
			return projectManager == null || projectManager.GS_EmailAddress.IsEmpty || !projectManager.IsWorkingToday;
		}

		void MakeAndSaveProject(string id)
		{
			MakeAndSaveProject(id, "");
		}

		EDIProject MakeAndSaveProject(string id, string assignedEmailAddress)
		{
			return MakeAndSaveProject(id, assignedEmailAddress, string.Empty);
		}

		EDIProject MakeAndSaveProject(string id, string assignedEmailAddress, string productType)
		{
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_ProjectNumber = id;
			project.WKP_SubType = productType;
			if (!string.IsNullOrEmpty(assignedEmailAddress))
			{
				GlbStaff staff = MakeAndSaveStaff("STAFF", assignedEmailAddress);
				project.WKP_GS_NKProjectManager = staff.GS_Code;
			}
			Factory.Save();
			return project;
		}

		GlbStaff MakeAndSaveStaff(string id, string assignedEmailAddress)
		{
			GlbStaff result = Factory.New<GlbStaff>();
			result.GS_Code = "ZAC";
			result.GS_NameTitle = id;
			result.GS_EmailAddress = assignedEmailAddress;
			Factory.Save();

			return result;
		}

		EmailBuilderForTesting MakeEmail(string subject)
		{
			return new EmailBuilderForTesting()
				.Subject(subject)
				.Body("testing ImplementationEmailProcessor")
				.From("testing@ImplementationEmailProcessor.xyz")
				.To("ImplementationEmailProcessor", "testing@ImplementationEmailProcessor.xyz");
		}

		protected override void SetUp()
		{
			SetUpInstallationGroupManagers();
			base.SetUp();
		}

		protected override string ExpectedEmailTypeName
		{
			get { return "Implementation"; }
		}

		protected override string ExpectedMailApplicationCode
		{
			get { return EDIMailApplication.Implementation; }
		}

		protected override ImplementationEmailProcessor GetNewBusinessObjectEmailProcessor()
		{
			return new ImplementationEmailProcessor();
		}

		protected override INumberFountainProxy NumberFountainForBusinessObject
		{
			get { return Env.NumberFountains.ProjectNo; }
		}

		void SetUpInstallationGroupManagers()
		{
			SetUpInstallationGroupManager(ProductTypes.Codes.Enterprise);
		}

		void SetUpInstallationGroupManager(string productType)
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = productType;
			GlbStaff manager = Factory.New<GlbStaff>();
			manager.GS_LoginName = "InstallionManager." + productType;
			manager.GS_Code = productType;
			manager.GS_Title = "Installation Manager" + productType;
			manager.GS_EmailAddress = "InstallationManager" + productType + "@testcargowise.com";
			group.Staff.Add(manager);
			manager.CurrentGroupLink.GK_MembershipType = Enterprise.ZArchitecture.Core.MembershipTypeList.Codes.MGR;
			GuidRegistryItem installationGroupRegistryItem = EDIDataRegistry.Instance.IncidentInstallationsGroupENT;

			if (installationGroupRegistryItem != null)
			{
				installationGroupRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			}
			Factory.Save();
		}

		protected override void SetReferenceNumber(EDIProject bizO, string refNumber)
		{
			bizO.WKP_ProjectNumber = refNumber;
		}
	}
}
