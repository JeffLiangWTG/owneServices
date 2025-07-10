using System.Data;
using CargoWise.Data;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace CargoWise.ProductRegistration.Service.Test
{
	class ConnectionManagerTest : TestCase
	{
		readonly string defaultConnectionString = $"Server={Db.ServerName};Database={Db.DatabaseName};Trusted_Connection=false;Pooling=true;Max Pool Size=10;Application Name=CargoWise.ProductRegistration.Service;TrustServerCertificate=True;";

		public void TestConnectionDoesNotExistAndNeedsToBeCreated()
		{
			// Arrange
			settings.PrimaryConnectionString = defaultConnectionString;
			settings.SecondaryConnectionString = string.Empty;

			// Act
			CreateAndOpenConnectionIfNeeded();

			// Assert
			AssertEquals("Connection should be open", ConnectionState.Open, connectionManager.Connection.State);
		}

		public void TestConnectionExistsAndNotNeedToBeCreated()
		{
			// Arrange
			settings.PrimaryConnectionString = defaultConnectionString;
			settings.SecondaryConnectionString = string.Empty;

			CreateAndOpenConnectionIfNeeded();
			var firstConnection = connectionManager.Connection;

			// Act
			CreateAndOpenConnectionIfNeeded();

			// Assert
			AssertEquals("No new connection is created", firstConnection, connectionManager.Connection);
			AssertEquals("Connection should be open", ConnectionState.Open, connectionManager.Connection.State);
		}

		public void TestSecondaryConnectionConnects()
		{
			// Arrange
			settings.PrimaryConnectionString = "Server=wrong;Database=incorrect;Trusted_Connection=false;Pooling=true;Max Pool Size=10;Application Name=CargoWise.ProductRegistration.Service;TrustServerCertificate=True;";
			settings.SecondaryConnectionString = defaultConnectionString;

			// Act
			CreateAndOpenConnectionIfNeeded();

			// Assert
			AssertEquals("Connection should be open", ConnectionState.Open, connectionManager.Connection.State);
		}

		public void TestUpgradeExceptionIsThrownWhenDbIsLockedOut()
		{
			settings.PrimaryConnectionString = defaultConnectionString;
			_ = AssertExceptionThrown<DatabaseUpgradeInProgressException>(() =>
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					try
					{
						adminConnection.BeginTransaction();
						DbLockout.AcquireTransactionLockout(adminConnection);
						CreateAndOpenConnectionIfNeeded();
					}
					finally
					{
						adminConnection.RollbackTransaction();
					}
				}
			});
		}

		void CreateAndOpenConnectionIfNeeded()
		{
			connectionManager.CreateAndOpenConnectionIfNeeded();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var serviceCollection = new ServiceCollection();
			serviceCollection.ConfigureProtectedDataFactoryServices();
			serviceCollection.ConfigureProtectedDataSqlExtensions(ApplicationType.Web);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			var connectionProvider = serviceProvider.GetRequiredService<ISqlConnectionProvider>();

			settings = new DatabaseSettings();
			connectionManager = new ConnectionManager(connectionProvider, Options.Create(settings));
		}

		protected override void TearDown()
		{
			connectionManager?.Dispose();
			connectionManager = null;
		}

		DatabaseSettings settings;
		ConnectionManager connectionManager;
	}
}
