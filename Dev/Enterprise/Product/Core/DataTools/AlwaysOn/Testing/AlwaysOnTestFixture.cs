using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	public class AlwaysOnTestFixture : AlwaysOnUITestFixture
	{
		public IDisposable executionScope;
		protected override void SetUp()
		{
			base.SetUp();
			executionScope = Program.SqlContextManager.NewExecutionScope();
		}

		protected override void TearDown()
		{
			executionScope.Dispose();
			base.TearDown();
		}
	}
	public class AlwaysOnUITestFixture : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			Program.SetupServiceProvider();
		}

		protected override void TearDown()
		{
			base.TearDown();
		}
	}

	class SqlExecutionContextForTest : ISqlExecutionContext
	{
		readonly ISqlExecutionContext baseContext;
		public List<string> sqlCommandString { get; private set; }

		public IDataReader dataReaderOverride;
		public object scalarValue;
		public bool overrideExecuteScalar;

		public SqlExecutionContextForTest(List<String> sqlCommandString, ISqlExecutionContext baseContext)
		{
			this.sqlCommandString = sqlCommandString;
			this.baseContext = baseContext;
		}

		public int ExecuteNonQuery(string commandText, CommandType commandType = CommandType.Text, Action<IDbCommand> setupParameters = null)
		{
			sqlCommandString.Add(commandText);
			return baseContext?.ExecuteNonQuery(commandText, commandType, setupParameters) ?? -1;
		}

		public IDataReader ExecuteReader(string commandText, CommandType commandType = CommandType.Text, Action<IDbCommand> setupParameters = null)
		{
			sqlCommandString.Add(commandText);
			return dataReaderOverride ?? baseContext?.ExecuteReader(commandText, commandType, setupParameters);
		}

		public object ExecuteScalar(string commandText, CommandType commandType = CommandType.Text, Action<IDbCommand> setupParameters = null)
		{
			sqlCommandString?.Add(commandText);
			if (overrideExecuteScalar)
			{
				return scalarValue;
			}
			return baseContext.ExecuteScalar(commandText, commandType, setupParameters);
		}
	}
}
