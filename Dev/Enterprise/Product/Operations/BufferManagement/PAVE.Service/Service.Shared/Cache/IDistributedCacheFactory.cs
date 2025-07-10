using System.Text.Json;
using CargoWise.PAVE.Common.Cache;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface IDistributedCacheFactory
	{
		IDistributedCache Create();

		IDistributedCache Create(JsonSerializerOptions jsonSerializeOptions);

		IDistributedCache Create(ISerializer serializer);

		IDistributedCache Create(IDBConnectionFactory dbConnectionFactory);

		IDBConnectionFactory CreateConnectionFactory(bool reuseConnection);
	}
}
