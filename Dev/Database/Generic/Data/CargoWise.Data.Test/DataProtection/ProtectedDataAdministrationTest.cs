using System;
using CargoWise.Data.Test.DataProtection;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.DataProtection.Testing
{
	public class ProtectedDataAdministrationTest : TransactionedTestCase
	{
		Mock<IProtectedDataService> pdsMock;
		Mock<IProtectedDataServiceFactory> pdsFactoryMock;
		Mock<ISqlServerLoginUtilities> loginUtilsMock;
		IServiceProvider serviceProvider;

		protected override void SetUp()
		{
			pdsMock = new Mock<IProtectedDataService>();

			pdsFactoryMock = new Mock<IProtectedDataServiceFactory>();
			pdsFactoryMock.Setup(x => x.CreateSystemService(It.IsAny<string>(), It.IsAny<string>())).Returns(() => pdsMock.Object);

			loginUtilsMock = new Mock<ISqlServerLoginUtilities>();

			var services = new ServiceCollection();

			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			services.ConfigureProtectedDataAdministrationServices();
			services.ConfigureProtectedDataSqlServerAdministrationServices();

			services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, DataProtectionTestSqlContextManager>();

			services.RemoveAll<IProtectedDataServiceFactory>();
			services.AddTransient((sp) => pdsFactoryMock.Object);

			services.RemoveAll<ISqlServerLoginUtilities>();
			services.AddTransient((sp) => loginUtilsMock.Object);

			serviceProvider = services.BuildServiceProvider();
		}

		public void TestGenerateApplicationLogins()
		{
			AssertSecretGetsStoredInPDSAndRegisteredInStateTable<CargoWiseReaderLoginCredentials>();
			AssertSecretGetsStoredInPDSAndRegisteredInStateTable<CargoWiseWriterLoginCredentials>();
			AssertSecretGetsStoredInPDSAndRegisteredInStateTable<RestrictedReaderLoginCredentials>();
			AssertSecretGetsStoredInPDSAndRegisteredInStateTable<RestrictedWriterLoginCredentials>();
			AssertSecretGetsStoredInPDSAndRegisteredInStateTable<UnrestrictedWriterLoginCredentials>();
		}

		public void TestActivateApplicationLogins()
		{
			AssertSecretGetsMarkedAsActiveAndLoginUtilsGetCalled<CargoWiseReaderLoginCredentials>();
			AssertSecretGetsMarkedAsActiveAndLoginUtilsGetCalled<CargoWiseWriterLoginCredentials>();
			AssertSecretGetsMarkedAsActiveAndLoginUtilsGetCalled<RestrictedReaderLoginCredentials>();
			AssertSecretGetsMarkedAsActiveAndLoginUtilsGetCalled<RestrictedWriterLoginCredentials>();
			AssertSecretGetsMarkedAsActiveAndLoginUtilsGetCalled<UnrestrictedWriterLoginCredentials>();
		}

		void AssertSecretGetsStoredInPDSAndRegisteredInStateTable<TCredentials>() where TCredentials : DBCredentials
		{
			// Arrange
			var typeName = new ProtectedDataServiceCapabilities().GetSecretTypeName<TCredentials>();
			var adminService = serviceProvider.GetRequiredService<IProtectedDataAdministrationService>();
			pdsMock.Setup(x => x.StoreSecret(It.IsAny<IProtectedData>())).Verifiable();

			// Act
			var generated = adminService.GenerateSecret(pdsMock.Object, Db.ServerName, Db.DatabaseName, typeName, DateTimeOffset.Parse("2024/09/02"), DateTimeOffset.Parse("2024/09/03"));

			// Assert
			var dtResult1 = DataUtils.GetDataTableFromQuery(Db.Connection, $"Select * from dbo.stmProtectedDataState where PDS_ProtectedDataID = '{generated.Id}'");
			AssertEquals(dtResult1.Rows.Count, 1);

			var reg1 = dtResult1.Rows[0];

			AssertEquals(generated.Id, reg1["PDS_ProtectedDataID"]);

			AssertEquals(false, reg1["PDS_Active"]);

			var registrationInfo = reg1["PDS_RegistrationInfo"] as string;
			var validFrom1 = reg1["PDS_ValidFrom"];
			var validTo1 = reg1["PDS_ValidTo"];

			AssertEquals(DateTimeOffset.Parse("2024/09/02"), validFrom1);
			AssertEquals(DateTimeOffset.Parse("2024/09/03"), validTo1);
			AssertContains(Environment.MachineName, registrationInfo);
			AssertContains(Environment.UserDomainName, registrationInfo);
			AssertContains(Environment.UserName, registrationInfo);

			pdsMock.Verify();
		}

		void AssertSecretGetsMarkedAsActiveAndLoginUtilsGetCalled<TCredentials>() where TCredentials : DBCredentials
		{
			// Arrange
			var typeName = new ProtectedDataServiceCapabilities().GetSecretTypeName<TCredentials>();
			var adminService = serviceProvider.GetRequiredService<IProtectedDataAdministrationService>();
			var generated = (ProtectedData<TCredentials>)adminService.GenerateSecret(pdsMock.Object, Db.ServerName, Db.DatabaseName, typeName, DateTimeOffset.Parse("2024/09/02"), DateTimeOffset.Parse("2024/09/03"));
			pdsMock.Setup(x => x.LoadSecret(generated.Id)).Returns(generated);
			loginUtilsMock.Setup(x => x.EnableOrCreateLogin(It.IsAny<ISqlExecutionContext>(), generated.Value.UserName, generated.Value.Password, false, false)).Verifiable();

			// Act
			adminService.ActivateSecret(pdsMock.Object, Db.ServerName, Db.DatabaseName, generated.Id);

			// Assert
			var dtResult1 = DataUtils.GetDataTableFromQuery(Db.Connection, $"Select * from dbo.stmProtectedDataState where PDS_ProtectedDataID = '{generated.Id}'");
			AssertEquals(dtResult1.Rows.Count, 1);

			var reg1 = dtResult1.Rows[0];

			AssertEquals(generated.Id, reg1["PDS_ProtectedDataID"]);

			AssertEquals(true, reg1["PDS_Active"]);
			var activationInfo = reg1["PDS_ActivationInfo"] as string;

			AssertContains(Environment.MachineName, activationInfo);
			AssertContains(Environment.UserDomainName, activationInfo);
			AssertContains(Environment.UserName, activationInfo);

			loginUtilsMock.Verify();
		}

		public void TestGenerateAndActivateOdysseyAdmin_DoesNotRegisterInStateTable()
		{
			// Arrange
			pdsMock.Setup(x => x.StoreSecret(It.IsAny<IProtectedData>())).Verifiable();
			var adminService = serviceProvider.GetRequiredService<IProtectedDataAdministrationService>();
			var generated = (ProtectedData<OdysseyAdminCredentials>)adminService.GenerateSecret(pdsMock.Object, Db.ServerName, Db.DatabaseName, "OdysseyAdmin", DateTimeOffset.Parse("2024/09/02"), DateTimeOffset.Parse("2024/09/03"));
			pdsMock.Setup(x => x.LoadSecret(generated.Id)).Returns(generated);
			loginUtilsMock.Setup(x => x.EnableOrCreateLogin(It.IsAny<ISqlExecutionContext>(), generated.Value.UserName, generated.Value.Password, true, true)).Verifiable();

			// Act
			adminService.ActivateSecret(pdsMock.Object, Db.ServerName, Db.DatabaseName, generated.Id);

			// Assert
			var dtResult1 = DataUtils.GetDataTableFromQuery(Db.Connection, $"Select * from dbo.stmProtectedDataState where PDS_ProtectedDataID = '{generated.Id}'");
			AssertEquals(dtResult1.Rows.Count, 0);

			pdsMock.Verify();
			loginUtilsMock.Verify();
		}
	}
}
