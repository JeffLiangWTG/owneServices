using System;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Shared
{
	public sealed class DbColumnDependencyRemoverNonTransactionalTest : TestCase
	{
		public void TestDropRelatedPrimaryKey()
		{
			using (var testConn = Db.NewAdminConnection())
			{
				string testDbName = "~DbColumnDependencyRemoverTestDb~";
				string testTableName = "TestTable";

				try
				{
					AdoTestUtils.CreateDbDropExisting(testConn, testDbName);

					// enable CHANGE TRACKING on test DB
					string sqlText = String.Format("ALTER DATABASE [{0}] SET CHANGE_TRACKING = ON", testDbName);
					testConn.ExecuteNonQuery(sqlText);

					using (((ICurrentDbControl)testConn).UseDatabase(testDbName))
					{
						// create table, column, pk and enable table change tracking...
						sqlText = String.Format(@"
						CREATE TABLE [{0}] (ColToKeep int default 0, ColToDrop int not null, CONSTRAINT [TestPk] PRIMARY KEY NONCLUSTERED(ColToDrop));
						ALTER TABLE [{0}] ENABLE CHANGE_TRACKING;",
							testTableName);
						testConn.ExecuteNonQuery(sqlText);

						AssertEquals("Column exists?", true, DbObjectCreator.ColumnExists(testConn, testTableName, "ColToDrop"));
						AssertEquals("PK exists?", true, DoesTableHavePrimaryKey(testConn, testTableName));
						AssertEquals("Is change tracking enabled?", true, IsChangeTrackingEnabled(testConn, testTableName));

						// remove dependencies
						var columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, testTableName, "ColToDrop");
						columnDependencyRemover.DropRelateObjects(testConn);

						AssertEquals("Column exists?", true, DbObjectCreator.ColumnExists(testConn, testTableName, "ColToDrop"));
						AssertEquals("PK exists?", false, DoesTableHavePrimaryKey(testConn, testTableName));
						AssertEquals("Is change tracking enabled?", false, IsChangeTrackingEnabled(testConn, testTableName));

						// remove column... 
						sqlText = String.Format("ALTER TABLE [{0}] DROP COLUMN ColToDrop", testTableName);
						testConn.ExecuteNonQuery(sqlText);

						AssertEquals("Column exists?", false, DbObjectCreator.ColumnExists(testConn, testTableName, "ColToDrop"));
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testConn, testDbName);
				}
			}
		}

		public void TestDropRelatedPrimaryKey_WithSpatialIndex()
		{
			using (var testConn = Db.NewAdminConnection())
			{
				var testDbName = "~DbColumnDependencyRemoverTestDb~";
				var testTableName = "TestTable";
				var testSpatialIndexName = "TestSpatialIndex";

				try
				{
					AdoTestUtils.CreateDbDropExisting(testConn, testDbName);

					// enable CHANGE TRACKING on test DB
					string sqlText = String.Format("ALTER DATABASE [{0}] SET CHANGE_TRACKING = ON", testDbName);
					testConn.ExecuteNonQuery(sqlText);

					using (((ICurrentDbControl)testConn).UseDatabase(testDbName))
					{
						// create table, column, pk and enable table change tracking...
						sqlText = String.Format(@"
						CREATE TABLE [{0}] (ColToKeep int default 0, ColToDrop int not null, GeoData geography not null default convert(geography, 'POINT EMPTY') CONSTRAINT [TestPk] PRIMARY KEY CLUSTERED(ColToDrop));
						CREATE SPATIAL INDEX [{1}] ON [{0}] ([GeoData])
							USING GEOGRAPHY_AUTO_GRID
							WITH (CELLS_PER_OBJECT = 16)
						ALTER TABLE [{0}] ENABLE CHANGE_TRACKING;",
							testTableName, testSpatialIndexName);
						testConn.ExecuteNonQuery(sqlText);

						AssertEquals("Column exists?", true, DbObjectCreator.ColumnExists(testConn, testTableName, "ColToDrop"));
						AssertEquals("PK exists?", true, DoesTableHavePrimaryKey(testConn, testTableName));
						AssertEquals("Is change tracking enabled?", true, IsChangeTrackingEnabled(testConn, testTableName));
						AssertEquals("Spatial index exists?", true, DoesTableHaveSpatialIndex(testConn, testTableName));

						// remove dependencies
						var columnDependencyRemover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, testTableName, "ColToDrop");
						columnDependencyRemover.DropRelateObjects(testConn);

						AssertEquals("Column exists?", true, DbObjectCreator.ColumnExists(testConn, testTableName, "ColToDrop"));
						AssertEquals("PK exists?", false, DoesTableHavePrimaryKey(testConn, testTableName));
						AssertEquals("Is change tracking enabled?", false, IsChangeTrackingEnabled(testConn, testTableName));
						AssertEquals("Spatial index exists?", false, DoesTableHaveSpatialIndex(testConn, testTableName));

						// remove column... 
						sqlText = String.Format("ALTER TABLE [{0}] DROP COLUMN ColToDrop", testTableName);
						testConn.ExecuteNonQuery(sqlText);

						AssertEquals("Column exists?", false, DbObjectCreator.ColumnExists(testConn, testTableName, "ColToDrop"));
					}
				}
				finally
				{
					AdoTestUtils.DropDbIfExists(testConn, testDbName);
				}
			}
		}

		bool DoesTableHavePrimaryKey(DbConnection conn, string tableName)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM sys.tables tab 
				INNER JOIN sys.key_constraints pk ON tab.object_id = pk.parent_object_id
				WHERE tab.name = '{0}' AND pk.type = 'PK'",
				tableName);
			bool result = (Convert.ToInt32(conn.ExecuteScalar(sqlText)) > 0);
			return result;
		}

		bool DoesTableHaveSpatialIndex(DbConnection conn, string tableName)
		{
			string sqlText = string.Format(@"
				SELECT
					count(ind.name)
				FROM
					sys.columns col
					INNER JOIN sys.tables tab ON tab.object_id = col.object_id
					INNER JOIN sys.key_constraints constobj ON constobj.parent_object_id = tab.object_id
					INNER JOIN sys.index_columns indkey
						ON indkey.object_id = tab.object_id AND indkey.column_id = col.column_id AND indkey.index_id = constobj.unique_index_id
					INNER JOIN sys.spatial_indexes ind ON ind.object_id = indkey.object_id
				WHERE
					tab.name = '{0}'",
				tableName);
			bool result = (Convert.ToInt32(conn.ExecuteScalar(sqlText)) > 0);
			return result;
		}

		bool IsChangeTrackingEnabled(DbConnection conn, string tableName)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM sys.tables tab
				INNER JOIN sys.change_tracking_tables ctt ON ctt.object_id = tab.object_id
				WHERE tab.name = '{0}'",
				tableName);
			bool result = (Convert.ToInt32(conn.ExecuteScalar(sqlText)) > 0);
			return result;
		}
	}
}
