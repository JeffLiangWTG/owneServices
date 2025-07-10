using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Module
{
	public class PostcodeZoneCodeDescriptionPairList : CodeDescriptionPairList
	{
		public PostcodeZoneCodeDescriptionPairList(BusinessObjectFactory factory)
		{
			ZQuery filter = new ZQuery(RateTransportProviderSchema.TP_OH_RelatedParty, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			RateTransportProvider[] providers = factory.Load<RateTransportProvider>(filter);

			AddPair(UPEFilterConstants.MetroCountry.Metro, UPEFilterConstants.MetroCountry.Metro);
			AddPair(UPEFilterConstants.MetroCountry.Other, UPEFilterConstants.MetroCountry.Other);
			AddPair("", "");

			foreach (RateTransportProvider provider in providers)
			{
				foreach (RateTransportZone zone in provider.Zones)
				{
					AddPair(zone.TZ_ZoneName, zone.TZ_ZoneName);
				}
			}
		}
	}
}
