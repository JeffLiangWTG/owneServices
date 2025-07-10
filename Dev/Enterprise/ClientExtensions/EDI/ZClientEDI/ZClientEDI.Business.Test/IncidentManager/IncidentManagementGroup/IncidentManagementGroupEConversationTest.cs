using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class IncidentManagementGroupEConversationTest : TestCaseWithFactory
	{
		public void TestCreateEConversation()
		{
			var group = GetNewIncidentManagementGroup();
			Assert(!group.IsInDatabase);
			var expectConversation = group.EConversation.Conversation;
			AssertNotNull(expectConversation);

			AssertEquals("Conversation should be keeping same", expectConversation, group.EConversation.Conversation);
		}

		public void TestConversation()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			Factory.Save();

			var groupInAnotherFactory = new BusinessObjectFactory().Load<IncidentManagementGroup>(group.PK);

			AssertEquals(group.EConversation.Conversation.PK, groupInAnotherFactory.EConversation.Conversation.PK);
		}

		public void TestMessageCountChanged()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();

			var counter = 0;
			var conversation = group.EConversation;
			conversation.MessageCountChanged += (object sender, EventArgs e) =>
			{
				counter++;
			};

			conversation.AddMessageFromCurrentUser("NTZ", true, true);
			AssertEquals(1, counter);

			Factory.Saving += factory =>
			{
				conversation.AddMessageFromCurrentUser("test", false, false);
				AssertEquals("not called during transaction", 1, counter);
			};

			Factory.Save();
			AssertEquals("one call after transaction", 2, counter);
		}

		public void TestAddMessageFromCurrentUser()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var group = GetNewIncidentManagementGroup();

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				group.EConversation.AddMessageFromCurrentUser("stuff", false, false);
			}

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				var participant1 = group.EConversation.Conversation.Participants.FirstOrDefault();
				participant1.JCP_IsSubscribed = false;
				Factory.Save();

				participant1.JCP_IsSubscribed = true;

				AssertEquals("the number of Participants should be only one", 1, group.EConversation.Conversation.Participants.Count);
				group.EConversation.AddMessageFromCurrentUser("stuff_2", false, false);
				AssertEquals("the number of Participants changed", 2, group.EConversation.Conversation.Participants.Count);
				AssertEquals("Participant's new status should not be covered by old data", true, participant1.JCP_IsSubscribed);
			}
		}

		IncidentManagementGroup GetNewIncidentManagementGroup()
		{
			var group = Factory.NewWithValidTestData<IncidentManagementGroup>();
			return group;
		}
	}
}
