using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class BusinessObjectFetchStrategyBaseOnlyTest : TestCaseWithFactory
	{
		public void TestFetchForValidateCoreCalledOnlyOnce()
		{
			BusinessObjectFetchStrategyForTest test = new BusinessObjectFetchStrategyForTest(DummyBusinessObject.New(Factory));
			AssertEquals(0, test.FetchForValidateCoreCount);
			test.FetchForValidate();
			AssertEquals(1, test.FetchForValidateCoreCount);
			test.FetchForValidate();
			AssertEquals(1, test.FetchForValidateCoreCount);
		}

		public void TestFetchForLoadCalled()
		{
			BusinessObjectFetchStrategyForTest test = new BusinessObjectFetchStrategyForTest(DummyBusinessObject.New(Factory));
			AssertEquals(0, test.FetchForLoadCoreCount);
			test.FetchForLoad();
			AssertEquals(1, test.FetchForLoadCoreCount);
		}

		public void TestFetchForDeleteCalled()
		{
			BusinessObjectFetchStrategyForTest test = new BusinessObjectFetchStrategyForTest(DummyBusinessObject.New(Factory));
			AssertEquals(0, test.FetchForDeleteCoreCount);
			test.FetchForDelete();
			AssertEquals(1, test.FetchForDeleteCoreCount);
		}

		public void TestFetchForFactorySaveCalled()
		{
			BusinessObjectFetchStrategyForTest test = new BusinessObjectFetchStrategyForTest(DummyBusinessObject.New(Factory));
			AssertEquals(0, test.FetchForFactorySaveCoreCount);
			test.FetchForFactorySave();
			AssertEquals(1, test.FetchForFactorySaveCoreCount);
		}

		public void TestFetchForViewIgnoresNonZGuidValues()
		{
			DummyDependantBusinessObject dep1 = Factory.New<DummyDependantBusinessObject>();
			dep1.ZD1_Code = "ABC";
			dep1.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn(DummyBizoSchema.Constants.TableName, DummyDependentBizoSchema.ZD1_Code.Name) });
			AssertEquals(0, dep1.Factory.ActiveTableFetchHints);
		}

		public void TestFetchForViewIsNotGeneratedWithNonColumnGuid()
		{
			var bizO = DummyBusinessObject.New(Factory);
			new BusinessObjectFetchStrategy(bizO).FetchForView(new[] { new TableColumn("GlbStaff", "Z0_CalculatedGuid") });
			AssertEquals(0, Factory.ActiveTableFetchHints);
		}

		public void TestFetchForViewPreFetchesCorrectly()
		{
			DummyBusinessObject bizO1 = DummyBusinessObject.New(Factory);
			DummyDependantBusinessObject dep1 = Factory.New<DummyDependantBusinessObject>();
			dep1.ZD1_Z0 = bizO1.PK;

			DummyBusinessObject bizO2 = DummyBusinessObject.New(Factory);
			DummyDependantBusinessObject dep2 = Factory.New<DummyDependantBusinessObject>();
			dep2.ZD1_Z0 = bizO2.PK;

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyDependantBusinessObject dep1Factory2 = factory2.Load<DummyDependantBusinessObject>(dep1.PK);
			factory2.Load(typeof(DummyBusinessObject), bizO1.PK);
			DummyDependantBusinessObject dep2Factory2 = factory2.Load<DummyDependantBusinessObject>(dep2.PK);
			factory2.Load(typeof(DummyBusinessObject), bizO2.PK);
			AssertEquals("Precondition : Default behaviour - one hit per load", 4, factory2.DatabaseLoadCount);

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			DummyDependantBusinessObject dep1Factory3 = factory3.Load<DummyDependantBusinessObject>(dep1.PK);
			dep1Factory3.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn(DummyBizoSchema.Constants.TableName, DummyDependentBizoSchema.ZD1_Z0.Name) });
			DummyDependantBusinessObject dep2Factory3 = factory3.Load<DummyDependantBusinessObject>(dep2.PK);
			dep2Factory3.FetchStrategy.FetchForView(new TableColumn[] { new TableColumn(DummyBizoSchema.Constants.TableName, DummyDependentBizoSchema.ZD1_Z0.Name) });

			factory3.Load(typeof(DummyBusinessObject), bizO1.PK);
			factory3.Load(typeof(DummyBusinessObject), bizO2.PK);
			AssertEquals("Optimised load", 3, factory3.DatabaseLoadCount);
		}

		public void TestFetchForValidateWithAfterClearQueryCache()
		{
			BusinessObjectFetchStrategyForTest fetchStrategy = new BusinessObjectFetchStrategyForTest(Factory.New<DummyBusinessObject>());
			AssertEquals("Precondition", 0, fetchStrategy.FetchForValidateCoreCount);

			fetchStrategy.FetchForValidate();
			AssertEquals("First fetch", 1, fetchStrategy.FetchForValidateCoreCount);

			fetchStrategy.FetchForValidate();
			AssertEquals("No second fetch", 1, fetchStrategy.FetchForValidateCoreCount);

			PersistentFactoryCacheManager.Instance.ClearAllQueryCaches();
			fetchStrategy.FetchForValidate();
			AssertEquals("New fetch after query caches cleared", 2, fetchStrategy.FetchForValidateCoreCount);

			fetchStrategy.FetchForValidate();
			AssertEquals("No second fetch", 2, fetchStrategy.FetchForValidateCoreCount);
		}
	}
}
