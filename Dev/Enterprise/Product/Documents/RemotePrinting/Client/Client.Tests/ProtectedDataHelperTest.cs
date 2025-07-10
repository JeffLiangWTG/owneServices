using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class ProtectedDataHelperTest : TestCase
	{
		public void TestProtectAndUnprotect()
		{
			var testPassword = "123456";
			var protectedPwd = ProtectedDataHelper.Protect(testPassword);
			AssertNotEquals(testPassword, protectedPwd);

			var unProtectedPwd = ProtectedDataHelper.Unprotect(protectedPwd);
			AssertEquals(testPassword, unProtectedPwd);

			AssertEquals(testPassword, ProtectedDataHelper.Unprotect(testPassword));
		}
	}
}
