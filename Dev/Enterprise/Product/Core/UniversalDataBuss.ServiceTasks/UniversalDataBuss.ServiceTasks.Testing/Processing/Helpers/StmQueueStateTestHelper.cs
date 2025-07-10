using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	public class StmQueueStateTestHelper : TestCase
	{
		public static void InsertRowIntoDatabase(string taskCode, string status, Guid? pK = null,
			Guid? parentId = null, string parentTableCode = null, string keys = null,
			DateTime? processedTimeUtc = null, DateTime? createTimeUtc = null,
			string createUser = null, Guid? chainId = null, string parentMessageNumber = null,
			DateTime? lastEditTimeUtc = null, string lastEditUser = null)
		{
			var sqlText = $@"
			INSERT INTO dbo.StmQueueState
				({StmQueueStateSchema.Constants.PK}, {StmQueueStateSchema.Constants.SQS_ServiceTaskCode},
				{StmQueueStateSchema.Constants.SQS_ParentID}, {StmQueueStateSchema.Constants.SQS_ParentTableCode},
				{StmQueueStateSchema.Constants.SQS_Keys}, {StmQueueStateSchema.Constants.SQS_Status},
				{StmQueueStateSchema.Constants.SQS_ProcessedTimeUtc}, {StmQueueStateSchema.Constants.SQS_SystemCreateTimeUtc},
				{StmQueueStateSchema.Constants.SQS_SystemCreateUser}, {StmQueueStateSchema.Constants.SQS_ChainID},
				{StmQueueStateSchema.Constants.SQS_ParentMessageNumber}, {StmQueueStateSchema.Constants.SQS_SystemLastEditTimeUtc},
				{StmQueueStateSchema.Constants.SQS_SystemLastEditUser})
				VALUES (@{StmQueueStateSchema.Constants.PK}, @{StmQueueStateSchema.Constants.SQS_ServiceTaskCode},
				@{StmQueueStateSchema.Constants.SQS_ParentID}, @{StmQueueStateSchema.Constants.SQS_ParentTableCode},
				@{StmQueueStateSchema.Constants.SQS_Keys}, @{StmQueueStateSchema.Constants.SQS_Status},
				@{StmQueueStateSchema.Constants.SQS_ProcessedTimeUtc}, @{StmQueueStateSchema.Constants.SQS_SystemCreateTimeUtc},
				@{StmQueueStateSchema.Constants.SQS_SystemCreateUser}, @{StmQueueStateSchema.Constants.SQS_ChainID},
				@{StmQueueStateSchema.Constants.SQS_ParentMessageNumber}, @{StmQueueStateSchema.Constants.SQS_SystemLastEditTimeUtc},
				@{StmQueueStateSchema.Constants.SQS_SystemLastEditUser})";

			var now = ZDateTime.UtcNow.ToDateTime();
			string currentUser = GlbStaff.CurrentUser.GS_Code;
			var randomString = "zzz";
			var dbCommand = Db.Connection.Command(sqlText);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.PK}", SqlDbType.UniqueIdentifier, pK ?? Guid.NewGuid());
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_ServiceTaskCode}", SqlDbType.Char, taskCode);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_ParentID}", SqlDbType.UniqueIdentifier, parentId ?? Guid.NewGuid());
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_ParentTableCode}", SqlDbType.NVarChar, parentTableCode ?? "Z0");
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_Keys}", SqlDbType.NVarChar, keys ?? randomString);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_Status}", SqlDbType.Char, status);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_ProcessedTimeUtc}", SqlDbType.DateTime, processedTimeUtc ?? now);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_SystemCreateTimeUtc}", SqlDbType.DateTime, createTimeUtc ?? now);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_SystemCreateUser}", SqlDbType.VarChar, createUser ?? currentUser);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_ChainID}", SqlDbType.UniqueIdentifier, chainId ?? Guid.NewGuid());
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_ParentMessageNumber}", SqlDbType.VarChar, parentMessageNumber ?? randomString);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_SystemLastEditTimeUtc}", SqlDbType.SmallDateTime, lastEditTimeUtc ?? now);
			dbCommand.AddParameter($"@{StmQueueStateSchema.Constants.SQS_SystemLastEditUser}", SqlDbType.VarChar, lastEditUser ?? currentUser);
			dbCommand.ExecuteNonQuery();
		}
	}
}
