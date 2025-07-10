using System;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobCollection))]
	public class JobCollection_InnerTest : CargoWise.EntityFramework.Testing.BusinessObjectCollectionTestCase
	{
		public void TestAddNewShouldRaiseAnError()
		{
			Assert("Adding a new element in JobCollection is not autorized", !TestCollection.AllowNewCore_ForTestOnly);
			AssertExceptionThrown<NotSupportedException>("You cannot directly add to this collection. You need to use the JobHeader.Loader to create a new Job.",
				() => TestCollection.AddNew());
		}

		public void TestCreateFilter()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.MarkAsInactive();
			Factory.Save();

			var testCollection = new JobCollection(Factory);
			testCollection.Load();
			AssertEquals("Collection should contain only 1 record", 1, testCollection.Count);
			Assert(testCollection.Contains(job));
		}

		protected JobCollection TestCollection;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			Factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			var result = base.GetNewElementToAddToTheCollection();
			Factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCollection = new JobCollection(Factory);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new JobCollection(Factory);
		}

		#region Implementation

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		#endregion
	}
}
