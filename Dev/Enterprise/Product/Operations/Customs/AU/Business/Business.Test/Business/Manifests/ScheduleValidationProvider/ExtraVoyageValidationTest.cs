using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	internal sealed class ExtraVoyageValidationTest : BusinessObjectValidationTestCase
	{
		#region JV_VoyageType

		public void TestValidateJV_VoyageType()
		{
			const string warningMainVoyageSlotManifest = "You have flagged this sailing schedule as Main, but a Slot Manifest for this vessel-voyage already exists under Shipping>Customs Export Manifest.";
			const string warningSlotVoyageMainManifest = "You have flagged this sailing schedule as Slot, but a Main Manifest for this vessel-voyage already exists under Shipping>Customs Export Manifest.";

			voyage.Validation.ValidateJV_VoyageType();
			AssertNoNotifications("Valid Voyage Type", voyage.JV_VoyageTypeInfo);

			voyage.JV_VoyageType = "AAA";
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningMainVoyageSlotManifest);
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningSlotVoyageMainManifest);

			voyage.JV_VoyageType = Core.Constants.VoyageType.MainVoyage;
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningMainVoyageSlotManifest);
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningSlotVoyageMainManifest);

			manifest.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			voyage.Validation.ValidateJV_VoyageType();
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningMainVoyageSlotManifest);
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningSlotVoyageMainManifest);

			manifest.ED_ManifestType = ManifestTypeList.Codes.SlotExportSubManifest;
			voyage.Validation.ValidateJV_VoyageType();
			AssertHasWarning("Should be Warning", voyage.JV_VoyageTypeInfo, warningMainVoyageSlotManifest);
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningSlotVoyageMainManifest);

			voyage.JV_VoyageType = Core.Constants.VoyageType.SlotVoyage;
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningMainVoyageSlotManifest);
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningSlotVoyageMainManifest);

			manifest.ED_ManifestType = ManifestTypeList.Codes.ExportMainManifest;
			voyage.Validation.ValidateJV_VoyageType();
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningMainVoyageSlotManifest);
			AssertHasWarning("Should be Warning", voyage.JV_VoyageTypeInfo, warningSlotVoyageMainManifest);

			manifest.ED_ManifestType = ManifestTypeList.Codes.DepartureReport;
			voyage.Validation.ValidateJV_VoyageType();
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningMainVoyageSlotManifest);
			AssertNoWarning("Should be NO Warning", voyage.JV_VoyageTypeInfo, warningSlotVoyageMainManifest);
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.SetCountry("AU");

			vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Code = "Vessel";

			voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
			voyage.JV_VoyageType = ZString.Empty;
			voyage.JV_VoyageFlight = "123456";
			voyage.JV_RV_NKVessel = vessel.RV_Code;

			manifest = Factory.NewWithValidTestData<ExportCustomsManifestHeader>();
			manifest.ED_VesselName = vessel.RV_Code;
			manifest.ED_VoyageNumber = voyage.JV_VoyageFlight;
			manifest.ED_TransportMode = voyage.JV_AirSeaRoad;
		}

		JobVoyage voyage;
		RefVessel vessel;
		ExportCustomsManifestHeader manifest;

		#endregion
	}
}
