using System;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.Administration.SqlServer;
using Enterprise.AlwaysOn.Setup;
using Moq;
using Moq.Protected;
using DbSecurity = Enterprise.AlwaysOn.Setup.DbSecurity;

namespace Enterprise.AlwaysOn.Testing
{
	class DbServerInstanceTest : AlwaysOnTestFixture
	{
		public void TestSecondaryServerInstance_OdysseyAdminLogin()
		{
			var masterOdysseyAdminLogin = new DbLoginInfo(OdysseyAdminCredentials.AdminUserName, "S", "master", DbSecurity.OdysseyAdminLoginSid, DbSecurity.OdysseyAdminPwdHash, true);
			var serverInfo = new SqlServerInfo(Db.ServerName);
			var primaryServerInstanceMock = new Mock<IPrimaryServerInstance>();
			primaryServerInstanceMock.Setup(x => x.ServerInfo).Returns(serverInfo);

			var secondaryServerInstanceMock = new Mock<SecondaryServerInstance>(serverInfo, primaryServerInstanceMock.Object) { CallBase = true };
			var serverInstance = secondaryServerInstanceMock.As<IDbServerInstance>().Object;
			secondaryServerInstanceMock.Protected().Setup("LoadUnsafe", ItExpr.IsAny<ISqlExecutionContext>()).Verifiable();

			serverInstance.Load();

			Assert(serverInstance.OdysseyAdminLogin != default);
			AssertEquals(serverInstance.OdysseyAdminLogin.LoginName, OdysseyAdminCredentials.AdminUserName);

			secondaryServerInstanceMock.Verify();
			primaryServerInstanceMock.Verify();
		}

		public void TestPrimaryServerInstance_OdysseyAdminLogin()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);

			var primaryServerInstanceMock = new Mock<PrimaryServerInstance>(serverInfo) { CallBase = true };
			var serverInstance = primaryServerInstanceMock.As<IDbServerInstance>().Object;
			primaryServerInstanceMock.Protected().Setup("LoadUnsafe", ItExpr.IsAny<ISqlExecutionContext>()).Verifiable();

			serverInstance.Load();

			Assert(serverInstance.OdysseyAdminLogin != default);
			AssertEquals(serverInstance.OdysseyAdminLogin.LoginName, OdysseyAdminCredentials.AdminUserName);
			primaryServerInstanceMock.Verify();
		}

		public void TestPrimaryServerInstance_OnSuccessfulLoad()
		{
			const string expectedWarningMessage = @"The operation cannot be performed on database Ody because it is involved in a database mirroring session or an availability group. Some operations are not allowed on a database that is participating in a database mirroring session or in an availability group.
ALTER DATABASE statement failed.";

			var serverInfo = new SqlServerInfo(Db.ServerName);

			var primaryServerInstanceMock = new Mock<PrimaryServerInstance>(serverInfo) { CallBase = true };
			var serverInstance = primaryServerInstanceMock.As<IDbServerInstance>().Object;
			primaryServerInstanceMock.Protected().Setup("LoadUnsafe", ItExpr.IsAny<ISqlExecutionContext>()).Verifiable();
			primaryServerInstanceMock.Protected()
				.Setup("InvokeAdditionalTasksAfterLoad", ItExpr.IsAny<ISqlExecutionContext>())
				.Throws(new Exception(expectedWarningMessage))
				.Verifiable();

			serverInstance.Load();

			AssertNoExceptionThrown(() => primaryServerInstanceMock.Verify());
		}

		public void TestChangeInstanceLevelAuthorizationsToSysAdmin_OnSuccessfulLoad()
		{
			var serverInfo = new SqlServerInfo(Db.ServerName);

			var instanceMock = new Mock<PrimaryServerInstance>(serverInfo) { CallBase = true };
			var serverInstance = instanceMock.As<IDbServerInstance>().Object;
			instanceMock.Protected().Setup("LoadUnsafe", ItExpr.IsAny<ISqlExecutionContext>()).Verifiable();
			instanceMock.Protected()
				.Setup("ChangeInstanceLevelAuthorizationsToSysAdmin", ItExpr.IsAny<ISqlExecutionContext>())
				.Verifiable();

			serverInstance.Load();

			AssertNoExceptionThrown(() => instanceMock.Verify());
		}
	}
}
