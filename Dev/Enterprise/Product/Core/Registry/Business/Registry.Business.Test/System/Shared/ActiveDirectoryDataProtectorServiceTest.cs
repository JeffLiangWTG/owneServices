using System;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text.RegularExpressions;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

[assembly: UsesConstants(typeof(TestConstants))]
namespace Enterprise.Registry.Business.Testing
{
	sealed class ActiveDirectoryDataProtectorServiceTest : TestCase
	{
		static readonly SecurityIdentifier WorldSid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
		static readonly SecurityIdentifier NullSid = new SecurityIdentifier(WellKnownSidType.NullSid, null);

		public void TestConstructor_NullSid_Throws()
		{
			var e = AssertExceptionThrown<ArgumentNullException>(() => _ = new ActiveDirectoryDataProtectorService(null));
			AssertEquals("sid", e?.ParamName);
		}

		public void TestDecrypt_WorldSid()
		{
			var encryptedString = "MIIBUgYJKoZIhvcNAQcDoIIBQzCCAT8CAQIxgfKige8CAQQwgbIEgYABAAAAS0RTSwIAAABqAQAAEgAAAAYAAADEtmI9yb1CIAsYorrSJ4tFIAAAABIAAAAaAAAApw+kVuBftMA3uYVmIpHaZMRSRRIE+pib/LM8YFggRU93AHQAZwAuAHoAbwBuAGUAAAB3AHQAZwByAG8AbwB0AC4AegBvAG4AZQAAADAtBgkrBgEEAYI3SgEwIAYKKwYBBAGCN0oBATASMBAwDgwDU0lEDAdTLTEtMS0wMAsGCWCGSAFlAwQBLQQoCDqUO0h87GSGh22NRqNriqRWLPuVRDpOtxT9QLe95wNO+s7b3NAa5jBFBgkqhkiG9w0BBwEwHgYJYIZIAWUDBAEuMBEEDDxf+jl5iglIOTIDVwIBEIAYdvTI6k3H705lepL6J0pZCKBeOWQXYP1N";
			var expectedDecodedString = "testData";

			var dataProtectorService = new ActiveDirectoryDataProtectorService(WorldSid);
			var decryptedString = dataProtectorService.Decrypt(encryptedString);

			AssertEquals(expectedDecodedString, decryptedString);
		}

		public void TestDecrypt_NullString_Throws()
		{
			var dataProtectorService = new ActiveDirectoryDataProtectorService(WorldSid);

			var e = AssertExceptionThrown<ArgumentException>(() => dataProtectorService.Decrypt(null));
			CombineAssertions(() =>
			{
				AssertContains("Text to decrypt cannot be null or empty.", e.Message);
				AssertEquals("encryptedString", e.ParamName);
			});
		}

		public void TestDecrypt_EmptyString_Throws()
		{
			var dataProtectorService = new ActiveDirectoryDataProtectorService(WorldSid);

			var e = AssertExceptionThrown<ArgumentException>(() => dataProtectorService.Decrypt(string.Empty));
			CombineAssertions(() =>
			{
				AssertContains("Text to decrypt cannot be null or empty.", e.Message);
				AssertEquals("encryptedString", e.ParamName);
			});
		}

		public void TestDecrypt_InvalidString_Throws()
		{
			var dataProtectorService = new ActiveDirectoryDataProtectorService(WorldSid);
			var invalidEncryptedString = "NotBase64Format";

			var e = AssertExceptionThrown<CryptographicException>(() => dataProtectorService.Decrypt(invalidEncryptedString));
			AssertEquals("Failed to decode Base64 string for 'SID=S-1-1-0'.", e.Message);
		}

		public void TestDecrypt_InvalidSecret_Throws()
		{
			var dataProtectorService = new ActiveDirectoryDataProtectorService(WorldSid);
			var invalidEncryptedString = "SW52YWxpZEVuY3J5cHRlZFN0cmluZw==";

			var e = AssertExceptionThrown<CryptographicException>(() => dataProtectorService.Decrypt(invalidEncryptedString));
			AssertEquals("Failed to unprotect secret for 'SID=S-1-1-0'. HResult: 8009310B", e.Message);
			AssertEquals("ASN1 bad tag value met", e.InnerException?.Message);
		}

