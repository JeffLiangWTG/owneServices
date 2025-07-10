using System;
using System.Drawing;
using System.Windows.Forms;
using Moq;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class InstallationResultsFormTest : TestCase
	{
		InstallationResultsForm Form;

		protected override void SetUp()
		{
			base.SetUp();
			Form = InstallationResultsForm.New();
		}

		protected override void TearDown()
		{
			if (Form != null)
			{
				Form.Dispose();
			}

			base.TearDown();
		}

		public void TestFormBehaviour()
		{
			AssertEquals("Installation Results", Form.Text);
			Assert("OK button should close form", Form.OKButton.DialogResult != DialogResult.None);
			Assert("Text box should be read only", Form.WarningErrorTextBox.ReadOnly);
			AssertEquals(FormStartPosition.CenterParent, Form.StartPosition);
			AssertEquals(ContentAlignment.MiddleCenter, Form.WarningLabel.TextAlign);
			AssertEquals(ContentAlignment.MiddleCenter, Form.ErrorLabel.TextAlign);
		}

		public void TestFormShowsWarnings()
		{
			InstallationResultCollection results = new InstallationResultCollection();
			results.Add(InstallationResult.Warning("Warning one"));
			results.Add(InstallationResult.Warning("Warning two"));
			results.Add(InstallationResult.OK());
			results.Add(InstallationResult.OK());
			results.Add(InstallationResult.Warning("Warning three"));

			Form.LoadInstallationResults(results);
			Form.Show();

			Assert("Warning message visible", Form.WarningLabel.Visible);
			Assert("Error message hidden", !Form.ErrorLabel.Visible);
			string nL = Environment.NewLine;
			AssertEquals("Text box", "Warning one" + nL + nL + "Warning two" + nL + nL + "Warning three", Form.WarningErrorTextBox.Text);
		}

		public void TestFormShowsWarnings_AllWarningsAreNotAffectCargoWiseFunctionsWarning()
		{
			var results = new InstallationResultCollection
			{
				InstallationResult.NotAffectCargoWiseFunctionsWarning("Warning one"),
				InstallationResult.NotAffectCargoWiseFunctionsWarning("Warning two"),
				InstallationResult.OK(),
				InstallationResult.OK(),
				InstallationResult.NotAffectCargoWiseFunctionsWarning("Warning three")
			};

			Form.LoadInstallationResults(results);
			Form.Show();

			Assert("Warning message visible", Form.WarningLabel.Visible);
			Assert("Error message hidden", !Form.ErrorLabel.Visible);
			var nL = Environment.NewLine;
			AssertEquals("Warning one" + nL + nL + "Warning two" + nL + nL + "Warning three", Form.WarningErrorTextBox.Text);
			AssertEquals("Warning label should not say 'CargoWise functions will be affected' when all warnings are 'not affect CargoWise functions Warning'", "Warning! CargoWise One detected some problems. You or your IT administrator should address these problems.", Form.WarningLabel.Text);
		}

		public void TestFormShowsWarnings_NotAllWarningsAreNotAffectCargoWiseFunctionsWarning()
		{
			var results = new InstallationResultCollection
			{
				InstallationResult.NotAffectCargoWiseFunctionsWarning("Warning one"),
				InstallationResult.NotAffectCargoWiseFunctionsWarning("Warning two"),
				InstallationResult.OK(),
				InstallationResult.OK(),
				InstallationResult.Warning("Warning three")
			};

			Form.LoadInstallationResults(results);
			Form.Show();

			Assert("Warning message visible", Form.WarningLabel.Visible);
			Assert("Error message hidden", !Form.ErrorLabel.Visible);
			var nL = Environment.NewLine;
			AssertEquals("Warning one" + nL + nL + "Warning two" + nL + nL + "Warning three", Form.WarningErrorTextBox.Text);
			AssertEquals("Warning label should say 'CargoWise functions will be affected' when not all warnings are 'not affect CargoWise functions Warning'", "Warning! CargoWise One detected some problems. CargoWise One will still run, but performance may be severely reduced and some functions may not work. You or your IT administrator should address these problems.", Form.WarningLabel.Text);
		}

		public void TestFormShowsErrors()
		{
			InstallationResultCollection results = new InstallationResultCollection();
			results.Add(InstallationResult.Warning("Warning shouldn't be shown because errors are more important"));
			results.Add(InstallationResult.Error("Error one"));
			results.Add(InstallationResult.OK());
			results.Add(InstallationResult.OK());
			results.Add(InstallationResult.Error("Error two"));

			Form.LoadInstallationResults(results);
			Form.Show();

			Assert("Warning message hidden", !Form.WarningLabel.Visible);
			Assert("Error message visible", Form.ErrorLabel.Visible);
			string nL = Environment.NewLine;
			AssertEquals("Text box", "Error one" + nL + nL + "Error two", Form.WarningErrorTextBox.Text);
		}

		public void TestFactoryDefault()
		{
			AssertEquals("Factory should return InstallationResultsForm by default", typeof(InstallationResultsForm), Form.GetType());
		}

		public void TestFactoryOverride()
		{
			var formMock = new Mock<InstallationResultsForm>();
			formMock.CallBase = true;
			using (InstallationResultsForm.OverrideFactoryForTest(formMock.Object))
			{
				using (InstallationResultsForm form = InstallationResultsForm.New())
				{
					AssertSame("Factory should return overridden object", formMock.Object, form);
				}
			}

			using (InstallationResultsForm form = InstallationResultsForm.New())
			{
				AssertEquals("Factory should go back to default behaviour when override is disposed", typeof(InstallationResultsForm), form.GetType());
			}
		}
	}
}
