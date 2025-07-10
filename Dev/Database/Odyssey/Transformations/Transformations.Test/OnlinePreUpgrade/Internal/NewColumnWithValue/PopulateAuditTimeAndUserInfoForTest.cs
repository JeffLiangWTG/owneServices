using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Transformations.PreUpgrade;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing
{
	[UseSnapshotProtection]
	class PopulateAuditTimeAndUserInfoForTest : IPopulateAuditTimeAndUserInfo
	{
		public PopulateAuditTimeAndUserInfoForTest(ITableSchema targetTableSchema)
		{
			this.targetTableSchema = targetTableSchema;
		}

		readonly ITableSchema targetTableSchema;

		#region IPopulateAuditTimeAndUserInfo Members

		ITableSchema IPopulateAuditTimeAndUserInfo.TableSchema
		{
			get { return targetTableSchema; }
		}

		public SchemaDateTimeColumn CreateTime
		{
			get { return new SchemaDateTimeColumn(targetTableSchema, Schema.CreateTimeColumn, 1, SqlDbType.SmallDateTime, null, true); }
		}

		public SchemaStringColumn CreateUser
		{
			get { return new SchemaStringColumn(targetTableSchema, Schema.CreateUserColumn, 1, SqlDbType.VarChar, "", false, 3); }
		}

		public SchemaDateTimeColumn LastEditTime
		{
			get { return new SchemaDateTimeColumn(targetTableSchema, Schema.LastEditTimeColumn, 1, SqlDbType.SmallDateTime, null, true); }
		}

		public SchemaStringColumn LastEditUser
		{
			get { return new SchemaStringColumn(targetTableSchema, Schema.LastEditUserColumn, 1, SqlDbType.VarChar, "", false, 3); }
		}

		#endregion

		public static class Schema
		{
			public const string CreateTimeColumn = "TST_SystemCreateTime";
			public const string CreateUserColumn = "TST_SystemCreateUser";
			public const string LastEditTimeColumn = "TST_SystemLastEditTime";
			public const string LastEditUserColumn = "TST_SystemLastEditUser";
		}
	}

	class ColumnCreatorPopulateAuditTimeAndUserTest : CreateAndPopulateColumnsTest
	{
		protected override IEnumerable<string> NewColumnList
		{
			get
			{
				return new string[]
				{
					$"{PopulateAuditTimeAndUserInfoForTest.Schema.CreateTimeColumn} smalldatetime NULL",
					$"{PopulateAuditTimeAndUserInfoForTest.Schema.CreateUserColumn} varchar(3) NOT NULL DEFAULT ('')",
					$"{PopulateAuditTimeAndUserInfoForTest.Schema.LastEditTimeColumn} smalldatetime NULL",
					$"{PopulateAuditTimeAndUserInfoForTest.Schema.LastEditUserColumn} varchar(3) NOT NULL DEFAULT ('')",
				};
			}
		}

		protected override ColumnCreatorDelegateForTest GetColumnCreatorDelegateForTest()
		{
			return (manager, columnMetadata, dbBeingUpgraded) => new ColumnCreatorPopulateAuditTimeAndUser(manager, columnMetadata, dbBeingUpgraded, new PopulateAuditTimeAndUserInfoForTest(base.TargetTableSchema));
		}

		Guid PK_1 = Guid.NewGuid();
		Guid PK_2 = Guid.NewGuid();
		Guid PK_3 = Guid.NewGuid();

		protected override void PrerequisiteSetup()
		{
			base.PrerequisiteSetup();

			var pk_1 = PK_1.ToString();
			var pk_2 = PK_2.ToString();
			var pk_3 = PK_3.ToString();
			var sql = String.Format(@"
INSERT [dbo].[{0}] ([{1}]) VALUES
	('{2}'),
	('{3}'),
	('{4}');
"
				, base.TargetTable                // 0
				, base.PK.First().Split().First() // 1
				, pk_1                            // 2
				, pk_2                            // 3
				, pk_3                            // 4
				);

			Db.Connection.ExecuteNonQuery(sql);

			sql = String.Format(@"
INSERT [dbo].[{0}] ({1}, {2}, {3}, {4}, {5}, {6}, {7}) VALUES
	(NEWID(), '{8}', '{9}' , '2004-01-01 00:01', '2004-01-02 00:01', 'XYZ', 'ADD'),
	(NEWID(), '{8}', '{9}' , '2004-01-01 00:02', '2004-01-01 00:02', 'ABC', 'AAA'),
	(NEWID(), '{8}', '{10}', '2004-01-01 00:03', '2004-01-02 00:03', 'DEF', 'EDT'),
	(NEWID(), '{8}', '{11}', '2004-01-01 00:04', '2004-01-02 00:04', 'U_3', 'EDT'),
	(NEWID(), '{8}', '{11}', '2004-01-01 00:05', '2004-01-02 00:05', 'U_1', 'EDT'),
	(NEWID(), '{8}', '{11}', '2004-01-01 00:06', '2004-01-02 00:06', 'U_4', 'EDT'),
	(NEWID(), '{8}', '{11}', '2004-01-01 00:07', '2004-01-02 00:07', 'U_2', 'EDT');
"
				, StmALogSchema.Constants.TableName        // 0
				, StmALogSchema.Constants.PK               // 1
				, StmALogSchema.Constants.SL_Table         // 2
				, StmALogSchema.Constants.SL_Parent        // 3
				, StmALogSchema.Constants.SL_PostedTimeUtc // 4
				, StmALogSchema.Constants.SL_EventTime     // 5
				, StmALogSchema.Constants.SL_GS_NKUser     // 6
				, StmALogSchema.Constants.SL_SE_NKEvent    // 7
				, TargetTable                              // 8
				, pk_1                                     // 9
				, pk_2                                     // 10
				, pk_3                                     // 11
				);

			Db.Connection.ExecuteNonQuery(sql);
		}

		[UseSnapshotProtection]
		public override void TestCreateAndPopulateColumns()
		{
			PrerequisiteSetup();

			AssertEquals("Precondition. New columns dont exist", false, NewColumnsExist());

			PreSynchroniser.AddAndPopulateAuditAndNaturalKeyColumns();
			AssertEquals("New columns should exist", true, NewColumnsExist());
			AssertContainsExactElementsInAnyOrder("New columns created as declared", NewColumnList, GetColumnsDeclaration());
			AssertEquals("All columns should be populated", 0, GetEmptyValueCount());

			PreSynchroniser.AddAndPopulateAuditAndNaturalKeyColumns();
			AssertEquals("[After ResumePopulate] Blank value count", 0, GetEmptyValueCount());

			AssertPopulatedColumns(expectedPk: PK_3,
				expectedCreateTime: new DateTime(2004, 01, 01, 00, 04, 00), expectedCreateUser: "U_3",
				expectedEditTime: new DateTime(2004, 01, 01, 00, 07, 00), expectedEditUser: "U_2");
		}

		protected override void AssertHighWatermark(string highWatermark)
		{
			AssertEquals(
				true,
				DateTime.TryParseExact(
					highWatermark,
					"s",
					DateTimeFormatInfo.InvariantInfo,
					DateTimeStyles.None,
					out var dateTime));
			AssertCloseEnough(DateTime.UtcNow, dateTime, 3);
		}

		void AssertPopulatedColumns(Guid expectedPk, DateTime expectedCreateTime, string expectedCreateUser, DateTime expectedEditTime, string expectedEditUser)
		{
			var sql = String.Format($@"
SELECT
	{PopulateAuditTimeAndUserInfoForTest.Schema.CreateTimeColumn},
	{PopulateAuditTimeAndUserInfoForTest.Schema.CreateUserColumn},
	{PopulateAuditTimeAndUserInfoForTest.Schema.LastEditTimeColumn},
	{PopulateAuditTimeAndUserInfoForTest.Schema.LastEditUserColumn}
FROM
	[dbo].[{{0}}]
WHERE
	{{1}} = '{{2}}'
;",
				TargetTable,                     // 0
				base.PK.First().Split().First(), // 1
				expectedPk.ToString()            // 2
				);

			using (var cmd = Db.Connection.Command(sql))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					CombineAssertions(() =>
					{
						AssertEquals("CreateTime:", expectedCreateTime, reader.GetDateTime(0));
						AssertEquals("CreateUser:", expectedCreateUser, reader.GetString(1));
						AssertEquals("LastEditTime:", expectedEditTime, reader.GetDateTime(2));
						AssertEquals("LastEditUser:", expectedEditUser, reader.GetString(3));
					});
				}
			}
		}
	}
}
