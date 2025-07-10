using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	class AvailabilityMenuItemTest : BMSTestCaseWithFactory
	{
		public void TestAvailableForBMSWorkMenuItem()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.Holidays.RemoveAndDeleteAll();
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				AssertEquals(3, availabilityMenuItem.DropDownItems.Count);

				var todayMenuItem = FindTodayMenuItem(availabilityMenuItem);

				AssertEquals(2, todayMenuItem.DropDownItems.Count);

				var awayMenuItem = FindAwayMenuItem(todayMenuItem);
				var atWorkMenuItem = FindAtWorkMenuItem(todayMenuItem);

				AssertNotNull(awayMenuItem);
				AssertNotNull(atWorkMenuItem);

				var tomorrowMenuItem = FindTomorrowMenuItem(availabilityMenuItem);

				AssertEquals(2, tomorrowMenuItem.DropDownItems.Count);

				var tomorrowAwayMenuItem = FindAwayMenuItem(tomorrowMenuItem);
				var tomorrowAtWorkMenuItem = FindAtWorkMenuItem(tomorrowMenuItem);

				AssertNotNull(tomorrowAwayMenuItem);
				AssertNotNull(tomorrowAtWorkMenuItem);

				var advancedMenuItem = FindMenuItem(availabilityMenuItem, "Advanced");

				AssertNotNull(advancedMenuItem);
				AssertEquals(0, advancedMenuItem.DropDownItems.Count);
			}
		}

		#region AtWork

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestBMSWorkMenuItem_AtWorkTodayWithNoExistingBMSLeave()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.HolidaysIncBMSLeave.RemoveAndDeleteAll();
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				var preClickHolidayCount = resource.HolidaysIncBMSLeave.Count;

				var atWorkMenuItem = FindAtWorkMenuItem(FindTodayMenuItem(availabilityMenuItem));
				AssertNotNull(atWorkMenuItem);

				atWorkMenuItem.PerformClick();

				AssertEquals(preClickHolidayCount, resource.HolidaysIncBMSLeave.Count);
				AssertEquals(AvailabilityMenuItem.ResourceAlreadyAtWorkMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestBMSWorkMenuItem_AtWorkTodayWithExistingBMSLeave()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.HolidaysIncBMSLeave.RemoveAndDeleteAll();
			var existingHoliday = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), resource.PK, BMConstants.BMSLeaveType, BMConstants.BMSLeaveType, availabilityFactor: 0);
			resource.HolidaysIncBMSLeave.Add(existingHoliday);
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				var preClickHolidayCount = resource.HolidaysIncBMSLeave.Count;

				var atWorkMenuItem = FindAtWorkMenuItem(FindTodayMenuItem(availabilityMenuItem));
				AssertNotNull(atWorkMenuItem);

				atWorkMenuItem.PerformClick();

				AssertEquals(preClickHolidayCount - 1, resource.HolidaysIncBMSLeave.Count);
			}
		}

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestBMSWorkMenuItem_OutsideWorkingHoursWithNoExistingBMSLeave()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.HolidaysIncBMSLeave.RemoveAndDeleteAll();
			resource.WorkTimes.ThursdayWorkingHours = "                 *******************";
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				var atWorkMenuItem = FindAtWorkMenuItem(FindTodayMenuItem(availabilityMenuItem));
				AssertNotNull(atWorkMenuItem);

				atWorkMenuItem.PerformClick();
				AssertEquals(AvailabilityMenuItem.ResourceAlreadyAtWorkMessage, UnitTestUserNotification.Instance.LastMessage.Text);

				resource.WorkTimes.ThursdayWorkingHours = "           ";
				Factory.Save();

				atWorkMenuItem.PerformClick();
				AssertEquals(AvailabilityMenuItem.ResourceOutsideOfWorkingHoursMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Away

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestBMSWorkMenuItem_AwayToday()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.HolidaysIncBMSLeave.RemoveAndDeleteAll();
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				var preClickHolidayCount = resource.HolidaysIncBMSLeave.Count;

				var awayMenuItem = FindAwayMenuItem(FindTodayMenuItem(availabilityMenuItem));
				AssertNotNull(awayMenuItem);

				awayMenuItem.PerformClick();

				AssertEquals(preClickHolidayCount + 1, resource.HolidaysIncBMSLeave.Count);
				AssertHolidayIsBMSHoliday((GlbStaffHoliday)resource.HolidaysIncBMSLeave.FirstOrDefault(), ZDateTime.Today.Date, buffer.PK);
			}
		}

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestBMSWorkMenuItem_AwayKeepsExistingHoliday()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.HolidaysIncBMSLeave.RemoveAndDeleteAll();
			var existingHoliday = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), resource.PK, leaveDaysTaken: 10);
			resource.HolidaysIncBMSLeave.Add(existingHoliday);
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				var preClickHolidayCount = resource.HolidaysIncBMSLeave.Count;

				var awayMenuItem = FindAwayMenuItem(FindTodayMenuItem(availabilityMenuItem));
				AssertNotNull(awayMenuItem);

				awayMenuItem.PerformClick();

				AssertEquals(preClickHolidayCount, resource.HolidaysIncBMSLeave.Count);

				AssertEquals("A leave record of type: ANN exists.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestBMSWorkMenuItem_AwayTodayAlreadyHasAwayRecord()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.HolidaysIncBMSLeave.RemoveAndDeleteAll();
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				var preClickHolidayCount = resource.HolidaysIncBMSLeave.Count;

				var awayMenuItem = FindAwayMenuItem(FindTodayMenuItem(availabilityMenuItem));
				AssertNotNull(awayMenuItem);
				awayMenuItem.PerformClick();

				AssertEquals(preClickHolidayCount + 1, resource.HolidaysIncBMSLeave.Count);

				AssertHolidayIsBMSHoliday((GlbStaffHoliday)resource.HolidaysIncBMSLeave.FirstOrDefault(), ZDateTime.Today.Date, buffer.PK);

				awayMenuItem.PerformClick();
				AssertEquals(preClickHolidayCount + 1, resource.HolidaysIncBMSLeave.Count);
				AssertEquals("A leave record of type: BMS exists.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2013, 12, 5, 9, 0, 0)]
		public void TestBMSWorkMenuItem_AwayTomorrowAlreadyHasAwayRecord()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.HolidaysIncBMSLeave.RemoveAndDeleteAll();
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				var preClickHolidayCount = resource.HolidaysIncBMSLeave.Count;

				var awayMenuItem = FindAwayMenuItem(FindTomorrowMenuItem(availabilityMenuItem));
				AssertNotNull(awayMenuItem);
				awayMenuItem.PerformClick();

				AssertEquals(preClickHolidayCount + 1, resource.HolidaysIncBMSLeave.Count);

				AssertHolidayIsBMSHoliday((GlbStaffHoliday)resource.HolidaysIncBMSLeave.FirstOrDefault(), ZDateTime.Today.AddDays(1).Date, buffer.PK);

				awayMenuItem.PerformClick();
				AssertEquals(preClickHolidayCount + 1, resource.HolidaysIncBMSLeave.Count);
				AssertEquals("A leave record of type: BMS exists.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Advanced

		public void TestAdvancedAvailabilityForm_Cancel()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;
			var staff = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			var leave1 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(2), ZDateTime.Now.AddDays(3), staff.PK, recordType: BMConstants.BMSLeaveType, workHolidayType: BMConstants.BMSLeaveType, availabilityFactor: 1, comment: "111");
			var leave2 = BMSTestHelper.CreateStaffHoliday(Factory, ZDateTime.Now.AddDays(4), ZDateTime.Now.AddDays(5), staff.PK, recordType: BMConstants.BMSLeaveType, workHolidayType: BMConstants.BMSLeaveType, availabilityFactor: 2, comment: "222");
			staff.Holidays.Add(leave1);
			staff.Holidays.Add(leave2);
			Factory.Save();

			using (ZFormModaliser.SuspendDispose())
			using (var menuItem = new AvailabilityMenuItem(staff, buffer))
			{
				var advancedMenuItem = FindMenuItem(menuItem, "Advanced");
				advancedMenuItem.PerformClick();

				var advancedAvailabilityForm = ZFormModaliser.LastFormShownDialogForTest as AdvancedAvailabilityForm;
				AssertNotNull("GIVEN AdvancedAvailabilityForm shown", advancedAvailabilityForm);

				advancedAvailabilityForm.Show();

				var resourceAvailabilityOverrideViewModel = advancedAvailabilityForm.DataSource as ResourceAvailabilityOverrideViewModel;

				var grid = advancedAvailabilityForm.FindAll<ZGrid>().Single(c => c.Name == "bmsLeaveGrid");

				AssertEquals("GIVEN 2 bmsLeave rows", 2, grid.ListManager.List.Count);
				AssertContainsExactElementsInAnyOrder("GIVEN 2 bmsLeave rows", new[] { "111", "222" }, resourceAvailabilityOverrideViewModel.BMSLeave.Select(l => l.GA_LeaveComment));

				CreateUpdateDeleteLeave(resourceAvailabilityOverrideViewModel);

				var cancelButton = advancedAvailabilityForm.FindAll<ZButton>().Single(c => c.Name == "cancelButton");
				cancelButton.PerformClick();
				Application.DoEvents();

				advancedAvailabilityForm.Dispose();

				AssertEquals("THEN hit cancel hence form closed", true, advancedAvailabilityForm.IsDisposed);

				advancedMenuItem.PerformClick();
				advancedAvailabilityForm = ZFormModaliser.LastFormShownDialogForTest as AdvancedAvailabilityForm;
				AssertNotNull("THEN reopening AdvancedAvailabilityForm", advancedAvailabilityForm);
				advancedAvailabilityForm.Show();

				grid = advancedAvailabilityForm.FindAll<ZGrid>().Single(c => c.Name == "bmsLeaveGrid");
				AssertEquals("SHOULD only have 2 BMSLeave grid rows", 2, grid.ListManager.List.Count);
				resourceAvailabilityOverrideViewModel = advancedAvailabilityForm.DataSource as ResourceAvailabilityOverrideViewModel;
				AssertContainsExactElementsInAnyOrder("SHOULD only have 2 BMSLeave grid rows", new[] { "111", "222" }, resourceAvailabilityOverrideViewModel.BMSLeave.Select(l => l.GA_LeaveComment));

				CreateUpdateDeleteLeave(resourceAvailabilityOverrideViewModel);

				var saveButton = advancedAvailabilityForm.FindAll<ZButton>().Single(c => c.Name == "okButton");
				saveButton.PerformClick();
				Application.DoEvents();

				advancedMenuItem.PerformClick();
				advancedAvailabilityForm = ZFormModaliser.LastFormShownDialogForTest as AdvancedAvailabilityForm;
				AssertNotNull("WHEN save THEN reopening AdvancedAvailabilityForm", advancedAvailabilityForm);
				advancedAvailabilityForm.Show();

				grid = advancedAvailabilityForm.FindAll<ZGrid>().Single(c => c.Name == "bmsLeaveGrid");
				AssertEquals("SHOULD only have 2 BMSLeave grid rows", 2, grid.ListManager.List.Count);
				resourceAvailabilityOverrideViewModel = advancedAvailabilityForm.DataSource as ResourceAvailabilityOverrideViewModel;
				AssertContainsExactElementsInAnyOrder("SHOULD only have 2 BMSLeave grid rows i.e. 333 (new), XXX (updated from 111), 222 (deleted)", new[] { "XXX", "333" }, resourceAvailabilityOverrideViewModel.BMSLeave.Select(l => l.GA_LeaveComment));
			}
		}

		void CreateUpdateDeleteLeave(ResourceAvailabilityOverrideViewModel resourceAvailabilityOverrideViewModel)
		{
			var leave3 = resourceAvailabilityOverrideViewModel.BMSLeave.AddNew();
			leave3.GA_AvailabilityPercentage = 10;
			leave3.GA_StartTime = ZDateTime.Now.AddDays(10);
			leave3.GA_EndTime = ZDateTime.Now.AddDays(11);
			leave3.GA_LeaveComment = "333";

			AssertNotNull("WHEN adding leave3", resourceAvailabilityOverrideViewModel.BMSLeave.FirstOrDefault(l => l.GA_LeaveComment == "333"));

			var leave1 = resourceAvailabilityOverrideViewModel.BMSLeave.First(l => l.GA_LeaveComment == "111");
			leave1.GA_LeaveComment = "XXX";
			AssertEquals("WHEN updating leave1", "XXX", leave1.GA_LeaveComment);

			var leave2 = resourceAvailabilityOverrideViewModel.BMSLeave.First(l => l.GA_LeaveComment == "222");
			leave2.Delete();
			AssertNull("WHEN deleting leave2", resourceAvailabilityOverrideViewModel.BMSLeave.FirstOrDefault(l => l.GA_LeaveComment == "222"));
		}

		public void TestAdvancedFormShowsOnClick()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.Holidays.RemoveAndDeleteAll();
			Factory.Save();

			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				var advancedMenuItem = FindMenuItem(availabilityMenuItem, "Advanced");
				AssertNotNull(advancedMenuItem);
				advancedMenuItem.PerformClick();

				AssertEquals("AdvancedAvailabilityForm", ZFormModaliser.LastFormShownDialogForTest.Name);
			}
		}

		public void TestAdvancedForm_NotAllowed()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.Holidays.RemoveAndDeleteAll();
			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				Env.Security.BMBoardChangeBMSAvailabilityForCurrentUser.IsAllowed = false;

				var advancedMenuItem = FindMenuItem(availabilityMenuItem, "Advanced");
				AssertNotNull(advancedMenuItem);
				advancedMenuItem.PerformClick();

				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Buffer Management -> Visual Boards -> Set BMS Availability for Current User", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Security

		public void TestBMSAvailabilityItemSecurity_DoesntHavePermissionForAnotherUser()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.Holidays.RemoveAndDeleteAll();
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				Env.Security.BMBoardChangeBMSAvailabilityForOtherUsers.IsAllowed = false;
				var preClickHolidayCount = resource.Holidays.Count;

				var awayMenuItem = FindAwayMenuItem(FindTodayMenuItem(availabilityMenuItem));
				AssertNotNull(awayMenuItem);
				awayMenuItem.PerformClick();

				AssertEquals(preClickHolidayCount, resource.Holidays.Count);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Buffer Management -> Visual Boards -> Set BMS Availability for Other Users", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBMSAvailabilityItemSecurity_DoesntHavePermissionForSelf()
		{
			var buffer = BMSTestHelper.CreateSystemAndBuffer(Factory, "ORG").Item2;

			var resource = CreateStaffInCurrentBranchDept("MTX", "John Matrix");
			resource.Holidays.RemoveAndDeleteAll();
			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var availabilityMenuItem = new AvailabilityMenuItem(resource, buffer))
			{
				Env.Security.BMBoardChangeBMSAvailabilityForCurrentUser.IsAllowed = false;
				var preClickHolidayCount = resource.Holidays.Count;

				var awayMenuItem = FindAwayMenuItem(FindTodayMenuItem(availabilityMenuItem));
				AssertNotNull(awayMenuItem);
				awayMenuItem.PerformClick();

				AssertEquals(preClickHolidayCount, resource.Holidays.Count);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Buffer Management -> Visual Boards -> Set BMS Availability for Current User", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		void AssertHolidayIsBMSHoliday(GlbStaffHoliday holiday, ZDateTime start, ZGuid parentId)
		{
			AssertEquals(BMConstants.BMSLeaveType, holiday.GA_RecordType);
			AssertEquals((Byte)0, holiday.GA_AvailabilityPercentage);
			AssertEquals(parentId, holiday.GA_ParentID);
			if (start != ZDateTime.Empty)
			{
				AssertEquals(start, holiday.GA_StartTime);
			}
			AssertEquals(BMComponentSchema.Constants.Prefix, holiday.GA_ParentTableCode);
		}

		ToolStripMenuItem FindTodayMenuItem(AvailabilityMenuItem availabilityMenuItem)
		{
			return FindMenuItem(availabilityMenuItem, "Today");
		}

		ToolStripMenuItem FindTomorrowMenuItem(AvailabilityMenuItem availabilityMenuItem)
		{
			return FindMenuItem(availabilityMenuItem, "Tomorrow");
		}

		ToolStripMenuItem FindAwayMenuItem(ToolStripMenuItem menuItem)
		{
			return FindMenuItem(menuItem, "Away");
		}

		ToolStripMenuItem FindAtWorkMenuItem(ToolStripMenuItem menuItem)
		{
			return FindMenuItem(menuItem, "AtWork");
		}

		ToolStripMenuItem FindMenuItem(ToolStripMenuItem menuItem, string name)
		{
			return menuItem.DropDownItems.Find(name, true).FirstOrDefault() as ToolStripMenuItem;
		}

		#endregion
	}
}
