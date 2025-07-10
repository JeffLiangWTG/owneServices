using System.Diagnostics.CodeAnalysis;
using CargoWise.Data;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.Builder.Generator.RestoreDatabase;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Enterprise.Builder.Generator.Testing
{
	public class RestorerTests : TestCase
	{
		readonly TestRestorer TestRestorer;

		public RestorerTests()
		{
			TestRestorer = new TestRestorer();
		}

		[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClassesAnalyzer", Justification = "Should continue to use SqlConnection rather than CargoWise.Data.Db.Connection here.'")]
		public void TestCreateDatabaseIfNotExists()
		{
			// Arrange
			var serverName = Db.ServerName;
			var databaseName = "DatabaseShouldNotExist";

			var connectionStringBuilder = new SqlConnectionStringBuilder
			{
				PersistSecurityInfo = false,
				Pooling = false,
				ConnectTimeout = 120,
				ApplicationName = "CargoWiseOneBuilder",
				DataSource = serverName,
				InitialCatalog = "master"
			};

			connectionStringBuilder.Encrypt = false;
			connectionStringBuilder.IntegratedSecurity = true;

			using (var connection = new SqlConnection(connectionStringBuilder.ToString()))
			{
				var checkDbQuery = $"SELECT database_id FROM sys.databases WHERE name = '{databaseName}'";
				using (var command = new SqlCommand(checkDbQuery, connection))
				{
					connection.Open();
					var result = command.ExecuteScalar();
					AssertEquals("Expected the database to not exist.", null, result);
				}

				// Act
				TestRestorer.CreateDatabaseIfNotExists(serverName, databaseName);

				// Assert
				using (var command = new SqlCommand(checkDbQuery, connection))
				{
					var result = command.ExecuteScalar();
					AssertNotNull("Expected the database to exist.", result);
				}

				var dropDbQuery = $"DROP DATABASE [{databaseName}]";
				using (var dropCommand = new SqlCommand(dropDbQuery, connection))
				{
					dropCommand.ExecuteNonQuery();
				}
			}
		}

		public void TestDoesNotThrowWhenClosingTwice()
		{
			// Arrange
			var serverName = Db.ServerName;
			var databaseName = "DatabaseShouldNotExist";

			// Act
			var contextManager = (SqlExecutionContextManager)Application.ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();
			// Initialising Connection for contextManager variable
			contextManager.GetSqlExecutionContext(serverName, databaseName);

			// Assert
			contextManager.Close();
			AssertNoExceptionThrown(() => contextManager.Close());
		}
	}

	class TestRestorer : Restorer
	{
		public new void CreateDatabaseIfNotExists(string serverName, string databaseName)
		{
			base.CreateDatabaseIfNotExists(serverName, databaseName);
		}
	}
}
