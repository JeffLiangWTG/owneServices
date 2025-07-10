using System;
using NUnit.Framework;

namespace Enterprise.Core.Forms
{
	sealed class LicenceErrorForm_Test : TestCase
	{
		public void TestLicenceErrorForm()
		{
			using (var form = new LicenceErrorForm("Modulename", "text"))
			{
				AssertEquals("Module name text", form.ModuleNameLabel.Text, "Modulename");
				AssertEquals("Licence Error text", form.LicenceErrorRichBox.Text, "text");
			}
		}

		public void OKButtonClicked()
		{
			using (var form = new LicenceErrorForm("Modulename", "text"))
			{
				form.OKButton_Click(null, EventArgs.Empty);
				Assert("Form should be disposed", form.IsDisposed);
			}
		}
	}
}
