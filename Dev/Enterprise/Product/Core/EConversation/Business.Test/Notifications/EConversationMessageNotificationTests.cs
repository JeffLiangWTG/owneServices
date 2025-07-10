using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.EConversation.Testing
{
	[TestedType(typeof(EConversationMessageNotification))]
	sealed class EConversationMessageNotificationTests : NonPersistentBusinessObjectTestCase
	{
		public void TestID()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var conversation = GetTestConversation();
			var notification = new EConversationMessageNotification(conversation, Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, null);

			AssertEquals("ID should be the parent name", conversation.Parent.HumanReadableName, notification.ID);
		}

		public void TestIDWithSubjectContentOverride()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProvider);

			var dummyProvider = Factory.NewWithValidTestData<DummyConversationProvider>();
			dummyProvider.SetEmailSubjectContentOverride("SubjectContentOverride");
			Factory.Save();

			var conversation = JobConversation.GetOrCreate(dummyProvider);
			var notification = new EConversationMessageNotification(conversation, Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, null);

			AssertEquals("ID should be the override subject content", "SubjectContentOverride", notification.ID);
		}

		public void TestBusinessObjectName()
		{
			var conversation = GetTestConversation();
			var notification = new EConversationMessageNotification(conversation, Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, null);

			AssertEquals(conversation.Parent.HumanReadableName, notification.BusinessObjectName);
		}

		public void TestBusinessObjectHyperlink()
		{
			var hyperlink = "http://wisegrid.net/bizo";
			var notification = new EConversationMessageNotification(GetTestConversation(), Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), hyperlink, null);

			AssertEquals(hyperlink, notification.BusinessObjectHyperlink);
		}

		public void TestIsInternalRecipient()
		{
			var noParticipantNotificaton = new EConversationMessageNotification(GetTestConversation(), Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, null);
			Assert(noParticipantNotificaton.IsInternalRecipient);

			var mockParticipant = new Mock<IConversationParticipant>();
			mockParticipant.SetupGet(x => x.IsInternal).Returns(false);
			var notInternalNotification = new EConversationMessageNotification(GetTestConversation(), Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, mockParticipant.Object);
			Assert(!notInternalNotification.IsInternalRecipient);

			var mockInternalParticipant = new Mock<IConversationParticipant>();
			mockInternalParticipant.SetupGet(x => x.IsInternal).Returns(true);
			var internalNotification = new EConversationMessageNotification(GetTestConversation(), Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, mockInternalParticipant.Object);
			Assert(internalNotification.IsInternalRecipient);
		}

		[TestDate(2021, 1, 6)]
		public void TestUtcOffset()
		{
			var utcOffset = ZDateTimeOffset.Now.ToDateTimeOffset();
			var notification = new EConversationMessageNotification(GetTestConversation(), Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, null);

			AssertEquals(Env.Time.FormatOffset(utcOffset.Offset), notification.UtcOffset);
		}

		[TestDate(2021, 1, 6)]
		public void TestNewMessages()
		{
			var now = ZDateTime.UtcNow;

			var sender1 = "Sender 1";
			var message1 = "New Message 1";
			var time1 = now;
			var sender2 = "Sender 2";
			var message2 = "New Message 2";
			var time2 = now.AddMinutes(-1);

			var mockMessage1 = new Mock<IConversationMessage>();
			mockMessage1.SetupGet(x => x.SenderDisplayName).Returns(sender1);
			mockMessage1.SetupGet(x => x.Body).Returns(message1);
			mockMessage1.SetupGet(x => x.SystemCreateTimeInUtc).Returns(time1);
			var mockMessage2 = new Mock<IConversationMessage>();
			mockMessage2.SetupGet(x => x.SenderDisplayName).Returns(sender2);
			mockMessage2.SetupGet(x => x.Body).Returns(message2);
			mockMessage2.SetupGet(x => x.SystemCreateTimeInUtc).Returns(time2);

			var notification = new EConversationMessageNotification(GetTestConversation(), new[] { mockMessage1.Object, mockMessage2.Object }, Enumerable.Empty<IConversationMessage>(), string.Empty, null);

			var localTime1 = Env.Time.GetLocalTimeFromUtc(time1.ToDateTime());
			var expectedTime1 = new ZDateTime(localTime1, DateTimeKind.Local).ToSmallDateTime();
			var localTime2 = Env.Time.GetLocalTimeFromUtc(time2.ToDateTime());
			var expectedTime2 = new ZDateTime(localTime2, DateTimeKind.Local).ToSmallDateTime();

			var expectedNewMessages = $@"
<h2>New Messages</h2>
<table>
<tr>
<td align=""right"" style=""font-weight: bold; width: 120px; padding: 5px 10px 5px 5px;"">
{sender1}
</td>
<td style=""border: solid 1px #A9A9A9; border-radius: 10px; padding: 5px 5px; width: 440px; background-color: #F5F5F5;"">
<table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" height=""100%"" bgcolor=""#F5F5F5"" style=""background-color: #F5F5F5;"">
<tr width=""100%""><td style=""background-color: #F5F5F5;"">{message1}</td></tr>

<tr width=""100%""><td align=""right"" style=""color: #696969;background-color: #F5F5F5;""><em style=""font-size: 10px"">{expectedTime1}</em></td></tr>
</table>
</td>
</tr>
<tr>
<td align=""right"" style=""font-weight: bold; width: 120px; padding: 5px 10px 5px 5px;"">
{sender2}
</td>
<td style=""border: solid 1px #A9A9A9; border-radius: 10px; padding: 5px 5px; width: 440px; background-color: #F5F5F5;"">
<table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" height=""100%"" bgcolor=""#F5F5F5"" style=""background-color: #F5F5F5;"">
<tr width=""100%""><td style=""background-color: #F5F5F5;"">{message2}</td></tr>

<tr width=""100%""><td align=""right"" style=""color: #696969;background-color: #F5F5F5;""><em style=""font-size: 10px"">{expectedTime2}</em></td></tr>
</table>
</td>
</tr>
</table>";

			AssertEqualsIgnoringNewLines(expectedNewMessages, notification.NewMessages);
		}

		public void TestPreviousMessages_NoMessages()
		{
			var notification = new EConversationMessageNotification(GetTestConversation(), Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, null);
			var expectedPreviousMessages = "<p>There are no previous messages</p>";

			AssertEqualsIgnoringNewLines(expectedPreviousMessages, notification.PreviousMessages);
		}

		[TestDate(2021, 1, 6)]
		public void TestPreviousMessages()
		{
			var now = ZDateTime.UtcNow;

			var sender1 = "Sender 1";
			var message1 = "Previous Message 1";
			var time1 = now;
			var sender2 = "Sender 2";
			var message2 = "Previous Message 2";
			var time2 = now.AddMinutes(-1);

			var mockMessage1 = new Mock<IConversationMessage>();
			mockMessage1.SetupGet(x => x.SenderDisplayName).Returns(sender1);
			mockMessage1.SetupGet(x => x.Body).Returns(message1);
			mockMessage1.SetupGet(x => x.SystemCreateTimeInUtc).Returns(time1);
			var mockMessage2 = new Mock<IConversationMessage>();
			mockMessage2.SetupGet(x => x.SenderDisplayName).Returns(sender2);
			mockMessage2.SetupGet(x => x.Body).Returns(message2);
			mockMessage2.SetupGet(x => x.SystemCreateTimeInUtc).Returns(time2);

			var notification = new EConversationMessageNotification(GetTestConversation(), Enumerable.Empty<IConversationMessage>(), new[] { mockMessage1.Object, mockMessage2.Object }, string.Empty, null);

			var localTime1 = Env.Time.GetLocalTimeFromUtc(time1.ToDateTime());
			var expectedTime1 = new ZDateTime(localTime1, DateTimeKind.Local).ToSmallDateTime();
			var localTime2 = Env.Time.GetLocalTimeFromUtc(time2.ToDateTime());
			var expectedTime2 = new ZDateTime(localTime2, DateTimeKind.Local).ToSmallDateTime();

			var expectedPreviousMessages = $@"
<h2>Previous Messages</h2>
<table>
<tr>
<td align=""right"" style=""font-weight: bold; width: 120px; padding: 5px 10px 5px 5px;"">
{sender1}
</td>
<td style=""border: solid 1px #A9A9A9; border-radius: 10px; padding: 5px 5px; width: 440px; background-color: #F5F5F5;"">
<table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" height=""100%"" bgcolor=""#F5F5F5"" style=""background-color: #F5F5F5;"">
<tr width=""100%""><td style=""background-color: #F5F5F5;"">{message1}</td></tr>

<tr width=""100%""><td align=""right"" style=""color: #696969;background-color: #F5F5F5;""><em style=""font-size: 10px"">{expectedTime1}</em></td></tr>
</table>
</td>
</tr>
<tr>
<td align=""right"" style=""font-weight: bold; width: 120px; padding: 5px 10px 5px 5px;"">
{sender2}
</td>
<td style=""border: solid 1px #A9A9A9; border-radius: 10px; padding: 5px 5px; width: 440px; background-color: #F5F5F5;"">
<table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" height=""100%"" bgcolor=""#F5F5F5"" style=""background-color: #F5F5F5;"">
<tr width=""100%""><td style=""background-color: #F5F5F5;"">{message2}</td></tr>

<tr width=""100%""><td align=""right"" style=""color: #696969;background-color: #F5F5F5;""><em style=""font-size: 10px"">{expectedTime2}</em></td></tr>
</table>
</td>
</tr>
</table>";
			AssertEqualsIgnoringNewLines(expectedPreviousMessages, notification.PreviousMessages);
		}

		public void TestAddFormattedMessages_CorrectNewLineFormatting()
		{
			var sender = "Sender 1";
			var message = "This\r is\n a\r\n test message with a new line";

			var mockMessage = new Mock<IConversationMessage>();
			mockMessage.SetupGet(x => x.SenderDisplayName).Returns(sender);
			mockMessage.SetupGet(x => x.Body).Returns(message);
			mockMessage.SetupGet(x => x.SystemCreateTimeInUtc).Returns(ZDateTime.UtcNow);

			var notification = new EConversationMessageNotification(GetTestConversation(), Enumerable.Empty<IConversationMessage>(), new[] { mockMessage.Object }, string.Empty, null);

			message = "This\r is<br />\n a<br />\r\n test message with a new line";
			var expectedPreviousMessages = $@"<h2>Previous Messages</h2>
<table>
<tr>
<td align=""right"" style=""font-weight: bold; width: 120px; padding: 5px 10px 5px 5px;"">
{sender}
</td>
<td style=""border: solid 1px #A9A9A9; border-radius: 10px; padding: 5px 5px; width: 440px; background-color: #F5F5F5;"">
<table border=""0"" cellspacing=""0"" cellpadding=""0"" width=""100%"" height=""100%"" bgcolor=""#F5F5F5"" style=""background-color: #F5F5F5;"">
<tr width=""100%""><td style=""background-color: #F5F5F5;"">{message}</td></tr>{"\n"}";
			AssertStartsWith("Newline and/or related characters were not formated correctly.", expectedPreviousMessages, notification.PreviousMessages);
		}

		public void TestEmailIdentifier()
		{
			var conversation = GetTestConversation();
			var notification = new EConversationMessageNotification(conversation, Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, null);
			var identifier = $"<div name=\"EConversationIdentifier\" id=\"{conversation.Parent.TablePrefix}|{conversation.Parent.PK}\">Please don't edit the content of this email as your message may not be processed</div>";

			AssertEqualsIgnoringNewLines(identifier, notification.EmailIdentifier);
		}

		void AssertEqualsIgnoringNewLines(string expected, string actual)
		{
			var expectedTrim = Regex.Replace(expected, @"\t|\n|\r", string.Empty);
			var actualTrim = Regex.Replace(actual, @"\t|\n|\r", string.Empty);

			AssertEquals(expectedTrim, actualTrim);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var conversation = GetTestConversation();

			return new EConversationMessageNotification(conversation, Enumerable.Empty<IConversationMessage>(), Enumerable.Empty<IConversationMessage>(), string.Empty, new Mock<IConversationParticipant>().Object);
		}

		JobConversation GetTestConversation()
		{
			var dummyProvider = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();
			return JobConversation.GetOrCreate(dummyProvider);
		}
	}
}
