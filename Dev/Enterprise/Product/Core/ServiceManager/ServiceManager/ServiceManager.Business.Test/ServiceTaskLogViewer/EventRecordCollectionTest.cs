using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(EventRecordCollection))]
	sealed class EventRecordCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EventRecordCollection>
	{
		public void TestLoad()
		{
			var collection = new EventRecordCollection();
			var testFile =
				"2007-07-20 16:54:53      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 16:59:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:04:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:09:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:14:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:19:49      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:24:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:29:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:34:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:39:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:44:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:49:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:54:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 17:59:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 18:04:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 18:09:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 18:14:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 18:19:50      Information              10008                    LogWalker Cycle Started\r\n" +
				"2007-07-20 18:24:50      Information              10008                    LogWalker Cycle Started\r\n";
			using (var testMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testFile)))
			{
				collection.Load(testMemoryStream);
			}

			AssertEquals(19, collection.Count);
			AssertEquals(new ZDateTime(2007, 7, 20, 18, 24, 50), collection[18].DateTime);
			collection.Sort("DateTime", ListSortDirection.Descending);
			AssertEquals(new ZDateTime(2007, 7, 20, 18, 24, 50), collection[0].DateTime);

			testFile =
				"2007-07-20 18:00:03      Information              10008                    Database LOG Backup started.\r\n" +
				"2007-07-20 18:00:03      Information              10008                    Backup Directory - C:\\\\TEMP\\\\BACKUP\r\n" +
				"2007-07-20 18:00:03      Information              10008                    Backup Log [Odyssey]\\n\\tC:\\\\TEMP\\\\BACKUP\\\\Odyssey_20070721_010003.lbk\r\n" +
				"2007-07-20 18:00:08      Information              10008                    Backup Log [Odyssey_SD001]\\n\\tC:\\\\TEMP\\\\BACKUP\\\\Odyssey_SD001_20070721_010003.lbk\r\n" +
				"2007-07-20 18:00:08      Information              10008                    Skipped Backup Log [Odyssey_SD001] - no current full backup\r\n" +
				"2007-07-20 18:00:08      Information              10008                    Database log backup completed\r\n2007-07-20 18:00:08      Information              \r\n";
			using (var testMemoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testFile)))
			{
				collection.Load(testMemoryStream);
			}
			AssertEquals(7, collection.Count);
			AssertEquals(new ZDateTime(2007, 7, 20, 18, 0, 3), collection[6].DateTime);
		}

		public void TestRemoveAndDeleteAll()
		{
			var collection = new EventRecordCollection();
			using (var stream = LWK_20070720Txt)
			{
				collection.Load(stream);
			}

			AssertEquals(19, collection.Count);
			collection.RemoveAndDeleteAll();
			AssertEquals(0, collection.Count);
		}

		public void TestSequenceNumberCalculation()
		{
			var collection = new EventRecordCollection();
			using (var stream = LWK_20070720Txt)
			{
				collection.Load(stream);
			}

			AssertEquals(0, collection[0].SequenceNumber);
			AssertEquals(collection.Count - 1, collection[collection.Count - 1].SequenceNumber);
		}

		protected override EventRecordCollection GetCollectionToTest()
		{
			return new EventRecordCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return EventRecordTest.GetNewEventRecord();
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() =>  new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;

		Stream LWK_20070720Txt => resourceRetriever.Value.GetStream("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.LWK_20070720.TXT");
	}
}
