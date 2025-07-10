using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryAuditColumnCacheKeyTest : TestCase
	{
		public void TestKey()
		{
			Guid guid1 = new Guid("03052ED3-2C64-49AC-97D8-C6079D5015B5");
			Guid guid2 = new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
			Guid guid3 = new Guid("D58CB7EC-EFCA-434B-A307-594FF5B00608");

			var key1 = new RegistryAuditColumnCacheKey("NAME", "COLUMN1", guid1, guid2, guid3).Key;
			var key2 = new RegistryAuditColumnCacheKey("NAME", "COLUMN2", guid1, guid2, guid3).Key;
			AssertContains("COLUMN1", key1);
			AssertContains("COLUMN2", key2);
			Assert(key1 != key2);
		}
	}
}
