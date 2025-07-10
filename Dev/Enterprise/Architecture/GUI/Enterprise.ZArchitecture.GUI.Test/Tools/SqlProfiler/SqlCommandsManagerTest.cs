using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Tools.Testing
{
	[TestedType(typeof(SqlCommandsManager))]
	public class SqlCommandsManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddSqlCommandsEvent()
		{
			AssertEquals(0, CommandsManager.CommandList.Count);
			CommandsManager.Start();
			AddSqlCommands(new Exception("Testing"));
			AssertEquals(1, CommandsManager.CommandList.Count);
			AssertEquals("Testing", CommandsManager.CommandList[0].ExceptionMessage);
			AssertEquals("Select * From dbo.JobContainer", CommandsManager.CommandList[0].SqlText);
			AssertEquals(true, CommandsManager.CommandList[0].IsFailed);
			AddSqlCommands(null);
			AssertEquals(2, CommandsManager.CommandList.Count);
			AssertEquals(string.Empty, CommandsManager.CommandList[1].ExceptionMessage);
			AssertEquals("Select * From dbo.JobContainer", CommandsManager.CommandList[1].SqlText);
			AssertEquals(false, CommandsManager.CommandList[1].IsFailed);
		}

		public void TestState()
		{
			AssertEquals(SqlProfilerState.Stopped, CommandsManager.State);
			AddSqlCommands(new Exception("Testing1"));
			AssertEquals(SqlProfilerState.Stopped, CommandsManager.State);
			AssertEquals(0, CommandsManager.CommandList.Count);
			CommandsManager.Start();
			AssertEquals(SqlProfilerState.Running, CommandsManager.State);
			AddSqlCommands(new Exception("Testing2"));
			AssertEquals(1, CommandsManager.CommandList.Count);
			AssertEquals("Testing2", CommandsManager.CommandList[0].ExceptionMessage);
			CommandsManager.Pause();
			AssertEquals(SqlProfilerState.Suspended, CommandsManager.State);
			AddSqlCommands(new Exception("Testing3"));
			AssertEquals(1, CommandsManager.CommandList.Count);
			AssertEquals("Testing2", CommandsManager.CommandList[0].ExceptionMessage);
			CommandsManager.Clear();
			AssertEquals(0, CommandsManager.CommandList.Count);
		}

		public void TestMaxLength()
		{
			CommandsManager.Start();
			for (var i = 0; i < 1000; i++)
			{
				AddSqlCommands(new Exception(i.ToString()));
			}

			AssertEquals(1000, CommandsManager.CommandList.Count);
			AddSqlCommands(new Exception("1000"));
			AssertEquals(1000, CommandsManager.CommandList.Count);
			AssertEquals("1", CommandsManager.CommandList[0].ExceptionMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CommandsManager = new SqlCommandsManager(TaskScheduler.FromCurrentSynchronizationContext());
			instances.Add(CommandsManager);
		}

		protected override void TearDown()
		{
			foreach (var instance in instances)
			{
				instance.Dispose();
			}

			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject();
			instances.Add((SqlCommandsManager)result);
			return result;
		}

		void AddSqlCommands(Exception exception)
		{
			var dbCommandText = "Select * From dbo.JobContainer";
			CommandsManager.AddSqlCommandsEventCore(new SqlCommandExecutedEventArgs(dbCommandText, exception, DateTime.UtcNow));
		}

		SqlCommandsManager CommandsManager;
		readonly List<SqlCommandsManager> instances = new List<SqlCommandsManager>();
	}
}
