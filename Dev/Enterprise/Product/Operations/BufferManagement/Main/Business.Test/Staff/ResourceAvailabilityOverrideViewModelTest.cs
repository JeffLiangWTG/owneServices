using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ResourceAvailabilityOverrideViewModel))]
	public class ResourceAvailabilityOverrideViewModelTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			return new ResourceAvailabilityOverrideViewModel(resource);
		}

		[TestDate(2014, 06, 13)]
		public void TestGetBMSLeaveForStaff()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var holiday = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now, ZDateTime.Now.AddDays(1), staff.PK);
			staff.Holidays.Add(holiday);

			var bmsLeave = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3), staff.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0);
			staff.Holidays.Add(bmsLeave);

			Factory.Save();

			var viewModel = new ResourceAvailabilityOverrideViewModel(staff);

			AssertEquals(1, viewModel.BMSLeave.Count);

			AssertEquals(BMConstants.BMSLeaveType, viewModel.BMSLeave.Single().GA_RecordType);
		}

		[TestDate(2014, 06, 13)]
		public void TestGetBMSLeaveForStaff_TwoStaff()
		{
			var staff1 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var holiday = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now, ZDateTime.Now.AddDays(1), staff1.PK);
			staff1.Holidays.Add(holiday);

			var bmsLeave1 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3), staff1.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0);
			staff1.Holidays.Add(bmsLeave1);

			var bmsLeave2 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3), staff2.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0);
			staff2.Holidays.Add(bmsLeave2);

			Factory.Save();

			var viewModel = new ResourceAvailabilityOverrideViewModel(staff1);

			AssertEquals(1, viewModel.BMSLeave.Count);

			var leave = viewModel.BMSLeave.Single();
			AssertEquals(BMConstants.BMSLeaveType, leave.GA_RecordType);
			AssertEquals(staff1.PK, leave.GA_GS);
		}

		[TestDate(2014, 06, 13)]
		public void TestGetNearFutureLeave()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);

			var holiday1 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now, ZDateTime.Now.AddDays(1), staff.PK); // normal holiday
			staff.Holidays.Add(holiday1);

			var holiday2 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3), staff.PK); // normal holiday
			staff.Holidays.Add(holiday2);

			var holiday3 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(-2), ZDateTime.Now.AddDays(-1), staff.PK); // holiday in past
			staff.Holidays.Add(holiday3);

			var holiday4 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(31), ZDateTime.Now.AddDays(35), staff.PK); // holiday outside 30 days
			staff.Holidays.Add(holiday4);

			var bmsLeave = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(3), ZDateTime.Now.AddDays(4), staff.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0); // bmsleave
			staff.Holidays.Add(bmsLeave);

			Factory.Save();

			var viewModel = new ResourceAvailabilityOverrideViewModel(staff);

			AssertEquals(1, viewModel.BMSLeave.Count);

			AssertCollectionContains(holiday1, viewModel.NearFutureLeave);
			AssertCollectionContains(holiday2, viewModel.NearFutureLeave);
			AssertCollectionNotContains(holiday3, viewModel.NearFutureLeave);
			AssertCollectionNotContains(holiday4, viewModel.NearFutureLeave);

			AssertEquals(2, viewModel.NearFutureLeave.Count);
		}

		public void TestViewModelSave_Unsuccessful()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			staff.Holidays.RemoveAndDeleteAll();
			var bmsLeave = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(3), ZDateTime.Now.AddDays(4), staff.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0); // bmsleave
			staff.Holidays.Add(bmsLeave);

			Factory.Save();

			var viewModel = new ResourceAvailabilityOverrideViewModel(staff);

			var leave = viewModel.BMSLeave.AddNew();
			leave.GA_AvailabilityPercentage = 101; // Invalid Percentage
			leave.GA_StartTime = ZDateTime.Now.AddDays(1);
			leave.GA_EndTime = ZDateTime.Now.AddDays(2);

			AssertEquals(false, leave.IsInDatabase);
			viewModel.Save();
			AssertEquals(false, leave.IsInDatabase);
			AssertEquals(1, leave.Notifications.Count(n => n.Type == NotificationType.Error));

			AssertEquals("Error - GA_AvailabilityPercentage: Please enter an 'Availability Percentage' within the range 0 to 100.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestViewModelSave_Successful()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			staff.Holidays.RemoveAndDeleteAll();
			var bmsLeave = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(3), ZDateTime.Now.AddDays(4), staff.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0); // bmsleave
			staff.Holidays.Add(bmsLeave);

			Factory.Save();

			var viewModel = new ResourceAvailabilityOverrideViewModel(staff);

			var leave = viewModel.BMSLeave.AddNew();
			leave.GA_AvailabilityPercentage = 69; // ;) Valid Percentage
			leave.GA_StartTime = ZDateTime.Now.AddDays(1);
			leave.GA_EndTime = ZDateTime.Now.AddDays(2);

			AssertEquals(false, leave.IsInDatabase);
			viewModel.Save();
			AssertEquals(true, leave.IsInDatabase);
		}

		public void TestViewModelSave_ConcurrencyHandling()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			staff.Holidays.RemoveAndDeleteAll();
			var bmsLeave = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(3), ZDateTime.Now.AddDays(4), staff.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0); // bmsleave
			staff.Holidays.Add(bmsLeave);

			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var staff2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory2);
			var bmsLeave2 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(3), ZDateTime.Now.AddDays(4), staff2.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0); // bmsleave
			staff2.Holidays.Add(bmsLeave2);

			var viewModel = new ResourceAvailabilityOverrideViewModel(staff);

			var leave = viewModel.BMSLeave.AddNew();
			leave.GA_AvailabilityPercentage = 69; // ;) Valid Percentage
			leave.GA_StartTime = ZDateTime.Now.AddDays(1);
			leave.GA_EndTime = ZDateTime.Now.AddDays(2);

			viewModel.Save();

			var leave2 = viewModel.BMSLeave.AddNew();
			leave2.GA_AvailabilityPercentage = 69; // ;) Valid Percentage
			leave2.GA_StartTime = ZDateTime.Now.AddDays(1);
			leave2.GA_EndTime = ZDateTime.Now.AddDays(2);

			viewModel.Save();

			AssertExceptionThrown<ZSaveException>(Factory.Save);
		}
	}
}
