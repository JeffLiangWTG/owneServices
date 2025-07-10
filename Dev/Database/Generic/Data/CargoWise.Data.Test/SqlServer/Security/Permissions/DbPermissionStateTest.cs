using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class DbPermissionStateTest : TestCase
	{
		public void TestCode()
		{
			AssertEquals("Deny.Code", DbPermissionState.Constants.Deny.Code, DbPermissionState.Deny.Code);
			AssertEquals("Grant.Code", DbPermissionState.Constants.Grant.Code, DbPermissionState.Grant.Code);
		}

		public void TestDescription()
		{
			AssertEquals("Deny.Description", DbPermissionState.Constants.Deny.Description, DbPermissionState.Deny.Description);
			AssertEquals("Grant.Description", DbPermissionState.Constants.Grant.Description, DbPermissionState.Grant.Description);
		}

		public void TestDeny()
		{
			AssertEquals("Deny.Code", DbPermissionState.Constants.Deny.Code, DbPermissionState.Deny.Code);
			AssertEquals("Deny.Description", DbPermissionState.Constants.Deny.Description, DbPermissionState.Deny.Description);
		}

		public void TestGrant()
		{
			AssertEquals("Grant.Code", DbPermissionState.Constants.Grant.Code, DbPermissionState.Grant.Code);
			AssertEquals("Grant.Description", DbPermissionState.Constants.Grant.Description, DbPermissionState.Grant.Description);
		}

		public void TestGetState()
		{
			var denyPermission = DbPermissionState.GetState("D");
			AssertEquals(DbPermissionState.Deny.Code, denyPermission.Code);
			AssertEquals(DbPermissionState.Deny.Description, denyPermission.Description);

			var grantPermission = DbPermissionState.GetState("G");
			AssertEquals(DbPermissionState.Grant.Code, grantPermission.Code);
			AssertEquals(DbPermissionState.Grant.Description, grantPermission.Description);

			AssertExceptionThrown<NotImplementedException>(() => DbPermissionState.GetState("R"));
		}
	}
}
