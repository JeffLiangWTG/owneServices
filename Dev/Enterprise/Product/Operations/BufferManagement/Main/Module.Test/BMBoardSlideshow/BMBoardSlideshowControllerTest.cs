using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMBoardSlideshowController))]
	class BMBoardSlideshowControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMBoardSlideshow;
		}

		#region ShowEditForm

		public void TestShouldOpenSlideshowEditForm_WhenTheUserCanEditSlideshows()
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
				Env.Security.BMSlideShowsView.IsAllowed = true;
				Env.Security.BMSlideShowsEdit.IsAllowed = true;

				var controller = new BMBoardSlideshowController();
				using (var form = (ZForm)controller.ShowEditForm(slideshow))
				{
					AssertNotNull(form);
					AssertEquals(ODisplayMode.Browse, form.DisplayMode);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestShouldOpenSlideshowViewForm_WhenTheUserCannotEditSlideshows_ButCanViewSlideshows()
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
				Env.Security.BMSlideShowsView.IsAllowed = true;
				Env.Security.BMSlideShowsEdit.IsAllowed = false;

				var controller = new BMBoardSlideshowController();
				using (var form = (ZForm)controller.ShowEditForm(slideshow))
				{
					AssertNotNull(form);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
					AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestShouldInformAboutInsufficientSlideshowEditRights_WhenTheUserCannotEditSlideshows()
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
				Env.Security.BMSlideShowsView.IsAllowed = false;
				Env.Security.BMSlideShowsEdit.IsAllowed = false;

				var controller = new BMBoardSlideshowController();
				using (var form = (ZForm)controller.ShowEditForm(slideshow))
				{
					AssertNull(form);
					AssertEquals(Env.Security.BMSlideShowsView.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		#endregion
	}
}
