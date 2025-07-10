using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business
{
	[TestedType(typeof(ConversationMessageCollection))]
	sealed class ConversationMessageCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ConversationMessageCollection>
	{
		public void TestConstructorAddsExistingMessages()
		{
			var parent = Factory.NewWithValidTestData<IncidentApproval>();
			var notes = new ConversationNoteCollection(parent, Factory);
			var conversation = new ConversationMessageCollection(notes);

			var first = conversation.AddNew();
			first.Id = ZGuid.NewZGuid();
			first.SentTimeInUtc = ZDateTime.Now;
			first.UserCode = "ANS";
			first.UserName = "Anup Sastry";
			first.Body = "FIRST!!1!";
			first.MessageType = ConversationMessage.MessageTypes.LocalPublished;

			first.SetNoteText();

			Factory.Save();

			var reloaded = new ConversationMessageCollection(notes);

			AssertEquals("Reloaded count", 1, reloaded.Count);
			AssertEquals("Should have the existing message", first.Body, reloaded[0].Body);
			Assert("HasChanges should be false since these notes already exist", !reloaded.HasChanges);
		}

		public void TestHasChangesIsSetFromChildren()
		{
			var parent = Factory.NewWithValidTestData<IncidentApproval>();
			var notes = new ConversationNoteCollection(parent, Factory);
			var conversation = new ConversationMessageCollection(notes);

			Assert("PRE: Nothing new, no changes", !conversation.HasChanges);

			var first = conversation.AddNew();
			first.Id = ZGuid.NewZGuid();
			first.SentTimeInUtc = ZDateTime.Now;
			first.UserCode = "ANS";
			first.UserName = "Anup Sastry";
			first.Body = "FIRST!!1!";
			first.MessageType = ConversationMessage.MessageTypes.LocalPublished;

			Assert("Message was added, HasChanges should be true", conversation.HasChanges);

			first.SetNoteText();
			Factory.Save();

			Assert("Changes saved, HasChanges should be false", !conversation.HasChanges);

			first.Like();

			Assert("Message was modified, HasChanges should be true", conversation.HasChanges);
		}

		public void TestAddNote()
		{
			var parent = Factory.NewWithValidTestData<IncidentApproval>();
			var notes = new ConversationNoteCollection(parent, Factory);
			var conversation = new ConversationMessageCollection(notes);

			Assert("PRE: Nothing new, no changes", !conversation.HasChanges);

			var note = notes.AddNew();
			var msg = conversation.Add(note);

			AssertEquals(note.PK, msg.NotePk);
		}

		protected override ConversationMessageCollection GetCollectionToTest()
		{
			return new ConversationMessageCollection(Notes);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ConversationMessage(Notes.AddNew());
		}

		IncidentApproval parent;
		IncidentApproval Parent => LazyInitializer.EnsureInitialized(ref parent, Factory.NewWithValidTestData<IncidentApproval>);

		ConversationNoteCollection notes;
		ConversationNoteCollection Notes => LazyInitializer.EnsureInitialized(ref notes, () => new ConversationNoteCollection(Parent, Factory));
	}
}
