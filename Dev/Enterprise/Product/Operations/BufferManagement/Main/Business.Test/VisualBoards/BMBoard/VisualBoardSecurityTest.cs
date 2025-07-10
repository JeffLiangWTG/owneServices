using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business.Test
{
	class VisualBoardSecurityTest : BMSTestCaseWithFactory
	{
		#region Boards

		public void TestCanCurrentUserEditBoard_OwnerOfBoard()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = staff.GS_Code;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardEdit.IsAllowed = false;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = false;

				AssertEquals(true, VisualBoardSecurity.CanCurrentUserEditBoard(board));
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanCurrentUserEditBoard_NotOwnerOfBoard()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardEdit.IsAllowed = false;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = false;

				AssertEquals(false, VisualBoardSecurity.CanCurrentUserEditBoard(board));
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanCurrentUserEditBoard_CanEditAllBoards()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardEdit.IsAllowed = true;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = false;

				AssertEquals(true, VisualBoardSecurity.CanCurrentUserEditBoard(board));
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanCurrentUserEditBoard_TeamBoard_NoPermission()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = group.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardEdit.IsAllowed = false;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = false;

				AssertEquals(false, VisualBoardSecurity.CanCurrentUserEditBoard(board));
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanCurrentUserEditBoard_TeamBoard_HasPermission()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = group.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardEdit.IsAllowed = false;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = true;

				AssertEquals(true, VisualBoardSecurity.CanCurrentUserEditBoard(board));
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanCurrentUserEditBoard_AnotherTeamsBoard()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			staff.Groups.Add(group);

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GG_ReleaseGroup = Factory.NewWithValidTestData<GlbGroup>().PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMBoardEdit.IsAllowed = false;
				Env.Security.BMBoardEditTeamBoards.IsAllowed = true;

				AssertEquals(false, VisualBoardSecurity.CanCurrentUserEditBoard(board));
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestIsVisibleToCurrentCompany()
		{
			var otherBranch = Factory.NewWithValidTestData<GlbBranch>();
			var otherDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			var otherStaff = Factory.NewWithValidTestData<GlbStaff>();
			otherStaff.GS_GB_HomeBranch = otherBranch.PK;
			otherStaff.GS_GE_HomeDepartment = otherDepartment.PK;

			Factory.Save();

			var system = BMSTestHelper.CreateSystem(Factory, "WKI");

			var myGlobalBoard = BMSTestHelper.CreateBoard(system, "My global board");
			myGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myGlobalBoard.IsGlobal = true;

			var myNonGlobalBoard = BMSTestHelper.CreateBoard(system, "My non-global board");
			myNonGlobalBoard.MB_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			myNonGlobalBoard.IsGlobal = false;

			BMBoard otherStaffGlobalBoard;
			BMBoard otherStaffNonGlobalBoard;
			using (Env.SetTemporaryUserContext(otherStaff.PK.ToGuid(), otherBranch.PK.ToGuid(), otherDepartment.PK.ToGuid()))
			{
				otherStaffGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's global board");
				otherStaffGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffGlobalBoard.IsGlobal = true;

				otherStaffNonGlobalBoard = BMSTestHelper.CreateBoard(system, "Someone else's non-global board");
				otherStaffNonGlobalBoard.MB_GS_NKStaffCode = otherStaff.GS_Code;
				otherStaffNonGlobalBoard.IsGlobal = false;
			}

			AssertEquals("Global boards should be visible to any company.", true, VisualBoardSecurity.IsVisibleToCurrentCompany(myGlobalBoard));
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("Non-global boards belonging to current company should be visible.", true, VisualBoardSecurity.IsVisibleToCurrentCompany(myNonGlobalBoard));
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("Global boards should be visible to any company.", true, VisualBoardSecurity.IsVisibleToCurrentCompany(otherStaffGlobalBoard));
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals(false, VisualBoardSecurity.IsVisibleToCurrentCompany(otherStaffNonGlobalBoard));
			AssertEquals("Cannot open non-global visual boards belonging to other companies.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Slideshows

		public void TestCanCurrentUserEditSlideshow_IsAllowed_ShouldBeTrue()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "Mew Board");
			var board2 = BMSTestHelper.CreateBoard(system, "Mewtwo Board");
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);
			slideshow.MD_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMSlideShowsEdit.IsAllowed = true;

				AssertEquals(true, VisualBoardSecurity.CanCurrentUserEditBoard(slideshow));
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanCurrentUserEditSlideshow_IsAllowed_ShouldBeFalse()
		{
			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory);
			var system = BMSTestHelper.CreateSystem(Factory);
			var board1 = BMSTestHelper.CreateBoard(system, "Pikachu Board");
			var board2 = BMSTestHelper.CreateBoard(system, "Raichu Board");
			var slideshow = BMSTestHelper.CreateSlideshow(Factory, board1, board2);
			slideshow.MD_GS_NKStaffCode = Factory.NewWithValidTestData<GlbStaff>().GS_Code;

			Factory.Save();

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.BMSlideShowsEdit.IsAllowed = false;

				AssertEquals(false, VisualBoardSecurity.CanCurrentUserEditBoard(slideshow));
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion
	}
}
