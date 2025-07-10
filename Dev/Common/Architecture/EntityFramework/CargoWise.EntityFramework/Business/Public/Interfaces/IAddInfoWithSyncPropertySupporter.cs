using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public interface IAddInfoWithSyncPropertySupporter
	{
		ZGuid PK { get; }
		IAddInfoWithSyncProperty AddInfo { get; }
		ZPropertyInfoHashtable ZPropertyInfoHash { get; }
	}
}
