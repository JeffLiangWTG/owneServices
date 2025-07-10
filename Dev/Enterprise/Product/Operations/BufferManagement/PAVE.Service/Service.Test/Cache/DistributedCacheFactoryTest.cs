// Ignore Spelling: Serializer Json

using System;
using System.Data;
using System.Text.Json;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.PAVE.Common.Cache;
using Enterprise.BufferManagement.Service.Cache;
using Moq;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test.Cache
{
	public class DistributedCacheFactoryTest : TestCase
	{
		public void TestCreate()
		{
			AssertNotNull(new DistributedCacheFactory().Create());
		}

		[UseSnapshotProtection]
		[TestDate(2023, 10, 20, 1, 2, 5)]
		public void TestCreateWithJsonOptions()
		{
			var jsonSerializerOptions = new JsonSerializerOptions
			{
				WriteIndented = true,
			};

			AssertEquals("Precondition to check that WriteIndented = true is not the default", false, new JsonSerializerOptions().WriteIndented);

			var distributedCache = new DistributedCacheFactory().Create(jsonSerializerOptions);
			distributedCache.Set("key", new { P1 = "A", P2 = "B" }, new DistributedCacheOptions(TimeSpan.FromSeconds(1)), "E");
			var cacheItem = Db.Connection.ExecuteScalar("SELECT TPC_Value FROM dbo.TemporalCache WHERE TPC_Identifier = 'key'") as byte[];
			var cacheItemString = System.Text.Encoding.UTF8.GetString(cacheItem);

			AssertEquals("{\r\n  \"P1\": \"A\",\r\n  \"P2\": \"B\"\r\n}", cacheItemString);
		}

		[UseSnapshotProtection]
		[TestDate(2023, 10, 20, 1, 2, 5)]
		public void TestCreateWithCustomSerializer()
		{
			var mockSerializer = new Mock<ISerializer>();
			var serilizeResult = new byte[1] { 2 };
			var deserializeResult = "I knew how to convert bytes! 🤣";

			mockSerializer.Setup(m => m.Serialize(It.IsAny<string>())).Returns(serilizeResult);
			mockSerializer.Setup(m => m.Deserialize<string>(It.IsAny<byte[]>())).Returns(deserializeResult);

			var distributedCache = new DistributedCacheFactory().Create(mockSerializer.Object);

			distributedCache.Set("key", "Cache or Crash!", new DistributedCacheOptions(TimeSpan.FromSeconds(1)), "E");

			var cacheItemFromDB = Db.Connection.ExecuteScalar("SELECT TPC_Value FROM dbo.TemporalCache WHERE TPC_Identifier = 'key'") as byte[];
			var cacheItem = distributedCache.Get<string>("key");

			AssertEquals(serilizeResult, cacheItemFromDB);
			AssertEquals(deserializeResult, cacheItem);
		}

		[UseSnapshotProtection]
		[TestDate(2023, 10, 20, 1, 2, 5)]
		public void TestCreateWithCustomConnectionFactory()
		{
			var factory = new DistributedCacheFactory();

			IDistributedCache distributedCache = null;
			IDbConnection connection = null;

			using (var connectionFactory = factory.CreateConnectionFactory(true))
			{
				distributedCache = new DistributedCacheFactory().Create(connectionFactory);

				connection = connectionFactory.CreateOpenedConnection();
				distributedCache.Set("key", "BLA", new DistributedCacheOptions(TimeSpan.FromSeconds(1)), "E");
				AssertEquals(connection, connectionFactory.CreateOpenedConnection());

				var item = distributedCache.Get<string>("key");
				AssertEquals(connection, connectionFactory.CreateOpenedConnection());
				AssertEquals("BLA", item);
			}

			AssertEquals(ConnectionState.Closed, connection.State);

			AssertEquals("BLA", distributedCache.Get<string>("key"));
		}
	}
}
