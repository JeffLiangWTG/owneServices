using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZGeographyEditWithFactoryTest : TestCaseWithFactory
	{
		public void TestInvalidToEmpty()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			dummy.Z0_Geography = new ZGeography("POINT (-122 47)");

			using (var form = new ZChildForm())
			{
				var testGeographyEdit =
					new ZGeographyEdit
					{
						BindTo = AutoDummyBizo.Schema.Z0_Geography
					};
				form.Controls.Add(testGeographyEdit);

				form.SetDataBinding(dummy, "");
				var textBox = new ZTextBox();
				form.Controls.Add(textBox);
				form.Show();

				UserIdleWorker.Flush();
				testGeographyEdit.GeographyTextBox.Text = "INVLD";
				textBox.Focus();

				AssertEquals("Not Valid after invalid input", false, dummy.Z0_Geography.IsValid);
				dummy.Z0_Geography = ZGeography.Empty;

				AssertEquals("Text cleared when set to empty", "", testGeographyEdit.GeographyTextBox.Text);
			}
		}

		#region Copy and Cut

		[DeveloperOnlyTest]
		public void TestCopyAndCut()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			using (var form = new ZChildForm())
			{
				var testGeographyEdit = new ZGeographyEdit { TabIndex = 0, BindTo = AutoDummyBizo.Schema.Z0_Geography };
				form.Controls.Add(testGeographyEdit);
				form.SetDataBinding(dummy, "");
				form.Show();

				testGeographyEdit.Focus();
				SafeClipboard.Clear();
				testGeographyEdit.GeographyTextBox.Text = "POINT (-122 47)";
				var mes = new Message();
				testGeographyEdit.ProcessCmdKeyExposed(ref mes, Keys.Control | Keys.C);
				System.Threading.Thread.Sleep(50);
				AssertEquals("POINT (-122 47)", SafeClipboard.GetText());
				AssertEquals("POINT (-122 47)", testGeographyEdit.GeographyTextBox.Text);

				SafeClipboard.Clear();
				testGeographyEdit.ProcessCmdKeyExposed(ref mes, Keys.Control | Keys.X);
				System.Threading.Thread.Sleep(50);
				AssertEquals("POINT (-122 47)", SafeClipboard.GetText());
				AssertEquals("", testGeographyEdit.GeographyTextBox.Text);
			}
		}

		#endregion

	}
}
