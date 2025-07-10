using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.PreUpgrade.Testing
{
	public abstract class PopulateAuditTimeAndUserTest<T> : TransactionedTestCase where T : PopulateAuditTimeAndUser, new()
	{
		protected T GetTransformation()
		{
			return new T();
		}

		abstract protected string CreateTwoRows();

		#region TestTransform

		protected virtual void PrepareTestDatabase() { }

		public void TestTransformWhenNoHighWatermark()
		{
			PrepareTestDatabase();

			PopulateAuditTimeAndUser transformation = GetTransformation();
			var tableName = transformation.TableSchema.TableName;
			if (DbObjectCreator.TableExists(Db.Connection, tableName))
			{
				var info = (IPopulateAuditTimeAndUserInfo)transformation;
				Guid tablePK;
				using (TestWhsDataSetupHelper.SuspendInsertAuditTrigger(tableName))
				{
					tablePK = new Guid(this.CreateTwoRows());
				}
				SetUpData(info, tablePK);

				var columnTime = (info.CreateTime != null) ? info.CreateTime.Name : info.LastEditTime.Name;
				var columnUser = (info.CreateUser != null) ? info.CreateUser.Name : info.LastEditUser.Name;
				var columnTimeAllowNullValues = (info.CreateTime != null) ? info.CreateTime.IsNullable : info.LastEditTime.IsNullable;

				AssertNotNull("At least one set of populating columns should exist", info.CreateTime ?? info.LastEditTime);

				if (info.CreateTime != null)
				{
					AssertEquals("[PRE-CONDITION] New column exists?", false, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.CreateTime.Name));
					AssertEquals("[PRE-CONDITION] New column exists?", false, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.CreateUser.Name));
				}

				if (info.LastEditTime != null)
				{
					AssertEquals("[PRE-CONDITION] New column exists?", false, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.LastEditTime.Name));
					AssertEquals("[PRE-CONDITION] New column exists?", false, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.LastEditUser.Name));
				}

				transformation.Run();

				if (info.CreateTime != null)
				{
					AssertEquals("[PRE-CONDITION] New column exists?", true, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.CreateTime.Name));
					AssertEquals("[PRE-CONDITION] New column exists?", true, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.CreateUser.Name));
				}

				if (info.LastEditTime != null)
				{
					AssertEquals("[PRE-CONDITION] New column exists?", true, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.LastEditTime.Name));
					AssertEquals("[PRE-CONDITION] New column exists?", true, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.LastEditUser.Name));
				}

				var expectedCreateTime = "2004-01-01 00:01:00";
				var expectedCreateUser = "XYZ";
				var expectedEditTime = "2004-01-01 00:06:00";
				var expectedEditUser = "U_2";

				AssertPopulatedRow(info, tablePK, expectedCreateTime, expectedCreateUser, expectedEditTime, expectedEditUser);
				AssertEquals("Default value count", ExpectedDefaultValueCount, GetNewValueCount(info.TableSchema.TableName, columnTime, columnUser, (columnTimeAllowNullValues ? "NULL" : "NOT NULL"), transformation.DefaultUserValue));

				UpdateRow(info, tablePK, "NULL", "''", "NULL", "''");
				AssertPopulatedRow(info, tablePK, "NULL", "", "NULL", "");
				transformation.Run();
				AssertPopulatedRow(info, tablePK, expectedCreateTime, expectedCreateUser, expectedEditTime, expectedEditUser);

				UpdateRow(info, tablePK, "'2010-10-10 10:10:00'", "'XXX'", "'2010-10-10 10:10:00'", "'XXX'");
				AssertPopulatedRow(info, tablePK, "2010-10-10 10:10:00", "XXX", "2010-10-10 10:10:00", "XXX");
				transformation.Run();

				if (!transformation.ShouldOverwriteOldValues)
				{
					AssertPopulatedRow(info, tablePK, "2010-10-10 10:10:00", "XXX", "2010-10-10 10:10:00", "XXX");
				}
				else
				{
					AssertPopulatedRow(info, tablePK, expectedCreateTime, expectedCreateUser, expectedEditTime, expectedEditUser);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestTransformWhenHighWatermarkRecorded()
		{
			PrepareTestDatabase();

			PopulateAuditTimeAndUser transformation = GetTransformation();

			var tableName = transformation.TableSchema.TableName;
			RecordHighWatermark(transformation.TableSchema.SqlSchemaName, tableName, new DateTime(2004, 1, 1, 1, 5, 0, DateTimeKind.Utc));

			if (DbObjectCreator.TableExists(Db.Connection, tableName))
			{
				var info = (IPopulateAuditTimeAndUserInfo)transformation;
				Guid tablePK;
				using (TestWhsDataSetupHelper.SuspendInsertAuditTrigger(tableName))
				{
					tablePK = new Guid(this.CreateTwoRows());
				}
				SetUpData(info, tablePK);

				var columnTime = (info.CreateTime != null) ? info.CreateTime.Name : info.LastEditTime.Name;
				var columnUser = (info.CreateUser != null) ? info.CreateUser.Name : info.LastEditUser.Name;
				var columnTimeAllowNullValues = (info.CreateTime != null) ? info.CreateTime.IsNullable : info.LastEditTime.IsNullable;

				AssertNotNull("At least one set of populating columns should exist", info.CreateTime ?? info.LastEditTime);

				if (info.CreateTime != null)
				{
					AssertEquals("[PRE-CONDITION] New column exists?", false, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.CreateTime.Name));
					AssertEquals("[PRE-CONDITION] New column exists?", false, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.CreateUser.Name));
				}

				if (info.LastEditTime != null)
				{
					AssertEquals("[PRE-CONDITION] New column exists?", false, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.LastEditTime.Name));
					AssertEquals("[PRE-CONDITION] New column exists?", false, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.LastEditUser.Name));
				}

				transformation.Run();

				if (info.CreateTime != null)
				{
					AssertEquals("[PRE-CONDITION] New column exists?", true, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.CreateTime.Name));
					AssertEquals("[PRE-CONDITION] New column exists?", true, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.CreateUser.Name));
				}

				if (info.LastEditTime != null)
				{
					AssertEquals("[PRE-CONDITION] New column exists?", true, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.LastEditTime.Name));
					AssertEquals("[PRE-CONDITION] New column exists?", true, DbObjectCreator.ColumnExists(TestConnection, info.TableSchema.TableName, info.LastEditUser.Name));
				}

				var expectedCreateTime = "2004-01-01 00:06:00";
				var expectedCreateUser = "U_2";
				var expectedEditTime = "2004-01-01 00:06:00";
				var expectedEditUser = "U_2";

				AssertPopulatedRow(info, tablePK, expectedCreateTime, expectedCreateUser, expectedEditTime, expectedEditUser);
				AssertEquals("Default value count", ExpectedDefaultValueCount, GetNewValueCount(info.TableSchema.TableName, columnTime, columnUser, (columnTimeAllowNullValues ? "NULL" : "NOT NULL"), transformation.DefaultUserValue));

				UpdateRow(info, tablePK, "NULL", "''", "NULL", "''");
				AssertPopulatedRow(info, tablePK, "NULL", "", "NULL", "");
				transformation.Run();
				AssertPopulatedRow(info, tablePK, expectedCreateTime, expectedCreateUser, expectedEditTime, expectedEditUser);

				UpdateRow(info, tablePK, "'2010-10-10 10:10:00'", "'XXX'", "'2010-10-10 10:10:00'", "'XXX'");
				AssertPopulatedRow(info, tablePK, "2010-10-10 10:10:00", "XXX", "2010-10-10 10:10:00", "XXX");
				transformation.Run();

				if (!transformation.ShouldOverwriteOldValues)
				{
					AssertPopulatedRow(info, tablePK, "2010-10-10 10:10:00", "XXX", expectedEditTime, expectedEditUser);
				}
				else
				{
					AssertPopulatedRow(info, tablePK, expectedCreateTime, expectedCreateUser, expectedEditTime, expectedEditUser);
				}
			}
			else
			{
				Assert(true);
			}
		}

		void RecordHighWatermark(string schemaName, string tableName, DateTimeOffset dateTimeOffset)
		{
			ExtProperty.Table.Update(
				TestConnection,
				schemaName,
				tableName,
				HighWatermarks.PopulateAuditTimeAndUserHighWatermark,
				dateTimeOffset.UtcDateTime.ToString("s"));
		}

		void AssertPopulatedRow(IPopulateAuditTimeAndUserInfo info, Guid tablePK, string expectedCreateTime, string expectedCreateUser, string expectedEditTime, string expectedEditUser)
		{
			var columns = String.Format("{0}{1}{2}{3}",
				(info.CreateTime != null) ? String.Format(", {0} = ISNULL(CONVERT(varchar(19), {0}, 120), 'NULL')", info.CreateTime.Name) : String.Empty,
				(info.CreateUser != null) ? String.Format(", {0}", info.CreateUser.Name) : String.Empty,
				(info.LastEditTime != null) ? String.Format(", {0} = ISNULL(CONVERT(varchar(19), {0}, 120), 'NULL')", info.LastEditTime.Name) : String.Empty,
				(info.LastEditUser != null) ? String.Format(", {0}", info.LastEditUser.Name) : String.Empty
				);

			var sql = String.Format(@"
SELECT
	PK = {1}
	{3}
FROM
	{0}
WHERE
	{1} = '{2}'
;",
				info.TableSchema.TableName, // 0
				info.TableSchema.PK.Name,   // 1
				tablePK,                    // 2
				columns                     // 3
				);

			using (var cmd = TestConnection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					if (info.CreateTime != null)
					{
						AssertEquals("CreateTime", expectedCreateTime, (string)reader[info.CreateTime.Name]);
					}

					if (info.CreateUser != null)
					{
						AssertEquals("CreateUser", expectedCreateUser, (string)reader[info.CreateUser.Name]);
					}

					if (info.LastEditTime != null)
					{
						AssertEquals("LastEditTime", expectedEditTime, (string)reader[info.LastEditTime.Name]);
					}

					if (info.LastEditUser != null)
					{
						AssertEquals("LastEditUser", expectedEditUser, (string)reader[info.LastEditUser.Name]);
					}
				}
			}
		}

		public void TestShouldOverwriteOldValues()
		{
			PopulateAuditTimeAndUser transformation = GetTransformation();
			AssertEquals("ShouldOverwriteOldValues", ExpectedShouldOverwriteOldValues, transformation.ShouldOverwriteOldValues);
		}

		public void TestDefaultUserValue()
		{
			PopulateAuditTimeAndUser transformation = GetTransformation();
			AssertEquals(ExpectedDefaultUserValue, transformation.DefaultUserValue);
		}

		protected virtual bool ExpectedShouldOverwriteOldValues
		{
			get { return false; }
		}

		protected virtual string ExpectedDefaultUserValue
		{
			get { return null; }
		}

		protected int ExpectedDefaultValueCount { get; set; } = 1;

		protected void UpdateRow(IPopulateAuditTimeAndUserInfo info, Guid tablePK, string newCreateTime, string newCreateUser, string newEditTime, string newEditUser)
		{
			var columns = String.Format("{0}{1}{2}{3}",
				(info.CreateTime != null) ? String.Format(", {0} = {1}", info.CreateTime.Name, newCreateTime) : String.Empty,
				(info.CreateUser != null) ? String.Format(", {0} = {1}", info.CreateUser.Name, newCreateUser) : String.Empty,
				(info.LastEditTime != null) ? String.Format(", {0} = {1}", info.LastEditTime.Name, newEditTime) : String.Empty,
				(info.LastEditUser != null) ? String.Format(", {0} = {1}", info.LastEditUser.Name, newEditUser) : String.Empty
				);

			string sqlText = String.Format(@"
UPDATE {0} SET
	{1} = '{2}'
	{3}
WHERE
	{1} = '{2}'
;",
				info.TableSchema.TableName, // {0}
				info.TableSchema.PK.Name,   // {1}
				tablePK,                    // {2}
				columns                     // {3}
				);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected int GetNewValueCount(string tableName, string columnTime, string columnUser, string time, string user)
		{
			string timeFilter = time.EndsWith("NULL", StringComparison.OrdinalIgnoreCase)
				? "is " + time
				: "= '" + time + "'";

			string sqlText = String.Format("SELECT COUNT(*) FROM {0} WHERE {1} {3} AND {2} = '{4}';",
				tableName,  // {0}
				columnTime, // {1}
				columnUser, // {2}
				timeFilter, // {3}
				user        // {4}
				);

			int result = (int)TestConnection.ExecuteScalar(sqlText);
			return result;
		}

		void SetUpData(IPopulateAuditTimeAndUserInfo info, Guid tablePK)
		{
			string sqlText = String.Format(@"
INSERT {0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}) VALUES
--  PK     , SL_Table, SL_Parent, SL_PostedTimeUtc  , SL_EventTime      , SL_GS_NKUser, SL_SE_NKEvent
	(NEWID(), '{9}'   , '{8}'    , '2004-01-01 00:01', '2004-01-02 00:01', 'XYZ'       , 'ADD'         ),
	(NEWID(), '{9}'   , '{8}'    , '2004-01-01 00:02', '2004-01-02 00:02', 'ABC'       , 'AAA'         ),
	(NEWID(), '{9}'   , '{8}'    , '2004-01-01 00:03', '2004-01-02 00:03', 'U_3'       , 'EDT'         ),
	(NEWID(), '{9}'   , '{8}'    , '2004-01-01 00:04', '2004-01-02 00:04', 'U_1'       , 'EDT'         ),
	(NEWID(), '{9}'   , '{8}'    , '2004-01-01 00:05', '2004-01-02 00:05', 'U_4'       , 'EDT'         ),
	(NEWID(), '{9}'   , '{8}'    , '2004-01-01 00:06', '2004-01-02 00:06', 'U_2'       , 'EDT'         )
;",
				StmALogSchema.Constants.TableName,        // 0
				StmALogSchema.Constants.PK,               // 1
				StmALogSchema.Constants.SL_Table,         // 2
				StmALogSchema.Constants.SL_Parent,        // 3
				StmALogSchema.Constants.SL_PostedTimeUtc, // 4
				StmALogSchema.Constants.SL_EventTime,     // 5
				StmALogSchema.Constants.SL_GS_NKUser,     // 6
				StmALogSchema.Constants.SL_SE_NKEvent,    // 7
				tablePK,                                  // 8
				info.TableSchema.TableName                // 9
				);

			TestConnection.ExecuteNonQuery(sqlText);

			DropColumnsForTestSetup(info);
		}

		void DropColumnsForTestSetup(IPopulateAuditTimeAndUserInfo info)
		{
			var columnList = new List<string>();
			var columnListWithoutComma = new List<string>();
			if (info.CreateTime != null)
			{
				columnList.Add(String.Format("'{0}'", info.CreateTime.Name));
				columnListWithoutComma.Add(info.CreateTime.Name);
			}

			if (info.CreateUser != null)
			{
				columnList.Add(String.Format("'{0}'", info.CreateUser.Name));
				columnListWithoutComma.Add(info.CreateUser.Name);
			}

			if (info.LastEditTime != null)
			{
				columnList.Add(String.Format("'{0}'", info.LastEditTime.Name));
				columnListWithoutComma.Add(info.LastEditTime.Name);
			}

			if (info.LastEditUser != null)
			{
				columnList.Add(String.Format("'{0}'", info.LastEditUser.Name));
				columnListWithoutComma.Add(info.LastEditUser.Name);
			}

			foreach (var colName in columnListWithoutComma)
			{
				var remover = new DbColumnDependencyRemover(Db.SqlDbOwnerSchema, info.TableSchema.TableName, colName);
				remover.DropRelateObjects(Db.Connection);
			}

			var sqlText = String.Format(@"
				-- DROP DEFAULT CONSTRAINTS
				DECLARE @dropDefaultScript varchar(max) = '';

				SELECT
					@dropDefaultScript = @dropDefaultScript + 'ALTER TABLE [{0}] DROP [' + constobj.name + ']; '
					FROM
						sys.columns col
						INNER JOIN sys.tables tab ON tab.object_id = col.object_id
						INNER JOIN sys.default_constraints constobj
							ON constobj.parent_object_id = tab.object_id AND constobj.parent_column_id = col.column_id
					WHERE
						tab.name = '{0}'
						AND col.name in ({1});

				IF (@dropDefaultScript != '') EXEC (@dropDefaultScript);

				-- DROP INDEXES
				DECLARE @dropIndexScript varchar(max) = '';

				SELECT
					@dropIndexScript = @dropIndexScript + 'DROP INDEX [' + name + '] ON [{0}]; '
					FROM
						(select distinct ind.Name, ind.index_Id from 
						sys.columns col
						INNER JOIN sys.tables tab ON tab.object_id = col.object_id
						INNER JOIN sys.index_columns indkey ON indkey.object_id = tab.object_id AND indkey.column_id = col.column_id
						INNER JOIN sys.indexes ind ON ind.object_id = col.object_id AND ind.index_id = indkey.index_id
					WHERE
						tab.name = '{0}'
						AND col.name in ({1})
						AND ind.type != 0
					) IQ
					ORDER BY
						index_id DESC;

				IF (@dropIndexScript != '') EXEC (@dropIndexScript);

				-- DROP COLUMNS
				DECLARE @dropColumnScript varchar(max) = '';

				SELECT
					@dropColumnScript = @dropColumnScript + 'ALTER TABLE [{0}] DROP COLUMN [' + col.name + ']; '
					FROM
						sys.columns col
						INNER JOIN sys.tables tab ON tab.object_id = col.object_id
					WHERE
						tab.name = '{0}'
						AND col.name in ({1});

				IF (@dropColumnScript != '') EXEC (@dropColumnScript);
				",
				info.TableSchema.TableName,
				String.Join(", ", columnList)
				);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		#endregion
	}
}
