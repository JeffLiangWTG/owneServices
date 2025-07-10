using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.Module.Testing
{
	sealed class UPEJobDeclarationModuleTest : TestCaseWithDummy
	{
		public void TestLicenceAndSecurityCheckpoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
			AssertEquals(Env.Security.None, Module.SecurityCheckpoint);
		}

		public void TestModuleID()
		{
			AssertEquals(ClientModuleRegistration.JobDeclaration, Module.ID);
		}

		public void TestAllowNewAndDelete()
		{
			AssertEquals(true, Module.AllowNew);
			AssertEquals(false, Module.AllowDelete);
		}

		public void TestGetNewController()
		{
			AssertEquals(typeof(UPEJobDeclarationController), Module.GetNewController(Dummy).GetType());
		}

		public void TestGetNewFilterControl()
		{
			using (UPEJobDeclarationFilterControl filterControl = Module.GetNewFilterControl() as UPEJobDeclarationFilterControl)
			{
				AssertNotNull("Type should be UPEJobDeclarationFilterControl", filterControl);
			}
		}

		public void TestGetNewGridCollection()
		{
			JobDeclarationCollection gridCollection = Module.GetNewGridCollection() as JobDeclarationCollection;
			AssertNotNull(gridCollection);
			AssertEquals(Module.Factory, gridCollection.Factory);
		}

		public void TestGetNewFilterBusinessObject()
		{
			AssertEquals(typeof(UPEJobDeclarationFilterBusinessObject), Module.GetNewFilterBusinessObject().GetType());
		}

		public void TestBulkStatusUpdatingToolbarButton()
		{
			ZToolBarButton bulkStatusUpdateButton = (ZToolBarButton)Module.ToolBarButtons.FindByText("Bulk Status Update");
			AssertNotNull("The button should exist", bulkStatusUpdateButton);
			JobDeclaration dec1 = Factory.New<JobDeclaration>();
			JobDeclaration dec2 = Factory.New<JobDeclaration>();
			Factory.Save();
			Module.SelectedGridElements = new BusinessObject[] { dec1, dec2 };
			bulkStatusUpdateButton.PerformClick();
			AssertEquals("ShowDialog should have been called on the form", true, Module.LastCreatedBulkStatusUpdatingForm.ShowDialogWasCalled);
			AssertEquals("There should be 2 items selected for bulk update", 2, ((BulkStatusUpdating)Module.LastCreatedBulkStatusUpdatingForm.LastDataSourceForTest).ItemsToBulkUpdate.Length);
			Module.LastCreatedBulkStatusUpdatingForm.Dispose();
		}

		public void TestGetNewActionMenu()
		{
			MenuItem[] menuItemCollection = Module.GetNewActionMenuItems();
			foreach (MenuItem menuItem in menuItemCollection)
			{
				if (menuItem.Text == "Move To Classifier Queue")
				{
					menuItem.PerformClick();
					AssertEquals("Warning You must select a declaration to move.", UnitTestUserNotification.Instance.LastMessage.ToString());
					break;
				}
			}
		}

		public void TestMoveToClassifierQueue()
		{
			JobDeclaration dec1 = Factory.New<JobDeclaration>();
			JobDeclaration dec2 = Factory.New<JobDeclaration>();
			Factory.Save();
			Module.fSelectedElements = Array.Empty<BusinessObject>();
			Module.PerformOnMoveToClassifierQueue_Click();
			AssertEquals("Warning You must select a declaration to move.", UnitTestUserNotification.Instance.LastMessage.ToString());
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Module.fSelectedElements = new BusinessObject[] { dec1, dec2 };
			Module.PerformOnMoveToClassifierQueue_Click();
			AssertEquals("Warning You may only move one declaration at a time.", UnitTestUserNotification.Instance.LastMessage.ToString());
			Module.fSelectedElements = new BusinessObject[] { dec1 };
			Module.PerformOnMoveToClassifierQueue_Click();
			AssertEquals(DeclarationQueueCodeDescriptionPairList.Codes.Classification, dec1.CurrentQueue.P4_CustomsQueue);
			AssertEquals("", dec1.CurrentQueue.P4_CustomsStatus);
		}

		#region Setup
		UPEJobDeclarationModuleForTest Module;
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
			Module = new UPEJobDeclarationModuleForTest();
		}

		protected override void TearDown()
		{
			Module.Dispose();
			base.TearDown();
		}
		#endregion
	}
}
