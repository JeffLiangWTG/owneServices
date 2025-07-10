using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MailManager.Business;
using Moq;
using static Enterprise.MailManager.MailFilters.Testing.MailTestHelpers;

namespace Enterprise.MailManager.MailFilters.Testing
{
	sealed class MailBatchProcessorTests : TestCaseWithFactory
	{
		readonly Mock<INotifications> dummyNotifications = new Mock<INotifications>();
		readonly IMailFilter runMeFilter = new QueryMailFilter("ZQ1", subject: "RunMe");

		static List<MailItem> CreateRunMeMail(int amount = 1, BusinessObjectFactory factory = null)
		{
			factory = factory ?? new BusinessObjectFactory();

			var result = new List<MailItem>();
			while (--amount >= 0)
			{
				result.Add(CreateMail(factory, "RunMe", "foo@bar.com", application: "ZQ1"));
			}

			factory.Save();
			return result;
		}

		public void TestUnmatchedItemsArentConstantlyReprocessed()
		{
			CreateRunMeMail(1);

			var count = 0;
			var processor = new MailBatchProcessor(runMeFilter, (m, p) =>
			{
				if (++count > 10)
				{
					Fail("You appear to be in an infinite loop, constantly reprocessing the same thing");
				}

				return MailProcessingResult.Unmatch;
			});

			processor.Process(dummyNotifications.Object);
			AssertLessThan("We should not reprocess an unmatched record.", count, 10);
		}

		public void TestHonorsFilter()
		{
			var f1 = new QueryMailFilter("MF1", subject: "I should be picked up by F1");
			var f2 = new QueryMailFilter("MF2", subject: "I should be picked up by F2");

			CreateMail(Factory, "I should be picked up by F1", "any@one.com", application: "MF1");
			CreateMail(Factory, "I should be picked up by F2", "any@one.com", application: "MF2");

			Factory.Save();

			var p = new MailBatchProcessor(f1, CollectMailItems(out var f1Processed));
			p.Process(dummyNotifications.Object);

			AssertEquals("I should be picked up by F1", f1Processed.Single().MI_Subject);

			p = new MailBatchProcessor(f2, CollectMailItems(out var f2Processed));
			p.Process(dummyNotifications.Object);
			AssertEquals("I should be picked up by F2", f2Processed.Single().MI_Subject);
		}

		public void TestUsesBatchSize()
		{
			CreateRunMeMail(amount: 10);

			var totalProcessed = 0;
			var totalProcessedPerSave = new List<int>();

			void SaveHandler(BusinessObjectFactory ignored, bool ignored2)
			{
				totalProcessedPerSave.Add(totalProcessed);
			}

			var processor = new MailBatchProcessor(runMeFilter, (m, p) =>
			{
				totalProcessed++;

				m.Factory.Saved -= SaveHandler;
				m.Factory.Saved += SaveHandler;

				return MailProcessingResult.Delete;
			}, batchSize: 3);

			processor.Process(dummyNotifications.Object);

			AssertArrayEqualsByElements("Expect a save every three items, and a final Save for the leftovers", new[] { 3, 6, 9, 10 }, totalProcessedPerSave.ToArray());
		}

		public void TestCancellationToken()
		{
			CreateRunMeMail(amount: 4);

			var token = new CancellationTokenSource();
			var processor = new MailBatchProcessor(runMeFilter, CollectMailItems(out var processed, token.Cancel), batchSize: 2);

			AssertEquals("Before running we should see all 4 items", 4, runMeFilter.Load(new BusinessObjectFactory(), 50).Length);

			AssertExceptionThrown<OperationCanceledException>(() => processor.Process(dummyNotifications.Object, token.Token));
			AssertEquals("The first batch should have been saved", 2, runMeFilter.Load(new BusinessObjectFactory(), 50).Length);
		}

		public void TestSetsStatus_Processed()
		{
			var mail = CreateRunMeMail().Single();
			var processor = new MailBatchProcessor(runMeFilter, (m, p) => MailProcessingResult.MarkSuccess);

			processor.Process(dummyNotifications.Object);

			AssertEquals("Successful items should be processed", MailStatus.Processed, Factory.Load<MailItem>(mail.PK).MI_Status);
		}

		public void TestApplicationIsChecked()
		{
			var mail = CreateRunMeMail().Single();
			mail.MI_Application = "STD";
			mail.Factory.Save();
			var processor = new MailBatchProcessor(runMeFilter, (m, p) => MailProcessingResult.MarkSuccess);

			processor.Process(dummyNotifications.Object);

			AssertEquals("Application being changed mattered", MailStatus.Queued, Factory.Load<MailItem>(mail.PK).MI_Status);
		}

		public void TestSetsStatus_Unmatch()
		{
			var mail = CreateRunMeMail().Single();
			var processor = new MailBatchProcessor(runMeFilter, (m, p) => MailProcessingResult.Unmatch);

			processor.Process(dummyNotifications.Object);

			mail = Factory.Load<MailItem>(mail.PK);
			CombineAssertions("Unmatching items should be returned to the pool", () =>
			{
				AssertEquals("MI_Status", MailStatus.Queued, mail.MI_Status);
				AssertEquals("MI_Application", "ZQ1", mail.MI_Application);
			});
		}

		public void TestSetsStatus_Delete()
		{
			var mail = CreateRunMeMail().Single();
			var processor = new MailBatchProcessor(runMeFilter, (m, p) => MailProcessingResult.Delete);

			processor.Process(dummyNotifications.Object);

			AssertNull("When we request a delete, the mail should be deleted", Factory.Load<MailItem>(mail.PK));
		}

		public void TestSetsStatus_Exception()
		{
			var mail = CreateRunMeMail().Single();
			var processor = new MailBatchProcessor(runMeFilter, (m, p) => throw new InvalidOperationException("Oi"));

			processor.Process(dummyNotifications.Object);
			AssertEquals("MI_Status", MailStatus.Failed, Factory.Load<MailItem>(mail.PK).MI_Status);
			AssertEquals(ErrorReporter.LastExceptionReported.Message, "Oi");

			ErrorReporter.Clear();
		}

		ProcessMailItem CollectMailItems(out List<IMailItem> itemsProcessed, Action extraAction = null)
		{
			var capturedProcessed = itemsProcessed = new List<IMailItem>();
			return (m, p) =>
			{
				capturedProcessed.Add(m);
				extraAction?.Invoke();

				return MailProcessingResult.MarkFailed;
			};
		}
	}
}
