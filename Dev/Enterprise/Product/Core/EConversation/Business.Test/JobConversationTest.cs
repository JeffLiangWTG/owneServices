using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	[TestedType(typeof(JobConversation))]
	public class JobConversationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNextMessageDoesNotAffectHasChanges()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			AssertEquals("Should correctly return the parent", parent.PK, conversation.Parent.PK);

			Factory.Save();

			Assert("PRE: After save the conversation should not be marked as having changes", !conversation.HasChanges);

			conversation.NextMessage = ZBlob.FromUTF8("Literally anything, it doesn't matter");

			Assert("The NextMessage property should not effect the conversations HasChanges or enable Save. The property exists for dummy binding to provide templating features and will not change any persistent values", !conversation.HasChanges);
		}

		public void TestParent()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			AssertEquals("Should correctly return the parent", parent.PK, conversation.Parent.PK);

			Factory.Save();

			var fromAnotherFactory = new BusinessObjectFactory().Load<JobConversation>(conversation.PK);
			AssertEquals("Should return even after a reload", parent.PK, fromAnotherFactory.Parent.PK);

			var noParent = Factory.New<JobConversation>();
			AssertNull("Should just return null when there is no parent", noParent.Parent);
		}

		public void TestCreatingSimaltaniouslyDoesntResultInAnError()
		{
			var factory1 = Factory;
			var parent1 = factory1.NewWithValidTestData<DummyConversationProvider>();

			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var parent2 = factory2.Load<DummyConversationProvider>(parent1.PK);

			var bill = Factory.NewWithValidTestData<GlbStaff>();
			var ben = Factory.NewWithValidTestData<GlbStaff>();

			AddMessage(parent2, ben, "First");
			AddMessage(parent1, bill, "Second");

			factory1.Save();
			factory2.Save();

			var convoPostSave = JobConversation.GetOrCreate(parent1);
			AssertEquals("Should have recognised two identical conversation headers and merged them", 1, Factory.GetDatabaseCount(typeof(JobConversation), new ZQuery(JobConversationSchema.JCC_ParentID, parent1.PK)));
			AssertContainsExactElementsInAnyOrder("Should have merged the messages", new[] { "First", "Second" }, convoPostSave.Messages.Where(msg => !msg.IsSystemMessage).Select(msg => msg.Body.ToString()));

			AssertEquals("Should have merged the participants - no duplicates", 2, convoPostSave.Participants.Count);
			AssertContainsExactElementsInAnyOrder("Should have merged the participants", new[] { bill, ben }, convoPostSave.Participants.Select(participent => participent.Parent));
		}

		public void TestParticipantsAreRegisteredWhenAccessingViews()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = parent.eConversation;
			conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<GlbStaff>());
			conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<GlbGroup>());
			conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<OrgHeader>());

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertModifyingCollectionSetsHasChanges(conversation, nameof(conversation.Staff));
				AssertModifyingCollectionSetsHasChanges(conversation, nameof(conversation.Groups));
				AssertModifyingCollectionSetsHasChanges(conversation, nameof(conversation.RelatedParties));
			});
		}

		void AssertModifyingCollectionSetsHasChanges(JobConversation conversation, string collectionName)
		{
			var factory = new BusinessObjectFactory();
			var convoReloaded = factory.Load<JobConversation>(conversation.PK);
			var collection = (JobConversationParticipantCollection)convoReloaded[collectionName];
			collection[0].JCP_IsSubscribed = !collection[0].JCP_IsSubscribed;

			Assert($"When you modify elements of the {collectionName} collection it should set the parents HasChanges", convoReloaded.HasChanges);
		}

		public void TestRelatedPartiesIsOkWithHavingContactsAndOrgs()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			conversation.RelatedParties.AddNewParticipant(Factory.NewWithValidTestData<OrgContact>());
			conversation.RelatedParties.AddNewParticipant(Factory.NewWithValidTestData<OrgHeader>());

			AssertEquals("Should accept both", 2, conversation.RelatedParties.Count);

			AssertNoExceptionThrown(() =>
			{
				foreach (var participant in conversation.RelatedParties)
				{
					// Just testing it doesnt blow up on iteration
				}
			});
		}

		void AddMessage(IConversationProvider parent, IConversationParticipant sender, string body)
		{
			parent.eConversation.Messages.AddNew(parent.eConversation.Participants.GetOrAdd(sender), body);
		}

		public void TestGetOrCreate()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			AssertEquals("Should use the provided argument", conversation.JCC_ParentID, parent.PK);

			var anotherConversation = JobConversation.GetOrCreate(parent);
			AssertEquals("Should return the existing conversation", conversation.PK, anotherConversation.PK);
		}

		public void TestGetOrCreateHandleMutexLockFail()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			SqlApplicationLock lock1 = null;
			SqlApplicationLock lock2 = null;
			try
			{
				using (var conn2 = Db.NewExtraConnectionToMainDb())
				{
					AssertEquals("Precondition: Should successfully obtain lock", true, conn2.TryGetLock("CreatingNewConversation" + parent.PK, out lock1));
					Assert(lock1.IsHoldingLock());

					AssertEquals("Precondition: Should not be able to obtain lock", false, Db.Connection.TryGetLock("CreatingNewConversation" + parent.PK, out lock2));
					AssertNull(lock2);
					AssertExceptionThrown("Should throw invalid operation exception if it can't obtain the mutex", typeof(InvalidOperationException), () =>
					{
						JobConversation.GetOrCreate(parent);
					});
				}
			}
			finally
			{
				lock1?.Dispose();
				lock2?.Dispose();
			}
		}

		public void TestGetConversation()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			AssertNull(JobConversation.GetConversation(parent));

			var conversationId = Guid.NewGuid();
			var query = string.Format("INSERT INTO dbo.JobConversation (JCC_PK, JCC_ParentTableCode, JCC_ParentID) VALUES ('{0}', '{1}', '{2}')", conversationId, "INC", parent.PK);
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}

			AssertNotNull(JobConversation.GetConversation(parent));
		}

		public void TestCreateWithoutCheckingForExistingConversation_DefaultMessage()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var defaultMsg = "defaultMessage";
			var conversationPk = JobConversation.CreateAndSaveNewConversationOnNewFactory(parent, defaultMsg);
			var conversation = parent.Factory.Load<JobConversation>(conversationPk);

			AssertEquals("Should use the provided argument", conversation.JCC_ParentID, parent.PK);
			Assert(conversation.Messages.First().Body.Contains(defaultMsg));
		}

		public void TestAddMessageFromCurrentUser()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			conversation.AddMessageFromCurrentUser("Hello there", true);

			AssertEquals("Hello there", conversation.Messages.First().JCM_Body);
			AssertEquals("IsInternal", true, conversation.Messages.First().JCM_IsInternal);
			AssertEquals("IsLocal", true, conversation.Messages.First().JCM_IsLocal);
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, conversation.Messages.First().Sender.JCP_ParticipantID);

			Thread.Sleep(100);

			conversation.AddMessageFromCurrentUser("Hello back", false);

			AssertEquals("Hello back", conversation.Messages.First().JCM_Body);
			AssertEquals("IsInternal", false, conversation.Messages.First().JCM_IsInternal);
			AssertEquals("IsLocal", true, conversation.Messages.First().JCM_IsLocal);
			AssertEquals("We shouldnt create another participant when one exists from the first run", conversation.Messages.Last().Sender, conversation.Messages.First().Sender);
		}

		[TestDate(2024, 1, 1)]
		[TestDateIncremental(0, 0, 0, 1)]
		public void TestDependantCollectionsAreLoadedCorrectly()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			var participant = conversation.Participants.AddNewParticipant(user);

			var message = conversation.Messages.AddNew();
			message.JCM_JCP_Participant = participant.PK;
			message.JCM_Body = "Hello there";
			message.JCM_Language = Core.SharedConstants.Languages.English;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedConversation = newFactory.Load<JobConversation>(conversation.PK);

			AssertEquals("Should have the added participant", 1, reloadedConversation.Participants.Count);
			AssertEquals("Should have the added message", 1, reloadedConversation.Messages.Count(msg => !msg.IsSystemMessage));

			AssertEquals("Participant", user.PK, reloadedConversation.Participants[0].JCP_ParticipantID);
			AssertEquals("Message", "Hello there", reloadedConversation.Messages.Last().Body);
		}

		[TestDate(2016, 12, 23)]
		public void TestGetTimeOrderedMessages()
		{
			var conversation = Factory.NewWithValidTestData<JobConversation>();

			// Intentionally created out of order
			var middle = conversation.Messages.AddNew();
			middle.JCM_PostedTimeUtc = ZDateTime.Now.AddDays(-1);
			middle.JCM_Body = nameof(middle);

			var youngest = conversation.Messages.AddNew();
			youngest.JCM_PostedTimeUtc = ZDateTime.Now;
			youngest.JCM_Body = nameof(youngest);

			var oldest = conversation.Messages.AddNew();
			oldest.JCM_PostedTimeUtc = ZDateTime.Now.AddDays(-2);
			oldest.JCM_Body = nameof(oldest);

			var result = conversation.GetTimeOrderedMessages();
			AssertEquals("Youngest message should be first", nameof(youngest), result[0].Body);
			AssertEquals("Second oldest message should be second", nameof(middle), result[1].Body);
			AssertEquals("Oldest message should be last", nameof(oldest), result[2].Body);
		}

		public void TestStaffIncludingCurrentUser()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);

			var firstUser = Factory.NewWithValidTestData<GlbStaff>();
			var firstParticipant = conversation.Participants.AddNewParticipant(firstUser);

			var secondUser = Factory.NewWithValidTestData<GlbStaff>();
			var secondParticipant = conversation.Participants.AddNewParticipant(secondUser);

			Factory.Save();

			using (Env.SetTemporaryUserContext(secondUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("There should be exactly 2 participants to our collection.", 2, conversation.Staff.Count);
				AssertNotNull("The first participant should be in the Staff grid.", conversation.Staff.FindByPK(firstParticipant.PK));
				AssertNotNull("The second participant should be in the Staff grid.", conversation.Staff.FindByPK(secondParticipant.PK));
			}
		}

		public void TestSavingConversationProvider_ShouldSendEmailNotifications_WithOnSavingAction()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider); // otherwise Factory.Load on DummyConversationProviderWithOnSaveNotifications will return a DummyBaseBusinessObject!
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();

			var newParticipant = Factory.NewWithValidTestData<GlbStaff>();
			newParticipant.GS_EmailAddress = "valid@email.address";
			parent.SetSendEmailNotificationsOnSave(true);

			void UpdateConversationOnSavingAction(IConversationProvider provider)
			{
				provider.eConversation.Participants.AddNewParticipant(newParticipant);
			}

			parent.AssignUpdateOnSavingAction(provider => UpdateConversationOnSavingAction(provider));

			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "meat@themenu.boys";

			var participant = conversation.Participants.AddNewParticipant(staff);

			Factory.Save();

			AssertEquals("Our Conversation now has changes, but we haven't created an email notification manually, so none exist.", 0, Env.AllEmailsCreated.Count());
			AssertEquals("Our conversation should have only a singular participant", 1, conversation.Participants.Count);

			conversation.Messages.AddNew(conversation.Participants.First(), "`");

			Factory.Save();

			AssertEquals("Our Conversation now has changes AND a message to send, but we haven't created an email notification manually, so none exist.", 0, Env.AllEmailsCreated.Count());
			AssertEquals("Our conversation should have only a singular participant", 1, conversation.Participants.Count);

			AddMessage(parent, staff, "!");
			Factory.Save();

			var emails = Env.AllEmailsCreated.ToArray();

			AssertEquals("Our ConversationProvider now has changes, so saving our Factory should cause emails to be generated for all conversation participants.", 2, emails.Length);
			CombineAssertions("Our generated emails were the same email, sent to two conversation participants", () =>
			{
				AssertEquals("Same body", emails[0].Body, emails[1].Body);
				AssertNotEquals("Different recipient", emails[0].Recipients[0], emails[1].Recipients[0]);
			});

			AssertEquals("Our conversation should have only a third participant added due to our OnSaving action", 2, conversation.Participants.Count);
		}

		public void TestSavingConversationProvider_ShouldSendEmailNotifications_WithoutOnSavingAction()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider); // otherwise Factory.Load on DummyConversationProviderWithOnSaveNotifications will return a DummyBaseBusinessObject!
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();

			var newParticipant = Factory.NewWithValidTestData<GlbStaff>();
			newParticipant.GS_EmailAddress = "valid@email.address";
			parent.SetSendEmailNotificationsOnSave(true);

			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "meat@themenu.boys";

			var participant = conversation.Participants.AddNewParticipant(staff);

			Factory.Save();

			AssertEquals("Our Conversation now has changes, but we haven't created an email notification manually, so none exist.", 0, Env.AllEmailsCreated.Count());
			AssertEquals("Our conversation should have only a singular participant", 1, conversation.Participants.Count);

			conversation.Messages.AddNew(conversation.Participants.First(), "`");

			Factory.Save();

			AssertEquals("Our Conversation now has changes AND a message to send, but we haven't created an email notification manually, so none exist.", 0, Env.AllEmailsCreated.Count());
			AssertEquals("Our conversation should have only a singular participant", 1, conversation.Participants.Count);

			AddMessage(parent, staff, "!");
			Factory.Save();

			var emails = Env.AllEmailsCreated.ToArray();

			AssertEquals("Our ConversationProvider has no on-saving action, so only one mail will exist.", 1, emails.Length);
			AssertEquals("Our conversation should have one participant", 1, conversation.Participants.Count);
		}

		public void TestSavingConversationProvider_DoesNotSendOnSavingWithoutonSavingFlag()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider); // otherwise Factory.Load on DummyConversationProviderRequiringManualEmailGeneration will return a DummyBaseBusinessObject!
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			parent.SetSendEmailNotificationsOnSave(false);

			var newParticipant = Factory.NewWithValidTestData<GlbStaff>();
			newParticipant.GS_EmailAddress = "valid@email.address";

			void UpdateConversationOnSavingAction(IConversationProvider provider)
			{
				provider.eConversation.Participants.AddNewParticipant(newParticipant);
			}

			parent.AssignUpdateOnSavingAction(provider => UpdateConversationOnSavingAction(provider));

			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "meat@themenu.boys";

			var participant = conversation.Participants.AddNewParticipant(staff);

			Factory.Save();

			AssertEquals("Our Conversation now has changes, but we haven't created an email notification manually, so none exist.", 0, Env.AllEmailsCreated.Count());
			AssertEquals("Our conversation should have only a singular participant", 1, conversation.Participants.Count);

			conversation.Messages.AddNew(conversation.Participants.First(), "`");

			Factory.Save();

			AssertEquals("Our Conversation now has changes AND a message to send, but we haven't created an email notification manually, so none exist.", 0, Env.AllEmailsCreated.Count());
			AssertEquals("Our conversation should have only a singular participant", 1, conversation.Participants.Count);

			AddMessage(parent, staff, "!");
			Factory.Save();

			var emails = Env.AllEmailsCreated.ToArray();

			AssertEquals("Our ConversationProvider now has changes, but we aren't using a IConversationProviderWithOnSavingUpdate, so saving our Factory should cause no emails to be generated for all conversation participants.", 0, emails.Length);
		}

		public void TestOnSaving_BroadcastMessagesShouldSendToBroadcastRecipients()
		{
			var parent = Factory.NewWithValidTestData<WorkTaskRelatedItemProvider>();
			var relatedDummyProvider = Factory.NewWithValidTestData<DummyConversationProvider>();
			var relatedBroadcastRecipient = Factory.NewWithValidTestData<DummyBroadcastRecipient>();
			parent.RelatedItems.Add(relatedDummyProvider);
			parent.RelatedItems.Add(relatedBroadcastRecipient);
			Factory.Save();

			parent.eConversation.AddMessageFromCurrentUser("hiya", false, false, true);
			AssertEquals("Should not have broadcast yet", 0, relatedBroadcastRecipient.eConversation.Messages.Count);
			AssertEquals("Precondition", 0, relatedDummyProvider.eConversation.Messages.Count);

			Factory.Save();

			AssertEquals("Should have broadcast to recipient", 1, relatedBroadcastRecipient.eConversation.Messages.Count);
			AssertEquals("Should not have broadcast since this related item doesn't implement broadcast recipient interface", 0, relatedDummyProvider.eConversation.Messages.Count);
		}

		public void TestOnSaving_BroadcastMessageEmails()
		{
			var parent = Factory.NewWithValidTestData<WorkTaskRelatedItemProvider>();
			var relatedDummyProvider = Factory.NewWithValidTestData<DummyConversationProvider>();
			var relatedBroadcastRecipient = Factory.NewWithValidTestData<DummyBroadcastRecipient>();
			parent.RelatedItems.Add(relatedDummyProvider);
			parent.RelatedItems.Add(relatedBroadcastRecipient);
			Factory.Save();

			parent.eConversation.AddMessageFromCurrentUser("hiya", false, false, true);
			AssertEquals("Should not have broadcast yet", 0, relatedBroadcastRecipient.eConversation.Messages.Count);
			AssertEquals("Precondition", 0, relatedDummyProvider.eConversation.Messages.Count);
			AssertEquals("Should not have created any emails yet", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			Factory.Save();
			AssertEquals("Should have created an email for the broadcast recipient", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Should be populated via interface method", "Update on Dummy", email.Subject);
			AssertEquals("Should be populated via interface method", "Dummy Body", email.Body);
		}

		public void TestGetTextTemplateContextID()
		{
			var jobConversationBusinessObject = (JobConversation)GetNewBusinessObject();
			var bindingMemberNextMessagePlainText = new KBindingMemberInfo("NextMessagePlainText");
			AssertEquals("JobConversation.NextMessage", ((ICustomTextTemplateContext)jobConversationBusinessObject).GetTextTemplateContextID(jobConversationBusinessObject, bindingMemberNextMessagePlainText));

			var bindingMemberNextMessage = new KBindingMemberInfo("NextMessage");
			AssertEquals("JobConversation.NextMessage", ((ICustomTextTemplateContext)jobConversationBusinessObject).GetTextTemplateContextID(jobConversationBusinessObject, bindingMemberNextMessage));

			var bindingMember = new KBindingMemberInfo("blha");
			AssertEquals("JobConversation.blha", ((ICustomTextTemplateContext)jobConversationBusinessObject).GetTextTemplateContextID(jobConversationBusinessObject, bindingMember));
		}

		public void TestHtmlProperty()
		{
			var conversation = Factory.New<JobConversation>();
			AssertEquals(ZBlob.Empty, conversation.NextMessage);
			AssertEquals(ZBlob.Empty, conversation.NextMessage_HTML);

			conversation.NextMessage_HTML = ZBlob.FromUTF8("<p>123</p>");

			AssertEquals(@"{\rtf1\ansi\ansicpg1252\deflang3081\nouicompat\uc0{\fonttbl}{\colortbl}{{123}\par}}", ORtfTextUtil.GeneratorInfoRegex.Replace(conversation.NextMessage.ToUTF8(), string.Empty));
			AssertEquals("<p>123</p>", conversation.NextMessage_HTML.ToUTF8());
		}

		public void TestHtmlFromTextProperty()
		{
			var conversation = Factory.New<JobConversation>();
			AssertEquals(ZBlob.Empty, conversation.NextMessage);
			AssertEquals(ZBlob.Empty, conversation.NextMessage_HTML);

			conversation.NextMessage = ZBlob.FromUTF8("1234\r\n5678");

			AssertEquals("<p>1234</p><p>5678</p>", conversation.NextMessage_HTML.ToUTF8());

			conversation.NextMessage = ZBlob.FromUTF8("{\\rtf1\\test\\ansi\\ansicpg1252\\nouicompat\\deflang3081\r\n{\\*\\generator Riched20 10.0.19041}\\viewkind4\\uc1 \\pard rtf\\par\r\n}\r\n");

			AssertEquals("<p>rtf</p>", conversation.NextMessage_HTML.ToUTF8());
		}

		#region Implementation

		sealed class WorkTaskRelatedItemProvider : DummyConversationProvider, IWorkTaskRelatedItemProvider, IWorkTaskRelatedItemSource
		{
			DummyRelatedItemCollection relatedItems;

			public WorkTaskRelatedItemProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public void PopulateNewRelatedItem(string relatedItemType, IWorkTaskRelatedItem relatedItem)
			{
			}

			WorkTaskRelatedItemCollection IWorkTaskRelatedItemSource.RelatedItems => RelatedItems;

			public IEnumerable<WorkTaskRelatedItemModuleInfo> SupportedRelatedItemModules { get; }
			public ZBool ShowOnlyNonClosedItems { get; set; }
			public FilteredWorkTaskRelatedItemCollection FilteredRelatedItems { get; }
			public ZBool ShouldAddRelatedItemAsParent { get; set; }

			public DummyRelatedItemCollection RelatedItems
			{
				get
				{
					if (relatedItems == null)
					{
						relatedItems = new DummyRelatedItemCollection(this);
					}

					return relatedItems;
				}
			}

			JobConversation conversation;
			public override JobConversation eConversation
			{
				get
				{
					if (conversation == null)
					{
						var newFactory = new BusinessObjectFactory();
						var conversationInNewFactory = newFactory.New<JobConversationForTest>();
						conversationInNewFactory.JCC_ParentID = PK;
						conversationInNewFactory.JCC_ParentTableCode = TablePrefix;
						newFactory.Save();

						conversation = Factory.Load<JobConversationForTest>(conversationInNewFactory.PK);
						RegisterEditableChildObject(conversation);
					}

					return conversation;
				}
			}

			IBusinessObjectCollection IWorkTaskRelatedItemProvider.RelatedItems => RelatedItems;
		}

		public class JobConversationForTest : JobConversation
		{
			public JobConversationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override IWorkTaskRelatedItemProvider GetRelatedItemProvider()
			{
				var provider = Factory.Load<WorkTaskRelatedItemProvider>(Parent.PK);
				return provider;
			}
		}

		public class DummyRelatedItemCollection : WorkTaskRelatedItemCollection
		{
			public DummyRelatedItemCollection(IWorkTaskRelatedItemSource master) : base(master)
			{
			}

			protected override IPivotBusinessObjectCollection[] GetNewPivotCollections(BusinessObject master)
			{
				return new IPivotBusinessObjectCollection[] { new GenPivotCollection(master, Enterprise.Core.Constants.GenPivotTypes.ProcessManagement) };
			}

			protected override bool ShouldAddToCollection(BusinessObject relatedItem) => true;

			protected override void OnAdded(BusinessObject bizOAdded) { }
		}

		sealed class DummyBroadcastRecipient : DummyConversationProvider, IConversationBroadcastRecipient
		{
			public DummyBroadcastRecipient(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			void IConversationBroadcastRecipient.GenerateAndSendBroadcastEmailNotifications()
			{
				var email = new EmailDef { Body = "Dummy Body", Subject = "Update on Dummy", ContentType = EmailContentTypes.HTML };
				email.AddRecipientForUserCommunication("alex@alex.alex");
				Env.OutgoingMailManager.Create(Factory, email);
			}

			JobConversation IConversationBroadcastRecipient.EConversation => eConversation;
		}

		#endregion
	}
}
