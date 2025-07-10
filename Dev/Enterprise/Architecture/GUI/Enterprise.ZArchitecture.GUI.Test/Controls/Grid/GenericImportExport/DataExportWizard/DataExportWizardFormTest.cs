using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.DataMapping.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.DataMapping.Testing
{
	[TestedType(typeof(DataExportWizardForm))]
	sealed class DataExportWizardFormTest : ZFormBasherTest
	{
		public void TestFileNameExpressionTextBoxNotEnabled()
		{
			var helper = new ExportWizardTest.ExportWizardTestHelper(Factory);

			using (var form = new DataExportWizardForm(helper.CollectionInfo, null))
			{
				form.Show();

				var fieldInfo = form.GetType().GetField("FileNameExpressionTextBox", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				var textBox = (ZTextBox)fieldInfo.GetValue(form);
				AssertEquals("FileNameExpressionTextBox.Enabled should be false", false, textBox.Enabled);
				AssertEquals("FileNameExpressionTextBox.Visible should be false", false, textBox.Visible);

				var wizard = (ExportWizard)form.BusinessEntity;

				AssertEquals("ExportWizard should not have FileNameExpression set", string.Empty, wizard.FileNameExpression);
				AssertNull("ExportWizard should not have FileNameExpressionObject set", wizard.FileNameExpressionObject);
			}
		}

		public void TestFileNameExpressionTextBoxEnabled()
		{
			var helper = new ExportWizardTest.ExportWizardTestHelper(Factory);

			using (var form = new DataExportWizardForm(helper.CollectionInfo, null))
			{
				form.Show();

				var wizard = (ExportWizard)form.BusinessEntity;
				wizard.FileNameExpressionObject = wizard;
				wizard.FileNameExpression = "\"filename.csv\"";

				AssertEquals("ExportWizard should not have FileNameExpression set", "\"filename.csv\"", wizard.FileNameExpression);
				AssertNotNull("ExportWizard should not have FileNameExpressionObject set", wizard.FileNameExpressionObject);

				var fieldInfo = form.GetType().GetField("FileNameExpressionTextBox", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
				var textBox = (ZTextBox)fieldInfo.GetValue(form);
				AssertEquals("FileNameExpressionTextBox.Enabled should be false", true, textBox.Enabled);
				AssertEquals("FileNameExpressionTextBox.Visible should be false", true, textBox.Visible);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DataExportWizardForm(Helper.GetMultiTypeCollectionInfo(System.Array.Empty<BusinessObject>()), "");
		}

		ExportWizardTest.ExportWizardTestHelper Helper
		{
			get
			{
				if (helper == null)
				{
					helper = new ExportWizardTest.ExportWizardTestHelper(Factory);
				}

				return helper;
			}
		}

		ExportWizardTest.ExportWizardTestHelper helper;

		#endregion
	}
}