		public void TestEncrypt_NullString_Throws()
		{
			var dataProtectorService = new ActiveDirectoryDataProtectorService(WorldSid);

			var e = AssertExceptionThrown<ArgumentException>(() => dataProtectorService.Encrypt(null));
			CombineAssertions(() =>
			{
				AssertContains("Text to encrypt cannot be null or empty.", e.Message);
				AssertEquals("textToEncrypt", e.ParamName);
			});
		}

		public void TestEncrypt_EmptyString_Throws()
		{
			var dataProtectorService = new ActiveDirectoryDataProtectorService(WorldSid);

			var e = AssertExceptionThrown<ArgumentException>(() => dataProtectorService.Encrypt(string.Empty));
			CombineAssertions(() =>
			{
				AssertContains("Text to encrypt cannot be null or empty.", e.Message);
				AssertEquals("textToEncrypt", e.ParamName);
			});
		}

		public void TestEncryptDecrypt_WorldSid()
		{
			var testString = "Hello, 世界! 🌍";
			var dataProtectorService = new ActiveDirectoryDataProtectorService(WorldSid);
			var encryptedString = dataProtectorService.Encrypt(testString);

			CombineAssertions(() =>
			{
				AssertNotNull(encryptedString);
				AssertNotEquals(testString, encryptedString);
			});

			var decryptedString = dataProtectorService.Decrypt(encryptedString);
			AssertEquals(testString, decryptedString);
		}

		public void TestEncryptDecrypt_NotMyGroupSid()
		{
			var testString = Guid.NewGuid().ToString();
			var dataProtectorService = new ActiveDirectoryDataProtectorService(NullSid);
			var encryptedString = dataProtectorService.Encrypt(testString);

			CombineAssertions(() =>
			{
				AssertNotNull(encryptedString);
				AssertNotEquals(testString, encryptedString);
			});

			var e = AssertExceptionThrown<CryptographicException>(() => dataProtectorService.Decrypt(encryptedString));
			AssertEquals("Failed to unprotect secret for 'SID=S-1-0-0'. HResult: 8009002C", e.Message);
			AssertEquals("The specified data could not be decrypted", e.InnerException?.Message);
		}

		public void TestEncryptDecrypt_OtherUser()
		{
			var testString = "testData";
			var account = new NTAccount(TestConstants.ADTestUserAccount.NameWithDomainPreWindows2000);
			var sid = (SecurityIdentifier)account.Translate(typeof(SecurityIdentifier));
			var dataProtectorService = new ActiveDirectoryDataProtectorService(sid);

			var encryptedString = dataProtectorService.Encrypt(testString);

			CombineAssertions(() =>
			{
				AssertNotNull(encryptedString);
				AssertNotEquals(testString, encryptedString);

				using (new WindowsIdentityImpersonator(TestConstants.ADTestAdminAccount.NameWithDomain, TestConstants.ADTestAdminAccount.Password, () => { }))
				{
					// expect a failure as encryption is done for another user
					var e = AssertExceptionThrown<CryptographicException>(() => dataProtectorService.Decrypt(encryptedString));
					AssertMatch(new Regex("Failed to unprotect secret for 'SID=S[-0-9]+'. HResult: 8009002C"), e.Message);
					AssertEquals("The specified data could not be decrypted", e.InnerException?.Message);
				}

				using (new WindowsIdentityImpersonator(TestConstants.ADTestUserAccount.NameWithDomain, TestConstants.ADTestUserAccount.Password, () => { }))
				{
					// expect a success as encryption is done for the same user
					var decryptedString = dataProtectorService.Decrypt(encryptedString);
					AssertEquals(testString, decryptedString);
				}
			});
		}
	}
}
