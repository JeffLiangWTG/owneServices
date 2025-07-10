using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	[SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "this assembly has no reference to Enterprise libraries")]
	public partial class LoginRepairServiceTest : TestCase
	{
		Mock<IProtectedDataService> pdsMock;
		Mock<IProtectedDataStateService> stateServiceMock;
		Mock<IProtectedDataServiceFactory> pdsFactoryMock;
		IServiceProvider serviceProvider;

#pragma warning disable IDE0001 // Simplify Names - Justification: Service types has to be explicitly specified 
		protected override void SetUp()
		{
			base.SetUp();
			pdsMock = new Mock<IProtectedDataService>();
			stateServiceMock = new Mock<IProtectedDataStateService>();
			pdsFactoryMock = new Mock<IProtectedDataServiceFactory>();

			var services = new ServiceCollection();
			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			services.ConfigureProtectedDataAdministrationServices();
			services.ConfigureProtectedDataSqlServerAdministrationServices();

			services.AddSingleton<IProtectedDataServiceFactory>((sp) => pdsFactoryMock.Object);
			services.AddSingleton<IProtectedDataStateService>((sp) => stateServiceMock.Object);
			services.AddSingleton<ProtectedDataServiceCapabilities, ProtectedDataServiceCapabilitiesWithCustom>();
			services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, CargowisePDSAdministrationSqlContextManager>();
			services.AddTransient<ProtectedDataManager<CustomCredentials>, ApplicationLoginManager<CustomCredentials>>();

			services.AddTransient<LoginRepairService, LoginRepairService>();

			serviceProvider = services.BuildServiceProvider();
		}
#pragma warning restore IDE0001 // Simplify Names

		public void TestLoginRepairService_LoginDoesNotExist_Success()
		{
			// Arrange

			var secretId = Guid.NewGuid();
			var credentials = new CustomCredentials(nameof(TestLoginRepairService_LoginDoesNotExist_Success) + "Login", "SomePassword!!##.123");
			var secret = new ProtectedData<CustomCredentials>(secretId, "Custom", DateTimeOffset.Now, DateTimeOffset.Now, credentials);
			pdsMock.Setup(x => x.LoadSecret(It.Is<Guid>((value) => value == secretId))).Returns(secret);
			stateServiceMock.Setup(x => x.GetActiveProtectedDataId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(secretId);
			pdsFactoryMock.Setup(x => x.CreateSystemService(It.IsAny<string>(), It.IsAny<string>())).Returns(pdsMock.Object);

			using var adminConnection = Db.NewAdminConnection();
			DropLoginIfExists(adminConnection, credentials.UserName);

			try
			{
				// Act
				var repairService = ActivatorUtilities.CreateInstance<LoginRepairService>(serviceProvider);
				repairService.ReviveApplicationLogin<CustomCredentials>(adminConnection, Db.DatabaseName, adminConnection);

				// Assert
				var csBuilder = new SqlConnectionStringBuilder()
				{
					DataSource = Db.ServerName,
					UserID = credentials.UserName,
					Password = credentials.Password,
					TrustServerCertificate = true,
					Encrypt = false,
					Pooling = false,
				};
				using (var sqlConnection = new SqlConnection(csBuilder.ConnectionString))
				{
					AssertNoExceptionThrown(() => sqlConnection.Open());
				}
			}
			finally
			{
				DropLoginIfExists(adminConnection, credentials.UserName);
			}
		}

		public void TestLoginRepairService_LoginDisabled_Success()
		{
			// Arrange

			var secretId = Guid.NewGuid();
			var credentials = new CustomCredentials(nameof(TestLoginRepairService_LoginDisabled_Success) + "Login", "SomePassword!!##.123");
			var secret = new ProtectedData<CustomCredentials>(secretId, "Custom", DateTimeOffset.Now, DateTimeOffset.Now, credentials);
			pdsMock.Setup(x => x.LoadSecret(It.Is<Guid>((value) => value == secretId))).Returns(secret);
			stateServiceMock.Setup(x => x.GetActiveProtectedDataId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(secretId);
			pdsFactoryMock.Setup(x => x.CreateSystemService(It.IsAny<string>(), It.IsAny<string>())).Returns(pdsMock.Object);

			using var adminConnection = Db.NewAdminConnection();
			DropLoginIfExists(adminConnection, credentials.UserName);
			CreateLogin(adminConnection, credentials.UserName, credentials.Password);
			DisableLogin(adminConnection, credentials.UserName);
			try
			{
				// Act
				var repairService = ActivatorUtilities.CreateInstance<LoginRepairService>(serviceProvider);
				repairService.ReviveApplicationLogin<CustomCredentials>(adminConnection, Db.DatabaseName, adminConnection);

				// Assert
				var csBuilder = new SqlConnectionStringBuilder()
				{
					DataSource = Db.ServerName,
					UserID = credentials.UserName,
					Password = credentials.Password,
					TrustServerCertificate = true,
					Encrypt = false,
					Pooling = false,
				};
				using (var sqlConnection = new SqlConnection(csBuilder.ConnectionString))
				{
					AssertNoExceptionThrown(() => sqlConnection.Open());
				}
			}
			finally
			{
				DropLoginIfExists(adminConnection, credentials.UserName);
			}
		}

		public void TestLoginRepairService_WrongPassword_Success()
		{
			// Arrange

			var secretId = Guid.NewGuid();
			var credentials = new CustomCredentials(nameof(TestLoginRepairService_WrongPassword_Success) + "Login", "SomePassword!!##.123");
			var secret = new ProtectedData<CustomCredentials>(secretId, "Custom", DateTimeOffset.Now, DateTimeOffset.Now, credentials);
			pdsMock.Setup(x => x.LoadSecret(It.Is<Guid>((value) => value == secretId))).Returns(secret);
			stateServiceMock.Setup(x => x.GetActiveProtectedDataId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(secretId);
			pdsFactoryMock.Setup(x => x.CreateSystemService(It.IsAny<string>(), It.IsAny<string>())).Returns(pdsMock.Object);

			using var adminConnection = Db.NewAdminConnection();
			DropLoginIfExists(adminConnection, credentials.UserName);
			CreateLogin(adminConnection, credentials.UserName, "WrongPassword!!!!##.1234");

			try
			{
				// Act
				var repairService = ActivatorUtilities.CreateInstance<LoginRepairService>(serviceProvider);
				repairService.ReviveApplicationLogin<CustomCredentials>(adminConnection, Db.DatabaseName, adminConnection);

				// Assert
				var csBuilder = new SqlConnectionStringBuilder()
				{
					DataSource = Db.ServerName,
					UserID = credentials.UserName,
					Password = credentials.Password,
					TrustServerCertificate = true,
					Encrypt = false,
					Pooling = false,
				};
				using (var sqlConnection = new SqlConnection(csBuilder.ConnectionString))
				{
					AssertNoExceptionThrown(() => sqlConnection.Open());
				}
			}
			finally
			{
				DropLoginIfExists(adminConnection, credentials.UserName);
			}
		}

		void DropLoginIfExists(DbConnection adminConnection, string loginName)
		{
			adminConnection.ExecuteNonQuery(@$"
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = '{loginName}')
BEGIN
    DROP LOGIN [{loginName}];
END");
		}

		void DisableLogin(DbConnection adminConnection, string loginName)
		{
			adminConnection.ExecuteNonQuery(@$"ALTER LOGIN [{loginName}] DISABLE;");
		}

		void CreateLogin(DbConnection adminConnection, string loginName, string password)
		{
			adminConnection.ExecuteNonQuery(@$"CREATE LOGIN [{loginName}] WITH PASSWORD = '{password}';");
		}
	}
}
