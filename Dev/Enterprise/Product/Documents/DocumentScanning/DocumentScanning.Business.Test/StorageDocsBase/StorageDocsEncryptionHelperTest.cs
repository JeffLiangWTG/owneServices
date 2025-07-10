using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class StorageDocsEncryptionHelperTest : TestCaseWithFactory
	{
		public void TestDataKeyEncryption()
		{
			var dataKey = StorageDocsEncryptionHelper.GetRandomBytes(16);
			var masterKey = StorageDocsEncryptionHelper.GetRandomBytes(16);

			var encryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, masterKey);
			var decryptedDataKey = StorageDocsEncryptionHelper.DecryptDataKey(encryptedDataKey, masterKey);

			AssertEquals(dataKey, decryptedDataKey);
		}

		public void TestDocumentEncryption()
		{
			var dataForTest = StorageDocsEncryptionHelper.GetRandomBytes(1000000);
			var dataKey = StorageDocsEncryptionHelper.GetRandomBytes(16);

			var encryptedDoc = StorageDocsEncryptionHelper.Encrypt(dataForTest, dataKey);
			var decryptedDoc = StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, dataKey);
			AssertEquals(dataForTest, decryptedDoc);

			decryptedDoc = StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, dataKey, Guid.NewGuid());
			AssertEquals("With PK", dataForTest, decryptedDoc);
		}

		public void TestDocumentEncryption_BackwardCompatibility()
		{
			var dataForTest = StorageDocsEncryptionHelper.GetRandomBytes(1000000);
			var dataKey = StorageDocsEncryptionHelper.GetRandomBytes(16);
			var pk = Guid.NewGuid();

			var encryptedDocWithOldLogic = StorageDocsEncryptionHelperForTest.EncryptDocumentWithPK(dataForTest, dataKey, pk);
			var decryptedDoc = StorageDocsEncryptionHelper.DecryptDocument(encryptedDocWithOldLogic, dataKey, pk);
			AssertEquals("Old encryption", dataForTest, decryptedDoc);

			//Error if decrypt without PK
			AssertExceptionThrown<S3CryptoException>("Decrypt without PK should throw for old encryption", () => StorageDocsEncryptionHelper.DecryptDocument(encryptedDocWithOldLogic, dataKey));
		}

		public void TestDocumentEncryption_BackwardCompatibility_SmallFile()
		{
			var dataForTest = StorageDocsEncryptionHelper.GetRandomBytes(11);
			var dataKey = StorageDocsEncryptionHelper.GetRandomBytes(16);
			var pk = Guid.NewGuid();

			var encryptedDocWithOldLogic = StorageDocsEncryptionHelperForTest.EncryptDocumentWithPK(dataForTest, dataKey, pk);
			var decryptedDoc = StorageDocsEncryptionHelper.DecryptDocument(encryptedDocWithOldLogic, dataKey, pk);
			AssertEquals("Old encryption", dataForTest, decryptedDoc);

			//Error if decrypt without PK
			AssertExceptionThrown<S3CryptoException>("Decrypt without PK should throw for old encryption", () => StorageDocsEncryptionHelper.DecryptDocument(encryptedDocWithOldLogic, dataKey));
		}

		public void TestDocumentEncryption_EmptyDoc()
		{
			var dataForTest = ZBlob.Empty;
			var dataKey = StorageDocsEncryptionHelper.GetRandomBytes(16);

			var encryptedDoc = StorageDocsEncryptionHelper.Encrypt(dataForTest, dataKey);
			var decryptedDoc = StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, dataKey);

			AssertEquals(dataForTest, decryptedDoc);
		}

		public void TestDecryptDataKeyWithErrors()
		{
			var dataKey = StorageDocsEncryptionHelper.GetRandomBytes(16);
			var masterKey = StorageDocsEncryptionHelper.GetRandomBytes(16);

			var encryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, masterKey);

			AssertExceptionThrown<S3CryptoException>("Incorrect EncryptDataKey", () => StorageDocsEncryptionHelper.DecryptDataKey(StorageDocsEncryptionHelper.GetRandomBytes(encryptedDataKey.Length), masterKey));
			AssertExceptionThrown<S3CryptoException>("Incorrect EncryptDataKey - Shorter", () => StorageDocsEncryptionHelper.DecryptDataKey(StorageDocsEncryptionHelper.GetRandomBytes(encryptedDataKey.Length - 1), masterKey));
			AssertExceptionThrown<S3CryptoException>("Incorrect EncryptDataKey - Shorter than nonce", () => StorageDocsEncryptionHelper.DecryptDataKey(StorageDocsEncryptionHelper.GetRandomBytes(10), masterKey));
			AssertExceptionThrown<S3CryptoException>("Empty EncryptDataKey", () => StorageDocsEncryptionHelper.DecryptDataKey(Array.Empty<byte>(), masterKey));

			AssertExceptionThrown<S3CryptoException>("Incorrect MasterKey", () => StorageDocsEncryptionHelper.DecryptDataKey(encryptedDataKey, StorageDocsEncryptionHelper.GetRandomBytes(16)));
			AssertExceptionThrown<S3CryptoException>("Empty MasterKey", () => StorageDocsEncryptionHelper.DecryptDataKey(encryptedDataKey, Array.Empty<byte>()));

			AssertEquals("Should report S3CryptoException", true, ErrorReporter.HasBeenReported("S3CryptoException"));
			ErrorReporter.Clear();

			var decryptedDataKey = StorageDocsEncryptionHelper.DecryptDataKey(encryptedDataKey, masterKey);
			AssertEquals(dataKey, decryptedDataKey);
		}

		public void TestDecryptDocumentWithErrors()
		{
			var dataForTest = StorageDocsEncryptionHelper.GetRandomBytes(1000000);
			var dataKey = StorageDocsEncryptionHelper.GetRandomBytes(16);
			var pk = Guid.NewGuid();

			var encryptedDoc = StorageDocsEncryptionHelper.Encrypt(dataForTest, dataKey);

			AssertExceptionThrown<S3CryptoException>("Incorrect Ciphertext - Same Length", () => StorageDocsEncryptionHelper.DecryptDocument(StorageDocsEncryptionHelper.GetRandomBytes(encryptedDoc.Length), dataKey, pk));
			AssertExceptionThrown<S3CryptoException>("Incorrect Ciphertext - Longer", () => StorageDocsEncryptionHelper.DecryptDocument(StorageDocsEncryptionHelper.GetRandomBytes(encryptedDoc.Length + 10), dataKey, pk));
			AssertExceptionThrown<S3CryptoException>("Incorrect Ciphertext - Shorter", () => StorageDocsEncryptionHelper.DecryptDocument(StorageDocsEncryptionHelper.GetRandomBytes(encryptedDoc.Length - 1), dataKey, pk));
			AssertExceptionThrown<S3CryptoException>("Incorrect Ciphertext - Shorter than tag", () => StorageDocsEncryptionHelper.DecryptDocument(StorageDocsEncryptionHelper.GetRandomBytes(5), dataKey, pk));
			AssertExceptionThrown<S3CryptoException>("Empty Ciphertext", () => StorageDocsEncryptionHelper.DecryptDocument(Array.Empty<byte>(), dataKey, pk));

			AssertExceptionThrown<S3CryptoException>("Incorrect DataKey", () => StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, StorageDocsEncryptionHelper.GetRandomBytes(dataKey.Length), pk));
			AssertExceptionThrown<S3CryptoException>("Empty DataKey", () => StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, Array.Empty<byte>(), pk));
			AssertExceptionThrown<S3CryptoException>("Incorrect DataKey with null pk", () => StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, StorageDocsEncryptionHelper.GetRandomBytes(dataKey.Length)));

			AssertNoExceptionThrown("Incorrect PK should not throw", () => StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, dataKey, Guid.NewGuid()));
			AssertNoExceptionThrown("No PK should not throw", () => StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, dataKey));

			var decryptedDoc = StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, dataKey);
			AssertEquals(dataForTest, decryptedDoc);
		}

		public void TestEncryptDataKeyWithErrors()
		{
			AssertExceptionThrown<S3CryptoException>("Empty master key", () => StorageDocsEncryptionHelper.Encrypt(StorageDocsEncryptionHelper.GetRandomBytes(16), Array.Empty<byte>()));
			AssertEquals("Should report S3CryptoException", true, ErrorReporter.HasBeenReported("S3CryptoException"));
			ErrorReporter.Clear();
		}

		public void TestEncryptDocumentWithErrors()
		{
			AssertExceptionThrown<S3CryptoException>("Empty data key", () => StorageDocsEncryptionHelper.Encrypt(StorageDocsEncryptionHelper.GetRandomBytes(16), Array.Empty<byte>()));
			AssertEquals("Should report S3CryptoException", true, ErrorReporter.HasBeenReported("S3CryptoException"));
			ErrorReporter.Clear();
		}

		[DeveloperOnlyTest]
		[ExpectNoExceptions]
		public void TestDocumentEncryptionStressTest()
		{
			var encryptionResults = new List<(byte[] dataForTest, byte[] encryptedDoc, byte[] encryptedDataKey)>();
			for (int i = 0; i < 1000; i++)
			{
				encryptionResults.Add(CreateAndEncyptDocument());
			}

			foreach (var (dataForTest, encryptedDoc, encryptedDataKey) in encryptionResults)
			{
				var decryptedData = StorageDocsEncryptionHelper.DecryptDocument(encryptedDoc, StorageDocsEncryptionHelper.DecryptDataKey(encryptedDataKey, StorageDocsMasterKeyProvider.GetCurrentMasterKey().SCK_KeyValue));
				AssertEquals(dataForTest, decryptedData);
			}
		}

		(byte[] dataForTest, byte[] encryptedDoc, byte[] encryptedDataKey) CreateAndEncyptDocument()
		{
			var dataForTest = StorageDocsEncryptionHelper.GetRandomBytes(1000000);
			var dataKey = StorageDocsEncryptionHelper.GetRandomBytes(16);
			var encryptedDoc = StorageDocsEncryptionHelper.Encrypt(dataForTest, dataKey);
			var encryptedDataKey = StorageDocsEncryptionHelper.Encrypt(dataKey, StorageDocsMasterKeyProvider.GetCurrentMasterKey().SCK_KeyValue);
			return (dataForTest, encryptedDoc, encryptedDataKey);
		}
	}

	public static class StorageDocsEncryptionHelperForTest
	{
		// Old encryption method that uses PK as nonce, only use for backward compatibility tests
		public static byte[] EncryptDocumentWithPK(byte[] plainData, byte[] dataKey, Guid pk)
		{
			var nonce = StorageDocsEncryptionHelper.GetNonceFromGuid(pk);
			return StorageDocsEncryptionHelper.EncryptWithBouncyCastle(plainData, dataKey, nonce);
		}
	}
}
