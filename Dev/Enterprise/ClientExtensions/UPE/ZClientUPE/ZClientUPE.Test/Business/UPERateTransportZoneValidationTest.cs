using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class UPERateTransportZoneValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestValidateTZ_ZoneName()
		{
			RateTransportProvider transportProvider = Factory.New<RateTransportProvider>();
			transportProvider.TP_OH_RelatedParty = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			UPERateTransportZone zone = (UPERateTransportZone)transportProvider.Zones.AddNew();
			zone.TZ_ZoneName = "splaty";
			AssertHasErrors("Should have errors because neither Metro nor Country are specified", zone.TZ_ZoneNameInfo);
			zone.TZ_ZoneName = "splaty MetRo";
			AssertNoErrors("Should not have errors because Metro specified", zone.TZ_ZoneNameInfo);
			zone.TZ_ZoneName = "splaty CounTry";
			AssertNoErrors("Should not have errors because Country specified", zone.TZ_ZoneNameInfo);
			zone.TZ_ZoneName = "splaty MehMetroMeh";
			AssertHasErrors("Should have errors because neither Metro nor Country are specified", zone.TZ_ZoneNameInfo);
			zone.TZ_ZoneName = "splaty MehCountryMeh";
			AssertHasErrors("Should have errors because neither Metro nor Country are specified", zone.TZ_ZoneNameInfo);
			transportProvider.TP_OH_RelatedParty = ZGuid.NewZGuid();
			zone.TZ_ZoneName = "splaty";
			AssertNoErrors("Should not have errors because the transport provider isn't the current company org proxy", zone.TZ_ZoneNameInfo);
		}
	}
}
