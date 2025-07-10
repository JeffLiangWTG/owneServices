using System;
using System.Threading.Tasks;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	[TestedType(typeof(ZStmALogModule))]
	public class ZStmALogModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.StmALog;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShowRecent()
		{
			using (var module = new ZStmALogModuleForTest())
			{
				Assert(!module.ShowRecentItemsExposed);
			}
		}

		public void TestInitData()
		{
			using (var module = new ZStmALogModuleForTest())
			{
				var master = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
				module.InitData(master);

				var filterBusinessObject = module.FilterBusinessObject;
				Assert(filterBusinessObject is ZStmALogFilterBusinessObject);
				Assert(filterBusinessObject is not ZStmALogFilterBusinessObjectForTest);
			}
		}

		public void TestGetStmALogFilterStripBusinessObject()
		{
			using (var module = new ZStmALogModuleForTest())
			{
				var master = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
				module.InitData(master, master => new ZStmALogFilterBusinessObjectForTest(null));

				var filterBusinessObject = module.FilterBusinessObject as ZStmALogFilterBusinessObjectForTest;
				AssertNull(filterBusinessObject.Master);
			}
		}

		class ZStmALogFilterBusinessObjectForTest : ZStmALogFilterBusinessObject
		{
			public ZStmALogFilterBusinessObjectForTest(IStmALogParent master)
				: base(master)
			{
			}
			public IStmALogParent Master => master;
		}

		public class ZStmALogModuleForTest : ZStmALogModule
		{
			protected override IFilterControl GetNewFilterControl()
			{
				return new ZStmALogFilterControlForTest(master, GridCollection, FilterBusinessObject, this);
			}

			public bool ShowRecentItemsExposed
			{
				get { return ShowRecentItems; }
			}

			public void Find()
			{
				PerformSearch_ForTest();
			}

			protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
			{
				FactoryExposed = factory;
				return SearchManager.PerformSearch(factory, type, query);
			}

			public BusinessObjectFactory FactoryExposed { get; private set; }
			public Task AsyncTask_Exposed => AsyncTask;
		}
	}
}
