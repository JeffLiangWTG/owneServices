using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Module
{
	[TestedType(typeof(ResourceStringsController))]
	class ResourceStringsControllerTest : ZArchitecture.Modules.Testing.ZControllerBasherTest
	{
		public void TestBatchDeleteCancelsIfAnyOfSelectedResourcesIsNotCheckedOutToCurrentUser()
		{
			HelpDataString res1 = new HelpDataString();
			res1.HD_Code = "key1";
			res1.HD_IsCheckedOut = false;
			res1.HD_Caption = "hello";

			HelpDataString res2 = new HelpDataString();
			res2.HD_Code = "key2";
			res2.HD_IsCheckedOut = false;
			res2.HD_Caption = "world";

			ZController controller = ZControllerFactory.Create(ControllerIDs.ResourceStrings);
			controller.DeleteMultiple(new BusinessObject[] { res1, res2 });
			AssertEquals("One of the resource strings is not checked out to you. Cannot proceed with undo.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(false, res1.IsDeleted);
			AssertEquals(false, res2.IsDeleted);

			res1.HD_IsCheckedOut = true;
			res2.HD_IsCheckedOut = true;

			controller = ZControllerFactory.Create(ControllerIDs.ResourceStrings);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.DeleteMultiple(new BusinessObject[] { res1, res2 });

			Assert(UnitTestUserNotification.Instance.LastMessage.Contains("Are you sure you want to undo 2 resource strings?"));

			AssertEquals(true, res1.IsDeleted);
			AssertEquals(true, res2.IsDeleted);
		}

		public override void TestEditForm()
		{
			HelpDataString helpDataString = new HelpDataString();
			helpDataString.HD_Code = "key1";
			helpDataString.HD_IsCheckedOut = false;
			helpDataString.HD_Caption = "hello";
			helpDataString.HD_FullDescription = "description";

			ZController controller = ZControllerFactory.Create(ControllerIDs.ResourceStrings);
			using (ZForm form = (ZForm)controller.ShowEditForm(helpDataString))
			{
				AssertEquals("Should be editable", false, ((BusinessObject)form.BusinessEntity).ReadOnly);
				AssertNotEquals("Should be new", helpDataString.PK, ((BusinessObject)form.BusinessEntity).PK);
			}

			helpDataString.HD_Code = "key2";
			helpDataString.HD_IsCheckedOut = true;
			helpDataString.HD_Caption = "teapot";
			helpDataString.HD_FullDescription = "description";
			using (ZForm form = (ZForm)controller.ShowEditForm(helpDataString))
			{
				AssertEquals("Should be editable", false, ((BusinessObject)form.BusinessEntity).ReadOnly);
				AssertEquals("Should be same", helpDataString.HD_Code, ((HelpDataString)form.BusinessEntity).HD_Code);
			}
		}

		public override void TestNewForm()
		{
			ZController controller = ZControllerFactory.Create(ControllerIDs.ResourceStrings);
			using (ZForm form = (ZForm)controller.ShowNewForm())
			{
				AssertEquals("Should be editable", false, ((BusinessObject)form.BusinessEntity).ReadOnly);
			}
		}

		public override void TestViewForm()
		{
			Assert(true);
		}

		public override void TestDeleteForm()
		{
			HelpDataString hd = new HelpDataString();
			hd.HD_Code = "key2";
			hd.HD_IsCheckedOut = false;
			hd.HD_Caption = "hello";
			hd.HD_FullDescription = "description";

			ZController controller = ZControllerFactory.Create(ControllerIDs.ResourceStrings);
			using (ZForm form = (ZForm)controller.ShowDeleteForm(hd))
			{
				AssertNull(form);
				AssertEquals("This resource string is not checked out to you. You cannot undo it.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			hd.HD_IsCheckedOut = true;
			hd.HD_Caption = "teapot";
			hd.HD_FullDescription = "description";

			controller = ZControllerFactory.Create(ControllerIDs.ResourceStrings);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			controller.ShowDeleteForm(hd);
			AssertEquals(true, hd.IsDeleted);
		}

		public void TestModuleID()
		{
			ResourceStringsController controller = (ResourceStringsController)ZControllerFactory.Create(GetControllerID());
			AssertEquals(ModuleIDs.ResourceStrings, controller.ModuleID);
		}

		#region Implementation

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ResourceStrings;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return new HelpDataString();
		}

		protected override void SetUp()
		{
			mockSources = ResourceStringsFactory.MockSources();
			base.SetUp();
		}

		protected override void TearDown()
		{
			if (mockSources != null)
			{
				mockSources.Dispose();
			}
			base.TearDown();
		}

		IDisposable mockSources;

		#endregion
	}
}
