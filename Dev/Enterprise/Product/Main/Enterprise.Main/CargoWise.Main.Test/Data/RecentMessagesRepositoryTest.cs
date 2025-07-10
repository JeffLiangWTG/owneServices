#if !WINZOR
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Data;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.Main.Test.Data;

public class RecentMessagesRepositoryTest : TestCaseWithFactory
{
	JobConversation conversation;
	List<JobConversationMessage> conversationMessages;

	public void TestGetLatestMessages()
	{
		// act
		var messages = new RecentMessagesRepository().GetLatestMessages(10).ToList();

		// assert
		AssertEquals("Should find Recent Messages to the current user", 1, messages.Count);

		AssertEquals("Sender Name", "Contact Name", messages[0].SenderName);
		AssertEquals("Sender Company", "Header", messages[0].SenderCompanyName);
		AssertEquals("Body", "Last message", messages[0].Body);

		AssertCollectionNotContains(messages, m => string.IsNullOrEmpty(m.JobTableCode));
		AssertCollectionNotContains(messages, m => m.JobId == Guid.Empty);
		AssertCollectionNotContains(messages, m => m.Id == Guid.Empty);
	}

	public void TestGetLatestMessages_WhenLimitIsOne()
	{
		// act
		var messages = new RecentMessagesRepository().GetLatestMessages(1).ToList();

		// assert
		AssertEquals("Should limit the amount of Recent Messages", 1, messages.Count);
	}

	public void TestGetLatestMessages_WhenBodyIsLongerThan300Characters()
	{
		// arrange
		foreach (var message in conversationMessages)
		{
			message.JCM_Body = new string('a', 301);
		}

		Factory.Save();

		// act
		var messages = new RecentMessagesRepository().GetLatestMessages(10).ToList();

		// assert
		AssertEquals("Should find Recent Messages to the current user", 1, messages.Count);

		var expected = new string('a', 300) + "...";
		AssertEquals("Body", expected, messages[0].Body);
	}

	public void TestGetLatestMessages_WhenPostedAtDifferentTimes()
	{
		// arrange
		conversationMessages[1].JCM_IsInternal = false;
		conversationMessages[2].JCM_IsInternal = false;

		Factory.Save();

		// act
		var messages = new RecentMessagesRepository().GetLatestMessages(10).ToList();

		// assert
		AssertEquals("Should find Recent Messages to the current user", 1, messages.Count);

		AssertEquals("Posted Time Ago", "30 minutes ago", messages[0].PostedTimeAgo);
	}

	public void TestGetLatestMessages_WhenPostedOnDifferentTimes()
	{
		// arrange
		conversationMessages[2].JCM_PostedTimeUtc = new ZDateTime(2025, 2, 1, 9, 0, 0);
		conversationMessages[0].JCM_PostedTimeUtc = new ZDateTime(2025, 2, 1, 10, 0, 0);
		conversationMessages[1].JCM_PostedTimeUtc = new ZDateTime(2025, 2, 1, 10, 30, 0);

		var lastPostedTime = new ZDateTime(2025, 2, 1, 11, 0, 0);
		conversationMessages[3].JCM_PostedTimeUtc = lastPostedTime;

		Factory.Save();

		// act
		var messages = new RecentMessagesRepository().GetLatestMessages(10).ToList();

		// assert
		AssertEquals("Should find Recent Messages to the current user", 1, messages.Count);

		AssertEquals("(0) PostedTime", lastPostedTime, messages[0].PostedTime);
		AssertEquals("(0) Body", "Last message", messages[0].Body);
		AssertEquals("(0) SenderCompanyName", "Header", messages[0].SenderCompanyName);
		AssertEquals("(0) SenderName", "Contact Name", messages[0].SenderName);
	}

	public void TestGetLatestMessages_WhenInternalMessages()
	{
		// arrange
		foreach (var message in conversationMessages)
		{
			message.JCM_IsInternal = true;
		}

		Factory.Save();

		// act
		var messages = new RecentMessagesRepository().GetLatestMessages(10).ToList();

		// assert
		AssertEquals("Should not find Recent Messages to the current user", 0, messages.Count);
	}

	public void TestGetLatestMessages_WhenSystemMessages()
	{
		// arrange
		foreach (var message in conversationMessages)
		{
			message.JCM_IsSystem = true;
		}

		Factory.Save();

		// act
		var messages = new RecentMessagesRepository().GetLatestMessages(10).ToList();

		// assert
		AssertEquals("Should not find Recent Messages to the current user", 0, messages.Count);
	}

	public void TestGetLatestMessages_WhenBelongsToBlacklistedTable()
	{
		// arrange
		conversation.JCC_ParentTableCode = GlbStaffChangeRequestSchema.Constants.Prefix;

		Factory.Save();

		// act
		var messages = new RecentMessagesRepository().GetLatestMessages(10).ToList();

		// assert
		AssertEquals("Should not find Recent Messages to the current user", 0, messages.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();
		GlbStaff.CurrentUser.GS_IsSystemAccount = false;

		conversation = Factory.NewWithValidTestData<JobConversation>();

		Factory.Save();

		var staff = Factory.NewWithValidTestData<GlbStaff>();
		staff.GS_FullName = "Test User";

		var senderGS = conversation.Participants.AddNewParticipant(staff);
		conversation.Participants.AddNewParticipant(GlbStaff.CurrentUser);
		conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<GlbGroup>());
		var senderOrg = conversation.Participants.AddNewParticipant(Factory.NewWithValidTestData<OrgContact>());

		Factory.Save();

		var orgContact = Factory.Load<OrgContact>(senderOrg.JCP_ParticipantID);
		orgContact.OC_ContactName = "Contact Name";

		Factory.Save();

		// conversation.Messages changes order when updating messages
		conversationMessages = new List<JobConversationMessage>()
		{
			conversation.Messages[0],
			conversation.Messages[1],
			conversation.Messages[2],
			conversation.Messages[3],
		};

		var firstPostedTime = ZDateTime.UtcNow.AddHours(-2);
		conversationMessages[0].JCM_Body = "First message";
		conversationMessages[0].JCM_IsInternal = false;
		conversationMessages[0].JCM_JCP_Participant = senderGS.PK;
		conversationMessages[0].JCM_PostedTimeUtc = firstPostedTime;

		conversationMessages[1].JCM_Body = "Second message";
		conversationMessages[1].JCM_IsInternal = false;
		conversationMessages[1].JCM_JCP_Participant = senderGS.PK;
		conversationMessages[1].JCM_PostedTimeUtc = ZDateTime.UtcNow.AddHours(-1);

		conversationMessages[2].JCM_Body = "Third message";
		conversationMessages[2].JCM_IsInternal = false;
		conversationMessages[2].JCM_JCP_Participant = senderOrg.PK;
		conversationMessages[2].JCM_PostedTimeUtc = ZDateTime.UtcNow.AddMinutes(-50);

		var lastPostedTime = ZDateTime.UtcNow.AddMinutes(-30);
		conversationMessages[3].JCM_Body = "Last message";
		conversationMessages[3].JCM_IsInternal = false;
		conversationMessages[3].JCM_JCP_Participant = senderOrg.PK;
		conversationMessages[3].JCM_PostedTimeUtc = lastPostedTime;

		Factory.Save();
	}

	protected override void TearDown()
	{
		GlbStaff.CurrentUser.GS_IsSystemAccount = true;
		base.TearDown();
	}
}
#endif
