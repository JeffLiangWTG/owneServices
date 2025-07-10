using System;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace CargoWise.Bi.Registration.Common.Testing
{
	public class BiReportUserTest : TransactionedTestCase
	{
		public class BiReportUserForTest : BiReportUser
		{
		}

		public void TestUpdatingRegistryUpdatesBiReportUser()
		{
			var reportUser = new BiReportUserForTest();
			var originalUsername = reportUser.UserName;

			var biReportCredential = new BiReportCredential();
			biReportCredential.Domain = "CORP";
			biReportCredential.UserName = "NonExistingUser";
			biReportCredential.Password = "Test123456";
			SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "testvalue");
			SystemDataRegistry.Instance.BiReportUserCredential.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, biReportCredential);

			reportUser = new BiReportUserForTest();
			Assert("Username has changed", reportUser.UserName != originalUsername);
			AssertEquals(reportUser.UserName, biReportCredential.UserName);
		}
	}
}
