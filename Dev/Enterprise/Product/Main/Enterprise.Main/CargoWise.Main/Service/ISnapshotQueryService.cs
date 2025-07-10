using CargoWise.Main.Navigation;

namespace CargoWise.Main.Service;

internal interface ISnapshotQueryService
{
	public int QuerySnapShotResult(Snapshot snapshot);
}
