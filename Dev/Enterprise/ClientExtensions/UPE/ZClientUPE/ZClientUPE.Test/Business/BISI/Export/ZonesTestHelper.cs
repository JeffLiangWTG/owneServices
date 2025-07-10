using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Client.UPE.Business.BISI
{
	public class ZonesTestHelper
	{
		// TODO - Move to Test project and UPETestHelper.
		public void CreateUPSZones(BusinessObjectFactory factory)
		{
			RateTransportProvider uPSTransportProvider = factory.New<RateTransportProvider>();
			uPSTransportProvider.TP_OH_RelatedParty = GlbCompany.CurrentCompany.OrgProxy.PK;
			uPSTransportProvider.TP_RN_NKCountry = "AU";

			var zone = uPSTransportProvider.Zones.AddNew();
			zone.TZ_ZoneName = "Sydney Metro";
			var item = zone.Items.AddNew();
			var postCode1 = factory.New<RefPostCode>();
			postCode1.RK_CityTownPostCode = "2000";
			item.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			var postCode2 = factory.New<RefPostCode>();
			postCode2.RK_CityTownPostCode = "2100";
			item.TQ_ToPostCode = postCode2.RK_CityTownPostCode;

			factory.Save();
		}
	}
}

