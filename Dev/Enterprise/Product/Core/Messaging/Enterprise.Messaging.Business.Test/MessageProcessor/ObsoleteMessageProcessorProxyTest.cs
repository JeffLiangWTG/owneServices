using System;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.InterchangeProviders;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	class ObsoleteMessageProcessorProxyTest : TestCaseWithFactory
	{
		[TestDate(2021, 01, 20)]
		public void TestProcessObsoleteMessages()
		{
			var originalValue = Env.Registry.MessagesPerInterchange;

			using (new DisposableAction(() => Env.Registry.MessagesPerInterchange = 1, () => Env.Registry.MessagesPerInterchange = originalValue))
			{
				var applicationCode = "TST";
				var now = ZDateTime.Now;

				var message1 = CreateMessage(applicationCode, "MSG1", now);
				var message2 = CreateMessage(applicationCode, "MSG2", now.AddDays(-2));
				var message3 = CreateMessage(applicationCode, "MSG3", now.AddDays(-5));
				var message4 = CreateMessage(applicationCode, "MSG4", now.AddDays(-10));
				var message5 = CreateMessage(applicationCode, "MSG5", now.AddDays(-100));

				message4.EM_Status = "LEG";

				Factory.Save();

				var logger = new LoggingInformation();
				using (var processor = new OutgoingMessageProcessorTestClass(logger))
				{
					var timeSpan = new TimeSpan(5, 0, 0, 0, 0);

					var proxy = new ObsoleteMessageProcessorProxy(processor);
					proxy.Process(CancellationToken.None, "LEG", timeSpan);

					var createTime = ZDateTime.Now.Add(-timeSpan);

					message1.Reload();
					message2.Reload();
					message3.Reload();
					message4.Reload();
					message5.Reload();

					CombineAssertions(() =>
					{
						Assert(logger.DebugLogStrings.ToList<string>().Any(c => c.Contains("2 legacy message(s) have been processed, the new status is 'LEG'.")));
						AssertEquals("Should not be changed as it's not obsolete.", EDIMessage.Status.Queued, message1.EM_Status);
						AssertEquals("Should not be changed as it's not obsolete.", EDIMessage.Status.Queued, message2.EM_Status);
						AssertEquals("Should be changed to 'LEG' as it's an obsolete message.", "LEG", message3.EM_Status);
						AssertEquals("Should not be changed as its status is LEG.", "LEG", message4.EM_Status);
						AssertEquals("Should be changed to 'LEG' as it's an obsolete message.", "LEG", message5.EM_Status);

						var expectedNote = $"It's marked as a legacy message because it was created before {createTime}.";

						AssertEquals("Should create a note for explain why the message was marked as legacy.", expectedNote, message3.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription).Single().ST_NoteText);
						AssertEquals("Should create a note for explain why the message was marked as legacy.", expectedNote, message5.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription).Single().ST_NoteText);
						Assert("Should not be created as its status is LEG.", !message4.Notes.FindByDescription(InterchangeProviderBase.ProcessingLogDescription).Any());
					});
				}
			}
		}

		EDIMessage CreateMessage(string applicationCode, string messageNum, ZDateTime createTime)
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
			result.EM_SystemCreateTimeUtc = createTime;

			return result;
		}
	}
}
