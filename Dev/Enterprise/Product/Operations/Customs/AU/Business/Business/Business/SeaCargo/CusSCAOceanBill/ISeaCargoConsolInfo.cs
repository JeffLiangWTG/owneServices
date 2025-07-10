using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Data.Mutex;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface ISeaCargoConsolInfo : IBusiness
	{
		BusinessObject TopLevelObject { get; }
		CusSCAOceanBill OceanBill { get; }
		bool IsVisible { get; }
		ZGlobalMutex Mutex { get; }
		SeaCargoSynchroniser SeaCargoSynchroniser { get; }
		IManifestProvider ManifestProvider { get; }
		void UnlockMutexIfNeeded();
		bool CanSynchronise { get; }
		bool NoSynchronisingWillOccur { get; }
		ZString SynchroniseFailureMessage { get; }
	}
}
