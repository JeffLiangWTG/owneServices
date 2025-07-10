using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class PrimaryKeyProviderTest : TestCase
	{
		public void TestDuplicatePrimaryKeys()
		{
			ZGuid guid = ZGuid.NewZGuid();
			ZQuery query = new ZQuery(DummyBizoSchema.PK, new ZGuid[] { guid, guid, guid });
			var provider = new PrimaryKeyProvider(query);
			AssertEquals(true, provider.ContainsKey(guid));
		}

		public void TestMultipleExclusivePrimaryKeyFilters()
		{
			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();
			ZGuid guid3 = ZGuid.NewZGuid();

			ZQuery query = new ZQuery(DummyBizoSchema.PK, guid1);
			query.AddToFilter(DummyBizoSchema.PK, new ZGuid[] { guid2, guid3 });

			var provider = new PrimaryKeyProvider(query);
			AssertEquals(true, provider.ContainsPrimaryKeys);
			AssertEquals(false, provider.ContainsKey(guid1));
			AssertEquals(false, provider.ContainsKey(guid2));
			AssertEquals(false, provider.ContainsKey(guid3));

			query = new ZQuery(DummyBizoSchema.PK, guid1);
			query.AddToFilter(DummyBizoSchema.PK, new ZGuid[] { guid1, guid2 });
			provider = new PrimaryKeyProvider(query);
			AssertEquals(true, provider.ContainsPrimaryKeys);
			AssertEquals(true, provider.ContainsKey(guid1));
			AssertEquals(false, provider.ContainsKey(guid2));
			AssertEquals(false, provider.ContainsKey(guid3));

			query = new ZQuery(DummyBizoSchema.PK, new ZGuid[] { guid1, guid3 });
			query.AddToFilter(DummyBizoSchema.PK, new ZGuid[] { guid1, guid2 });
			provider = new PrimaryKeyProvider(query);
			AssertEquals(true, provider.ContainsPrimaryKeys);
			AssertEquals(true, provider.ContainsKey(guid1));
			AssertEquals(false, provider.ContainsKey(guid2));
			AssertEquals(false, provider.ContainsKey(guid3));

			query = new ZQuery(DummyBizoSchema.PK, new ZGuid[] { guid1, guid2, guid3 });
			query.AddToFilter(DummyBizoSchema.PK, new ZGuid[] { guid1, guid2 });
			provider = new PrimaryKeyProvider(query);
			AssertEquals(true, provider.ContainsPrimaryKeys);
			AssertEquals(true, provider.ContainsKey(guid1));
			AssertEquals(true, provider.ContainsKey(guid2));
			AssertEquals(false, provider.ContainsKey(guid3));
		}
	}
}
