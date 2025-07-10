using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbPermissionClassTest : TestCase
	{
		public void TestCode()
		{
			AssertEquals("Database.Code", DbPermissionClass.Constants.Database.Code, DbPermissionClass.Database.Code);
			AssertEquals("Schema.Code", DbPermissionClass.Constants.Schema.Code, DbPermissionClass.Schema.Code);
			AssertEquals("User.Code", DbPermissionClass.Constants.User.Code, DbPermissionClass.User.Code);
			AssertEquals("Login.Code", DbPermissionClass.Constants.Login.Code, DbPermissionClass.Login.Code);
		}

		public void TestDescription()
		{
			AssertEquals("Database.Description", DbPermissionClass.Constants.Database.Description, DbPermissionClass.Database.Description);
			AssertEquals("Schema.Description", DbPermissionClass.Constants.Schema.Description, DbPermissionClass.Schema.Description);
			AssertEquals("User.Description", DbPermissionClass.Constants.User.Description, DbPermissionClass.User.Description);
			AssertEquals("Login.Description", DbPermissionClass.Constants.Login.Description, DbPermissionClass.Login.Description);
		}

		public void TestDatabase()
		{
			AssertEquals("Database.Code", DbPermissionClass.Constants.Database.Code, DbPermissionClass.Database.Code);
			AssertEquals("Database.Description", DbPermissionClass.Constants.Database.Description, DbPermissionClass.Database.Description);
		}

		public void TestSchema()
		{
			AssertEquals("Schema.Code", DbPermissionClass.Constants.Schema.Code, DbPermissionClass.Schema.Code);
			AssertEquals("Schema.Description", DbPermissionClass.Constants.Schema.Description, DbPermissionClass.Schema.Description);
		}

		public void TestUser()
		{
			AssertEquals("User.Code", DbPermissionClass.Constants.User.Code, DbPermissionClass.User.Code);
			AssertEquals("User.Description", DbPermissionClass.Constants.User.Description, DbPermissionClass.User.Description);
		}

		public void TestLogin()
		{
			AssertEquals("Login.Code", DbPermissionClass.Constants.Login.Code, DbPermissionClass.Login.Code);
			AssertEquals("Login.Description", DbPermissionClass.Constants.Login.Description, DbPermissionClass.Login.Description);
		}
	}
}
