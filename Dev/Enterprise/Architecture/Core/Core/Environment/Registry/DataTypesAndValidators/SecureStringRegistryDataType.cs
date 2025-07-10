using System;
using System.Runtime.InteropServices;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.ZArchitecture.Environment
{
	public class SecureStringRegistryDataType : StringRegistryDataType
	{
		public SecureStringRegistryDataType()
			: base()
		{
		}

		public SecureStringRegistryDataType(int minLength, int maxLength)
			: this()
		{
			MinLength = minLength;
			MaxLength = maxLength;
		}

		protected override byte[] SerialiseCore(string value)
		{
			var serialised = base.SerialiseCore(value);

			if (string.IsNullOrEmpty(value) || IsNullDataRepresentation(serialised))
			{
				return serialised;
			}

			var iv = Guid.NewGuid();
			var encoder = new TwoWayEncoder(iv);
			var ciphertext = encoder.Encrypt(serialised);

			var ivBytes = iv.ToByteArray();
			var ciphertextLength = ciphertext.Length;

			byte[] output = new byte[ivLength + ciphertextLength];

			Buffer.BlockCopy(ivBytes, 0, output, 0, ivLength);
			Buffer.BlockCopy(ciphertext, 0, output, ivLength, ciphertextLength);

			return output;
		}

		protected override string DeserialiseCore(byte[] value)
		{
			if (value.Length == 0 || IsNullDataRepresentation(value))
			{
				return base.DeserialiseCore(value);
			}

			var ivBytes = new byte[ivLength];
			Buffer.BlockCopy(value, 0, ivBytes, 0, ivLength);

			var ciphertextLength = value.Length - ivLength;
			var ciphertext = new byte[ciphertextLength];
			Buffer.BlockCopy(value, ivLength, ciphertext, 0, ciphertextLength);

			var decoder = new TwoWayEncoder(new Guid(ivBytes));
			var plaintext = decoder.Decrypt(ciphertext);

			return base.DeserialiseCore(plaintext);
		}

		readonly int ivLength = Marshal.SizeOf(Guid.Empty);
	}
}
