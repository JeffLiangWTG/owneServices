using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmActivityLogCollectionByStaff))]
	sealed class StmActivityLogCollectionByStaffTest : BusinessObjectCollectionTestCase
	{
		[TestDate(2012, 12, 4, 11, 0, 0)]
		[TestUtcOffset(10, 0, 0)]
		public void TestFilter()
		{
			IGlbStaff staff = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff.GS_Code = "ZUB";
			StmActivityLogCollectionByStaff logs = new StmActivityLogCollectionByStaff(staff);

			AssertEquals(ZDateTime.Today, logs.ActivityLogFilterDateFromLocal);
			AssertEquals(ZDateTime.Today.AddDays(1), logs.ActivityLogFilterDateToLocal);
			AssertEquals(StmActivityLogCollection.ActivityTypeAll, logs.ActivityLogFilterType);
			AssertEquals(ZString.Empty, logs.ActivityLogFilterFormCaption);

			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_GS_NKUser = staff.GS_Code;
			log.S7_OpenDateTimeUtc = new ZDateTime(2006, 5, 28);
			log.S7_FormCaption = "Microsoft Visual Studio";
			log.S7_EnterpriseActivity = false;

			StmActivityLog log2 = Factory.New<StmActivityLog>();
			log2.S7_GS_NKUser = staff.GS_Code;
			log2.S7_OpenDateTimeUtc = new ZDateTime(2006, 6, 28);
			log2.S7_FormCaption = "Company";
			log2.S7_EnterpriseActivity = true;

			StmActivityLog log3 = Factory.New<StmActivityLog>();
			log3.S7_GS_NKUser = staff.GS_Code;
			log3.S7_OpenDateTimeUtc = new ZDateTime(2006, 8, 28);
			log3.S7_FormCaption = "Some Visual Form";
			log3.S7_EnterpriseActivity = true;

			logs.Load();
			AssertEquals(0, logs.Count);

			logs.ActivityLogFilterDateFromUtc = new ZDateTime(2006, 5, 1);
			logs.ActivityLogFilterDateToUtc = new ZDateTime(2006, 5, 30);
			logs.Load();
			AssertEquals(1, logs.Count);
			AssertEquals(log, logs[0]);

			logs.ActivityLogFilterDateFromUtc = new ZDateTime(2006, 5, 1);
			logs.ActivityLogFilterDateToUtc = new ZDateTime(2006, 6, 30);
			logs.Load();
			AssertEquals(2, logs.Count);

			logs.ActivityLogFilterType = StmActivityLogCollection.ActivityTypeThisApp;
			logs.Load();
			AssertEquals(1, logs.Count);
			AssertEquals(log2, logs[0]);

			logs.ActivityLogFilterType = StmActivityLogCollection.ActivityTypeExternal;
			logs.Load();
			AssertEquals(1, logs.Count);
			AssertEquals(log, logs[0]);

			logs.ActivityLogFilterDateFromUtc = new ZDateTime(2006, 5, 1);
			logs.ActivityLogFilterDateToUtc = new ZDateTime(2006, 8, 30);
			logs.ActivityLogFilterType = StmActivityLogCollection.ActivityTypeAll;
			logs.ActivityLogFilterFormCaption = "Visual";
			logs.Load();
			AssertEquals(2, logs.Count);

			logs.ActivityLogFilterFormCaption = "";
			logs.Load();
			AssertEquals(3, logs.Count);
		}

		public void TestReadOnly()
		{
			IGlbStaff staff = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff.GS_Code = "ZUB";
			StmActivityLog log = Factory.New<StmActivityLog>();
			log.S7_GS_NKUser = staff.GS_Code;

			IGlbStaff staff2 = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			staff2.GS_Code = "RAK";
			StmActivityLog otherUserLog = Factory.New<StmActivityLog>();
			otherUserLog.S7_GS_NKUser = staff2.GS_Code;

			StmActivityLogCollectionByStaff logs = new StmActivityLogCollectionByStaff(staff);
			AssertEquals(true, logs.ReadOnly);
			AssertEquals(0, logs.Count);

			logs.ActivityLogFilterDateFromUtc = ZDateTime.Empty;
			logs.ActivityLogFilterDateToUtc = ZDateTime.Empty;
			logs.Load();

			AssertEquals(1, logs.Count);
			AssertEquals(log, logs[0]);
			AssertEquals(true, logs.ReadOnly);
		}

		[TestDate(2012, 12, 4, 12, 0, 0)]
		[TestUtcOffset(5, 0, 0)]
		public void TestActivityLogFilterDateToLocal()
		{
			var staff = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			StmActivityLogCollectionByStaff logs = new StmActivityLogCollectionByStaff(staff);

			logs.ActivityLogFilterDateToLocal = new ZDateTime(2012, 12, 4, 19, 0, 0);
			AssertEquals(new ZDateTime(2012, 12, 4, 19, 0, 0), logs.ActivityLogFilterDateToLocal);
			AssertEquals(new ZDateTime(2012, 12, 4, 14, 0, 0), logs.ActivityLogFilterDateToUtc);

			logs.ActivityLogFilterDateToLocal = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, logs.ActivityLogFilterDateToLocal);
			AssertEquals(ZDateTime.Empty, logs.ActivityLogFilterDateToUtc);
		}

		[TestDate(2012, 1, 1, 0, 0, 0)]
		[TestUtcOffset(5, 0, 0)]
		public void TestActivityLogFilterDateFromLocal()
		{
			var staff = (IGlbStaff)Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			StmActivityLogCollectionByStaff logs = new StmActivityLogCollectionByStaff(staff);

			logs.ActivityLogFilterDateFromLocal = new ZDateTime(2012, 12, 4, 19, 0, 0);
			AssertEquals(new ZDateTime(2012, 12, 4, 19, 0, 0), logs.ActivityLogFilterDateFromLocal);
			AssertEquals(new ZDateTime(2012, 12, 4, 14, 0, 0), logs.ActivityLogFilterDateFromUtc);

			logs.ActivityLogFilterDateFromLocal = ZDateTime.Empty;
			AssertEquals(ZDateTime.Empty, logs.ActivityLogFilterDateFromLocal);
			AssertEquals(ZDateTime.Empty, logs.ActivityLogFilterDateFromUtc);
		}

		public void TestAllowNew()
		{
			IGlbStaff staff = Factory.New<IGlbStaff>();
			StmActivityLogCollectionByStaff logs = new StmActivityLogCollectionByStaff(staff);
			AssertEquals(false, logs.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			IGlbStaff staff = Factory.New<IGlbStaff>();
			return new StmActivityLogCollectionByStaff(staff);
		}
	}
}
