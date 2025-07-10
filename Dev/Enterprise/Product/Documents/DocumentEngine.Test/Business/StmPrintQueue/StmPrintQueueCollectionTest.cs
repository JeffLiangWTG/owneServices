using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(StmPrintQueueCollection))]
	sealed class StmPrintQueueCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetOnlinePrinterNamesResolvesDuplicatesByAddingTheServerAndQueueName()
		{
			StmPrintQueueCollection collection = new StmPrintQueueCollection(Factory);
			StmPrintQueue queue1 = collection.AddNew();
			queue1.SQ_DisplayName = "My Printer";
			queue1.SQ_QueueName = "MyPrinter";
			queue1.SQ_ServerName = "Tom";

			StmPrintQueue queue2 = collection.AddNew();
			queue2.SQ_DisplayName = "My Printer";
			queue2.SQ_QueueName = "MyPrinter";
			queue2.SQ_ServerName = "Dick";

			StmPrintQueue queue3 = collection.AddNew();
			queue3.SQ_DisplayName = "My Printer";
			queue3.SQ_QueueName = "MyPrinter";
			queue3.SQ_ServerName = "Harry";

			StmPrintQueue queue4 = collection.AddNew();
			queue4.SQ_DisplayName = "Other Printer";
			queue4.SQ_QueueName = "MyPrinter";
			queue4.SQ_ServerName = "Harry";

			CodeDescriptionPairList list = collection.GetOnlinePrinterNames();
			ZStringBuilder result = new ZStringBuilder();
			foreach (ICodeDescription pair in list)
			{
				result.Append(pair.Code + " -:- " + pair.Description);
			}
			AssertMultilineASCIIEquals("Complete Printer List", @"
My Printer (\\Dick\MyPrinter) -:- Dick
My Printer (\\Harry\MyPrinter) -:- Harry
My Printer (\\Tom\MyPrinter) -:- Tom
Other Printer -:- Harry
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		public void TestStmPrintQueueCollection()
		{
			StmPrintQueue queue1 = Factory.New<StmPrintQueue>();
			queue1.SQ_QueueName = "Queue1";

			StmPrintQueue queue2 = Factory.New<StmPrintQueue>();
			queue2.SQ_QueueName = "Queue2";

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			StmPrintQueueCollection collection = new StmPrintQueueCollection(newFactory);
			ZQuery filter = new ZQuery();
			filter.OrderBy = StmPrintQueue.Schema.SQ_QueueName;
			collection.Load(filter);

			AssertEquals("Count", 2, collection.Count);
			AssertEquals("[0].SQ_QueueName", "Queue1", collection[0].SQ_QueueName);
			AssertEquals("[1].SQ_QueueName", "Queue2", collection[1].SQ_QueueName);
		}

		public void TestGetPrintersVisibleToCurrentUser()
		{
			StmPrintQueue printer1 = Collection.AddNew();
			StmPrintQueue printer2 = Collection.AddNew();
			StmPrintQueue printer3 = Collection.AddNew();
			StmPrintQueue printer4 = Collection.AddNew();

			Env.Security.GetPrintQueueCheckPoint(printer3.PK.ToGuid(), null).IsAllowed = false;
			Env.Security.GetPrintQueueCheckPoint(printer4.PK.ToGuid(), null).IsAllowed = false;

			StmPrintQueueCollection collection = StmPrintQueueCollection.GetPrintersVisibleToCurrentUser(Factory, false);
			AssertEquals("Count", 4, collection.Count);
			AssertEquals("Contains(printer1)", true, collection.Contains(printer1));
			AssertEquals("Contains(printer2)", true, collection.Contains(printer2));
			AssertEquals("Contains(printer3)", true, collection.Contains(printer3));
			AssertEquals("Contains(printer4)", true, collection.Contains(printer4));

			collection = StmPrintQueueCollection.GetPrintersVisibleToCurrentUser(Factory, true);
			AssertEquals("Count", 2, collection.Count);
			AssertEquals("Contains(printer1)", true, collection.Contains(printer1));
			AssertEquals("Contains(printer2)", true, collection.Contains(printer2));
		}

		public void TestGetPrintersVisibleToCurrentUserGreaterThanOrEqualTo100()
		{
			var rowFactory = ((IBusinessObjectFactoryInternals)Factory).RowFactory;
			rowFactory.MaximumRowsBeforeUsingIndex = 2;

			Enumerable.Range(0, 2).ForEach(i =>
			{
				var queue = Factory.New<StmPrintQueue>();
				queue.SQ_QueueName = $"Queue{i}";
				queue.SQ_AllowPrinting = true ;
			});
			Factory.Save();

			var collection = StmPrintQueueCollection.GetPrintersVisibleToCurrentUser(Factory, hideUnallowed: false, onlyOnline: true);
			AssertEquals(2, collection.Count);
		}

		[TestDate(2005, 1, 2, 13, 20, 20)]
		public void TestGetOnlinePrinterNames()
		{
			StmPrintQueue printer0 = Collection.AddNew();
			StmPrintQueue printer1 = Collection.AddNew();
			StmPrintQueue printer2 = Collection.AddNew();

			printer0.SQ_DisplayName = "x";
			printer0.SQ_ServerName = "s0";
			printer1.SQ_DisplayName = "x";
			printer1.SQ_ServerName = "s1";
			printer1.SQ_QueueDeleted = new ZDateTime(2005, 1, 1);
			printer2.SQ_DisplayName = "x";
			printer2.SQ_ServerName = "s2";

			Env.Security.GetPrintQueueCheckPoint(printer2.PK.ToGuid(), null).IsAllowed = false;

			CodeDescriptionPairList printerNames = Collection.GetOnlinePrinterNames();
			AssertEquals("Count", 2, printerNames.Count);
			AssertEquals("[0].Code", "x", printerNames[0].Code);
			AssertEquals("[0].Description", "s0", printerNames[0].Description);
			AssertEquals("[1].Code", "x (No Rights)", printerNames[1].Code);
			AssertEquals("[1].Description", "s2", printerNames[1].Description);
		}

		public void TestAddNotificationWhenAdditionalFilterNotMet()
		{
			var collection = new StmPrintQueueCollection(Factory);
			var queue = Factory.NewWithValidTestData<StmPrintQueue>();
			queue.SQ_DisplayName = "Jerry Test Printer";
			queue.SQ_AllowPrinting = false;
			var notification = collection.GetAllNotificationsWhenAdditionalFilterNotMet(queue);

			AssertEquals("This printer is inactive and cannot be used.", notification);
		}

		new StmPrintQueueCollection Collection
		{
			get { return (StmPrintQueueCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmPrintQueueCollection(Factory);
		}
	}
}
