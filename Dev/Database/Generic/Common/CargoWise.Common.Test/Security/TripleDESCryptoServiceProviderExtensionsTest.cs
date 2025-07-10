using System;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class TripleDESCryptoServiceProviderExtensionsTest : TestCase
	{
		public void TestEncryptorDecryptor()
		{
			var provider = TripleDES.Create();
			provider.InitializeForCargoWise();
			var original = Encoding.ASCII.GetBytes("The byte[] to encrypt");
			var encrypted = provider.CreateEncryptor().TransformFinalBlock(original, 0, original.Length);
			AssertEquals("Encrypted value (which should not change or existing client data will break)", "L2M6DoWLU76kz5/opHbcDvUGbxVw1ysT", Convert.ToBase64String(encrypted));
			var decrypted = provider.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
			AssertEquals("Decrypted value should be the same as the encrypted value", Encoding.ASCII.GetString(original), Encoding.ASCII.GetString(decrypted));
		}

		public void TestCreateSecurityHashAndIsValidSecurityHash()
		{
			var provider = TripleDES.Create();
			provider.InitializeForCargoWise();
			var data = Encoding.ASCII.GetBytes("The byte[] to create a security hash for");
			var securityHash = provider.CreateSecurityHash(data);
			AssertEquals("Security hash (which should not change or existing client data will break)", "OHss1oL4BQHqx3T3fzMU2xSYvO/r+yao", Convert.ToBase64String(securityHash));
			AssertEquals("Generated security hash should be valid", true, provider.IsValidSecurityHash(data, securityHash));
			securityHash[securityHash.Length - 2] = 1;
			AssertEquals("Tampered security hash should be invalid", false, provider.IsValidSecurityHash(data, securityHash));
		}
	}
}