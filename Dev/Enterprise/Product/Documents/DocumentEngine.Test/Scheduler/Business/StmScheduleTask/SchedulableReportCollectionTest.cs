using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(SchedulableReportCollection))]
	sealed class SchedulableReportCollectionTest : StmMenuItemCollectionTest
	{
		public void TestRelationshipFilterWorks()
		{
			var report1 = Factory.New<StmMenuItem>();
			report1.SU_BusinessContext = "RepAReport";
			report1.SU_MenuName = "Test Report";

			var report2 = Factory.New<StmMenuItem>();
			report2.SU_BusinessContext = "RepAReport";
			report2.SU_MenuName = "Another Test Report";

			var document = Factory.New<StmMenuItem>();
			document.SU_BusinessContext = "ADocument";
			document.SU_MenuName = "Test Document";

			var reportPrivate = Factory.New<StmMenuItem>();
			reportPrivate.SU_IsPublished = false;
			reportPrivate.SU_GS_NKStaffCode = "OOO";
			reportPrivate.SU_BusinessContext = "RepBReport";
			reportPrivate.SU_MenuName = "Test Private Report";

			var collection = new SchedulableReportCollection(Factory);
			collection.Load();

			AssertCollectionContains(report1, collection);
			AssertCollectionContains(report2, collection);
			AssertCollectionNotContains(document, collection);
			AssertCollectionNotContains(reportPrivate, collection);

			collection.Load(new ZQuery(StmMenuItemSchema.SU_MenuName, SQLComparisonOperator.StartsWith, "Test"));
			AssertCollectionContains(report1, collection);
			AssertCollectionNotContains(report2, collection);
			AssertCollectionNotContains(document, collection);
			AssertCollectionNotContains(reportPrivate, collection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new SchedulableReportCollection(Factory);
		}
	}
}
