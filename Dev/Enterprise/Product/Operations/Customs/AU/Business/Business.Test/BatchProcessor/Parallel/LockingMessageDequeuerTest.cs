using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class LockingMessageDequeuerTest : TestCaseWithFactory
	{
		public void TestDequeueMessages_AllKeysAreDifferent()
		{
			var messages = CreateMessages(50);

			var lockMechanismFactory = new LockMechanismForTestFactory();
			var keySetExtractor = new KeySetExtractorForTest(mn => mn);
			var logger = new LoggingInformation();

			var dequeuer1 = new LockingMessageDequeuer(lockMechanismFactory.Create("d1"), logger, keySetExtractor);
			var dequeuer2 = new LockingMessageDequeuer(lockMechanismFactory.Create("d2"), logger, keySetExtractor);
			var dequeuer3 = new LockingMessageDequeuer(lockMechanismFactory.Create("d3"), logger, keySetExtractor);

			var query = CreateQuery(10);
			using (var batch1 = dequeuer1.DequeueMessages(Factory, query))
			using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
			using (var batch3 = dequeuer3.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(10)), ToNumberAndPK(batch1));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(10).Take(10)), ToNumberAndPK(batch2));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(20).Take(10)), ToNumberAndPK(batch3));
			}
		}

		public void TestDequeueMessages_2Keys()
		{
			var messages = CreateMessages(50);

			var lockMechanismFactory = new LockMechanismForTestFactory();
			var keySetExtractor = new KeySetExtractorForTest(mn => (mn / 12) % 2);
			var logger = new LoggingInformation();

			var dequeuer1 = new LockingMessageDequeuer(lockMechanismFactory.Create("d1"), logger, keySetExtractor);
			var dequeuer2 = new LockingMessageDequeuer(lockMechanismFactory.Create("d2"), logger, keySetExtractor);
			var dequeuer3 = new LockingMessageDequeuer(lockMechanismFactory.Create("d3"), logger, keySetExtractor);

			var query = CreateQuery(10);
			using (var batch1 = dequeuer1.DequeueMessages(Factory, query))
			using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
			using (var batch3 = dequeuer3.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(10)), ToNumberAndPK(batch1));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(12).Take(10)), ToNumberAndPK(batch2));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(int.MaxValue)), ToNumberAndPK(batch3));
			}
		}

		public void TestDequeueMessages_3Keys()
		{
			var messages = CreateMessages(50);

			var lockMechanismFactory = new LockMechanismForTestFactory();
			var keySetExtractor = new KeySetExtractorForTest(mn => (mn / 12) % 3);
			var logger = new LoggingInformation();

			var dequeuer1 = new LockingMessageDequeuer(lockMechanismFactory.Create("d1"), logger, keySetExtractor);
			var dequeuer2 = new LockingMessageDequeuer(lockMechanismFactory.Create("d2"), logger, keySetExtractor);
			var dequeuer3 = new LockingMessageDequeuer(lockMechanismFactory.Create("d3"), logger, keySetExtractor);

			var query = CreateQuery(10);
			using (var batch1 = dequeuer1.DequeueMessages(Factory, query))
			using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
			using (var batch3 = dequeuer3.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(10)), ToNumberAndPK(batch1));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(12).Take(10)), ToNumberAndPK(batch2));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(24).Take(10)), ToNumberAndPK(batch3));
			}
		}

		public void TestQueryParametersAreUsed()
		{
			var messages = CreateMessages(50);

			var lockMechanismFactory = new LockMechanismForTestFactory();
			var keySetExtractor = new KeySetExtractorForTest(mn => mn);
			var logger = new LoggingInformation();

			var dequeuer = new LockingMessageDequeuer(lockMechanismFactory.Create("d1"), logger, keySetExtractor);

			var query = CreateQuery(10);
			using (var batch = dequeuer.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(10)), ToNumberAndPK(batch));
			}

			query.MaximumRows = 5;
			using (var batch = dequeuer.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(5)), ToNumberAndPK(batch));
			}

			query.AddToFilter(EDIMessageSchema.EM_MessageNum, SQLComparisonOperator.GreaterThanOrEqualTo, "000025");
			using (var batch = dequeuer.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(25).Take(5)), ToNumberAndPK(batch));
			}

			query.OrderBy = $"{EDIMessageSchema.EM_MessageNum.Name} DESC";
			using (var batch = dequeuer.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(25).Reverse().Take(5)), ToNumberAndPK(batch));
			}
		}

		public void TestDeepScan()
		{
			var messages = CreateMessages(200);

			var lockMechanismFactory = new LockMechanismForTestFactory();
			var keySetExtractor1 = new KeySetExtractorForTest(mn => (mn / 150));
			var keySetExtractor2 = new KeySetExtractorForTest(mn => (mn / 150));
			var logger1 = new LoggingInformation();
			var logger2 = new LoggingInformation();

			var dequeuer1 = new LockingMessageDequeuer(lockMechanismFactory.Create("d1"), logger1, keySetExtractor1);
			var dequeuer2 = new LockingMessageDequeuer(lockMechanismFactory.Create("d2"), logger2, keySetExtractor2);

			var query = CreateQuery(10);
			dequeuer1.MinQuickScanMessageCount = 100;
			dequeuer2.MinQuickScanMessageCount = 100;
			dequeuer1.KeyGenerationBatchSize = 1000;
			dequeuer2.KeyGenerationBatchSize = 1000;

			using (var batch1 = dequeuer1.DequeueMessages(Factory, query))
			using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(10)), ToNumberAndPK(batch1));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(150).Take(10)), ToNumberAndPK(batch2));

				AssertEquals(100, keySetExtractor1.ExtractionCount);
				AssertEquals(200, keySetExtractor2.ExtractionCount);
			}

			dequeuer1.MinQuickScanMessageCount = 1000;
			dequeuer2.MinQuickScanMessageCount = 1000;
			dequeuer1.KeyGenerationBatchSize = 1000;
			dequeuer2.KeyGenerationBatchSize = 1000;
			keySetExtractor1.ExtractionCount = 0;
			keySetExtractor2.ExtractionCount = 0;

			using (var batch1 = dequeuer1.DequeueMessages(Factory, query))
			using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(10)), ToNumberAndPK(batch1));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(150).Take(10)), ToNumberAndPK(batch2));

				AssertEquals(200, keySetExtractor1.ExtractionCount);
				AssertEquals(200, keySetExtractor2.ExtractionCount);
			}

			dequeuer1.MinQuickScanMessageCount = 10;
			dequeuer2.MinQuickScanMessageCount = 10;
			dequeuer1.KeyGenerationBatchSize = 5;
			dequeuer2.KeyGenerationBatchSize = 5;
			keySetExtractor1.ExtractionCount = 0;
			keySetExtractor2.ExtractionCount = 0;

			using (var batch1 = dequeuer1.DequeueMessages(Factory, query))
			using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(10)), ToNumberAndPK(batch1));
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Skip(150).Take(10)), ToNumberAndPK(batch2));

				AssertEquals(10, keySetExtractor1.ExtractionCount);
				AssertEquals(160, keySetExtractor2.ExtractionCount);
			}
		}

		public void TestBatchKeepsLock()
		{
			var messages = CreateMessages(15);

			var lockMechanismFactory = new LockMechanismForTestFactory();
			var keySetExtractor = new KeySetExtractorForTest(mn => mn);
			var logger = new LoggingInformation();

			var dequeuer1 = new LockingMessageDequeuer(lockMechanismFactory.Create("d1"), logger, keySetExtractor);
			var dequeuer2 = new LockingMessageDequeuer(lockMechanismFactory.Create("d2"), logger, keySetExtractor);

			var query = CreateQuery(5);
			using (var batch1 = dequeuer1.DequeueMessages(Factory, query))
			using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements(ToNumberAndPK(messages.Take(5)), ToNumberAndPK(batch1));
				AssertArrayEqualsByElements("first messages are locked", ToNumberAndPK(messages.Skip(5).Take(5)), ToNumberAndPK(batch2));
			}

			using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
			{
				AssertArrayEqualsByElements("first messages are available once batch is disposed", ToNumberAndPK(messages.Take(5)), ToNumberAndPK(batch2));
			}
		}

		public void TestBrokenKeySetExtractor()
		{
			var messages = CreateMessages(15);

			var lockMechanismFactory = new LockMechanismForTestFactory();
			var keySetExtractor = new BrokenKeySetExtractorForTest();
			var logger = new LoggingInformation();

			var dequeuer1 = new LockingMessageDequeuer(lockMechanismFactory.Create("d1"), logger, keySetExtractor);
			var dequeuer2 = new LockingMessageDequeuer(lockMechanismFactory.Create("d2"), logger, keySetExtractor);
			dequeuer1.KeyGenerationBatchSize = 1;

			var query = CreateQuery(5);
			using (var batch1 = dequeuer1.DequeueMessages(Factory, query))
			{
				AssertEquals("exception should be reported once", 1, ExceptionReporterTestListener.Instance.Count);
				AssertEquals("This key set extractor is broken.", ExceptionReporterTestListener.Instance[0].Message);

				AssertEquals("exception is logged once", 1, logger.Logs.Count(l => l.Message.Contains("This key set extractor is broken.")));
				var logMessage = logger.Logs.First(l => l.Message.Contains("This key set extractor is broken."));
				AssertEquals(LogType.Error, logMessage.Type);
				AssertContains($"Failed to extract keys for message (PK={messages[0].PK}).", logMessage.Message);
				AssertContains("System.InvalidOperationException: This key set extractor is broken.", logMessage.Message);
				AssertContains(nameof(BrokenKeySetExtractorForTest), logMessage.Message);

				using (var batch2 = dequeuer2.DequeueMessages(Factory, query))
				{
					AssertEquals("exception is logged once", 1, logger.Logs.Count(l => l.Message.Contains("This key set extractor is broken.")));
					AssertEquals("exception should be reported once for all instances", 1, ExceptionReporterTestListener.Instance.Count);

					AssertArrayEqualsByElements("messages were dequeued for unknown key", ToNumberAndPK(messages.Take(5)), ToNumberAndPK(batch1));
					AssertArrayEqualsByElements("all messages share same unknown key", ToNumberAndPK(messages.Skip(int.MaxValue)), ToNumberAndPK(batch2));
				}
			}

			ExceptionReporterTestListener.Instance.Clear();
		}

		string[] ToNumberAndPK(IEnumerable<EDIMessage> messages)
		{
			return messages.Select(m => $"{m.EM_MessageNum} - {m.PK}").ToArray();
		}

		ZQuery CreateQuery(int maxRows)
		{
			var query = new ZQuery();
			query.MaximumRows = maxRows;
			query.OrderBy = EDIMessageSchema.EM_MessageNum.Name;
			return query;
		}

		EDIMessage[] CreateMessages(int messageCount)
		{
			var messages = new EDIMessage[messageCount];
			for (int i = 0; i < messageCount; i++)
			{
				var message = Factory.New<TestEDIMessage>();
				message.EM_MessageNum = i.ToString("D6");
				message.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(i - messages.Length);
				messages[i] = message;
			}

			Factory.Save();

			AssertEquals("sanity check", "000000", messages[0].EM_MessageNum);

			return messages;
		}

		sealed class KeySetExtractorForTest : IMessageKeySetExtractor
		{
			readonly Func<int, int> getKey;

			public int ExtractionCount { get; set; }

			public KeySetExtractorForTest(Func<int, int> getKey)
			{
				this.getKey = getKey;
			}

			public MessageKeySet ExtractKeys(EDIMessage message)
			{
				ExtractionCount++;
				return new MessageKeySet(new[] { getKey(int.Parse(message.EM_MessageNum)).ToString() });
			}
		}

		sealed class BrokenKeySetExtractorForTest : IMessageKeySetExtractor
		{
			public MessageKeySet ExtractKeys(EDIMessage message)
			{
				throw new InvalidOperationException("This key set extractor is broken.");
			}
		}

		sealed class TestEDIMessage : EDIMessage
		{
			public TestEDIMessage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override string GetMessageReferenceNumber() => EM_MessageNum;
		}
	}
}
