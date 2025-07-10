using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyManagedBatchProcessorTestCase : ManagedBatchProcessorTestCase<DummyManagedBatchProcessor, DummyBusinessObject>
	{
		#region Impl

		protected override DummyBusinessObject AddToQueue() => Factory.New<DummyBusinessObject>();
		protected override DummyManagedBatchProcessor GetProcessor() => new DummyManagedBatchProcessor();
		protected override QueueStatus GetStatus(DummyBusinessObject row)
		{
			switch (row.Z0_Number)
			{
				case 0:
					return QueueStatus.Queued;
				case 1:
					return QueueStatus.Succeeded;
				case 2:
					return QueueStatus.Failed;
				default:
					throw new InvalidOperationException("Unrecognised number: " + row.Z0_Number);
			}
		}

		class EvensAndOddsGrouper : ManagedBatchProcessor<DummyBusinessObject>.IBatchGrouper
		{
			public IEnumerable<(BusinessObjectFactory groupedBatchFactory, IList<DummyBusinessObject> groupedBatch, object groupKey)> GetGroups(BusinessObjectFactory factory, IList<DummyBusinessObject> batch)
			{
				var groups = batch.GroupBy(b => b.Z0_AnotherNumber % 2 == 0);
				foreach (var group in groups)
				{
					var batchFactory = new BusinessObjectFactory();
					var groupedBatch = batchFactory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.PK, group.Select(b => b.PK)));
					yield return (batchFactory, groupedBatch, group.Key);
				}
			}
		}

		#endregion

		void AssertException(Exception ex)
		{
			var queue = SetupQueue(10);
			var processor = GetProcessor();
			processor.ProcessRow_Override = (a) => throw ex;
			processor.Process(Notifications);

			queue.ForEach(q => q.Reload());
			AssertEquals("Should be marked as bad.", 10, queue.Count(l => GetStatus(l) == QueueStatus.Failed));
		}

		public void TestReportsProgress()
		{
			var queue = SetupQueue(4);
			var processor = GetProcessor();
			processor.BatchSizeOverride = 3;

			var indicies = new List<(int, int)>();
			processor.ProcessRowWithIndex_Override += (r, i) => indicies.Add(i);
			processor.Process(null);

			AssertArrayEqualsByElements("Indicies should be reported by base-1-index out of batch-length", new[] { (1, 3), (2, 3), (3, 3), (1, 1) }, indicies.ToArray());
		}

		public void TestHandlesCommonExceptionTypes()
		{
			AssertException(new InvalidOperationException());
			AssertException(new Exception());
			AssertException(new NullReferenceException());

			AssertNotNullOrEmpty(ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestRetry()
		{
			var queue = SetupQueue(10);
			var processor = GetProcessor();
			var failed = false;
			processor.ProcessRow_Override = (a) =>
			{
				if (!failed)
				{
					failed = true;
					throw new InvalidOperationException();
				}
			};
			processor.Process(Notifications);
			queue.ForEach(q => q.Reload());
			AssertEquals("Should retry and get processed", 10, queue.Count(l => GetStatus(l) == QueueStatus.Succeeded));
		}

		public void TestLargeNumbers()
		{
			AssertAllMessagesGetProcessed(1000);
		}

		public void TestGrouping()
		{
			var queue = SetupQueue(10);
			for (int i = 0; i < 10; i++)
			{
				Factory.New<DummyBusinessObject>().Z0_AnotherNumber = i;
			}
			Factory.Save();

			var batchCounter = 0;
			var processor = GetProcessor();
			processor.GrouperOverride = new EvensAndOddsGrouper();
			processor.WithBatch_Override = (a, b, c) => {
				batchCounter++;
				return null;
			};
			processor.Process(Notifications);

			AssertEquals("Since we batched the rows by whether or not Z0_AnotherNumber is disible by 2, there should be 2 batches", 2, batchCounter);
		}

		void ProcessWithException(DummyManagedBatchProcessor processor, Exception ex)
		{
			var queue = SetupQueue(1);
			processor.ProcessRow_Override = (a) => throw ex;
			processor.Process(Notifications);

			queue.ForEach(q => q.Reload());
		}

		public void TestWarningLogging()
		{
			var processor = GetProcessor();
			var ex = new InvalidOperationException();
			ProcessWithException(processor, ex);

			var processingFailureWarnings = Notifications.Where(w => w.Message.Contains("Failure in processing")).ToList();
			Assert("Log the error message when we have to retry batch processing", processingFailureWarnings.All(w => w.Message.Contains(ex.Message)));
			Assert("Should not add the full stack trace to logs", processingFailureWarnings.All(w => !w.Message.Contains(ex.StackTrace)));

			ErrorReporter.Clear();
		}

		public void TestErrorLogging()
		{
			var processor = GetProcessor();
			var markRowAsBadException = new NullReferenceException();
			processor.MarkRowAsBad_Override = (a, b, c) => throw markRowAsBadException;
			var ex = new InvalidOperationException();
			ProcessWithException(processor, ex);

			var processingFailureErrors = Notifications.Where(e => e.Message.Contains("Error in batching occurred")).ToList();
			Assert("Log the error message when batch processing fails completely", processingFailureErrors.All(e => e.Message.Contains(markRowAsBadException.Message)));
			Assert("Should not add the full stack trace to logs", processingFailureErrors.All(e => !e.Message.Contains(markRowAsBadException.StackTrace)));

			ErrorReporter.Clear();
		}
	}
}
