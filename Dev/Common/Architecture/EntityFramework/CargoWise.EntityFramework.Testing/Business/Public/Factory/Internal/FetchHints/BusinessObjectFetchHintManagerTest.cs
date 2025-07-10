using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFetchHintManagerTest : TestCaseWithFactory
	{
		public void TestAddFetchHintAndActiveFetchHintsForTable()
		{
			BusinessObjectFetchHintManager manager = new BusinessObjectFetchHintManager(Factory);
			AssertEquals(0, manager.ActiveTableFetchHints);
			manager.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, ZGuid.NewZGuid()));
			AssertEquals(1, manager.ActiveTableFetchHints);
			manager.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, ZGuid.NewZGuid()));
			AssertEquals(1, manager.ActiveTableFetchHints);
			manager.AddFetchHint(new ImmediateFetchHint(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.PK, ZGuid.NewZGuid()));
			AssertEquals(2, manager.ActiveTableFetchHints);
		}

		public void TestHasFetchHints()
		{
			AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			ImmediateFetchHint hint = new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, ZGuid.NewZGuid());
			Factory.AddFetchHint(hint);
			hint.IsDataHintLoaded = true;
			AssertEquals(1, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			Factory.Load<DummyBusinessObject>(new ZQuery());
			AssertEquals(0, Factory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestFetchTable()
		{
			BusinessObjectFactory factoryCreate = new BusinessObjectFactory();
			BusinessObject bizO = DummyBusinessObject.New(factoryCreate);
			factoryCreate.Save();

			BusinessObjectFetchHintManager manager = new BusinessObjectFetchHintManager(Factory);
			AssertEquals(false, manager.HasFetchHints(DummyBizoSchema.Constants.TableName));
			ImmediateFetchHint hint = new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, bizO.PK);
			manager.AddFetchHint(hint);
			hint.IsDataHintLoaded = true;
			Factory.RowFactory.LoadFromPK(DummyBizoSchema.Constants.TableName, bizO.PK);
			manager.FetchTable(DummyBizoSchema.Constants.TableName);
			ZQuery filter = new ZQuery(DummyBizoSchema.PK, bizO.PK);
			filter.FetchOnlyFromLocalCache = true;
			AssertNotNull(Factory.LoadTop1(typeof(DummyBusinessObject), filter));
		}
	}
}
