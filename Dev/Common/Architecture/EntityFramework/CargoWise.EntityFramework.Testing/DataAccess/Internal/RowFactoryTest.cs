using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class RowFactoryTest : TestCaseWithFactory
	{
		public void TestDynamicNonPersistentParameterSwap()
		{
			RowFactory rowFactory = new RowFactory();
			var parameters = new ZSqlParameter[]
				{
						ZSqlParameter.New("@param1", "Hello", DummyBizoSchema.Z0_Code),
						ZSqlParameter.New("@param2", true, DummyBizoSchema.Z0_BitFalse),
						ZSqlParameter.New("@param3", true, DummyBizoSchema.Z0_BitFiltered),
				};

			rowFactory.LoadDynamicNonPersistent("select count(*) cnt from dbo.DummyBizo where Z0_Code = @param1 and Z0_BitFalse = @param2 and Z0_BitFiltered = @param3", new ZSqlParameterCollection(parameters));
			AssertContains("Last SQL Query (parameter)", "select count(*) cnt from dbo.DummyBizo where Z0_Code = @param1 and Z0_BitFalse = @param2 and Z0_BitFiltered = 1", SqlEventTracker.Instance.LastSqlQuery);
		}

		public void TestFetchRelatedTable()
		{
			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Code = "BOB";
			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Code = "WENDY";
			bizO2.Z0_Guid = bizO1.PK;
			var bizO3 = Factory.New<DummyBusinessObject>();
			bizO3.Z0_Code = "BOB";
			var bizO4 = Factory.New<DummyBusinessObject>();
			bizO4.Z0_Code = "JACK";
			bizO4.Z0_Guid = bizO3.PK;
			var bizO5 = Factory.New<DummyBusinessObject>();
			bizO5.Z0_Code = "BOB";
			var bizO6 = Factory.New<DummyBusinessObject>();
			bizO6.Z0_Code = "WENDY";
			bizO6.Z0_Guid = bizO5.PK;
			var bizO7 = Factory.New<DummyBusinessObject>();
			bizO7.Z0_Code = "JANE";
			var bizO8 = Factory.New<DummyBusinessObject>();
			bizO8.Z0_Code = "BOB";
			Factory.Save();

			DummyBusinessObject[] bizObjs;
			var newFactory = new BusinessObjectFactory();
			IExternalFetchHintSupporter supporter = newFactory;
			DataTable dummyBizoTable;
			using (supporter.SetupCreator())
			{
				supporter.AddTableFetchHintCreator(DummyBizoSchema.Instance, (row) =>
				{
					var query = new ZQuery(DummyBizoSchema.Z0_Guid, Extensions.DataIndexerExtensions.GetValue(row, DummyBizoSchema.PK));
					query.AddToFilter(DummyBizoSchema.Z0_Code, "WENDY");
					return new IFetchHint[] { new ZQueryFetchHint(DummyBizoSchema.Instance, query) };
				});
				bizObjs = newFactory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "BOB"));
				AssertEquals(4, bizObjs.Length);
				dummyBizoTable = newFactory.RowFactory.data.Tables[DummyBizoSchema.Constants.TableName];
				AssertEquals("Should contain only rows loaded", 4, dummyBizoTable.Rows.Count);
				AssertNotNull("bizO1 row", dummyBizoTable.Rows.Find(bizO1.PK.ToGuid()));
				AssertNotNull("bizO3 row", dummyBizoTable.Rows.Find(bizO3.PK.ToGuid()));
				AssertNotNull("bizO4 row", dummyBizoTable.Rows.Find(bizO5.PK.ToGuid()));
				AssertNotNull("bizO8 row", dummyBizoTable.Rows.Find(bizO8.PK.ToGuid()));
				AssertEquals("Should have 4 fetch hints", 4, newFactory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				AssertEquals(1, newFactory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value);
				newFactory.Load<DummyBusinessObject>(bizO7.PK);
				AssertEquals("Should contain only rows loaded", 7, dummyBizoTable.Rows.Count);
				AssertNotNull("bizO1 row", dummyBizoTable.Rows.Find(bizO1.PK.ToGuid()));
				AssertNotNull("bizO2 row", dummyBizoTable.Rows.Find(bizO2.PK.ToGuid()));
				AssertNotNull("bizO3 row", dummyBizoTable.Rows.Find(bizO3.PK.ToGuid()));
				AssertNotNull("bizO5 row", dummyBizoTable.Rows.Find(bizO5.PK.ToGuid()));
				AssertNotNull("bizO6 row", dummyBizoTable.Rows.Find(bizO6.PK.ToGuid()));
				AssertNotNull("bizO7 row", dummyBizoTable.Rows.Find(bizO7.PK.ToGuid()));
				AssertNotNull("bizO8 row", dummyBizoTable.Rows.Find(bizO8.PK.ToGuid()));
				AssertEquals("Should have 1 fetch hint from the recent loaded rows", 1, newFactory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				AssertEquals("2 fetch hints and 1 load", 4, newFactory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value);
			}
			using (supporter.SetupCreator())
			{
				newFactory.Load<DummyBusinessObject>(bizO4.PK);
				dummyBizoTable = newFactory.RowFactory.data.Tables[DummyBizoSchema.Constants.TableName];
				AssertEquals("Should have not fetch hints", 0, newFactory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				AssertEquals("1 fetch hint and 1 load", 6, newFactory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value);
				AssertEquals("Should contain only rows loaded", 8, dummyBizoTable.Rows.Count);
				AssertNotNull("bizO1 row", dummyBizoTable.Rows.Find(bizO1.PK.ToGuid()));
				AssertNotNull("bizO2 row", dummyBizoTable.Rows.Find(bizO2.PK.ToGuid()));
				AssertNotNull("bizO3 row", dummyBizoTable.Rows.Find(bizO3.PK.ToGuid()));
				AssertNotNull("bizO4 row", dummyBizoTable.Rows.Find(bizO4.PK.ToGuid()));
				AssertNotNull("bizO5 row", dummyBizoTable.Rows.Find(bizO5.PK.ToGuid()));
				AssertNotNull("bizO6 row", dummyBizoTable.Rows.Find(bizO6.PK.ToGuid()));
				AssertNotNull("bizO7 row", dummyBizoTable.Rows.Find(bizO7.PK.ToGuid()));
				AssertNotNull("bizO8 row", dummyBizoTable.Rows.Find(bizO8.PK.ToGuid()));
			}
			newFactory = new BusinessObjectFactory();
			bizObjs = newFactory.Load<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, "BOB"));
			AssertEquals(4, bizObjs.Length);
			dummyBizoTable = newFactory.RowFactory.data.Tables[DummyBizoSchema.Constants.TableName];
			AssertEquals("Should contain only rows loaded", 4, dummyBizoTable.Rows.Count);
			AssertNotNull("bizO1 row", dummyBizoTable.Rows.Find(bizO1.PK.ToGuid()));
			AssertNotNull("bizO3 row", dummyBizoTable.Rows.Find(bizO3.PK.ToGuid()));
			AssertNotNull("bizO4 row", dummyBizoTable.Rows.Find(bizO5.PK.ToGuid()));
			AssertNotNull("bizO8 row", dummyBizoTable.Rows.Find(bizO8.PK.ToGuid()));
			AssertEquals("Should not have any fetch hints", 0, newFactory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			AssertEquals(1, newFactory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value);
			newFactory.Load<DummyBusinessObject>(bizO7.PK);
			AssertEquals("Should contain only rows loaded", 5, dummyBizoTable.Rows.Count);
			AssertNotNull("bizO1 row", dummyBizoTable.Rows.Find(bizO1.PK.ToGuid()));
			AssertNotNull("bizO3 row", dummyBizoTable.Rows.Find(bizO3.PK.ToGuid()));
			AssertNotNull("bizO5 row", dummyBizoTable.Rows.Find(bizO5.PK.ToGuid()));
			AssertNotNull("bizO7 row", dummyBizoTable.Rows.Find(bizO7.PK.ToGuid()));
			AssertNotNull("bizO8 row", dummyBizoTable.Rows.Find(bizO8.PK.ToGuid()));
			AssertEquals("Should not have any fetch hints", 0, newFactory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
			AssertEquals(2, newFactory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value);

			newFactory = new BusinessObjectFactory();
			supporter = newFactory;
			using (supporter.SetupCreator())
			{
				var bizO5InNewFactory = newFactory.Load<DummyBusinessObject>(bizO5.PK);
				dummyBizoTable = newFactory.RowFactory.data.Tables[DummyBizoSchema.Constants.TableName];
				AssertEquals("Should contain only rows loaded", 1, dummyBizoTable.Rows.Count);
				AssertNotNull("bizO5 row", dummyBizoTable.Rows.Find(bizO5.PK.ToGuid()));

				supporter.AddTableFetchHintCreator(DummyBizoSchema.Instance, (row) =>
				{
					var query = new ZQuery(DummyBizoSchema.Z0_Guid, Extensions.DataIndexerExtensions.GetValue(row, DummyBizoSchema.PK));
					query.AddToFilter(DummyBizoSchema.Z0_Code, "WENDY");
					return new IFetchHint[] { new ZQueryFetchHint(DummyBizoSchema.Instance, query) };
				});
				AssertEquals("No active fetch hints", 0, newFactory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				supporter.AddRelatedTableHints(DummyBizoSchema.Constants.TableName, new IColumnIndexer[] { (IColumnIndexer)((INeedRow)bizO5InNewFactory).Row });
				AssertEquals("Should have 1 fetch hint", 1, newFactory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				AssertEquals(1, newFactory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value);
				newFactory.Load<DummyBusinessObject>(bizO7.PK);
				AssertEquals("Should contain only rows loaded", 3, dummyBizoTable.Rows.Count);
				AssertNotNull("bizO5 row", dummyBizoTable.Rows.Find(bizO5.PK.ToGuid()));
				AssertNotNull("bizO6 row", dummyBizoTable.Rows.Find(bizO6.PK.ToGuid()));
				AssertNotNull("bizO7 row", dummyBizoTable.Rows.Find(bizO7.PK.ToGuid()));
				AssertEquals("Should have 1 fetch hint from the recent loaded rows", 1, newFactory.RowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
				AssertEquals("2 fetch hints and 1 load", 4, newFactory.RowFactory.GetTableHitCount(DummyBizoSchema.Constants.TableName).Value);
			}
		}

		public void TestAddTableFetchHintCreator()
		{
			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Code = "bizO1";

			var child01Biz01 = Factory.New<DummyDependantBusinessObject>();
			child01Biz01.ZD1_Z0 = bizO1.PK;
			var child02Biz01 = Factory.New<DummyDependantBusinessObject>();
			child02Biz01.ZD1_Z0 = bizO1.PK;
			var child03Biz01 = Factory.New<DummyDependantBusinessObject>();
			child03Biz01.ZD1_Z0 = bizO1.PK;

			var bizO2 = Factory.New<DummyBusinessObject>();
			bizO2.Z0_Code = "bizO2";

			var child01Biz02 = Factory.New<DummyDependantBusinessObject>();
			child01Biz02.ZD1_Z0 = bizO2.PK;

			var unlinkedChild = Factory.New<DummyDependantBusinessObject>();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			IExternalFetchHintSupporter supporter = newFactory;
			using (supporter.SetupCreator())
			{
				supporter.AddTableFetchHintCreator(DummyBizoSchema.Instance, (row) =>
				{
					var bizoPK = Extensions.DataIndexerExtensions.GetValue(row, DummyBizoSchema.PK);
					return new IFetchHint[] { new FetchHint(DummyDependentBizoSchema.ZD1_Z0, bizoPK) };
				});

				AssertEquals("No fetch hint for DummyDependentBizo until needed", 0, newFactory.RowFactory.ActiveFetchHintsForTable(DummyDependentBizoSchema.Constants.TableName));

				var bizO1InNewFactory = newFactory.Load<DummyBusinessObject>(bizO1.PK);
				var dummyBizoTable = newFactory.RowFactory.data.Tables[DummyBizoSchema.Constants.TableName];
				AssertNotNull("bizO1 row", dummyBizoTable.Rows.Find(bizO1.PK.ToGuid()));

				AssertEquals("should now have a fetch hint for DummyDependentBizo", 1, newFactory.RowFactory.ActiveFetchHintsForTable(DummyDependentBizoSchema.Constants.TableName));

				var child01Biz01InNewFactory = newFactory.Load<DummyDependantBusinessObject>(child01Biz01.PK);

				var dummyDependentBizoTable = newFactory.RowFactory.data.Tables[DummyDependentBizoSchema.Constants.TableName];
				AssertEquals("Should also contain the other children of bizO1, but none from bizO2", 3, dummyDependentBizoTable.Rows.Count);
				AssertNotNull("child01Biz01 row", dummyDependentBizoTable.Rows.Find(child01Biz01.PK.ToGuid()));
				AssertNotNull("child02Biz01 row", dummyDependentBizoTable.Rows.Find(child02Biz01.PK.ToGuid()));
				AssertNotNull("child03Biz01 row", dummyDependentBizoTable.Rows.Find(child03Biz01.PK.ToGuid()));
			}
		}

		public void TestAddTableFetchHintCreator_IgnoresLoadedTable()
		{
			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Code = "bizO1";

			var child01Biz01 = Factory.New<DummyDependantBusinessObject>();
			child01Biz01.ZD1_Z0 = bizO1.PK;
			var child02Biz01 = Factory.New<DummyDependantBusinessObject>();
			child02Biz01.ZD1_Z0 = bizO1.PK;
			var child03Biz01 = Factory.New<DummyDependantBusinessObject>();
			child03Biz01.ZD1_Z0 = bizO1.PK;

			var unlinkedChild = Factory.New<DummyDependantBusinessObject>();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var bizO1InNewFactory = newFactory.Load<DummyBusinessObject>(bizO1.PK);
			var dummyBizoTable = newFactory.RowFactory.data.Tables[DummyBizoSchema.Constants.TableName];
			AssertNotNull("bizO1 row", dummyBizoTable.Rows.Find(bizO1.PK.ToGuid()));

			IExternalFetchHintSupporter supporter = newFactory;
			using (supporter.SetupCreator())
			{
				supporter.AddTableFetchHintCreator(DummyBizoSchema.Instance, (row) =>
				{
					var bizoPK = Extensions.DataIndexerExtensions.GetValue(row, DummyBizoSchema.PK);
					return new IFetchHint[] { new FetchHint(DummyDependentBizoSchema.ZD1_Z0, bizoPK) };
				});

				AssertEquals("No fetch hint for DummyDependentBizo until needed", 0, newFactory.RowFactory.ActiveFetchHintsForTable(DummyDependentBizoSchema.Constants.TableName));

				var child01Biz01InNewFactory = newFactory.Load<DummyDependantBusinessObject>(child01Biz01.PK);
				AssertEquals("Still No fetch hint for DummyDependentBizo because parent table already loaded, so never requested.", 0, newFactory.RowFactory.ActiveFetchHintsForTable(DummyDependentBizoSchema.Constants.TableName));

				var dummyDependentBizoTable = newFactory.RowFactory.data.Tables[DummyDependentBizoSchema.Constants.TableName];
				AssertEquals("Does not contain the other children of bizO1", 1, dummyDependentBizoTable.Rows.Count);
				AssertNotNull("child01Biz01 row", dummyDependentBizoTable.Rows.Find(child01Biz01.PK.ToGuid()));
			}
		}

		public void TestAddTableFetchHintCreator_ApplyToExistingRows()
		{
			var bizO1 = Factory.New<DummyBusinessObject>();
			bizO1.Z0_Code = "bizO1";

			var child01Biz01 = Factory.New<DummyDependantBusinessObject>();
			child01Biz01.ZD1_Z0 = bizO1.PK;
			var child02Biz01 = Factory.New<DummyDependantBusinessObject>();
			child02Biz01.ZD1_Z0 = bizO1.PK;
			var child03Biz01 = Factory.New<DummyDependantBusinessObject>();
			child03Biz01.ZD1_Z0 = bizO1.PK;

			var unlinkedChild = Factory.New<DummyDependantBusinessObject>();

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var bizO1InNewFactory = newFactory.Load<DummyBusinessObject>(bizO1.PK);
			var dummyBizoTable = newFactory.RowFactory.data.Tables[DummyBizoSchema.Constants.TableName];
			AssertNotNull("bizO1 row", dummyBizoTable.Rows.Find(bizO1.PK.ToGuid()));

			IExternalFetchHintSupporter supporter = newFactory;
			using (supporter.SetupCreator())
			{
				AssertEquals("No fetch hint for DummyDependentBizo exists", 0, newFactory.RowFactory.ActiveFetchHintsForTable(DummyDependentBizoSchema.Constants.TableName));

				supporter.AddTableFetchHintCreator(DummyBizoSchema.Instance, applyToExistingRows: true, getFetchHints: (row) =>
				{
					var bizoPK = Extensions.DataIndexerExtensions.GetValue(row, DummyBizoSchema.PK);
					return new IFetchHint[] { new FetchHint(DummyDependentBizoSchema.ZD1_Z0, bizoPK) };
				});

				AssertEquals("should now have a fetch hint for DummyDependentBizo", 1, newFactory.RowFactory.ActiveFetchHintsForTable(DummyDependentBizoSchema.Constants.TableName));

				var child01Biz01InNewFactory = newFactory.Load<DummyDependantBusinessObject>(child01Biz01.PK);
				var dummyDependentBizoTable = newFactory.RowFactory.data.Tables[DummyDependentBizoSchema.Constants.TableName];
				AssertEquals("Should also contain the other children of bizO1", 3, dummyDependentBizoTable.Rows.Count);
				AssertNotNull("child01Biz01 row", dummyDependentBizoTable.Rows.Find(child01Biz01.PK.ToGuid()));
				AssertNotNull("child02Biz01 row", dummyDependentBizoTable.Rows.Find(child02Biz01.PK.ToGuid()));
				AssertNotNull("child03Biz01 row", dummyDependentBizoTable.Rows.Find(child03Biz01.PK.ToGuid()));
			}
		}

		public void TestGetDatabaseCountSwitchesParamsToConstants()
		{
			RowFactory rowFactory = new RowFactory();

			rowFactory.GetDatabaseCount("DummyBizo", new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertContains("Last SQL Query (parameter)", "SELECT COUNT(*) FROM dbo.DummyBizo WHERE Z0_Code = @CWO1_", SqlEventTracker.Instance.LastSqlQuery);

			rowFactory.GetDatabaseCount("DummyBizo", new ZQuery(DummyBizoSchema.Z0_BitFalse, true));
			AssertContains("Last SQL Query (parameter)", "SELECT COUNT(*) FROM dbo.DummyBizo WHERE Z0_BitFalse = @CWO1_", SqlEventTracker.Instance.LastSqlQuery);

			rowFactory.GetDatabaseCount("DummyBizo", new ZQuery(DummyBizoSchema.Z0_BitFiltered, true));
			AssertContains("Last SQL Query (parameter)", "SELECT COUNT(*) FROM dbo.DummyBizo WHERE Z0_BitFiltered = 1", SqlEventTracker.Instance.LastSqlQuery);
		}

		public void TestCachedTypeBizOIsNotRefreshedFromUberFactoryCacheInSameFactoryIfDeleted()
		{
			using (RowFactory.SetCachedTables("DummyBizo"))
			{
				// Arrange
				DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
				bizO.Z0_Code = "123";
				var pK = bizO.PK;
				Factory.Save();
				bizO.Delete();

				// Act
				var bizO2 = Factory.Load<DummyBusinessObject>(pK);

				// Assert
				AssertEquals("BizO is still deleted", true, bizO.IsDeleted);
				AssertNull("BizO2 was not reloaded", bizO2);
			}
		}

		public void TestPartialMatchFetchAndTop1MakeSingleDBHit()
		{
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_Code = "123";
			Factory.Save();

			RowFactory rowFactory = new RowFactory();
			ZQuery fetchQuery = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "1");
			IFetchHint hint = new ZQueryFetchHint(DummyBizoSchema.Instance, fetchQuery);
			rowFactory.AddFetchHint(hint);
			ZQuery top1Query = new ZQuery(DummyBizoSchema.Z0_Code, "123")
			{
				MaximumRows = 1
			};
			DataRow[] rows = rowFactory.Load("DummyBizo", top1Query);
			AssertEquals(1, rows.Length);
			AssertEquals(1, rowFactory.DatabaseLoadCount);
		}

		public void TestUberFactoryIncludedInDbHits()
		{
			RowFactory rowFactory = new RowFactory();

			rowFactory.Load("RefCountry", new ZQuery() { MaximumRows = 1 });
			AssertEquals(1, rowFactory.DatabaseLoadCount);
			AssertEquals("RefCountry", rowFactory.TableSelects.Last().TableName);

			rowFactory.Load("RefCurrency", new ZQuery());
			AssertEquals(2, rowFactory.DatabaseLoadCount);
			AssertEquals(1, rowFactory.TableSelects.Single(x => x.TableName == "RefCountry").Value);
			AssertEquals(1, rowFactory.TableSelects.Single(x => x.TableName == "RefCurrency").Value);

			rowFactory.Load("RefCountry", new ZQuery() { MaximumRows = 2 });
			AssertEquals(3, rowFactory.DatabaseLoadCount);
			AssertEquals(2, rowFactory.TableSelects.Single(x => x.TableName == "RefCountry").Value);
			AssertEquals(1, rowFactory.TableSelects.Single(x => x.TableName == "RefCurrency").Value);
		}

		public void TestAddPrimaryKeyFetchHint()
		{
			ZGuid pK = DummyBusinessObject.New(Factory).PK;
			Factory.Save();
			RowFactory rowFactory = new RowFactory();

			rowFactory.AddFetchHint(new FetchHint(DummyBizoSchema.PK, pK));
			AssertEquals(1, rowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));

			rowFactory.AddFetchHint(new FetchHint(DummyBizoSchema.PK, pK));
			AssertEquals("Count should not move", 1, rowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));
		}

		public void TestAddFetchHintForColumn()
		{
			DummyBusinessObject bizO1 = DummyBusinessObject.New(Factory);
			Factory.Save();
			RowFactory rowFactory = new RowFactory();

			rowFactory.AddFetchHint(new FetchHint(DummyBizoSchema.PK, bizO1.PK));
			AssertEquals(1, rowFactory.ActiveFetchHintsForTable(DummyBizoSchema.Constants.TableName));

			rowFactory.fetcher.FetchTable(DummyBizoSchema.Constants.TableName);
			AssertEquals(1, rowFactory.DatabaseLoadCount);

			rowFactory.AddFetchHint(new FetchHint(DummyBizoSchema.PK, bizO1.PK));
			rowFactory.fetcher.FetchTable(DummyBizoSchema.Constants.TableName);
			AssertEquals(1, rowFactory.DatabaseLoadCount);
		}

		public void TestEnableFetchHintsProcessingWithoutTableHitCounter()
		{
			var factory1 = new BusinessObjectFactory();
			var dummy1 = factory1.New<DummyBusinessObject>();
			dummy1.Z0_Code = "AAA";
			var dummy2 = factory1.New<DummyBusinessObject>();
			dummy2.Z0_Code = "ZZZ";
			var dummy3 = factory1.New<DummyBusinessObject>();
			dummy3.Z0_Code = "ZAZ";
			factory1.Save();

			// FetchTable
			var rowFactory2 = new RowFactory();
			rowFactory2.AddFetchHint(new FetchHint(DummyBizoSchema.PK, dummy1.PK));
			rowFactory2.AddFetchHint(new FetchHint(DummyBizoSchema.PK, dummy2.PK));
			rowFactory2.fetcher.FetchTable(DummyBizoSchema.Constants.TableName);
			var tableSelect = rowFactory2.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect.Value);
			AssertEquals(1, rowFactory2.DatabaseLoadCount);
			AssertNotNull("Loading dummy1", rowFactory2.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy1.PK));
			tableSelect = rowFactory2.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect.Value);
			AssertEquals(1, rowFactory2.DatabaseLoadCount);
			AssertNotNull("Loading dummy2", rowFactory2.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy2.PK));
			tableSelect = rowFactory2.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect.Value);
			AssertEquals(1, rowFactory2.DatabaseLoadCount);
			AssertNotNull("Loading dummy3", rowFactory2.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy3.PK));
			tableSelect = rowFactory2.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(2, tableSelect.Value);
			AssertEquals(2, rowFactory2.DatabaseLoadCount);

			// FetchAllTables
			var rowFactory3 = new RowFactory();
			rowFactory3.AddFetchHint(new FetchHint(DummyBizoSchema.PK, dummy1.PK));
			rowFactory3.AddFetchHint(new FetchHint(DummyBizoSchema.PK, dummy2.PK));
			rowFactory3.fetcher.FetchAllTables();
			tableSelect = rowFactory3.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect.Value);
			AssertEquals(1, rowFactory3.DatabaseLoadCount);
			AssertNotNull("Loading dummy1", rowFactory3.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy1.PK));
			tableSelect = rowFactory3.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect.Value);
			AssertEquals(1, rowFactory3.DatabaseLoadCount);
			AssertNotNull("Loading dummy2", rowFactory3.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy2.PK));
			tableSelect = rowFactory3.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(1, tableSelect.Value);
			AssertEquals(1, rowFactory3.DatabaseLoadCount);
			AssertNotNull("Loading dummy3", rowFactory3.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy3.PK));
			tableSelect = rowFactory3.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals(2, tableSelect.Value);
			AssertEquals(2, rowFactory3.DatabaseLoadCount);

			var rowFactory4 = new RowFactory();
			rowFactory4.AddFetchHint(new FetchHint(DummyBizoSchema.PK, dummy1.PK));
			rowFactory4.AddFetchHint(new FetchHint(DummyBizoSchema.PK, dummy2.PK));
			using (rowFactory4.EnableFetchHintsProcessingWithoutTableHitCounter())
			{
				rowFactory4.fetcher.FetchAllTables();
				tableSelect = rowFactory4.GetTableHitCount(DummyBizoSchema.Constants.TableName);
				AssertEquals(1, rowFactory4.DatabaseLoadCount);
				AssertEquals("Load via fetch hint should not be counted", 0, tableSelect.Value);
				AssertNotNull("Loading dummy3", rowFactory4.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy3.PK));
				tableSelect = rowFactory4.GetTableHitCount(DummyBizoSchema.Constants.TableName);
				AssertEquals("dummy3 is not part of fetch hint, so it should count", 1, tableSelect.Value);
				AssertEquals(2, rowFactory4.DatabaseLoadCount);
			}
			AssertNotNull("Loading dummy2", rowFactory4.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy2.PK));
			tableSelect = rowFactory4.TableSelects.FirstOrDefault(x => x.TableName == DummyBizoSchema.Constants.TableName);
			AssertEquals("Shoubl be one from dummy3", 1, tableSelect.Value);
			AssertEquals(2, rowFactory4.DatabaseLoadCount);
		}

		public void TestFilterWithOrPartsWorksProperlyForPK()
		{
			DummyBusinessObject bizO1 = DummyBusinessObject.New(Factory);
			DummyBusinessObject bizO2 = DummyBusinessObject.New(Factory);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.PK, SQLComparisonOperator.Equal, bizO1.PK);
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.PK, SQLComparisonOperator.Equal, bizO2.PK);

			Factory.Load(typeof(DummyBusinessObject), filter);
			AssertEquals(0, Factory.DatabaseLoadCount);
		}

		public void TestFilterWithOrPartsWorksProperlyForForeignKey()
		{
			ZGuid zGuid1 = ZGuid.NewZGuid();
			ZGuid zGuid2 = ZGuid.NewZGuid();

			DummyBusinessObject bizO1 = DummyBusinessObject.New(Factory);
			bizO1.Z0_Guid = zGuid1;

			DummyBusinessObject bizO2 = DummyBusinessObject.New(Factory);
			bizO2.Z0_Guid = zGuid2;

			AssertEquals(0, Factory.ActiveTableFetchHints);
			Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, zGuid1);
			Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, zGuid2);
			AssertEquals(1, Factory.ActiveTableFetchHints);

			AssertEquals(0, Factory.DatabaseLoadCount);
			Factory.Load(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Guid, zGuid1)); // Force Fetch Hints to execute
			AssertEquals(1, Factory.DatabaseLoadCount);
			AssertEquals("Precondition", 0, Factory.ActiveTableFetchHints);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, zGuid1);
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Guid, SQLComparisonOperator.Equal, zGuid2);

			Factory.Load(typeof(DummyBusinessObject), filter);
			AssertEquals(1, Factory.DatabaseLoadCount);
		}

		[SnailTest]
		public void TestDataViewIndexPerformance()
		{
			RowFactory rowFactory = new RowFactory
			{
				IndexingEnabled = false
			};
			for (int i = 0; i < 50000; i++)
			{
				DataRow row = rowFactory.New(DummyBizoSchema.Constants.TableName);
				DummyBusinessObject.SetDataRowDefaultValues(row);
				row.Table.Rows.Add(row); // this is done by bizO factory, once defaults are set
			}

			AssertEquals("Precondition", 50000, rowFactory.GetTable(DummyBizoSchema.Constants.TableName).Rows.Count);

			Stopwatch hpc = Stopwatch.StartNew();

			ZQuery nonMatchingFilter = new ZQuery(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid());
			ZQuery innerFilter = new ZQuery();
			innerFilter.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, ZDateTime.Now);
			innerFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Date, SQLComparisonOperator.Equal, ZDateTime.Now.AddDays(1));
			nonMatchingFilter.AddToFilter(innerFilter);

			for (int selectCount = 0; selectCount < 100; selectCount++)
			{
				rowFactory.Select(DummyBizoSchema.Constants.TableName, nonMatchingFilter);
			}

			hpc.Stop();
			double resultWithoutIndex = hpc.Elapsed.TotalMilliseconds;

			hpc = Stopwatch.StartNew();
			rowFactory.IndexingEnabled = true;

			DataTable table = rowFactory.GetTable(DummyBizoSchema.Constants.TableName);
			for (int selectCount = 0; selectCount < 100; selectCount++)
			{
				rowFactory.Select(DummyBizoSchema.Constants.TableName, nonMatchingFilter);
			}

			hpc.Stop();
			double resultWithIndex = hpc.Elapsed.TotalMilliseconds;

			string msg = "Should be ten times faster with index (With Index: {0}, Without Index: {1})";
			Assert(string.Format(msg, resultWithIndex, resultWithoutIndex), resultWithIndex * 10 < resultWithoutIndex);
		}

		public void TestOrderIsUnimportantForIndexSelection()
		{
			RowFactory rowFactory = new RowFactory
			{
				IndexingEnabled = true
			};
			for (int i = 0; i < 10000; i++)
			{
				DataRow row = rowFactory.New(DummyBizoSchema.Constants.TableName);
				DummyBusinessObject.SetDataRowDefaultValues(row);
				row.Table.Rows.Add(row); // this is done by bizO factory, once defaults are set
				row[DummyBizoSchema.Z0_Code.Name] = "ABC";  // totally not unique...
				row[DummyBizoSchema.Z0_Guid.Name] = Guid.NewGuid(); // totally so unique...
			}

			AssertEquals("Precondition", 10000, rowFactory.GetTable(DummyBizoSchema.Constants.TableName).Rows.Count);

			ZQuery queryWithGuidFirst = new ZQuery(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid());
			queryWithGuidFirst.AddToFilter(DummyBizoSchema.Z0_Code, "ABC");

			rowFactory.Select(DummyBizoSchema.Constants.TableName, queryWithGuidFirst);

			var sw1 = Stopwatch.StartNew();
			for (int i = 0; i < 10000; i++)
			{
				rowFactory.Select(DummyBizoSchema.Constants.TableName, queryWithGuidFirst);
			}
			sw1.Stop();

			ZQuery queryWithCodeFirst = new ZQuery(DummyBizoSchema.Z0_Code, "ABC");
			queryWithCodeFirst.AddToFilter(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid());

			rowFactory.Select(DummyBizoSchema.Constants.TableName, queryWithCodeFirst);

			var sw2 = Stopwatch.StartNew();
			for (int i = 0; i < 10000; i++)
			{
				rowFactory.Select(DummyBizoSchema.Constants.TableName, queryWithCodeFirst);
			}
			sw2.Stop();

			const string msg = "Should be approximately the same regardless of order (Guid First: {0}, Code First: {1})";
			double ratio = sw1.ElapsedMilliseconds / ((double)sw2.ElapsedMilliseconds);
			Assert(string.Format(msg, sw1.ElapsedMilliseconds, sw2.ElapsedMilliseconds), ratio > .5 && ratio < 2.0);
		}

		#region Test thread-safe

		public void TestAccessIsThreadSafe()
		{
			RowFactory.uniqueColumns = null;
			List<Thread> threads = new List<Thread>();
			for (int i = 0; i < 10; i++)
			{
				Thread t = new Thread(new ThreadStart(HammerFactory));
				threads.Add(t);
				t.Start();
			}

			foreach (Thread t in threads)
			{
				t.Join();
			}
			Assert("Should not die", true);
		}

		void HammerFactory()
		{
			using (Db.DisposableActionForDbConnection())
			{
				Thread.Sleep(0);
				new RowFactory().CheckKeyIsUnique("GlbBranch", "GB_Code");
				Assert("Should not die", true);
			}
		}

		[UseSnapshotProtection]
		public void TestCheckKeyIsUnique()
		{
			var dbName = $"TestDB{nameof(TestCheckKeyIsUnique)}";

			// clear previous cache
			RowFactory.ResetUniqueColumnInfo();

			using (AdoTestUtils.CreateDbDropExistingDisposable(dbName))
			{
				// cache uniqueColumns in empty db (dbName)
				new RowFactory().CheckKeyIsUnique("DUMMYBIZO", "Z0_CODE");

				AssertNoExceptionThrown
				(
					"Should not access cache from empty db (dbName) that has no GlbBranch.GB_Code index and give error 'LoadFromNaturalKey was used on a table + field that does not have a unique index.'",
					() => new RowFactory().CheckKeyIsUnique("GlbBranch", "GB_Code")
				);
			}
		}

		#endregion

		#region Test DBOnlyQuery Caching

		public void TestDBOnlyQueryPartsAreCachedWithRowsResult()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = 1;
			dummy.Z0_Code = "XYZ";
			Factory.Save();

			ZQuery standardQuery = new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThanOrEqualTo, 0);

			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Code, "XYZ");

			ZQuery fullQuery = new ZQuery(standardQuery, dbOnlyQuery);

			RowFactory rowFactory = new RowFactory();

			AssertNull("Precondition - nothing cached yet", rowFactory.GetCachedDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, fullQuery));
			AssertNull("Precondition - nothing cached yet", rowFactory.GetCachedDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, standardQuery));
			AssertNull("Precondition - nothing cached yet", rowFactory.GetCachedDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, dbOnlyQuery));

			AssertNull("Precondition - nothing cached yet", rowFactory.GetCachedNarrowDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, fullQuery));
			AssertNull("Precondition - nothing cached yet", rowFactory.GetCachedNarrowDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, standardQuery));
			AssertNull("Precondition - nothing cached yet", rowFactory.GetCachedNarrowDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, dbOnlyQuery));

			rowFactory.Load(DummyBizoSchema.Constants.TableName, fullQuery);

			AssertNotNull("Full query should be cached", rowFactory.GetCachedDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, fullQuery));
			AssertNull("Query parts should not be cached in main cache", rowFactory.GetCachedDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, standardQuery));
			AssertNull("Query parts should not be cached in main cache", rowFactory.GetCachedDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, dbOnlyQuery));

			AssertNull("Full query should not be cached in narrow cache", rowFactory.GetCachedNarrowDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, fullQuery));
			AssertNotNull("Standard query should be cached as it is unclear if it is DBOnly at this stage", rowFactory.GetCachedNarrowDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, standardQuery));
			AssertNotNull("DBOnlyQuery part should be cached", rowFactory.GetCachedNarrowDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, dbOnlyQuery));
		}

		public void TestDBOnlyQueryFetchHintIsCached()
		{
			DummyBusinessObject dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = "XYZ";
			Factory.Save();

			ZDBOnlyQuery dbOnlyQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			dbOnlyQuery.AddToFilter(DummyBizoSchema.Z0_Code, "XYZ");

			RowFactory rowFactory = new RowFactory();

			AssertNull("Precondition - nothing cached yet", rowFactory.GetCachedDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, dbOnlyQuery));

			rowFactory.AddFetchHint(new ZQueryFetchHint(DummyBizoSchema.Instance, dbOnlyQuery));
			rowFactory.ExecuteAllFetchHints();

			DataRow[] cachedRows = rowFactory.GetCachedDbOnlyQueryResult(DummyBizoSchema.Constants.TableName, dbOnlyQuery);
			AssertNotNull(cachedRows);
			AssertEquals(1, cachedRows.Length);
			AssertEquals(dummy.PK.ToGuid(), cachedRows[0][DummyBizoSchema.Constants.PK]);
		}

		#endregion

		public void TestClearCacheByTableNameClearsEntireCacheWhenRequired()
		{
			var rowFactory = new RowFactory();
			rowFactory.SeedQueryCache("Table1", new ZQuery());
			AssertEquals(1, rowFactory.QueryCache.CachedKeys.Length);

			rowFactory.ClearQueryCache("Table2");
			AssertEquals(true, rowFactory.IsCached("Table1", new ZQuery()));
			AssertEquals(1, rowFactory.QueryCache.CachedKeys.Length);

			rowFactory.ShouldPerformFullQueryCacheCleanOnSave = true;
			rowFactory.ClearQueryCache("Table2");
			AssertEquals(false, rowFactory.IsCached("Table1", new ZQuery()));
			AssertEquals(0, rowFactory.QueryCache.CachedKeys.Length);
		}

		public void TestClearCacheIsLazy()
		{
			var rowFactory = new RowFactory();
			rowFactory.SeedQueryCache("Table1", new ZQuery());
			AssertEquals(true, rowFactory.IsCached("Table1", new ZQuery()));
			rowFactory.ClearQueryCache("Table1");
			AssertEquals("Not cleared until we actually read from it.", 1, rowFactory.QueryCache.CachedKeys.Length);
			AssertEquals(false, rowFactory.IsCached("Table1", new ZQuery()));
			AssertEquals(0, rowFactory.QueryCache.CachedKeys.Length);
		}

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
		}
	}
}
