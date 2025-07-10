using System;
using System.IO;
using System.Security.Cryptography;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Sensitive
{
	public sealed class AES256_HMACSHA256
	{
		public string Name = "AES256_HMACSHA256";
		public int BlockSizeBytes => 16;

		public int HashSizeBytes => 32;

		public void ValidateParameters(ReadOnlyMemory<byte> secret, ReadOnlyMemory<byte>? iv)
		{
			if (secret.Length < 64)
			{
				throw new CryptographicException("Insufficient secret data. Please pass at least 64 random bytes.");
			}
			if (iv != null && iv.Value.Length < 16)
			{
				throw new CryptographicException("Insufficient initialization vector (iv) data. Please pass null or at least 16 random bytes.");
			}
		}

		public ReadOnlyMemory<byte> CreateIV(ReadOnlyMemory<byte>? iv, RandomNumberGenerator randomGenerator)
		{
			if (iv != null)
			{
				return iv.Value;
			}

			var result = new byte[16];
			randomGenerator.GetBytes(result);
			return result;
		}

		public ReadOnlyMemory<byte> Encrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> secret, ReadOnlySpan<byte> iv)
		{
			using var aes = CreateAes();
			aes.Key = secret.Slice(0, 32).ToArray();
			aes.IV = iv.ToArray();

			using (var encryptor = aes.CreateEncryptor())
			using (var source = new MemoryStream(data.ToArray()))
			using (var target = new MemoryStream(data.Length))
			using (var stream = new CryptoStream(target, encryptor, CryptoStreamMode.Write))
			{
				source.CopyTo(stream);
				stream.FlushFinalBlock();
				return target.ToArray();
			}
		}

		public ReadOnlyMemory<byte> Decrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> secret, ReadOnlySpan<byte> iv)
		{
			using var aes = CreateAes();
			aes.Key = secret.Slice(0, 32).ToArray();
			aes.IV = iv.ToArray();

			using (var decryptor = aes.CreateDecryptor())
			using (var source = new MemoryStream(data.ToArray()))
			using (var target = new MemoryStream(data.Length))
			using (var stream = new CryptoStream(source, decryptor, CryptoStreamMode.Read))
			{
				stream.CopyTo(target);
				return target.ToArray();
			}
		}

		public ReadOnlyMemory<byte> Hash(ReadOnlySpan<byte> toHash, ReadOnlySpan<byte> secret)
		{
			using var hasher = new HMACSHA256(secret.ToArray());
			var hash = hasher.ComputeHash(toHash.ToArray());
			return hash;
		}

		Aes CreateAes()
		{
			var aes = Aes.Create();
			aes.KeySize = 256;
			aes.BlockSize = 128;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;
			return aes;
		}
	}
}
