using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectNKCacheTest : TestCaseWithDummy
	{
		#region Add / Fetch

		public void TestAddAndFetch()
		{
			Cache.Add(Dummy, DummyBizoSchema.Z0_Description, Dummy.Z0_Description);
			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, Dummy.Z0_Description));
			AssertEquals(1, Cache.BusinessObjectCount);
		}

		public void TestAddAndFetchWithMultipleColumnsOnSameBizo()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Code = "KEY1";
			dummy1.Z0_Description = "KEY2";
			dummy2.Z0_Code = "KEY3";
			dummy2.Z0_Description = "KEY3";

			Cache.Add(dummy1, DummyBizoSchema.Z0_Code, dummy1.Z0_Code);
			Cache.Add(dummy1, DummyBizoSchema.Z0_Description, dummy1.Z0_Description);
			Cache.Add(dummy2, DummyBizoSchema.Z0_Code, dummy2.Z0_Code);
			Cache.Add(dummy2, DummyBizoSchema.Z0_Description, dummy2.Z0_Description);

			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, dummy1.Z0_Code));
			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy1.Z0_Description));
			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Code, dummy2.Z0_Code));
			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy2.Z0_Description));
			AssertEquals(4, Cache.BusinessObjectCount);
		}

		[ExpectNoExceptions]
		public void TestDoesNotAddSameBizoTwice()
		{
			Cache.Add(Dummy, DummyBizoSchema.Z0_Description, Dummy.Z0_Description);
			Cache.Add(Dummy, DummyBizoSchema.Z0_Description, Dummy.Z0_Description);
			AssertEquals(1, Cache.BusinessObjectCount);
		}

		[ExpectNoExceptions]
		public void TestDoesNotAddNullObject()
		{
			Cache.Add(null, DummyBizoSchema.Z0_Description, Dummy.Z0_Description);
			Cache.Add(Dummy, null, Dummy.Z0_Description);
			Cache.Add(Dummy, DummyBizoSchema.Z0_Description, null);
			Cache.Add(null, null, null);
			AssertEquals(0, Cache.BusinessObjectCount);
		}

		public void TestDoesNotAddEmptyStringKey()
		{
			Cache.Add(Dummy, DummyBizoSchema.Z0_Description, ZString.Empty);
			AssertEquals(0, Cache.BusinessObjectCount);
		}

		// Not yet supported
		//public void TestAddAndFetchMultipleObjectsAroundSingleRow()
		//{
		//    DummyBaseBusinessObject DummyBase = new DummyBaseBusinessObject(Factory, Dummy.Row);

		//    Cache.Add(Dummy, DummyBizoSchema.Z0_Description, Dummy.Z0_Description);
		//    AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, Dummy.Z0_Description));
		//    AssertNull(Cache.Fetch(typeof(DummyBaseBusinessObject), DummyBizoSchema.Z0_Description, DummyBase.Z0_Description));

		//    Cache.Add(DummyBase, DummyBizoSchema.Z0_Description, DummyBase.Z0_Description);
		//    AssertEquals(2, Cache.BusinessObjectCount);
		//    AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, Dummy.Z0_Description));
		//    AssertNotNull(Cache.Fetch(typeof(DummyBaseBusinessObject), DummyBizoSchema.Z0_Description, DummyBase.Z0_Description));
		//}

		#endregion

		#region Remove

		public void TestRemove()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();

			dummy1.Z0_Description = "KEY2";
			dummy2.Z0_Description = "KEY3";

			Cache.Add(dummy1, DummyBizoSchema.Z0_Description, dummy1.Z0_Description);
			Cache.Add(dummy2, DummyBizoSchema.Z0_Description, dummy2.Z0_Description);

			Cache.Remove(dummy1);
			AssertNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy1.Z0_Description));
			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy2.Z0_Description));

			Cache.Remove(dummy2);
			AssertNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy2.Z0_Description));
		}

		#endregion

		#region Update Cache

		public void TestNotifyNaturalKeyValueChanged()
		{
			DummyBusinessObject dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "MAAAAA";
			dummy2.Z0_Description = "BAAAAA";

			Cache.Add(Dummy, DummyBizoSchema.Z0_Description, Dummy.Z0_Description);
			Cache.Add(dummy1, DummyBizoSchema.Z0_Description, dummy1.Z0_Description);
			Cache.Add(dummy2, DummyBizoSchema.Z0_Description, dummy2.Z0_Description);

			ZString oldKey = dummy1.Z0_Description;
			dummy1.Z0_Description = "CLIFF";
			Cache.UpdateNaturalKeyCache(dummy1, DummyBizoSchema.Z0_Description, oldKey, dummy1.Z0_Description);

			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, Dummy.Z0_Description));
			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy1.Z0_Description));
			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy2.Z0_Description));
			AssertNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, oldKey));
		}

		public void TestUpdateCacheDoesNotReAddIfOldValueNotInCacheButNewValueIs()
		{
			DummyBusinessObject dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy1.Z0_Description = "ABC";
			dummy2.Z0_Description = "DEF";

			Cache.Add(dummy2, DummyBizoSchema.Z0_Description, dummy2.Z0_Description);
			Cache.UpdateNaturalKeyCache(dummy1, DummyBizoSchema.Z0_Description, new ZString("ABC"), new ZString("DEF"));

			AssertNotNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, new ZString("DEF")));
			AssertNull(Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, new ZString("ABC")));
			AssertEquals(1, Cache.BusinessObjectCount);
		}

		public void TestUpdateCacheIgnoresEmptyString()
		{
			DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Description = "ABC";
			Cache.Add(dummy, DummyBizoSchema.Z0_Description, dummy.Z0_Description);
			AssertNotNull(
				"Non-empty value exists",
				Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy.Z0_Description));

			var oldValue = dummy.Z0_Description;
			dummy.Z0_Description = "";
			Cache.UpdateNaturalKeyCache(dummy, DummyBizoSchema.Z0_Description, oldValue, dummy.Z0_Description);
			AssertNull(
				"Does not add empty to cache",
				Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy.Z0_Description));
			AssertNull(
				"Removes old key from cache",
				Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, oldValue));

			oldValue = dummy.Z0_Description;
			dummy.Z0_Description = "DEF";
			Cache.UpdateNaturalKeyCache(dummy, DummyBizoSchema.Z0_Description, oldValue, dummy.Z0_Description);
			AssertNotNull(
				"Adds non-empty to cache",
				Cache.Fetch(typeof(DummyBusinessObject), DummyBizoSchema.Z0_Description, dummy.Z0_Description));
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Dummy.Z0_Description = "ABC";
		}

		BusinessObjectNKCache cache;
		BusinessObjectNKCache Cache
		{
			get { return cache ?? (cache = new BusinessObjectNKCache()); }
		}

		#endregion
	}
}
