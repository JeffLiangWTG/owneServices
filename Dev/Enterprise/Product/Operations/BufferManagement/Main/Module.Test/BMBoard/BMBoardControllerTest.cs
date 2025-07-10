using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMBoardController))]
	class BMBoardControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMBoard;
		}

		#region ShowEditForm

		public void TestShouldOpenBoardEditForm_WhenBoardIsGlobal()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;
			staff.GS_GE_HomeDepartment = department.PK;

			Factory.Save();

			BMBoard board;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var system = Factory.NewWithValidTestData<BMSystem>();
				board = system.Boards.AddNew();
				board.MB_GS_NKStaffCode = staff.GS_Code;
				board.IsGlobal = true;

				Factory.Save();
			}

			AssertOpensBoardConfigForm(ODisplayMode.Browse, board, staff,
				allowStaffToView: false,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardEditForm_WhenBoardBelongsToCurrentCompany()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = staff.GS_Code;
			board.IsGlobal = false;

			Factory.Save();

			AssertOpensBoardConfigForm(ODisplayMode.Browse, board, staff,
				allowStaffToView: false,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardEditForm_WhenBoardBelongsToAnotherCompany()
		{
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = branch.PK;
			staff.GS_GE_HomeDepartment = department.PK;

			Factory.Save();

			BMBoard board;
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				var system = Factory.NewWithValidTestData<BMSystem>();
				board = system.Boards.AddNew();
				board.MB_GS_NKStaffCode = staff.GS_Code;
				board.IsGlobal = false;

				Factory.Save();
			}

			AssertOpensBoardConfigForm(ODisplayMode.Browse, board, staff,
				allowStaffToView: false,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: false,
				"Cannot open non-global visual boards belonging to other companies.");
		}

		public void TestShouldOpenBoardEditForm_WhenTheUserIsOwner()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = staff.GS_Code;

			Factory.Save();

			AssertOpensBoardConfigForm(ODisplayMode.Browse, board, staff,
				allowStaffToView: false,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardEditForm_WhenTheUserIsNotOwner_ButCanEditAllBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			AssertOpensBoardConfigForm(ODisplayMode.Browse, board, staff,
				allowStaffToView: false,
				allowStaffToEdit: true,
				allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardViewForm_WhenTheUserIsNotOwner_AndCannotEditAllBoards_ButCanViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			AssertOpensBoardConfigForm(ODisplayMode.ReadOnly, board, staff,
				allowStaffToView: true,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: false);
		}

		public void TestShouldInformAboutInsufficientBoardViewRights_WhenTheUserIsNotOwner_AndCannotEditAllBoards_AndCannotViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			AssertDisplaysMessageAboutInsufficientBoardViewRights(board, staff,
				allowStaffToView: false,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardEditForm_WhenTheUserIsWithinBoardReleaseGroup_AndHasRightToEditTeamBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = group.PK;

			Factory.Save();

			AssertOpensBoardConfigForm(ODisplayMode.Browse, board, staff,
				allowStaffToView: false,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: true);
		}

		public void TestShouldOpenBoardViewForm_WhenTheUserIsWithinBoardReleaseGroup_ButHasNoRightToEditTeamBoards_ButStillCanViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = group.PK;

			Factory.Save();

			AssertOpensBoardConfigForm(ODisplayMode.ReadOnly, board, staff,
				allowStaffToView: true,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: false);
		}

		public void TestShouldInformAboutInsufficientBoardViewRights_WhenTheUserIsWithinBoardReleaseGroup_ButHasNoRightToEditTeamBoards_AndCannotViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = group.PK;

			Factory.Save();

			AssertDisplaysMessageAboutInsufficientBoardViewRights(board, staff,
				allowStaffToView: false,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: false);
		}

		public void TestShouldOpenBoardViewForm_WhenTheUserHasRightToEditTeamBoards_ButIsNotWithinBoardReleaseGroup_ButStillCanViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;

			Factory.Save();

			AssertOpensBoardConfigForm(ODisplayMode.ReadOnly, board, staff,
				allowStaffToView: true,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: true);
		}

		public void TestShouldInformAboutInsufficientBoardViewRights_WhenTheUserHasRightToEditTeamBoards_ButIsNotWithinBoardReleaseGroup_AndCannotViewBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;

			Factory.Save();

			AssertDisplaysMessageAboutInsufficientBoardViewRights(board, staff,
				allowStaffToView: false,
				allowStaffToEdit: false,
				allowStaffToEditTeamBoards: true);
		}

		void AssertOpensBoardConfigForm(ODisplayMode expectedDisplayMode, BMBoard board, GlbStaff staff, bool allowStaffToView, bool allowStaffToEdit, bool allowStaffToEditTeamBoards, string errorMessage = null)
		{
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardView.IsAllowed = allowStaffToView;
				Env.Security.BMBoardEdit.IsAllowed = allowStaffToEdit;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = allowStaffToEditTeamBoards;

				var controller = new BMBoardController();
				using (var form = (ZForm)controller.ShowEditForm(board))
				{
					if (errorMessage == null)
					{
						AssertNotNull(form);
						AssertEquals(expectedDisplayMode, form.DisplayMode);
						AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertNull(form);
						AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
		}

		void AssertDisplaysMessageAboutInsufficientBoardViewRights(BMBoard board, GlbStaff staff, bool allowStaffToView, bool allowStaffToEdit, bool allowStaffToEditTeamBoards)
		{
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardView.IsAllowed = allowStaffToView;
				Env.Security.BMBoardEdit.IsAllowed = allowStaffToEdit;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = allowStaffToEditTeamBoards;

				var controller = new BMBoardController();
				using (var form = (ZForm)controller.ShowEditForm(board))
				{
					AssertNull(form);
					AssertEquals(Env.Security.BMBoardView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion
	}
}
