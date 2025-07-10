using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Enterprise.ZArchitecture.Core.Encryption
{
	/// <summary>
	/// Wrapper to encrypt and decrypt strings using RijndaelManaged algorithm.
	/// </summary>
	public class TwoWayEncoder
	{
		public TwoWayEncoder(Guid initialisationVector)
		{
			if (initialisationVector == Guid.Empty)
			{
				throw new ArgumentException("InitialisationVector cannot be an empty Guid");
			}

			ExtractIV(initialisationVector);
		}

		public static TwoWayEncoder NewWithStandardInitialisationVector()
		{
			return new TwoWayEncoder(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"));
		}

		/// <summary>
		/// Encrypts the Text and returns the Encrypted Text.
		/// </summary>
		public string Encrypt(string textToEncrypt)
		{
			return Convert.ToBase64String(Encrypt(Encoding.Unicode.GetBytes(textToEncrypt)));
		}

		/// <summary>
		/// Decrypts the Encrypted Text and returns the original Text
		/// </summary>
		public string Decrypt(string encryptedText)
		{
			return Decrypt(Key, IV, encryptedText);
		}

		public byte[] Encrypt(byte[] dataToEncrypt)
		{
			return Encrypt(Key, IV, dataToEncrypt);
		}

		public byte[] Decrypt(byte[] encryptedData)
		{
			byte[] result;
			int n = Decrypt(encryptedData, out result);
			if (result != null && n != result.Length)
			{
				byte[] tmp = result;
				result = new byte[n];
				Array.Copy(tmp, result, n);
			}
			return result;
		}

		[SuppressMessage("Microsoft.Design", "CA1021", Justification = "Old code baseline")]
		public int Decrypt(byte[] encryptedData, out byte[] decryptedData)
		{
			return Decrypt(Key, IV, encryptedData, out decryptedData);
		}

		#region Implementation

		byte[] IV;
		readonly byte[] Key = Encoding.ASCII.GetBytes("6052D90C81D64D5B8F5677AA055CAE23");

		//Note: This is not correct way to do Encrypt/Decrypt for proper AES, but it's too late to change it.
		// https://stackoverflow.com/questions/23406135/error-rijndaelmanaged-padding-is-invalid-and-cannot-be-removed
		[SuppressMessage("Microsoft.Maintainability", "CA1500", Justification = "Old code baseline")]
		byte[] Encrypt(byte[] key, byte[] iV, byte[] dataToEncrypt)
		{
			var algorithm = Aes.Create();
			ICryptoTransform encryptor = algorithm.CreateEncryptor(key, iV);

			//Encrypt the data.
			using (MemoryStream transformationStream = new MemoryStream())
			{
				using (CryptoStream encryptStream = new CryptoStream(transformationStream, encryptor, CryptoStreamMode.Write))
				{
					//Write all data to the crypto stream and flush it.
					encryptStream.Write(dataToEncrypt, 0, dataToEncrypt.Length);
					encryptStream.FlushFinalBlock();

					return transformationStream.ToArray();
				}
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1500", Justification = "Old code baseline")]
		string Decrypt(byte[] key, byte[] iV, string encryptedPassword)
		{
			string result = "";

			if (!string.IsNullOrEmpty(encryptedPassword))
			{
				byte[] encryptedData = Convert.FromBase64String(encryptedPassword);
				byte[] decryptedData;
				int n = Decrypt(key, iV, encryptedData, out decryptedData);
				result = Encoding.Unicode.GetString(decryptedData, 0, n);
			}

			return result;
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1500", Justification = "Old code baseline")]
		int Decrypt(byte[] key, byte[] iV, byte[] encryptedData, out byte[] decryptedData)
		{
			decryptedData = null;

			if (encryptedData != null && encryptedData.Length != 0)
			{
				var algorithm = Aes.Create();
				ICryptoTransform decryptor = algorithm.CreateDecryptor(key, iV);

				//Decrypt the data.
				using (var decryptedMemoryStream = new MemoryStream(encryptedData.Length))
				using (var transformationStream = new MemoryStream(encryptedData))
				using (var decryptStream = new CryptoStream(transformationStream, decryptor, CryptoStreamMode.Read))
				{
					decryptStream.CopyTo(decryptedMemoryStream);
					if (decryptedMemoryStream.Length == decryptedMemoryStream.GetBuffer().Length)
					{
						// Avoid copying the array if it's the correct length
						decryptedData = decryptedMemoryStream.GetBuffer();
					}
					else
					{
						decryptedData = decryptedMemoryStream.ToArray();
					}

					try
					{
						decryptStream.Clear();
						decryptStream.Dispose();
					}
					catch (IndexOutOfRangeException)
					{
						//ignore the weird .net bug in Dispose
						// Probably fixed in .Net Core March 2019 - https://github.com/dotnet/corefx/pull/36048
						// Still need this check while we're running on .Net Framework
					}
				}
			}

			return decryptedData == null ? 0 : decryptedData.Length;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1304", Justification = "Old code baseline")]
		void ExtractIV(Guid iVPK)
		{
			string formattedIV = iVPK.ToString().ToLower().Replace("-", "").Substring(5, 16);
			IV = Encoding.ASCII.GetBytes(formattedIV);
		}

		#endregion
	}
}
