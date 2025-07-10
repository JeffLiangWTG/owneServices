using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDIGlbStaffWorkflowDescriptor))]
	class EDIGlbStaffWorkflowDescriptorTest : WorkflowDescriptorTestCase<EDIGlbStaffWorkflowDescriptor>
	{
		public override void TestID()
		{
			AssertEquals(GlbStaffWorkflowDescriptor.WorkflowTypeCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Staff and Resources", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals(0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(false, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy | MessageRecipientPartyType.Staff; }
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get => new[] { new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.EnrolInWiseTechAcademyCourse, WorkflowTriggerActionTypeConstants.Descriptions.EnrolInWiseTechAcademyCourse) };
		}

		public void TestNotificationEmailTriggerShouldConvertDummyTokenInContactPasswordInstructionMacroUrl()
		{
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupUserAccount();
			otherContact.OC_PER = contact.OC_PER;
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount-portal.cargowise.com/myAccount");

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var trigger = staff.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Complete CCO";
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompletedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				action.PQ_EmailAddr = "alex@aaa.com";
				action.PQ_EmailText = "(*Person.ContactCollection.Find(\"{OC_IsActive}\"==\"Y\").PasswordInstructionMacroUrl*)";

				trigger.Parent.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "CCO - Certified Operator");
				Factory.Save();

				AssertEquals("Precondition", 2, staff.Person.ContactCollection.Count);

				var mockRepo = new Mock<IProductRegistration>();
				mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
				using (ObjectFactory.Substitute(mockRepo.Object))
				{
					MasterFilesTestHelper.RunLogWalker();
				}

				var emails = Environment.Env.OutgoingMailManager.EmailsCreated;
				AssertEquals("Precondition: Email should have been created", 1, emails.Count);
				var email = emails[0];
				AssertEquals("Precondition: 1 Recipient", 1, email.Recipients.Count);
				AssertEquals("Precondition: Recipient should be the specified", "alex@aaa.com", email.Recipients[0].Email);
				AssertContains("Body should contain macro", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value, email.Body);
				var tokenQuery = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
				var token = Factory.LoadTop1<StmAccessToken>(tokenQuery);
				AssertContains("Dummy macro should be replaced by token", FormattableString.Invariant($"https://myaccount-portal.cargowise.com/myAccount/Admin/SetPassword.aspx?SetKey={token.SAT_Token}"), email.Body);
			}
		}

		public void TestConvertDummyTokenInContactPasswordInstructionMacroUrlResetUrlGeneration()
		{
			SetupUserAccount();
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount-portal.cargowise.com/myAccount");

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var trigger = staff.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Complete CCO";
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompletedCode;

				var passwordLogContact1 = Factory.New<StmALog>();
				using (passwordLogContact1.LockForUpdatingKeyFieldsForTesting())
				{
					passwordLogContact1.SL_Table = OrgContactSchema.Constants.TableName;
					passwordLogContact1.SL_Parent = contact.PK;
					passwordLogContact1.SL_SE_NKEvent = AutoEvents.WebAccessPasswordChangedCode;
					passwordLogContact1.SL_EventTime = ZDate.Today.AddDays(-1);
				}
				Factory.Save();

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				action.PQ_EmailAddr = "alex@aaa.com";
				action.PQ_EmailText = "(*Person.ContactCollection.Find(\"{OC_IsActive}\"==\"Y\").PasswordInstructionMacroUrl*)";

				trigger.Parent.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "CCO - Certified Operator");
				Factory.Save();

				var mockRepo = new Mock<IProductRegistration>();
				mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
				using (ObjectFactory.Substitute(mockRepo.Object))
				{
					MasterFilesTestHelper.RunLogWalker();
				}

				var emails = Environment.Env.OutgoingMailManager.EmailsCreated;
				AssertEquals("Precondition: Email should have been created", 1, emails.Count);
				var email = emails[0];
				AssertEquals("Precondition: 1 Recipient", 1, email.Recipients.Count);
				AssertEquals("Precondition: Recipient should be the specified", "alex@aaa.com", email.Recipients[0].Email);
				AssertContains("Body should contain macro", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value, email.Body);
				var tokenQuery = new ZQuery(StmAccessTokenSchema.SAT_Scope, contact.OC_Email);
				var token = Factory.LoadTop1<StmAccessToken>(tokenQuery);
				AssertContains("Reset page should be used since password email has been sent before", FormattableString.Invariant($"https://myaccount-portal.cargowise.com/myAccount/Admin/ResetMasterPassword.aspx?ResetKey={token.SAT_Token}"), email.Body);
			}
		}

		public void TestTokenShouldNotBeGeneratedIfMacroNotPresent()
		{
			SetupUserAccount();
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount-portal.cargowise.com/myAccount");

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var trigger = staff.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Complete CCO";
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompletedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				action.PQ_EmailAddr = "alex@aaa.com";
				action.PQ_EmailText = "blah blah blah";

				trigger.Parent.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "CCO - Certified Operator");
				Factory.Save();

				var mockRepo = new Mock<IProductRegistration>();
				mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
				using (ObjectFactory.Substitute(mockRepo.Object))
				{
					MasterFilesTestHelper.RunLogWalker();
				}

				var emails = Environment.Env.OutgoingMailManager.EmailsCreated;
				AssertEquals("Precondition: Email should have been created", 1, emails.Count);
				var email = emails[0];
				AssertEquals("1 Recipient", 1, email.Recipients.Count);
				AssertEquals("Recipient should be the OC_Email", "alex@aaa.com", email.Recipients[0].Email);
				AssertNotContains("Body should not contain macro", WebDataRegistry.Instance.CargoWiseUserPortalUrl.Value, email.Body);

				var tokenQuery = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
				AssertEquals("No tokens should be generated", 0, Factory.Load<StmAccessToken>(tokenQuery).Length);
			}
		}

		public void TestConvertDummyTokenInContactPasswordInstructionMacroUrlMultipleUrls()
		{
			var otherContact = Factory.NewWithValidTestData<OrgContact>();
			Factory.Save();
			SetupUserAccount();
			otherContact.OC_PER = contact.OC_PER;
			WebDataRegistry.Instance.CargoWiseUserPortalUrl.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://myaccount-portal.cargowise.com/myAccount");

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var trigger = staff.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Complete CCO";
				trigger.TriggerConditions.TriggerEventCode = AutoEvents.AccreditationAttemptCompletedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
				action.PQ_EmailAddr = "abc@email.com";
				action.PQ_EmailText = "(*Person.ContactCollection.Find(\"{OC_IsActive}\"==\"Y\").PasswordInstructionMacroUrl*) blah (*Person.ContactCollection.Find(\"{OC_IsActive}\"==\"Y\").PasswordInstructionMacroUrl*)";

				trigger.Parent.Logs.AddNew(AutoEvents.AccreditationAttemptCompleted, "CCO - Certified Operator");
				Factory.Save();

				var mockRepo = new Mock<IProductRegistration>();
				mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
				using (ObjectFactory.Substitute(mockRepo.Object))
				{
					MasterFilesTestHelper.RunLogWalker();
				}

				var emails = Environment.Env.OutgoingMailManager.EmailsCreated;
				AssertEquals("Precondition: Email should have been created", 1, emails.Count);
				var email = emails[0];
				var tokenQuery = new ZQuery(StmAccessTokenSchema.SAT_ParentId, contact.PK);
				var tokens = Factory.Load<StmAccessToken>(tokenQuery);
				AssertEquals("Only 1 token should be generated", 1, tokens.Length);
				var token = tokens[0];
				AssertContains("Dummy macro should be replaced by token in both instances", FormattableString.Invariant($"https://myaccount-portal.cargowise.com/myAccount/Admin/SetPassword.aspx?SetKey={token.SAT_Token} blah https://myaccount-portal.cargowise.com/myAccount/Admin/SetPassword.aspx?SetKey={token.SAT_Token}"), email.Body);
			}
		}

		protected override void SetNotificationEmailAddress(BusinessObject line, ProcessTaskNotification notification, string emailAddress)
		{
			if (notification.PQ_TriggerParty == MessageRecipientPartyTypeList.Codes.Staff)
			{
				((GlbStaff)notification.Parent.GetParent()).GS_EmailAddress = emailAddress;
			}
			else
			{
				base.SetNotificationEmailAddress(line, notification, emailAddress);
			}
		}

		#region WiseTech Academy Enrolment Trigger Action

		protected override ITemplateTrigger CreateTemplateTriggerForWtaEnrolment(ProcessTaskTemplate template, string unitId)
		{
			var trigger = MasterFilesTestHelper.CreateTemplateTrigger(template, Events.AttachedCode, EventReferenceConditionList.Codes.EventReferenceParameters, EventReferenceParameters.Codes.Group + "=GRAVEY");
			MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.EnrolInWiseTechAcademyCourse, MessageRecipientPartyTypeList.Codes.Staff, actionReference: unitId);
			return trigger;
		}

		protected override IWorkflowProvider GetWorkflowProviderForWtaEnrolmentTrigger(GlbStaff staffToBeEnrolled)
		{
			return staffToBeEnrolled;
		}

		protected override void RaiseEventForWtaEnrolmentTrigger(IWorkflowProvider job)
		{
			var group = MasterFilesTestHelper.CreateGroup(Factory, "GRAVEY");
			group.Staff.Add((GlbStaff)job);
			Factory.Save();
		}

		#endregion

		#region Implementation

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			var glbStaff = Factory.NewWithValidTestData<EDIGlbStaff>();
			return new IWorkflowProvider[] { glbStaff };
		}

		protected override string EDIMessageSubType => EDIMessageSubTypeList.Codes.XmlNativeStaff;

		void SetupUserAccount()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			database = licence.Database;
			database.LD_DatabaseNumber = 3001;
			var org = database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			staff = Factory.NewWithValidTestData<EDIGlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_FullName = "Test User A";
			staff.GS_EmailAddress = "test.usera@test.org";
			Factory.Save();

			var mockRepo = new Mock<IProductRegistration>();
			mockRepo.Setup(m => m.Key.DatabaseNumber).Returns(database.LD_DatabaseNumber);
			using (ObjectFactory.Substitute(mockRepo.Object))
			{
				var importer = new StaffContactImporter(Factory);
				importer.CreateOrUpdateContactFromStaff(staff);
			}

			org.Contacts.Reload(false);
			AssertEquals("Precondition", 1, org.Contacts.Count);
			contact = org.Contacts[0];
			AssertEquals("Precondition", "Test User A", contact.OC_ContactName);
			AssertEquals("Precondition", "test.usera@test.org", contact.OC_Email);
			AssertEquals("Precondition", false, staff.GS_PER.IsEmpty);
			AssertEquals("Precondition", staff.GS_PER, contact.OC_PER);
		}

		OrgContact contact;
		EDIGlbStaff staff;
		LicenceDatabase database;

		#endregion
	}
}
