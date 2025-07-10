using CargoWise.Types;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Module.Testing
{
	[TestedType(typeof(PrintJobFilterBusinessObject))]
	class PrintJobFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Filters

		public void TestPrintJobType()
		{
			var job1 = GetNewJob();
			var job2 = GetNewJob();
			var job3 = GetNewJob();

			job1.SP_JobType = nameof(PrintJobType.EML);
			job2.SP_JobType = nameof(PrintJobType.PRN);
			job3.SP_JobType = nameof(PrintJobType.FAX);

			Factory.Save();

			var filter1 = new PrintJobFilterBusinessObject();
			((ModuleTextFilter)filter1["Job Type"]).Property = "EML";
			((ModuleTextFilter)filter1["Job Type"]).IsActive = true;

			var collection = new StmPrintJobCollection(Factory);
			collection.Load(filter1.Filter);

			AssertCollectionContains(job1, collection);
			AssertCollectionNotContains(job2, collection);
			AssertCollectionNotContains(job3, collection);

			var filter2 = new PrintJobFilterBusinessObject();
			((ModuleTextFilter)filter2["Status"]).Property = "ALL";
			((ModuleTextFilter)filter2["Status"]).IsActive = true;

			collection = new StmPrintJobCollection(Factory);
			collection.Load(filter2.Filter);

			AssertCollectionContains(job1, collection);
			AssertCollectionContains(job2, collection);
			AssertCollectionContains(job3, collection);
		}

		public void TestSignByFilter()
		{
			var job1 = GetNewJob();
			var job2 = GetNewJob();
			var job3 = GetNewJob();

			job1.SP_SignBy = DocumentsSignBy.NON;
			job2.SP_SignBy = DocumentsSignBy.PFX;
			job3.SP_SignBy = DocumentsSignBy.DOS;

			Factory.Save();

			var filter1 = new PrintJobFilterBusinessObject();
			((ModuleTextFilter)filter1["Sign By"]).Property = DocumentsSignBy.NON;
			((ModuleTextFilter)filter1["Sign By"]).IsActive = true;

			var collection = new StmPrintJobCollection(Factory);
			collection.Load(filter1.Filter);

			AssertCollectionContains(job1, collection);
			AssertCollectionNotContains(job2, collection);
			AssertCollectionNotContains(job3, collection);

			var filter2 = new PrintJobFilterBusinessObject();
			((ModuleTextFilter)filter2["Sign By"]).Property = DocumentsSignBy.PFX;
			((ModuleTextFilter)filter2["Sign By"]).IsActive = true;

			collection = new StmPrintJobCollection(Factory);
			collection.Load(filter2.Filter);

			AssertCollectionNotContains(job1, collection);
			AssertCollectionContains(job2, collection);
			AssertCollectionNotContains(job3, collection);

			var filter3 = new PrintJobFilterBusinessObject();
			((ModuleTextFilter)filter3["Sign By"]).Property = DocumentsSignBy.DOS;
			((ModuleTextFilter)filter3["Sign By"]).IsActive = true;

			collection = new StmPrintJobCollection(Factory);
			collection.Load(filter3.Filter);

			AssertCollectionNotContains(job1, collection);
			AssertCollectionNotContains(job2, collection);
			AssertCollectionContains(job3, collection);
		}

		public void TestActiveStatusFilter()
		{
			var job1 = GetNewJob();
			var job2 = GetNewJob();
			var job3 = GetNewJob();
			var job4 = GetNewJob();

			job1.SP_RetryAttempts = 0;
			job1.SP_Status = nameof(PrintJobStatus.QUE);
			job2.SP_RetryAttempts = 1;
			job2.SP_Status = nameof(PrintJobStatus.QUE);
			job3.SP_RetryAttempts = 2;
			job3.SP_Status = nameof(PrintJobStatus.FAL);
			job4.SP_RetryAttempts = 0;
			job4.SP_Status = nameof(PrintJobStatus.FAL);

			Factory.Save();

			var progressFilter = new PrintJobFilterBusinessObject();
			((ModuleTextFilter)progressFilter["Status"]).Property = "PRG";
			((ModuleTextFilter)progressFilter["Status"]).IsActive = true;

			var collection = new StmPrintJobCollection(Factory);
			collection.Load(progressFilter.Filter);

			AssertCollectionContains(job1, collection);
			AssertCollectionNotContains(job2, collection);
			AssertCollectionNotContains(job3, collection);
			AssertCollectionNotContains(job4, collection);

			var failedFilter = new PrintJobFilterBusinessObject();
			((ModuleTextFilter)failedFilter["Status"]).Property = "FLD";
			((ModuleTextFilter)failedFilter["Status"]).IsActive = true;

			collection = new StmPrintJobCollection(Factory);
			collection.Load(failedFilter.Filter);

			AssertCollectionNotContains(job1, collection);
			AssertCollectionContains(job2, collection);
			AssertCollectionContains(job3, collection);
			AssertCollectionContains(job4, collection);
		}

		[TestDate(2010, 10, 5, 5, 30, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestDateFilters()
		{
			var job1 = GetNewJob();
			job1.SP_RunDateTime = new ZDateTime(2010, 10, 4, 19, 30, 0);
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			job1.Logs.AddNew(AutoEvents.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.

			Factory.Save();

			var filter = new PrintJobFilterBusinessObject { ["Run (UTC)"] = { IsActive = true } };

			((ModuleDateFilter)filter["Run (UTC)"]).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			var collection = new StmPrintJobCollection(Factory);

			collection.Load(filter.Filter);
			AssertEquals(0, collection.Count);

			((ModuleDateFilter)filter["Run (UTC)"]).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			collection.Load(filter.Filter);
			AssertEquals(1, collection.Count);

			filter["Run (UTC)"].IsActive = false;
			filter["Run (Local)"].IsActive = true;

			((ModuleDateFilter)filter["Run (Local)"]).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Today;
			collection = new StmPrintJobCollection(Factory);

			collection.Load(filter.Filter);
			AssertEquals(1, collection.Count);

			((ModuleDateFilter)filter["Run (Local)"]).PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			collection.Load(filter.Filter);
			AssertEquals(0, collection.Count);
		}

		#endregion

		#region Implementation

		protected virtual StmPrintJob GetNewJob()
		{
			return Factory.NewWithValidTestData<StmPrintJob>();
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new PrintJobFilterBusinessObject();
		}

		#endregion
	}
}
