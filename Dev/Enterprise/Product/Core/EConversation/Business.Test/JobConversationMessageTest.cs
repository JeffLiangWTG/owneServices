using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	[TestedType(typeof(JobConversationMessage))]
	sealed class JobConversationMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFetchHints()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			for (var i = 0; i < 20; i++)
			{
				var sender = Factory.NewWithValidTestData<GlbStaff>();
				var participant = conversation.Participants.AddNewParticipant(sender);
				conversation.Messages.AddNew(participant, "Hello world x " + i);
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newConvo = newFactory.Load<JobConversation>(conversation.PK);
			AssertNotNull(newConvo.Participants.Count);

			var forceLoadParticipants = string.Join(", ", newConvo.Messages.Select(m => m.SenderDisplayName));

			CombineAssertions(() =>
			{
				AssertEquals("We should be able to fetch all participants at once", 1, newFactory.GetTableHitCount(nameof(JobConversationParticipant)));
				AssertEquals("We should be able to fetch all staff participants at once", 1, newFactory.GetTableHitCount(nameof(GlbStaff)));
			});
		}

		public void TestSender_ReturnsNullWhenSystemMessage()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			var newMessage = conversation.Messages.AddNew();

			AssertNull("Sender should be null since we did not set the sender", newMessage.Sender);
		}

		public void TestAdditionalNoteForDisplay()
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(parent);
			var message = conversation.Messages.AddNew();

			AssertEquals(string.Empty, message.AdditionalNoteForDisplay);

			message.JCM_IsInternal = true;
			AssertEquals("[Internal Only]", message.AdditionalNoteForDisplay);

			message.JCM_IsSystem = true;
			AssertEquals("[Internal System Message]", message.AdditionalNoteForDisplay);

			message.JCM_IsInternal = false;
			AssertEquals("[System Message]", message.AdditionalNoteForDisplay);
		}
	}
}
