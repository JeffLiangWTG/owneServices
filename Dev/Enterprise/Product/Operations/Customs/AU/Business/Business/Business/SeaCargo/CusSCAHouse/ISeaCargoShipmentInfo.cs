using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaCargoShipmentInfo : IBusiness
	{
		CusSCAOceanBill OceanBill { get; }
		CusSCAHouse HouseBill { get; }
		bool IsVisible { get; }
		ZGlobalMutex Mutex { get; }
		SeaCargoSynchroniser SeaCargoSynchroniser { get; }
		IManifestProvider ManifestProvider { get; }
		void UnlockMutexIfNeeded();
		void SynchroniseIfWeCan();
		CusSCAHouse GetHouseBill { get; }
		bool RegisterTopLevelBusinessObjectAsEditable { get; }
		BusinessObject TopLevelObject { get; }
		void StartSynchronising();
		bool CanSynchronise { get; }
		bool NoSynchronisingWillOccur { get; }
		ZString SynchroniseFailureMessage { get; }
	}
}
