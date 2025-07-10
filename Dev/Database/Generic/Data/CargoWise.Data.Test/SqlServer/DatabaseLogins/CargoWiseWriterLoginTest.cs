using System.Data;
using System.Linq;
using CargoWise.DataProtection;
using Microsoft.Extensions.DependencyInjection;

namespace CargoWise.Data.Testing
{
	sealed class CargoWiseWriterLoginTest : DatabaseLoginTestCase
	{
		public void TestUserDetailsMatch()
		{
			var login = DatabaseLogin;
			var baseName = TestAdminConnection.CurrentDatabase;

			CombineAssertions("The login details must precisely match the legacy values", () =>
			{
				AssertEquals("Login Name does not match", baseName + "_CargoWiseWriterLogin", login.LoginName);
				AssertEquals("Login Suffix does not match", "CargoWiseWriterLogin", login.LoginSuffix);
			});
		}

		[UseSnapshotProtection]
		public void TestCannotAccessHrm()
		{
				AssertExceptionThrown("The CargoWiseWriterLogin should not have access to sensitive data", typeof(SqlException), "The SELECT permission was denied on the object 'GlbStaffRemuneration'", SelectSensitiveInfo, assertStartsWith: true);

				void SelectSensitiveInfo()
					=> ExecuteScalarWithLogin("SELECT COUNT(*) FROM hrm.GlbStaffRemuneration");
		}

		[UseSnapshotProtection]
		public void TestCanAccessReferenceData()
		{
				AssertNoExceptionThrown("We should be able to access refdb data", () => ExecuteScalarWithLogin("select top 1 ZZD_Code from RefDatabase_RefCusCodeList"));
		}

		[UseSnapshotProtection]
		public void TestCanAccessStmUpgrade()
		{
			AssertNoExceptionThrown("We should be able to access StmUpgrade. This is required to upgrade web services.", () => ExecuteScalarWithLogin("select top 1 SZ_PK from dbo.StmUpgrade"));
		}

		object ExecuteScalarWithLogin(string query)
		{
			var login = DatabaseLogin;
			login.EnsureLogin();

			var baseType = login.GetType().BaseType;
			if (!baseType.IsGenericType)
			{
				baseType = baseType.BaseType;
			}

			var credentialsType = baseType.GetGenericArguments()[0];
			var pdsFactory = ProtectedDataService.GlobalServiceProvider.GetRequiredService<IProtectedDataServiceFactory>();
			var pds = pdsFactory.CreateSystemService(TestConnection.ServerName, TestConnection.CurrentDatabase);
			var connectionProvider = ProtectedDataService.GlobalServiceProvider.GetRequiredService<ISqlConnectionProvider>();

			var connectionFactoryMethodInfo = typeof(ISqlConnectionProvider)
				.GetMethods().Where(m => m.Name == nameof(ISqlConnectionProvider.OpenNewSqlConnection) && m.IsGenericMethod)
				.Single().MakeGenericMethod(credentialsType);

			var csBuilder = (SqlConnectionStringBuilder builder) =>
			{
				builder.DataSource = Db.ServerName;
				builder.InitialCatalog = Db.DatabaseName;
				builder.IntegratedSecurity = false;
#if NETCOREAPP
				builder.TrustServerCertificate = true;
#endif
			};

			using var connection = (IDbConnection)connectionFactoryMethodInfo.Invoke(connectionProvider, new object[] { pds, csBuilder });

			var cmd = connection.CreateCommand();
			cmd.CommandText = query;

			return cmd.ExecuteScalar();
		}

		protected override DatabaseLogin DatabaseLogin => new CargoWiseWriterLogin(TestAdminConnection);
	}
}
