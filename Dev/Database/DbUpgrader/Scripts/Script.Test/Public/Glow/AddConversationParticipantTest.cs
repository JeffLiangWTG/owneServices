using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Scripts.Definitions.Glow;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Glow
{
	[TestedType(typeof(AddConversationParticipant))]
	class AddConversationParticipantTest : DbCreateScriptTest
	{
		public void TestExplicitTransactionProperlyManaged()
		{
			AssertEquals("Script has 'SET XACT_ABORT ON'", true, ScriptToTest.Text.IndexOf("SET XACT_ABORT ON", StringComparison.OrdinalIgnoreCase) >= 0);
		}

		public void TestConcurrentInsert()
		{
			var parentTableCode = "INC";
			var tableCode = "OC";
			var parentId = Guid.NewGuid();
			var staffPk = GetStaffPk();
			var auditUserCode = "ZZ";
			var manualResetEvent = new ManualResetEventSlim(false);

			void AddToConversation(Guid parentIdValue)
			{
				using (Db.DisposableActionForDbConnection())
				{
					{
						AssertNoExceptionThrown(() =>
						{
							manualResetEvent.Wait();
							using (var command = LocalConnection.Command("dbo.AddConversationParticipant"))
							{
								command.CommandTimeout = 10;
								command.CommandType = CommandType.StoredProcedure;
								command.AddParameter("@ParentTableCode", SqlDbType.VarChar, parentTableCode);
								command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, parentIdValue);
								command.AddParameter("@TableCode", SqlDbType.VarChar, tableCode);
								command.AddTableValuedParameter("@UserPk", TVPHelper.TVP_uniqueidentifier, new[] { staffPk });
								command.AddParameter("@AuditUserCode", SqlDbType.VarChar, auditUserCode);
								command.ExecuteNonQuery();
							}
						});
					}
				}
			}

			using (manualResetEvent)
			{
				for (var i = 0; i < 5; i++)
				{
					manualResetEvent.Reset();
					AssertEquals(0, ConversationCount(parentId));
					var tasks = new List<Task>();
					for (var j = 0; j < 100; j++)
					{
						tasks.Add(Task.Run(() => AddToConversation(parentId)));
					}
					manualResetEvent.Set();
					Task.WaitAll(tasks.ToArray());

					AssertEquals(1, ConversationCount(parentId));
					parentId = Guid.NewGuid();
				}
			}
		}

		DbConnection LocalConnection => Db.Connection;

		int ConversationCount(Guid parentId)
		{
			var commandText = @"SELECT COUNT(*) FROM dbo.JobConversationParticipant WHERE JCP_JCC_Conversation IN (SELECT JCC_PK FROM dbo.JobConversation WHERE JCC_ParentID = @ParentId);";

			using (var command = LocalConnection.Command(commandText))
			{
				command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, parentId);

				return (int)command.ExecuteScalar();
			}
		}

		Guid GetStaffPk()
		{
			var commandText = @"SELECT TOP 1 OC_PK FROM dbo.OrgContact;";
			using (var command = LocalConnection.Command(commandText))
			{
				return (Guid)command.ExecuteScalar();
			}
		}
	}
}
