using System;
using CargoWise.Data.Test.DataProtection;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.DataProtection.DefaultSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;

namespace CargoWise.Data.DataProtection.Testing
{
	public class ProtectedDataInitializationServiceTest : TransactionedTestCase
	{
		readonly ProtectedData<CargoWiseWriterLoginCredentials> protectedData1 = new ProtectedData<CargoWiseWriterLoginCredentials>(Guid.Parse("11111111-0000-0000-0000-000000000000"), "Writer", DateTimeOffset.Parse("2024/04/24"), DateTimeOffset.Parse("2025/05/25 12:13:14"), new CargoWiseWriterLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest1" });
		readonly ProtectedData<CargoWiseWriterLoginCredentials> protectedData2 = new ProtectedData<CargoWiseWriterLoginCredentials>(Guid.Parse("22222222-0000-0000-0000-000000000000"), "Writer", DateTimeOffset.Now, DateTimeOffset.Now.AddHours(2), new CargoWiseWriterLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest2" });

		Mock<IProtectedDataService> pdsMock;
		Mock<IProtectedDataServiceFactory> pdsFactoryMock;
		IServiceProvider serviceProvider;

		protected override void SetUp()
		{
			pdsMock = new Mock<IProtectedDataService>();
			pdsMock.Setup(x => x.LoadSecret(protectedData1.Id)).Returns(protectedData1);
			pdsMock.Setup(x => x.LoadSecret(protectedData2.Id)).Returns(protectedData2);

			pdsFactoryMock = new Mock<IProtectedDataServiceFactory>();
			pdsFactoryMock.Setup(x => x.CreateSystemService(It.IsAny<string>(), It.IsAny<string>())).Returns(() => pdsMock.Object);

			var services = new ServiceCollection();

			services.ConfigureProtectedDataFactoryServices();
			services.ConfigureProtectedDataSqlExtensions(ApplicationType.Default);
			services.ConfigureProtectedDataAdministrationServices();
			services.ConfigureProtectedDataSqlServerAdministrationServices();

			services.AddSingleton<IProtectedDataAdministrationSqlExecutionContextManager, DataProtectionTestSqlContextManager>();

			services.RemoveAll<IProtectedDataServiceFactory>();
			services.AddScoped((sp) => pdsFactoryMock.Object);

			//ConfigureOtherServices(services);

			serviceProvider = services.BuildServiceProvider();
		}

		public void TestProtectedDataStatusService_RegisterAndActivate_Success()
		{
			var protectedDataStateService = serviceProvider.GetRequiredService<IProtectedDataStateService>();

			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData1, comments: "Registration For Test 1");
			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData2, comments: "Registration For Test 2");
			protectedDataStateService.MarkProtectedDataAsActive(Db.ServerName, Db.DatabaseName, protectedData1, comments: "Activation For Test 1");

			var dtResult1 = DataUtils.GetDataTableFromQuery(Db.Connection, "Select * from dbo.stmProtectedDataState where PDS_ProtectedDataID = '11111111-0000-0000-0000-000000000000'");
			AssertEquals(dtResult1.Rows.Count, 1);
			var dtResult2 = DataUtils.GetDataTableFromQuery(Db.Connection, "Select * from dbo.stmProtectedDataState where PDS_ProtectedDataID = '22222222-0000-0000-0000-000000000000'");
			AssertEquals(dtResult2.Rows.Count, 1);

			var reg1 = dtResult1.Rows[0];
			var reg2 = dtResult2.Rows[0];

			AssertEquals(reg1["PDS_ProtectedDataID"], protectedData1.Id);
			AssertEquals(reg2["PDS_ProtectedDataID"], protectedData2.Id);

			AssertEquals(reg1["PDS_Active"], true);
			AssertEquals(reg2["PDS_Active"], false);

			var registrationInfo1 = reg1["PDS_RegistrationInfo"] as string;
			var validFrom1 = reg1["PDS_ValidFrom"];
			var validTo1 = reg1["PDS_ValidTo"];
			var registrationInfo2 = reg2["PDS_RegistrationInfo"] as string;
			var activationInfo = reg1["PDS_ActivationInfo"] as string;

			AssertEquals(DateTimeOffset.Parse("2024/04/24"), validFrom1);
			AssertEquals(DateTimeOffset.Parse("2025/05/25 12:13:14"), validTo1);
			AssertContains(Environment.MachineName, registrationInfo1);
			AssertContains(Environment.UserDomainName, registrationInfo1);
			AssertContains(Environment.UserName, registrationInfo1);
			AssertContains("Registration For Test 1", registrationInfo1);

			AssertContains(Environment.MachineName, registrationInfo2);
			AssertContains(Environment.UserDomainName, registrationInfo2);
			AssertContains(Environment.UserName, registrationInfo2);
			AssertContains("Registration For Test 2", registrationInfo2);

			AssertContains(Environment.MachineName, activationInfo);
			AssertContains(Environment.UserDomainName, activationInfo);
			AssertContains(Environment.UserName, activationInfo);
		}

