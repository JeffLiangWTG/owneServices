using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.GraphEngine.Test;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	[UseSnapshotProtection]
	class ExtensionsTest : TestCase
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			LazyEntityFactory = new Lazy<DummyEntityFactory>(() => new DummyEntityFactory());
			LazyFactory = new Lazy<BusinessObjectFactory>(() => new BusinessObjectFactory());
		}

		protected override void TearDown()
		{
			foreach (var disposable in disposables)
			{
				try
				{
					disposable.Dispose();
				}
				catch (Exception e)
				{
					ErrorReporter.ReportOnce("Exceptiohn during dispose in test", e);
				}
			}
			disposables.Clear();
			base.TearDown();
		}

		readonly List<IDisposable> disposables = new List<IDisposable>();

		DummyEntityFactory EntityFactory => LazyEntityFactory.Value;
		BusinessObjectFactory Factory => LazyFactory.Value;
		Lazy<DummyEntityFactory> LazyEntityFactory { get; set; }
		Lazy<BusinessObjectFactory> LazyFactory { get; set; }

		BusinessObjectFactory GetFactoryWithNewConnection()
		{
			var connection = Db.NewExtraConnectionToMainDb();
			disposables.Add(connection);
			return new BusinessObjectFactory(connection) { NameForDebugging = GetType().FullName };
		}

		#endregion

		public void TestLoadWithApplock()
		{
			var aPPLOCK_KEY1 = "MyApplockKey";
			var aPPLOCK_KEY2 = "OtherKey";

			var bizos = EntityFactory.New().New().New().MakeBizos(Factory);
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));

			using (var resulSet1 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY1, query, 2))
			{
				AssertEquals("Only load as many rows as the batch size.", 2, resulSet1.Values.Count());

				using (var resultSet2 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY1, query, 30))
				{
					AssertEquals("Locked entities are not loaded.", 1, resultSet2.Values.Count());

					using (var resultSet3 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY1, query, int.MaxValue))
					{
						AssertEquals("When all entities are locked, there are no entities.", 0, resultSet3.Values.Count());
					}

					using (var resultSet4 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY2, query, int.MaxValue))
					{
						AssertEquals("The lock keys matters", 3, resultSet4.Values.Count());
					}
				}
			}

			using (var resulSet5 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY1, query, 2))
			{
				AssertEquals("Dispose removes the applocks.", 2, resulSet5.Values.Count());
			}
		}

		public void TestRaceCondition_WholeRowProcessedBeforeApplockTaken()
		{
			var aPPLOCK_KEY1 = "MyApplockKey";

			var bizos = EntityFactory.New().New().New().MakeBizos(Factory);
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, "BUNG");

			var factory1 = GetFactoryWithNewConnection();
			var dummies = factory1.Load<DummyBusinessObject>(query);

			foreach (var bizo in bizos.Skip(1))
			{
				bizo.Z0_VarCharMax = "BUNG";
			}

			Factory.Save();

			using (var resultSet = dummies.ApplyAppLocks(aPPLOCK_KEY1, new[] { DummyBizoSchema.PK }))
			{
				AssertEquals(3, resultSet.ItemsWithLocks.Count);
				AssertEquals(1, resultSet.ReloadRowsToAvoidRaceCondition(factory1, query).ItemsWithLocks.Count);
			}
		}

		public void TestLoadWithApplock_CustomKeyColumns()
		{
			var kEY = "Swag";
			var myGuid = ZGuid.NewZGuid();
			var bizos = EntityFactory.New().New().New().MakeBizos(Factory);
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			var pkColumns = new[] { DummyBizoSchema.Z0_Guid, DummyBizoSchema.PK };

			using (var resultSet1 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(kEY, pkColumns, query, 2))
			{
				using (var resultSet2 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(kEY, pkColumns, query, 30))
				{
					AssertEquals("Should match the batch size.", 2, resultSet1.ItemsWithLocks.Count);
					AssertEquals("Should be one, as Z0_Guid is empty", 1, resultSet2.ItemsWithLocks.Count);
				}
			}

			bizos.ForEach(b => b.Z0_Guid = myGuid);
			Factory.Save();
			using (var resultSet1 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(kEY, pkColumns, query, 2))
			{
				using (var resultSet2 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(kEY, pkColumns, query, 30))
				{
					AssertEquals("Should match the batch size.", 2, resultSet1.ItemsWithLocks.Count);
					AssertEquals("Should be none, because all items are locked", 0, resultSet2.ItemsWithLocks.Count);
				}
			}
		}

		public void TestApplyApplock_IgnoreStubImplementations()
		{
			var kEY = "Swag";
			var myGuid = ZGuid.NewZGuid();
			var pkColumns = new[] { DummyBizoSchema.Z0_Guid, DummyBizoSchema.PK };

			var dummies = EntityFactory.New().New().New();
			var bizos = dummies.MakeBizos<DummyEntityBusinessObject>(Factory);
			Factory.Save();

			using (var resultSet1 = bizos.Cast<IDummyEntity>().Concat(dummies.Cast<IDummyEntity>()).ToArray().ApplyAppLocks(kEY, pkColumns))
			{
				using (var resultSet2 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(kEY, pkColumns, new ZDBOnlyQuery(typeof(DummyBusinessObject)), 2))
				{
					AssertEquals("Should match the batch size.", 6, resultSet1.ItemsWithLocks.Count);
					AssertEquals("Should be none, because all items are locked", 0, resultSet2.ItemsWithLocks.Count);
				}
			}
		}

		public void TestReloadRowsToAvoidRaceCondition_DoNotLoadNewRows()
		{
			var aPPLOCK_KEY1 = "MyApplockKey";

			var bizos = EntityFactory.New().New().New().MakeBizos(Factory);
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, "BUNG");

			var factory1 = GetFactoryWithNewConnection();
			var dummies = factory1.Load<DummyBusinessObject>(query);

			Factory.Save();

			using (var resultSet = dummies.ApplyAppLocks(aPPLOCK_KEY1, new[] { DummyBizoSchema.PK }))
			{
				AssertEquals(3, resultSet.ItemsWithLocks.Count);
				EntityFactory.New().New().New().MakeBizos(Factory);
				Factory.Save();
				AssertEquals(3, resultSet.ReloadRowsToAvoidRaceCondition(factory1, query).ItemsWithLocks.Count);
			}
		}

		public void TestReloadRowsToAvoidRaceCondition_DisposeOfUnusedLocks()
		{
			var aPPLOCK_KEY1 = "MyApplockKey";

			var bizos = EntityFactory.New().New().New().MakeBizos(Factory);
			Factory.Save();
			var query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.NotEqual, "BUNG");

			var factory1 = GetFactoryWithNewConnection();
			var dummies = factory1.Load<DummyBusinessObject>(query);

			foreach (var bizo in bizos.Skip(1))
			{
				bizo.Z0_VarCharMax = "BUNG";
			}

			Factory.Save();

			using (var resultSet = dummies.ApplyAppLocks(aPPLOCK_KEY1, new[] { DummyBizoSchema.PK }))
			{
				AssertEquals(3, resultSet.ItemsWithLocks.Count);
				AssertEquals(1, resultSet.ReloadRowsToAvoidRaceCondition(factory1, query).ItemsWithLocks.Count);
				using (var resultSet2 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY1, new ZDBOnlyQuery(typeof(DummyBusinessObject)), 10))
				{
					AssertEquals("Expect the two yielded rows to be loadable.", 2, resultSet2.ItemsWithLocks.Count);
				}
			}
		}

		public void TestApplocksAndReleasesAreCounted()
		{
			var aPPLOCK_KEY1 = "MyApplockKey";

			var bizos = EntityFactory.New().New().New().MakeBizos(Factory);
			Factory.Save();

			using (var resultSet = bizos.Concat(bizos).ToArray().ApplyAppLocks(aPPLOCK_KEY1, new[] { DummyBizoSchema.PK }))
			{
				AssertEquals("All items are locked", 6, resultSet.ItemsWithLocks.Count);
				var set1 = resultSet.ItemsWithLocks.Take(3);
				var set2 = resultSet.ItemsWithLocks.Skip(3);
				using (var resultSet2 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY1, new ZDBOnlyQuery(typeof(DummyBusinessObject)), 10))
				{
					AssertEquals("None should load", 0, resultSet2.ItemsWithLocks.Count);
				}

				set1.ForEach(s => s.Dispose());
				using (var resultSet3 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY1, new ZDBOnlyQuery(typeof(DummyBusinessObject)), 10))
				{
					AssertEquals("Still none should load because everything will be still locked", 0, resultSet3.ItemsWithLocks.Count);
				}

				set2.First().Dispose();

				using (var resultSet4 = GetFactoryWithNewConnection().LoadWithApplocks<DummyBusinessObject>(aPPLOCK_KEY1, new ZDBOnlyQuery(typeof(DummyBusinessObject)), 10))
				{
					AssertEquals("Now one", 1, resultSet4.ItemsWithLocks.Count);
				}
			}
		}
	}
}