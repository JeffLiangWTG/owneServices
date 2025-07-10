using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using Enterprise.RemotePrinting.Server.JobPrinting;
using NUnit.Framework;

[assembly: WTG.StaticAnalysis.Annotation.UsesConstants(typeof(Enterprise.RemotePrinting.Client.WebClient))]

namespace Enterprise.RemotePrinting.Server.Testing
{
	/// <summary>
	/// Tests class Enterprise.RemotePrinting.Server.RPSCore.PrintJobPoller (RPSCore project)
	/// </summary>
	public class PrintJobPollerTest : TestCase
	{
		public void TestCheckForJobs()
		{
			var sqlText = string.Format(@"
				DECLARE @ServerPk1 UNIQUEIDENTIFIER = NEWID()
				DECLARE @ServerPk2 UNIQUEIDENTIFIER = NEWID()
				DECLARE @ServerPk3 UNIQUEIDENTIFIER = NEWID()
				INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (@ServerPk1, '~TestPrintServer1')
				INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (@ServerPk2, '~TestPrintServer2')
				INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (@ServerPk3, '~TestPrintServer3')

				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName)
					VALUES ('{0}', @ServerPk1, 'Queue1', 'QueueDisplay1')
				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName)
					VALUES ('{1}', @ServerPk2, 'Queue2', 'QueueDisplay2')
				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName)
					VALUES ('{2}', @ServerPk3, 'Queue3', 'QueueDisplay3')

				INSERT dbo.StmDeliveryGroup (SB_PK, SB_IsProcessed)
					VALUES ('{3}', 0)
				INSERT dbo.StmDeliveryGroup (SB_PK, SB_IsProcessed)
					VALUES ('{4}', 1)

				INSERT dbo.StmPrintJob (SP_PK, SP_RunDateTime, SP_JobType, SP_SQ, SP_SB_DeliveryGroup)
					VALUES (NEWID(), '2006-07-26', 'PRN', '{0}', '{3}')
				INSERT dbo.StmPrintJob (SP_PK, SP_RunDateTime, SP_JobType, SP_SQ, SP_SB_DeliveryGroup)
					VALUES (NEWID(), '2006-07-26', 'PRN', '{0}', '{4}')
				INSERT dbo.StmPrintJob (SP_PK, SP_RunDateTime, SP_JobType, SP_SQ, SP_SB_DeliveryGroup)
					VALUES (NEWID(), '2006-07-26', 'PRN', '{1}', '{4}')
				INSERT dbo.StmPrintJob (SP_PK, SP_RunDateTime, SP_JobType, SP_SQ, SP_SB_DeliveryGroup)
					VALUES (NEWID(), '2006-07-26', 'PRN', '{2}', '{3}')",
				Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(),
				Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

			List<string> serverList = new List<string>();
			serverList.Add("~TestPrintServer1");
			serverList.Add("~TestPrintServer2");
			serverList.Add("~TestPrintServer3");

			using (DbConnection conn = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					conn.BeginTransaction();
					conn.ExecuteNonQuery(sqlText);

					PrintJobPollerForTesting testPrintPoller = new PrintJobPollerForTesting(conn);
					testPrintPoller.CheckForJobs_Exposed(serverList);

					AssertEquals("There should be 2 print servers", 2, testPrintPoller.ReleasedServerList.Count);

					if (testPrintPoller.ReleasedServerList[0] == "~TestPrintServer1")
					{
						AssertEquals("Released Server 0", "~TestPrintServer1", testPrintPoller.ReleasedServerList[0]);
						AssertEquals("Released Server 1", "~TestPrintServer2", testPrintPoller.ReleasedServerList[1]);
					}
					else
					{
						AssertEquals("Released Server 0", "~TestPrintServer2", testPrintPoller.ReleasedServerList[0]);
						AssertEquals("Released Server 1", "~TestPrintServer1", testPrintPoller.ReleasedServerList[1]);
					}
				}
				finally
				{
					if (conn.State == ConnectionState.Open && conn.IsInTransaction)
					{
						conn.RollbackTransaction();
					}
				}
			}
		}

		public void TestCheckForJobsWithNoJobs()
		{
			var sqlText = string.Format(@"
				DECLARE @ServerPk1 UNIQUEIDENTIFIER = NEWID()
				DECLARE @ServerPk2 UNIQUEIDENTIFIER = NEWID()
				DECLARE @ServerPk3 UNIQUEIDENTIFIER = NEWID()
				INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (@ServerPk1, '~TestPrintServer1')
				INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (@ServerPk2, '~TestPrintServer2')
				INSERT dbo.StmPrintServer (SPS_PK, SPS_ServerName) VALUES (@ServerPk3, '~TestPrintServer3')

				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName)
					VALUES ('{0}', @ServerPk1, 'Queue1', 'QueueDisplay1')
				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName)
					VALUES ('{1}', @ServerPk2, 'Queue2', 'QueueDisplay2')
				INSERT dbo.StmPrintQueue (SQ_PK, SQ_SPS_Server, SQ_QueueName, SQ_DisplayName)
					VALUES ('{2}', @ServerPk3, 'Queue3', 'QueueDisplay3')

				INSERT dbo.StmDeliveryGroup (SB_PK, SB_IsProcessed)
					VALUES ('{3}', 0)

				INSERT dbo.StmPrintJob (SP_PK, SP_RunDateTime, SP_JobType, SP_SQ, SP_SB_DeliveryGroup)
					VALUES (NEWID(), '2006-07-26', 'PRN', '{0}', '{3}')
				INSERT dbo.StmPrintJob (SP_PK, SP_RunDateTime, SP_JobType, SP_SQ, SP_SB_DeliveryGroup)
					VALUES (NEWID(), '2006-07-26', 'PRN', '{2}', '{3}')",
				Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

			List<string> serverList = new List<string>();
			serverList.Add("~TestPrintServer1");
			serverList.Add("~TestPrintServer2");
			serverList.Add("~TestPrintServer3");

			using (DbConnection conn = Db.NewExtraConnectionToMainDb())
			{
				try
				{
					conn.BeginTransaction();
					conn.ExecuteNonQuery(sqlText);

					PrintJobPollerForTesting testPrintPoller = new PrintJobPollerForTesting(conn);
					testPrintPoller.CheckForJobs_Exposed(serverList);

					AssertEquals("There should be 2 print servers", 0, testPrintPoller.ReleasedServerList.Count);
				}
				finally
				{
					if (conn.State == ConnectionState.Open && conn.IsInTransaction)
					{
						conn.RollbackTransaction();
					}
				}
			}
		}
	}

	class PrintJobPollerForTesting : PrintJobPoller
	{
		public PrintJobPollerForTesting(DbConnection testConnection)
		{
			this.testConnection = testConnection;
		}

		public List<string> ReleasedServerList = new List<string>();

		public void CheckForJobs_Exposed(List<string> serverList)
		{
			ReleasedServerList.Clear();
			CheckForJobs(serverList);
		}

		protected override void SignalJobWaiter(string serverName)
		{
			ReleasedServerList.Add(serverName);
		}

		protected override DbConnection NewConnection()
		{
			return TestConnection;
		}

		internal DbConnection TestConnection => testConnection ?? (testConnection = Db.NewExtraConnectionToMainDb());
		DbConnection testConnection;
	}
}
