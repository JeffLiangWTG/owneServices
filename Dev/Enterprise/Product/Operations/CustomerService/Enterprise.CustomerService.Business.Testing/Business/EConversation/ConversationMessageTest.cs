using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business.Testing
{
	[TestedType(typeof(ConversationMessage))]
	internal sealed class ConversationMessageTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDelete()
		{
			var message = GetNewConversationMessage();
			var note = Factory.Load<ConversationNote>(message.NotePk);

			message.Delete();

			AssertEquals("Deleting a conversation message deletes the note behind it.", true, note.IsDeleted);
		}

		public void TestRating_SetsHasChanges()
		{
			var incident = Factory.NewWithValidTestData<IncidentApproval>();
			incident.EConversation.AddMessageFromLocalUser("Hi there");

			Factory.Save();
			Assert("PRE: HasChanges is false", !incident.HasChanges);

			var message = incident.EConversation.GetTimeOrderedMessages().Single();
			message.Like();

			Assert("Liking should have changed the parent Incidents HasChanges", incident.HasChanges);

			Factory.Save();
			Assert("PRE: HasChanges is false", !incident.HasChanges);

			message.Dislike();
			Assert("Disliking should have changed the parent Incidents HasChanges", incident.HasChanges);
		}

		public void TestIsLocalMessage()
		{
			var message = GetNewConversationMessage();

			message.MessageType = ConversationMessage.MessageTypes.LocalInternal;
			AssertEquals(true, message.IsLocalMessage);

			message.MessageType = ConversationMessage.MessageTypes.LocalPublished;
			AssertEquals(true, message.IsLocalMessage);

			message.MessageType = ConversationMessage.MessageTypes.Remote;
			AssertEquals(false, message.IsLocalMessage);
		}

		public void TestIsLocalPublishedMessage()
		{
			var message = GetNewConversationMessage();

			message.MessageType = ConversationMessage.MessageTypes.LocalInternal;
			AssertEquals(false, message.IsLocalPublishedMessage);

			message.MessageType = ConversationMessage.MessageTypes.LocalPublished;
			AssertEquals(true, message.IsLocalPublishedMessage);

			message.MessageType = ConversationMessage.MessageTypes.Remote;
			AssertEquals(false, message.IsLocalPublishedMessage);
		}

		public void TestIsRemoteMessage()
		{
			var message = GetNewConversationMessage();

			message.MessageType = ConversationMessage.MessageTypes.LocalInternal;
			AssertEquals(false, message.IsRemoteMessage);

			message.MessageType = ConversationMessage.MessageTypes.LocalPublished;
			AssertEquals(false, message.IsRemoteMessage);

			message.MessageType = ConversationMessage.MessageTypes.Remote;
			AssertEquals(true, message.IsRemoteMessage);
		}

		public void TestToXmlString()
		{
			var message = GetNewConversationMessage();
			message.SentTimeInUtc = new ZDateTime(2012, 5, 21, 16, 26, 31);
			message.UserCode = "US1";
			message.UserName = "User 1";
			message.Body = "hello";
			message.MessageType = ConversationMessage.MessageTypes.LocalInternal;
			message.MessageSubType = ConversationMessage.MessageSubTypes.UserMessage;
			message.Id = ZGuid.Missing;
			string xml = message.ToXmlString();
			const string expectedXml = "<Message><SentTimeInUtc>2012-05-21 16:26:31.000</SentTimeInUtc><UserCode>US1</UserCode><UserName>User 1</UserName><MessageType>LIN</MessageType><MessageSubType>USR</MessageSubType><Body>hello</Body><AdditionalNote></AdditionalNote><Id>1111ffff-ffff-ffff-ffff-ffffffffffff</Id></Message>";
			AssertEquals("xml", expectedXml, xml);

			AssertEquals("PRE: Note text should be blank", "", Factory.Load<ConversationNote>(message.NotePk).ST_NoteText);

			message.SetNoteText();

			AssertEquals("note text", expectedXml, Factory.Load<ConversationNote>(message.NotePk).ST_NoteText);
		}

		public void TestConstuctedFromNote()
		{
			var note = Factory.New<ConversationNote>();
			note.ST_NoteText = "<Message><SentTimeInUtc>2012-05-21 16:26:31.000</SentTimeInUtc><UserCode>US1</UserCode><UserName>User 1</UserName><MessageType>LIN</MessageType><MessageSubType>USR</MessageSubType><Body>hello</Body><AdditionalNote></AdditionalNote><Id>1111ffff-ffff-ffff-ffff-ffffffffffff</Id></Message>";

			var message = new ConversationMessage(note);
			AssertEquals("SentTimeInUtc", new ZDateTime(2012, 5, 21, 16, 26, 31), message.SentTimeInUtc);
			AssertEquals("UserCode", "US1", message.UserCode);
			AssertEquals("UserName", "User 1", message.UserName);
			AssertEquals("Body", "hello", message.Body);
			AssertEquals("MessageType", "LIN", message.MessageType);
			AssertEquals("MessageSubType", "USR", message.MessageSubType);
			AssertEquals("Id", ZGuid.Missing, message.Id);
		}

		[TestDate(2009, 1, 2, 13, 20, 20)]
		[TestUtcOffset(8, 0, 0)]
		public void TestSendLocalDateTime()
		{
			ConversationMessage message = GetNewConversationMessage();
			message.SentTimeInUtc = ZDateTime.UtcNow;
			AssertEquals(message.SentTimeInUtc.AddHours(8), message.SendLocalDateTime);

			TestUtcOffsetAttribute.Time = new TimeSpan(11, 0, 0);
			AssertEquals(message.SentTimeInUtc.AddHours(11), message.SendLocalDateTime);
		}

		[ExpectNoExceptions]
		public void TestBodyMaxLength()
		{
			ConversationMessage message = GetNewConversationMessage();
			string bodyString = string.Empty;
			for (int i = 0; i < 8001; i++)
			{
				bodyString += "X";
			}
			message.UserCode = "US1";
			message.UserName = "User 1";
			message.MessageType = ConversationMessage.MessageTypes.LocalInternal;
			message.Body = bodyString;
		}

		public void TestAllowUnicode()
		{
			var message = GetNewConversationMessage();
			message.Body = "你好";
			message.UserName = "程序员";
			message.AdditionalNote = "备忘录";

			AssertNoErrors(message.BodyInfo);
			AssertNoErrors(message.UserNameInfo);
			AssertNoErrors(message.AdditionalNoteInfo);
		}

		#region Like / Dislike

		public void TestRating()
		{
			var note = Factory.New<ConversationNote>();
			ConversationMessage message = new ConversationMessage(note);
			message.Body = "Test rating";
			message.MessageSubType = ConversationMessage.MessageSubTypes.UserMessage;
			message.Id = ZGuid.NewZGuid();
			note.ST_NoteText = message.ToXmlString();
			AssertEquals(0, message.Rating);

			message.Like();
			AssertEquals("Rating increase", 1, message.Rating);
			message.Like();
			AssertEquals("Rating increase", 2, message.Rating);
			message.Like();
			AssertEquals("Rating increase", 3, message.Rating);
			message.Like();
			AssertEquals("Maximum rating is 3", 3, message.Rating);

			message.Dislike();
			AssertEquals("Rating decrease", 2, message.Rating);
			message.Dislike();
			AssertEquals("Rating decrease", 1, message.Rating);
			message.Dislike();
			AssertEquals("Rating decrease", 0, message.Rating);
			message.Dislike();
			AssertEquals("Rating decrease", -1, message.Rating);
			message.Dislike();
			AssertEquals("Rating decrease", -2, message.Rating);
			message.Dislike();
			AssertEquals("Rating decrease", -3, message.Rating);
			message.Dislike();
			AssertEquals("Minimum rating is -3", -3, message.Rating);

			Factory.Save();

			var loadedNote = new BusinessObjectFactory().Load<ConversationNote>(note.PK);
			message = new ConversationMessage(loadedNote);
			AssertEquals(-3, message.Rating);

			AssertEquals("No changes since loaded", false, message.HasRatingChanged);
			message.Like();
			AssertEquals("Has changes", true, message.HasRatingChanged);
			message.Dislike();
			AssertEquals("Still has changes even rating value is the same", true, message.HasRatingChanged);

			loadedNote.Factory.Save();
			AssertEquals("Rating change has been saved", false, message.HasRatingChanged);
		}

		public void TestLogRemoteRatingChange()
		{
			var note = Factory.New<ConversationNote>();
			ConversationMessage message = new ConversationMessage(note);
			message.Body = "Test rating";
			message.MessageSubType = ConversationMessage.MessageSubTypes.UserMessage;
			message.Id = ZGuid.NewZGuid();
			note.ST_NoteText = message.ToXmlString();
			AssertEquals(0, message.Rating);

			message.Like();
			message.Like();
			AssertEquals(2, message.Rating);
			Factory.Save();

			var loadedNote = new BusinessObjectFactory().Load<ConversationNote>(note.PK);
			message = new ConversationMessage(loadedNote);
			AssertEquals(2, message.Rating);

			var remoteRating = new Xsd.ConversationMessageRating();
			remoteRating.Rating = -2;
			remoteRating.UserCode = "SCW";
			remoteRating.UserName = "Samuel";
			message.LogRemoteRatingChange(remoteRating);
			loadedNote.Factory.Save();

			loadedNote = new BusinessObjectFactory().Load<ConversationNote>(note.PK);
			message = new ConversationMessage(loadedNote);
			AssertEquals(-2, message.Rating);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var note = Factory.New<ConversationNote>();
			note.ST_Table = "DummyBizo";
			return new ConversationMessage(note);
		}

		ConversationMessage GetNewConversationMessage()
		{
			return (ConversationMessage)GetNewBusinessObject();
		}

		#endregion
	}
}
