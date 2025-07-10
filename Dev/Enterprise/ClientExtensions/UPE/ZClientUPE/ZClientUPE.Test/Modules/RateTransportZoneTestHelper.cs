using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Client.UPE.Module.Testing
{
	internal static class RateTransportZoneTestHelper
	{
		internal static RateTransportZoneItem CreateTransportProviderWithPostcodeRange(BusinessObjectFactory factory, ZGuid transportProviderOrgPK, ZString fromPostCode, ZString toPostCode, string zoneName = UPSTestZoneName)
		{
			var rateTransport = factory.NewWithValidTestData<RateTransportProvider>();
			rateTransport.TP_OH_RelatedParty = transportProviderOrgPK;
			rateTransport.TP_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			var zone = rateTransport.Zones.AddNew();
			zone.TZ_ZoneName = zoneName;
			var item = zone.Items.AddNew();
			var postCode1 = factory.New<RefPostCode>();
			postCode1.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			postCode1.RK_CityTownPostCode = fromPostCode;
			item.TQ_FromPostCode = postCode1.RK_CityTownPostCode;
			var postCode2 = factory.New<RefPostCode>();
			postCode2.RK_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			postCode2.RK_CityTownPostCode = toPostCode;
			item.TQ_ToPostCode = postCode2.RK_CityTownPostCode;
			return item;
		}

		internal const string UPSTestZoneName = "UPSZone";
	}
}
