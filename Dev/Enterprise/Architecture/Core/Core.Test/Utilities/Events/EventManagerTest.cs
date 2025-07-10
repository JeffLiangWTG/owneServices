using System;
using System.Data;
using System.Text.RegularExpressions;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class EventManagerTest : TransactionedTestCase
	{
		public void TestAddAuditLogEvent()
		{
			string tableName = "AuditLogEventTest";
			Guid parentPK = new Guid("EF008503-87CD-404B-B7D2-4F1883B20D73");
			DateTime eventTime = GetTestCurrentLocalTime();
			EventManager em = new EventManager();

			try
			{
				em.AddAuditLogEvent("ADD", tableName, parentPK, SqlDateTime.MinValue.Value);
				Fail();
			}
			catch (ArgumentException e)
			{
				AssertNotNull("Should have thrown an exception because the event time is invalid", e);
			}

			try
			{
				em.AddAuditLogEvent("ADD", "", parentPK, eventTime);
				Fail();
			}
			catch (ArgumentException e)
			{
				AssertNotNull("Should have thrown an exception because the table name is empty", e);
			}

			em.AddAuditLogEvent("ADD", tableName, parentPK, eventTime, "Ref", "N", "N");
			em.AddAuditLogEvent("EDT", tableName, parentPK, eventTime, "Ref");
			em.AddAuditLogEvent("DEP", tableName, parentPK, eventTime);
			em.AddAuditLogEvent("ARV", tableName, parentPK, eventTime);

			AssertEquals("StmALog Row Count", 4, GetAuditLogRowCount(tableName));
		}

		public void TestGetUserNameWhoLastEditedRecord()
		{
			Guid testPk = Guid.NewGuid();
			DateTime eventTime = GetTestCurrentLocalTime();
			EventManager em = new EventManager();
			em.AddAuditLogEvent("EDT", "RefCommodityCode", testPk, eventTime);

			// UserNameAndTime should be something like "UserFullName @ 18 May 2004 18:34:50"
			string userNameAndTime = em.GetUserNameAndTimeOfLastEditOrDeleteOfARecord("RefCommodityCode", testPk);
			Assert("GetUserLastModifiedRecord", userNameAndTime.StartsWith(EnvProxy.Instance.CurrentUser.FullName + " @ "));
			string matchPattern = EnvProxy.Instance.CurrentUser.FullName + @" @ [0-9]{2} [a-zA-Z]{3} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2}";
			Assert("GetUserLastModifiedRecord", Regex.IsMatch(userNameAndTime, matchPattern));
		}

		public void TestGetUserNameWhoDeletedRecord()
		{
			Guid testPk = Guid.NewGuid();
			DateTime eventTime = GetTestCurrentLocalTime();
			EventManager em = new EventManager();
			em.AddAuditLogEvent("DEL", "RefCommodityCode", testPk, eventTime);

			// UserNameAndTime should be something like "Developer @ 18 May 2004 18:34:50"
			string userNameAndTime = em.GetUserNameAndTimeOfLastEditOrDeleteOfARecord("RefCommodityCode", testPk);
			Assert("GetUserDeletedRecord", userNameAndTime.StartsWith(EnvProxy.Instance.CurrentUser.FullName + " @ "));
			string matchPattern = EnvProxy.Instance.CurrentUser.FullName + @" @ [0-9]{2} [a-zA-Z]{3} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2}";
			Assert("GetUserLastModifiedRecord", Regex.IsMatch(userNameAndTime, matchPattern));
		}

		public void TestGetUserCodeWhoLastModifiedRecord()
		{
			Guid testPk = Guid.NewGuid();
			DateTime eventTime = GetTestCurrentLocalTime();
			EventManager em = new EventManager();
			em.AddAuditLogEvent("EDT", GlbStaffSchema.Constants.TableName, testPk, eventTime);

			string userWhoLastModifiedRecordCode = em.GetUserCodeWhoAddedOrLastEditedRecord(GlbStaffSchema.Constants.TableName, testPk).TrimEnd();
			AssertEquals("Last Update dbo.GlbStaff (Current User)", EnvProxy.Instance.CurrentUser.Initials, userWhoLastModifiedRecordCode);

			Guid newGroupPk = Guid.NewGuid();
			em.AddAuditLogEvent("ADD", GlbGroupSchema.Constants.TableName, newGroupPk, eventTime);

			userWhoLastModifiedRecordCode = em.GetUserCodeWhoAddedOrLastEditedRecord(GlbGroupSchema.Constants.TableName, Guid.NewGuid()).TrimEnd();
			AssertEquals("Inexisting Record - User Who Modified Should be Empty Guid", "", userWhoLastModifiedRecordCode);

			userWhoLastModifiedRecordCode = em.GetUserCodeWhoAddedOrLastEditedRecord(GlbGroupSchema.Constants.TableName, newGroupPk).TrimEnd();
			AssertEquals("Last Insert dbo.GlbGroup", EnvProxy.Instance.CurrentUser.Initials, userWhoLastModifiedRecordCode);
		}

		public void TestGetUserNameAndTimeOfLastEditOrDeleteOfARecordForWrongDateCast()
		{
			Guid orgHeaderPK = Guid.NewGuid();
			string orgHeaderInsertScript = "INSERT INTO " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + " ( " +
									OrgHeaderSchema.PK.Name + ", " +
									OrgHeaderSchema.OH_Code.Name + ", " +
									OrgHeaderSchema.OH_FullName.Name + ", " +
									OrgHeaderSchema.OH_SystemCreateUser.Name + ", " +
									OrgHeaderSchema.OH_SystemCreateTimeUtc.Name + ", " +
									OrgHeaderSchema.OH_SystemLastEditUser.Name + ", " +
									OrgHeaderSchema.OH_SystemLastEditTimeUtc.Name + ") " +
									"VALUES ( @OH_PK, 'NICK', 'NICK_ORG', '~BP', GetUtcDate(), @OH_SystemLastEditUser, @OH_SystemLastEditTimeUtc)";
			DbCommand orgHeaderCmd = Db.Connection.Command(orgHeaderInsertScript);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_PK", orgHeaderPK, OrgHeaderSchema.PK);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_SystemLastEditUser", EnvProxy.Instance.CurrentUser.Initials, OrgHeaderSchema.OH_SystemLastEditUser);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_SystemLastEditTimeUtc", new DateTime(2000, 1, 31, 12, 0, 0, 0), OrgHeaderSchema.OH_SystemLastEditTimeUtc);

			orgHeaderCmd.ExecuteNonQuery();

			EventManager em = new EventManager();
			em.AddAuditLogEvent("EDT", OrgHeaderSchema.Constants.TableName, orgHeaderPK, new DateTime(2008, 1, 31, 13, 0, 0, 0));

			DataRow orgHeaderRow = Utilities.GetDataTableFromQuery(
				"SELECT " + OrgHeaderSchema.PK.Name + ", " +
										OrgHeaderSchema.OH_Code.Name + ", " +
										OrgHeaderSchema.OH_FullName.Name + ", " +
										OrgHeaderSchema.OH_SystemLastEditUser.Name + ", " +
										OrgHeaderSchema.OH_Code.Name + " as OH_SystemLastEditTimeUtc" +
										" FROM " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + " WHERE OH_PK = '" + orgHeaderPK.ToString() + "'").Rows[0];

			AssertNoExceptionThrown(delegate
			{ em.GetUserNameAndTimeOfLastEditOrDeleteOfARecord(orgHeaderRow); });
		}

		[TestUtcOffset(5, 0, 0)]
		public void TestGetUserNameAndTimeOfLastEditOrDeleteOfARecordFromRow()
		{
			Guid orgHeaderPK = Guid.NewGuid();
			string orgHeaderInsertScript = "INSERT INTO " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + " ( " +
									OrgHeaderSchema.PK.Name + ", " +
									OrgHeaderSchema.OH_Code.Name + ", " +
									OrgHeaderSchema.OH_FullName.Name + ", " +
									OrgHeaderSchema.OH_SystemLastEditUser.Name + ", " +
									OrgHeaderSchema.OH_SystemLastEditTimeUtc.Name + ", " +
									OrgHeaderSchema.OH_SystemCreateUser.Name + ", " +
									OrgHeaderSchema.OH_SystemCreateTimeUtc.Name + ") " +
									"VALUES ( @OH_PK, 'SERGEY', 'SERGEY_ORG', @OH_SystemLastEditUser, @OH_SystemLastEditTimeUtc, '~BP', '1999/01/01')";
			DbCommand orgHeaderCmd = Db.Connection.Command(orgHeaderInsertScript);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_PK", orgHeaderPK, OrgHeaderSchema.PK);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_SystemLastEditUser", EnvProxy.Instance.CurrentUser.Initials, OrgHeaderSchema.OH_SystemLastEditUser);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_SystemLastEditTimeUtc", new DateTime(2000, 1, 31, 12, 0, 0, 0), OrgHeaderSchema.OH_SystemLastEditTimeUtc);

			Guid glbStaffPK = Guid.NewGuid();
			string glbStaffInsertScript = "INSERT INTO " + GlbStaffSchema.Constants.SqlSchemaName + "." + GlbStaffSchema.Constants.TableName + " ( " +
									GlbStaffSchema.PK.Name + ", " +
									GlbStaffSchema.GS_Code.Name + ", " +
									GlbStaffSchema.GS_FullName.Name + ") " +
									"VALUES ( " +
									"'" + glbStaffPK.ToString() + "', " +
									"'T.U' , 'Test User')";
			DbCommand glbStaffCmd = Db.Connection.Command(glbStaffInsertScript);

			orgHeaderCmd.ExecuteNonQuery();

			Db.Connection.ExecuteNonQuery(@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_GlbStaff_AuditDetailsAreNotMissing_Insert')
BEGIN
	DISABLE TRIGGER TG_GlbStaff_AuditDetailsAreNotMissing_Insert ON GlbStaff
END");
			glbStaffCmd.ExecuteNonQuery();
			Db.Connection.ExecuteNonQuery(@"
IF EXISTS (SELECT NULL FROM sys.triggers WHERE name = 'TG_GlbStaff_AuditDetailsAreNotMissing_Insert')
BEGIN
	ENABLE TRIGGER TG_GlbStaff_AuditDetailsAreNotMissing_Insert ON GlbStaff
END");

			EventManager em = new EventManager();
			em.AddAuditLogEvent("EDT", OrgHeaderSchema.Constants.TableName, orgHeaderPK, new DateTime(2008, 1, 31, 13, 0, 0, 0));
			em.AddAuditLogEvent("EDT", GlbStaffSchema.Constants.TableName, glbStaffPK, new DateTime(2008, 1, 31, 13, 0, 0, 0));

			DataRow orgHeaderRow = Utilities.GetDataTableFromQuery("SELECT * FROM dbo.OrgHeader WHERE OH_PK = '" + orgHeaderPK.ToString() + "'").Rows[0];
			DataRow glbStaffRow = Utilities.GetDataTableFromQuery("SELECT * FROM dbo.GlbStaff WHERE GS_PK = '" + glbStaffPK.ToString() + "'").Rows[0];

			AssertEquals("As OrgHeader table contains the SystemLastEditTimeUtc and SystemLastEditUser fields the log info should be retutr from them",
				EnvProxy.Instance.CurrentUser.FullName + " @ 31 Jan 2000 17:00:00", em.GetUserNameAndTimeOfLastEditOrDeleteOfARecord(orgHeaderRow));

			string matchPattern = EnvProxy.Instance.CurrentUser.FullName + @" @ [0-9]{2} [a-zA-Z]{3} [0-9]{4} [0-9]{2}:[0-9]{2}:[0-9]{2}";
			Assert("GetUserNameAndTimeOfLastEditOrDeleteOfARecord(table, pk) should get log info from dbo.StmALog", Regex.IsMatch(em.GetUserNameAndTimeOfLastEditOrDeleteOfARecord("OrgHeader", orgHeaderPK), matchPattern));
			AssertNotEquals("GetUserNameAndTimeOfLastEditOrDeleteOfARecord(table, pk) should get log info from dbo.StmALog",
				EnvProxy.Instance.CurrentUser.FullName + " @ 31 Jan 2000 17:00:00", em.GetUserNameAndTimeOfLastEditOrDeleteOfARecord("OrgHeader", orgHeaderPK));

			Assert("As GlbStaff table doesn't contains the SystemLastEditTimeUtc and SystemLastEditUser fields the log info should be retutr from dbo.StmALog", Regex.IsMatch(em.GetUserNameAndTimeOfLastEditOrDeleteOfARecord("GlbStaff", glbStaffPK), matchPattern));
		}

		public void TestGetUserNameAndTimeOfLastEditOrDeleteOfARecordFromDeletedRow()
		{
			Guid orgHeaderPK = Guid.NewGuid();
			string orgHeaderInsertScript = "INSERT INTO " + OrgHeaderSchema.Constants.SqlSchemaName + "." + OrgHeaderSchema.Constants.TableName + " ( " +
									OrgHeaderSchema.PK.Name + ", " +
									OrgHeaderSchema.OH_Code.Name + ", " +
									OrgHeaderSchema.OH_FullName.Name + ", " +
									OrgHeaderSchema.OH_SystemCreateUser.Name + ", " +
									OrgHeaderSchema.OH_SystemCreateTimeUtc.Name + ", " +
									OrgHeaderSchema.OH_SystemLastEditUser.Name + ", " +
									OrgHeaderSchema.OH_SystemLastEditTimeUtc.Name + ") " +
									"VALUES ( @OH_PK, 'SERGEY', 'SERGEY_ORG', '~BP', '1999/01/01', @OH_SystemLastEditUser, @OH_SystemLastEditTimeUtc)";
			DbCommand orgHeaderCmd = Db.Connection.Command(orgHeaderInsertScript);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_PK", orgHeaderPK, OrgHeaderSchema.PK);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_SystemLastEditUser", EnvProxy.Instance.CurrentUser.Initials, OrgHeaderSchema.OH_SystemLastEditUser);
			orgHeaderCmd.AddParameterBasedOnDbColumn("@OH_SystemLastEditTimeUtc", new DateTime(2000, 1, 31, 12, 0, 0, 0), OrgHeaderSchema.OH_SystemLastEditTimeUtc);
			orgHeaderCmd.ExecuteNonQuery();

			DataRow orgHeaderRow = Utilities.GetDataTableFromQuery("SELECT * FROM dbo.OrgHeader WHERE OH_PK = '" + orgHeaderPK.ToString() + "'").Rows[0];
			orgHeaderRow.Delete();

			EventManager em = new EventManager();

			Assert(em.GetUserNameAndTimeOfLastEditOrDeleteOfARecord(orgHeaderRow).StartsWith(EnvProxy.Instance.CurrentUser.FullName));
		}

		public void TestGetBranchAndDepartmentOfUserWhoLastEditedRecord()
		{
			Guid testPk = Guid.NewGuid();
			DateTime eventTime = GetTestCurrentLocalTime();
			EventManager em = new EventManager();
			em.AddAuditLogEvent("EDT", "RefCommodityCode", testPk, eventTime);

			string branch, department;

			GetBranchAndDepartmentFromLastEvent("RefCommodityCode", testPk, out branch, out department);

			AssertEquals("Branch code", EnvProxy.Instance.CurrentBranch.Code, branch);
			AssertEquals("Department code", EnvProxy.Instance.CurrentDepartment.Code, department);
		}

		#region Implementation

		int GetAuditLogRowCount(string tableName)
		{
			string sqlQueryText = @"SELECT count(*) FROM dbo.StmALog WHERE SL_Table = @SL_Table";
			DbCommand command = Db.Connection.Command(sqlQueryText);
			command.AddParameterBasedOnDbColumn("@SL_Table", tableName, StmALogSchema.SL_Table);
			return (int)command.ExecuteScalar();
		}

		DateTime GetTestCurrentLocalTime()
		{
			return DateTime.Now; // this code used for testing only
		}

		void GetBranchAndDepartmentFromLastEvent(string tableName, Guid parentPK, out string branch, out string department)
		{
			branch = department = null;

			var sqlText = string.Format(@"SELECT {0}, {1} FROM {2} WHERE {3} = @SL_Table AND {4} = @SL_Parent ORDER BY {5} DESC",
				/*0*/StmALogSchema.Constants.SL_GB_NKBranch, /*1*/StmALogSchema.Constants.SL_GE_NKDepartment, /*2*/StmALogSchema.Constants.TableName,
				/*3*/StmALogSchema.Constants.SL_Table, /*4*/StmALogSchema.Constants.SL_Parent, /*5*/StmALogSchema.Constants.SL_PostedTimeUtc);

			using (var command = Db.Connection.Command(sqlText))
			{
				command.AddParameterBasedOnDbColumn("@SL_Parent", parentPK, StmALogSchema.SL_Parent);
				command.AddParameterBasedOnDbColumn("@SL_Table", tableName, StmALogSchema.SL_Table);

				using (IDataReader reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						branch = reader[StmALogSchema.Constants.SL_GB_NKBranch].ToString();
						department = reader[StmALogSchema.Constants.SL_GE_NKDepartment].ToString();
					}
				}
			}
		}

		#endregion
	}
}
