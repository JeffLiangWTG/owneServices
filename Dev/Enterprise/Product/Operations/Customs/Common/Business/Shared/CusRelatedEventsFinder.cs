using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Common
{
	public static class CusRelatedEventsFinder
	{
		public static BusinessObject[] GetRelatedBizOs(Forwarding.IForwardingShipment shipment, ZString referenceNumber)
		{
			var provider = GetCusRelatedBizOsProvider();
			return provider.CusRelatedBusinessObjects(shipment, referenceNumber);
		}

		public static BusinessObject[] GetRelatedParentBizOs(Forwarding.IForwardingShipment shipment, ZString referenceNumber)
		{
			var provider = GetCusRelatedParentBizOsProvider();
			return provider.CusRelatedParentBusinessObjects(shipment);
		}

		internal static Integration.Customs.Shared.ICusRelatedParentEventsProvider GetCusRelatedParentBizOsProvider()
		{
			return GetCusRelatedParentBizOsProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static Integration.Customs.Shared.ICusRelatedParentEventsProvider GetCusRelatedParentBizOsProvider(string countryCode)
		{
			var providers = ObjectFactory.Get<Hashtable>("CusRelatedEventsProviders");
			var providerHandle = providers.ContainsKey(countryCode)
				? (ObjectHandle)providers[countryCode]
				: (ObjectHandle)providers["Shared"];
			return (Integration.Customs.Shared.ICusRelatedParentEventsProvider)providerHandle.GetObject();
		}

		internal static Integration.Customs.Shared.ICusRelatedEventsProvider GetCusRelatedBizOsProvider()
		{
			return GetCusRelatedBizOsProvider(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static Integration.Customs.Shared.ICusRelatedEventsProvider GetCusRelatedBizOsProvider(string countryCode)
		{
			var providers = ObjectFactory.Get<Hashtable>("CusRelatedEventsProviders");
			var providerHandle = providers.ContainsKey(countryCode)
				? (ObjectHandle)providers[countryCode]
				: (ObjectHandle)providers["Shared"];
			return (Integration.Customs.Shared.ICusRelatedEventsProvider)providerHandle.GetObject();
		}
	}
}
