using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Common
{
	public static class DeclarationBeingCreatedForShipmentMutexCreator
	{
		public static ZGlobalMutex Create(ZGuid shipmentPK)
		{
			return Create(shipmentPK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
		}

		public static ZGlobalMutex Create(ZGuid shipmentPK, ZString countryCode)
		{
			return new ZGlobalMutex(MutexIDs.JobBeingCreatedForShipment, shipmentPK.ToString() + countryCode);
		}
	}
}

