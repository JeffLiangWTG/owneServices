using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(FutureLeaveCollection))]
	public class FutureLeaveCollectionTest : ActiveBusinessObjectCollectionTestCase<FutureLeaveCollection>
	{
		protected override FutureLeaveCollection GetCollectionToTest()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			return new FutureLeaveCollection(staff, new ZQuery());
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(FutureLeaveCollection);
		}

		[TestDate(2014, 06, 18)]
		public void TestCollectionReturnsLeaveWithinNearness()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			staff.Holidays.RemoveAndDeleteAll();

			var holiday1 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(-10), ZDateTime.Now.AddDays(-1), staff.PK);
			var holiday2 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(-10), ZDateTime.Now.AddDays(1), staff.PK);
			var holiday3 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3), staff.PK);
			var holiday4 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(-10), ZDateTime.Now.AddDays(20), staff.PK);
			var holiday5 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(3), ZDateTime.Now.AddDays(20), staff.PK);
			var holiday6 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(20), ZDateTime.Now.AddDays(21), staff.PK);

			staff.Holidays.Add(holiday1);
			staff.Holidays.Add(holiday2);
			staff.Holidays.Add(holiday3);
			staff.Holidays.Add(holiday4);
			staff.Holidays.Add(holiday5);
			staff.Holidays.Add(holiday6);

			Factory.Save();

			var filter = new ZQuery();
			filter.AddToFilter(FutureLeaveCollection.GetFilterForFutureLeaveWithinXDays(5));
			var collection = new FutureLeaveCollection(staff, filter);

			AssertCollectionNotContains(holiday1, collection);
			AssertCollectionContains(holiday2, collection);
			AssertCollectionContains(holiday3, collection);
			AssertCollectionContains(holiday4, collection);
			AssertCollectionContains(holiday5, collection);
			AssertCollectionNotContains(holiday6, collection);
			AssertEquals(4, collection.Count);
		}
	}
}
