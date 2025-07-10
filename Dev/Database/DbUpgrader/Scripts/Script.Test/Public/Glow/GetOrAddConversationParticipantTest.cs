using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(GetOrAddConversationParticipant))]
	class GetOrAddConversationParticipantTest : DbCreateScriptTest
	{
		public void TestGetParticipant_WithNoExistingConversation()
		{
			const string parentTableCode = "INC";
			const string participantTableCode = "OC";
			const string auditUserCode = "ZZ";

			var result1 = Execute(parentTableCode, Guid.NewGuid(), participantTableCode, Guid.NewGuid(), auditUserCode);
			var result2 = Execute(parentTableCode, Guid.NewGuid(), participantTableCode, Guid.NewGuid(), auditUserCode);

			Assert(GetRowCount("dbo.JobConversation") == 2);
			Assert(GetRowCount("dbo.JobConversationParticipant") == 2);

			Assert(result1.ConversationId != result2.ConversationId);
			Assert(result1.ParticipantId != result2.ParticipantId);
		}

		public void TestGetParticipant_WithExistingConversation()
		{
			const string parentTableCode = "INC";
			const string participantTableCode = "OC";
			var parentId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			const string auditUserCode = "ZZ";

			AddConversation(conversationId, parentTableCode, parentId);

			var result1 = Execute(parentTableCode, parentId, participantTableCode, Guid.NewGuid(), auditUserCode);
			var result2 = Execute(parentTableCode, parentId, participantTableCode, Guid.NewGuid(), auditUserCode);

			Assert(GetRowCount("dbo.JobConversation") == 1);
			Assert(GetRowCount("dbo.JobConversationParticipant") == 2);

			Assert(result1.ConversationId == conversationId);
			Assert(result2.ConversationId == conversationId);
			Assert(result1.ParticipantId != result2.ParticipantId);
		}

		public void TestGetParticipant_WithExistingParticipant()
		{
			const string parentTableCode = "INC";
			const string participantTableCode = "OC";
			var parentId = Guid.NewGuid();
			var conversationId = Guid.NewGuid();
			var userPk = Guid.NewGuid();
			const string auditUserCode = "ZZ";

			AddConversation(conversationId, parentTableCode, parentId);
			AddParticipant(Guid.NewGuid(), conversationId, userPk, participantTableCode);

			var result1 = Execute(parentTableCode, parentId, participantTableCode, userPk, auditUserCode);
			var result2 = Execute(parentTableCode, parentId, participantTableCode, userPk, auditUserCode);

			Assert(GetRowCount("dbo.JobConversation") == 1);
			Assert(GetRowCount("dbo.JobConversationParticipant") == 1);

			Assert(result1.ConversationId == conversationId);
			Assert(result2.ConversationId == conversationId);
			Assert(result1.ParticipantId == result2.ParticipantId);
		}

		[UseSnapshotProtection]
		public void TestGetParticipant_ConcurrentParticipants()
		{
			using (var mainConnection = Db.NewExtraConnectionToMainDb())
			{
				const string parentTableCode = "INC";
				const string participantTableCode = "OC";
				var parentId = Guid.NewGuid();
				const string auditUserCode = "ZZ";

				addConversation();

				var userPk = Guid.NewGuid();
				var otherUserPk = Guid.NewGuid();

				using (var insertManager = mainConnection.BeginTransactionWithManager())
				{
					var participantPk = Execute(mainConnection, parentTableCode, parentId, participantTableCode, userPk, auditUserCode).ParticipantId;

					Execute(mainConnection, parentTableCode, parentId, participantTableCode, otherUserPk, auditUserCode);

					Guid result = Guid.NewGuid();
					var loadTask = Task.Run(() =>
					{
						using (Db.DisposableActionForDbConnection())
						using (var selectConnection = Db.NewExtraConnectionToMainDb())
						using (var selectManager = selectConnection.BeginTransactionWithManager())
						{
							result = Execute(selectConnection, parentTableCode, parentId, participantTableCode, userPk, auditUserCode).ParticipantId;
							selectManager.CommitTransaction();
						}
					});

					Task.Delay(10).Wait();
					insertManager.CommitTransaction();
					loadTask.Wait();

					AssertEquals($"should load the participant of the user instead of the other user", result, participantPk);
				}

				void addConversation()
				{
					using (var conversationManager = mainConnection.BeginTransactionWithManager())
					{
						var conversationId = Guid.NewGuid();
						AddConversation(mainConnection, conversationId, parentTableCode, parentId);
						conversationManager.CommitTransaction();
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ClearTable(JobConversationMessageSchema.Constants.TableName);
			ClearTable(JobConversationParticipantSchema.Constants.TableName);
			ClearTable(JobConversationSchema.Constants.TableName);
		}

		static void ClearTable(string tableName)
		{
			var query = string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0}", tableName);
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}

		static (Guid ConversationId, Guid ParticipantId) Execute(string parentTableCode, Guid parentId, string participantTableCode, Guid userPk, string auditUserCode)
		{
			return Execute(Db.Connection, parentTableCode, parentId, participantTableCode, userPk, auditUserCode);
		}

		static (Guid ConversationId, Guid ParticipantId) Execute(DbConnection connection, string parentTableCode, Guid parentId, string participantTableCode, Guid userPk, string auditUserCode)
		{
			using (var command = connection.Command("dbo.GetOrAddConversationParticipant"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@ParentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, parentId);
				command.AddParameter("@ParticipantTableCode", SqlDbType.VarChar, participantTableCode);
				command.AddParameter("@UserPk", SqlDbType.UniqueIdentifier, userPk);
				command.AddParameter("@AuditUserCode", SqlDbType.VarChar, auditUserCode);
				command.AddOutputParameter("@ConversationId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@ParticipantId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.ExecuteNonQuery();

				return ((Guid)command.GetParameterValue("@ConversationId"), (Guid)command.GetParameterValue("@ParticipantId"));
			}
		}

		static int GetRowCount(string tableName)
		{
			var query = FormattableString.Invariant($"SELECT COUNT(*) FROM {tableName}");
			using (var command = Db.Connection.Command(query))
			{
				return (int)command.ExecuteScalar();
			}
		}

		static void AddConversation(Guid conversationId, string parentTableCode, Guid parentId)
		{
			AddConversation(Db.Connection, conversationId, parentTableCode, parentId);
		}

		static void AddConversation(DbConnection connection, Guid conversationId, string parentTableCode, Guid parentId)
		{
			var query = FormattableString.Invariant($@"
INSERT INTO dbo.JobConversation (JCC_PK, JCC_ParentTableCode, JCC_ParentID)
VALUES ('{conversationId}', '{parentTableCode}', '{parentId}')
");
			using (var command = connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}

		static void AddParticipant(Guid participantId, Guid conversationId, Guid userPk, string participantTableCode)
		{
			var query = FormattableString.Invariant($@"
INSERT INTO dbo.JobConversationParticipant (
	JCP_PK, JCP_JCC_Conversation, JCP_IsSubscribed, JCP_ParticipantID, JCP_ParticipantTableCode, JCP_Relation,
	JCP_SystemCreateTimeUtc, JCP_SystemCreateUser, JCP_SystemLastEditTimeUtc, JCP_SystemLastEditUser)
VALUES (
	'{participantId}', '{conversationId}', 1, '{userPk}', '{participantTableCode}', '',
	'2020-01-02 03:04', 'TST', '2020-01-02 03:04', 'TST')
");
			using (var command = Db.Connection.Command(query))
			{
				command.ExecuteNonQuery();
			}
		}
	}
}
