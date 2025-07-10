using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Server.JobPrinting
{
	sealed class PrintServerTest : TestCase
	{
		// This unit test is used for confirming that transaction.Rollback() is automatically called at the end
		// of block of using (var transaction = connection.BeginTransaction())
		public void TestCallRollbackWhenDisposingSqlTransaction()
		{
			Db.Connection.ExecuteNonQuery(@"IF OBJECT_ID ('TestingTable', 'U') IS NOT NULL DROP TABLE TestingTable");

			bool hasException = false;

			try
			{
				using (var connection = Db.NewExtraConnectionToMainDb())
				{
					connection.RunTransactioned(() =>
					{
						var sqlText = @"
Create Table TestingTable
(
	MyKey int PRIMARY KEY,
	MyValue char(10)
);

INSERT INTO TestingTable
VALUES (100, N'A');

INSERT INTO TestingTable
VALUES (100, N'B');

INSERT INTO TestingTable
VALUES (300, N'B');
";
						using (var command = connection.Command(sqlText))
						{
							command.ExecuteNonQuery();
						}
					}
					);
				}
			}
			catch (Exception)
			{
				hasException = true;
				int count = (int)Db.Connection.ExecuteScalar("SELECT COUNT(*) FROM information_schema.tables WHERE table_name = 'TestingTable'");
				AssertEquals("The transaction should be rolled back. No table is created.", 0, count);
			}

			Assert(hasException);
		}

		[UseSnapshotProtection]
		public void TestResetRetryAttemptsOnSuccess()
		{
			var printJobPK1 = Guid.NewGuid();
			var printJobPK2 = Guid.NewGuid();
			var printJobPK3 = Guid.NewGuid();
			var printJobPK4 = Guid.NewGuid();

			var insertNewStmPrintJobSql = $@"
INSERT INTO dbo.StmPrintJob (SP_PK, SP_JobType, SP_RetryAttempts) VALUES ('{printJobPK1}', 'EML', 3);
INSERT INTO dbo.StmPrintJob (SP_PK, SP_JobType, SP_RetryAttempts) VALUES ('{printJobPK2}', 'EML', 4);
INSERT INTO dbo.StmPrintJob (SP_PK, SP_JobType, SP_RetryAttempts) VALUES ('{printJobPK3}', 'EML', 1);
INSERT INTO dbo.StmPrintJob (SP_PK, SP_JobType, SP_RetryAttempts) VALUES ('{printJobPK4}', 'EML', 0);";

			Db.Connection.ExecuteNonQuery(insertNewStmPrintJobSql);

			var printServer = new PrintServer();
			printServer.SetPrintJobSuccess(new List<Guid> { printJobPK1, printJobPK2, printJobPK3, printJobPK4 });

			AssertRetryAttempts(printJobPK1, 0);
			AssertRetryAttempts(printJobPK2, 0);
			AssertRetryAttempts(printJobPK3, 0);
			AssertRetryAttempts(printJobPK3, 0);
		}

		void AssertRetryAttempts(Guid pK, int expectedRetryAttempts)
		{
			var selectPrintJobSql = $"SELECT SP_RetryAttempts FROM dbo.StmPrintJob WHERE SP_PK = '{pK}'";
			var retryAttempts = Convert.ToInt32(Db.Connection.ExecuteScalar(selectPrintJobSql));
			AssertEquals(expectedRetryAttempts, retryAttempts);
		}

		[UseSnapshotProtection]
		public void TestSetPrintQueuesEx()
		{
			var queuePK1 = Guid.NewGuid();
			var queuePK2 = Guid.NewGuid();
			var serverPK = Guid.NewGuid();

			// Q1 has user edits - cannot have SQ_AllowPrinting changed
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmPrintServer(SPS_PK, SPS_ServerName) values('{serverPK}', 'S1')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values('{queuePK1}', 1, 'Q1', 'Q1', '{serverPK}')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent) values(newid(), 'StmPrintQueue', '{queuePK1}', getdate(), '~BP', 'ADD')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent) values(newid(), 'StmPrintQueue', '{queuePK1}', getdate(), '~BP', 'EDT')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent) values(newid(), 'StmPrintQueue', '{queuePK1}', getdate(), 'M.K', 'EDT')");

			// Q2 has no user edits - may have SQ_AllowPrinting changed
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values('{queuePK2}', 1, 'Q2', 'Q2', '{serverPK}')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent) values(newid(), 'StmPrintQueue', '{queuePK2}', getdate(), '~BP', 'ADD')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmALog(SL_PK, SL_Table, SL_Parent, SL_EventTime, SL_GS_NKUser, SL_SE_NKEvent) values(newid(), 'StmPrintQueue', '{queuePK2}', getdate(), '~BP', 'EDT')");

			// Already disabled print queues should remain disabled
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(newid(), 0, 'Q3', 'Q3', '{serverPK}')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(newid(), 0, 'Q4', 'Q4', '{serverPK}')");

			// Enabled print queues (without user edits) can have SQ_AllowPrinting changed
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(newid(), 1, 'Q5', 'Q5', '{serverPK}')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(newid(), 1, 'Q6', 'Q6', '{serverPK}')");
			Db.Connection.ExecuteNonQuery($"insert into dbo.StmPrintQueue(SQ_PK, SQ_AllowPrinting, SQ_DisplayName, SQ_QueueName, SQ_SPS_Server) values(newid(), 1, 'Q7', 'Q7', '{serverPK}')");

			var printServer = new PrintServer();
			printServer.SetPrintQueuesEx("S1", new[]
			{
				new PrintQueueInfo { Name = "Q1", IsSuspectedSurrogate = true },
				new PrintQueueInfo { Name = "Q2", IsSuspectedSurrogate = true },
				new PrintQueueInfo { Name = "Q3", IsSuspectedSurrogate = false },
				new PrintQueueInfo { Name = "Q4", IsSuspectedSurrogate = true },
				// Q5 is not present - should be marked as deleted
				new PrintQueueInfo { Name = "Q6", IsSuspectedSurrogate = false },
				new PrintQueueInfo { Name = "Q7", IsSuspectedSurrogate = true },
				new PrintQueueInfo { Name = "Q8", IsSuspectedSurrogate = false },
				new PrintQueueInfo { Name = "Q9", IsSuspectedSurrogate = true },
			});

			AssertPrintQueue("Q1", true, true, "Queue with user edits");
			AssertPrintQueue("Q2", true, false, "Queue without user edits");
			AssertPrintQueue("Q3", true, false);
			AssertPrintQueue("Q4", true, false);
			AssertPrintQueue("Q5", false, true);
			AssertPrintQueue("Q6", true, true);
			AssertPrintQueue("Q7", true, false);
			AssertPrintQueue("Q8", true, true);
			AssertPrintQueue("Q9", true, false);
		}

		void AssertPrintQueue(string queueName, bool expectExists, bool expectAllowPrinting, string message = null)
		{
			if (message == null)
			{
				message = queueName;
			}

			var queueDeletedObject = Db.Connection.ExecuteScalar($"select SQ_QueueDeleted from dbo.StmPrintQueue where SQ_QueueName = '{queueName}'");
			var queueDeleted = (queueDeletedObject is DateTime dateTimeValue) ? dateTimeValue : DateTime.MinValue;

			if (!expectExists)
			{
				Assert(message + "should be marked as deleted", queueDeleted.Year >= 2021);
			}
			else
			{
				Assert(message + "should not be marked as deleted", queueDeleted.Year < 2000);

				var allowPrintObject = Db.Connection.ExecuteScalar($"select SQ_AllowPrinting from dbo.StmPrintQueue where SQ_QueueName = '{queueName}'");

				Assert(message + " should exist", allowPrintObject is bool);
				AssertEquals(message + ".SQ_AllowPrinting", expectAllowPrinting, (bool)allowPrintObject);
			}
		}
	}
}
