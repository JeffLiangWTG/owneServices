using System;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing;

sealed class NoDataProtectorServiceTest : TestCase
{
	public void TestEncrypt_Null()
	{
		TestEncrypt(null);
	}

	public void TestEncrypt_Empty()
	{
		TestEncrypt(string.Empty);
	}

	public void TestEncrypt_Valid()
	{
		TestEncrypt("testData");
	}

	public void TestDecrypt_Null()
	{
		TestDecrypt(null);
	}

	public void TestDecrypt_Empty()
	{
		TestDecrypt(string.Empty);
	}

	public void TestDecrypt_Valid()
	{
		TestDecrypt("testData");
	}

	void TestEncrypt(string testData)
	{
		var dataProtectorService = new NoProtectionDataProtectorService();
		var encryptedString = dataProtectorService.Encrypt(testData);
		AssertNull(encryptedString);
	}

	void TestDecrypt(string testData)
	{
		var dataProtectorService = new NoProtectionDataProtectorService();
		var e = AssertExceptionThrown<NotImplementedException>(() => dataProtectorService.Decrypt(testData));
		AssertEquals("Decrypt is not implemented", e.Message);
	}
}
