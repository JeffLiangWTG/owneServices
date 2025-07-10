using System;
using System.Data;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using CargoWise.Definitions;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow.Testing.EndAuthenticatedSessionTesting
{
	[TestedType(typeof(EndAuthenticatedSession))]
	internal sealed class CreateScriptTest : DbCreateScriptTest
	{
	}

	internal sealed class ConcurrencyTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestIsConcurrencySafe()
		{
			Helper.SetUpTestData();

			var startingUtcDateTime = DateTime.UtcNow;

			const int parallelism = 20;
			var options = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
			Parallel.For(0, parallelism, options, i =>
			{
				using (var connection = Db.NewExtraConnectionToMainDb())
				using (var command = connection.Command("EndAuthenticatedSession"))
				{
					command.CommandType = CommandType.StoredProcedure;

					command.AddParameter("@HeartBeat", SqlDbType.UniqueIdentifier, Helper.HeartbeatPKForStaff);
					command.AddParameter("@Logreference", SqlDbType.VarChar, Helper.RemoteEndPointAddress);
					command.AddParameter("@SL_DataSource", SqlDbType.Char, StmALogConstants.DataSource.Glow);

					command.ExecuteNonQuery();
				}
			});

			var numLogEntriesCreated = Helper.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [{0}] WHERE SL_PostedTimeUtc > '{1}'", StmALogSchema.Constants.TableName, startingUtcDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
			AssertEquals("When attempting to end the same session concurrently, only one log entry should be created", 1, numLogEntriesCreated);
		}
	}

	internal sealed class ScriptTest : TransactionedTestCase
	{
		public void TestDeletesAssociatedSemaphores()
		{
			Helper.RunScript(Helper.HeartbeatPKForStaff);
			var semaphores = Helper.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [{0}] WHERE SS_SV = '{1}'", StmServiceSemaphoreSchema.Constants.TableName, Helper.HeartbeatPKForStaff);
			AssertEquals("Should have deleted the semaphores associated with the deleted heartbeat", 0, semaphores);
		}

		public void TestDoesNotDeleteUnassociatedSemaphores()
		{
			var otherSemaphoresBefore = Helper.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [{0}] WHERE SS_SV != '{1}'", StmServiceSemaphoreSchema.Constants.TableName, Helper.HeartbeatPKForStaff);
			Helper.RunScript(Helper.HeartbeatPKForStaff);
			var otherSemaphoresAfter = Helper.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [{0}] WHERE SS_SV != '{1}'", StmServiceSemaphoreSchema.Constants.TableName, Helper.HeartbeatPKForStaff);

			AssertEquals("Should not have deleted the semaphores that are not associated with the deleted heartbeat", otherSemaphoresBefore, otherSemaphoresAfter);
		}

		public void TestDeletesHeartbeat()
		{
			Helper.RunScript(Helper.HeartbeatPKForStaff);
			var semaphores = Helper.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [{0}] WHERE SV_PK = '{1}'", StmServiceHeartBeatSchema.Constants.TableName, Helper.HeartbeatPKForStaff);
			AssertEquals("Should have deleted the heartbeat", 0, semaphores);
		}

		public void TestDoesNotDeleteOtherHeartbeats()
		{
			var otherHeartbeatsBefore = Helper.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [{0}] WHERE SV_PK != '{1}'", StmServiceHeartBeatSchema.Constants.TableName, Helper.HeartbeatPKForStaff);
			Helper.RunScript(Helper.HeartbeatPKForStaff);
			var otherHeartbeatsAfter = Helper.ExecuteSqlScalar<int>("SELECT COUNT(*) FROM [{0}] WHERE SV_PK != '{1}'", StmServiceHeartBeatSchema.Constants.TableName, Helper.HeartbeatPKForStaff);

			AssertEquals("Should not have deleted other heartbeats", otherHeartbeatsBefore, otherHeartbeatsAfter);
		}

		public void TestCreatesLogEventForStaffUser()
		{
			Helper.RunScript(Helper.HeartbeatPKForStaff);

			using (var command = Db.Connection.Command(string.Format("SELECT TOP 1 * FROM [{0}] ORDER BY SL_PostedTimeUtc DESC", StmALogSchema.Constants.TableName)))
			using (var reader = command.ExecuteReader())
			{
				Assert("reader should have data", reader.Read());

				var postedTime = (DateTime)reader[StmALogSchema.Constants.SL_PostedTimeUtc];
				AssertDateTimeWithinOneSecond("Posted Time should be UTC now", DateTime.UtcNow, postedTime);

				var eventTime = (DateTime)reader[StmALogSchema.Constants.SL_EventTime];
				AssertDateTimeWithinOneSecond("Event Time should be now", DateTime.Now, eventTime);

				var eventCode = (string)reader[StmALogSchema.Constants.SL_SE_NKEvent];
				AssertEquals("Event Code should be 'LGO' (Logged Out)", "LGO", eventCode);

				var reference = (string)reader[StmALogSchema.Constants.SL_Reference];
				AssertEquals("Reference should be remote endpoint", Helper.RemoteEndPointAddress, reference);

				var table = (string)reader[StmALogSchema.Constants.SL_Table];
				AssertEquals("Log entry should be attached to GlbStaff", GlbStaffSchema.Constants.TableName, table);

				var parent = (Guid)reader[StmALogSchema.Constants.SL_Parent];
				AssertEquals("Log entry should be attached to GlbStaff", Helper.StaffPK, parent);

				var userCode = (string)reader[StmALogSchema.Constants.SL_GS_NKUser];
				AssertEquals("Log entry should be reference user code", "A--", userCode);

				var dataSource = (string)reader["SL_DataSource"];
				AssertEquals("Log entry data source should be set by parameter @SL_DataSource", StmALogConstants.DataSource.Glow, dataSource);
			}
		}

		public void TestCreatesLogEventForContactUser()
		{
			Helper.RunScript(Helper.HeartbeatPKForContact);

			using (var command = Db.Connection.Command(string.Format("SELECT TOP 1 * FROM [{0}] ORDER BY SL_PostedTimeUtc DESC", StmALogSchema.Constants.TableName)))
			using (var reader = command.ExecuteReader())
			{
				Assert("reader should have data", reader.Read());

				var postedTime = (DateTime)reader[StmALogSchema.Constants.SL_PostedTimeUtc];
				AssertDateTimeWithinOneSecond("Posted Time should be UTC now", DateTime.UtcNow, postedTime);

				var eventTime = (DateTime)reader[StmALogSchema.Constants.SL_EventTime];
				AssertDateTimeWithinOneSecond("Event Time should be now", DateTime.Now, eventTime);

				var eventCode = (string)reader[StmALogSchema.Constants.SL_SE_NKEvent];
				AssertEquals("Event Code should be 'LGO' (Logged Out)", "LGO", eventCode);

				var reference = (string)reader[StmALogSchema.Constants.SL_Reference];
				AssertEquals("Reference should be remote endpoint", Helper.RemoteEndPointAddress, reference);

				var table = (string)reader[StmALogSchema.Constants.SL_Table];
				AssertEquals("Log entry should be attached to contact", OrgContactSchema.Constants.TableName, table);

				var parent = (Guid)reader[StmALogSchema.Constants.SL_Parent];
				AssertEquals("Log entry should be attached to contact", Helper.ContactPK, parent);

				var userCode = (string)reader[StmALogSchema.Constants.SL_GS_NKUser];
				Assert("Contact user log should not have staff code", string.IsNullOrWhiteSpace(userCode));

				var dataSource = (string)reader["SL_DataSource"];
				AssertEquals("Log entry data source should be set by parameter @SL_DataSource", StmALogConstants.DataSource.Glow, dataSource);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper.SetUpTestData();
		}
	}

	static class Helper
	{
		public static void SetUpTestData()
		{
			ExecuteSql("DELETE [{0}]", StmServiceSemaphoreSchema.Constants.TableName);
			ExecuteSql("DELETE [{0}]", StmServiceHeartBeatSchema.Constants.TableName);
			ExecuteSql(
				"INSERT INTO [{0}] (SV_PK, SV_ParentId, SV_ParentTableCode, SV_ExpiresAtUtc) VALUES ('{1}', '{2}', 'GS', DateAdd(hour, 1, GetUtcDate()))",
				StmServiceHeartBeatSchema.Constants.TableName,
				HeartbeatPKForStaff,
				StaffPK);

			ExecuteSql(
				"INSERT INTO [{0}] (SV_PK, SV_ParentId, SV_ParentTableCode, SV_ExpiresAtUtc) VALUES ('{1}', '{2}', 'OC', DateAdd(hour, 1, GetUtcDate()))",
				StmServiceHeartBeatSchema.Constants.TableName,
				HeartbeatPKForContact,
				ContactPK);

			var perPk = Guid.NewGuid();
			ExecuteSql(
				"INSERT INTO [{0}] (PER_PK, PER_FullName, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser) VALUES ('{1}', 'name', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				GlbPersonSchema.Constants.TableName,
				perPk);

			ExecuteSql(
				"INSERT INTO [{0}] (GS_PK, GS_Code, GS_LoginName, GS_PER, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES ('{1}', 'A--', 'unittestuser1', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				GlbStaffSchema.Constants.TableName,
				StaffPK,
				perPk);

			ExecuteSql(
				"INSERT INTO [{0}] (OH_PK, OH_Code, OH_SystemCreateTimeUtc, OH_SystemCreateUser, OH_SystemLastEditTimeUtc, OH_SystemLastEditUser) VALUES ('{1}', 'EASUTEST', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				OrgHeaderSchema.Constants.TableName,
				OrganizationPK);

			ExecuteSql(
				"INSERT INTO [{0}] (OC_PK, OC_ContactName, OC_OH, OC_SystemCreateTimeUtc, OC_SystemCreateUser, OC_SystemLastEditTimeUtc, OC_SystemLastEditUser) VALUES ('{1}', 'Bob', '{2}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')",
				OrgContactSchema.Constants.TableName,
				ContactPK,
				OrganizationPK);

			CreateSemaphore(HeartbeatPKForStaff, string.Empty);
			CreateSemaphore(HeartbeatPKForStaff, "SomeSemaphore");
			CreateSemaphore(HeartbeatPKForContact, string.Empty);
			CreateSemaphore(HeartbeatPKForContact, "SomeSemaphore");
		}

		public static Guid HeartbeatPKForStaff
		{
			get { return new Guid("805C32DA-C24B-41BF-B19F-AD7D53C6A114"); }
		}

		public static Guid HeartbeatPKForContact
		{
			get { return new Guid("29E5CC7F-E1BB-44A9-AB30-504FAC1F88D7"); }
		}

		public static Guid StaffPK
		{
			get { return new Guid("AAB46B01-3CB9-484C-88E9-74F02C80A040"); }
		}

		public static Guid OrganizationPK
		{
			get { return new Guid("8D6BAA2A-4DFD-4232-84F5-EE1B7B58D9A0"); }
		}

		public static Guid ContactPK
		{
			get { return new Guid("3D217762-C08D-4782-8FDB-516C07A91B24"); }
		}

		public static string RemoteEndPointAddress
		{
			get { return "127.0.0.1"; }
		}

		public static void CreateSemaphore(Guid parentHeartBeat, string lockInfo)
		{
			ExecuteSql(
				"INSERT INTO [{0}] (SS_PK, SS_LockInfo, SS_SV) VALUES (NEWID(), '{1}' ,'{2}')",
				StmServiceSemaphoreSchema.Constants.TableName,
				lockInfo,
				parentHeartBeat);
		}

		public static void ExecuteSql(string format, params object[] args)
		{
			var sqlText = string.Format(format, args);
			using (var command = Db.Connection.Command(sqlText))
			{
				command.ExecuteNonQuery();
			}
		}

		public static void RunScript(Guid heartbeat)
		{
			RunScript(Db.Connection, heartbeat);
		}

		public static void RunScript(DbConnection connection, Guid heartbeat)
		{
			using (var command = connection.Command("EndAuthenticatedSession"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@HeartBeat", SqlDbType.UniqueIdentifier, heartbeat);
				command.AddParameter("@Logreference", SqlDbType.VarChar, Helper.RemoteEndPointAddress);
				command.AddParameter("@SL_DataSource", SqlDbType.Char, StmALogConstants.DataSource.Glow);

				command.ExecuteNonQuery();
			}
		}

		public static T ExecuteSqlScalar<T>(string format, params object[] args)
		{
			var sqlText = string.Format(format, args);
			using (var command = Db.Connection.Command(sqlText))
			{
				return (T)command.ExecuteScalar();
			}
		}
	}
}

