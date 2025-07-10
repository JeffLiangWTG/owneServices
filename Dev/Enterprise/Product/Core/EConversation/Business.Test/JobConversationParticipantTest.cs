using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	[TestedType(typeof(JobConversationParticipant))]
	public class JobConversationParticipantTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetupDataForSettingValueCallsRefreshBindingTestIfNeeded(ZPropertyInfo info)
		{
			if (info.Name == "RelatedPartyTypeName")
			{
				info.Value = new ZString("Contact");
			}
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
			=> new Dictionary<string, IZType>
			{
				{ "RelatedPartyTypeName", new ZString("Organization") }
			};

		public void TestInvalidParentKeySetsInvalidParticipantID()
		{
			var participant = Factory.New<JobConversationParticipant>();
			participant.JCP_ParticipantTableCode = "GS";

			AssertEquals("Before being set the parentId should be empty", ZGuid.Empty, participant.JCP_ParticipantID);

			participant.ParentKey = "Invalid";

			AssertEquals("If the ParentKey is Invalid, it should set the PK to invalid.", ZGuid.Invalid, participant.JCP_ParticipantID);
		}

		public void TestParentKeyForContacts()
		{
			var testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_Code = "ORGCOD";

			var testContact = testOrg.Contacts.AddNew();
			testContact.OC_ContactName = "The Dude";

			AssertParentKeyValue("Should recognise the org code in parens", "The Dude (ORGCOD)", testContact);
			AssertParentKeyValue("Blank should return null", string.Empty, null, tablePrefix: "OC");
		}

		public void TestParentKeyForContacts_TwoWithSameName()
		{
			var firstOrg = Factory.NewWithValidTestData<OrgHeader>();
			firstOrg.OH_Code = "ORGCOD";

			var secondOrg = Factory.NewWithValidTestData<OrgHeader>();
			secondOrg.OH_Code = "DIFORG";

			var firstContact = firstOrg.Contacts.AddNew();
			var secondContact = secondOrg.Contacts.AddNew();
			firstContact.OC_ContactName = secondContact.OC_ContactName = "The Dude";

			AssertParentKeyValue("Should use the brackets to find the right contact", "The Dude (ORGCOD)", firstContact);
			AssertParentKeyValue("Should use the brackets to find the right contact (DIFORG)", "The Dude (DIFORG)", secondContact);
		}

		void AssertParentKeyValue(string message, string parentKey, IConversationParticipant expectedParticipant, string tablePrefix = null)
		{
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			participant.JCP_ParticipantTableCode = tablePrefix ?? ((BusinessObject)expectedParticipant).TablePrefix;

			participant.ParentKey = parentKey;
			AssertEquals($"Message: {message}. Key: {parentKey}.", expectedParticipant, participant.Parent);
		}

		public void TestJCP_ParticipantID_SetsParentCode()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XCX";

			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			participant.JCP_ParticipantTableCode = "GS";
			participant.JCP_ParticipantID = staff.PK;

			AssertEquals("Updating JCP_ParentID should also update the ParentKey", "XCX", participant.ParentKey);
		}

		public void TestOrgParticipantsNotAutomaticallySubscribed_WhenParentTableCodeIsINC()
		{
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();
			participant.Conversation.JCC_ParentTableCode = IncidentRequestSchema.Constants.Prefix;
			participant.RelatedPartyTypeName = "Organization";

			AssertEquals(OrgHeaderSchema.Constants.Prefix, participant.JCP_ParticipantTableCode);
			AssertEquals("The org participants aren't automatically subscribed", false, participant.JCP_IsSubscribed);
		}

		public void TestRelatedPartyTableIsLinkedToParticipantTableCode()
		{
			var participant = Factory.NewWithValidTestData<JobConversationParticipant>();

			participant.JCP_ParticipantTableCode = "OH";
			AssertEquals("Organization", participant.RelatedPartyTypeName);

			participant.RelatedPartyTypeName = "Contact";
			AssertEquals("OC", participant.JCP_ParticipantTableCode);
		}

		public void TestCanFindRelatedContact()
		{
			var firstOrg = Factory.NewWithValidTestData<OrgHeader>();
			firstOrg.OH_Code = "FIRST1";

			var firstContact = Factory.NewWithValidTestData<OrgContact>();
			firstContact.OC_ContactName = "John Smith";
			firstContact.OC_OH = firstOrg.PK;

			var secondOrg = Factory.NewWithValidTestData<OrgHeader>();
			secondOrg.OH_Code = "SECOND";

			var secondContact = Factory.NewWithValidTestData<OrgContact>();
			secondContact.OC_ContactName = "John Smith";
			secondContact.OC_OH = secondOrg.PK;

			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var firstParticipant = provider.eConversation.Participants.AddNewParticipant(firstContact);
			AssertEquals(firstContact, firstParticipant.Parent);

			var secondParticipant = provider.eConversation.Participants.AddNewParticipant(secondContact);
			AssertEquals(secondContact, secondParticipant.Parent);
		}

		public void TestAddsEventsToParent()
		{
			using (SetBizoTypeForPrefix(DummyBaseBusinessObject.Schema.TablePrefix, typeof(DummyConversationProvider)))
			{
				var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
				Factory.Save();

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_FullName = "Bob";
				staff.GS_Code = "XCX";

				var participant = parent.eConversation.Participants.AddNewParticipant(staff);
				Factory.Save();

				AssertHasLog("Should have a log for bob being subscribed", parent, Events.EditedARecord, "Bob has been subscribed");

				participant.JCP_IsSubscribed = false;
				Factory.Save();

				AssertHasLog("Should have a log for bob being UNsubscribed", parent, Events.EditedARecord, "Bob has been unsubscribed");
			}
		}

		IDisposable SetBizoTypeForPrefix(string prefix, Type newType)
		{
			const string prefixMapName = "EnterpriseBusinessObjectPrefixTypes";

			var mock = new Mock<ObjectHandle>();
			mock.Setup(m => m.GetObjectType()).Returns(newType);

			var prefixTypes = (Hashtable)((Hashtable)ObjectFactory.Get(prefixMapName)).Clone();
			prefixTypes[prefix] = mock.Object;

			return ObjectFactory.Substitute("EnterpriseBusinessObjectPrefixTypes", prefixTypes);
		}

		void AssertHasLog(string message, IStmALogParent parent, Event logEvent, string reference)
		{
			var assertionMessage = new StringBuilder(message).AppendLine();
			var logs = parent.Logs;

			assertionMessage.AppendLine("Number of logs: " + logs.DatabaseCount);
			foreach (var log in parent.Logs.Find(new ZQuery()))
			{
				assertionMessage.AppendFormat("{0} - {1}", log.SL_SE_NKEvent, log.SL_Reference).AppendLine();
			}

			Assert(assertionMessage.ToString(), parent.Logs.Find(log => log.SL_SE_NKEvent == logEvent.Code && log.SL_Reference == reference).Any());
		}

		public void TestConversationIsNotifiedWhenAdded()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Bob";
			staff.GS_Code = "XCX";

			var participant = parent.eConversation.Participants.AddNewParticipant(staff);
			Factory.Save();

			var systemMessages = parent.eConversation.Messages.Where(msg => msg.JCM_JCP_Participant.IsEmpty);

			AssertHasMessage("A message should be added when a participant is added", parent.eConversation, null, "Bob has been added to the conversation.");

			Factory.Save();

			AssertHasMessage("We should only add the message when the participant is first added", parent.eConversation, null, "Bob has been added to the conversation.");

			participant.JCP_IsSubscribed = false;
			Factory.Save();

			AssertHasMessage("We should also add a message when a user has unsubscribed", parent.eConversation, null, "Bob has been unsubscribed from the conversation and will no longer receive notifications.");

			participant.JCP_IsSubscribed = true;
			Factory.Save();

			AssertHasMessage("We should also add a message when a user has re-subsribed", parent.eConversation, null, "Bob has been subscribed to the conversation.");

			var systemParticipant = parent.eConversation.Participants.AddNewParticipant((IConversationParticipant)Environment.Env.CurrentUser);
			Factory.Save();

			var systemMessages2 = parent.eConversation.Messages.Where(msg => msg.JCM_JCP_Participant.IsEmpty);
			AssertEquals("no message for system participant", systemMessages.Count(), systemMessages2.Count());
		}

		void AssertHasMessage(string assertionMessage, JobConversation conversation, JobConversationParticipant participant, string body, int amount = 1)
		{
			var participantPk = participant?.PK ?? ZGuid.Empty;
			var messagesFromUser = conversation.Messages.Where(msg => msg.JCM_JCP_Participant == participantPk).Select(msg => msg.Body);
			var messagesSentForAssertionMessage = "Messages from user:\r\n" + string.Join("\r\n", messagesFromUser);

			AssertEquals(assertionMessage + "\r\n" + messagesSentForAssertionMessage, amount, messagesFromUser.Count(msgBody => msgBody == body));
		}

		public void TestConversationIsNotChangedWhenParticipantIsNotAdded()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FriendlyName = "Bob";
			staff.GS_Code = "XCX";

			var participant = parent.eConversation.Participants.AddNewParticipant(staff);

			// Oh no! I accidently added bob when I didnt mean to, better remove him!
			participant.Delete();
			Factory.Save();

			var systemMessages = parent.eConversation.Messages.Where(msg => msg.JCM_JCP_Participant.IsEmpty);
			Assert("A message should ONLY be added when a participant is added", !systemMessages.Any(msg => msg.Body == "Bob has been added to the conversation"));
		}

		public void TestCanDelete_IsInDatabase()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XCX";

			var participant = parent.eConversation.Participants.AddNewParticipant(staff);

			Assert("Can delete since we are unsaved", participant.CanDelete);
			Factory.Save();

			Assert("Can not delete when participant has already been added to the db", !participant.CanDelete);
			AssertEquals("Message should say why we cant delete", "You can not delete existing participants. You may un-subscribe them if you do not wish them to be notified of any further messages.", participant.ReasonForNotAbleToDelete);
		}

		public void TestParentCode_ReadOnlyWhenThereIsMessages()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XCX";

			var participant = parent.eConversation.Participants.AddNewParticipant(staff);
			Assert("PRE: Can modify since we are unsaved and have no messages", !participant.ParentKeyInfo.ReadOnly);

			parent.eConversation.Messages.AddNew(participant, "Now I cant be saved because I added a message");

			Assert("Cannot modify a participant who has messages in the conversation", participant.ParentKeyInfo.ReadOnly);
		}

		public void TestNameIsReadOnlyForEmailParticipant()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant("demo@test.com");

			Assert(participant.ParentKeyInfo.ReadOnly);
			Assert(!participant.EmailAddressInfo.ReadOnly);
		}

		public void TestEmailIsReadOnlyForNonEmailParticipant()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var organisation = conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<OrgHeader>());
			var orgContact = conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<OrgContact>());

			Assert(!organisation.ParentKeyInfo.ReadOnly);
			Assert(organisation.EmailAddressInfo.ReadOnly);

			Assert(!orgContact.ParentKeyInfo.ReadOnly);
			Assert(orgContact.EmailAddressInfo.ReadOnly);
		}

		public void TestCanDelete_HasMessages()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XCX";

			var participant = parent.eConversation.Participants.AddNewParticipant(staff);

			Assert("PRE: Can delete since we are unsaved and have no messages", participant.CanDelete);

			parent.eConversation.Messages.AddNew(participant, "Now I cant be saved because I added a message");

			Assert("Cannot delete a participant who has messages in the conversation", !participant.CanDelete);
			AssertEquals("Message should say why we cant delete", "You can not delete participants who have sent messages. You may un-subscribe them if you do not wish them to be notified of any further messages.", participant.ReasonForNotAbleToDelete);
		}

		public void TestDuplicateParticipantsAreMerged()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();
			var otherFactory = new BusinessObjectFactory();

			AddMessageFromUserInFactory(Factory, parent, staff, "Hi there");
			AddMessageFromUserInFactory(otherFactory, parent, staff, "Hello again");

			Factory.Save();

			try
			{
				otherFactory.Save();
				Fail("Didnt expect it to get this far");
			}
			catch (ZSaveException ex)
			{
				var handler = ex.BusinessObjects
					.OfType<IBusinessObjectInternals>()
					.FirstOrDefault(bizo => bizo.UniqueIndexFailureHandlers.Single().HandledUniqueIndexNames.Contains(ex.IndexNameIfUniqueIndexViolation.ToString()));

				AssertNotNull("Should handle the duplicate participants index", handler);

				var notifications = new Mock<INotificationHandler>();
				handler.UniqueIndexFailureHandlers.Single().NotifyUserAndAttemptToResolve(notifications.Object, handler.UniqueIndexFailureHandlers.Single().HandledUniqueIndexNames.Single());
			}

			AssertEquals("Should merge the partcipants", staff, parent.eConversation.Participants.Single().Parent);
			AssertContainsExactElementsInAnyOrder("Should have both messages", new[] { "Hi there", "Hello again" }, parent.eConversation.Messages.Where(msg => !msg.IsSystemMessage).Select(m => m.Body.ToString()));
		}

		void AddMessageFromUserInFactory(BusinessObjectFactory factory, IConversationProvider parent, IConversationParticipant staff, string message)
		{
			var p = (IConversationProvider)factory.Load(parent.GetType(), ((BusinessObject)parent).PK);
			var participant = p.eConversation.Participants.AddNewParticipant(staff);
			p.eConversation.Messages.AddNew(participant, message);
		}

		public void TestCalculatedProperties_Staff()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			staff.GS_Code = "RBR";
			staff.GS_FullName = "Robert Baratheon";
			staff.GS_GB_HomeBranch = branch.PK;
			staff.GS_Title = "King";

			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant(staff);

			AssertEquals("Should use the full name", "RBR", participant.Parent.Code);
			AssertEquals("Should use the full name", "Robert Baratheon", participant.Parent.Name);
			AssertEquals("We can get location from home branch", "AUSYD", participant.Parent.Location);
			AssertEquals("Should grab the title", "King", participant.Parent.JobTitle);
			AssertEquals("IsActive", true, participant.Parent.IsActive);

			staff.GS_IsActive = false;

			AssertEquals("IsActive", false, participant.Parent.IsActive);
		}

		public void TestCalculatedProperties_Group()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.Branches.Add(branch);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_GC = company.PK;
			group.GG_Code = "STA";
			group.GG_Desc = "Stark Family";

			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant(group);

			AssertEquals("Should use the full name", "STA", participant.Parent.Code);
			AssertEquals("Should use the full name", "Stark Family", participant.Parent.Name);
			AssertEquals("We can get location from home branch", "AUSYD", participant.Parent.Location);
			AssertEquals("IsActive", true, participant.Parent.IsActive);

			group.GG_IsActive = false;

			AssertEquals("IsActive", false, participant.Parent.IsActive);
		}

		public void TestCalculatedProperties_Contact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Northern Houses";
			org.OH_Code = "STARK";

			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_Address1 = "32 Winter St";
			address.OA_City = "Winterfell";
			address.OA_State = "Westeros";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_OH = org.PK;

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Rob Stark";
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_Title = "King of the North";
			contact.OC_OH = org.PK;

			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant(contact);

			AssertEquals("Should use the full name", "Rob Stark (STARK)", participant.Parent.Name);
			AssertEquals("Can grab the org name", "Northern Houses", participant.Parent.OrganisationName);
			AssertEquals("We can get location from home branch", "AUSYD", participant.Parent.Location);
			AssertEquals("Should grab the title", "King of the North", participant.Parent.JobTitle);
			AssertEquals("IsActive", true, participant.Parent.IsActive);

			contact.OC_IsActive = false;

			AssertEquals("IsActive", false, participant.Parent.IsActive);
		}

		public void TestParentFindsCorrectParent()
		{
			var parentsToTest = new BusinessObject[]
			{
				Factory.NewWithValidTestData<OrgContact>(),
				Factory.NewWithValidTestData<OrgHeader>(),
				Factory.NewWithValidTestData<GlbStaff>(),
				Factory.NewWithValidTestData<GlbGroup>(),
			};

			foreach (var parent in parentsToTest)
			{
				var participant = Factory.New<JobConversationParticipant>();
				participant.JCP_ParticipantTableCode = parent.TablePrefix;
				participant.JCP_ParticipantID = parent.PK;

				AssertEquals(parent, participant.Parent);
			}
		}

		public void TestEmailParent()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant("Josephine@b.com");
			AssertEquals("Josephine@b.com", participant.EmailAddress);
			AssertEquals("Josephine@b.com", participant.JCP_EmailAddress);
			AssertEquals("Josephine", participant.Parent.Code);
			AssertEquals("Josephine@b.com", participant.Parent.Email);
			AssertEquals("Josephine", participant.Parent.Name);
			AssertEquals("", participant.JCP_ParticipantTableCode);
		}

		public void TestEmailValidation()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant("a@b.com");
			var participant2 = conversation.Participants.AddNewParticipant("A@b.cOm");
			participant2.RunPreSaveValidation();
			AssertHasErrors("This participant has already been added", participant2.EmailAddressInfo);
			var participant3 = conversation.Participants.AddNewParticipant("c@d.com");
			participant3.RunPreSaveValidation();
			AssertNoErrors(participant3.EmailAddressInfo);
			var participant4 = conversation.Participants.AddNewParticipant("dfhgdfhjg");
			participant4.RunPreSaveValidation();
			AssertHasErrors("Enter a valid Email Address.", participant4.EmailAddressInfo);
		}

		public void TestDuplicateEmailWithContact()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_OH = org.PK;
			contact.OC_Email = "a@b.com";

			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant(contact);
			var participant2 = conversation.Participants.AddNewParticipant("A@b.cOm");
			participant2.RunPreSaveValidation();
			AssertHasErrors("This participant has already been added", participant2.EmailAddressInfo);
			var participant3 = conversation.Participants.AddNewParticipant("c@d.com");
			participant3.RunPreSaveValidation();
			AssertNoErrors(participant3.EmailAddressInfo);
		}
	}
}
