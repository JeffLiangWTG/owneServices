using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFetcherTest : TestCaseWithFactory
	{
		public void TestPreCachedFetchHints()
		{
			BusinessObjectFetcher fetcher = new BusinessObjectFetcher();
			AssertEquals(0, fetcher.PreCachedFetchHints);
			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, ZGuid.NewZGuid()));
			AssertEquals(1, fetcher.PreCachedFetchHints);
		}

		public void TestAddFetchHint()
		{
			BusinessObjectFetcher fetcher = new BusinessObjectFetcher();
			AssertEquals(0, fetcher.PreCachedFetchHints);

			ZGuid guid = ZGuid.NewZGuid();
			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, guid));
			AssertEquals(1, fetcher.PreCachedFetchHints);
			fetcher.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, guid));
			AssertEquals("Duplicate fetch hint should be ignored", 1, fetcher.PreCachedFetchHints);
		}

		public void TestFetchDoesNotHitDB()
		{
			BusinessObjectFactory factoryCreate = new BusinessObjectFactory();
			BusinessObject bizO = DummyBusinessObject.New(factoryCreate);
			factoryCreate.Save();
			BusinessObjectFetcher fetcher = new BusinessObjectFetcher();
			IImmediateHint hint = new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, bizO.PK);
			fetcher.AddFetchHint(hint);
			hint.IsDataHintLoaded = true;
			fetcher.Fetch(Factory);
			AssertEquals(0, Factory.DatabaseLoadCount);
		}

		public void TestFetchLoadsBusinessObject()
		{
			BusinessObjectFactory factoryCreate = new BusinessObjectFactory();
			BusinessObject bizO = DummyBusinessObject.New(factoryCreate);
			factoryCreate.Save();
			BusinessObjectFetcher fetcher = new BusinessObjectFetcher();
			ImmediateFetchHint hint = new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, bizO.PK);
			fetcher.AddFetchHint(hint);
			hint.IsDataHintLoaded = true;
			Factory.RowFactory.LoadFromPK(DummyBizoSchema.Constants.TableName, bizO.PK);
			fetcher.Fetch(Factory);

			ZQuery filter = new ZQuery(DummyBizoSchema.PK, bizO.PK);
			filter.FetchOnlyFromLocalCache = true;
			AssertNotNull(Factory.LoadTop1(typeof(DummyBusinessObject), filter));
		}

		#region Testing fetch hints that hint to fetch again
		public void TestSelfFetchingHint()
		{
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			BusinessObjectFactory factoryCreate = new BusinessObjectFactory();
			factoryCreate.RefreshEnabled = false;

			for (int i = 0; i < 64; i++)
			{
				var bizO1 = factoryCreate.New<DummyBusinessObject>();
				var bizO2 = factoryCreate.New<DummyBusinessObject>();
				bizO1.Z0_Guid = bizO2.PK;
				bizO2.Z0_Guid = bizO1.PK;
				factory2.AddFetchHint(typeof(DummyBusinessObjectWithFetch), new ZQuery(DummyBizoSchema.PK, bizO1.Z0_Guid));
			}
			factoryCreate.Save();

			// This will force the load of all the existing fetch hints
			factory2.LoadTop1<DummyBusinessObjectWithFetch>(new ZQuery());
			AssertEquals("DB Hits", 1, factory2.DatabaseLoadCount);
			// but will generate new fetch hints
			AssertEquals(1, factory2.ActiveTableFetchHints);
			AssertEquals(64, factory2.ActiveFetchHintsForTable("DummyBizO"));

			// forcing a load of these hints
			factory2.Load<DummyBusinessObjectWithFetch>(new ZQuery(DummyBizoSchema.Z0_AnotherDecimal, 6m));
			// will make another db hit or 2
			// Note - if you get a failing test from optimisations that make this 2, well done. Change the number to 2.
			AssertEquals("DB Hits", 3, factory2.DatabaseLoadCount);
			// but will clear all fetch hints
			AssertEquals(0, factory2.ActiveTableFetchHints);
			AssertEquals(0, factory2.ActiveFetchHintsForTable("DummyBizO"));
		}

		class DummyBusinessObjectWithFetch : DummyBusinessObject
		{
			public DummyBusinessObjectWithFetch(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override IBusinessObjectFetchStrategy GetFetchStrategy()
			{
				return new DummyFetch(this);
			}

			class DummyFetch : BusinessObjectFetchStrategy
			{
				public DummyFetch(DummyBusinessObjectWithFetch parent)
					: base(parent)
				{
					Parent = parent;
				}

				readonly DummyBusinessObjectWithFetch Parent;

				protected override void FetchForLoadCore()
				{
					base.FetchForLoadCore();
					Factory.AddFetchHint(typeof(DummyBusinessObjectWithFetch), new ZQuery(DummyBizoSchema.PK, Parent.Z0_Guid));
				}
			}
		}

		#endregion
	}
}
