using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;
using ApplicationCodeTypeList = Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	class ItineraryForManifestHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var form = new ZForm(manifest))
			using (var control = new ItineraryForManifestHeaderUserControl())
			{
				control.SetDataBinding(manifest, string.Empty);

				form.Controls.Add(control);
				form.Show();

				EUICS2GUITestHelper.AssertGridLayout(control, "itineraryGrid", "CY_Order", "CY_Data", "CountryName");
			}
		}

		public void TestVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (var control = new ItineraryForManifestHeaderUserControl() as ASYCUDA.GUI.IAdditionalTabPage)
			{
				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				Assert("Is visible on VOC (Carrier) Manifest", control.AdditionalControlVisibility.isVisible(manifest));

				manifest.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				Assert("Is visible on NVC (Forwarder) Manifest", control.AdditionalControlVisibility.isVisible(manifest));
			}
		}
	}
}
