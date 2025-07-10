using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.CustomerService.Business.Testing
{
	sealed class EConversationTest : TestCaseWithFactory
	{
		public void TestLimitMessages()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var bizo = Factory.New<DummyEnterpriseBusinessObject>();
			var eConversation = new EConversation(bizo, 10);

			var now = ZDateTime.UtcNow;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				for (var i = 0; i < 20; i++)
				{
					eConversation.AddMessageFromLocalUser("Bork bork bork bork. I'm a chicken.");
				}
			}

			AssertEquals(10, eConversation.LocalMessages.Count());
		}

		public void TestTrimTrailingWhiteSpace()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var bizo = Factory.New<DummyEnterpriseBusinessObject>();
			var eConversation = new EConversation(bizo, 10);

			var now = ZDateTime.UtcNow;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				eConversation.AddMessageFromLocalUser(@"

Bork bork bork bork.

");
			}

			var expected = @"

Bork bork bork bork.";

			AssertEquals("It's time to stop accidentally sending blank lines to clients", expected, eConversation.LocalMessages.Single().Body);
		}

		[TestDate(2012, 4, 16, 10, 32, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestAdd()
		{
			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "US1";
			staff1.GS_FullName = "User 1";
			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "US2";
			staff2.GS_FullName = "User 2";
			Factory.Save();

			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);

			ZDateTime time1 = ZDateTime.UtcNow;
			using (Env.SetTemporaryUserContext(staff1.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				eConversation.AddMessageFromLocalUser("test message 1");
			}

			TestDateAttribute.Date = time1.AddMinutes(10).ToDateTime();
			Xsd.ConversationMessageCollection remoteMessages = new Xsd.ConversationMessageCollection();
			remoteMessages.Add(new Xsd.ConversationMessage() { Body = "remote" });
			eConversation.AddMessagesFromRemoteUser(remoteMessages);

			using (Env.SetTemporaryUserContext(staff2.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				eConversation.AddMessageFromLocalUser("test message 2");
			}

			Xsd.ConversationMessageCollection messages = new Xsd.ConversationMessageCollection();
			eConversation.GetNewLocalPublishedMessages(messages);
			AssertEquals("Count", 2, messages.Count);
			var message = messages[0];
			AssertEquals("US1", message.UserCode);
			AssertEquals("User 1", message.UserName);
			AssertEquals("2012-04-16T10:32:00.0000000Z", message.SentTimeInUtc);
			AssertEquals("test message 1", message.Body);

			message = messages[1];
			AssertEquals("US2", message.UserCode);
			AssertEquals("User 2", message.UserName);
			AssertEquals("2012-04-16T10:42:00.0000000Z", message.SentTimeInUtc);
			AssertEquals("test message 2", message.Body);
		}

		[TestDate(2012, 8, 6, 5, 50, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestAddRemoteMessage()
		{
			Xsd.ConversationMessageCollection messages = new Xsd.ConversationMessageCollection();
			messages.Add(new Xsd.ConversationMessage()
			{
				AdditionalNote = "Additional Note 2",
				Body = "Message 2",
				SentTimeInUtc = new ZDateTime(2012, 8, 6, 5, 39, 20).ToString("o"),
				UserCode = "SCW",
				UserName = "Sam"
			});

			messages.Add(new Xsd.ConversationMessage()
			{
				AdditionalNote = "Additional Note 4",
				Body = "Message 4",
				SentTimeInUtc = new ZDateTime(2012, 8, 6, 5, 39, 40).ToString("o"),
				UserCode = "SCW",
				UserName = "Sam"
			});

			messages.Add(new Xsd.ConversationMessage()
			{
				AdditionalNote = "Additional Note 1",
				Body = "Message 1",
				SentTimeInUtc = new ZDateTime(2012, 8, 6, 5, 39, 10).ToString("o"),
				UserCode = "SCW",
				UserName = "Sam"
			});

			messages.Add(new Xsd.ConversationMessage()
			{
				AdditionalNote = "Additional Note 3",
				Body = "Message 3",
				SentTimeInUtc = new ZDateTime(2012, 8, 6, 5, 39, 30).ToString("o"),
				UserCode = "SCW",
				UserName = "Sam"
			});

			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			eConversation.AddMessagesFromRemoteUser(messages);
			Factory.Save();

			eConversation = new EConversation(bizo);
			var messageList = eConversation.GetTimeOrderedMessages();
			ZDateTime expectedSentLocalTime = EnvProxy.Instance.Time.GetLocalTimeFromUtc(ZDateTime.UtcNow.ToDateTime());

			AssertEConversationMessage((ConversationMessage)messageList[0], "SCW", "Sam", expectedSentLocalTime.AddMilliseconds(3), "Message 4", "Additional Note 4", ConversationMessage.MessageTypes.Remote);
			AssertEConversationMessage((ConversationMessage)messageList[1], "SCW", "Sam", expectedSentLocalTime.AddMilliseconds(2), "Message 3", "Additional Note 3", ConversationMessage.MessageTypes.Remote);
			AssertEConversationMessage((ConversationMessage)messageList[2], "SCW", "Sam", expectedSentLocalTime.AddMilliseconds(1), "Message 2", "Additional Note 2", ConversationMessage.MessageTypes.Remote);
			AssertEConversationMessage((ConversationMessage)messageList[3], "SCW", "Sam", expectedSentLocalTime, "Message 1", "Additional Note 1", ConversationMessage.MessageTypes.Remote);
		}

		[TestDate(2012, 8, 6, 5, 50, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestAddRemoteMessage_OldMessages()
		{
			Xsd.ConversationMessageCollection messages = new Xsd.ConversationMessageCollection();
			ZDateTime oldTime = new ZDateTime(2010, 8, 6, 5, 39, 20);
			messages.Add(new Xsd.ConversationMessage()
			{
				Body = "Message Old",
				SentTimeInUtc = oldTime.ToString("o"),
				UserCode = "SCW",
				UserName = "Sam"
			});

			messages.Add(new Xsd.ConversationMessage()
			{
				Body = "Message New",
				SentTimeInUtc = new ZDateTime(2012, 8, 6, 5, 39, 40).ToString("o"),
				UserCode = "SCW",
				UserName = "Sam"
			});

			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			eConversation.AddMessagesFromRemoteUser(messages);
			Factory.Save();

			eConversation = new EConversation(bizo);
			var messageList = eConversation.GetTimeOrderedMessages();

			ZDateTime expectedOldSentTime = EnvProxy.Instance.Time.GetLocalTimeFromUtc(oldTime.ToDateTime());
			ZDateTime expectedNewSentTime = EnvProxy.Instance.Time.GetLocalTimeFromUtc(ZDateTime.UtcNow.ToDateTime());

			AssertEConversationMessage((ConversationMessage)messageList[0], "SCW", "Sam", expectedNewSentTime, "Message New", "", ConversationMessage.MessageTypes.Remote);
			AssertEConversationMessage((ConversationMessage)messageList[1], "SCW", "Sam", expectedOldSentTime, "Message Old", "", ConversationMessage.MessageTypes.Remote);
		}

		public void TestGetNewLocalMessages_ClearedAfterSave()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			eConversation.AddMessageFromLocalUser("hello");

			Xsd.ConversationMessageCollection messages = new Xsd.ConversationMessageCollection();
			eConversation.GetNewLocalPublishedMessages(messages);
			AssertEquals("Count", 1, messages.Count);

			Factory.Save();
			messages = new Xsd.ConversationMessageCollection();
			eConversation.GetNewLocalPublishedMessages(messages);
			AssertEquals("Count", 0, messages.Count);
		}

		[TestDate(2012, 6, 5, 11, 0, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestGetNewLocalMessages_SinceLastUpdate()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			eConversation.AddMessageFromLocalUser("hello");
			eConversation.AddInternalMessageFromLocalUser("no comment");
			eConversation.AddMessage(ZGuid.NewZGuid(), new ZDateTime(2012, 6, 5, 10, 55, 0), "AAA", "User A", "A message added 5 minutes before save");
			Factory.Save();

			Xsd.ConversationMessageCollection messages = new Xsd.ConversationMessageCollection();
			eConversation.GetNewLocalPublishedMessagesSinceLastUpdate(messages, new ZDateTime(2012, 6, 5, 10, 59, 59));
			AssertEquals("Should contain 2 messages", 2, messages.Count);
			AssertEquals("hello", messages[0].Body);
			AssertEquals("A message added 5 minutes before save", messages[1].Body);
		}

		public void TestHasChanges()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			AssertEquals("HasChanges", false, eConversation.HasChanges);

			eConversation.AddMessageFromLocalUser("hi");
			AssertEquals("HasChanges", true, eConversation.HasChanges);

			Factory.Save();
			AssertEquals("HasChanges", false, eConversation.HasChanges);

			eConversation.AddMessageFromLocalUser("there");
			AssertEquals("HasChanges", true, eConversation.HasChanges);

			var factory2 = new BusinessObjectFactory();
			var bizoInFactory2 = factory2.Load<DummyEnterpriseBusinessObject>(bizo.PK);
			var eConversationInFactory2 = new EConversation(bizoInFactory2);
			AssertEquals("HasChanges", false, eConversationInFactory2.HasChanges);
		}

		public void TestIsEmpty()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			AssertEquals("IsEmpty", true, eConversation.IsEmpty);

			eConversation.AddMessageFromLocalUser("hi");
			AssertEquals("IsEmpty", false, eConversation.IsEmpty);

			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var bizoInFactory2 = factory2.Load<DummyEnterpriseBusinessObject>(bizo.PK);
			var eConversationInFactory2 = new EConversation(bizoInFactory2);
			AssertEquals("IsEmpty", false, eConversationInFactory2.IsEmpty);
		}

		public void TestAnyLocalMessageContains()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			AssertEquals(false, eConversation.AnyLocalMessageContains("hi there"));

			Xsd.ConversationMessageCollection remoteMessages = new Xsd.ConversationMessageCollection();
			remoteMessages.Add(new Xsd.ConversationMessage() { Body = "hi there" });
			AssertEquals(false, eConversation.AnyLocalMessageContains("hi there"));

			eConversation.AddMessageFromLocalUser("ignore me");
			AssertEquals(false, eConversation.AnyLocalMessageContains("hi there"));

			eConversation.AddMessageFromLocalUser("well hi there mate");
			AssertEquals(true, eConversation.AnyLocalMessageContains("hi there"));

			eConversation = new EConversation(bizo);
			eConversation.AddInternalMessageFromLocalUser("hi there buddy");
			AssertEquals(true, eConversation.AnyLocalMessageContains("hi there"));

			eConversation = new EConversation(bizo);
			eConversation.AddInternalMessageFromLocalUser("so hi there");
			AssertEquals(true, eConversation.AnyLocalMessageContains("hi there"));
		}

		public void TestAnyNewLocalMessageContains()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			AssertEquals(false, eConversation.AnyNewLocalMessageContains("hi there"));

			Xsd.ConversationMessageCollection remoteMessages = new Xsd.ConversationMessageCollection();
			remoteMessages.Add(new Xsd.ConversationMessage() { Body = "hi there" });
			AssertEquals(false, eConversation.AnyNewLocalMessageContains("hi there"));

			eConversation.AddMessageFromLocalUser("ignore me");
			AssertEquals(false, eConversation.AnyNewLocalMessageContains("hi there"));

			eConversation.AddMessageFromLocalUser("well hi there mate");
			AssertEquals(true, eConversation.AnyNewLocalMessageContains("hi there"));

			Factory.Save();
			AssertEquals(false, eConversation.AnyNewLocalMessageContains("hi there"));

			eConversation = new EConversation(bizo);
			eConversation.AddInternalMessageFromLocalUser("hi there buddy");
			AssertEquals(true, eConversation.AnyNewLocalMessageContains("hi there"));

			Factory.Save();
			AssertEquals(false, eConversation.AnyNewLocalMessageContains("hi there"));

			eConversation = new EConversation(bizo);
			eConversation.AddInternalMessageFromLocalUser("so hi there");
			AssertEquals(true, eConversation.AnyNewLocalMessageContains("hi there"));
		}

		public void TestReload()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);
			Xsd.ConversationMessageCollection remoteMessages = new Xsd.ConversationMessageCollection();
			remoteMessages.Add(new Xsd.ConversationMessage() { Id = ZGuid.NewZGuid().ToString(), Body = "Remote message", MessageSubType = ConversationMessage.MessageSubTypes.UserMessage });
			eConversation.AddMessagesFromRemoteUser(remoteMessages);
			Factory.Save();

			var message = eConversation.GetTimeOrderedMessages()[0];
			message.Like();

			BusinessObjectFactory anotherFactory = new BusinessObjectFactory();
			anotherFactory.RefreshEnabled = false;

			using (GetFactoryIsolater(Factory))
			using (GetFactoryIsolater(anotherFactory))
			{
				DummyEnterpriseBusinessObject bizoInAnotherFactory = anotherFactory.Load<DummyEnterpriseBusinessObject>(bizo.PK);
				EConversation eConversationInAnotherFactory = new EConversation(bizoInAnotherFactory);
				eConversationInAnotherFactory.AddMessageFromLocalUser("Local message");
				anotherFactory.Save();
			}

			eConversation.Reload();
			var messageList = eConversation.GetTimeOrderedMessages();
			AssertEquals("New message is loaded", 2, messageList.Count);
			AssertEquals("Rating value should be kept", true, messageList.Any(msg => msg.Body == "Remote message" && msg.Rating == 1));
		}

		[TestDate(2013, 9, 30, 10, 0, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestGetTimeOrderedMessages()
		{
			DummyEnterpriseBusinessObject bizo = Factory.New<DummyEnterpriseBusinessObject>();
			EConversation eConversation = new EConversation(bizo);

			TestDateAttribute.Date = new DateTime(2013, 9, 30, 10, 0, 0);
			Xsd.ConversationMessageCollection remoteMessages = new Xsd.ConversationMessageCollection();
			remoteMessages.Add(new Xsd.ConversationMessage() { Id = ZGuid.NewZGuid().ToString(), SentTimeInUtc = "2013-09-30T10:10:00.0000000Z", Body = "Remote system log 1", MessageSubType = ConversationMessage.MessageSubTypes.SystemLog });
			remoteMessages.Add(new Xsd.ConversationMessage() { Id = ZGuid.NewZGuid().ToString(), SentTimeInUtc = "2013-09-30T10:20:00.0000000Z", Body = "Remote user message 1", MessageSubType = ConversationMessage.MessageSubTypes.UserMessage });
			remoteMessages.Add(new Xsd.ConversationMessage() { Id = ZGuid.NewZGuid().ToString(), SentTimeInUtc = "2013-09-30T10:30:00.0000000Z", Body = "Remote user message 2", MessageSubType = ConversationMessage.MessageSubTypes.UserMessage });
			eConversation.AddMessagesFromRemoteUser(remoteMessages);

			var messages = eConversation.GetTimeOrderedMessages().Cast<ConversationMessage>().ToList();
			AssertEquals("Not allow rating on remote user message", false, messages[0].CanRate);
			AssertEquals("Not allow ratingrating on remote user message", false, messages[1].CanRate);
			AssertEquals("Not allow rating on remote system log", false, messages[2].CanRate);

			TestDateAttribute.Date = new DateTime(2013, 9, 30, 11, 0, 0);
			eConversation.AddMessageFromLocalUser("Local user message");
			messages = eConversation.GetTimeOrderedMessages().Cast<ConversationMessage>().ToList();
			AssertEquals("Not allow rating on local user message", false, messages[0].CanRate);
			AssertEquals("Not allow rating on remote messages because new local user message is added", false, messages[1].CanRate);
			AssertEquals("Not allow rating on remote messages because new local user message is added", false, messages[2].CanRate);
			AssertEquals("Not allow rating on remote messages because new local user message is added", false, messages[3].CanRate);

			TestDateAttribute.Date = new DateTime(2013, 9, 30, 11, 30, 0);
			remoteMessages = new Xsd.ConversationMessageCollection();
			remoteMessages.Add(new Xsd.ConversationMessage() { Id = ZGuid.NewZGuid().ToString(), SentTimeInUtc = "2013-09-30T11:30:00.0000000Z", Body = "Remote user message 3", MessageSubType = ConversationMessage.MessageSubTypes.UserMessage });

			TestDateAttribute.Date = new DateTime(2013, 9, 30, 11, 45, 0);
			eConversation.AddMessagesFromRemoteUser(remoteMessages);

			messages = eConversation.GetTimeOrderedMessages().Cast<ConversationMessage>().ToList();
			AssertEquals("Not allow rating on new remote user message", false, messages[0].CanRate);

			TestDateAttribute.Date = new DateTime(2013, 9, 30, 12, 0, 0);
			eConversation.AddSystemLogFromLocalUser("Local system log");
			messages = eConversation.GetTimeOrderedMessages().Cast<ConversationMessage>().ToList();
			AssertEquals("Not allow rating on last remote user message even if latest local message is system log", false, messages[1].CanRate);
		}

		void AssertEConversationMessage(ConversationMessage message, ZString userCode, ZString userName, ZDateTime sentTime, ZString body, ZString additionalNote, ZString messageType)
		{
			AssertEquals("User Code", userCode, message.UserCode);
			AssertEquals("User Name", userName, message.UserName);
			AssertEquals("Sent Time", sentTime, message.SendLocalDateTime);
			AssertEquals("Body", body, message.Body);
			AssertEquals("Additional Note", additionalNote, message.AdditionalNote);
			AssertEquals("Message Type", messageType, message.MessageType);
		}
	}
}
