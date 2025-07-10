using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.DataProtection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Providers.Common.Test
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Test Class")]
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "This assembly has no reference to enterprise libraries")]
	public class SqlDataProviderFactoryTest
	{
		class SqlDataProviderFactoryForTest : SqlDataProviderFactory
		{
			public SqlDataProviderFactoryForTest() : base(new Mock<IProtectedDataService>().Object, Mock.Of<ISqlConnectionProvider>())
			{
			}

			protected override IDbConnection OpenConnection(string connectionString)
			{
				return new SqlConnection(connectionString);
			}
		}

		[Test]
		public void TestSqlDataProviderFactoryConnection()
		{
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: Array.Empty<string>()))
			{
				var dataProviderFactory = new SqlDataProviderFactoryForTest();
				using var connection = dataProviderFactory.OpenNewDbConnection("sdpftServer", "sdpftDb", "sdpftLogin", "sdpftPwd", "sdpftApp", 10, false, 0, 0, 0);

				Assert.That(connection, Is.Not.Null);
				Assert.That(connection.ConnectionString, Does.Not.Contain("MultiSubnetFailover=True"));
				Assert.That(connection.Database, Is.EqualTo("sdpftDb"));
				Assert.That(connection.State, Is.EqualTo(ConnectionState.Closed));

				var command = dataProviderFactory.NewDbCommand("sdpftCmdText", connection, null);
				Assert.That(command, Is.InstanceOf(typeof(SqlCommand)));
				Assert.That(command.CommandText, Is.EqualTo("sdpftCmdText"));
				Assert.That(command.Connection, Is.EqualTo(connection));
				Assert.That(command.Transaction, Is.Null);

				var adapter = dataProviderFactory.NewDataAdapter(dataProviderFactory.NewDbCommand("sdpftCmdText", connection, null));
				Assert.That(adapter, Is.InstanceOf(typeof(SqlDataAdapter)));
				Assert.That(adapter.SelectCommand.CommandText, Is.EqualTo("sdpftCmdText"));
				Assert.That(adapter.InsertCommand, Is.Null);
				Assert.That(adapter.UpdateCommand, Is.Null);
				Assert.That(adapter.DeleteCommand, Is.Null);
			}
		}

		[Test]
		public void TestNewDbParameter()
		{
			var dataProviderFactory = new SqlDataProviderFactoryForTest();
			var parameter = dataProviderFactory.NewDbParameter("sdpftParam", SqlDbType.VarChar, 10);
			Assert.That(parameter, Is.InstanceOf(typeof(SqlParameter)));
			Assert.That(parameter.ParameterName, Is.EqualTo("sdpftParam"));
			Assert.That(parameter.DbType, Is.EqualTo(DbType.AnsiString));
			Assert.That(parameter.Size, Is.EqualTo(10));
		}

		[Test]
		public void EnabledMultiSubnetFailover()
		{
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: new[] { "sdpftServer" }))
			{
				using var connection = new SqlDataProviderFactoryForTest().OpenNewDbConnection("sdpftServer", "sdpftDb", "sdpftLogin", "sdpftPwd", "sdpftApp", 10, false, 0, 0, 0);
				Assert.That(new SqlConnectionStringBuilder(connection.ConnectionString).MultiSubnetFailover, Is.EqualTo(true), nameof(SqlConnectionStringBuilder.MultiSubnetFailover));
			}
		}

		[Test]
		public void DisabledMultiSubnetFailover()
		{
			using (SqlFailoverSettingsTestHelper.SetMockWindowsRegistry(serversThatAreEnabled: Array.Empty<string>()))
			{
				using var connection = new SqlDataProviderFactoryForTest().OpenNewDbConnection("sdpftServer", "sdpftDb", "sdpftLogin", "sdpftPwd", "sdpftApp", 10, false, 0, 0, 0);
				Assert.That(new SqlConnectionStringBuilder(connection.ConnectionString).MultiSubnetFailover, Is.EqualTo(false), nameof(SqlConnectionStringBuilder.MultiSubnetFailover));
			}
		}
	}
}
