using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.ExitControl.GUI;
using Enterprise.Customs.IE.ExitControl.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	class ExitControlUploadSupportingDocumentsMenuCreatorTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			CombineAssertions(() =>
			{
				var uploadSupportingDocumentsMenuItem = new ExitControlUploadSupportingDocumentsMenuCreator(null).Create();
				AssertEquals("Upload Supporting Documents", uploadSupportingDocumentsMenuItem.Text);

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					var form = (ExitControlMessageSendingForm)obj;
					AssertType<ExitControlMessageSendingObjectParent>("MessageSendingObjectParent", form.MessageSendingObjectParent);
				});
				uploadSupportingDocumentsMenuItem.PerformClick();
			});
		}

		[RequiresSTA]
		public void TestUploadSupportingDocumentsMenuItem_OnClick()
		{
			var exitHeder = Factory.New<CusExitHeader>();
			var report1 = exitHeder.CusExitReports.AddNew();
			var report2 = exitHeder.CusExitReports.AddNew();

			using (var form = new ExitControlForm(exitHeder))
			{
				form.Show();

				var exitControlUserControl = form.FindSingle<ExitControlUserControl>();
				var exitControlTabControl = exitControlUserControl.ExitControlTabControl;
				exitControlTabControl.SelectedTab = exitControlUserControl.ReportsTabPage;

				var reportsTabUserControl = exitControlUserControl.FindSingle<ReportsTabUserControl>();
				var grid = reportsTabUserControl.FindSingle<ZGrid>("ReportsGrid");
				grid.Select(0);
				grid.Select(1);
				var exitControlMenuItem = form.Menu.MenuItems.FindByText("E&xit Control");
				var uploadSupportingDocumentsMenuItem = exitControlMenuItem.MenuItems.FindByText("Upload Supporting Documents");
				UnitTestUserNotification.Instance.ClearMessages();
				uploadSupportingDocumentsMenuItem.PerformClick();
				AssertEquals("Multi selection is not allowed.", UnitTestUserNotification.Instance.LastMessage.Text);

				grid.UnSelectAll();
				UnitTestUserNotification.Instance.ClearMessages();
				uploadSupportingDocumentsMenuItem.PerformClick();
				AssertEquals("Please select an Exit Report first.", UnitTestUserNotification.Instance.LastMessage.Text);

				grid.Select(0);
				UnitTestUserNotification.Instance.ClearMessages();
				uploadSupportingDocumentsMenuItem.PerformClick();
				AssertEquals("'Entry/Consignment' must have a valid value before uploading supporting documents.", UnitTestUserNotification.Instance.LastMessage.Text);

				var consignment1 = exitHeder.CusExitConsignments.AddNew();
				var consignmentItem1 = consignment1.CusExitConsignmentItems.AddNew();
				consignmentItem1.CusExitConsignmentPackagePivots.AddNew();
				report1.CER_CXC_Consignment = consignment1.PK;
				UnitTestUserNotification.Instance.ClearMessages();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(obj =>
				{
					AssertType<DocumentsSendingForm>(obj);
				});
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				uploadSupportingDocumentsMenuItem.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
