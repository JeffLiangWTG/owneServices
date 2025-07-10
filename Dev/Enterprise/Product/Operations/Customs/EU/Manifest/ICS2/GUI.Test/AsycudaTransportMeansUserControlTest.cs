using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI.Test
{
	class AsycudaTransportMeansUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			bill.AsycudaTransportMeans.AddNew();

			using (var form = new ZForm(manifestHeader))
			using (var control = new AsycudaTransportMeansUserControl())
			{
				control.SetDataBinding(bill, string.Empty);

				form.Controls.Add(control);
				form.Show();

				EUICS2GUITestHelper.AssertGridLayout(control, "AsycudaTransportMeansGrid", "TPM_IdentificationNumber", "TPM_ReferenceNumber", "TPM_TypeOfIdentification", "TPM_TypeOfTransportMeans", "TPM_RN_NKTransportNationality");
			}
		}

		public void TestVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using (IAdditionalTabPage control = new AsycudaTransportMeansUserControl())
			{
				manifest.AMA_TransportMode = TransportModes.Rail;
				manifest.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F51;
				AssertEquals("Is visible", true, control.AdditionalControlVisibility.isVisible(manifest));

				manifest.SpecificCircumstanceIndicator = EUICS2SpecificCircumstanceList.Codes.F41;
				AssertEquals("Is not visible", false, control.AdditionalControlVisibility.isVisible(manifest));

				manifest.AMA_TransportMode = TransportModes.Air;
				AssertEquals("Is not visible", false, control.AdditionalControlVisibility.isVisible(manifest));

				manifest.AMA_TransportMode = TransportModes.Road;
				AssertEquals("Is visible", true, control.AdditionalControlVisibility.isVisible(manifest));
			}
		}
	}
}
