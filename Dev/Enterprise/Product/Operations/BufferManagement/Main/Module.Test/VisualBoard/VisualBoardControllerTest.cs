using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(VisualBoardController))]
	class VisualBoardControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.VisualBoard;
		}

		protected override IZForm GetEditFormToShow()
		{
			Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase());
			return Application.OpenForms.OfType<VisualBoardForm>().SingleOrDefault();
		}

		protected override void SetUp()
		{
			asyncDisableAction = BMSTestCaseWithFactory.DisableAsyncBehaviour();
			base.SetUp();
		}

		protected override void TearDown()
		{
			base.TearDown();
			asyncDisableAction.Dispose();
		}

		IDisposable asyncDisableAction;

		public override void TestSaveFormWithCustomsPlugIns()
		{
			Assert(true); // non Customs Controller
		}

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

			Controller.ShowEditForm(board);
			using (var visualBoardForm = Application.OpenForms.OfType<VisualBoardForm>().SingleOrDefault())
			{
				AssertNotNull("Global boards should be visible to any company.", visualBoardForm);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldOpenBoardEditForm_WhenBoardBelongsToCurrentCompany()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var system = Factory.NewWithValidTestData<BMSystem>();
			var board = system.Boards.AddNew();
			board.MB_GS_NKStaffCode = staff.GS_Code;
			board.IsGlobal = false;

			Factory.Save();

			Controller.ShowEditForm(board);
			using (var visualBoardForm = Application.OpenForms.OfType<VisualBoardForm>().SingleOrDefault())
			{
				AssertNotNull("Non-global boards belonging to current company should be visible.", visualBoardForm);
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
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

			Controller.ShowEditForm(board);
			using (var visualBoardForm = Application.OpenForms.OfType<VisualBoardForm>().SingleOrDefault())
			{
				AssertNull(visualBoardForm);
				AssertEquals("Cannot open non-global visual boards belonging to other companies.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
