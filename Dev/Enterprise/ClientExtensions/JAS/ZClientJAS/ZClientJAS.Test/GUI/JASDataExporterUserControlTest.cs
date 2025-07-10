using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.JAS.Business;
using Enterprise.Client.JAS.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.JAS.GUI
{
	[TestedType(typeof(ZChildForm))]
	class JASDataExporterUserControlTest : ZFormBasherTest
	{
		public void TestBindingForIsVisibleForBindingPropertyAreSet()
		{
			JASDataExporterBizOForTest bizO = new JASDataExporterBizOForTest();
			UserControl.SetDataBinding(bizO, "");
			AssertIsVisibleForBindingZBinding(UserControl.EmailPanel.DataBindings, bizO, JASDataExporterBizO.Schema.IsEmailDeliveryMethod);
			AssertIsVisibleForBindingZBinding(UserControl.DirectoryPanel.DataBindings, bizO, JASDataExporterBizO.Schema.IsDirectoryDeliveryMethod);
			AssertIsVisibleForBindingZBinding(UserControl.EmailGroupGuidFindBox.DataBindings, bizO, JASDataExporterBizO.Schema.IsGroupEmailRecipient);
			AssertIsVisibleForBindingZBinding(UserControl.EmailTextBox.DataBindings, bizO, JASDataExporterBizO.Schema.IsIndividualEmailRecipient);
		}

		public void TestBrowseExportDirectory()
		{
			using (JASDataExporterUserControl userControl = new JASDataExporterUserControl())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				ZFormModaliser.PathToSelectInShowCommonDialog = "Test MEH MEH";
				userControl.BrowseButton.PerformClick();
				AssertEquals("Should be empty, dialog was cancelled", "", userControl.ExportDirectoryTextBox.Text);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.PathToSelectInShowCommonDialog = "Test MEH MEH";
				userControl.BrowseButton.PerformClick();
				AssertEquals("Test MEH MEH", userControl.ExportDirectoryTextBox.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			ZChildForm result = new ZChildForm();
			result.Size = new Size(400, 300);
			UserControl.SetDataBinding(new JASDataExporterBizOForTest(), "");
			result.Controls.Add(UserControl);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			UserControl = new JASDataExporterUserControl();
		}

		protected override void TearDown()
		{
			UserControl.Dispose();
			base.TearDown();
		}

		void AssertIsVisibleForBindingZBinding(ControlBindingsCollection bindingCollection, object expectedDataSource, string expectedDataMember)
		{
			foreach (Binding binding in bindingCollection)
			{
				if (binding is KBinding && binding.PropertyName == "IsVisibleForBinding")
				{
					AssertEquals("IsVisibleForBinding", binding.PropertyName);
					AssertEquals(expectedDataSource, binding.DataSource);
					AssertEquals(expectedDataMember, binding.BindingMemberInfo.BindingMember);
					return;
				}
			}

			Fail("IsVisibleForBinding property is not bound");
		}

		JASDataExporterUserControl UserControl;
	}
}
