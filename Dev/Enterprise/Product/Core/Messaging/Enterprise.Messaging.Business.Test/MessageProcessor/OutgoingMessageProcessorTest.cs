using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	public class OutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2012, 11, 11)]
		public void TestPackageIntoInterchanges()
		{
			using (BaseOutgoingMessageProcessorTestHelper.SetMessagesPerInterchange(5))
			{
				const string TestApplicationCode = "TST";
				var message1 = CreateMessage(TestApplicationCode, "MSG1");
				var message2 = CreateMessage(TestApplicationCode, "MSG2");

				var message3 = CreateMessage("XXX", "MSG3");
				var message4 = CreateMessage(TestApplicationCode, "MSG4");
				message4.EM_HeldUntilDate = ZDateTime.Now.AddHours(1);

				var message5 = CreateMessage(TestApplicationCode, "MSG5");

				var alternateCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
				var alternateBranch = alternateCompany.Branches[0];
				message5.EM_GB = alternateBranch.PK;

				var message6 = CreateMessage(TestApplicationCode, "MSG6");
				message6.EM_IsActive = false;
				var message7 = CreateMessage(TestApplicationCode, "MSG7");
				var message8 = CreateMessage(TestApplicationCode, "MSG8");
				var message9 = CreateMessage(TestApplicationCode, "MSG9");
				var message10 = CreateMessage(TestApplicationCode, "MSG91");

				Factory.Save();

				using (var processor = new OutgoingMessageProcessorTestClass(new LoggingInformation()))
				{
					processor.ProcessMessage(CancellationToken.None);

					AssertThatMessageHasBeenProcessed(message1);
					AssertThatMessageHasBeenProcessed(message2);
					AssertThatMessageHasBeenProcessed(message10);

					AssertThatMessageHasNotBeenProcessed("wrong application code", message3);
					AssertThatMessageHasNotBeenProcessed("held date in advance", message4);
					AssertThatMessageHasNotBeenProcessed("wrong branch", message5);
					AssertThatMessageHasNotBeenProcessed("message is not active", message6);
				}
			}
		}

		public void TestQueuedMessagesProcessedInOneInstance()
		{
			using (BaseOutgoingMessageProcessorTestHelper.SetMessagesPerInterchange(3))
			{
				const string TestApplicationCode = "TST";
				var message1 = CreateMessage(TestApplicationCode, "MSG1");
				var message2 = CreateMessage(TestApplicationCode, "MSG2");
				var message3 = CreateMessage(TestApplicationCode, "MSG3");
				var message4 = CreateMessage(TestApplicationCode, "MSG4");
				var message5 = CreateMessage(TestApplicationCode, "MSG5");

				Factory.Save();

				var logger = new LoggingInformation();
				using (var processor = new OutgoingMessageProcessorTestClass(logger))
				{
					processor.ProcessMessage(CancellationToken.None);

					var logs = string.Join("\r\n", logger.DebugLogStrings.ToList<string>());
					AssertEquals(@"	3 message(s) have been processed.
	2 message(s) have been processed.", logs);

					AssertThatMessageHasBeenProcessed(message1);
					AssertThatMessageHasBeenProcessed(message2);
					AssertThatMessageHasBeenProcessed(message3);
					AssertThatMessageHasBeenProcessed(message4);
					AssertThatMessageHasBeenProcessed(message5);
				}

				logger.ClearLogs();
				var message6 = CreateMessage(TestApplicationCode, "MSG6");
				var message7 = CreateMessage(TestApplicationCode, "MSG7");
				var message8 = CreateMessage(TestApplicationCode, "MSG8");
				var message9 = CreateMessage(TestApplicationCode, "MSG9");

				Factory.Save();

				using (var processor = new OutgoingMessageProcessorTestWithExceptionClass(logger))
				{
					processor.ProcessMessage(CancellationToken.None);

					AssertThatMessageHasBeenRejectedWithoutInterchangeCreated(message6);
					AssertThatMessageHasBeenRejectedWithoutInterchangeCreated(message7);
					AssertThatMessageHasBeenRejectedWithoutInterchangeCreated(message8);
					AssertThatMessageHasBeenRejectedWithoutInterchangeCreated(message9);
				}
			}
		}

		EDIMessage CreateMessage(string applicationCode, string messageNum)
		{
			var mockMessage = Factory.NewMoq<EDIMessage>();
			mockMessage.Protected()
					   .Setup("GetNumberFountainNumbersAndFillInPlaceHolders");
			var result = mockMessage.Object;
			result.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			result.EM_ApplicationCode = applicationCode;
			result.EM_MessageNum = messageNum;
			result.EM_MessageText = "MESSAGE TEXT FOR " + messageNum;
			result.EM_Status = EDIMessage.Status.Queued;
			return result;
		}

		void AssertThatMessageHasBeenProcessed(EDIMessage message)
		{
			message.Reload();
			var interchange = message.Interchange;
			AssertNotNull(interchange);
			AssertEquals(EDIMessage.Status.Sent, message.EM_Status);
			AssertEquals(message.PK, interchange.ContainedMessages[0].PK);
			AssertEquals(1, interchange.ContainedMessages.Count);
			AssertEquals(message.EM_MessageType, interchange.EI_InterchangeType);
		}

		void AssertThatMessageHasNotBeenProcessed(string notification, EDIMessage message)
		{
			message.Reload();
			var interchange = message.Interchange;
			AssertNull(notification, interchange);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
		}

		void AssertThatMessageHasBeenRejectedWithoutInterchangeCreated(EDIMessage message)
		{
			message.Reload();
			var interchange = message.Interchange;
			AssertNull("No interchange should be created", interchange);
			AssertEquals(EDIMessage.Status.Failed, message.EM_Status);

			var messageNotes = message.GetNotes().GetAllNotes().ToArray<StmNote>();
			AssertEquals(1, messageNotes.Length);
			AssertEquals(InterchangeProviderBase.ProcessingLogDescription, messageNotes[0].ST_Description);
			AssertEquals("Message fail to saving 3 times.", messageNotes[0].ST_NoteText);
		}
	}
}
