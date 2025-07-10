using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmActivityLogCollectionByParent))]
	sealed class StmActivityLogCollectionByParentTest : BusinessObjectCollectionTestCase
	{
		[TestDate(2012, 12, 4, 11, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestFilter()
		{
			IGlbStaff staff = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff.GS_Code = "ZUB";

			IGlbStaff staff2 = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff2.GS_Code = "RAK";

			DummyBusinessObject bizo = Factory.New<DummyBusinessObject>();
			DummyBusinessObject bizo2 = Factory.New<DummyBusinessObject>();
			StmActivityLogCollectionByParent logs = new StmActivityLogCollectionByParent(bizo);

			AssertEquals(ZDateTime.Today, logs.ActivityLogFilterDateFromLocal);
			AssertEquals(ZDateTime.Today.AddDays(1), logs.ActivityLogFilterDateToLocal);
			AssertEquals(StmActivityLogCollection.ActivityTypeAll, logs.ActivityLogFilterType);
			AssertEquals(ZString.Empty, logs.ActivityLogFilterFormCaption);

			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_GS_NKUser = staff.GS_Code;
			log.S7_OpenDateTimeUtc = new ZDateTime(2006, 5, 28);
			log.S7_FormCaption = "Microsoft Visual Studio";
			log.S7_ParentID = bizo.PK;
			log.S7_ParentTableCode = "Z0";

			StmActivityLog log2 = Factory.New<StmActivityLog>();
			log2.S7_GS_NKUser = staff.GS_Code;
			log2.S7_OpenDateTimeUtc = new ZDateTime(2006, 6, 28);
			log2.S7_FormCaption = "Company";
			log2.S7_EnterpriseActivity = true;
			log2.S7_ParentID = bizo2.PK;
			log2.S7_ParentTableCode = "Z0";

			StmActivityLog log3 = Factory.New<StmActivityLog>();
			log3.S7_GS_NKUser = staff2.GS_Code;
			log3.S7_OpenDateTimeUtc = new ZDateTime(2006, 8, 28);
			log3.S7_FormCaption = "Some Visual Form";
			log3.S7_EnterpriseActivity = true;
			log3.S7_ParentID = bizo.PK;
			log3.S7_ParentTableCode = "Z0";

			logs.Load();
			AssertEquals(0, logs.Count);

			logs.ActivityLogFilterDateFromUtc = new ZDateTime(2006, 5, 1);
			logs.ActivityLogFilterDateToUtc = new ZDateTime(2006, 5, 30);
			logs.Load();
			AssertEquals(1, logs.Count);
			AssertEquals(log, logs[0]);

			logs.ActivityLogFilterDateFromUtc = new ZDateTime(2006, 5, 1);
			logs.ActivityLogFilterDateToUtc = new ZDateTime(2006, 8, 30);
			logs.Load();
			AssertEquals(2, logs.Count);

			logs.ActivityLogFilterStaff = staff2.GS_Code;
			logs.Load();
			AssertEquals(1, logs.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			IGlbStaff staff = Factory.New<IGlbStaff>();
			return new StmActivityLogCollectionByParent((BusinessObject)staff);
		}
	}
}
