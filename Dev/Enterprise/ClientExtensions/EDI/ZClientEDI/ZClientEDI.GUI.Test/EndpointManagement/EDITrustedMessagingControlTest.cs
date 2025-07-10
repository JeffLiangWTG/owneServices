using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.EndpointManagement.GUI.Testing
{
	public class EDITrustedMessagingControlTest : TestCaseWithFactory
	{
		public void TestUserControl()
		{
			var licHeader = BillingTestHelper.CreateLicence(Factory, "ENT");
			var system = licHeader.Database.GetOrCreateTrustedSystem();
			system.ETS_SystemID = "SID:123";
			system.ETS_Description = "DES:456";
			Factory.Save();

			using (var form = new LicenceDatabaseForm(licHeader.Database, null))
			{
				var tabpage = form.Controls.Find("TrustedMessagingTabPage", true).Single();
				form.Show();
				tabpage.Show();

				var productBox = tabpage.Controls.Find("ProductBox", true).Single() as ZDropEdit;
				var hasSecretKeyCheckBox = tabpage.Controls.Find("HasSecretKeyCheckBox", true).Single() as ZCheckBox;
				var trustedSystemFindButton = tabpage.Controls.Find("TrustedSystemFindBox", true).Single() as ZGuidFindBox;

				AssertEquals(true, productBox.ReadOnly);
				AssertEquals("ENT", productBox.Text);
				AssertEquals(true, hasSecretKeyCheckBox.ReadOnly);
				AssertEquals(false, hasSecretKeyCheckBox.Checked);
				AssertEquals(false, trustedSystemFindButton.ReadOnly);
				AssertEquals("ETS000001", trustedSystemFindButton.Text);

				form.Close();
			}
		}
	}
}
