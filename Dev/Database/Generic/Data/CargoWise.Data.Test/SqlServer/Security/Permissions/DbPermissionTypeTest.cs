using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbPermissionTypeTest : TestCase
	{
		public void TestCode()
		{
			AssertEquals("Select.Code", DbPermissionType.Constants.Select.Code, DbPermissionType.Select.Code);
			AssertEquals("Impersonate.Code", DbPermissionType.Constants.Impersonate.Code, DbPermissionType.Impersonate.Code);
		}

		public void TestDescription()
		{
			AssertEquals("Select.Description", DbPermissionType.Constants.Select.Description, DbPermissionType.Select.Description);
			AssertEquals("Impersonate.Description", DbPermissionType.Constants.Impersonate.Description, DbPermissionType.Impersonate.Description);
		}

		public void TestSelect()
		{
			AssertEquals("Select.Code", DbPermissionType.Constants.Select.Code, DbPermissionType.Select.Code);
			AssertEquals("Select.Description", DbPermissionType.Constants.Select.Description, DbPermissionType.Select.Description);
		}

		public void TestImpersonate()
		{
			AssertEquals("Impersonate.Code", DbPermissionType.Constants.Impersonate.Code, DbPermissionType.Impersonate.Code);
			AssertEquals("Impersonate.Description", DbPermissionType.Constants.Impersonate.Description, DbPermissionType.Impersonate.Description);
		}
	}
}
