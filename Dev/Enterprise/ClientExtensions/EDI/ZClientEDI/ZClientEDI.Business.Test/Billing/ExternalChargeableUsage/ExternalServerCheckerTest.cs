using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.DataProtection.TestFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ZClientEDI.Business.Billing;

namespace Enterprise.Client.EDI.Billing.Test
{
	public class ExternalServerCheckerTest : TestCaseWithFactory
	{
		public void TestCheck()
		{
			CombineAssertions(() =>
			{
				//1 . all blank values.
				AssertEquals(@"*** Checking... WiseTech Global Client Extensions/Licence Billing/External Servers/Billing Testing
Connecting... Server:, Database:, UserID:x, Password:*
Error: Database connection information is incomplete. Please ensure all required fields (data source, initial catalog, user ID, and password) are provided.


*** Checking... WiseTech Global Client Extensions/Licence Billing/External Servers/eRouter Usage
Connecting... Server:, Database:, UserID:x, Password:*
Error: Database connection information is incomplete. Please ensure all required fields (data source, initial catalog, user ID, and password) are provided.


*** Checking... WiseTech Global Client Extensions/Licence Billing/External Servers/Fax Usage
Connecting... Server:, Database:, UserID:x, Password:*
Error: Database connection information is incomplete. Please ensure all required fields (data source, initial catalog, user ID, and password) are provided.


*** Checking... WiseTech Global Client Extensions/Licence Billing/External Servers/Hosting Stats Usage
Connecting... Server:, Database:, UserID:x, Password:*
Error: Database connection information is incomplete. Please ensure all required fields (data source, initial catalog, user ID, and password) are provided.


", Check("", "", "x", "x"));

			//2 . invalid user
			AssertContains("Error: Login failed for user ", Check(Db.Connection.ServerName, "master", Guid.NewGuid().ToString(), Guid.NewGuid().ToString()));

			//3 . valid user.
			var anyValidLogin = DataProtectionTestBed.Current.GetExpectedCredentials(TestEnvironmentWellKnownSecret.TestServerOdysseyAdmin);
			var log = Check(Db.Connection.ServerName, "master", anyValidLogin.UserName, anyValidLogin.Password);
			AssertContains("Permission granted to query data from table: [dbo].[spt_monitor]", log);
			AssertNotContains("Error: ", log);

			var connStr = "a=1;b=2;c=3;";
			AssertContains("Error: Keyword not supported: 'a'.", Check(connStr), true);

			connStr = $"Server=localhost;Database={ZGuid.NewZGuid()};User Id={ZGuid.NewZGuid()};Password={ZGuid.NewZGuid()};";
			AssertContains("Error: You have tried to establish a DB Connection using a generic machine name. (localhost) Please use your real machine name in place of this.", Check(connStr), true);

			connStr = $"Server={Db.Connection.ServerName};Database=master;User Id={anyValidLogin.UserName};Password={anyValidLogin.Password};";
			log = Check(connStr);

			AssertContains("Connecting... Server:", log, true);
			AssertContains("Permission granted to query data from table: [dbo].[spt_monitor]", log, true);
			AssertNotContains("Error: ", log);
			});
		}

		static string Check(string serverName, string dbName, string userName, string password)
		{
			var login = new ServerUsernamePasswordConfiguration() { UserName = userName, Password = password, ConfirmPassword = password };
			foreach (var connectionConfig in EDIDataRegistry.Instance.GetAllItems()
							.Where(x => x.Category.Contains(EDIDataRegistry.ExternalServersCategory))
							.OrderBy(x => x.Category)
							.GroupBy(x => x.Category).ToArray())
			{
				connectionConfig.OfType<StringRegistryItem>().Single(x => x.Name.EndsWith("ServerName")).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, serverName);
				connectionConfig.OfType<StringRegistryItem>().Single(x => x.Name.EndsWith("DBName") || x.Name.EndsWith("DatabaseName")).SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dbName);
				connectionConfig.OfType<ServerUsernamePasswordConfigurationRegistryItem>().Single().SetValue(Guid.Empty, Guid.Empty, Guid.Empty, login);
			}

			var logger = new SimpleLogger();
			new ExternalServerChecker(logger).Check();
			return logger.ToString();
		}

		static string Check(string connStr)
		{
			var logger = new SimpleLogger();
			new ExternalServerChecker(logger).Check(connStr);
			return logger.ToString();
		}
	}
}
