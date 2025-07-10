using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class DummyObserver : IIncludeInThePeriodicInvoiceChangedObserver
	{
		public void Notify(object sender) { }
	}

	[TestedType(typeof(TestUseDefaultingLogicCollection))]
	public class PeriodicInvoiceSelectableJobCollectionWithDefaultingLogicTest : NonPersistentBusinessObjectCollectionTestCase<TestUseDefaultingLogicCollection>
	{
		public void TestUseDefaultingLogicDisabled()
		{
			var collection = new TestUseDefaultingLogicCollection(Factory);
			observer = collection;
			collection.SetTrigger(true);
			var job1 = TestObjectCreator.CreateJob("J1", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			job1.JH_Status = MasterFiles.Business.JobHeaderStatus.WorkOnHold.Code;
			collection.Add(job1);
			AssertEquals("There is one item in the collection", 1, collection.Count);
			Assert("The item is included by default", collection[0].IncludeInThePeriodicInvoice);
			AssertEquals("No validations", 0, collection.IncludeInThePeriodicInvoiceValidations);
			AssertEquals("Parent include triggered", 1, collection.TriggerCount);

			collection.SetTrigger(true);
			var job2 = TestObjectCreator.CreateJob("J2", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			job2.JH_Status = MasterFiles.Business.JobHeaderStatus.Working.Code;
			collection.Add(job2);
			AssertEquals("There are two items in the collection", 2, collection.Count);
			AssertEquals("No validations", 0, collection.IncludeInThePeriodicInvoiceValidations);
			AssertEquals("Parent not triggered", 0, collection.TriggerCount);
		}

		public void TestUseDefaultingLogicEnabled()
		{
			var expectedMessage = "Jobs with status 'Invoice on hold' or 'Work on hold' cannot be included in the invoice.";
			var collection = new TestUseDefaultingLogicCollection(Factory);
			observer = collection;
			collection.UseDefaultingLogic();
			AssertEquals("There is nothing in the collection", 0, collection.Count);

			collection.IncludeInThePeriodicInvoiceValidations = 0;
			collection.SetTrigger(true);
			var job1 = TestObjectCreator.CreateJob("J1", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			job1.JH_Status = MasterFiles.Business.JobHeaderStatus.WorkOnHold.Code;
			collection.Add(job1);
			AssertEquals("There is one item in the collection", 1, collection.Count);
			Assert("The item is excluded by default", !collection[0].IncludeInThePeriodicInvoice);
			AssertHasWarning(collection[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("Validated once", 1, collection.IncludeInThePeriodicInvoiceValidations);
			AssertEquals("Parent include not triggered", 0, collection.TriggerCount);

			collection.IncludeInThePeriodicInvoiceValidations = 0;
			var job2 = TestObjectCreator.CreateJob("J2", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			job2.JH_Status = MasterFiles.Business.JobHeaderStatus.Working.Code;
			collection.Add(job2);
			AssertEquals("There are two items in the collection", 2, collection.Count);
			Assert("The first item is excluded by default", !collection[0].IncludeInThePeriodicInvoice);
			Assert("The second item is included by default", collection[1].IncludeInThePeriodicInvoice);
			AssertHasWarning(collection[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(collection[1].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("Parent include triggered", 1, collection.TriggerCount);
			AssertEquals("Two validations", 2, collection.IncludeInThePeriodicInvoiceValidations);

			collection.IncludeInThePeriodicInvoiceValidations = 0;
			collection.SetTrigger(true);
			var job3 = TestObjectCreator.CreateJob("J3", TestObjectCreator.AALSHI, 1, TestObjectCreator.AALSHI, 1);
			job3.JH_Status = MasterFiles.Business.JobHeaderStatus.InvoiceOnHold.Code;
			collection.Add(job3);
			AssertEquals("There are three items in the collection", 3, collection.Count);
			AssertEquals("The third item is the one just added", job3.PK, collection[2].Parent.PK);
			Assert("The third item is excluded by default", !collection[2].IncludeInThePeriodicInvoice);
			AssertHasWarning(collection[2].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("Validated once", 1, collection.IncludeInThePeriodicInvoiceValidations);
			AssertEquals("Parent not triggered", 0, collection.TriggerCount);

			collection.IncludeInThePeriodicInvoiceValidations = 0;
			collection.SetTrigger(true);
			collection[1].IncludeInThePeriodicInvoice = false;
			AssertEquals("Parent exclude triggered", 1, collection.TriggerCount);
			AssertEquals("Three validations", 3, collection.IncludeInThePeriodicInvoiceValidations);
			AssertNoWarning(collection[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(collection[1].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(collection[2].IncludeInThePeriodicInvoiceInfo, expectedMessage);

			collection.IncludeInThePeriodicInvoiceValidations = 0;
			collection.SetTrigger(true);
			collection[1].IncludeInThePeriodicInvoice = true;
			Assert("The first item is still excluded", !collection[0].IncludeInThePeriodicInvoice);
			Assert("The second item is still included", collection[1].IncludeInThePeriodicInvoice);
			Assert("The third item is still excluded", !collection[2].IncludeInThePeriodicInvoice);
			AssertHasWarning(collection[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(collection[1].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(collection[2].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("Parent include triggered", 1, collection.TriggerCount);
			AssertEquals("Three jobs validated", 3, collection.IncludeInThePeriodicInvoiceValidations);

			collection.IncludeInThePeriodicInvoiceValidations = 0;
			collection.SetTrigger(true);
			collection[2].IncludeInThePeriodicInvoice = true;
			AssertHasWarning(collection[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(collection[1].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(collection[2].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasError(collection[2].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("Parent not triggered", 0, collection.TriggerCount);
			AssertEquals("One job validated", 1, collection.IncludeInThePeriodicInvoiceValidations);

			collection.IncludeInThePeriodicInvoiceValidations = 0;
			collection.SetTrigger(true);
			collection[2].IncludeInThePeriodicInvoice = false;
			AssertHasWarning(collection[0].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertNoWarning(collection[1].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertHasWarning(collection[2].IncludeInThePeriodicInvoiceInfo, expectedMessage);
			AssertEquals("Parent not triggered", 0, collection.TriggerCount);
			AssertEquals("One job validated", 1, collection.IncludeInThePeriodicInvoiceValidations);
		}

		#region Implementation
		protected override Type GetExpectedCollectionType()
		{
			return typeof(TestUseDefaultingLogicCollection);
		}

		IIncludeInThePeriodicInvoiceChangedObserver observer;
		protected override void SetUp()
		{
			observer = new DummyObserver();
			base.SetUp();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PeriodicInvoiceSelectableJob(Factory, observer, Factory.NewJobWithValidTestDataForTesting<Job>());
		}

		protected override TestUseDefaultingLogicCollection GetCollectionToTest()
		{
			return new TestUseDefaultingLogicCollection(Factory);
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
