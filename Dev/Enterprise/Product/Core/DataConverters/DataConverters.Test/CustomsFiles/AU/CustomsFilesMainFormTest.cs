using System.Windows.Forms;
using Enterprise.DataConverters.CustomsFiles;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DataConverters.Testing.CustomsFiles.AU
{
	[TestedType(typeof(MainForm))]
	sealed internal class CustomsFilesMainFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MainForm();
		}

		public void TestInterbaseRadioButton_CheckedChanged()
		{
			using (var testForm = new MainForm())
			{
				testForm.InterbaseRadioButton.Checked = false;
				AssertEquals(false, testForm.ConnectionTextBox.Enabled);
				AssertEquals(false, testForm.ConnectionButton.Enabled);

				testForm.InterbaseRadioButton.Checked = true;
				AssertEquals(true, testForm.ConnectionTextBox.Enabled);
				AssertEquals(true, testForm.ConnectionButton.Enabled);
			}
		}
	}
}
