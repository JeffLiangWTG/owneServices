using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.GUI.Testing
{
	sealed class ExportSupportingDocumentsFieldsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSource()
		{
			AssertEquals("BindingSourceType", typeof(JobDeclaration), control.BindingSource.DataSourceType);
		}

		public void TestDynamicLayoutPanel()
		{
			var dynamicLayoutPanel = control.SupportingDocumentsFieldsDynamicLayoutPanel;
			CombineAssertions(() =>
			{
				AssertEquals("BindingMember", string.Empty, dynamicLayoutPanel.GetBindingMember());
				AssertEquals("GroupBox contains DynamicLayoutPanel", true, control.SupportingDocumentsGroupBox.Controls.Contains(dynamicLayoutPanel));
			});
		}

		public void TestGroupBox()
		{
			var groupBox = control.SupportingDocumentsGroupBox;
			AssertEquals("Caption", "[44] Supporting Documents", groupBox.CaptionResourceString.Caption);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new ExportSupportingDocumentsFieldsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		ExportSupportingDocumentsFieldsUserControl control;
	}
}
