using System;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformation.Common.Testing
{
	sealed class PasswordHashingTransformationHelperTest : TransactionedTestCase
	{
		public void TestPasswordHistoryCount()
		{
			PasswordHashingTransformationTestHelper.SetPasswordHistoryCount(0);
			AssertEquals(0, PasswordHashingTransformationHelper.GetPasswordHistoryCount());
			AssertEquals(false, PasswordHashingTransformationHelper.IsPasswordHistoryCountRegistrySet());

			PasswordHashingTransformationTestHelper.SetPasswordHistoryCount(5);
			AssertEquals(5, PasswordHashingTransformationHelper.GetPasswordHistoryCount());
			AssertEquals(true, PasswordHashingTransformationHelper.IsPasswordHistoryCountRegistrySet());

			PasswordHashingTransformationTestHelper.SetPasswordHistoryCount(123, true);
			AssertEquals("Default", 0, PasswordHashingTransformationHelper.GetPasswordHistoryCount());
			AssertEquals("Default", false, PasswordHashingTransformationHelper.IsPasswordHistoryCountRegistrySet());
		}

		public void TestIsADIntegrationEnabled()
		{
			AssertEquals("Not set", false, PasswordHashingTransformationHelper.IsADIntegrationEnabled());

			PasswordHashingTransformationTestHelper.SetEnableADIntegration(false);
			AssertEquals(false, PasswordHashingTransformationHelper.IsADIntegrationEnabled());

			PasswordHashingTransformationTestHelper.SetEnableADIntegration(true);
			AssertEquals(true, PasswordHashingTransformationHelper.IsADIntegrationEnabled());

			PasswordHashingTransformationTestHelper.SetEnableADIntegration(true, true);
			AssertEquals(false, PasswordHashingTransformationHelper.IsADIntegrationEnabled());
		}

		public void TestPasswordHashingIterationsCount()
		{
			AssertEquals("Default", 200_000, PasswordHashingTransformationHelper.GetPasswordHashingIterationsCount());

			PasswordHashingTransformationTestHelper.SetPasswordHashingIterationsCount(1000);
			AssertEquals(1000, PasswordHashingTransformationHelper.GetPasswordHashingIterationsCount());

			PasswordHashingTransformationTestHelper.SetPasswordHashingIterationsCount(987654321);
			AssertEquals(987654321, PasswordHashingTransformationHelper.GetPasswordHashingIterationsCount());

			PasswordHashingTransformationTestHelper.SetPasswordHashingIterationsCount(987654321, true);
			AssertEquals("Back to default", 200_000, PasswordHashingTransformationHelper.GetPasswordHashingIterationsCount());
		}

		public void TestEncryptDecrypt()
		{
			var guid = Guid.NewGuid();
			var plainText = "No one else is willing to do that, so that's what I will do.";
			var encryptedText = PasswordHashingTransformationHelper.EncryptWithTwoWayEncoder(guid, plainText);
			var decryptedText = PasswordHashingTransformationHelper.DecryptWithTwoWayEncoder(guid, encryptedText);
			AssertEquals(plainText, decryptedText);
		}

		public void TestEncryptDecryptWorkWithTwoWayEncoder()
		{
			var guid = new Guid("E11ECA5B-D9C5-427A-A091-3AAA9C9FAA8D");
			var plainText = "No one else is willing to do that, so that's what I will do.";

			var encryptedTextByTwoWayEncoder = "/PZ2Pdtas/x6Nqk6LLTrYsfHFoZmjx6o63tK57DyD++LN5weHEm2N8mukh7p/zzq4lxISomV+JAPw5Vm4QRajU7z59Ukpt/+ZtjMS88qX4u3omkYYPmTOUYLkxSymC5sFU3VpXH6Jw/2l5wyiGC09JVPdEmuQOIck0MFPdGZRi0=";
			var decryptedText = PasswordHashingTransformationHelper.DecryptWithTwoWayEncoder(guid, encryptedTextByTwoWayEncoder);
			AssertEquals("Decrypt", plainText, decryptedText);

			var encryptedText = PasswordHashingTransformationHelper.EncryptWithTwoWayEncoder(guid, plainText);
			AssertEquals("Encrypt", encryptedTextByTwoWayEncoder, encryptedText);
		}
	}
}
