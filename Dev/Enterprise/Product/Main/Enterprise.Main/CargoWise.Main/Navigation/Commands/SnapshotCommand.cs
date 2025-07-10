using System;
using CargoWise.Main.Service;

namespace CargoWise.Main.Navigation;

static class SnapshotCommand
{
	internal static void FetchValue(object obj, ISnapshotQueryService service)
	{
		if (obj is Snapshot snapshot)
		{
			try
			{
				snapshot.ErrorMessage = string.Empty;
				snapshot.Value = $"{service.QuerySnapShotResult(snapshot):N0}";
			}
			catch (Exception ex)
			{
				snapshot.ErrorMessage = ex.Message;
			}
		}
	}

	public static void FetchValue(object obj) => FetchValue(obj, SnapshotQueryService.Instance);
}
