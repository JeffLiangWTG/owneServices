using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	sealed class ZFilterGridModuleSealedTest : ZWebModule_Test
	{
		protected override ZWebModule GetNewZWebModule() => new WebDummyModule(Factory);

		protected override WebModuleID TestID => WebModuleIDs.Dummy;

		ZFilterGridModule FilterGridModule => base.TestZWebModule as ZFilterGridModule;

		ZFilterGridModule Module => base.TestZWebModule as ZFilterGridModule;

		public void TestLoadCollectionWithAdditionalFilter()
		{
			var filterBizO = Module.CreateNewFilterBusinessObject();

			var savingFactory = new BusinessObjectFactory();
			var pKs = new ZGuid[5];
			for (var i = 0; i < 10; i++)
			{
				var bizO = savingFactory.New<DummyBusinessObject>();
				bizO.Z0_Description = "WebDummy";
				if (i < 5)
				{
					pKs[i] = bizO.PK;
				}
			}
			savingFactory.Save();

			AssertEquals("Module should default to load 1000 rows", 1000, FilterGridModule.MaxRows);

			int actualCount = FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("LoadCollection returned row count different", 10, actualCount);
			AssertEquals("LoadCollection should have returned 0 rows", 10, FilterGridModule.GridCollection.Count);

			var keyColumn = Module.GetBusinessObjectPKColumn(filterBizO);
			var pKFilter = new ZQuery(keyColumn, pKs);
			((BusinessObjectCollection)FilterGridModule.GridCollection).RemoveAll();

			actualCount = FilterGridModule.LoadCollection(filterBizO, pKFilter);

			AssertEquals("LoadCollection will return only requested PKs", 5, actualCount);
			AssertEquals("LoadCollection should only load cached rows", 5, FilterGridModule.GridCollection.Count);

			foreach (var expectedPK in pKs)
			{
				Assert("Collection does not contains expected BizO", FilterGridModule.GridCollection.FindByPK(expectedPK) != null);
			}
		}

		public void TestLoadCollectionRowOrder()
		{
			var filterBizO = Module.CreateNewFilterBusinessObject();

			var savingFactory = new BusinessObjectFactory();

			var bizO1 = savingFactory.New<DummyBusinessObject>();
			bizO1.Z0_Code = "Z";
			bizO1.Z0_Description = "WebDummy1";

			var bizO2 = savingFactory.New<DummyBusinessObject>();
			bizO2.Z0_Code = "A";
			bizO2.Z0_Description = "WebDummy2";

			var bizO3 = savingFactory.New<DummyBusinessObject>();
			bizO3.Z0_Code = "A";
			bizO3.Z0_Description = "WebDummy3";

			savingFactory.Save();

			_ = FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("Row Order", "A", ((DummyBusinessObject)FilterGridModule.GridCollection[0]).Z0_Code);
			AssertEquals("Row Order", "A", ((DummyBusinessObject)FilterGridModule.GridCollection[1]).Z0_Code);
			AssertEquals("Row Order", "Z", ((DummyBusinessObject)FilterGridModule.GridCollection[2]).Z0_Code);

			AssertEquals("Row Order", true, ((DummyBusinessObject)FilterGridModule.GridCollection[0]).PK < ((DummyBusinessObject)FilterGridModule.GridCollection[1]).PK);
		}

		public void TestLoadCollectionReturnsRowCount()
		{
			var filterBizO = Module.CreateNewFilterBusinessObject();

			var savingFactory = new BusinessObjectFactory();

			for (int i = 0; i < 10; i++)
			{
				var bizO = savingFactory.New<DummyBusinessObject>();
				bizO.Z0_Description = "WebDummy";
			}
			savingFactory.Save();

			AssertEquals("Module should default to load 1000 rows", 1000, FilterGridModule.MaxRows);

			var actualCount = FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("LoadCollection returned row count different", 10, actualCount);
			AssertEquals("LoadCollection should have returned 0 rows", 10, FilterGridModule.GridCollection.Count);

			FilterGridModule.MaxRows = 5;
			((BusinessObjectCollection)FilterGridModule.GridCollection).RemoveAll();
			actualCount = FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("LoadCollection returned only maximum rows", 5, actualCount);
			AssertEquals("LoadCollection should have loaded 5 rows", 5, FilterGridModule.GridCollection.Count);

			FilterGridModule.MaxRows = 0;
			((BusinessObjectCollection)FilterGridModule.GridCollection).RemoveAll();
			actualCount = FilterGridModule.LoadCollection(filterBizO);
			AssertEquals("LoadCollection returned row count different", 10, actualCount);
			AssertEquals("LoadCollection should have returned 10 rows", 10, FilterGridModule.GridCollection.Count);
		}

		public void TestIsUsedAsLookup()
		{
			AssertNull("Pre=condition: Page is null", Module.Page);
			AssertEquals("IsUsedAsLookup", false, Module.IsUsedAsLookupInternal);
			Module.Page = new ZPage();
			AssertEquals("Is not IRememberFilterCriteriaPage", false, Module.Page is IRememberFilterCriteriaPage);
			AssertEquals("IsUsedAsLookup", true, Module.IsUsedAsLookupInternal);

			Module.Page = new ZPageForTest();
			AssertEquals("IsUsedAsLookup", false, Module.IsUsedAsLookupInternal);
		}

		#region Implementation

		class ZPageForTest : ZPage, IRememberFilterCriteriaPage
		{
		}

		#endregion

	}
}
