using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	sealed class ReferralRequestUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(header))
			using (var control = new ReferralRequestUserControl())
			{
				control.SetDataBinding(header, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var requestHeadersGrid = control.FindSingle<ZGrid>("RequestHeadersGrid");
				Assert("Request headers grid is visible", requestHeadersGrid.Visible);
				EUICS2GUITestHelper.AssertGridLayout(control, "RequestHeadersGrid", "EUS_Identifier", "EUS_Type", "RequestTypeDescription", "EUS_ScreeningMethod", "EUS_TransportDocumentType", "EUS_MemberState", "EUS_Status");

				var requestHeaderDetailsUserControl = control.FindSingle<ReferralRequestHeaderDetailsUserControl>("EUICS2ReferralRequestHeaderDetailsUserControl");
				Assert("Request header details user control is visible", requestHeaderDetailsUserControl.Visible);
			}
		}

		public void TestReadonly_RequestHeadersGridShoudBeReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(header))
			using (var control = new ReferralRequestUserControl())
			{
				control.SetDataBinding(header, string.Empty);

				form.Controls.Add(control);
				form.Show();

				Assert(control.FindSingle<ZGrid>("RequestHeadersGrid").ReadOnly);
			}
		}
	}
}
