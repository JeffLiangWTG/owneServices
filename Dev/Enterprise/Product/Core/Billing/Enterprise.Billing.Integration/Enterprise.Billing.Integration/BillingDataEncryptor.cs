using System;
using System.IO;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.Billing.Integration
{
	public static class BillingDataEncryptor
	{
		public static string Encrypt(byte[] data)
		{
			var compressedData = Compress(data);
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			return Convert.ToBase64String(encoder.Encrypt(compressedData));
		}

		public static byte[] Decrypt(string encryptedData)
		{
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			var compressedData = encoder.Decrypt(Convert.FromBase64String(encryptedData));
			return Decompress(compressedData);
		}

		static byte[] Compress(byte[] data)
		{
			using (var inputStream = new MemoryStream(data))
			using (var outputStream = new MemoryStream())
			{
				var creator = new ZipCreator();
				creator.ZipStream(ZipFileName, inputStream, outputStream);
				return outputStream.ToArray();
			}
		}

		static byte[] Decompress(byte[] compressedData)
		{
			using (var inputStream = new MemoryStream(compressedData))
			using (var outputStream = new MemoryStream())
			{
				var extractor = new ZipExtractor();
				extractor.ExtractZipStream(inputStream, outputStream, ZipFileName);
				return outputStream.ToArray();
			}
		}

		const string ZipFileName = "BillingData.xml";
	}
}