using System.Data;
using System.Linq;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.LogWalker;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Core.LogWalker.Testing
{
	[TestedType(typeof(GetSubscribersWithQueuedLogs))]
	class GetSubscribersWithQueuedLogsTest : DbCreateScriptTest
	{
		public void TestGetSubscribersWithQueuedLogsOutput()
		{
			var insertJobQueueData = $@"
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST1', 'QUE', NEWID(), GETUTCDATE(), GETDATE(), GETUTCDATE(), NEWID(), 'JS');
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST2', 'QUE', NEWID(), GETUTCDATE(), GETDATE(), GETUTCDATE(), NEWID(), 'JS');
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST3', 'QUE', NEWID(), GETUTCDATE(), GETDATE(), GETUTCDATE(), NEWID(), 'JS');
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST4', 'PRS', NEWID(), GETUTCDATE(), GETDATE(), GETUTCDATE(), NEWID(), 'JS');
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST1', 'QUE', NEWID(), GETUTCDATE(), GETDATE(), GETUTCDATE(), NEWID(), 'JS');
insert into StmJobQueue (SJ_PK, SJ_FilterName, SJ_Status, SJ_ALogReference, SJ_PostedTimeUtc, SJ_EventTime, SJ_EventTimeUtc, SJ_ParentID, SJ_ParentTableCode) values (NEWID(), 'TEST1', 'FAI', NEWID(), GETUTCDATE(), GETDATE(), GETUTCDATE(), NEWID(), 'JS');";

			AssertNoExceptionThrown(() => TestConnection.ExecuteNonQuery(insertJobQueueData));

			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				using (var command = TestConnection.Command("GetSubscribersWithQueuedLogs"))
				{
					command.CommandType = CommandType.StoredProcedure;
					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							var queryResult = reader.GetString(0);
							AssertEquals("TEST1|TEST2|TEST3", queryResult);
						}
					}
				}
				
				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans.First().Item2;
				foreach (var plan in queryPlans)
				{
					var queryPlanAnalyzer = new QueryPlanalyzer(plan);
					Assert("There should be no table scans.", !queryPlanAnalyzer.TableScans.Any());
					Assert("There should be no index scans.", !queryPlanAnalyzer.IndexScans.Any());
				}
			}
		}
	}
}