		public void TestProtectedDataStatusService_DoubleRegistration_NotAllowed()
		{
			var protectedDataStateService = serviceProvider.GetRequiredService<IProtectedDataStateService>();

			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData1, comments: "Test Comments");

			AssertExceptionThrown<SqlException>(@"A protected data item with the Id ""11111111-0000-0000-0000-000000000000"" is already registered.",
			() =>
			{
				protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData1, comments: "Test Comments");
			});
		}

		public void TestProtectedDataStatusService_DoubleRegistration_DifferentProtectedDataWithSameId_NotAllowed()
		{
			var protectedData1 = new ProtectedData<CargoWiseWriterLoginCredentials>(Guid.Parse("11111111-0000-0000-0000-000000000000"), "Writer", DateTimeOffset.Now, DateTimeOffset.Now.AddHours(2), new CargoWiseWriterLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest1" });
			var protectedData2 = new ProtectedData<CargoWiseReaderLoginCredentials>(Guid.Parse("11111111-0000-0000-0000-000000000000"), "Reader", DateTimeOffset.Now, DateTimeOffset.Now.AddHours(2), new CargoWiseReaderLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest2" });

			var protectedDataStateService = serviceProvider.GetRequiredService<IProtectedDataStateService>();

			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData1, comments: "Test Comments");

			AssertExceptionThrown<SqlException>(@"A protected data item with the Id ""11111111-0000-0000-0000-000000000000"" is already registered.",
			() =>
			{
				protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData1, comments: "Test Comments");
			});
		}

		public void TestProtectedDataStatusService_DoubleActivation_FirstActivationRemains()
		{
			var protectedData1 = new ProtectedData<CargoWiseWriterLoginCredentials>(Guid.Parse("11111111-0000-0000-0000-000000000000"), "Writer", DateTimeOffset.Parse("2024/04/24"), DateTimeOffset.Parse("2025/05/25"), new CargoWiseWriterLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest1" });
			var protectedData2 = new ProtectedData<CargoWiseReaderLoginCredentials>(Guid.Parse("11111111-0000-0000-0000-000000000000"), "Reader", DateTimeOffset.Parse("2022/02/22"), DateTimeOffset.Parse("2023/03/23"), new CargoWiseReaderLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest2" });

			var protectedDataStateService = serviceProvider.GetRequiredService<IProtectedDataStateService>();

			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData1, comments: "Test Comments");
			protectedDataStateService.MarkProtectedDataAsActive(Db.ServerName, Db.DatabaseName, protectedData1, comments: "GoodStuff");
			protectedDataStateService.MarkProtectedDataAsActive(Db.ServerName, Db.DatabaseName, protectedData2, comments: "BadStuff");

			var dtResult = DataUtils.GetDataTableFromQuery(Db.Connection, "Select * from dbo.stmProtectedDataState where PDS_ProtectedDataID = '11111111-0000-0000-0000-000000000000'");
			AssertEquals(dtResult.Rows.Count, 1);
			var row = dtResult.Rows[0];

			var activationInfo = row["pds_activationInfo"] as string;
			AssertContains("GoodStuff", activationInfo);
			AssertNotContains("BadStuff", activationInfo);
			AssertEquals("Writer", row["PDS_Type"]);
			AssertEquals(DateTimeOffset.Parse("2024/04/24"), row["PDS_ValidFrom"]);
			AssertEquals(DateTimeOffset.Parse("2025/05/25"), row["PDS_ValidTo"]);
		}

		public void TestProtectedDataStatusService_GetActiveProtectedData_UnsupportedDatabase_Success()
		{
			pdsMock.As<IProtectedDataServiceWithDefaults>().Setup(x => x.GetDefaultProtectedDataIdFor("Writer")).Returns(protectedData1.Id).Verifiable();

			var protectedDataStateServiceMock = new Mock<ProtectedDataStateService>(() => new ProtectedDataStateService(
				serviceProvider.GetRequiredService<IProtectedDataServiceFactory>(),
				serviceProvider.GetRequiredService<ProtectedDataServiceCapabilities>(),
				serviceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>(),
				serviceProvider.GetRequiredService<IEnvironment>()));

			protectedDataStateServiceMock.Setup(x => x.DatabaseSchemaSupportsProtectedDataState(It.IsAny<ISqlExecutionContext>(), It.IsAny<string>())).Returns(false);
			var protectedDataStateService = protectedDataStateServiceMock.Object;

			var result1 = protectedDataStateService.GetActiveProtectedData<CargoWiseWriterLoginCredentials>(Db.ServerName, Db.DatabaseName);

			AssertEquals("PasswordForTest1", result1.Password);
		}

		public void TestProtectedDataStatusService_GetActiveProtectedData_SupportedDatabase_Success()
		{
			pdsMock.As<IProtectedDataServiceWithDefaults>().Setup(x => x.GetDefaultProtectedDataIdFor("Writer")).Returns(protectedData1.Id).Verifiable();

			var protectedDataStateServiceMock = new Mock<ProtectedDataStateService>(() => new ProtectedDataStateService(
				serviceProvider.GetRequiredService<IProtectedDataServiceFactory>(),
				serviceProvider.GetRequiredService<ProtectedDataServiceCapabilities>(),
				serviceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>(),
				serviceProvider.GetRequiredService<IEnvironment>()));

			//var protectedDataStateServiceMock = new Mock<ProtectedDataStateService>(pdsFactoryMock.Object, new ProtectedDataServiceCapabilities());
			protectedDataStateServiceMock.Setup(x => x.DatabaseSchemaSupportsProtectedDataState(It.IsAny<ISqlExecutionContext>(), It.IsAny<string>())).Returns(true);

			var protectedDataStateService = protectedDataStateServiceMock.Object;

			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData1, comments: "-");
			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData2, comments: "-");

			protectedDataStateService.MarkProtectedDataAsActive(Db.ServerName, Db.DatabaseName, protectedData1, comments: "-");
			var result1 = protectedDataStateService.GetActiveProtectedData<CargoWiseWriterLoginCredentials>(Db.ServerName, Db.DatabaseName);

			protectedDataStateService.MarkProtectedDataAsActive(Db.ServerName, Db.DatabaseName, protectedData2, comments: "-");
			var result2 = protectedDataStateService.GetActiveProtectedData<CargoWiseWriterLoginCredentials>(Db.ServerName, Db.DatabaseName);

			AssertEquals("PasswordForTest1", result1.Password);
			AssertEquals("PasswordForTest2", result2.Password);
		}

		public void TestProtectedDataStatusService_GetActiveProtectedData_NoActiveItem()
		{
			var protectedDataStateService = serviceProvider.GetRequiredService<IProtectedDataStateService>();

			Db.Connection.ExecuteNonQuery("DELETE dbo.stmProtectedDataState");
			AssertExceptionThrown<Exception>("There is no active protected data item of the specified type (Writer) available",
				() =>
				{
					_ = protectedDataStateService.GetActiveProtectedData<CargoWiseWriterLoginCredentials>(Db.ServerName, Db.DatabaseName);
				});
		}

		public void TestProtectedDataStatusService_GetActiveProtectedData_TypeMismatch()
		{
			var protectedDataStateService = serviceProvider.GetRequiredService<IProtectedDataStateService>();

			var id = Guid.Parse("00000000-0000-0000-0000-000000000001");

			var pd1 = new ProtectedData<CargoWiseWriterLoginCredentials>(
				id,
				"Writer",
				DateTimeOffset.Now,
				DateTimeOffset.Now.AddHours(2),
				new CargoWiseWriterLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest1" });

			var pd2 = new ProtectedData<CargoWiseReaderLoginCredentials>(
				id,
				"Reader",
				DateTimeOffset.Now,
				DateTimeOffset.Now.AddHours(2),
				new CargoWiseReaderLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest1" });

			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, pd1, "-");
			protectedDataStateService.MarkProtectedDataAsActive(Db.ServerName, Db.DatabaseName, pd1, "-");
			pdsMock.Setup(x => x.LoadSecret(id)).Returns(pd2);

			AssertExceptionThrown<Exception>("An unexpected protected data item was found that does not match the requested type (Writer).",
				() =>
				{
					_ = protectedDataStateService.GetActiveProtectedData<CargoWiseWriterLoginCredentials>(Db.ServerName, Db.DatabaseName);
				});
		}

		public void TestProtectedDataStatusService_GetActiveProtectedData_OldDatabseAlwaysReturnsDefaultCredentials()
		{
			// Arrange
			var defaultId = SystemProtectedDataServiceWithDefaults.DefaultCargoWiseWriterLoginCredentialsId;
			var otherId = Guid.NewGuid();

			var protectedData1 = new ProtectedData<CargoWiseWriterLoginCredentials>(defaultId, "Writer", DateTimeOffset.Now, DateTimeOffset.Now.AddHours(2), new CargoWiseWriterLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest1" });
			var protectedData2 = new ProtectedData<CargoWiseWriterLoginCredentials>(otherId, "Writer", DateTimeOffset.Now, DateTimeOffset.Now.AddHours(2), new CargoWiseWriterLoginCredentials() { Database = "TESTDB", Password = "PasswordForTest1" });

			pdsMock.Setup(x => x.LoadSecret(defaultId)).Returns(protectedData1).Verifiable(Times.Once);
			pdsMock.Setup(x => x.LoadSecret(otherId)).Returns(protectedData2).Verifiable(Times.Once);
			pdsMock.As<IProtectedDataServiceWithDefaults>().Setup(x => x.GetDefaultProtectedDataIdFor("Writer"))
				.Returns(defaultId);

			var protectedDataStateService = serviceProvider.GetRequiredService<IProtectedDataStateService>();

			Db.Connection.ExecuteNonQuery("DELETE dbo.stmProtectedDataState");
			protectedDataStateService.RegisterProtectedData(Db.ServerName, Db.DatabaseName, protectedData2, "-");
			protectedDataStateService.MarkProtectedDataAsActive(Db.ServerName, Db.DatabaseName, protectedData2, "-");

			// Act
			DbRegistry.DatabaseMajorSchemaVersion.SaveValue(ProtectedDataStateService.MinimumDatabaseMajorSchemaVersionToSupportPDSState, Db.Connection);
			var result1 = protectedDataStateService.GetActiveProtectedData<CargoWiseWriterLoginCredentials>(Db.ServerName, Db.DatabaseName);

			DbRegistry.DatabaseMajorSchemaVersion.SaveValue(ProtectedDataStateService.MinimumDatabaseMajorSchemaVersionToSupportPDSState - 1, Db.Connection);
			var result2 = protectedDataStateService.GetActiveProtectedData<CargoWiseWriterLoginCredentials>(Db.ServerName, Db.DatabaseName);

			// Asssert
			AssertEquals(protectedData2.Data, result1);
			AssertEquals(protectedData1.Data, result2);
			pdsMock.Verify(x => x.LoadSecret(defaultId));
			pdsMock.Verify(x => x.LoadSecret(otherId));
		}
	}
}
