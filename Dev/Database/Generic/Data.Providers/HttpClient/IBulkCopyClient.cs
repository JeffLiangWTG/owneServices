using CargoWise.Data.SqlProxy.Interface.Models;

namespace CargoWise.Data.HttpClient
{
	public interface IBulkCopyClient
	{
		ExecuteBulkCopyResult BulkCopy(SqlProxyBulkCopyRequest request);
	}
}
