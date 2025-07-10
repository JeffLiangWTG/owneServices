// Ignore Spelling: serializer json

using System.Text.Json;
using CargoWise.PAVE.Common.Cache;
using Enterprise.BufferManagement.Service.Shared;

namespace Enterprise.BufferManagement.Service.Cache
{
	public class DistributedCacheFactory : IDistributedCacheFactory
	{
		public IDistributedCache Create() => Create(jsonSerializeOptions: null);

		public IDistributedCache Create(JsonSerializerOptions jsonSerializeOptions) => new DistributedCache(jsonSerializeOptions);

		public IDistributedCache Create(ISerializer serializer) => new DistributedCache(serializer);

		public IDistributedCache Create(IDBConnectionFactory dbConnectionFactory) => new DistributedCache(dbConnectionFactory);

		public IDBConnectionFactory CreateConnectionFactory(bool reuseConnection) => new DBConnectionFactory(reuseConnection);
	}
}
