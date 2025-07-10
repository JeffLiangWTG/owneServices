using System;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryCacheKeyTest : NUnit.Framework.TestCase
	{
		public void TestOwnerPK()
		{
			Guid companyPK = Guid.Empty;
			Guid branchPK = Guid.Empty;
			Guid departmentPK = Guid.Empty;
			RegistryCacheKey cacheKey = new RegistryCacheKey("Name", companyPK, branchPK, departmentPK);
			AssertEquals(Guid.Empty, cacheKey.OwnerPK);

			companyPK = Guid.NewGuid();
			cacheKey = new RegistryCacheKey("Name", companyPK, branchPK, departmentPK);
			AssertEquals(companyPK, cacheKey.OwnerPK);

			companyPK = Guid.Empty;
			branchPK = Guid.NewGuid();
			cacheKey = new RegistryCacheKey("Name", companyPK, branchPK, departmentPK);
			AssertEquals(branchPK, cacheKey.OwnerPK);

			companyPK = Guid.NewGuid();
			cacheKey = new RegistryCacheKey("Name", companyPK, branchPK, departmentPK);

			try
			{
				Guid dummy = cacheKey.OwnerPK;
				Fail("Should have thrown ArgumentException");
			}
			catch (ArgumentException)
			{
			}
		}

		public void TestKeyReturnsDifferentKey()
		{
			Guid samplePK = Guid.NewGuid();

			Guid companyPK = Guid.Empty;
			Guid branchPK = Guid.Empty;
			Guid departmentPK = Guid.Empty;
			RegistryCacheKey cacheKey = new RegistryCacheKey("Name", companyPK, branchPK, departmentPK);

			string keyEmpty = cacheKey.Key;

			companyPK = samplePK;
			cacheKey = new RegistryCacheKey("Name", companyPK, branchPK, departmentPK);
			string keyCompany = cacheKey.Key;

			companyPK = Guid.Empty;
			branchPK = samplePK;
			cacheKey = new RegistryCacheKey("Name", companyPK, branchPK, departmentPK);
			string keyBranch = cacheKey.Key;

			branchPK = Guid.Empty;
			departmentPK = samplePK;
			cacheKey = new RegistryCacheKey("Name", companyPK, branchPK, departmentPK);
			string keyDepartment = cacheKey.Key;

			Assert(keyEmpty != keyCompany);
			Assert(keyEmpty != keyBranch);
			Assert(keyEmpty != keyDepartment);

			Assert(keyCompany != keyDepartment);
			Assert(keyBranch != keyDepartment);

			Assert(keyBranch != keyCompany);
		}

		public void TestGuidsToStringKey()
		{
			Guid guid1 = new Guid("AAAA12F1-91E8-48f9-B1DE-68F462A26319");
			Guid guid2 = new Guid("EB3263AE-7E34-414e-998E-E3D74B39C32A");
			Guid guid3 = new Guid("DF746D34-CA6A-4925-9874-E44537597A69");

			RegistryCacheKey cacheKey = new RegistryCacheKey("", guid1, guid2, guid3);

			AssertKey(cacheKey.GuidsToStringKey(guid1),
				4849, 43690, 37352, 18681, 57009, 62568, 41570, 6499);

			AssertKey(cacheKey.GuidsToStringKey(guid1, guid2),
				4849, 43690, 37352, 18681, 57009, 62568, 41570, 6499,
				25518, 60210, 32308, 16718, 36505, 55267, 14667, 10947);

			AssertKey(cacheKey.GuidsToStringKey(guid1, guid2, guid3),
				4849, 43690, 37352, 18681, 57009, 62568, 41570, 6499,
				25518, 60210, 32308, 16718, 36505, 55267, 14667, 10947,
				27956, 57204, 51818, 18725, 29848, 17892, 22839, 27002);

			AssertKey(cacheKey.GuidsToStringKey(guid2, Guid.Empty),
				25518, 60210, 32308, 16718, 36505, 55267, 14667, 10947,
				0, 0, 0, 0, 0, 0, 0, 0);
		}

		void AssertKey(string key, params int[] expectedChars)
		{
			AssertEquals("key.Length", expectedChars.Length, key.Length);
			for (int i = 0; i < expectedChars.Length; i++)
			{
				AssertEquals("key[" + i + "]", (char)expectedChars[i], key[i]);
			}
		}

		public void TestUseFallBackDefault()
		{
			RegistryCacheKey cacheKey1 = new RegistryCacheKey("", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
			RegistryCacheKey cacheKey2 = new RegistryCacheKey("", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), true);
			Assert(!cacheKey1.UseFallback);
			Assert(cacheKey2.UseFallback);
		}

		public void TestUseFallBackParticipatesInKey()
		{
			Guid guid1 = Guid.NewGuid();
			Guid guid2 = Guid.NewGuid();
			Guid guid3 = Guid.NewGuid();
			RegistryCacheKey key1 = new RegistryCacheKey("magic", guid1, guid2, guid3);
			RegistryCacheKey key2 = new RegistryCacheKey("spell", guid1, guid2, guid3, true);

			string key = key1.GuidsToStringKey(new Guid[] { guid1, guid2, guid3 });
			AssertEquals("magicFalse", key1.Key.Replace(key, ""));
			AssertEquals("spellTrue", key2.Key.Replace(key, ""));
		}
	}
}
