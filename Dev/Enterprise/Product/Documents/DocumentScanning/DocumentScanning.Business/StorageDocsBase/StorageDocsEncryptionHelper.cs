using System;
using System.Linq;
using System.Security.Cryptography;
using CargoWise.Common;
using Enterprise.DocumentScanning.Integration;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;

namespace Enterprise.DocumentScanning.Business
{
	public static class StorageDocsEncryptionHelper
	{
		public static byte[] NewDataKey() => GetRandomBytes(DataKeyLength);

		public static byte[] NewMasterKey() => GetRandomBytes(MasterKeyLength);

		public static byte[] Encrypt(byte[] plainData, byte[] key)
		{
			try
			{
				var nonce = GetRandomBytes(NonceLength);
				var encryptedData = EncryptWithBouncyCastle(plainData, key, nonce);

				// We concat nonce to the encryptedData
				// ie. encryptedData = nonce + ciphertext + tag
				return nonce.Concat(encryptedData).ToArray();
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce(nameof(S3CryptoException), $"Encrpytion error", ex);
				throw new S3CryptoException("Encrpytion error", ex);
			}
		}

		internal static byte[] Decrypt(byte[] encryptedData, byte[] key)
		{
			var nounce = new byte[NonceLength];
			var bcCiphertext = new byte[encryptedData.Length - NonceLength];
			Buffer.BlockCopy(encryptedData, 0, nounce, 0, NonceLength);
			Buffer.BlockCopy(encryptedData, NonceLength, bcCiphertext, 0, bcCiphertext.Length);

			return DecryptWithBouncyCastle(bcCiphertext, key, nounce);
		}

		public static byte[] DecryptDataKey(byte[] encryptedDataKey, byte[] masterKey)
		{
			try
			{
				return Decrypt(encryptedDataKey, masterKey);
			}
			catch (Exception ex)
			{
				ErrorReporter.ReportOnce(nameof(S3CryptoException), $"Cannot decrypt data key.", ex);
				throw new S3CryptoException("Data key cannot be decrypted", ex);
			}
		}

		// For backward compatibility only, we are no longer using PK/Guid as nonce for document encryption
#if DEBUG
		public
#else
		internal
#endif
		static byte[] GetNonceFromGuid(Guid guid)
		{
			var result = new byte[NonceLength];
			Buffer.BlockCopy(guid.ToByteArray(), 0, result, 0, NonceLength);
			return result;
		}

		// Provide the pk of the document if it was encrypted by the old encryption that uses pk as nonce
		public static byte[] DecryptDocument(byte[] encryptedData, byte[] dataKey, Guid? pk = null)
		{
			try
			{
				try
				{
					return Decrypt(encryptedData, dataKey);
				}
				catch (Exception) when (pk.HasValue)
				{
					// Backward compatible with old encryption that uses PK as nonce
					var nonce = GetNonceFromGuid(pk.Value);
					return DecryptWithBouncyCastle(encryptedData, dataKey, nonce);
				}
			}
			catch (Exception ex)
			{
				throw new S3CryptoException("Document cannot be decrypted", ex);
			}
		}

		const int NonceLength = 12;

		const int TagLength = 16;

		const int DataKeyLength = 16;

		const int MasterKeyLength = 16;

		public static byte[] GetRandomBytes(int sizeInByte)
		{
			using (var rng = RandomNumberGenerator.Create())
			{
				var result = new byte[sizeInByte];
				rng.GetBytes(result);
				return result;
			}
		}
#if DEBUG
		public
#else
		internal
#endif
		static byte[] EncryptWithBouncyCastle(byte[] plaintextBytes, byte[] key, byte[] nonce)
		{
			var bouncyCastleCiphertext = new byte[plaintextBytes.Length + TagLength];

			var cipher = new GcmBlockCipher(new AesEngine());
			var parameters = new AeadParameters(new KeyParameter(key), TagLength * 8, nonce);
			cipher.Init(true, parameters);

			var offset = cipher.ProcessBytes(plaintextBytes, 0, plaintextBytes.Length, bouncyCastleCiphertext, 0);
			cipher.DoFinal(bouncyCastleCiphertext, offset);

			// Bouncy Castle includes the authentication tag in the ciphertext
			// ie. bouncyCastleCiphertext = ciphertext + tag;
			return bouncyCastleCiphertext;
		}

		static byte[] DecryptWithBouncyCastle(byte[] bouncyCastleCiphertext, byte[] key, byte[] nonce)
		{
			var plaintextBytes = new byte[bouncyCastleCiphertext.Length - TagLength];

			var cipher = new GcmBlockCipher(new AesEngine());
			var parameters = new AeadParameters(new KeyParameter(key), TagLength * 8, nonce);
			cipher.Init(false, parameters);

			var offset = cipher.ProcessBytes(bouncyCastleCiphertext, 0, bouncyCastleCiphertext.Length, plaintextBytes, 0);
			cipher.DoFinal(plaintextBytes, offset);

			return plaintextBytes;
		}
	}

	[Serializable]
	public class S3CryptoException : ExternalStorageException
	{
		public S3CryptoException(string message, Exception innerException) : base(message, Core.Constants.EDocsStorageProviders.Code.S3, innerException)
		{
		}

#if NETFRAMEWORK
		protected S3CryptoException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
#endif
	}
}
