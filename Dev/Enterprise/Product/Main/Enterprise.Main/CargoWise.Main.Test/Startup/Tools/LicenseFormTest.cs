using System.Linq;
using System.Windows.Forms;
using CargoWise.Main.Startup.Tools;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Startup.Tools.Testing
{
	[TestedType(typeof(LicenseForm))]
	sealed class LicenseFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var form = new LicenseForm("McLaren");

			var txtLicense = form.Controls.Find("txtLicense", true).Single();
			var btnAccept = form.Controls.Find("btnAccept", true).Single();
			var btnDecline = form.Controls.Find("btnDecline", true).Single();

			MissingResourceStringChecker.ExcludeFromTest(txtLicense);
			MissingResourceStringChecker.ExcludeFromTest(btnAccept);
			MissingResourceStringChecker.ExcludeFromTest(btnDecline);

			return form;
		}

		public void TestAcceptClick_ReturnOkResult()
		{
			using (var form = new LicenseForm("McLaren"))
			{
				form.Show();

				var readLicense = (CheckBox)form.Controls.Find("chkHasReadLicense", true).Single();
				readLicense.Checked = true;

				var button = (Button)form.Controls.Find("btnAccept", true).Single();
				button.PerformClick();

				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestDeclineClick_ReturnCancelResult()
		{
			using (var form = new LicenseForm("McLaren"))
			{
				form.Show();

				var readLicense = (CheckBox)form.Controls.Find("chkHasReadLicense", true).Single();
				readLicense.Checked = true;

				var button = (Button)form.Controls.Find("btnDecline", true).Single();
				button.PerformClick();

				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}
	}
}
