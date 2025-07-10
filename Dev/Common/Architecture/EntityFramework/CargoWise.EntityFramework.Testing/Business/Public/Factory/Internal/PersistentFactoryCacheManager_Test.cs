using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class PersistentFactoryCacheManager_Test : NUnit.Framework.TestCase
	{
		public void TestGetRowFactories()
		{
			PersistentFactoryCacheManager manager = new PersistentFactoryCacheManager();
			AssertEquals(0, manager.GetRowFactories().Length);
			BusinessObjectFactory factory = new BusinessObjectFactory();
			manager.Add(factory);
			AssertEquals(1, manager.GetRowFactories().Length);
			AssertEquals(factory.RowFactory, manager.GetRowFactories()[0]);
		}

		public void TestTrackAllCreatedFactories_ForTest()
		{
			CreateFactory("Not cached");

			GC.Collect();
			GC.WaitForFullGCComplete();

			AssertFactoryRetained("Not cached", false);

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				CreateFactory("Is cached");

				GC.Collect();
				GC.WaitForFullGCComplete();

				AssertFactoryRetained("Is cached", true);
			}

			GC.Collect();
			GC.WaitForFullGCComplete();

			AssertFactoryRetained("Is cached", false);
		}

		public void TestUberFactoryIsNeverTracked()
		{
			var uber = RowFactory.UberFactoryRememberToLock;
			AssertEquals(false, PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Contains(uber));
		}

		public void TestTrackAllCreatedFactories_ForTestWithTableTracking()
		{
			BusinessObjectFactory factory1;
			BusinessObjectFactory factory2;
			BusinessObjectFactory factory3;
			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest(new[] { CusCodeDataSchema.Constants.TableName }))
			{
				factory1 = new BusinessObjectFactory { NameForDebugging = "factory1" };
				factory1.Load(CusCodeDataSchema.Constants.Prefix, ZGuid.NewZGuid());
				factory1.Load(CusAddInfoSchema.Constants.Prefix, ZGuid.NewZGuid());
				factory2 = new BusinessObjectFactory { NameForDebugging = "factory2" };
				factory2.Load(CusAddInfoSchema.Constants.Prefix, ZGuid.NewZGuid());
				factory3 = new BusinessObjectFactory { NameForDebugging = "factory3" };
				factory3.Load(CusCodeDataSchema.Constants.Prefix, ZGuid.NewZGuid());
			}
			AssertQueries(factory1.TableSelects, true, true, true);
			AssertQueries(factory2.TableSelects, false, false, true);
			AssertQueries(factory3.TableSelects, true, true, false);
			factory1.ResetDatabaseLoadCount();
			factory1.Load(CusCodeDataSchema.Constants.Prefix, ZGuid.NewZGuid());
			AssertQueries(factory1.TableSelects, true, false, false);

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				factory1 = new BusinessObjectFactory { NameForDebugging = "factory1" };
				factory1.Load(CusCodeDataSchema.Constants.Prefix, ZGuid.NewZGuid());
				factory1.Load(CusAddInfoSchema.Constants.Prefix, ZGuid.NewZGuid());
				factory2 = new BusinessObjectFactory { NameForDebugging = "factory2" };
				factory2.Load(CusAddInfoSchema.Constants.Prefix, ZGuid.NewZGuid());
				factory3 = new BusinessObjectFactory { NameForDebugging = "factory3" };
				factory3.Load(CusCodeDataSchema.Constants.Prefix, ZGuid.NewZGuid());
			}
			AssertQueries(factory1.TableSelects, true, false, true);
			AssertQueries(factory2.TableSelects, false, false, true);
			AssertQueries(factory3.TableSelects, true, false, false);
		}

		void AssertQueries(TableHitCount[] tableSelects, bool shouldHaveCusCodeDataHit, bool hasCusCodeDataQueries, bool shouldHavecusAddInfoHit)
		{
			var cusCodeDataTableHitCount = tableSelects.FirstOrDefault(x => x.TableName == CusCodeDataSchema.Constants.TableName);
			AssertEquals("CusCodeData Hit", shouldHaveCusCodeDataHit ? 1 : 0, cusCodeDataTableHitCount.Value);
			AssertEquals("CusCodeData Queries", hasCusCodeDataQueries, cusCodeDataTableHitCount.Queries?.Any() ?? false);
			var cusAddInfoTableHitCount = tableSelects.FirstOrDefault(x => x.TableName == CusAddInfoSchema.Constants.TableName);
			AssertEquals("CusAddInfo Hit", shouldHavecusAddInfoHit ? 1 : 0, cusAddInfoTableHitCount.Value);
			AssertNull("CusAddInfo Queries", cusAddInfoTableHitCount.Queries);
		}

		static void CreateFactory(string name)
		{
			new BusinessObjectFactory { NameForDebugging = name };
		}

		static void AssertFactoryRetained(string name, bool retained)
		{
			var factory = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().SingleOrDefault(f => f.NameForDebugging == name);

			if (retained)
			{
				AssertNotNull(factory);
			}
			else
			{
				AssertNull(factory);
			}
		}
	}
}
