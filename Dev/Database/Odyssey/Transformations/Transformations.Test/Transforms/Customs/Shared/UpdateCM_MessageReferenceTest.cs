using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using CargoWise.Data;
using Enterprise.DbUpgrader.Shared.Testing;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(UpdateCM_MessageReference))]
	sealed class UpdateCM_MessageReferenceTest : DataTransformationTestCase
	{
		readonly Dictionary<Guid, (string MessageReference, string Note)> ShouldNotUpdateRecords = new();
		readonly Dictionary<Guid, (string ExpectedMessageReference, string Note)> ShouldUpdateRecords = new();
		long ExpectedLastId { get; set; }

		protected override void AssertTransformationResults()
		{
			var countSqlText = "SELECT COUNT(*) FROM [dbo].[CusMAWB] WHERE [CM_MessageReference] = '' and [CM_ApplicationCode] not in ('TSW', 'NZE')";
			var lineCount = int.MaxValue;
			Db.Connection.ExecuteReader(countSqlText, (IDataRecord record) => lineCount = record.GetInt32(0));
			AssertEquals("All CusMAWBs should have a CM_MessageReference", 0, lineCount);

			var hasErrorDataForNoUpdateData = false;
			var queryScript = "SELECT [CM_MessageReference] FROM [dbo].[CusMAWB] WHERE [CM_PK] = @DataPk";
			var noUpdateErrorMessage = new StringBuilder(@"Below CusMAWB's CM_MessageReference should not be update:
  |                 GUID                 | Should Be |  Actual   | Test Case Note
");
			foreach (var shouldNotUpdateRecord in ShouldNotUpdateRecords)
			{
				Db.Connection.ExecuteReader(queryScript,
					(DbCommand command) => command.AddParameter("@DataPk", SqlDbType.UniqueIdentifier, shouldNotUpdateRecord.Key),
					(IDataRecord record) =>
				{
					var actual = record.GetString(0);
					if (actual != shouldNotUpdateRecord.Value.MessageReference)
					{
						hasErrorDataForNoUpdateData = true;
						noUpdateErrorMessage.AppendLine($"  | {shouldNotUpdateRecord.Key:D} | {shouldNotUpdateRecord.Value.MessageReference} | {actual} | {shouldNotUpdateRecord.Value.Note}");
					}
				});
			}

			var hasErrorDataForUpdatedData = false;
			var updatedDataErrorMessage = new StringBuilder(@"Below CusMAWB's CM_MessageReference are not expected:
  |                 GUID                 | Should Be |  Actual   | Test Case Note
");
			foreach (var shouldUpdateRecord in ShouldUpdateRecords)
			{
				Db.Connection.ExecuteReader(queryScript,
					(DbCommand command) => command.AddParameter("@DataPk", SqlDbType.UniqueIdentifier, shouldUpdateRecord.Key),
					(IDataRecord record) =>
					{
						var actual = record.GetString(0);
						if (actual != shouldUpdateRecord.Value.ExpectedMessageReference)
						{
							hasErrorDataForUpdatedData = true;
							updatedDataErrorMessage.AppendLine($"  | {shouldUpdateRecord.Key:D} | {shouldUpdateRecord.Value.ExpectedMessageReference} | {actual} | {shouldUpdateRecord.Value.Note}");
						}
					});
			}

			Assert((hasErrorDataForNoUpdateData ? noUpdateErrorMessage.ToString() : string.Empty) + (hasErrorDataForUpdatedData ? updatedDataErrorMessage : string.Empty), !hasErrorDataForNoUpdateData && !hasErrorDataForUpdatedData);

			var nextId = GetNextId("CusMAWBMessageReferenceNumberFountain");
			Assert("Should refresh CusMAWBMessageReferenceNumberFountain at the end of transformation", nextId >= ExpectedLastId);
		}

		protected override void PrepareTestData()
		{
			var script = new StringBuilder();
			script.AppendLine("INSERT INTO [dbo].[CusMAWB] ([CM_PK], [CM_ApplicationCode], [CM_MessageReference], [CM_SystemCreateTimeUtc], [CM_SystemCreateUser], [CM_SystemLastEditTimeUtc],[CM_SystemLastEditUser])");
			var newIdFrom = UpdateCM_MessageReference.MinReferenceValue;
			AppendShouldUpdateRecords("", new DateTime(2024, 1, 1, 6, 0, 0), $"X{newIdFrom + 6:00000000}", "Empty CM_ApplicationCode");
			AppendShouldUpdateRecords("DEF", new DateTime(2024, 1, 1, 5, 0, 0), $"X{newIdFrom + 5:00000000}", "DEF");
			AppendShouldUpdateRecords("CMR", new DateTime(2024, 1, 1, 4, 0, 0), $"X{newIdFrom + 4:00000000}", "CMR");
			AppendShouldUpdateRecords("LEG", new DateTime(2024, 1, 1, 3, 0, 0), $"X{newIdFrom + 3:00000000}", "LEG");
			AppendShouldUpdateRecords("CUK", new DateTime(2024, 1, 1, 2, 0, 0), $"X{newIdFrom + 2:00000000}", "CUK");
			AppendShouldUpdateRecords("CVC", new DateTime(2024, 1, 1, 1, 0, 0), $"X{newIdFrom + 1:00000000}", "CVC");
			AppendShouldUpdateRecords("CVC", null, $"X{newIdFrom:00000000}", "CreateTime is NULL");

			ExpectedLastId = newIdFrom + ShouldUpdateRecords.Count;

			AppendShouldNotUpdateRecords("TSW", "", "NZ.TSW, with empty CM_MessageReference");
			AppendShouldNotUpdateRecords("NZE", "", "NZ.NZE, with empty CM_MessageReference");
			AppendShouldNotUpdateRecords("TSW", "X00001000", "NZ.TSW");
			AppendShouldNotUpdateRecords("NZE", "X00001001", "NZ.NZE");

			void AppendShouldUpdateRecords(string applicationCode, DateTime? createTime, string expectedMessageReference, string note)
			{
				var guid = Guid.NewGuid();
				ShouldUpdateRecords.Add(guid, (expectedMessageReference, note));
				var timeString = createTime == null ? "NULL" : $"'{createTime.Value:yyyy-MM-dd HH:mm:ss}'";
				if (ShouldUpdateRecords.Count == 1)
				{
					script.AppendLine($"VALUES ('{guid:D}', '{applicationCode}', '', {timeString}, 'TST', {timeString}, 'TST')");
				}
				else
				{
					script.AppendLine($"      ,('{guid:D}', '{applicationCode}', '', {timeString}, 'TST', {timeString}, 'TST')");
				}
			}

			void AppendShouldNotUpdateRecords(string applicationCode, string messageReference, string note)
			{
				var guid = Guid.NewGuid();
				ShouldNotUpdateRecords.Add(guid, (messageReference, note));
				script.AppendLine($"      ,('{guid:D}', '{applicationCode}', '{messageReference}', GetUtcDate(), 'TST', GetUtcDate(), 'TST')");
			}
			var queryScript = script.ToString();
			SuspendAuditTriggers();
			Db.Connection.ExecuteNonQuery(queryScript);
		}

		void SuspendAuditTriggers()
		{
			// This code is based on DBConnection.AuditTriggerSuspension.ExecuteSetSessionContext
			var connection = TestConnection as IDbConnectionInternals;
			using var command = connection.InternalDbConnection.CreateCommand();
			command.CommandType = CommandType.StoredProcedure;
			command.CommandText = "sys.sp_set_session_context";
			command.Transaction = connection.InternalDbTransaction;

			var auditGuardTriggerName = "Suspend_System_Audit_Columns_Guard";
			var sessionState = "ALL";

			AddParameter("@Key", DbType.String, 128, auditGuardTriggerName);
			AddParameter("@Value", DbType.Object, 8016, sessionState);

			_ = command.ExecuteNonQuery();

			void AddParameter(string name, DbType type, int size, object value)
			{
				var param = command.CreateParameter();
				param.ParameterName = name;
				param.DbType = type;
				param.Size = size;
				param.Value = value;

				_ = command.Parameters.Add(param);
			}
		}

		long GetNextId(string fountainName)
		{
			var getNextIdScript = $"select [SN_Value] from [dbo].[StmNums] WHERE [SN_Name] = '{fountainName}'";
			return Db.Connection.ExecuteScalar<long>(getNextIdScript);
		}

		protected override DataTransformation GetNewTestTransformationInstance()
		{
			var manager = new UpgradeManagerForTestWithOutputBuffer();
			var transform = new UpdateCM_MessageReference();
			transform.Initialise(null, manager);
			return transform;
		}
	}
}
