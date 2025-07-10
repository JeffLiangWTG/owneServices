using System;
using System.Security.Principal;
using System.Text;
using CargoWise.eHub.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.RemotePrinting.Server.RPSCore;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.RemotePrinting.Server.Testing;

sealed class CustomsServerEncryptorTest : TestCaseWithFactory
{
	public void TestEncryptWithoutCredential()
	{
		var mockIdentity = new Mock<IIdentity>();
		mockIdentity.Setup(x => x.Name).Returns("CWTest");

		var identity = mockIdentity.Object;

		var normalPassword = "@TestPassword_0123";

		var authenticationMock = new Mock<IAuthentication>();
		authenticationMock.Setup(x => x.ApplicationUser).Returns(string.Empty);
		authenticationMock.Setup(x => x.ApplicationPwd).Returns(string.Empty);

		var authentication = authenticationMock.Object;

		CombineAssertions(() =>
		{
			AssertEquals("NULL String", string.Empty, CustomsServerEncryptor.Encrypt(identity, authentication, (string)null));
			AssertEquals("Empty String", string.Empty, CustomsServerEncryptor.Encrypt(identity, authentication, string.Empty));

			AssertEquals("NULL Byte Array", string.Empty, CustomsServerEncryptor.Encrypt(identity, authentication, (byte[])null));
			AssertEquals("Empty Byte Array", string.Empty, CustomsServerEncryptor.Encrypt(identity, authentication, Array.Empty<byte>()));

			var expectedPassword = Convert.ToBase64String(Encoding.UTF8.GetBytes(normalPassword));
			AssertEquals("Normal Password", expectedPassword, CustomsServerEncryptor.Encrypt(identity, authentication, normalPassword));
		});
	}

	public void TestEncryptWithSupportToken()
	{
		var validToken = CWSupportLoginToken.TokenForTest;
		var authString = Authentication.SupportUserPrefix + "CW2025";

		var mockIdentity = new Mock<IIdentity>();
		mockIdentity.Setup(x => x.Name).Returns(authString);

		var identity = mockIdentity.Object;

		var credential = new CodeDescriptionPairList();
		credential.AddPair("CWAlternativeUser", "CWAlternativePwd");

		var authenticationMock = new Mock<IAuthentication>();
		authenticationMock.Setup(x => x.ApplicationUser).Returns("CWAppUser");
		authenticationMock.Setup(x => x.ApplicationPwd).Returns("CWAppPwd");
		authenticationMock.Setup(x => x.AlternativeCredentials).Returns(credential);
		var authentication = authenticationMock.Object;

		AssertStartsWith("Should encrypt the text with the prefix - '@'.", "@", CustomsServerEncryptor.Encrypt(identity, authentication, "Test123"));
	}

	public void TestEncryptWithAlternativeCredentials()
	{
		var mockIdentity = new Mock<IIdentity>();
		mockIdentity.Setup(x => x.Name).Returns("CWTest");

		var identity = mockIdentity.Object;

		var credential = new CodeDescriptionPairList();
		credential.AddPair("CWTest", "CW2025");

		var authenticationMock = new Mock<IAuthentication>();
		authenticationMock.Setup(x => x.ApplicationUser).Returns("CWAppUser");
		authenticationMock.Setup(x => x.ApplicationPwd).Returns("CWAppPwd");
		authenticationMock.Setup(x => x.AlternativeCredentials).Returns(credential);

		var authentication = authenticationMock.Object;

		AssertEncrypt("*", identity, authentication);
	}

	public void TestEncryptWithApplicationCredential()
	{
		var mockIdentity = new Mock<IIdentity>();
		mockIdentity.Setup(x => x.Name).Returns("CWTest");

		var identity = mockIdentity.Object;

		var authenticationMock = new Mock<IAuthentication>();
		authenticationMock.Setup(x => x.ApplicationUser).Returns("CWTest");
		authenticationMock.Setup(x => x.ApplicationPwd).Returns("CW2025");

		var authentication = authenticationMock.Object;

		AssertEncrypt("#", identity, authentication);
	}

	void AssertEncrypt(string prefix, IIdentity identity, IAuthentication authentication)
	{
		CombineAssertions(() =>
		{
			AssertEquals("NULL String", string.Empty, CustomsServerEncryptor.Encrypt(identity, authentication, (string)null));
			AssertEquals("Empty String", string.Empty, CustomsServerEncryptor.Encrypt(identity, authentication, string.Empty));

			AssertEquals("NULL Byte Array", string.Empty, CustomsServerEncryptor.Encrypt(identity, authentication, (byte[])null));
			AssertEquals("Empty Byte Array", string.Empty, CustomsServerEncryptor.Encrypt(identity, authentication, Array.Empty<byte>()));

			AssertEquals("Short Special String", prefix + "DMTVJGiPiizKPVhM2qmieQ==", CustomsServerEncryptor.Encrypt(identity, authentication, "@"));
			AssertEquals("Short Normal String", prefix + "R75pXlNwjxfU3LuONFmBCg==", CustomsServerEncryptor.Encrypt(identity, authentication, "A"));
			AssertEquals("Normal String", prefix + "mI83euebgc0TV46feKjNrQ==", CustomsServerEncryptor.Encrypt(identity, authentication, "Test123"));
			AssertEquals("Complex String", prefix + "JVTLVG2Iv/RNdFs52XWJbEBud+BhMRyhJUYOMk0SHa8=", CustomsServerEncryptor.Encrypt(identity, authentication, "@TestPassword_0123"));

			var expectedPassword = prefix + @"Uq8fv4H0tE7GS1+Qes6U1TRTRnd5QXDoa5byk5tfGU+GZIv9FSztMl0zj96F3"
				+ @"Pj98w4dXmSQlidAt5wQQvgSQM+Dgd7S7SmLoxr7hk2x0e70FHlx1Zw3kCjiL3UEiGD5HaUdNX4icBp"
				+ @"B40dIdkgWScNe1Y3hUgQlLRgLdGcHUvv/2ZUUXW67pmVm3eNGbe9DptoEONEEohJKaQPyR8ABqJdfT"
				+ @"qe3HIMPG8BQK98Gnf7nue3lWFgxB4nInykw+fcF";

			AssertEquals("Node Password", expectedPassword, CustomsServerEncryptor.Encrypt(identity, authentication, SHA512Encryptor.Encrypt("CUSTST")));
		});
	}
}
