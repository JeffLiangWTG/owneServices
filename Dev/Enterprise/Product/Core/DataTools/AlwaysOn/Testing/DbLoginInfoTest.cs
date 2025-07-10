using System;
using Enterprise.AlwaysOn.Setup;
using NUnit.Framework;

namespace Enterprise.AlwaysOn.Testing
{
	class DbLoginInfoTest : TestCase
	{
		public void TestAttributes()
		{
			var dbLogin = new DbLoginInfo("Login01", "S", "SomeDb", "0x_mock_sid", "0x_mock_pwd_hash");
			AssertEquals("LoginName", "Login01", dbLogin.LoginName);
			AssertEquals("Type", DbLoginInfo.LoginType.SQL, dbLogin.Type);
			AssertEquals("DefaultDatabase", "SomeDb", dbLogin.DefaultDatabase);
			AssertEquals("LoginSid", "0x_mock_sid", dbLogin.LoginSid);
			AssertEquals("PwdHash", "0x_mock_pwd_hash", dbLogin.PwdHash);
		}

		public void TestConstructWithInvalidLoginType()
		{
#if NETFRAMEWORK
			var expectedMessage = "Invalid login type [Z]. Valid values are [S, U, G].\r\nParameter name: type";
#else
			var expectedMessage = "Invalid login type [Z]. Valid values are [S, U, G]. (Parameter 'type')";
#endif
			AssertExceptionThrown(
				typeof(ArgumentException),
				expectedMessage,
				() => new DbLoginInfo("name", "Z", null, null, null));
		}

		public void TestConstructSqlLoginWithInvalidSid()
		{
#if NETFRAMEWORK
			var expectedMessage = "SID is mandatory for SQL logins.\r\nParameter name: loginSid";
#else
			var expectedMessage = "SID is mandatory for SQL logins. (Parameter 'loginSid')";
#endif
			AssertExceptionThrown(
				typeof(ArgumentException),
				expectedMessage,
				() => new DbLoginInfo("name", "S", "adb", null, "0x0200017422689ED954059C41A7DB8AA3E84F6C73B01100D82C32EF42644908917D3D963F17CAF656490CD02B1F8FCEACFAE325F9089B29B541E695EF104D9015A038EF36ADA7"));

			AssertNoExceptionThrown(
				"SID is not required nor validated for Windows users/groups => No error expected.",
				() => new DbLoginInfo("name", "U", "adb", null, ""));
		}

		public void TestConstructSqlLoginWithInvalidPwdHash()
		{
			AssertNoExceptionThrown(
				"PasswordHash is not required nor validated for Windows users/groups => No error expected.",
				() => new DbLoginInfo("name", "G", "adb", " ", null));
		}
	}
}
