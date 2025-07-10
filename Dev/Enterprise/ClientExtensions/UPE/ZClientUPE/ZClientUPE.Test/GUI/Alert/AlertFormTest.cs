using System.Collections.Specialized;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI.Testing
{
	internal class AlertFormTest : TestCaseWithFactory
	{
		public void TestFormHeading()
		{
			using (AlertForm form = new AlertForm(new Alert(new StringCollection(), Factory)))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Alerts", form.Text);
			}
		}

		public void TestAlert()
		{
			using (AlertForm alertForm = new AlertForm(new Alert(new StringCollection(), Factory)))
			{
				AssertNotNull(alertForm.Alert);
			}
		}
	}
}
