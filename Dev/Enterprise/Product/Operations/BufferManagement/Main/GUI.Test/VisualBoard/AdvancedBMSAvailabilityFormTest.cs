using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(AdvancedAvailabilityForm))]
	public class AdvancedBMSAvailabilityFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var viewModel = new ResourceAvailabilityOverrideViewModel(resource);
			return new AdvancedAvailabilityForm(viewModel, resource);
		}

		public void TestClickSaveSavesViewModel()
		{
			var resource = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var viewModel = new ResourceAvailabilityOverrideViewModel(resource);

			var leave = viewModel.BMSLeave.AddNew();
			leave.GA_StartTime = ZDateTime.Now;
			leave.GA_EndTime = ZDateTime.Now;

			AssertEquals(false, leave.IsInDatabase);

			using (var form = new AdvancedAvailabilityForm(viewModel, resource))
			{
				form.Show();

				var button = (ZButton)form.Controls.Find("okButton", true).Single();
				button.PerformClick();

				AssertEquals(true, leave.IsInDatabase);
			}
		}

		#region Future Leave Grid

		public void TestFutureLeaveGrid_WhenFormBoundToOtherUser_AndViewingUserHasNoPermissionToViewOthersLeave_ShouldNotBeVisible_AndExplanationShouldBeVisible()
		{
			var currentUser = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition: the user should not have permission to view all leave.", false, Env.Security.StaffLeave.IsAllowed);
				AssertEquals("Precondition: the user should not have permission to view others' leave.", false, Env.Security.StaffViewOtherLeave.IsAllowed);

				var otherUser = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
				var viewModel = new ResourceAvailabilityOverrideViewModel(otherUser);
				AssertEquals("Precondition: the user should not be allowed to view others' leave.", false, GlbStaffVisibilityHelper.CanShowLeave(otherUser));

				using (var form = new AdvancedAvailabilityForm(viewModel, otherUser))
				{
					form.Show();
					Application.DoEvents();

					var grid = form.FindSingleOrDefault<ZGrid>("futureLeaveGrid");
					var label = form.FindSingleOrDefault<ZLabel>("LeaveViewNotAllowedLabel");

					AssertEquals("The user doesn't have permission to view someone else's leave so the grid should be invisible.", false, grid.Visible);
					AssertEquals(true, label.Visible);
				}
			}
		}

		public void TestFutureLeaveGrid_WhenFormBoundToCurrentUser_AndViewingUserHasNoPermissionToViewOthersLeave_ShouldBeVisible_AndExplanationShouldNotBeVisible()
		{
			var currentUser = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition: the user should not have permission to view all leave.", false, Env.Security.StaffLeave.IsAllowed);
				AssertEquals("Precondition: the user should not have permission to view others' leave.", false, Env.Security.StaffViewOtherLeave.IsAllowed);

				var viewModel = new ResourceAvailabilityOverrideViewModel(currentUser);

				using (var form = new AdvancedAvailabilityForm(viewModel, currentUser))
				{
					form.Show();
					Application.DoEvents();

					var grid = form.FindSingleOrDefault<ZGrid>("futureLeaveGrid");
					var label = form.FindSingleOrDefault<ZLabel>("LeaveViewNotAllowedLabel");

					AssertEquals("The user is viewing their own leave so the grid should be visible.", true,
						grid.Visible);
					AssertEquals(false, label.Visible);
				}
			}
		}

		public void TestFutureLeaveGrid_WhenFormBoundToOtherUser_AndViewingUserHasPermissionToViewOthersLeave_ShouldBeVisible_AndExplanationShouldNotBeVisible()
		{
			var currentUser = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition: the user should not have permission to view all leave.", false, Env.Security.StaffLeave.IsAllowed);
				Env.Security.StaffViewOtherLeave.IsAllowed = true;

				var otherUser = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
				var viewModel = new ResourceAvailabilityOverrideViewModel(otherUser);

				using (var form = new AdvancedAvailabilityForm(viewModel, otherUser))
				{
					form.Show();
					Application.DoEvents();

					var grid = form.FindSingleOrDefault<ZGrid>("futureLeaveGrid");
					var label = form.FindSingleOrDefault<ZLabel>("LeaveViewNotAllowedLabel");

					AssertEquals("The user has permission to view someone else's leave so the grid should be visible.", true, grid.Visible);
					AssertEquals(false, label.Visible);
				}
			}
		}

		public void TestFutureLeaveGrid_WhenFormBoundToCurrentUser_AndViewingUserHasPermissionToViewOthersLeave_ShouldBeVisible_AndExplanationShouldNotBeVisible()
		{
			var currentUser = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			Factory.Save();

			using (Env.SetTemporaryUserContext(currentUser.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				AssertEquals("Precondition: the user should not have permission to view all leave.", false, Env.Security.StaffLeave.IsAllowed);
				Env.Security.StaffViewOtherLeave.IsAllowed = true;

				var viewModel = new ResourceAvailabilityOverrideViewModel(currentUser);

				using (var form = new AdvancedAvailabilityForm(viewModel, currentUser))
				{
					form.Show();
					Application.DoEvents();

					var grid = form.FindSingleOrDefault<ZGrid>("futureLeaveGrid");
					var label = form.FindSingleOrDefault<ZLabel>("LeaveViewNotAllowedLabel");

					AssertEquals("The user is viewing their own leave so the grid should be visible.", true, grid.Visible);
					AssertEquals(false, label.Visible);
				}
			}
		}

		#endregion
	}
}
