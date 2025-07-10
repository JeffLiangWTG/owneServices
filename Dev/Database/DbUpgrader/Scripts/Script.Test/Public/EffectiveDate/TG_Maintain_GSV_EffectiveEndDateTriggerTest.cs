using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.EffectiveDate;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.EffectiveDate
{
	[TestedType(typeof(TG_Maintain_GSV_EffectiveEndDateTrigger))]
	[UseSnapshotProtection(skipTransaction: true)]
	class TG_Maintain_GSV_EffectiveEndDateTriggerTest : DBCreateTriggerScriptTest
	{
		public void TestFirstSingleInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var review = new GlbStaffReview(staff.PK, new DateTime(2020, 1, 1));

			staff.AppendInsertAndReturnObject(sql);
			review.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffReview.AssertFromDB(TestConnection, review.PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSV_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestOneStaffFirstMultiInsert()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var reviews = new[]
			{
				new GlbStaffReview(staff.PK, new DateTime(2020, 1, 1)),
				new GlbStaffReview(staff.PK, new DateTime(2020, 2, 1)),
				new GlbStaffReview(staff.PK, new DateTime(2020, 3, 1)),
			};

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(reviews, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffReview.AssertFromDB(TestConnection, reviews[0].PK)
				.ExpectEquals("A review's end date should match the proceeding row", l => l.GSV_AutoEffectiveEndDate, reviews[1].GSV_EffectiveDate)
				.VerifyAll();

			GlbStaffReview.AssertFromDB(TestConnection, reviews[1].PK)
				.ExpectEquals("A review's end date should match the proceeding row", l => l.GSV_AutoEffectiveEndDate, reviews[2].GSV_EffectiveDate)
				.VerifyAll();

			GlbStaffReview.AssertFromDB(TestConnection, reviews[2].PK)
				.ExpectEquals("There is no next end-date, so it should be null", l => l.GSV_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateFieldsOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSV = new GlbStaffReview(staff.PK, new DateTime(2020, 1, 1));
			var gslToUpdate = new GlbStaffReview(staff.PK, new DateTime(2020, 2, 1));
			var nextGSV = new GlbStaffReview(staff.PK, new DateTime(2020, 3, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSV, gslToUpdate, nextGSV }, r => r.AppendInsertAndReturnObject(sql));
			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			var thingsToUpdate = new (string column, string sqlValue, object assertValue)[]
			{
				(nameof(GlbStaffReview.GSV_GS_NKReviewer), "'CCC'", "CCC"),
				(nameof(GlbStaffReview.GSV_Score), "'11'", (byte)11),
				(nameof(GlbStaffReview.GSV_Comments), "'New Comment 1'", "New Comment 1"),
			};

			foreach (var (column, sqlValue, assertValue) in thingsToUpdate)
			{
				TestConnection.ExecuteNonQuery($@"
					UPDATE hrm.GlbStaffReview
					SET
						{column} = {sqlValue}
					WHERE
						GSV_PK = @pk
					", p => p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffReviewSchema.PK));

				GlbStaffReview.AssertFromDB(TestConnection, gslToUpdate.PK)
					.ExpectEquals($"The update statement should have had an effect ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffReview.AssertFromDB(TestConnection, previousGSV.PK)
					.ExpectNotEquals($"The update should not affect the previous review ({column})", GetValue, assertValue)
					.VerifyAll();

				GlbStaffReview.AssertFromDB(TestConnection, nextGSV.PK)
					.ExpectNotEquals($"The update should not affect the next review ({column})", GetValue, assertValue)
					.VerifyAll();

				object GetValue(GlbStaffReview review)
					=> review.GetType().GetProperty(column).GetValue(review);
			}
		}

		public void TestUpdateEffectiveDateOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSV = new GlbStaffReview(staff.PK, new DateTime(2020, 1, 1), "AA1");
			var gslToUpdate = new GlbStaffReview(staff.PK, new DateTime(2020, 2, 1), "BB2");
			var nextGSV = new GlbStaffReview(staff.PK, new DateTime(2020, 3, 1), "CC3");
			var newDate = new DateTimeOffset(new DateTime(2020, 2, 15));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSV, gslToUpdate, nextGSV }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffReview
				SET
					GSV_EffectiveDate = @newDate
				WHERE
					GSV_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffReviewSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffReviewSchema.GSV_EffectiveDate);
			});

			GlbStaffReview.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSV_EffectiveDate, newDate)
				.ExpectEquals("The row still has the same end date", r => r.GSV_AutoEffectiveEndDate, nextGSV.GSV_EffectiveDate)
				.VerifyAll();

			GlbStaffReview.AssertFromDB(TestConnection, previousGSV.PK)
				.ExpectEquals("The update should not change the previous review's effective date", r => r.GSV_EffectiveDate, previousGSV.GSV_EffectiveDate)
				.ExpectEquals("The update should change the previous review's end date", r => r.GSV_AutoEffectiveEndDate, newDate)
				.VerifyAll();

			GlbStaffReview.AssertFromDB(TestConnection, nextGSV.PK)
				.ExpectEquals("The update should not affect the next review's effective date", r => r.GSV_EffectiveDate, nextGSV.GSV_EffectiveDate)
				.ExpectEquals("The update should not affect the next review's end date", r => r.GSV_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		public void TestUpdateRearrangesEffectiveDatesOnExistingRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");
			var previousGSV = new GlbStaffReview(staff.PK, new DateTime(2020, 1, 1), "A1A");
			var gslToUpdate = new GlbStaffReview(staff.PK, new DateTime(2020, 2, 1), "B2B");
			var nextGSV = new GlbStaffReview(staff.PK, new DateTime(2020, 3, 1), "C3C");
			var newDate = new DateTimeOffset(new DateTime(2020, 6, 1));

			staff.AppendInsertAndReturnObject(sql);
			Array.ForEach(new[] { previousGSV, gslToUpdate, nextGSV }, r => r.AppendInsertAndReturnObject(sql));

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());
			TestConnection.ExecuteNonQuery(@"
				UPDATE hrm.GlbStaffReview
				SET
					GSV_EffectiveDate = @newDate
				WHERE
					GSV_PK = @pk
			", p =>
			{
				p.AddParameterBasedOnDbColumn("@pk", gslToUpdate.PK, GlbStaffReviewSchema.PK);
				p.AddParameterBasedOnDbColumn("@newDate", newDate, GlbStaffReviewSchema.GSV_EffectiveDate);
			});

			GlbStaffReview.AssertFromDB(TestConnection, gslToUpdate.PK)
				.ExpectEquals("The update statement should have had an effect", r => r.GSV_EffectiveDate, newDate)
				.ExpectEquals("The row is now current and has a null end date", r => r.GSV_AutoEffectiveEndDate, null)
				.VerifyAll();

			GlbStaffReview.AssertFromDB(TestConnection, previousGSV.PK)
				.ExpectEquals("The update should not change the previous review's effective date", r => r.GSV_EffectiveDate, previousGSV.GSV_EffectiveDate)
				.ExpectEquals("The update should change the previous review's end date to the next row's end date", r => r.GSV_AutoEffectiveEndDate, nextGSV.GSV_EffectiveDate)
				.VerifyAll();

			GlbStaffReview.AssertFromDB(TestConnection, nextGSV.PK)
				.ExpectEquals("The update should not affect the next review's effective date", r => r.GSV_EffectiveDate, nextGSV.GSV_EffectiveDate)
				.ExpectEquals("The update has now made this row no longer current and should update the end date", r => r.GSV_AutoEffectiveEndDate, newDate)
				.VerifyAll();
		}

		public void TestInsertMultipleStaffReviews()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[]
			{
				new GlbStaff("XYZ"),
				new GlbStaff("YZX"),
				new GlbStaff("ZXY"),
			};

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));
			TestConnection.ExecuteNonQuery(@"
				INSERT INTO hrm.GlbStaffReview
					(GSV_PK, GSV_GS_Staff, GSV_EffectiveDate, GSV_GS_NKReviewer, GSV_Score, GSV_Comments, GSV_SystemCreateTimeUtc, GSV_SystemLastEditTimeUtc, GSV_SystemCreateUser, GSV_SystemLastEditUser)
				VALUES
					(NEWID(), @staff0, '2020-01-01 01:00:00 +01:00', 'AC1', '1', 'Comment 101', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff0, '2020-02-01 11:00:00 +07:00', 'AC2', '5', 'Comment 111', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-03-01 03:00:00 +11:00', 'AC3', '10', 'Comment 121', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff1, '2020-04-01 17:00:00 -03:00', 'AC4', '15', 'Comment 131', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-01-01 09:00:00 +00:00', 'AC5', '20', 'Comment 141', GETUTCDATE(), GETUTCDATE(), 'E', 'E'),
					(NEWID(), @staff2, '2020-04-01 21:00:00 -05:00', 'AC6', '25', 'Comment 151', GETUTCDATE(), GETUTCDATE(), 'E', 'E')
			", p =>
			{
				for (var i = 0; i < staffs.Length; i++)
				{
					p.AddParameterBasedOnDbColumn("@staff" + i, staffs[i].PK, GlbStaffReviewSchema.GSV_GS_Staff);
				}
			});

			AssertEquals("All six rows should be inserted", 6, GlbStaffReview.CountInDB(TestConnection));
			AssertReviewDateHistory("First staff has two reviews on subsequent months", staffs[0].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 1, 0, 0), new TimeSpan(1, 0, 0)), new DateTimeOffset(new DateTime(2020, 2, 1, 11, 0, 0), new TimeSpan(7, 0, 0)));
			AssertReviewDateHistory("Second staff has two reviews on subsequent months", staffs[1].PK, new DateTimeOffset(new DateTime(2020, 3, 1, 3, 0, 0), new TimeSpan(11, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 17, 0, 0), new TimeSpan(-3, 0, 0)));
			AssertReviewDateHistory("Third staff has two reviews split over months", staffs[2].PK, new DateTimeOffset(new DateTime(2020, 1, 1, 9, 0, 0), new TimeSpan(0, 0, 0)), new DateTimeOffset(new DateTime(2020, 4, 1, 21, 0, 0), new TimeSpan(-5, 0, 0)));
		}

		public void TestUpdatingColumnsOneByOne()
		{
			var sql = new SqlQueryBuilder();
			var staffs = new[] { new GlbStaff("XYZ"), new GlbStaff("YZX") };

			TestConnection.ExecuteNonQuery(GlbStaff.GetBulkInsertStatement(staffs));

			var pk = Guid.NewGuid();
			var shouldNeverBeUpdatedDirectly = new HashSet<string>
			{
				"GSV_PK", // We won't ever update the primary key of a row
				"GSV_AutoEffectiveEndDate", // This is set by the trigger, so any value we set will be overwritten
				"GSV_AutoVersion", // Also set by a trigger, so the value we pass in is irrelevant
			};

			var original = new Dictionary<string, string>
			{
				{ "GSV_PK", $"'{pk}'" },
				{ "GSV_AutoVersion", "0" },
				{ "GSV_GS_Staff", $"'{staffs[0].PK}'" },
				{ "GSV_EffectiveDate", "'2020-01-01 01:00:00 +07:00'" },
				{ "GSV_GS_NKReviewer", "'B01'" },
				{ "GSV_Score", "'255'" },
				{ "GSV_Comments", "'Old Comment 202'" },
				{ "GSV_SystemCreateTimeUtc", "'2020-01-01 00:00'" },
				{ "GSV_SystemCreateUser", "'XYZ'" },
				{ "GSV_SystemLastEditTimeUtc", "'2020-01-02 00:00'" },
				{ "GSV_SystemLastEditUser", "'YZX'" },
			};

			var edits = new (string column, string sqlValue, object assertValue)[]
			{
				("GSV_GS_Staff", $"'{staffs[1].PK}'", staffs[1].PK),
				("GSV_EffectiveDate", "'2021-01-01 21:00:00 -01:00'", new DateTimeOffset(new DateTime(2021, 1, 1, 21, 0, 0), new TimeSpan(-1, 0, 0))),
				("GSV_GS_NKReviewer", "'B02'", "B02"),
				("GSV_Score", "'100'", (byte)100),
				("GSV_Comments", "'New Comment ABC'", "New Comment ABC"),
				("GSV_SystemCreateTimeUtc", "'2021-01-01 00:00'", new DateTime(2021, 1, 1)),
				("GSV_SystemCreateUser", "'E'", "E"),
				("GSV_SystemLastEditTimeUtc", "'2021-01-02 00:00'", new DateTime(2021, 1, 2)),
				("GSV_SystemLastEditUser", "'XYZ'", "XYZ"),
			};

			var realColumns = ColumnNames("GlbStaffReview");
			AssertContainsExactElementsInAnyOrder("Please specify an original value to be updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), original.Keys.Except(shouldNeverBeUpdatedDirectly));
			AssertContainsExactElementsInAnyOrder("Please specify an update value, so we can ensure the value is truly updated.", realColumns.Except(shouldNeverBeUpdatedDirectly), edits.Select(e => e.column).Distinct());

			var orderedKeys = original.ToList();
			var insertStatement = $"INSERT INTO hrm.GlbStaffReview ({string.Join(", ", orderedKeys.Select(kvp => kvp.Key))}) VALUES ({string.Join(", ", orderedKeys.Select(kvp => kvp.Value))})";
			TestConnection.ExecuteNonQuery(insertStatement);

			CombineAssertions("After updating a property, the underlying row should be updated. The following properties are not having changes applied.", () =>
			{
				foreach (var edit in edits)
				{
					TestConnection.ExecuteNonQuery($"UPDATE hrm.GlbStaffReview SET {edit.column}={edit.sqlValue} WHERE GSV_PK='{pk}'");

					var value = TestConnection.ExecuteScalar($"SELECT {edit.column} FROM hrm.GlbStaffReview WHERE GSV_PK='{pk}'");
					AssertEquals(edit.column, edit.assertValue, value);
				}
			});
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

		public void TestInsertBothBeforeAndAfterCurrentRow()
		{
			var sql = new SqlQueryBuilder();
			var staff = new GlbStaff("XYZ");

			var previousGSV = new GlbStaffReview(staff.PK, new DateTime(2020, 1, 1), "AC1");
			var middleRow = new GlbStaffReview(staff.PK, new DateTime(2020, 2, 1), "AC2");
			var nextGSV = new GlbStaffReview(staff.PK, new DateTime(2020, 3, 1), "AC3");

			staff.AppendInsertAndReturnObject(sql);
			middleRow.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			sql.Clear();
			previousGSV.AppendInsertAndReturnObject(sql);
			nextGSV.AppendInsertAndReturnObject(sql);

			TestConnection.ExecuteNonQuery(sql.ToStringWithNewLineBetweenAppends());

			GlbStaffReview.AssertFromDB(TestConnection, middleRow.PK)
				.ExpectEquals("The middle row's effective date should be untouched", r => r.GSV_EffectiveDate, middleRow.GSV_EffectiveDate)
				.ExpectEquals("The middle row now has an end date", r => r.GSV_AutoEffectiveEndDate, nextGSV.GSV_EffectiveDate)
				.VerifyAll();

			GlbStaffReview.AssertFromDB(TestConnection, previousGSV.PK)
				.ExpectEquals("The previous row has its end date set to the value in middle row", r => r.GSV_AutoEffectiveEndDate, middleRow.GSV_EffectiveDate)
				.VerifyAll();

			GlbStaffReview.AssertFromDB(TestConnection, nextGSV.PK)
				.ExpectEquals("The next rows end date is null", r => r.GSV_AutoEffectiveEndDate, null)
				.VerifyAll();
		}

		void AssertReviewDateHistory(string message, Guid staff, params DateTimeOffset?[] expectedHistory)
		{
			if (!expectedHistory.OrderBy(h => h).SequenceEqual(expectedHistory))
			{
				Fail("Pass the review dates in ascending order. It's hard enough to read already.");
			}

			var actual = new List<(DateTimeOffset?, DateTimeOffset?)>();
			TestConnection.ExecuteReader($"SELECT GSV_EffectiveDate, GSV_AutoEffectiveEndDate FROM hrm.GlbStaffReview WHERE GSV_GS_Staff = '{staff}' ORDER BY GSV_EffectiveDate ASC", r =>
			{
				var effectiveDate = (DateTimeOffset)r[nameof(GlbStaffReview.GSV_EffectiveDate)];
				var effectiveEndDate = r[nameof(GlbStaffReview.GSV_AutoEffectiveEndDate)];

				actual.Add((effectiveDate, effectiveEndDate == DBNull.Value ? null : (DateTimeOffset?)effectiveEndDate));
			});

			var expected = expectedHistory.Zip(expectedHistory.Skip(1).Append(null), (a, b) => (a, b)).ToList();
			AssertSequencesEqual(message, expected, actual);
		}

		protected override void SetUp()
		{
			var dbName = TestConnection.CurrentDatabase;

			connectionThatCanAccessHrmSchema = Db.NewExtraUnrestrictedWriterConnection(TestConnection.ServerName, dbName);
			base.SetUp();
		}

		protected override void TearDown()
		{
			connectionThatCanAccessHrmSchema?.Dispose();
			connectionThatCanAccessHrmSchema = null;
			base.TearDown();
		}

		protected override DbConnection TestConnection => connectionThatCanAccessHrmSchema ?? base.TestConnection;
		DbConnection connectionThatCanAccessHrmSchema;
	}

	class GlbStaffReview : SQLDataObject<GlbStaffReview>
	{
		public GlbStaffReview(Guid staff, DateTimeOffset effectiveDate, string reviewer = "ABC", byte score = 0, string comments = "No Comments")
		{
			GSV_GS_Staff = staff;
			GSV_EffectiveDate = effectiveDate;

			GSV_GS_NKReviewer = reviewer;
			GSV_Score = score;
			GSV_Comments = comments;

			GSV_SystemCreateTimeUtc = DateTime.UtcNow;
			GSV_SystemLastEditTimeUtc = DateTime.UtcNow;

			GSV_SystemCreateUser = "E";
			GSV_SystemLastEditUser = "E";
		}

		public Guid GSV_GS_Staff { get; }
		public DateTimeOffset GSV_EffectiveDate { get; }
		public DateTimeOffset? GSV_AutoEffectiveEndDate { get; }

		public string GSV_GS_NKReviewer { get; }
		public byte GSV_Score { get; }
		public string GSV_Comments { get; }

		public string GSV_SystemCreateUser { get; }
		public string GSV_SystemLastEditUser { get; }
		public DateTime GSV_SystemCreateTimeUtc { get; }
		public DateTime GSV_SystemLastEditTimeUtc { get; }

		protected override ITableDefinition GetSchema() => base.GetSchema()
			.Extend(nameof(GSV_AutoEffectiveEndDate), new DateTimeOffsetColumnInfo(0, isNullable: true));
	}
}
