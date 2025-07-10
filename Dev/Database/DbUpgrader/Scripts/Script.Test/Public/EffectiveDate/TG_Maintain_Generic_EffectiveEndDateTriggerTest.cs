using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.Testing.Public.EffectiveDate
{
	interface IContiguousHistory
	{
		DateTimeOffset EffectiveDate { get; }
		DateTimeOffset? AutoEffectiveEndDate { get; }
	}

	abstract class TG_GenericEndDateTriggerTest<TSchema, TEntity> : DBCreateTriggerScriptTest
		where TSchema : Schema, ITableSchema
		where TEntity : SQLDataObject<TEntity>, IContiguousHistory
	{
		readonly TSchema TableSchema = (TSchema)typeof(TSchema).GetField("Instance").GetValue(null);
		string TablePrefix => Schema.GetPrefixFromColumnName(TableSchema.PK.Name);
		string FullyQualifiedTableName => $"[{TableSchema.SqlSchemaName}].[{TableSchema.TableName}]";

		protected abstract IEnumerable<(string column, string sqlValue, object assertValue)> GetValidEdits();
		protected abstract TEntity Create(GlbStaff parent, DateTimeOffset effective);

		protected virtual void UpdateEffectiveDate(TEntity entity, DateTimeOffset newEffectiveDate)
			=> UpdateInDb(entity, $"{TablePrefix}_EffectiveDate", newEffectiveDate);

		protected virtual void UpdateInDb(TEntity entity, string column, object value)
		{
			Db.Connection.ExecuteNonQuery($"UPDATE {FullyQualifiedTableName} SET {column}=@v WHERE {TableSchema.PK.Name}=@pk", p =>
			{
				p.AddParameterBasedOnDbColumn("@v", value, TableSchema.GetSchemaColumn(column));
				p.AddParameter("@pk", System.Data.SqlDbType.UniqueIdentifier, entity.PK);
			});
		}

		public void TestSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var row = Create(staff, new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero));

			staff.AppendInsertAndReturnObject(sql);
			row.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, row.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var rows = new[]
			{
				Create(staff, new DateTime(2020, 1, 1)),
				Create(staff, new DateTime(2020, 2, 1)),
				Create(staff, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(rows, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, rows[0].PK)
				.ExpectEquals("A row's end date should match the proceeding row", l => l.AutoEffectiveEndDate, rows[1].EffectiveDate)
				.VerifyAll();

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, rows[1].PK)
				.ExpectEquals("A row's end date should match the proceeding row", l => l.AutoEffectiveEndDate, rows[2].EffectiveDate)
				.VerifyAll();

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, rows[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousRow = Create(staff, new DateTime(2020, 1, 1));
			var rowToUpdate = Create(staff, new DateTime(2020, 2, 1));
			var nextRow = Create(staff, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousRow, rowToUpdate, nextRow }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));
			UpdateEffectiveDate(rowToUpdate, newDate);

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, rowToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.AutoEffectiveEndDate, nextRow.EffectiveDate)
				.VerifyAll();

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, previousRow.PK)
				.ExpectEquals("The update should not change the previous row's effective date", r => r.EffectiveDate, previousRow.EffectiveDate)
				.ExpectEquals("The update should change the previous row's end date", r => r.AutoEffectiveEndDate, newDate)
				.VerifyAll();

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, nextRow.PK)
				.ExpectEquals("The update should not affect the next row's effective date", r => r.EffectiveDate, nextRow.EffectiveDate)
				.ExpectEquals("The update should not affect the next row's end date", r => r.AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousRow = Create(staff, new DateTime(2020, 1, 1));
			var rowToUpdate = Create(staff, new DateTime(2020, 2, 1));
			var nextRow = Create(staff, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousRow, rowToUpdate, nextRow }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));
			UpdateEffectiveDate(rowToUpdate, newDate);

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, rowToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.AutoEffectiveEndDate, null)
				.VerifyAll();

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, previousRow.PK)
				.ExpectEquals("The update should not change the previous row's effective date", r => r.EffectiveDate, previousRow.EffectiveDate)
				.ExpectEquals("The update should change the previous row's end date to the next row's end date", r => r.AutoEffectiveEndDate, nextRow.EffectiveDate)
				.VerifyAll();

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, nextRow.PK)
				.ExpectEquals("The update should not affect the next row's effective date", r => r.EffectiveDate, nextRow.EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertBothBeforeAndAfterCurrentRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");

			var previousRow = Create(staff, new DateTime(2020, 1, 1));
			var middleRow = Create(staff, new DateTime(2020, 2, 1));
			var nextRow = Create(staff, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousRow.AppendInsertAndReturnObject(sql);
			nextRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.EffectiveDate, middleRow.EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.AutoEffectiveEndDate, nextRow.EffectiveDate)
				.VerifyAll();

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, previousRow.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.AutoEffectiveEndDate, middleRow.EffectiveDate)
				.VerifyAll();

			SQLDataObject<TEntity>.AssertFromDB(TestConnection, nextRow.PK)
				.ExpectEquals("The next rows end date is null", r => r.AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousRow = Create(staff, new DateTime(2020, 1, 1));
			var rowToUpdate = Create(staff, new DateTime(2020, 2, 1));
			var nextRow = Create(staff, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousRow, rowToUpdate, nextRow }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			foreach (var (column, sqlValue, assertValue) in GetValidEdits())
			{
				UpdateInDb(rowToUpdate, column, assertValue);

				SQLDataObject<TEntity>.AssertFromDB(TestConnection, rowToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				SQLDataObject<TEntity>.AssertFromDB(TestConnection, previousRow.PK)
					.ExpectNotEquals($"The update should not affect the previous row ({column})", GetValue, assertValue)
					.VerifyAll();

				SQLDataObject<TEntity>.AssertFromDB(TestConnection, nextRow.PK)
					.ExpectNotEquals($"The update should not affect the next row ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(TEntity row)
					=> row.GetType().GetProperty(column).GetValue(row);
			}
		}

		public abstract void TestUpdateColumnOneByOne();

		protected void UpdateRowsOneByOneAndAssertChanged(Guid pk, Dictionary<string, string> initial, IEnumerable<string> shouldNeverBeUpdatedDirectly)
		{
			var realColumns = ColumnNames(TableSchema.TableName);
			var edits = GetValidEdits();
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), initial.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = initial.ToList();
			var insertStatement = $"INSERT INTO {FullyQualifiedTableName} ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE {FullyQualifiedTableName} SET {edit.column}={edit.sqlValue} WHERE {TableSchema.PK.Name}='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM {FullyQualifiedTableName} WHERE {TablePrefix}_PK='{pk}'");
					AssertEquals(edit.column, edit.assertValue, value);
				}
			});
		}

		public void TestInsertMultipleRowsInOneBatch()
		{
			var staffs = new[]
			{
				new GlbStaff("XYZ"),
				new GlbStaff("YZX"),
				new GlbStaff("ZXY"),
			};

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));
			var rows = new[]
			{
				Create(staffs[0], new DateTimeOffset(2020, 1, 1, 1, 0, 0, TimeSpan.FromHours(1))),
				Create(staffs[0], new DateTimeOffset(2020, 2, 1, 11, 0, 0, TimeSpan.FromHours(+7))),
				Create(staffs[1], new DateTimeOffset(2020, 3, 1, 3, 0, 0, TimeSpan.FromHours(+11))),
				Create(staffs[1], new DateTimeOffset(2020, 4, 1, 17, 0, 0, TimeSpan.FromHours(-3))),
				Create(staffs[2], new DateTimeOffset(2020, 1, 1, 9, 0, 0, TimeSpan.FromHours(+0))),
				Create(staffs[2], new DateTimeOffset(2020, 4, 1, 21, 0, 0, TimeSpan.FromHours(-5))),
			};

			TestConnection.ExecuteNonQuery(SQLDataObject<TEntity>.GetBulkInsertStatement(rows));

			AssertEquals("All six rows should be inserted", 6, SQLDataObject<TEntity>.CountInDB(TestConnection));
			AssertEffectiveDateHistory("First staff has two rows on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertEffectiveDateHistory("Second staff has two rows on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertEffectiveDateHistory("Third staff has two rows split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		void AssertEffectiveDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the expected EffectiveDates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();

			TestConnection.ExecuteReader($"SELECT {TablePrefix}_EffectiveDate, {TablePrefix}_AutoEffectiveEndDate FROM {FullyQualifiedTableName} WHERE {TablePrefix}_GS_Staff = @staff ORDER BY {TablePrefix}_EffectiveDate ASC", p => p.AddParameterBasedOnDbColumn("@staff", staff, GlbStaffSchema.PK), r =>
			{
				var effectiveDate = (DateTimeOffset)r[$"{TablePrefix}_EffectiveDate"];
				var effectiveEndDate = r[$"{TablePrefix}_AutoEffectiveEndDate"];

				actual.Add((effectiveDate, effectiveEndDate == DBNull.Value ? null : (DateTimeOffset?)effectiveEndDate));
			});

			var expected = expectedHistory.Zip(expectedHistory.Skip(1).Append(null), (a, b) => (a, b)).ToList();
			AssertSequencesEqual(message, expected, actual);
		}

		ICollection<string> ColumnNames(string tablename)
		{
			var result = new HashSet<string>();
			TestConnection.ExecuteReader(
				"select COLUMN_NAME from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = @table",
				p => p.AddParameter("@table", System.Data.SqlDbType.NVarChar, tablename),
				r => result.Add(r.GetString(0))
			);

			return result;
		}
	}
}
