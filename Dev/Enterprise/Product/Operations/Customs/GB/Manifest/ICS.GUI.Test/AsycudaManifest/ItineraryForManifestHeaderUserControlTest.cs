using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.ManifestBase.ApplicationCodeTypeList;

namespace Enterprise.Customs.GB.ICS.GUI.Testing
{
	class ItineraryForManifestHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestLayout()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EUImportControlSystem, Core.Constants.CountryCodes.UnitedKingdom, ZDateTime.Now, value: true))
			{
				var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

				using var form = new ZForm(manifest);
				using var control = new ItineraryForManifestHeaderUserControl();
				control.SetDataBinding(manifest, string.Empty);

				form.Controls.Add(control);
				form.Show();

				var itineraryGrid = control.FindSingle<ZArchitecture.ZGrid>("itineraryGrid");
				AssertEquals("itineraryGrid visible", expected: true, itineraryGrid.Visible);

				var orderColumnInfo = itineraryGrid.GetColumnStyle("CY_Order");
				AssertEquals("orderColumn visible", expected: true, orderColumnInfo.IsVisible);
				var dataColumnInfo = itineraryGrid.GetColumnStyle("CY_Data");
				AssertEquals("dataColumn visible", expected: true, dataColumnInfo.IsVisible);
			}
		}

		public void TestAdditionalControlVisibility()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			using var control = new ItineraryForManifestHeaderUserControl();
			var additionalTabPageVisibility = ((IAdditionalTabPage)control).AdditionalControlVisibility;

			manifest.AMA_ApplicationCode = Codes.ShippingLine;
			AssertEquals($"AMA_ApplicationCode = {manifest.AMA_ApplicationCode}", expected: true, additionalTabPageVisibility.isVisible(manifest));

			manifest.AMA_ApplicationCode = Codes.Consolidator;
			AssertEquals($"AMA_ApplicationCode = {manifest.AMA_ApplicationCode}", expected: true, additionalTabPageVisibility.isVisible(manifest));

			manifest.AMA_ApplicationCode = Codes.BreakBulk;
			AssertEquals($"AMA_ApplicationCode = {manifest.AMA_ApplicationCode}", expected: false, additionalTabPageVisibility.isVisible(manifest));
		}
	}
}
