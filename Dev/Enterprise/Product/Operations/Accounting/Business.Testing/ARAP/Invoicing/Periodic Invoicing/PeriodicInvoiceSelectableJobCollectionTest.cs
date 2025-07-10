using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceSelectableJobCollection))]
	public class PeriodicInvoiceSelectableJobCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PeriodicInvoiceSelectableJobCollection>
	{
		public void TestAddJobUsingCollectionFactoryAndContainsJob()
		{
			var creator = new TestObjectCreator(Factory);
			var job = TestObjectCreator.CreateJob("J1", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			var secondJob = TestObjectCreator.CreateJob("J2", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			Factory.Save();

			var collection = new PeriodicInvoiceSelectableJobCollection(Factory);
			AssertEquals("There is nothing in the collection", 0, collection.Count);
			collection.Add(job);
			AssertEquals("There is one item in the collection", 1, collection.Count);
			AssertEquals("The item is included by default", true, collection[0].IncludeInThePeriodicInvoice);
			AssertEquals("The item's job is the correct one", job.PK, collection[0].Parent.PK);
			AssertEquals("Checking method 'ContainsJob' with positive", true, collection.Contains(job));
			AssertEquals("Checking method 'ContainsJob' with negative", false, collection.Contains(secondJob));
		}

		public void TestLoadWithQuery()
		{
			var job = TestObjectCreator.CreateJob("J1", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			var secondJob = TestObjectCreator.CreateJob("J2", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			Factory.Save();

			var collectionInAnotherFactory = new PeriodicInvoiceSelectableJobCollection(new ReadOnlyBusinessObjectFactory());
			collectionInAnotherFactory.Load(new ZQuery(JobHeaderSchema.PK, job.PK));

			AssertEquals("There is one item in the collection", 1, collectionInAnotherFactory.Count);
			AssertEquals("The item's job is the correct one", job.PK, collectionInAnotherFactory[0].Parent.PK);
		}

		public void TestNotification()
		{
			var job = TestObjectCreator.CreateJob("J1", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			Factory.Save();
			var collectionInAnotherFactory = new PeriodicInvoiceSelectableJobCollection(new ReadOnlyBusinessObjectFactory());
			collectionInAnotherFactory.Add(job);

			var eventFired = false;
			var eventHandler = new EventHandler((object o, EventArgs e) => { eventFired = true; });

			try
			{
				collectionInAnotherFactory.IncludeInThePeriodicInvoiceChanged += eventHandler;
				AssertEquals("Event not yet fired", false, eventFired);
				collectionInAnotherFactory[0].IncludeInThePeriodicInvoice = false;
				AssertEquals("Event was fired", true, eventFired);
				eventFired = false;
				collectionInAnotherFactory[0].IncludeInThePeriodicInvoice = false;
				AssertEquals("Event not refired", false, eventFired);
				collectionInAnotherFactory[0].IncludeInThePeriodicInvoice = true;
				AssertEquals("Event now fired again", true, eventFired);
			}
			finally
			{
				collectionInAnotherFactory.IncludeInThePeriodicInvoiceChanged -= eventHandler;
			}
		}

		#region Implementation
		protected override Type GetExpectedCollectionType()
		{
			return typeof(PeriodicInvoiceSelectableJobCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PeriodicInvoiceSelectableJob(Factory, new DummyObserver(), Factory.NewJobWithValidTestDataForTesting<Job>());
		}

		protected override PeriodicInvoiceSelectableJobCollection GetCollectionToTest()
		{
			return new PeriodicInvoiceSelectableJobCollection(Factory);
		}

		TestObjectCreator testObjectCreator;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (testObjectCreator == null)
				{
					testObjectCreator = new TestObjectCreator(Factory);
				}
				return testObjectCreator;
			}
		}
		#endregion
	}
}
